/*********************************************************************************
 * AutoSave.cs
 * 
 * Copyright (c) 2004-2018 Henk Nicolai
 * 
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 * 
 *        http://www.apache.org/licenses/LICENSE-2.0
 * 
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 * 
 *********************************************************************************/
using SysExtensions;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Manages an auto-save file local to every non-roaming user.
    /// This class is assumed to have a lifetime equal to the application.
    /// See also: <seealso cref="Environment.SpecialFolder.LocalApplicationData"/>
    /// </summary>
    public sealed class AutoSave
    {
        // These values seem to be recommended.
        internal const int CharBufferSize = 1024;
        internal const int FileStreamBufferSize = 4096;

        /// <summary>
        /// Minimal delay in milliseconds between two auto save operations.
        /// </summary>
        public const int AutoSaveDelay = 500;

        /// <summary>
        /// Gets the name of the auto save file.
        /// </summary>
        public static readonly string AutoSaveFileName = ".autosave";

        private readonly FileStream autoSaveFileStream;
        private readonly Encoder encoder;

        /// <summary>
        /// Serves as input for the encoder.
        /// </summary>
        private readonly char[] buffer;

        /// <summary>
        /// Serves as output for the encoder.
        /// </summary>
        private readonly byte[] encodedBuffer;

        /// <summary>
        /// Settings as they are stored locally.
        /// </summary>
        private SettingObject localSettings;

        /// <summary>
        /// Settings representing how they are currently stored in the autosave file.
        /// </summary>
        private SettingObject remoteSettings;

        /// <summary>
        /// Contains scheduled updates to the remote settings.
        /// </summary>
        private readonly ConcurrentQueue<SettingCopy> updateQueue;

        /// <summary>
        /// Camcels the long running auto-save background task.
        /// </summary>
        private readonly CancellationTokenSource cts;

        /// <summary>
        /// Long running auto-save background task.
        /// </summary>
        private readonly Task autoSaveBackgroundTask;

        /// <summary>
        /// Initializes a new instance of <see cref="AutoSave"/>.
        /// </summary>
        /// <param name="appSubFolderName">
        /// The name of the subfolder to use in <see cref="Environment.SpecialFolder.LocalApplicationData"/>.
        /// </param>
        /// <param name="initialSettings">
        /// The initial default settings to use, in case e.g. the autosave file could not be opened.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="appSubFolderName"/> or <paramref name="initialSettings"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="appSubFolderName"/> is <see cref="string.Empty"/>,
        /// or contains one or more of the invalid characters defined in <see cref="Path.GetInvalidPathChars"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="appSubFolderName"/> contains a colon character (:) that is not part of a drive label ("C:\").
        /// </exception>
        public AutoSave(string appSubFolderName, SettingCopy initialSettings)
        {
            // Have to check for string.Empty because Path.Combine will not.
            if (appSubFolderName == null)
            {
                throw new ArgumentNullException(nameof(appSubFolderName));
            }

            if (initialSettings == null)
            {
                throw new ArgumentNullException(nameof(initialSettings));
            }

            if (appSubFolderName.Length == 0)
            {
                throw new ArgumentException($"{nameof(appSubFolderName)} is string.Empty.", nameof(appSubFolderName));
            }

            // Until the autosave file has been successfully opened, assume both settings are the same.
            localSettings = initialSettings.Commit();
            remoteSettings = initialSettings.Commit();

            // If creation of the auto-save file fails, because e.g. an instance is already running,
            // don't throw but just disable auto-saving and use default initial settings.
            try
            {
                var localApplicationFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var baseDir = Directory.CreateDirectory(Path.Combine(localApplicationFolder, appSubFolderName));

                // Create fileStream in such a way that:
                // a) Create if it doesn't exist, open if it already exists.
                // b) Only this process can access it. Protects the folder from deletion as well.
                // It gets automatically closed when the application exits, i.e. no need for IDisposable.
                autoSaveFileStream = new FileStream(Path.Combine(baseDir.FullName, AutoSaveFileName),
                                                    FileMode.OpenOrCreate,
                                                    FileAccess.ReadWrite,
                                                    FileShare.Read,
                                                    FileStreamBufferSize,
                                                    FileOptions.Asynchronous);

                // Assert capabilities of the file stream.
                Debug.Assert(autoSaveFileStream.CanSeek
                    && autoSaveFileStream.CanRead
                    && autoSaveFileStream.CanWrite
                    && !autoSaveFileStream.CanTimeout);

                // Initialize encoders and buffers.
                Encoding encoding = new UTF8Encoding();
                encoder = encoding.GetEncoder();
                buffer = new char[CharBufferSize];
                encodedBuffer = new byte[encoding.GetMaxByteCount(CharBufferSize)];

                // Set up long running task to keep auto-saving remoteSettings.
                updateQueue = new ConcurrentQueue<SettingCopy>();
                cts = new CancellationTokenSource();
                autoSaveBackgroundTask = autoSaveLoop(cts.Token);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (Exception initAutoSaveException)
            {
                // Throw exceptions caused by dev errors.
                // Trace the rest. (IOException, PlatformNotSupportedException, UnauthorizedAccessException, ...)
                initAutoSaveException.Trace();
            }
        }

        /// <summary>
        /// Gets the <see cref="SettingObject"/> which contains the latest setting values.
        /// </summary>
        public SettingObject CurrentSettings => localSettings;

        /// <summary>
        /// Creates and returns an update operation for the auto-save file.
        /// </summary>
        public SettingUpdateOperation CreateUpdate()
        {
            return new SettingUpdateOperation(this);
        }

        internal void Persist(SettingCopy workingCopy)
        {
            if (!workingCopy.EqualTo(localSettings))
            {
                // Commit to localSettings.
                localSettings = workingCopy.Commit();

                if (updateQueue != null)
                {
                    // Enqueue a copy so its values are not shared with other threads.
                    updateQueue.Enqueue(localSettings.CreateWorkingCopy());
                }
            }
        }

        private async Task autoSaveLoop(CancellationToken ct)
        {
            // Only return if the queue is empty and saved.
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(AutoSaveDelay);

                // Empty the queue, take the latest update from it.
                SettingCopy latestUpdate = null;

                SettingCopy update;
                while (updateQueue.TryDequeue(out update)) latestUpdate = update;

                if (latestUpdate != null && !latestUpdate.EqualTo(remoteSettings))
                {
                    remoteSettings = latestUpdate.Commit();

                    try
                    {
                        using (var writer = new SettingWriter(autoSaveFileStream, encoder, buffer, encodedBuffer))
                        {
                            foreach (var kv in remoteSettings)
                            {
                                writer.WriteKey(kv.Key);
                                writer.Visit(kv.Value);
                            }
                        }
                    }
                    catch (Exception writeException)
                    {
                        writeException.Trace();
                    }
                }
            }
        }

        /// <summary>
        /// Waits for the long running auto-saver Task to finish.
        /// </summary>
        public void Close()
        {
            if (cts != null)
            {
                cts.Cancel();
                try
                {
                    autoSaveBackgroundTask.Wait();
                }
                catch
                {
                    // Have to catch cancelled exceptions.
                }
            }
        }
    }

    /// <summary>
    /// Represents a single iteration of writing settings to a file.
    /// </summary>
    internal class SettingWriter : SettingValueVisitor, IDisposable
    {
        // Lowercase values, unlike bool.TrueString and bool.FalseString.
        private static readonly string TrueString = "true";
        private static readonly string FalseString = "false";
        private static readonly string KeyValueSeparator = ": ";

        private readonly FileStream outputStream;
        private readonly Encoder encoder;
        private readonly char[] buffer;
        private readonly byte[] encodedBuffer;

        // Fill up the character buffer before doing any writing.
        private int currentCharPosition;

        public SettingWriter(FileStream outputStream, Encoder encoder, char[] buffer, byte[] encodedBuffer)
        {
            this.outputStream = outputStream;
            this.encoder = encoder;
            this.buffer = buffer;
            this.encodedBuffer = encodedBuffer;

            outputStream.Seek(0, SeekOrigin.Begin);
        }

        private void encodeAndWrite(string value)
        {
            // How much of the given string still needs to be written.
            // Takes into account that the character buffer may overrun.
            int remainingLength = value.Length;

            // Number of characters already written from value. Loop invariant therefore is:
            // charactersCopied + remainingLength == value.Length.
            int charactersCopied = 0;

            while (remainingLength > 0)
            {
                // Determine number of characters to write.
                // AutoSave.CharBufferSize is known to be equal to buffer.Length.
                int charWriteCount = AutoSave.CharBufferSize - currentCharPosition;

                // Remember if this fill up the entire buffer.
                bool bufferFull = charWriteCount <= remainingLength;
                if (!bufferFull) charWriteCount = remainingLength;

                // Now copy to the character buffer after checking its range.
                value.CopyTo(charactersCopied, buffer, currentCharPosition, charWriteCount);

                // Update loop variables.
                charactersCopied += charWriteCount;
                remainingLength -= charWriteCount;

                // If the buffer is full, call the encoder to convert it into bytes.
                if (bufferFull)
                {
                    int bytes = encoder.GetBytes(buffer, 0, AutoSave.CharBufferSize, encodedBuffer, 0, false);
                    outputStream.Write(encodedBuffer, 0, bytes);
                    currentCharPosition = 0;
                }
                else
                {
                    currentCharPosition += charWriteCount;
                }
            }
        }

        public void WriteKey(SettingKey key)
        {
            encodeAndWrite(key.Key);
            encodeAndWrite(KeyValueSeparator);
        }

        public override void VisitBoolean(BooleanSettingValue value)
        {
            encodeAndWrite(value.Value ? TrueString : FalseString);
            encodeAndWrite(Environment.NewLine);
        }

        public override void VisitInt32(Int32SettingValue value)
        {
            // Assumed here is that int conversion is culture independent, even though it's implicitly used.
            encodeAndWrite(Convert.ToString(value.Value));
            encodeAndWrite(Environment.NewLine);
        }

        public void Dispose()
        {
            // Process remaining characters in the buffer and what's left in the Encoder.
            int bytes = encoder.GetBytes(buffer, 0, currentCharPosition, encodedBuffer, 0, true);
            if (bytes > 0)
            {
                outputStream.Write(encodedBuffer, 0, bytes);
            }

            // Truncate and flush.
            outputStream.SetLength(outputStream.Position);
            outputStream.Flush();
        }
    }
}

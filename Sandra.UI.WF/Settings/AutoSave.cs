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
        /// Minimal delay in milliseconds between two auto-save operations.
        /// </summary>
        public const int AutoSaveDelay = 500;

        /// <summary>
        /// Gets the name of the auto-save file.
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
        /// Settings representing how they are currently stored in the auto-save file.
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
        /// The initial default settings to use, in case e.g. the auto-save file could not be opened.
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
            if (appSubFolderName == null)
            {
                throw new ArgumentNullException(nameof(appSubFolderName));
            }

            if (initialSettings == null)
            {
                throw new ArgumentNullException(nameof(initialSettings));
            }

            // Have to check for string.Empty because Path.Combine will not.
            if (appSubFolderName.Length == 0)
            {
                throw new ArgumentException($"{nameof(appSubFolderName)} is string.Empty.", nameof(appSubFolderName));
            }

            // If exclusive access to the auto-save file cannot be acquired, because e.g. an instance is already running,
            // don't throw but just disable auto-saving and use default initial settings.
            localSettings = initialSettings.Commit();

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
                                                    FileOptions.SequentialScan);

                // Assert capabilities of the file stream.
                Debug.Assert(autoSaveFileStream.CanSeek
                    && autoSaveFileStream.CanRead
                    && autoSaveFileStream.CanWrite
                    && !autoSaveFileStream.CanTimeout);

                // Initialize encoders and buffers.
                Encoding encoding = Encoding.UTF8;
                encoder = encoding.GetEncoder();
                buffer = new char[CharBufferSize];
                encodedBuffer = new byte[encoding.GetMaxByteCount(CharBufferSize)];

                // Initialize input buffers small enough so that they don't end up on the large object heap.
                // buffer and encodedBuffer cannot be reused for this, because the character buffer would not have enough space:
                //
                // Encoding.UTF8.GetMaxByteCount(1024) => 3075
                // Encoding.UTF8.GetMaxCharCount(3075) => 3076
                //
                // ...and buffer cannot hold 3076 characters.
                byte[] inputBuffer = new byte[CharBufferSize];
                char[] decodedBuffer = new char[encoding.GetMaxCharCount(CharBufferSize)];

                // Load remote settings.
                remoteSettings = Load(encoding.GetDecoder(), inputBuffer, decodedBuffer);

                // If non-empty, override localSettings with it.
                if (remoteSettings.Count > 0)
                {
                    localSettings = remoteSettings;
                }

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
                        var writer = new SettingWriter();
                        foreach (var kv in remoteSettings)
                        {
                            writer.WriteKey(kv.Key);
                            writer.Visit(kv.Value);
                        }
                        writer.WriteToFile(autoSaveFileStream, encoder, buffer, encodedBuffer);
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

        private SettingObject Load(Decoder decoder, byte[] inputBuffer, char[] decodedBuffer)
        {
            // Reuse one string builder to build keys and values.
            StringBuilder sb = new StringBuilder();

            // Loop until the entire file is read.
            for (;;)
            {
                int bytes = autoSaveFileStream.Read(inputBuffer, 0, CharBufferSize);
                if (bytes == 0) break;

                int chars = decoder.GetChars(inputBuffer, 0, bytes, decodedBuffer, 0);
                if (chars > 0)
                {
                    sb.Append(decodedBuffer, 0, chars);
                }
            }

            // Load into an empty working copy.
            var workingCopy = new SettingCopy();

            // Optimistically parse.
            // Research if parser can be replaced by 3rd-party library or if a compact representation in binary is necessary.
            string text = sb.ToString();
            int startIndex = 0;

            for (;;)
            {
                int keyIndex = text.IndexOf(SettingWriter.KeyValueSeparator, startIndex);
                if (keyIndex < startIndex) break;

                SettingKey key = new SettingKey(text.Substring(startIndex, keyIndex - startIndex));
                startIndex = keyIndex + SettingWriter.KeyValueSeparator.Length;

                int valueIndex = text.IndexOf(Environment.NewLine, startIndex);
                if (valueIndex < startIndex) break;

                // TODO: this does not work for string values that contain newlines.
                string valueAsString = text.Substring(startIndex, valueIndex - startIndex);
                startIndex = valueIndex + Environment.NewLine.Length;

                ISettingValue value;
                int intValue;
                if (valueAsString == SettingWriter.TrueString)
                {
                    value = new BooleanSettingValue(true);
                }
                else if (valueAsString == SettingWriter.FalseString)
                {
                    value = new BooleanSettingValue(false);
                }
                else if (int.TryParse(valueAsString, out intValue))
                {
                    value = new Int32SettingValue(intValue);
                }
                else if (valueAsString.Length >= 2 && valueAsString.StartsWith("\"") && valueAsString.EndsWith("\""))
                {
                    value = new StringSettingValue(valueAsString.Substring(1, valueAsString.Length - 2).Replace("\"\"", "\""));
                }
                else
                {
                    // Corrupt value, break.
                    break;
                }

                workingCopy.KeyValueMapping[key] = value;
            }

            return workingCopy.Commit();
        }
    }

    /// <summary>
    /// Represents a single iteration of writing settings to a file.
    /// </summary>
    internal class SettingWriter : SettingValueVisitor
    {
        // Lowercase values, unlike bool.TrueString and bool.FalseString.
        internal static readonly string TrueString = "true";
        internal static readonly string FalseString = "false";
        internal static readonly string KeyValueSeparator = ": ";

        private readonly StringBuilder outputBuilder;

        public SettingWriter()
        {
            outputBuilder = new StringBuilder();
        }

        public void WriteKey(SettingKey key)
        {
            outputBuilder.Append(key.Key);
            outputBuilder.Append(KeyValueSeparator);
        }

        public override void VisitBoolean(BooleanSettingValue value)
        {
            outputBuilder.Append(value.Value ? TrueString : FalseString);
            outputBuilder.Append(Environment.NewLine);
        }

        public override void VisitInt32(Int32SettingValue value)
        {
            // Assumed here is that int conversion is culture independent, even though it's implicitly used.
            outputBuilder.Append(Convert.ToString(value.Value));
            outputBuilder.Append(Environment.NewLine);
        }

        public override void VisitString(StringSettingValue value)
        {
            // For now replace with double quotes, to avoid backslash parsing code.
            outputBuilder.Append("\"" + value.Value.Replace("\"", "\"\"") + "\"");
            outputBuilder.Append(Environment.NewLine);
        }

        public void WriteToFile(FileStream outputStream, Encoder encoder, char[] buffer, byte[] encodedBuffer)
        {
            // Return value of GetBytes().
            int bytes;
            string value = outputBuilder.ToString();

            // How much of the given string still needs to be written.
            // Takes into account that the character buffer may overrun.
            int remainingLength = value.Length;

            // Number of characters already written from value. Loop invariant therefore is:
            // charactersCopied + remainingLength == value.Length.
            int charactersCopied = 0;

            // Truncate and append. Spend as little time as possible writing to outputStream.
            outputStream.SetLength(0);

            // Fill up the character buffer before doing any writing.
            int currentCharPosition = 0;

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
                    bytes = encoder.GetBytes(buffer, 0, AutoSave.CharBufferSize, encodedBuffer, 0, false);
                    outputStream.Write(encodedBuffer, 0, bytes);
                    currentCharPosition = 0;
                }
                else
                {
                    currentCharPosition += charWriteCount;
                }
            }

            // Process remaining characters in the buffer and what's left in the Encoder.
            bytes = encoder.GetBytes(buffer, 0, currentCharPosition, encodedBuffer, 0, true);
            if (bytes > 0)
            {
                outputStream.Write(encodedBuffer, 0, bytes);
            }

            // Make sure everything is written to the file.
            outputStream.Flush();
        }
    }
}

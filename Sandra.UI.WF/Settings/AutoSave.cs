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
using Newtonsoft.Json;
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
        /// Gets the name of the first auto-save file.
        /// </summary>
        public static readonly string AutoSaveFileName1 = ".autosave1";

        /// <summary>
        /// Gets the name of the second auto-save file.
        /// </summary>
        public static readonly string AutoSaveFileName2 = ".autosave2";

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
                autoSaveFileStream = new FileStream(Path.Combine(baseDir.FullName, AutoSaveFileName1),
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
            for (;;)
            {
                // If cancellation is requested, stop waiting so the queue can be emptied as quickly as possible.
                if (!ct.IsCancellationRequested)
                {
                    await Task.Delay(AutoSaveDelay);
                }

                // Empty the queue, take the latest update from it.
                SettingCopy latestUpdate = null;

                SettingCopy update;
                while (updateQueue.TryDequeue(out update)) latestUpdate = update;

                // Only return if the queue is empty and saved.
                if (latestUpdate == null && ct.IsCancellationRequested)
                {
                    break;
                }

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

        private void readAssert(bool condition, string assertionFailMessage)
        {
            Debug.Assert(condition, assertionFailMessage);
            if (!condition)
            {
                throw new JsonReaderException(assertionFailMessage);
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

            // Optimistically parse as json.
            JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(sb.ToString()));

            // Make assertions about the format in which the auto-save file was written.
            readAssert(jsonTextReader.TokenType == JsonToken.None, "Token None expected");

            if (jsonTextReader.Read())
            {
                readAssert(jsonTextReader.TokenType == JsonToken.StartObject, "'{' expected");

                for (;;)
                {
                    jsonTextReader.Read();
                    if (jsonTextReader.TokenType == JsonToken.EndObject) break;
                    readAssert(jsonTextReader.TokenType == JsonToken.PropertyName, "PropertyName or EndObject '}' expected");

                    SettingKey key = new SettingKey((string)jsonTextReader.Value);
                    jsonTextReader.Read();

                    ISettingValue value;
                    switch (jsonTextReader.TokenType)
                    {
                        case JsonToken.Boolean:
                            value = new BooleanSettingValue((bool)jsonTextReader.Value);
                            break;
                        case JsonToken.Integer:
                            value = new Int32SettingValue(Convert.ToInt32(jsonTextReader.Value));
                            break;
                        case JsonToken.String:
                            value = new StringSettingValue((string)jsonTextReader.Value);
                            break;
                        default:
                            readAssert(false, "Boolean, Integer or String expected");
                            // Above call is guaranteed to throw, make compiler aware here.
                            throw new InvalidProgramException();
                    }

                    workingCopy.KeyValueMapping[key] = value;
                }
            }

            return workingCopy.Commit();
        }
    }

    /// <summary>
    /// Represents a single iteration of writing settings to a file.
    /// </summary>
    internal class SettingWriter : SettingValueVisitor
    {
        private readonly StringBuilder outputBuilder;
        private readonly JsonTextWriter jsonTextWriter;

        public SettingWriter()
        {
            outputBuilder = new StringBuilder();
            jsonTextWriter = new JsonTextWriter(new StringWriter(outputBuilder));
            jsonTextWriter.WriteStartObject();
        }

        public void WriteKey(SettingKey key)
        {
            jsonTextWriter.WritePropertyName(key.Key);
        }

        public override void VisitBoolean(BooleanSettingValue value)
        {
            jsonTextWriter.WriteValue(value.Value);
        }

        public override void VisitInt32(Int32SettingValue value)
        {
            jsonTextWriter.WriteValue(value.Value);
        }

        public override void VisitString(StringSettingValue value)
        {
            jsonTextWriter.WriteValue(value.Value);
        }

        public void WriteToFile(FileStream outputStream, Encoder encoder, char[] buffer, byte[] encodedBuffer)
        {
            jsonTextWriter.WriteEndObject();
            jsonTextWriter.Close();
            string output = outputBuilder.ToString();

            // How much of the output still needs to be written.
            int remainingLength = output.Length;

            // Number of characters already written from output. Loop invariant therefore is:
            // charactersCopied + remainingLength == output.Length.
            int charactersCopied = 0;

            // Truncate and append. Spend as little time as possible writing to outputStream.
            outputStream.SetLength(0);

            // Fill up the character buffer before doing any writing.
            for (;;)
            {
                // Determine number of characters to write.
                // AutoSave.CharBufferSize is known to be equal to buffer.Length.
                int charWriteCount = AutoSave.CharBufferSize;

                // Remember if this fill up the entire buffer.
                bool bufferFull = charWriteCount <= remainingLength;
                if (!bufferFull) charWriteCount = remainingLength;

                // Now copy to the character buffer after checking its range.
                output.CopyTo(charactersCopied, buffer, 0, charWriteCount);

                // If the buffer is full, call the encoder to convert it into bytes.
                if (bufferFull)
                {
                    int bytes = encoder.GetBytes(buffer, 0, AutoSave.CharBufferSize, encodedBuffer, 0, false);
                    outputStream.Write(encodedBuffer, 0, bytes);
                }

                // Update loop variables.
                charactersCopied += charWriteCount;
                remainingLength -= charWriteCount;

                if (remainingLength == 0)
                {
                    // Process what's left in the buffer and Encoder.
                    int bytes = encoder.GetBytes(buffer, 0, bufferFull ? 0 : charWriteCount, encodedBuffer, 0, true);
                    if (bytes > 0)
                    {
                        outputStream.Write(encodedBuffer, 0, bytes);
                    }

                    // Make sure everything is written to the file.
                    outputStream.Flush();
                    return;
                }
            }
        }
    }
}

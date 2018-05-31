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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
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
        /// Gets the name of the file which indicates which of both auto-save files contains the latest data.
        /// </summary>
        /// <remarks>
        /// FileInfo.LastWriteTimeUtc would be the alternative but it turns out not to be precise enough
        /// to select the right auto-save file.
        /// </remarks>
        public static readonly string AutoSaveFileName = ".autosave";

        /// <summary>
        /// Gets the name of the first auto-save file.
        /// </summary>
        public static readonly string AutoSaveFileName1 = ".autosave1";

        /// <summary>
        /// Gets the name of the second auto-save file.
        /// </summary>
        public static readonly string AutoSaveFileName2 = ".autosave2";

        private const byte LastWriteToFileStream1 = 1;
        private const byte LastWriteToFileStream2 = 2;

        private readonly FileStream autoSaveFileStream;
        private readonly FileStream autoSaveFileStream1;
        private readonly FileStream autoSaveFileStream2;
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
        /// Either <see cref="autoSaveFileStream1"/> or <see cref="autoSaveFileStream2"/>, whichever was last written to.
        /// </summary>
        private FileStream lastWrittenToFileStream;

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

                autoSaveFileStream = CreateAutoSaveFileStream(baseDir, AutoSaveFileName);

                try
                {
                    autoSaveFileStream1 = CreateAutoSaveFileStream(baseDir, AutoSaveFileName1);
                    autoSaveFileStream2 = CreateAutoSaveFileStream(baseDir, AutoSaveFileName2);
                }
                catch
                {
                    autoSaveFileStream.Dispose();
                    autoSaveFileStream = null;
                    if (autoSaveFileStream1 != null)
                    {
                        autoSaveFileStream1.Dispose();
                        autoSaveFileStream1 = null;
                    }
                    throw;
                }

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

                // Choose auto-save file to load from.
                int flag = LastWriteToFileStream1;
                if (autoSaveFileStream.Length > 0) flag = autoSaveFileStream.ReadByte();

                FileStream latestAutoSaveFileStream
                    = autoSaveFileStream2.Length == 0 ? autoSaveFileStream1
                    : autoSaveFileStream1.Length == 0 ? autoSaveFileStream2
                    : flag == LastWriteToFileStream2 ? autoSaveFileStream2
                    : autoSaveFileStream1;

                // Load remote settings.
                try
                {
                    remoteSettings = Load(latestAutoSaveFileStream, encoding.GetDecoder(), inputBuffer, decodedBuffer);
                }
                catch (Exception firstLoadException)
                {
                    // Trace and try the other auto-save file as a backup.
                    // Also use a new decoder.
                    firstLoadException.Trace();
                    latestAutoSaveFileStream
                        = latestAutoSaveFileStream == autoSaveFileStream1
                        ? autoSaveFileStream2
                        : autoSaveFileStream1;

                    try
                    {
                        remoteSettings = Load(latestAutoSaveFileStream, encoding.GetDecoder(), inputBuffer, decodedBuffer);
                    }
                    catch (Exception secondLoadException)
                    {
                        // In the unlikely event that both auto-save files generate an error,
                        // just initialize from localSettings so auto-saves are still enabled.
                        secondLoadException.Trace();
                        remoteSettings = localSettings;
                    }
                }

                // Make sure to save to the other file stream first.
                lastWrittenToFileStream = latestAutoSaveFileStream;

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

        private FileStream CreateAutoSaveFileStream(DirectoryInfo baseDir, string autoSaveFileName)
        {
            // Create fileStream in such a way that:
            // a) Create if it doesn't exist, open if it already exists.
            // b) Only this process can access it. Protects the folder from deletion as well.
            // It gets automatically closed when the application exits, i.e. no need for IDisposable.
            var autoSaveFileStream = new FileStream(Path.Combine(baseDir.FullName, autoSaveFileName),
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

            return autoSaveFileStream;
        }

        /// <summary>
        /// Gets the <see cref="SettingObject"/> which contains the latest setting values.
        /// </summary>
        public SettingObject CurrentSettings => localSettings;

        /// <summary>
        /// Creates and returns an update operation for the auto-save file.
        /// </summary>
        public void Persist(SettingKey settingKey, PValue value)
        {
            new SettingUpdateOperation(this).AddOrReplace(settingKey, value).Persist();
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
                        Dictionary<string, PValue> temp = new Dictionary<string, PValue>();
                        foreach (var kv in remoteSettings) temp.Add(kv.Key.Key, kv.Value);
                        PMap map = new PMap(temp);
                        var writer = new SettingWriter();
                        writer.Visit(map);

                        // Alterate between both auto-save files.
                        // autoSaveFileStream contains a byte indicating which auto-save file is last written to.
                        FileStream writefileStream;
                        autoSaveFileStream.Seek(0, SeekOrigin.Begin);
                        if (lastWrittenToFileStream == autoSaveFileStream1)
                        {
                            writefileStream = autoSaveFileStream2;
                            // Truncate and append.
                            writefileStream.SetLength(0);
                            // Exactly now signal that autoSaveFileStream2 is the latest.
                            autoSaveFileStream.WriteByte(LastWriteToFileStream2);
                        }
                        else
                        {
                            writefileStream = autoSaveFileStream1;
                            // Truncate and append.
                            writefileStream.SetLength(0);
                            // Exactly now signal that autoSaveFileStream1 is the latest.
                            autoSaveFileStream.WriteByte(LastWriteToFileStream1);
                        }
                        autoSaveFileStream.Flush();

                        // Spend as little time as possible writing to writefileStream.
                        writer.WriteToFile(writefileStream, encoder, buffer, encodedBuffer);

                        // Only save when completely successful, to maximize chances that at least
                        // one of both auto-save files is in a completely correct format.
                        lastWrittenToFileStream = writefileStream;
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

                // Dispose in opposite order of acquiring the lock on the files,
                // so that inner files can only be locked if outer files are locked too.
                autoSaveFileStream2.Dispose();
                autoSaveFileStream1.Dispose();
                autoSaveFileStream.Dispose();
            }
        }

        private SettingObject Load(FileStream autoSaveFileStream, Decoder decoder, byte[] inputBuffer, char[] decodedBuffer)
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

            SettingReader settingReader = new SettingReader(new StringReader(sb.ToString()));

            // Load into an empty working copy.
            var workingCopy = new SettingCopy();

            PValue rootValue;
            if (settingReader.TryParseValue(out rootValue))
            {
                if (!(rootValue is PMap))
                {
                    throw new JsonReaderException("Expected json object at root");
                }

                PMap map = (PMap)rootValue;
                foreach (var kv in map)
                {
                    workingCopy.KeyValueMapping[new SettingKey(kv.Key)] = kv.Value;
                }
            }

            return workingCopy.Commit();
        }
    }

    /// <summary>
    /// Represents a single iteration of loading settings from a file.
    /// </summary>
    internal class SettingReader
    {
        private readonly JsonTextReader jsonTextReader;

        public SettingReader(TextReader inputReader)
        {
            if (inputReader == null) throw new ArgumentNullException(nameof(inputReader));
            jsonTextReader = new JsonTextReader(inputReader);
        }

        private Exception TokenTypeNotSupported(JsonToken jsonToken)
            => new JsonReaderException($"Token type {jsonToken} is not supported for settings.");

        private JsonToken ReadSkipComments()
        {
            // Skip comments until encountering something meaningful.
            do jsonTextReader.Read(); while (jsonTextReader.TokenType == JsonToken.Comment);
            return jsonTextReader.TokenType;
        }

        public bool TryParseValue(out PValue value)
        {
            var tokenType = ReadSkipComments();
            if (tokenType == JsonToken.None)
            {
                value = default(PValue);
                return false;
            }

            value = ParseValue(tokenType);

            if (ReadSkipComments() != JsonToken.None)
            {
                throw new JsonReaderException("End of file expected");
            }

            return true;
        }

        private PMap ParseMap()
        {
            Dictionary<string, PValue> mapBuilder = new Dictionary<string, PValue>();

            for (;;)
            {
                var tokenType = ReadSkipComments();
                if (tokenType == JsonToken.EndObject)
                {
                    return new PMap(mapBuilder);
                }

                if (tokenType != JsonToken.PropertyName)
                {
                    throw new JsonReaderException("PropertyName or EndObject '}' expected");
                }

                string key = (string)jsonTextReader.Value;

                // Expect unique keys.
                if (mapBuilder.ContainsKey(key))
                {
                    throw new JsonReaderException($"Non-unique key in object: {key}");
                }

                mapBuilder.Add(key, ParseValue(ReadSkipComments()));
            }
        }

        private PList ParseList()
        {
            List<PValue> listBuilder = new List<PValue>();

            for (;;)
            {
                var tokenType = ReadSkipComments();
                if (tokenType == JsonToken.EndArray)
                {
                    return new PList(listBuilder);
                }

                listBuilder.Add(ParseValue(tokenType));
            }
        }

        private PValue ParseValue(JsonToken currentTokenType)
        {
            switch (currentTokenType)
            {
                case JsonToken.Boolean:
                    return new PBoolean((bool)jsonTextReader.Value);

                case JsonToken.Integer:
                    return jsonTextReader.Value is BigInteger
                        ? new PInteger((BigInteger)jsonTextReader.Value)
                        : new PInteger((long)jsonTextReader.Value); ;

                case JsonToken.String:
                    return new PString((string)jsonTextReader.Value);

                case JsonToken.StartObject:
                    return ParseMap();

                case JsonToken.StartArray:
                    return ParseList();

                case JsonToken.None:
                    throw new JsonReaderException("Unexpected end of file");

                case JsonToken.Comment:
                case JsonToken.EndArray:
                case JsonToken.EndObject:
                case JsonToken.PropertyName:
                    throw new JsonReaderException("'{', '[', Boolean, Integer or String expected");

                case JsonToken.Bytes:
                case JsonToken.Date:
                case JsonToken.EndConstructor:
                case JsonToken.Float:
                case JsonToken.Null:
                case JsonToken.Raw:
                case JsonToken.StartConstructor:
                case JsonToken.Undefined:
                default:
                    throw TokenTypeNotSupported(currentTokenType);
            }
        }
    }

    /// <summary>
    /// Represents a single iteration of writing settings to a file.
    /// </summary>
    internal class SettingWriter : PValueVisitor
    {
        private readonly StringBuilder outputBuilder;
        private readonly JsonTextWriter jsonTextWriter;

        public SettingWriter()
        {
            outputBuilder = new StringBuilder();
            jsonTextWriter = new JsonTextWriter(new StringWriter(outputBuilder));
        }

        public override void VisitBoolean(PBoolean value)
        {
            jsonTextWriter.WriteValue(value.Value);
        }

        public override void VisitInteger(PInteger value)
        {
            jsonTextWriter.WriteValue(value.Value);
        }

        public override void VisitList(PList value)
        {
            jsonTextWriter.WriteStartArray();
            value.ForEach(Visit);
            jsonTextWriter.WriteEndArray();
        }

        public override void VisitMap(PMap value)
        {
            jsonTextWriter.WriteStartObject();
            foreach (var kv in value)
            {
                jsonTextWriter.WritePropertyName(kv.Key);
                Visit(kv.Value);
            }
            jsonTextWriter.WriteEndObject();
        }

        public override void VisitString(PString value)
        {
            jsonTextWriter.WriteValue(value.Value);
        }

        public void WriteToFile(FileStream outputStream, Encoder encoder, char[] buffer, byte[] encodedBuffer)
        {
            jsonTextWriter.Close();
            string output = outputBuilder.ToString();

            // How much of the output still needs to be written.
            int remainingLength = output.Length;

            // Number of characters already written from output. Loop invariant therefore is:
            // charactersCopied + remainingLength == output.Length.
            int charactersCopied = 0;

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

#region License
/*********************************************************************************
 * AutoSave.cs
 *
 * Copyright (c) 2004-2019 Henk Nicolai
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
**********************************************************************************/
#endregion

using Eutherion.Text.Json;
using Eutherion.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Eutherion.Win.Storage
{
    /// <summary>
    /// Manages an auto-save file local to every non-roaming user.
    /// This class is assumed to have a lifetime equal to the application.
    /// See also: <seealso cref="Environment.SpecialFolder.LocalApplicationData"/>
    /// </summary>
    public sealed class AutoSave
    {
        /// <summary>
        /// Documented default value of the 'bufferSize' parameter of the <see cref="FileStream"/> constructor.
        /// </summary>
        public const int DefaultFileStreamBufferSize = 4096;

        // This value seem to be recommended.
        private const int CharBufferSize = 1024;

        /// <summary>
        /// Minimal delay in milliseconds between two auto-save operations.
        /// </summary>
        public static readonly int AutoSaveDelay = 5000;

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

        /// <summary>
        /// The primary auto-save file.
        /// </summary>
        private readonly FileStream autoSaveFile1;

        /// <summary>
        /// The secondary auto-save file.
        /// </summary>
        private readonly FileStream autoSaveFile2;

        /// <summary>
        /// The Encoder which converts updated text to bytes to write to the auto-save file.
        /// </summary>
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
        /// <param name="workingCopy">
        /// The schema to use.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="appSubFolderName"/> and/or <paramref name="workingCopy"/> are null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="appSubFolderName"/> is <see cref="string.Empty"/>,
        /// or contains one or more of the invalid characters defined in <see cref="Path.GetInvalidPathChars"/>,
        /// or targets a folder which is not a subfolder of <see cref="Environment.SpecialFolder.LocalApplicationData"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="appSubFolderName"/> contains a colon character (:) that is not part of a drive label ("C:\").
        /// </exception>
        public AutoSave(string appSubFolderName, SettingCopy workingCopy)
        {
            if (appSubFolderName == null)
            {
                throw new ArgumentNullException(nameof(appSubFolderName));
            }

            if (workingCopy == null)
            {
                throw new ArgumentNullException(nameof(workingCopy));
            }

            // Have to check for string.Empty because Path.Combine will not.
            if (appSubFolderName.Length == 0)
            {
                throw new ArgumentException($"{nameof(appSubFolderName)} is string.Empty.", nameof(appSubFolderName));
            }

            if (!SubFolderNameType.Instance.IsValid(appSubFolderName, out ITypeErrorBuilder _))
            {
                throw new ArgumentException($"{nameof(appSubFolderName)} targets AppData\\Local itself or is not a subfolder.", nameof(appSubFolderName));
            }

            // If exclusive access to the auto-save file cannot be acquired, because e.g. an instance is already running,
            // don't throw but just disable auto-saving and use initial empty settings.
            localSettings = workingCopy.Commit();

            try
            {
                var localApplicationFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var baseDir = Directory.CreateDirectory(Path.Combine(localApplicationFolder, appSubFolderName));

                autoSaveFileStream = CreateAutoSaveFileStream(baseDir, AutoSaveFileName);

                try
                {
                    autoSaveFile1 = CreateAutoSaveFileStream(baseDir, AutoSaveFileName1);
                    autoSaveFile2 = CreateAutoSaveFileStream(baseDir, AutoSaveFileName2);
                }
                catch
                {
                    autoSaveFileStream.Dispose();
                    autoSaveFileStream = null;
                    if (autoSaveFile1 != null)
                    {
                        autoSaveFile1.Dispose();
                        autoSaveFile1 = null;
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

                FileStream latestAutoSaveFile
                    = autoSaveFile2.Length == 0 ? autoSaveFile1
                    : autoSaveFile1.Length == 0 ? autoSaveFile2
                    : flag == LastWriteToFileStream2 ? autoSaveFile2
                    : autoSaveFile1;

                // Load remote settings.
                List<JsonErrorInfo> errors;
                bool tryOtherAutoSaveStream = false;
                try
                {
                    remoteSettings = Load(latestAutoSaveFile, encoding.GetDecoder(), inputBuffer, decodedBuffer, out errors);
                    if (errors.Count > 0)
                    {
                        errors.ForEach(x => new AutoSaveFileParseException(x).Trace());
                        tryOtherAutoSaveStream = true;
                    }
                }
                catch (Exception firstLoadException)
                {
                    // Trace and try the other auto-save file as a backup.
                    // Also use a new decoder.
                    firstLoadException.Trace();
                    tryOtherAutoSaveStream = true;
                }

                if (tryOtherAutoSaveStream)
                {
                    latestAutoSaveFile
                        = latestAutoSaveFile == autoSaveFile1
                        ? autoSaveFile2
                        : autoSaveFile1;

                    try
                    {
                        remoteSettings = Load(latestAutoSaveFile, encoding.GetDecoder(), inputBuffer, decodedBuffer, out errors);
                        if (errors.Count > 0)
                        {
                            errors.ForEach(x => new AutoSaveFileParseException(x).Trace());
                            remoteSettings = localSettings;
                        }
                    }
                    catch (Exception secondLoadException)
                    {
                        // In the unlikely event that both auto-save files generate an error,
                        // just initialize from localSettings so auto-saves are still enabled.
                        secondLoadException.Trace();
                        remoteSettings = localSettings;
                    }
                }

                // Override localSettings with remoteSettings.
                if (remoteSettings != null) localSettings = remoteSettings;

                // Set up long running task to keep auto-saving remoteSettings.
                updateQueue = new ConcurrentQueue<SettingCopy>();
                cts = new CancellationTokenSource();
                autoSaveBackgroundTask = AutoSaveLoop(latestAutoSaveFile, cts.Token);
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
                                                    DefaultFileStreamBufferSize,
                                                    FileOptions.SequentialScan | FileOptions.Asynchronous);

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
        public void Persist<TValue>(SettingProperty<TValue> property, TValue value)
        {
            SettingCopy workingCopy = localSettings.CreateWorkingCopy();
            workingCopy.AddOrReplace(property, value);

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

        private async Task AutoSaveLoop(FileStream lastWrittenToFile, CancellationToken ct)
        {
            for (; ; )
            {
                // If cancellation is requested, stop waiting so the queue can be emptied as quickly as possible.
                if (!ct.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(AutoSaveDelay, ct);
                    }
                    catch
                    {
                        // If the task was cancelled, empty the queue before leaving this method.
                    }
                }

                // Empty the queue, take the latest update from it.
                SettingCopy latestUpdate = null;

                while (updateQueue.TryDequeue(out SettingCopy update)) latestUpdate = update;

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
                        string textToSave = CompactSettingWriter.ConvertToJson(remoteSettings.Map);

                        // Alterate between both auto-save files.
                        // autoSaveFileStream contains a byte indicating which auto-save file is last written to.
                        FileStream targetFile;
                        autoSaveFileStream.Seek(0, SeekOrigin.Begin);
                        if (lastWrittenToFile == autoSaveFile1)
                        {
                            targetFile = autoSaveFile2;
                            // Truncate and append.
                            targetFile.SetLength(0);
                            // Exactly now signal that autoSaveFileStream2 is the latest.
                            autoSaveFileStream.WriteByte(LastWriteToFileStream2);
                        }
                        else
                        {
                            targetFile = autoSaveFile1;
                            // Truncate and append.
                            targetFile.SetLength(0);
                            // Exactly now signal that autoSaveFileStream1 is the latest.
                            autoSaveFileStream.WriteByte(LastWriteToFileStream1);
                        }
                        autoSaveFileStream.Flush();

                        // Spend as little time as possible writing to writefileStream.
                        await WriteToFileAsync(targetFile, textToSave);

                        // Only save when completely successful, to maximize chances that at least
                        // one of both auto-save files is in a completely correct format.
                        lastWrittenToFile = targetFile;
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
                autoSaveFile2.Dispose();
                autoSaveFile1.Dispose();
                autoSaveFileStream.Dispose();
            }
        }

        private SettingObject Load(FileStream autoSaveFileStream, Decoder decoder, byte[] inputBuffer, char[] decodedBuffer, out List<JsonErrorInfo> errors)
        {
            // Reuse one string builder to build keys and values.
            StringBuilder sb = new StringBuilder();

            // Loop until the entire file is read.
            for (; ; )
            {
                int bytes = autoSaveFileStream.Read(inputBuffer, 0, CharBufferSize);
                if (bytes == 0) break;

                int chars = decoder.GetChars(inputBuffer, 0, bytes, decodedBuffer, 0);
                if (chars > 0)
                {
                    sb.Append(decodedBuffer, 0, chars);
                }
            }

            // Load into a copy of localSettings, preserving defaults.
            var workingCopy = localSettings.CreateWorkingCopy();
            errors = SettingReader.ReadWorkingCopy(sb.ToString(), workingCopy);
            return workingCopy.Commit();
        }

        private async Task WriteToFileAsync(FileStream targetFile, string textToSave)
        {
            // How much of the output still needs to be written.
            int remainingLength = textToSave.Length;

            // Number of characters already written from output. Loop invariant therefore is:
            // charactersCopied + remainingLength == output.Length.
            int charactersCopied = 0;

            // Fill up the character buffer before doing any writing.
            for (; ; )
            {
                // Determine number of characters to write.
                // AutoSave.CharBufferSize is known to be equal to buffer.Length.
                int charWriteCount = CharBufferSize;

                // Remember if this fill up the entire buffer.
                bool bufferFull = charWriteCount <= remainingLength;
                if (!bufferFull) charWriteCount = remainingLength;

                // Now copy to the character buffer after checking its range.
                textToSave.CopyTo(charactersCopied, buffer, 0, charWriteCount);

                // If the buffer is full, call the encoder to convert it into bytes.
                if (bufferFull)
                {
                    int bytes = encoder.GetBytes(buffer, 0, CharBufferSize, encodedBuffer, 0, false);
                    await targetFile.WriteAsync(encodedBuffer, 0, bytes);
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
                        await targetFile.WriteAsync(encodedBuffer, 0, bytes);
                    }

                    // Make sure everything is written to the file before returning.
                    await targetFile.FlushAsync();
                    return;
                }
            }
        }
    }

    internal class AutoSaveFileParseException : Exception
    {
        public static string AutoSaveFileParseMessage(JsonErrorInfo jsonErrorInfo)
        {
            string paramDisplayString = StringUtilities.ToDefaultParameterListDisplayString(jsonErrorInfo.Parameters);
            return $"{jsonErrorInfo.ErrorCode}{paramDisplayString} at position {jsonErrorInfo.Start}, length {jsonErrorInfo.Length}";
        }

        public AutoSaveFileParseException(JsonErrorInfo jsonErrorInfo)
            : base(AutoSaveFileParseMessage(jsonErrorInfo)) { }
    }
}

namespace Eutherion.Win
{
    /// <summary>
    /// Encapsulates a pair of <see cref="FileStream"/>s which are used for auto-saving text files.
    /// This class is tailored for frequent sequential asynchronous writing of text, so it is a good idea
    /// to open the <see cref="FileStream"/>s with both <see cref="FileOptions.SequentialScan"/>
    /// and <see cref="FileOptions.Asynchronous"/>.
    /// </summary>
    /// <remarks>
    /// The auto-save <see cref="FileStream"/>s A and B go through these phases cyclically:
    ///
    /// (1) A is a valid non-empty state and B is empty.
    ///     The auto-save loop is waiting for the next auto-save operation.
    /// (2) B is non-empty and in the process of being written to.
    /// (3) Writing to B has finished and both A and B are in a valid non-empty state.
    /// (4) B is a valid non-empty state and A is empty.
    ///     The auto-save loop is waiting for the next auto-save operation.
    /// (5) A is non-empty and in the process of being written to.
    /// (6) Writing to A has finished and both A and B are in a valid non-empty state.
    /// (7) Back to (1).
    /// 
    /// Recovering from crashes is straightforward when the crash happened in phases (1) or (4),
    /// done by choosing the only non-empty file - and if both files were empty, nothing was saved yet.
    /// 
    /// Recovering from phases (3) or (6) can be done by choosing one of both files arbitrarily.
    /// Distinguishing between both phases is impossible but since these phases are by far the shortest,
    /// an arbitrary choice carries only little risk and has little impact because then the ignored
    /// phase is interpreted as being in its preceding phase. (FileInfo.LastWriteTimeUtc would be an idea
    /// but it turns out not to be precise enough to select the right auto-save file.)
    /// 
    /// Recovering from phases (2) or (5) is hardest because it assumes that a file can be in a valid
    /// as well as an invalid state, and that distinction is made solely based on whether or not
    /// the last character of the text was already written to the file. With e.g. json this would be
    /// trivial because its last character is always a closing brace, but for other auto-save file formats
    /// this is generally not the case.
    /// 
    /// The idea therefore is to start with the expected length of the text followed by a fixed newline
    /// character. This works because:
    /// (a) This length cannot be zero, and numbers do not start with a zero. So if the file is
    ///     non-empty, it is already immediately invalid.
    /// (b) After writing the last character to the file, it immediately becomes valid.
    /// </remarks>
    public sealed class AutoSaveTextFile
    {
    }
}

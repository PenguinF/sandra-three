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
using System.Globalization;
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
        private class SettingsRemoteState
        {
            /// <summary>
            /// Settings representing how they are currently stored in the auto-save file.
            /// </summary>
            public SettingObject RemoteSettings { get; private set; }

            public SettingsRemoteState(SettingObject defaultSettings) => RemoteSettings = defaultSettings;

            public void Initialize(string loadedText)
            {
                if (loadedText != null)
                {
                    // Load into a copy of RemoteSettings, preserving defaults.
                    var workingCopy = RemoteSettings.CreateWorkingCopy();
                    var errors = SettingReader.ReadWorkingCopy(loadedText, workingCopy);

                    if (errors.Count > 0)
                    {
                        // Leave RemoteSettings unchanged.
                        errors.ForEach(x => new AutoSaveFileParseException(x).Trace());
                    }
                    else
                    {
                        RemoteSettings = workingCopy.Commit();
                    }
                }
            }

            public bool ShouldSave(IReadOnlyList<SettingCopy> updates, out string textToSave)
            {
                SettingCopy latestUpdate = updates[updates.Count - 1];

                if (!latestUpdate.EqualTo(RemoteSettings))
                {
                    RemoteSettings = latestUpdate.Commit();
                    textToSave = CompactSettingWriter.ConvertToJson(RemoteSettings.Map);
                    return true;
                }

                textToSave = default(string);
                return false;
            }
        }

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
        /// Gets the name of the file which acts as an exclusive lock between different instances
        /// of this process which might race to obtain a reference to the auto-save files.
        /// </summary>
        public static readonly string LockFileName = ".lock";

        /// <summary>
        /// Gets the name of the first auto-save file.
        /// </summary>
        public static readonly string AutoSaveFileName1 = ".autosave1";

        /// <summary>
        /// Gets the name of the second auto-save file.
        /// </summary>
        public static readonly string AutoSaveFileName2 = ".autosave2";

        /// <summary>
        /// The lock file to grant access to the auto-save files by at most one instance of this process.
        /// </summary>
        private readonly FileStream lockFile;

        /// <summary>
        /// Contains both auto-save files.
        /// </summary>
        private readonly AutoSaveTextFile autoSaveFile;

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
                string localApplicationFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                DirectoryInfo baseDir = Directory.CreateDirectory(Path.Combine(localApplicationFolder, appSubFolderName));
                lockFile = CreateAutoSaveFileStream(baseDir, LockFileName);

                // In the unlikely event that both auto-save files generate an error,
                // just initialize from localSettings so auto-saves within the session are still enabled.
                var remoteState = new SettingsRemoteState(localSettings);
                FileStream autoSaveFile1 = null;
                FileStream autoSaveFile2 = null;

                try
                {
                    autoSaveFile1 = CreateAutoSaveFileStream(baseDir, AutoSaveFileName1);
                    autoSaveFile2 = CreateAutoSaveFileStream(baseDir, AutoSaveFileName2);
                    autoSaveFile = new AutoSaveTextFile(autoSaveFile1, autoSaveFile2);
                }
                catch
                {
                    // Dispose in opposite order of acquiring the lock on the files,
                    // so that inner files can only be locked if outer files are locked too.
                    if (autoSaveFile1 != null)
                    {
                        if (autoSaveFile2 != null)
                        {
                            autoSaveFile2.Dispose();
                            autoSaveFile2 = null;
                        }
                        autoSaveFile1.Dispose();
                        autoSaveFile1 = null;
                    }
                    lockFile.Dispose();
                    lockFile = null;
                    throw;
                }

                // Choose first auto-save file to load from.
                FileStream latestAutoSaveFile = autoSaveFile.autoSaveFile1.Length == 0 ? autoSaveFile.autoSaveFile2 : autoSaveFile.autoSaveFile1;

                string loadedText = null;
                try
                {
                    loadedText = autoSaveFile.Load(latestAutoSaveFile);
                }
                catch (Exception firstLoadException)
                {
                    // Trace and try the other auto-save file as a backup.
                    firstLoadException.Trace();
                }

                // If null is returned from the first Load(), the integrity check failed.
                if (loadedText == null)
                {
                    latestAutoSaveFile = autoSaveFile.Switch(latestAutoSaveFile);

                    try
                    {
                        loadedText = autoSaveFile.Load(latestAutoSaveFile);
                    }
                    catch (Exception secondLoadException)
                    {
                        secondLoadException.Trace();
                    }
                }

                // Initialize remote state with the loaded text.
                // If both reads failed, loadedText == null.
                remoteState.Initialize(loadedText);

                // Initialize encoders and buffers.
                // Always use UTF8 for auto-saved text files.
                Encoding encoding = Encoding.UTF8;
                encoder = encoding.GetEncoder();
                buffer = new char[CharBufferSize];
                encodedBuffer = new byte[encoding.GetMaxByteCount(CharBufferSize)];

                // Set up long running task to keep auto-saving updates.
                updateQueue = new ConcurrentQueue<SettingCopy>();
                cts = new CancellationTokenSource();
                autoSaveBackgroundTask = AutoSaveLoop(latestAutoSaveFile, remoteState, cts.Token);

                // Override localSettings with RemoteSettings.
                // This is thread-safe because nothing is yet persisted to either autoSaveFile1 or autoSaveFile2.
                localSettings = remoteState.RemoteSettings;
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
        /// Creates a <see cref="FileStream"/> in such a way that:
        /// a) Create if it doesn't exist, open if it already exists.
        /// b) Only this process can access it. Protects the folder from deletion as well.
        /// </summary>
        private FileStream CreateAutoSaveFileStream(DirectoryInfo baseDir, string autoSaveFileName)
        {
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
                    // Persist a copy so its values are not shared with other threads.
                    Persist(localSettings.CreateWorkingCopy());
                }
            }
        }

        private async Task AutoSaveLoop(FileStream lastWrittenToFile, SettingsRemoteState remoteState, CancellationToken ct)
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
                bool hasUpdate = updateQueue.TryDequeue(out SettingCopy firstUpdate);

                if (!hasUpdate)
                {
                    // Only return if the queue is empty and saved.
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }
                }
                else
                {
                    // Create a local (thread-safe) list of updates to process.
                    List<SettingCopy> updates = new List<SettingCopy> { firstUpdate };
                    while (updateQueue.TryDequeue(out SettingCopy update)) updates.Add(update);

                    try
                    {
                        if (remoteState.ShouldSave(updates, out string textToSave))
                        {
                            // Alterate between both auto-save files.
                            // autoSaveFileStream contains a byte indicating which auto-save file is last written to.
                            FileStream targetFile = autoSaveFile.Switch(lastWrittenToFile);

                            // Only truly necessary in the first iteration if the targetFile was initially a corrupt non-empty file.
                            // Theoretically, two thrown writeExceptions would have the same effect.
                            // In other cases, lastWrittenToFile.SetLength(0) below will already have done so.
                            targetFile.SetLength(0);

                            // Write the contents to the file.
                            await WriteToFileAsync(targetFile, textToSave);

                            // Only truncate the other file when completely successful, to indicate that
                            // the auto-save file which was just saved is in a completely correct format.
                            lastWrittenToFile.SetLength(0);

                            // Switch to writing to the other file in the next iteration.
                            lastWrittenToFile = targetFile;
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

                // Dispose in opposite order of acquiring the lock on the files,
                // so that inner files can only be locked if outer files are locked too.
                autoSaveFile.autoSaveFile2.Dispose();
                autoSaveFile.autoSaveFile1.Dispose();
                lockFile.Dispose();
            }
        }

        /// <summary>
        /// Persists an update to the auto-save file.
        /// Update objects must be thread-safe, or not contain any shared state.
        /// </summary>
        /// <param name="update">
        /// The update to persist.
        /// </param>
        public void Persist(SettingCopy update) => updateQueue.Enqueue(update);

        private async Task WriteToFileAsync(FileStream targetFile, string textToSave)
        {
            const char newLineChar = '\n';

            // How much of the output still needs to be written.
            int remainingLength = textToSave.Length;

            // Write the length of the text plus a newline character before the rest, to aid recovery from crashes.
            // The length of its string representation will be smaller than CharBufferSize
            // until the day string lengths exceed 10¹⁰²⁴ - 2.
            string firstLine = remainingLength.ToString(CultureInfo.InvariantCulture) + newLineChar;
            firstLine.CopyTo(0, buffer, 0, firstLine.Length);
            int firstLineBytes = encoder.GetBytes(buffer, 0, firstLine.Length, encodedBuffer, 0, false);
            await targetFile.WriteAsync(encodedBuffer, 0, firstLineBytes);

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
        /// <summary>
        /// The primary auto-save file.
        /// </summary>
        internal readonly FileStream autoSaveFile1;

        /// <summary>
        /// The secondary auto-save file.
        /// </summary>
        internal readonly FileStream autoSaveFile2;

        /// <summary>
        /// Initializes a new instance of <see cref="AutoSaveTextFile"/>.
        /// </summary>
        /// <param name="autoSaveFile1">
        /// The primary <see cref="FileStream"/> to write to.
        /// Any existing contents in the file will be overwritten.
        /// </param>
        /// <param name="autoSaveFile2">
        /// The secondary <see cref="FileStream"/> to write to.
        /// Any existing contents in the file will be overwritten.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="autoSaveFile1"/> and/or <paramref name="autoSaveFile2"/> are null.
        /// </exception>
        public AutoSaveTextFile(FileStream autoSaveFile1, FileStream autoSaveFile2)
        {
            this.autoSaveFile1 = autoSaveFile1 ?? throw new ArgumentNullException(nameof(autoSaveFile1));
            this.autoSaveFile2 = autoSaveFile2 ?? throw new ArgumentNullException(nameof(autoSaveFile2));
        }

        internal FileStream Switch(FileStream autoSaveFile)
            => autoSaveFile == autoSaveFile1 ? autoSaveFile2 : autoSaveFile1;

        internal string Load(FileStream autoSaveFile)
        {
            var streamReader = new StreamReader(autoSaveFile);
            int.TryParse(streamReader.ReadLine(), out int expectedLength);
            string loadedText = streamReader.ReadToEnd();

            // Integrity check: only allow loading from completed auto-save files.
            if (expectedLength != loadedText.Length) return null;
            return loadedText;
        }
    }
}

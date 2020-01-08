#region License
/*********************************************************************************
 * AutoSaveTextFile.cs
 *
 * Copyright (c) 2004-2020 Henk Nicolai
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

using Eutherion.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Eutherion.Win
{
    /// <summary>
    /// Uses a pair of <see cref="FileStream"/>s for auto-saving text files.
    /// This class is tailored for frequent sequential asynchronous writing of text, so it is a good idea
    /// to open the <see cref="FileStream"/>s with both <see cref="FileOptions.SequentialScan"/>
    /// and <see cref="FileOptions.Asynchronous"/>.
    /// </summary>
    /// <typeparam name="TUpdate">
    /// The type of updates to persist. A non-empty sequence of updates is converted to text which is then saved to the file.
    /// </typeparam>
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
    public sealed class AutoSaveTextFile<TUpdate> : IDisposable
    {
        /// <summary>
        /// Responsible for converting an arbitrary but non-empty sequence of persisted updates
        /// to a string, which can then be saved to the underlying <see cref="FileStream"/>.
        /// This conversion is done on an arbitrary background thread, and instances of this
        /// class are expected to be thread-safe.
        /// </summary>
        public abstract class RemoteState
        {
            /// <summary>
            /// Called after construction of the auto-save file with the loaded text.
            /// </summary>
            /// <param name="loadedText">
            /// The latest text contained in the auto-save file, or null if neither
            /// auto-save file could be loaded.
            /// </param>
            protected internal abstract void Initialize(string loadedText);

            /// <summary>
            /// Converts a non-empty sequence of persisted updates to a string
            /// so it can be saved to the underlying <see cref="FileStream"/>.
            /// </summary>
            /// <param name="updates">
            /// The non-empty sequence of persisted updates to convert.
            /// </param>
            /// <param name="textToSave">
            /// If this function returns true, the text to save. Otherwise, the default string value.
            /// </param>
            /// <returns>
            /// Whether or not to auto-save the result.
            /// </returns>
            protected internal abstract bool ShouldSave(IReadOnlyList<TUpdate> updates, out string textToSave);
        }

        // This value seem to be recommended.
        private const int CharBufferSize = 1024;

        /// <summary>
        /// Minimal delay in milliseconds between two auto-save operations.
        /// </summary>
        public static readonly int AutoSaveDelay = 1000;

        /// <summary>
        /// The auto-save files.
        /// </summary>
        internal readonly FileStreamPair AutoSaveFiles;

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
        /// Contains scheduled updates to the auto-save file.
        /// </summary>
        private readonly ConcurrentQueue<TUpdate> updateQueue;

        /// <summary>
        /// Camcels the long running auto-save background task.
        /// </summary>
        private readonly CancellationTokenSource cts;

        /// <summary>
        /// Long running auto-save background task.
        /// </summary>
        private readonly Task autoSaveBackgroundTask;

        /// <summary>
        /// Gets if this <see cref="AutoSaveTextFile{TUpdate}"/> is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="AutoSaveTextFile{TUpdate}"/>.
        /// </summary>
        /// <param name="remoteState">
        /// Object responsible for converting updates to text.
        /// </param>
        /// <param name="autoSaveFiles">
        /// The <see cref="FileStreamPair"/> containing <see cref="FileStream"/>s to write to.
        /// Any existing contents in the files will be overwritten.
        /// <see cref="AutoSaveTextFile{TUpdate}"/> assumes ownership of the <see cref="FileStream"/>s
        /// so it takes care of disposing it after use.
        /// To be used as an auto-save <see cref="FileStream"/>,
        /// it must support seeking, reading and writing, and not be able to time out.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="remoteState"/> and/or <paramref name="autoSaveFiles"/> are null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// One or both <see cref="FileStream"/>s in <paramref name="autoSaveFiles"/>
        /// do not have the right capabilities to be used as an auto-save file stream.
        /// See also: <seealso cref="CanAutoSaveTo"/>.
        /// </exception>
        public AutoSaveTextFile(RemoteState remoteState, FileStreamPair autoSaveFiles)
        {
            if (remoteState == null) throw new ArgumentNullException(nameof(remoteState));
            AutoSaveFiles = autoSaveFiles ?? throw new ArgumentNullException(nameof(autoSaveFiles));

            // Assert capabilities of the file streams.
            VerifyFileStream(autoSaveFiles.FileStream1, nameof(autoSaveFiles));
            VerifyFileStream(autoSaveFiles.FileStream2, nameof(autoSaveFiles));

            // Immediately attempt to load the saved contents from either FileStream.
            // Choose first auto-save file to load from.
            FileStream latestAutoSaveFile = autoSaveFiles.FileStream1.Length == 0 ? autoSaveFiles.FileStream2 : autoSaveFiles.FileStream1;

            string loadedText = null;
            try
            {
                loadedText = Load(latestAutoSaveFile);
            }
            catch (Exception firstLoadException)
            {
                // Trace and try the other auto-save file as a backup.
                firstLoadException.Trace();
            }

            // If null is returned from the first Load(), the integrity check failed.
            if (loadedText == null)
            {
                latestAutoSaveFile = autoSaveFiles.Different(latestAutoSaveFile);

                try
                {
                    loadedText = Load(latestAutoSaveFile);
                }
                catch (Exception secondLoadException)
                {
                    secondLoadException.Trace();
                }
            }

            // Initialize remote state with the loaded text.
            // If both reads failed, loadedText == null.
            remoteState.Initialize(loadedText);

            // Initialize encoder and buffers.
            // Always use UTF8 for auto-saved text files.
            Encoding encoding = Encoding.UTF8;
            encoder = encoding.GetEncoder();
            buffer = new char[CharBufferSize];
            encodedBuffer = new byte[encoding.GetMaxByteCount(CharBufferSize)];

            // Set up long running task to keep auto-saving updates.
            updateQueue = new ConcurrentQueue<TUpdate>();
            cts = new CancellationTokenSource();
            autoSaveBackgroundTask = Task.Run(() => AutoSaveLoop(latestAutoSaveFile, remoteState, cts.Token));
        }

        private void VerifyFileStream(FileStream fileStream, string parameterName)
        {
            if (!AutoSaveTextFile.CanAutoSaveTo(fileStream))
            {
                throw new ArgumentException(
                    $"One of the file streams in '{parameterName}' does not have the right capabilities to be used as an auto-save file stream.",
                    parameterName);
            }
        }

        private string Load(FileStream autoSaveFile)
        {
            var streamReader = new StreamReader(autoSaveFile, Encoding.UTF8);

            // Try other file if empty.
            if (!uint.TryParse(streamReader.ReadLine(), out uint expectedLength)) return null;
            if (expectedLength == 0) return null;

            string loadedText = streamReader.ReadToEnd();

            // Integrity check: only allow loading from completed auto-save files.
            if (expectedLength != (uint)loadedText.Length) return null;

            // Only accept valid UTF8 files.
            if (streamReader.CurrentEncoding != Encoding.UTF8) return null;

            return loadedText;
        }

        /// <summary>
        /// Persists an update to the auto-save file.
        /// Update objects must be thread-safe, or not contain any shared state.
        /// </summary>
        /// <param name="update">
        /// The update to persist.
        /// </param>
        public void Persist(TUpdate update) => updateQueue.Enqueue(update);

        private async Task WriteToFileAsync(FileStream targetFile, string textToSave)
        {
            const char newLineChar = '\n';

            // How much of the output still needs to be written.
            int remainingLength = textToSave.Length;

            // For zero length text, do not write anything at all.
            if (remainingLength == 0) return;

            // Write the length of the text plus a newline character before the rest, to aid recovery from crashes.
            // The length of its string representation will be smaller than CharBufferSize
            // until the day string lengths exceed 10¹⁰²⁴ - 2.
            string firstLine = remainingLength.ToStringInvariant() + newLineChar;
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
                // CharBufferSize is known to be equal to buffer.Length.
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

        private async Task AutoSaveLoop(FileStream lastWrittenToFile, RemoteState remoteState, CancellationToken ct)
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

                // Empty the queue, create a local (thread-safe) list of updates to process.
                bool hasUpdate = updateQueue.TryDequeue(out TUpdate firstUpdate);

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
                    List<TUpdate> updates = new List<TUpdate> { firstUpdate };
                    while (updateQueue.TryDequeue(out TUpdate update)) updates.Add(update);

                    try
                    {
                        if (remoteState.ShouldSave(updates, out string textToSave))
                        {
                            // Alternate between both auto-save files.
                            FileStream targetFile = AutoSaveFiles.Different(lastWrittenToFile);

                            // Only truly necessary in the first iteration if the targetFile was initially a corrupt non-empty file.
                            // Theoretically, two thrown writeExceptions would have the same effect.
                            // In other cases, lastWrittenToFile.SetLength(0) below will already have done this.
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
        /// Finishes auto-saving remaining updates, then closes the encapsulated <see cref="FileStream"/>s.
        /// </summary>
        public void Dispose()
        {
            if (!IsDisposed)
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

                cts.Dispose();
                AutoSaveFiles.Dispose();
                IsDisposed = true;
            }
        }
    }

    /// <summary>
    /// Contains utility methods for <see cref="AutoSaveTextFile{TUpdate}"/>.
    /// </summary>
    public static class AutoSaveTextFile
    {
        /// <summary>
        /// Returns if a <see cref="FileStream"/> has the right capabilities to be used for an <see cref="AutoSaveTextFile{TUpdate}"/>.
        /// The <see cref="FileStream"/> must support seeking, reading, writing, and must be guaranteed to not time-out.
        /// </summary>
        /// <param name="fileStream">
        /// The <see cref="FileStream"/> to check.
        /// </param>
        /// <returns>
        /// Whether or not the <see cref="FileStream"/> has sufficient capabilities to be used for an <see cref="AutoSaveTextFile{TUpdate}"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="fileStream"/> is null.
        /// </exception>
        public static bool CanAutoSaveTo(FileStream fileStream)
        {
            if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));

            return fileStream.CanSeek
                && fileStream.CanRead
                && fileStream.CanWrite
                && !fileStream.CanTimeout;
        }

        /// <summary>
        /// Opens an existing file in such a way that it can be used as an auto-save <see cref="FileStream"/>.
        /// Only this process can write to the <see cref="FileStream"/>.
        /// If the file does not exist, it is created.
        /// </summary>
        /// <param name="autoSaveFilePath">
        /// The relative or absolute path to the auto-save file.
        /// </param>
        /// <returns>
        /// The open auto-save <see cref="FileStream"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="autoSaveFilePath"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="autoSaveFilePath"/> is empty, contains only whitespace, or contains invalid characters
        /// (see also <seealso cref="Path.GetInvalidPathChars"/>), or is in an invalid format,
        /// or is a relative path and its absolute path could not be resolved.
        /// </exception>
        /// <exception cref="IOException">
        /// Part of the path which is expected to be a directory is actually a file,
        /// -or- The path contains a network name which cannot be resolved,
        /// -or- <paramref name="autoSaveFilePath"/> is longer than its maximum length (this is OS specific).
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have sufficient permissions to open or create the file.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have sufficient permissions to open or create the file.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// <paramref name="autoSaveFilePath"/> is invalid.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="autoSaveFilePath"/> is in an invalid format.
        /// </exception>
        public static FileStream OpenExistingAutoSaveFile(string autoSaveFilePath)
            => new FileStream(autoSaveFilePath,
                              FileMode.OpenOrCreate,
                              FileAccess.ReadWrite,
                              FileShare.Read,
                              FileUtilities.DefaultFileStreamBufferSize,
                              FileOptions.SequentialScan | FileOptions.Asynchronous);
    }
}

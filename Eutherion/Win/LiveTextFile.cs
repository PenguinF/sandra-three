﻿#region License
/*********************************************************************************
 * LiveTextFile.cs
 *
 * Copyright (c) 2004-2021 Henk Nicolai
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

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Eutherion.Win
{
    /// <summary>
    /// References a text file, and watches it for changes on the file system.
    /// </summary>
    public class LiveTextFile : IDisposable
    {
        private static bool IsExternalCauseFileException(Exception exception) =>
            exception is IOException ||
            exception is UnauthorizedAccessException ||
            exception is FileNotFoundException ||
            exception is DirectoryNotFoundException ||
            exception is SecurityException;

        /// <summary>
        /// Returns the full path to the live text file.
        /// </summary>
        public string AbsoluteFilePath { get; }

        private readonly FileWatcher watcher;

        // Thread synchronization fields.
        private readonly object updateSentinel = new object(); // Used to synchronize access to IsDirty.
        private readonly CancellationTokenSource cts;
        private readonly AutoResetEvent fileChangeSignalWaitHandle;
        private readonly ConcurrentQueue<FileChangeType> fileChangeQueue;
        private readonly Task pollFileChangesBackgroundTask;

        private readonly SynchronizationContext sc;
        private Union<Exception, string> loadedText;

        /// <summary>
        /// Gets if this <see cref="LiveTextFile"/> was updated and <see cref="LoadedText"/>
        /// will be reloaded on the next call.
        /// </summary>
        public bool IsDirty { get; private set; }

        /// <summary>
        /// Gets if this <see cref="LiveTextFile"/> is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="LiveTextFile"/>
        /// watching a file at a specific path.
        /// </summary>
        /// <param name="path">
        /// The path of the file to watch.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="path"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="path"/> is empty, contains only whitespace, or contains invalid characters
        /// (see also <seealso cref="Path.GetInvalidPathChars"/>), or is in an invalid format,
        /// or is a relative path and its absolute path could not be resolved.
        /// </exception>
        /// <exception cref="IOException">
        /// <paramref name="path"/> is longer than its maximum length (this is OS specific).
        /// </exception>
        /// <exception cref="SecurityException">
        /// The caller does not have sufficient permissions to read the file.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="path"/> is in an invalid format.
        /// </exception>
        public LiveTextFile(string path)
        {
            AbsoluteFilePath = FileUtilities.NormalizeFilePath(path);

            try
            {
                // Ensure the directory is created before creating the file watcher.
                // TODO: Extend FileWatcher so it works even when the directory
                //       doesn't exist yet.
                Directory.CreateDirectory(Path.GetDirectoryName(AbsoluteFilePath));
            }
            catch (Exception exception)
            {
                // 'Expected' exceptions can be traced, but rethrow developer errors.
                if (IsExternalCauseFileException(exception)) exception.Trace(); else throw;
            }

            IsDirty = true;
            watcher = new FileWatcher(AbsoluteFilePath);

            // Set up file change listener thread.
            cts = new CancellationTokenSource();
            fileChangeSignalWaitHandle = new AutoResetEvent(false);
            fileChangeQueue = new ConcurrentQueue<FileChangeType>();
            watcher.EnableRaisingEvents(fileChangeSignalWaitHandle, fileChangeQueue);

            // Capture synchronization context to raise events.
            sc = SynchronizationContext.Current;

            pollFileChangesBackgroundTask = Task.Run(() => PollFileChangesLoop(cts.Token));
        }

        /// <summary>
        /// Gets the loaded file as text in memory, or an exception if it could not be loaded.
        /// </summary>
        public Union<Exception, string> LoadedText
        {
            get
            {
                while (IsDirty)
                {
                    bool mustLoad = false;

                    lock (updateSentinel)
                    {
                        mustLoad = IsDirty;
                        IsDirty = false;
                    }

                    if (mustLoad)
                    {
                        // Must load again because of missed changes.
                        Load();
                    }
                }

                return loadedText;
            }
        }

        private void Load()
        {
            try
            {
                loadedText = FileUtilities.ReadAllText(AbsoluteFilePath);
            }
            catch (Exception exception)
            {
                // 'Expected' exceptions can be traced, but rethrow developer errors.
                if (!IsExternalCauseFileException(exception)) throw;
                loadedText = exception;
            }
        }

        /// <summary>
        /// Saves the text to the file.
        /// </summary>
        /// <param name="contents">
        /// The text to save.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="contents"/> is null.
        /// </exception>
        public void Save(string contents)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(AbsoluteFilePath));
            File.WriteAllText(AbsoluteFilePath, contents);
        }

        /// <summary>
        /// Occurs when the contents of the opened file changed.
        /// </summary>
        public event Action<LiveTextFile, EventArgs> FileUpdated;

        /// <summary>
        /// Raises the <see cref="FileUpdated"/> event.
        /// </summary>
        /// <param name="e">
        /// The event data.
        /// </param>
        protected virtual void OnFileUpdated(EventArgs e) => FileUpdated?.Invoke(this, e);

        private void PollFileChangesLoop(CancellationToken cancellationToken)
        {
            try
            {
                for (; ; )
                {
                    // Wait for a signal, then a tiny delay to buffer updates, and only then raise the event.
                    if (WaitHandle.WaitAny(new[] { fileChangeSignalWaitHandle, cancellationToken.WaitHandle }) != 0)
                    {
                        // cancellationToken was set.
                        break;
                    }

                    while (fileChangeSignalWaitHandle.WaitOne(50))
                    {
                        if (cancellationToken.IsCancellationRequested) break;
                    }

                    bool hasChanges = false;
                    bool disconnected = false;
                    while (fileChangeQueue.TryDequeue(out FileChangeType fileChangeType))
                    {
                        hasChanges |= fileChangeType != FileChangeType.ErrorUnspecified;
                        disconnected |= fileChangeType == FileChangeType.ErrorUnspecified;
                    }

                    if (cancellationToken.IsCancellationRequested) break;

                    if (hasChanges)
                    {
                        lock (updateSentinel)
                        {
                            IsDirty = true;
                            sc.Post(RaiseFileUpdatedEvent, null);
                        }
                    }

                    // Stop the loop if the FileWatcher errored out.
                    if (disconnected)
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                // In theory WaitOne() and Post() can throw, but it's extremely unlikely
                // in Windows 7 environments. Tracing the exception here is enough, but stop listening.
                exception.Trace();
            }

            watcher.Dispose();
        }

        private void RaiseFileUpdatedEvent(object state)
        {
            OnFileUpdated(EventArgs.Empty);
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                cts.Cancel();

                try
                {
                    pollFileChangesBackgroundTask.Wait();
                }
                catch
                {
                    // Any exceptions here must be ignored.
                }

                cts.Dispose();
                IsDirty = false;
                IsDisposed = true;
            }
        }
    }
}

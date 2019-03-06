#region License
/*********************************************************************************
 * LiveTextFile.cs
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

using Eutherion.Utils;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
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
        protected static bool IsExternalCauseFileException(Exception exception) =>
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
        private CancellationTokenSource cts;
        private AutoResetEvent fileChangeSignalWaitHandle;
        private ConcurrentQueue<FileChangeType> fileChangeQueue;
        private SynchronizationContext sc;
        private Task pollFileChangesBackgroundTask;

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
            AbsoluteFilePath = Path.GetFullPath(path);
            watcher = new FileWatcher(AbsoluteFilePath);
        }

        /// <summary>
        /// Gets the loaded file as text in memory, or an exception if it could not be loaded.
        /// </summary>
        public Union<Exception, string> LoadedText { get; private set; }

        protected void Load()
        {
            try
            {
                LoadedText = File.ReadAllText(AbsoluteFilePath);
            }
            catch (Exception exception)
            {
                // 'Expected' exceptions can be traced, but rethrow developer errors.
                if (IsExternalCauseFileException(exception)) exception.Trace(); else throw;
                LoadedText = exception;
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

        readonly WeakEvent<LiveTextFile, EventArgs> event_FileChanged = new WeakEvent<LiveTextFile, EventArgs>();

        /// <summary>
        /// <see cref="WeakEvent"/> which occurs when the contents of the opened file changed.
        /// </summary>
        public event Action<LiveTextFile, EventArgs> FileChanged
        {
            add
            {
                event_FileChanged.AddListener(value);
                StartWatching();
            }
            remove
            {
                event_FileChanged.RemoveListener(value);
            }
        }

        protected virtual void OnFileChanged(EventArgs e) => event_FileChanged.Raise(this, e);

        protected void StartWatching()
        {
            if (cts == null)
            {
                // Capture the synchronization context so events can be posted to it.
                sc = SynchronizationContext.Current;
                Debug.Assert(sc != null);

                // Set up file change listener thread.
                cts = new CancellationTokenSource();
                fileChangeSignalWaitHandle = new AutoResetEvent(false);
                fileChangeQueue = new ConcurrentQueue<FileChangeType>();
                watcher.EnableRaisingEvents(fileChangeSignalWaitHandle, fileChangeQueue);

                pollFileChangesBackgroundTask = Task.Run(() => PollFileChangesLoop(cts.Token));
            }
        }

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
                        Load();
                        if (cancellationToken.IsCancellationRequested) break;
                        sc.Post(RaiseFileChangedEvent, null);
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
                // In theory WaitOne() and Send() can throw, but it's extremely unlikely
                // in Windows 7 environments. Tracing the exception here is enough, but stop listening.
                exception.Trace();
            }

            watcher.Dispose();
        }

        private void RaiseFileChangedEvent(object state)
        {
            OnFileChanged(EventArgs.Empty);
        }

        public void Dispose()
        {
            if (cts != null)
            {
                cts.Cancel();
                try
                {
                    pollFileChangesBackgroundTask.Wait();
                    cts.Dispose();
                    cts = null;
                }
                catch
                {
                    // Any exceptions here must be ignored.
                }
            }
        }
    }
}

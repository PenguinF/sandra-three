/*********************************************************************************
 * FileWatcher.cs
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
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Sandra.UI.WF.Storage
{
    internal class FileWatcher : IDisposable
    {
        private readonly string filePath;

        private FileSystemWatcher fileSystemWatcher;

        private SynchronizationContext sc;

        public FileWatcher(string filePath)
        {
            this.filePath = filePath;
        }

        private FileSystemWatcher createFileSystemWatcher()
        {
            var fileSystemWatcher = new FileSystemWatcher(Path.GetDirectoryName(filePath), Path.GetFileName(filePath));
            fileSystemWatcher.Changed += watcher_ChangedCreatedDeleted;
            fileSystemWatcher.Created += watcher_ChangedCreatedDeleted;
            fileSystemWatcher.Deleted += watcher_ChangedCreatedDeleted;
            fileSystemWatcher.Renamed += watcher_Renamed;
            fileSystemWatcher.Error += watcher_Error;
            return fileSystemWatcher;
        }

        /// <summary>
        /// Make sure to only turn this on when the UI message loop is active.
        /// </summary>
        public void EnableRaisingEvents()
        {
            if (fileSystemWatcher == null)
            {
                // Capture the synchronization context so events can be posted to it.
                sc = SynchronizationContext.Current;
                Debug.Assert(sc != null);

                fileSystemWatcher = createFileSystemWatcher();
                fileSystemWatcher.EnableRaisingEvents = true;
            }
        }

        public event Action FileChanged;

        private enum FileChangeType
        {
            Change,
            ErrorBufferOverflow,
            ErrorUnspecified,
        }

        private void fileChangePosted(object state)
        {
            // Back on the UI thread, called from the background, so check if EnableRaisingEvents is still synchronized.
            if (fileSystemWatcher != null)
            {
                FileChangeType fileChangeType = (FileChangeType)state;

                if (fileChangeType == FileChangeType.ErrorBufferOverflow)
                {
                    // Recreate the watcher, also raise the Changed event.
                    fileSystemWatcher.Dispose();
                    fileSystemWatcher = createFileSystemWatcher();
                    fileSystemWatcher.EnableRaisingEvents = true;
                }

                if (fileChangeType != FileChangeType.ErrorUnspecified)
                {
                    FileChanged?.Invoke();
                }
            }
        }

        private void watcher_ChangedCreatedDeleted(object sender, FileSystemEventArgs e)
            => sc.Post(fileChangePosted, FileChangeType.Change);

        private void watcher_Renamed(object sender, RenamedEventArgs e)
            => sc.Post(fileChangePosted, FileChangeType.Change);

        private void watcher_Error(object sender, ErrorEventArgs e)
        {
            fileSystemWatcher.EnableRaisingEvents = false;
            if (e.GetException() is InternalBufferOverflowException)
            {
                sc.Post(fileChangePosted, FileChangeType.ErrorBufferOverflow);
            }
            else
            {
                sc.Post(fileChangePosted, FileChangeType.ErrorUnspecified);
            }
        }

        public void Dispose()
        {
            if (fileSystemWatcher != null)
            {
                fileSystemWatcher.Dispose();
                fileSystemWatcher = null;
            }
        }
    }
}

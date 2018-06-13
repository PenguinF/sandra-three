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
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace Sandra.UI.WF.Storage
{
    internal class FileWatcher : IDisposable
    {
        private readonly string filePath;

        private FileSystemWatcher fileSystemWatcher;
        private ConcurrentQueue<FileChangeType> fileChangeQueue;
        private EventWaitHandle eventWaitHandle;

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

        public void EnableRaisingEvents(EventWaitHandle eventWaitHandle, ConcurrentQueue<FileChangeType> fileChangeQueue)
        {
            if (fileSystemWatcher == null)
            {
                this.fileChangeQueue = fileChangeQueue;
                this.eventWaitHandle = eventWaitHandle;

                fileSystemWatcher = createFileSystemWatcher();
                fileSystemWatcher.EnableRaisingEvents = true;
            }
        }

        private void fileChangePosted(FileChangeType fileChangeType)
        {
            // Enqueue the file change, and signal wait handle to wake up listener thread.
            fileChangeQueue.Enqueue(fileChangeType);
            eventWaitHandle.Set();
        }

        private void watcher_ChangedCreatedDeleted(object sender, FileSystemEventArgs e)
            => fileChangePosted(FileChangeType.Change);

        private void watcher_Renamed(object sender, RenamedEventArgs e)
            => fileChangePosted(FileChangeType.Change);

        private void watcher_Error(object sender, ErrorEventArgs e)
        {
            fileSystemWatcher.EnableRaisingEvents = false;
            if (e.GetException() is InternalBufferOverflowException)
            {
                // Recreate the watcher.
                fileSystemWatcher.Dispose();
                fileSystemWatcher = createFileSystemWatcher();
                fileSystemWatcher.EnableRaisingEvents = true;
                fileChangePosted(FileChangeType.ErrorBufferOverflow);
            }
            else
            {
                fileChangePosted(FileChangeType.ErrorUnspecified);
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

    internal enum FileChangeType
    {
        Change,
        ErrorBufferOverflow,
        ErrorUnspecified,
    }
}

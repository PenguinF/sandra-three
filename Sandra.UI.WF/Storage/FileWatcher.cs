#region License
/*********************************************************************************
 * FileWatcher.cs
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

        private FileSystemWatcher CreateFileSystemWatcher()
        {
            var fileSystemWatcher = new FileSystemWatcher(Path.GetDirectoryName(filePath), Path.GetFileName(filePath));
            fileSystemWatcher.Changed += Watcher_ChangedCreatedDeleted;
            fileSystemWatcher.Created += Watcher_ChangedCreatedDeleted;
            fileSystemWatcher.Deleted += Watcher_ChangedCreatedDeleted;
            fileSystemWatcher.Renamed += Watcher_Renamed;
            fileSystemWatcher.Error += Watcher_Error;
            return fileSystemWatcher;
        }

        public void EnableRaisingEvents(EventWaitHandle eventWaitHandle, ConcurrentQueue<FileChangeType> fileChangeQueue)
        {
            if (fileSystemWatcher == null)
            {
                this.fileChangeQueue = fileChangeQueue;
                this.eventWaitHandle = eventWaitHandle;

                fileSystemWatcher = CreateFileSystemWatcher();
                fileSystemWatcher.EnableRaisingEvents = true;
            }
        }

        private void FileChangePosted(FileChangeType fileChangeType)
        {
            // Enqueue the file change, and signal wait handle to wake up listener thread.
            fileChangeQueue.Enqueue(fileChangeType);
            eventWaitHandle.Set();
        }

        private void Watcher_ChangedCreatedDeleted(object sender, FileSystemEventArgs e)
            => FileChangePosted(FileChangeType.Change);

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
            => FileChangePosted(FileChangeType.Change);

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            fileSystemWatcher.EnableRaisingEvents = false;
            if (e.GetException() is InternalBufferOverflowException)
            {
                // Recreate the watcher.
                fileSystemWatcher.Dispose();
                fileSystemWatcher = CreateFileSystemWatcher();
                fileSystemWatcher.EnableRaisingEvents = true;
                FileChangePosted(FileChangeType.ErrorBufferOverflow);
            }
            else
            {
                FileChangePosted(FileChangeType.ErrorUnspecified);
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

/*********************************************************************************
 * SettingsFile.cs
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
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Sandra.UI.WF.Storage
{
    /// <summary>
    /// Reads settings from a file.
    /// </summary>
    public class SettingsFile
    {
        private static bool IsExternalCauseFileException(Exception exception) =>
            exception is JsonReaderException ||
            exception is IOException ||
            exception is UnauthorizedAccessException ||
            exception is FileNotFoundException ||
            exception is DirectoryNotFoundException ||
            exception is SecurityException;

        /// <summary>
        /// Creates a <see cref="SettingsFile"/> given a valid file path.
        /// </summary>
        /// <param name="absoluteFilePath">
        /// The absolute file path from which to load the settings file.
        /// If the file does not exist, if access rights are insufficient,
        /// or if the settings file is corrupt, an empty <see cref="SettingsFile"/>
        /// object is returned.
        /// </param>
        /// <param name="workingCopy">
        /// The <see cref="SettingCopy"/> in which the values are stored.
        /// </param>
        /// <returns>
        /// The created <see cref="SettingsFile"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="absoluteFilePath"/> is a zero-length string, contains only white space,
        /// or contains one or more invalid characters as defined by <see cref="Path.InvalidPathChars"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="absoluteFilePath"/> and/or <paramref name="workingCopy"/> are null.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length.
        /// For example, on Windows-based platforms, paths must be less than 248 characters,
        /// and file names must be less than 260 characters.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="absoluteFilePath"/> is in an invalid format.
        /// </exception>
        public static SettingsFile Create(string absoluteFilePath, SettingCopy workingCopy)
        {
            if (absoluteFilePath == null) throw new ArgumentNullException(nameof(absoluteFilePath));
            if (workingCopy == null) throw new ArgumentNullException(nameof(workingCopy));

            var settingsFile = new SettingsFile(absoluteFilePath, workingCopy.Commit());
            settingsFile.settings = settingsFile.Load().Commit();
            return settingsFile;
        }

        /// <summary>
        /// Returns the full path to the settings file.
        /// </summary>
        public string AbsoluteFilePath { get; }

        /// <summary>
        /// Gets the template settings into which the values from the settings file are loaded.
        /// </summary>
        public SettingObject TemplateSettings { get; }

        /// <summary>
        /// Gets the most recent version of the settings.
        /// </summary>
        public SettingObject Settings => settings;

        private SettingObject settings;

        private readonly FileWatcher watcher;

        // Thread synchronization fields.
        private AutoResetEvent fileChangeSignalWaitHandle;
        private ConcurrentQueue<FileChangeType> fileChangeQueue;
        private SynchronizationContext sc;
        private Task pollFileChangesBackgroundTask;

        private SettingsFile(string absoluteFilePath, SettingObject templateSettings)
        {
            AbsoluteFilePath = absoluteFilePath;
            TemplateSettings = templateSettings;

            watcher = new FileWatcher(absoluteFilePath);
        }

        private SettingCopy Load()
        {
            SettingCopy workingCopy = TemplateSettings.CreateWorkingCopy();

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(AbsoluteFilePath));
                string fileText = File.ReadAllText(AbsoluteFilePath);
                workingCopy.LoadFromText(new StringReader(fileText));
            }
            catch (Exception exception)
            {
                // 'Expected' exceptions can be traced, but rethrow developer errors.
                if (IsExternalCauseFileException(exception)) exception.Trace(); else throw;
            }

            return workingCopy;
        }

        private readonly WeakEvent<object, EventArgs> event_SettingsChanged = new WeakEvent<object, EventArgs>();

        /// <summary>
        /// Weak event which occurs after the <see cref="Settings"/> have been updated in the file.
        /// </summary>
        public event Action<object, EventArgs> SettingsChanged
        {
            add
            {
                event_SettingsChanged.AddListener(value);

                if (pollFileChangesBackgroundTask == null)
                {
                    // Capture the synchronization context so events can be posted to it.
                    sc = SynchronizationContext.Current;
                    Debug.Assert(sc != null);

                    // Set up file change listener thread.
                    fileChangeSignalWaitHandle = new AutoResetEvent(false);
                    fileChangeQueue = new ConcurrentQueue<FileChangeType>();
                    watcher.EnableRaisingEvents(fileChangeSignalWaitHandle, fileChangeQueue);

                    pollFileChangesBackgroundTask = Task.Run(() => pollFileChangesLoop());
                }
            }
            remove
            {
                event_SettingsChanged.RemoveListener(value);
            }
        }

        private void pollFileChangesLoop()
        {
            try
            {
                for (;;)
                {
                    // Wait for a signal, then a tiny delay to buffer updates, and only then raise the event.
                    fileChangeSignalWaitHandle.WaitOne();
                    while (fileChangeSignalWaitHandle.WaitOne(50)) ;

                    bool hasChanges = false;
                    bool disconnected = false;
                    FileChangeType fileChangeType;
                    while (fileChangeQueue.TryDequeue(out fileChangeType))
                    {
                        hasChanges |= fileChangeType != FileChangeType.ErrorUnspecified;
                        disconnected |= fileChangeType == FileChangeType.ErrorUnspecified;
                    }

                    if (hasChanges)
                    {
                        sc.Post(raiseSettingsChangedEvent, Load());
                    }

                    // Stop the loop if the FileWatcher errored out.
                    if (disconnected)
                    {
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                // In theory WaitOne() and Send() can throw, but it's extremely unlikely
                // in Windows 7 environments. Tracing the exception here is enough, but stop listening.
                exception.Trace();
            }

            watcher.Dispose();
        }

        private void raiseSettingsChangedEvent(object state)
        {
            SettingCopy workingCopy = (SettingCopy)state;
            if (!workingCopy.EqualTo(settings))
            {
                settings = workingCopy.Commit();
                event_SettingsChanged.Raise(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Attempts to overwrite the setting file with the current values in <see cref="Settings"/>.
        /// </summary>
        /// <returns>
        /// Null if the operation was successful;
        /// otherwise the <see cref="Exception"/> which caused the operation to fail.
        /// </returns>
        public Exception WriteToFile() => WriteToFile(Settings, AbsoluteFilePath, false);

        /// <summary>
        /// Attempts to overwrite a file with the current values in a settings object.
        /// </summary>
        /// <param name="settings">
        /// The settings to write.
        /// </param>
        /// <param name="absoluteFilePath">
        /// The target file to write to. If the file already exists, it is overwritten.
        /// </param>
        /// <param name="commentOutProperties">
        /// True if the properties must be commented out, otherwise false.
        /// </param>
        /// <returns>
        /// Null if the operation was successful;
        /// otherwise the <see cref="Exception"/> which caused the operation to fail.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="settings"/> and/or <paramref name="absoluteFilePath"/> is null.
        /// </exception>
        public static Exception WriteToFile(SettingObject settings, string absoluteFilePath, bool commentOutProperties)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (absoluteFilePath == null) throw new ArgumentNullException(nameof(absoluteFilePath));

            SettingWriter writer = new SettingWriter(schema: settings.Schema, compact: false, commentOutProperties: commentOutProperties);
            writer.Visit(settings.Map);
            try
            {
                File.WriteAllText(absoluteFilePath, writer.Output());
                return null;
            }
            catch (Exception exception)
            {
                if (!IsExternalCauseFileException(exception)) throw;
                return exception;
            }
        }
    }
}

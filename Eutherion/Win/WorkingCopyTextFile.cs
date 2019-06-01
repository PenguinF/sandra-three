﻿#region License
/*********************************************************************************
 * WorkingCopyTextFile.cs
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
using System.Collections.Generic;

namespace Eutherion.Win
{
    /// <summary>
    /// Creates a working copy for a <see cref="LiveTextFile"/> which keeps track of local changes
    /// which persist across sessions using an <see cref="AutoSaveTextFile{TUpdate}"/>,
    /// and is aware of external updates to the text file on the file system.
    /// </summary>
    public sealed class WorkingCopyTextFile : IDisposable
    {
        /// <summary>
        /// Contains the remote state for <see cref="WorkingCopyTextFile"/> auto-save files.
        /// </summary>
        internal class TextAutoSaveState : AutoSaveTextFile<string>.RemoteState
        {
            /// <summary>
            /// Gets the text currently stored in the auto-save file, or null if it could not be loaded.
            /// </summary>
            public string LastAutoSavedText { get; private set; }

            protected internal override void Initialize(string loadedText) => LastAutoSavedText = loadedText;

            protected internal override bool ShouldSave(IReadOnlyList<string> updates, out string textToSave)
            {
                textToSave = updates[updates.Count - 1];

                if (textToSave != LastAutoSavedText)
                {
                    // Remember what was last auto-saved.
                    LastAutoSavedText = textToSave;
                    return true;
                }

                textToSave = default(string);
                return false;
            }
        }

        /// <summary>
        /// Initializes a new <see cref="WorkingCopyTextFile"/> from an open <see cref="LiveTextFile"/>
        /// and a <see cref="FileStreamPair"/> from which to load an <see cref="AutoSaveTextFile{TUpdate}"/> with auto-saved local changes.
        /// </summary>
        /// <param name="openTextFile">
        /// The open text file, or null to create a new file.
        /// </param>
        /// <param name="autoSaveFiles">
        /// The <see cref="FileStreamPair"/> from which to load the auto-save file that contains local changes,
        /// or null to not load from an auto-save file.
        /// </param>
        public WorkingCopyTextFile(LiveTextFile openTextFile, FileStreamPair autoSaveFiles)
        {
            OpenTextFile = openTextFile;

            if (autoSaveFiles != null)
            {
                var remoteState = new TextAutoSaveState();
                AutoSaveFile = new AutoSaveTextFile<string>(remoteState, autoSaveFiles);
                string autoSavedText = remoteState.LastAutoSavedText;

                // Interpret empty auto-saved text as there being no changes.
                // Has the slightly odd effect that when someone deletes all text from the editor,
                // and then closes+reopens the application, the loaded text is shown.
                if (!string.IsNullOrEmpty(autoSavedText))
                {
                    LocalCopyText = autoSavedText;
                    return;
                }
            }

            // Initialize with the loaded text if no initial auto-saved text or corrupt auto-save files.
            LocalCopyText = LoadedText;
        }

        /// <summary>
        /// Gets the opened <see cref="LiveTextFile"/>.
        /// </summary>
        public LiveTextFile OpenTextFile { get; }

        /// <summary>
        /// Returns the full path to the opened text file, or null for new files.
        /// </summary>
        public string OpenTextFilePath => OpenTextFile?.AbsoluteFilePath;

        /// <summary>
        /// Gets the loaded file as text in memory. If it could not be loaded, returns string.Empty. For new files it returns string.Empty.
        /// </summary>
        public string LoadedText
            => OpenTextFile == null ? string.Empty
            : OpenTextFile.LoadedText.Match(whenOption1: e => string.Empty, whenOption2: text => text);

        /// <summary>
        /// Returns the <see cref="Exception"/> from an unsuccessful attempt to read the file from the file system.
        /// </summary>
        public Exception LoadException => OpenTextFile?.LoadedText.Match(whenOption1: e => e, whenOption2: _ => null);

        /// <summary>
        /// Gets the opened <see cref="AutoSaveTextFile{TUpdate}"/> or null if nothing was auto-saved yet.
        /// </summary>
        public AutoSaveTextFile<string> AutoSaveFile { get; private set; }

        /// <summary>
        /// Occurs before an auto-save operation when an auto-save file does not yet exist.
        /// </summary>
        public event Action<WorkingCopyTextFile, QueryAutoSaveFileEventArgs> QueryAutoSaveFile;

        /// <summary>
        /// Gets the current local copy version of the text.
        /// </summary>
        public string LocalCopyText { get; private set; } = string.Empty;

        /// <summary>
        /// Updates the current working copy of the text.
        /// </summary>
        /// <param name="text">
        /// The current working copy of the text.
        /// </param>
        /// <param name="containsChanges">
        /// Whether or not the text contains changes.
        /// </param>
        public void UpdateLocalCopyText(string text, bool containsChanges)
        {
            LocalCopyText = text ?? string.Empty;

            if (containsChanges)
            {
                // Auto-save local changes.
                if (AutoSaveFile == null)
                {
                    // If no listeners, AutoSaveFile remains empty and nothing is auto-saved.
                    var queryAutoSaveFileEventArgs = new QueryAutoSaveFileEventArgs();
                    QueryAutoSaveFile?.Invoke(this, queryAutoSaveFileEventArgs);
                    AutoSaveFile = queryAutoSaveFileEventArgs.AutoSaveFile;
                }

                AutoSaveFile?.Persist(LocalCopyText);
            }
            else
            {
                // Indicate that there are no local changes.
                AutoSaveFile?.Persist(string.Empty);
            }
        }

        /// <summary>
        /// Saves the text to the file.
        /// </summary>
        public void Save()
        {
            OpenTextFile.Save(LocalCopyText);
        }

        /// <summary>
        /// Disposes of all owned managed resources.
        /// </summary>
        public void Dispose()
        {
            AutoSaveFile?.Dispose();
        }
    }

    /// <summary>
    /// Provides data for the <see cref="WorkingCopyTextFile.QueryAutoSaveFile"/> event.
    /// </summary>
    public class QueryAutoSaveFileEventArgs : EventArgs
    {
        private FileStreamPair autoSaveFileStreamPair;
        internal AutoSaveTextFile<string> AutoSaveFile;

        /// <summary>
        /// Gets or sets the <see cref="FileStreamPair"/> to use for auto-saving local changes.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// One or both <see cref="FileStream"/>s in the new value for <see cref="AutoSaveFileStreamPair"/>
        /// do not have the right capabilities to be used as an auto-save file stream.
        /// </exception>
        public FileStreamPair AutoSaveFileStreamPair
        {
            get => autoSaveFileStreamPair;
            set
            {
                if (autoSaveFileStreamPair != value)
                {
                    if (autoSaveFileStreamPair != null)
                    {
                        AutoSaveFile.Dispose();
                        AutoSaveFile = null;
                    }

                    if (value != null)
                    {
                        var autoSaveState = new WorkingCopyTextFile.TextAutoSaveState();
                        AutoSaveFile = new AutoSaveTextFile<string>(autoSaveState, value);
                    }

                    // If the AutoSaveTextFile constructor threw, no update must be made to autoSaveFileStreamPair.
                    autoSaveFileStreamPair = value;
                }
            }
        }
    }
}

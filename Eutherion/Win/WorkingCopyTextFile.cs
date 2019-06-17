#region License
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
using System.IO;

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

                textToSave = default;
                return false;
            }
        }

        /// <summary>
        /// Gets if this <see cref="WorkingCopyTextFile"/> was created with the <see cref="Open(string, FileStreamPair)"/> method.
        /// </summary>
        public bool IsTextFileOwner { get; }

        /// <summary>
        /// Initializes a new <see cref="WorkingCopyTextFile"/> from a file path and a <see cref="FileStreamPair"/>
        /// from which to load an <see cref="AutoSaveTextFile{TUpdate}"/> with auto-saved local changes.
        /// </summary>
        /// <param name="path">
        /// The path of the file to load and watch, or null to create a new file.
        /// </param>
        /// <param name="autoSaveFiles">
        /// The <see cref="FileStreamPair"/> from which to load the auto-save file that contains local changes,
        /// or null to not load from an auto-save file.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="path"/> is empty, contains only whitespace, or contains invalid characters
        /// (see also <seealso cref="Path.GetInvalidPathChars"/>), or is in an invalid format,
        /// or is a relative path and its absolute path could not be resolved.
        /// </exception>
        /// <exception cref="IOException">
        /// <paramref name="path"/> is longer than its maximum length (this is OS specific).
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have sufficient permissions to read the file.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="path"/> is in an invalid format.
        /// </exception>
        /// <returns>
        /// The new <see cref="WorkingCopyTextFile"/>.
        /// </returns>
        public static WorkingCopyTextFile Open(string path, FileStreamPair autoSaveFiles)
            => new WorkingCopyTextFile(
                path == null ? null : new LiveTextFile(path),
                autoSaveFiles,
                isTextFileOwner: true);

        /// <summary>
        /// Initializes a new <see cref="WorkingCopyTextFile"/> from an open <see cref="LiveTextFile"/>
        /// and a <see cref="FileStreamPair"/> from which to load an <see cref="AutoSaveTextFile{TUpdate}"/> with auto-saved local changes.
        /// Use this constructor for <see cref="LiveTextFile"/> instances which must remain live after this
        /// <see cref="WorkingCopyTextFile"/> is disposed.
        /// It is not possible to use <see cref="Replace(string)"/> on <see cref="WorkingCopyTextFile"/> instances
        /// created with this method.
        /// </summary>
        /// <param name="openTextFile">
        /// The open text file.
        /// </param>
        /// <param name="autoSaveFiles">
        /// The <see cref="FileStreamPair"/> from which to load the auto-save file that contains local changes,
        /// or null to not load from an auto-save file.
        /// </param>
        /// <returns>
        /// The new <see cref="WorkingCopyTextFile"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="openTextFile"/> is null.
        /// </exception>
        public static WorkingCopyTextFile FromLiveTextFile(LiveTextFile openTextFile, FileStreamPair autoSaveFiles)
            => new WorkingCopyTextFile(
                openTextFile ?? throw new ArgumentNullException(nameof(openTextFile)),
                autoSaveFiles,
                isTextFileOwner: false);

        private WorkingCopyTextFile(LiveTextFile openTextFile, FileStreamPair autoSaveFiles, bool isTextFileOwner)
        {
            IsTextFileOwner = isTextFileOwner;
            OpenTextFile = openTextFile;

            if (openTextFile != null)
            {
                openTextFile.FileUpdated += OpenTextFile_FileUpdated;
            }

            if (autoSaveFiles != null)
            {
                var autoSaveState = new TextAutoSaveState();
                AutoSaveFile = new AutoSaveTextFile<string>(autoSaveState, autoSaveFiles);
                string autoSavedText = autoSaveState.LastAutoSavedText;

                // Interpret empty auto-saved text as there being no changes.
                // Has the slightly odd effect that when someone deletes all text from the editor,
                // and then closes+reopens the application, the loaded text is shown.
                if (!string.IsNullOrEmpty(autoSavedText) && autoSavedText != LoadedText)
                {
                    LocalCopyText = autoSavedText;
                    ContainsChanges = true;
                    return;
                }
            }

            // Initialize with the loaded text if no initial auto-saved text or corrupt auto-save files.
            LocalCopyText = LoadedText;
        }

        /// <summary>
        /// Gets the opened <see cref="LiveTextFile"/>.
        /// </summary>
        internal LiveTextFile OpenTextFile { get; private set; }

        /// <summary>
        /// Returns the full path to the opened text file, or null for new files.
        /// </summary>
        public string OpenTextFilePath => OpenTextFile?.AbsoluteFilePath;

        /// <summary>
        /// Occurs after <see cref="OpenTextFilePath"/> was updated.
        /// </summary>
        public event Action<WorkingCopyTextFile, OpenTextFilePathChangedEventArgs> OpenTextFilePathChanged;

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
        /// Occurs when the contents of the opened file changed.
        /// The event is not raised when there are no changes and the contents of the file remain the same.
        /// </summary>
        public event Action<WorkingCopyTextFile, EventArgs> LoadedTextChanged;

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
        /// Returns if the text contains any unsaved changes.
        /// </summary>
        public bool ContainsChanges { get; private set; }

        /// <summary>
        /// Gets if this <see cref="WorkingCopyTextFile"/> is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        private void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                var displayFilePath = OpenTextFile == null ? "<Untitled>" : "\"" + OpenTextFile.AbsoluteFilePath + "\"";
                throw new ObjectDisposedException($"{nameof(WorkingCopyTextFile)}({displayFilePath})");
            }
        }

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
            ThrowIfDisposed();

            LocalCopyText = text ?? string.Empty;
            ContainsChanges = containsChanges;

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
            ThrowIfDisposed();

            if (OpenTextFile == null)
            {
                throw new InvalidOperationException();
            }

            OpenTextFile.Save(LocalCopyText);

            // Make sure ContainsChanges is false after saving.
            // This assumes that the OpenTextFile.FileUpdated event is raised on this thread,
            // so can only be called after this method returns.
            // As a consequence, LoadedText also still has its old value.
            ContainsChanges = false;
        }

        /// <summary>
        /// Replaces the current open text file with another and overwrites the file with the current text.
        /// </summary>
        /// <param name="path">
        /// The path of the file to overwrite and watch.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="path"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="path"/> is empty, contains only whitespace, or contains invalid characters
        /// (see also <seealso cref="Path.GetInvalidPathChars"/>), or is in an invalid format,
        /// or is a relative path and its absolute path could not be resolved.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <see cref="IsTextFileOwner"/> is false, making this operation invalid.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// This <see cref="WorkingCopyTextFile"/> is disposed.
        /// </exception>
        /// <exception cref="IOException">
        /// <paramref name="path"/> is longer than its maximum length (this is OS specific).
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have sufficient permissions to read the file.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="path"/> is in an invalid format.
        /// </exception>
        public void Replace(string path)
        {
            ThrowIfDisposed();

            if (!IsTextFileOwner)
            {
                throw new InvalidOperationException();
            }

            // Only update OpenTextFile and raise event if actually different.
            // Assume we're on a Windows case insensitive file system.
            string previousOpenTextFilePath = OpenTextFilePath;
            if (OpenTextFile == null || !Path.GetFullPath(path).Equals(previousOpenTextFilePath, StringComparison.OrdinalIgnoreCase))
            {
                // Open new text file before disposing the old one.
                var oldOpenTextFile = OpenTextFile;
                var newOpenTextFile = new LiveTextFile(path);

                try
                {
                    // Save to the new file before replacing the old one.
                    newOpenTextFile.Save(LocalCopyText);
                }
                catch
                {
                    newOpenTextFile.Dispose();
                    throw;
                }

                if (oldOpenTextFile != null)
                {
                    oldOpenTextFile.FileUpdated -= OpenTextFile_FileUpdated;
                    oldOpenTextFile.Dispose();
                }

                OpenTextFile = newOpenTextFile;
                newOpenTextFile.FileUpdated += OpenTextFile_FileUpdated;
                ContainsChanges = false;

                // Raise event when everything is completed.
                OpenTextFilePathChanged?.Invoke(
                    this,
                    new OpenTextFilePathChangedEventArgs(previousOpenTextFilePath));
            }
            else
            {
                // Just save to the same OpenTextFile.
                OpenTextFile.Save(LocalCopyText);
                ContainsChanges = false;
            }
        }

        private void OpenTextFile_FileUpdated(LiveTextFile _, EventArgs e)
        {
            bool raiseEvent = true;

            if (!ContainsChanges)
            {
                if (LoadException != null)
                {
                    // Make sure to auto-save the text.
                    // This covers the case in which the file was saved and unmodified, but then deleted remotely.
                    UpdateLocalCopyText(LocalCopyText, containsChanges: true);
                }
                else
                {
                    // Reload the text if different.
                    string reloadedText = LoadedText ?? string.Empty;

                    // Without this check the undo buffer gets an extra empty entry which is weird.
                    if (LocalCopyText != reloadedText)
                    {
                        UpdateLocalCopyText(reloadedText, containsChanges: false);
                    }
                    else
                    {
                        // Block raising the event when:
                        // - ContainsChanges is false;
                        // - The reloaded text is exactly the same as LocalCopyText.
                        raiseEvent = false;
                    }
                }
            }

            if (raiseEvent)
            {
                LoadedTextChanged?.Invoke(this, e);
            }
        }

        /// <summary>
        /// Disposes of all owned managed resources.
        /// </summary>
        public void Dispose()
        {
            if (!IsDisposed)
            {
                if (OpenTextFile != null)
                {
                    OpenTextFile.FileUpdated -= OpenTextFile_FileUpdated;

                    if (IsTextFileOwner)
                    {
                        OpenTextFile.Dispose();
                    }
                }

                if (AutoSaveFile != null)
                {
                    // Dispose first, then check if the files are empty.
                    AutoSaveFile.Dispose();

                    if (!ContainsChanges)
                    {
                        // Remove auto-save files.
                        try
                        {
                            File.Delete(AutoSaveFile.AutoSaveFiles.FileStream1.Name);
                        }
                        catch (Exception deleteException)
                        {
                            deleteException.Trace();
                        }

                        try
                        {
                            File.Delete(AutoSaveFile.AutoSaveFiles.FileStream2.Name);
                        }
                        catch (Exception deleteException)
                        {
                            deleteException.Trace();
                        }

                        AutoSaveFile = null;
                    }
                }

                IsDisposed = true;
            }
        }
    }

    /// <summary>
    /// Provides data for the <see cref="WorkingCopyTextFile.OpenTextFilePathChanged"/> event.
    /// </summary>
    public class OpenTextFilePathChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the previous value of <see cref="WorkingCopyTextFile.OpenTextFilePath"/>.
        /// </summary>
        public string PreviousOpenTextFilePath { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="OpenTextFilePathChangedEventArgs"/>.
        /// </summary>
        /// <param name="previousOpenTextFilePath">
        /// The previous value of <see cref="WorkingCopyTextFile.OpenTextFilePath"/>.
        /// </param>
        public OpenTextFilePathChangedEventArgs(string previousOpenTextFilePath)
            => PreviousOpenTextFilePath = previousOpenTextFilePath;
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

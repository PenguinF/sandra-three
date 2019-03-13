#region License
/*********************************************************************************
 * SettingsAutoSave.cs
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
using System.Collections.Generic;
using System.IO;

namespace Eutherion.Win.Storage
{
    /// <summary>
    /// Manages a pair of auto-save files of settings.
    /// This class is assumed to have a lifetime equal to the application.
    /// The recommended location for the auto-save files is in a subfolder of
    /// <see cref="Environment.SpecialFolder.LocalApplicationData"/>.
    /// </summary>
    public sealed class SettingsAutoSave
    {
        private class SettingsRemoteState : AutoSaveTextFile<SettingCopy>.RemoteState
        {
            /// <summary>
            /// Settings representing how they are currently stored in the auto-save file.
            /// </summary>
            public SettingObject RemoteSettings { get; private set; }

            public SettingsRemoteState(SettingObject defaultSettings) => RemoteSettings = defaultSettings;

            public override void Initialize(string loadedText)
            {
                if (loadedText != null)
                {
                    // Load into a copy of RemoteSettings, preserving defaults.
                    var workingCopy = RemoteSettings.CreateWorkingCopy();
                    var errors = SettingReader.ReadWorkingCopy(loadedText, workingCopy);

                    if (errors.Count > 0)
                    {
                        // Leave RemoteSettings unchanged.
                        errors.ForEach(x => new SettingsAutoSaveParseException(x).Trace());
                    }
                    else
                    {
                        RemoteSettings = workingCopy.Commit();
                    }
                }
            }

            public override bool ShouldSave(IReadOnlyList<SettingCopy> updates, out string textToSave)
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
        private readonly AutoSaveTextFile<SettingCopy> autoSaveFile;

        /// <summary>
        /// Gets the <see cref="SettingObject"/> which contains the latest setting values.
        /// </summary>
        public SettingObject CurrentSettings { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="SettingsAutoSave"/> which will generate auto-save files
        /// with names <see cref="AutoSaveFileName1"/> and <see cref="AutoSaveFileName2"/> in the specified folder.
        /// </summary>
        /// <param name="path">
        /// The location of the folder in which to store the auto-save files.
        /// </param>
        /// <param name="schema">
        /// The <see cref="SettingSchema"/> to use for the auto-save files.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="path"/> and/or <paramref name="schema"/> are null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="path"/> is empty, contains only whitespace, or contains invalid characters
        /// (see also <seealso cref="Path.GetInvalidPathChars"/>), or is in an invalid format,
        /// or is a relative path and its absolute path could not be resolved.
        /// </exception>
        /// <exception cref="IOException">
        /// The path which is expected to be a directory is actually a file,
        /// -or- The path contains a network name which cannot be resolved,
        /// -or- <paramref name="path"/> is longer than its maximum length (this is OS specific).
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have sufficient permissions to create the file or its directory.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have sufficient permissions to create the file or its directory.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// <paramref name="path"/> is invalid.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="path"/> is in an invalid format.
        /// </exception>
        public SettingsAutoSave(string path, SettingSchema schema)
        {
            if (schema == null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            // Any exceptions from these two methods should not be caught but propagated to the caller.
            string absoluteFolder = Path.GetFullPath(path);
            DirectoryInfo baseDir = Directory.CreateDirectory(absoluteFolder);

            // If exclusive access to the auto-save file cannot be acquired, because e.g. an instance is already running,
            // don't throw but just disable auto-saving and use initial empty settings.
            CurrentSettings = new SettingCopy(schema).Commit();

            try
            {
                // Specify DeleteOnClose so the lock file is automatically deleted when this process exits.
                // Assuming a buffer size of 1 means less allocated memory.
                lockFile = new FileStream(
                    Path.Combine(baseDir.FullName, LockFileName),
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.Read,
                    1,
                    FileOptions.DeleteOnClose);

                // In the unlikely event that both auto-save files generate an error,
                // just initialize from CurrentSettings so auto-saves within the session are still enabled.
                var remoteState = new SettingsRemoteState(CurrentSettings);
                FileStream autoSaveFile1 = null;
                FileStream autoSaveFile2 = null;

                try
                {
                    autoSaveFile1 = CreateAutoSaveFileStream(baseDir, AutoSaveFileName1);
                    autoSaveFile2 = CreateAutoSaveFileStream(baseDir, AutoSaveFileName2);
                    autoSaveFile = new AutoSaveTextFile<SettingCopy>(remoteState, new FileStreamPair(autoSaveFile1, autoSaveFile2));
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

                // Override CurrentSettings with RemoteSettings.
                // This is thread-safe because nothing is yet persisted to autoSaveFile.
                CurrentSettings = remoteState.RemoteSettings;
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
            => new FileStream(Path.Combine(baseDir.FullName, autoSaveFileName),
                              FileMode.OpenOrCreate,
                              FileAccess.ReadWrite,
                              FileShare.Read,
                              DefaultFileStreamBufferSize,
                              FileOptions.SequentialScan | FileOptions.Asynchronous);

        private void Persist(SettingCopy workingCopy)
        {
            if (!workingCopy.EqualTo(CurrentSettings))
            {
                // Commit to CurrentSettings.
                CurrentSettings = workingCopy.Commit();

                if (autoSaveFile != null)
                {
                    // Persist a copy so its values are not shared with other threads.
                    autoSaveFile.Persist(CurrentSettings.CreateWorkingCopy());
                }
            }
        }

        /// <summary>
        /// Persists a value to the auto-save file.
        /// </summary>
        public void Persist<TValue>(SettingProperty<TValue> property, TValue value)
        {
            SettingCopy workingCopy = CurrentSettings.CreateWorkingCopy();
            workingCopy.AddOrReplace(property, value);
            Persist(workingCopy);
        }

        /// <summary>
        /// Removes a value from the auto-save file.
        /// </summary>
        public void Remove<TValue>(SettingProperty<TValue> property)
        {
            SettingCopy workingCopy = CurrentSettings.CreateWorkingCopy();
            workingCopy.Remove(property);
            Persist(workingCopy);
        }

        /// <summary>
        /// Waits for the long running auto-saver Task to finish.
        /// </summary>
        public void Close()
        {
            autoSaveFile?.Dispose();
            lockFile?.Dispose();
        }
    }

    internal class SettingsAutoSaveParseException : Exception
    {
        public static string AutoSaveFileParseMessage(JsonErrorInfo jsonErrorInfo)
        {
            string paramDisplayString = StringUtilities.ToDefaultParameterListDisplayString(jsonErrorInfo.Parameters);
            return $"{jsonErrorInfo.ErrorCode}{paramDisplayString} at position {jsonErrorInfo.Start}, length {jsonErrorInfo.Length}";
        }

        public SettingsAutoSaveParseException(JsonErrorInfo jsonErrorInfo)
            : base(AutoSaveFileParseMessage(jsonErrorInfo)) { }
    }
}

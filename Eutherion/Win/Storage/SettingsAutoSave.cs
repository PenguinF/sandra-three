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
    /// Manages an auto-save file of settings local to every non-roaming user.
    /// This class is assumed to have a lifetime equal to the application.
    /// See also: <seealso cref="Environment.SpecialFolder.LocalApplicationData"/>
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
        /// Settings as they are stored locally.
        /// </summary>
        private SettingObject localSettings;

        /// <summary>
        /// Initializes a new instance of <see cref="SettingsAutoSave"/>.
        /// </summary>
        /// <param name="appSubFolderName">
        /// The name of the subfolder to use in <see cref="Environment.SpecialFolder.LocalApplicationData"/>.
        /// </param>
        /// <param name="workingCopy">
        /// The schema to use.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="appSubFolderName"/> and/or <paramref name="workingCopy"/> are null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="appSubFolderName"/> is <see cref="string.Empty"/>,
        /// or contains one or more of the invalid characters defined in <see cref="Path.GetInvalidPathChars"/>,
        /// or targets a folder which is not a subfolder of <see cref="Environment.SpecialFolder.LocalApplicationData"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="appSubFolderName"/> contains a colon character (:) that is not part of a drive label ("C:\").
        /// </exception>
        public SettingsAutoSave(string appSubFolderName, SettingCopy workingCopy)
        {
            if (appSubFolderName == null)
            {
                throw new ArgumentNullException(nameof(appSubFolderName));
            }

            if (workingCopy == null)
            {
                throw new ArgumentNullException(nameof(workingCopy));
            }

            // Have to check for string.Empty because Path.Combine will not.
            if (appSubFolderName.Length == 0)
            {
                throw new ArgumentException($"{nameof(appSubFolderName)} is string.Empty.", nameof(appSubFolderName));
            }

            if (!SubFolderNameType.Instance.IsValid(appSubFolderName, out ITypeErrorBuilder _))
            {
                throw new ArgumentException($"{nameof(appSubFolderName)} targets AppData\\Local itself or is not a subfolder.", nameof(appSubFolderName));
            }

            // If exclusive access to the auto-save file cannot be acquired, because e.g. an instance is already running,
            // don't throw but just disable auto-saving and use initial empty settings.
            localSettings = workingCopy.Commit();

            try
            {
                string localApplicationFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                DirectoryInfo baseDir = Directory.CreateDirectory(Path.Combine(localApplicationFolder, appSubFolderName));
                lockFile = CreateAutoSaveFileStream(baseDir, LockFileName);

                // In the unlikely event that both auto-save files generate an error,
                // just initialize from localSettings so auto-saves within the session are still enabled.
                var remoteState = new SettingsRemoteState(localSettings);
                FileStream autoSaveFile1 = null;
                FileStream autoSaveFile2 = null;

                try
                {
                    autoSaveFile1 = CreateAutoSaveFileStream(baseDir, AutoSaveFileName1);
                    autoSaveFile2 = CreateAutoSaveFileStream(baseDir, AutoSaveFileName2);
                    autoSaveFile = new AutoSaveTextFile<SettingCopy>(remoteState, autoSaveFile1, autoSaveFile2);
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

                // Override localSettings with RemoteSettings.
                // This is thread-safe because nothing is yet persisted to autoSaveFile.
                localSettings = remoteState.RemoteSettings;
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

        /// <summary>
        /// Gets the <see cref="SettingObject"/> which contains the latest setting values.
        /// </summary>
        public SettingObject CurrentSettings => localSettings;

        /// <summary>
        /// Creates and returns an update operation for the auto-save file.
        /// </summary>
        public void Persist<TValue>(SettingProperty<TValue> property, TValue value)
        {
            SettingCopy workingCopy = localSettings.CreateWorkingCopy();
            workingCopy.AddOrReplace(property, value);

            if (!workingCopy.EqualTo(localSettings))
            {
                // Commit to localSettings.
                localSettings = workingCopy.Commit();

                if (autoSaveFile != null)
                {
                    // Persist a copy so its values are not shared with other threads.
                    autoSaveFile.Persist(localSettings.CreateWorkingCopy());
                }
            }
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

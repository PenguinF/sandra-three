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

            protected internal override void Initialize(string loadedText)
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

            protected internal override bool ShouldSave(IReadOnlyList<SettingCopy> updates, out string textToSave)
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
        /// Gets the name of the first auto-save file.
        /// </summary>
        public static readonly string AutoSaveFileName1 = ".autosave1";

        /// <summary>
        /// Gets the name of the second auto-save file.
        /// </summary>
        public static readonly string AutoSaveFileName2 = ".autosave2";

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
        /// <param name="schema">
        /// The <see cref="SettingSchema"/> to use for the auto-save files.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="schema"/> is null.
        /// </exception>
        public SettingsAutoSave(SettingSchema schema, DirectoryInfo baseDir)
        {
            // If exclusive access to the auto-save file cannot be acquired, because e.g. an instance is already running,
            // don't throw but just disable auto-saving and use initial empty settings.
            CurrentSettings = new SettingCopy(schema).Commit();

            try
            {
                // In the unlikely event that both auto-save files generate an error,
                // just initialize from CurrentSettings so auto-saves within the session are still enabled.
                var remoteState = new SettingsRemoteState(CurrentSettings);
                FileStreamPair autoSaveFiles = null;

                try
                {
                    autoSaveFiles = FileStreamPair.Create(
                        AutoSaveTextFile.OpenExistingAutoSaveFile,
                        Path.Combine(baseDir.FullName, AutoSaveFileName1),
                        Path.Combine(baseDir.FullName, AutoSaveFileName2));

                    autoSaveFile = new AutoSaveTextFile<SettingCopy>(remoteState, autoSaveFiles);
                }
                catch
                {
                    autoSaveFiles?.Dispose();
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

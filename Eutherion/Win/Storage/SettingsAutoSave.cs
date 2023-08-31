#region License
/*********************************************************************************
 * SettingsAutoSave.cs
 *
 * Copyright (c) 2004-2023 Henk Nicolai
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

namespace Eutherion.Win.Storage
{
    /// <summary>
    /// Manages a pair of auto-save files of settings.
    /// </summary>
    public sealed class SettingsAutoSave
    {
        private class SettingsRemoteState : AutoSaveTextFile<PMap>.RemoteState
        {
            /// <summary>
            /// Settings representing how they were stored in the auto-save file at the time it was loaded.
            /// </summary>
            public SettingObject InitialRemoteSettings { get; private set; }

            private PMap RemoteSettings;

            public SettingsRemoteState(SettingObject defaultSettings)
            {
                InitialRemoteSettings = defaultSettings;
                RemoteSettings = defaultSettings.ConvertToMap();
            }

            protected internal override void Initialize(string loadedText)
            {
                if (loadedText != null)
                {
                    // Load into a copy of RemoteSettings, preserving defaults.
                    var workingCopy = InitialRemoteSettings.CreateWorkingCopy();

                    // Leave RemoteSettings unchanged if the loaded text contained any errors.
                    if (workingCopy.TryLoadFromText(loadedText))
                    {
                        InitialRemoteSettings = workingCopy.Commit();
                        RemoteSettings = InitialRemoteSettings.ConvertToMap();
                    }
                }
            }

            protected internal override bool ShouldSave(IReadOnlyList<PMap> updates, out string textToSave)
            {
                // Only take the latest update.
                PMap latestUpdate = updates[updates.Count - 1];

                if (!latestUpdate.EqualTo(RemoteSettings))
                {
                    RemoteSettings = latestUpdate;
                    textToSave = CompactSettingWriter.ConvertToJson(latestUpdate);
                    return true;
                }

                textToSave = default;
                return false;
            }
        }

        /// <summary>
        /// Contains both auto-save files.
        /// </summary>
        private readonly AutoSaveTextFile<PMap> autoSaveFile;

        /// <summary>
        /// Gets the <see cref="SettingObject"/> which contains the latest setting values.
        /// </summary>
        public SettingObject CurrentSettings { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="SettingsAutoSave"/> which will generate auto-save files
        /// using the specified <see cref="FileStreamPair"/>.
        /// </summary>
        /// <param name="schema">
        /// The <see cref="SettingSchema"/> to use for the auto-save files.
        /// </param>
        /// <param name="autoSaveFiles">
        /// The optional <see cref="FileStreamPair"/> containing <see cref="System.IO.FileStream"/>s to write to.
        /// Any existing contents in the files will be overwritten.
        /// <see cref="AutoSaveTextFile{TUpdate}"/> assumes ownership of the <see cref="System.IO.FileStream"/>s
        /// so it takes care of disposing it after use.
        /// To be used as an auto-save <see cref="System.IO.FileStream"/>,
        /// it must support seeking, reading and writing, and not be able to time out.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="schema"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// One or both <see cref="System.IO.FileStream"/>s in <paramref name="autoSaveFiles"/>
        /// do not have the right capabilities to be used as an auto-save file stream.
        /// See also: <seealso cref="AutoSaveTextFile.CanAutoSaveTo(System.IO.FileStream)"/>.
        /// </exception>
        public SettingsAutoSave(SettingSchema schema, FileStreamPair autoSaveFiles)
        {
            // If exclusive access to the auto-save file cannot be acquired, because e.g. an instance is already running,
            // don't throw but just disable auto-saving and use initial empty settings.
            CurrentSettings = SettingObject.CreateEmpty(schema);

            // If autoSaveFiles is null, just initialize from CurrentSettings so auto-saves within the session are still enabled.
            if (autoSaveFiles != null)
            {
                var remoteState = new SettingsRemoteState(CurrentSettings);
                autoSaveFile = new AutoSaveTextFile<PMap>(remoteState, autoSaveFiles);

                // Override CurrentSettings with RemoteSettings.
                // This is thread-safe because nothing is yet persisted to autoSaveFile.
                CurrentSettings = remoteState.InitialRemoteSettings;
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
                    autoSaveFile.Persist(CurrentSettings.CreateWorkingCopy().Commit().ConvertToMap());
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
}

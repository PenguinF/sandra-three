#region License
/*********************************************************************************
 * Session.cs
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

using Eutherion.Win.Storage;
using System;

namespace Eutherion.Win.AppTemplate
{
    /// <summary>
    /// Contains all ambient state which is global to a single user session.
    /// This includes e.g. an auto-save file, settings and preferences.
    /// </summary>
    public class Session : IDisposable
    {
        public static Session Current { get; private set; }

        public static Session Configure(string appDataSubFolderName, ISettingsProvider settingsProvider)
            => Current = new Session(appDataSubFolderName, settingsProvider);

        public AutoSave AutoSave { get; private set; }

        private Session(string appDataSubFolderName, ISettingsProvider settingsProvider)
        {
            AutoSave = new AutoSave(appDataSubFolderName, new SettingCopy(settingsProvider.CreateAutoSaveSchema()));
        }

        public void Dispose()
        {
            // Wait until the auto-save background task has finished.
            AutoSave.Close();
        }
    }

    public interface ISettingsProvider
    {
        /// <summary>
        /// Gets the schema to use for the auto-save file.
        /// </summary>
        SettingSchema CreateAutoSaveSchema();
    }
}

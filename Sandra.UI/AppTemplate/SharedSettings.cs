#region License
/*********************************************************************************
 * SharedSettings.cs
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

namespace Eutherion.Win.AppTemplate
{
    public static class SharedSettings
    {
        public static readonly string DefaultLocalPreferencesFileName = "Preferences.json";

        public static readonly string DefaultLangFolderName = "Languages";

        private static readonly string AppDataSubFolderNameDescription
            = "Subfolder of %APPDATA%/Local which should be used to store persistent data. "
            + "This includes the auto-save file, or e.g. a preferences file. "
            + "Backward slashes ('\\') must be escaped in json strings (e.g. \"C:\\\\Temp\\\\temp.txt\"). "
            + "Instead, forward slashes ('/') can be used to separate directories as well.";

        public static readonly SettingProperty<string> AppDataSubFolderName = new SettingProperty<string>(
            new SettingKey(SettingKey.ToSnakeCase(nameof(AppDataSubFolderName))),
            SubFolderNameType.Instance,
            new SettingComment(AppDataSubFolderNameDescription));

        private static readonly string LocalPreferencesFileNameDescription
            = "File name in the %APPDATA%/Local subfolder which contains the user-specific preferences.";

        public static readonly SettingProperty<string> LocalPreferencesFileName = new SettingProperty<string>(
            new SettingKey(SettingKey.ToSnakeCase(nameof(LocalPreferencesFileName))),
            FileNameType.Instance,
            new SettingComment(LocalPreferencesFileNameDescription));

        private static readonly string LangFolderNameDescription
            = "Subfolder of the application directory which is scanned for language files. "
            + "Backward slashes ('\\') must be escaped in json strings (e.g. \"C:\\\\Temp\\\\temp.txt\"). "
            + "Instead, forward slashes ('/') can be used to separate directories as well.";

        public static readonly SettingProperty<string> LangFolderName = new SettingProperty<string>(
            new SettingKey(SettingKey.ToSnakeCase(nameof(LangFolderName))),
            SubFolderNameType.Instance,
            new SettingComment(LangFolderNameDescription));

        public static readonly SettingProperty<PersistableFormState> DefaultSettingsWindow = new SettingProperty<PersistableFormState>(
            new SettingKey(SettingKey.ToSnakeCase(nameof(DefaultSettingsWindow))),
            PersistableFormState.Type);

        public static readonly SettingProperty<int> DefaultSettingsErrorHeight = new SettingProperty<int>(
            new SettingKey(SettingKey.ToSnakeCase(nameof(DefaultSettingsErrorHeight))),
            PType.CLR.Int32);

        public static readonly SettingProperty<AutoSaveFileNamePair> DefaultSettingsAutoSave = new SettingProperty<AutoSaveFileNamePair>(
            new SettingKey(SettingKey.ToSnakeCase(nameof(DefaultSettingsAutoSave))),
            AutoSaveFilePairPType.Instance);

        public static readonly SettingProperty<PersistableFormState> PreferencesWindow = new SettingProperty<PersistableFormState>(
            new SettingKey(SettingKey.ToSnakeCase(nameof(PreferencesWindow))),
            PersistableFormState.Type);

        public static readonly SettingProperty<int> PreferencesErrorHeight = new SettingProperty<int>(
            new SettingKey(SettingKey.ToSnakeCase(nameof(PreferencesErrorHeight))),
            PType.CLR.Int32);

        public static readonly SettingProperty<AutoSaveFileNamePair> PreferencesAutoSave = new SettingProperty<AutoSaveFileNamePair>(
            new SettingKey(SettingKey.ToSnakeCase(nameof(PreferencesAutoSave))),
            AutoSaveFilePairPType.Instance);

        public static readonly SettingProperty<PersistableFormState> LanguageWindow = new SettingProperty<PersistableFormState>(
            new SettingKey(SettingKey.ToSnakeCase(nameof(LanguageWindow))),
            PersistableFormState.Type);

        public static readonly SettingProperty<int> LanguageErrorHeight = new SettingProperty<int>(
            new SettingKey(SettingKey.ToSnakeCase(nameof(LanguageErrorHeight))),
            PType.CLR.Int32);

        public static readonly SettingProperty<int> JsonZoom = new SettingProperty<int>(
            new SettingKey(SettingKey.ToSnakeCase(nameof(JsonZoom))),
            ScintillaZoomFactor.Instance);
    }
}

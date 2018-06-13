/*********************************************************************************
 * Settings.cs
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
using Sandra.UI.WF.Storage;
using SysExtensions;
using System;
using System.Drawing;
using System.IO;

namespace Sandra.UI.WF
{
    internal static class SettingKeys
    {
        public static readonly string DefaultAppDataSubFolderName = "SandraChess";

        public static readonly string DefaultLocalPreferencesFileName = "Preferences.settings";

        private static string localApplicationDataPath(bool isLocalSchema)
            => !isLocalSchema ? string.Empty :
            $" ({Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DefaultAppDataSubFolderName)})";

        public static SettingComment DefaultSettingsSchemaDescription(bool isLocalSchema) => new SettingComment(
            "There are generally two copies of this file, one in the directory where "
            + Path.GetFileName(typeof(Program).Assembly.Location)
            + " is located ("
            + Program.DefaultSettingsFileName
            + "), and one that lives in the local application data folder"
            + localApplicationDataPath(isLocalSchema)
            + ".",
            "Preferences in the latter file override those that are specified in the default. "
            + "In the majority of cases, only the latter file is changed, while the default "
            + "settings serve as a template.");

        private static readonly string VersionDescription
            = "Identifies the version of the set of recognized properties. The only allowed value is 1.";

        public static readonly SettingProperty<int> Version = new SettingProperty<int>(
            new SettingKey(SettingKey.ToSnakeCase(nameof(Version))),
            VersionRange.Instance,
            new SettingComment(VersionDescription));

        private sealed class VersionRange : PType.Derived<PInteger, int>
        {
            public static readonly VersionRange Instance = new VersionRange();

            private VersionRange() : base(new PType.RangedInteger(1, 1)) { }

            public override bool TryGetTargetValue(PInteger integer, out int targetValue)
            {
                targetValue = (int)integer.Value;
                return true;
            }

            public override PInteger GetBaseValue(int value) => new PInteger(value);
        }

        private static readonly string AppDataSubFolderNameDescription
            = "Subfolder of %APPDATA%/Local which should be used to store persistent data. "
            + "This includes the auto-save file, or e.g. a preferences file. "
            + "Use forward slashes to separate directories, an unrecognized escape sequence such as in \"Test\\Test\" renders the whole file unusable.";

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

        public static readonly SettingProperty<PersistableFormState> Window = new SettingProperty<PersistableFormState>(
            new SettingKey(SettingKey.ToSnakeCase(nameof(Window))),
            PersistableFormState.Type);

        public static readonly SettingProperty<MovesTextBox.MFOSettingValue> Notation = new SettingProperty<MovesTextBox.MFOSettingValue>(
            new SettingKey(SettingKey.ToSnakeCase(nameof(Notation))),
            new PType.Enumeration<MovesTextBox.MFOSettingValue>(EnumHelper<MovesTextBox.MFOSettingValue>.AllValues));

        public static readonly SettingProperty<int> Zoom = new SettingProperty<int>(
            new SettingKey(SettingKey.ToSnakeCase(nameof(Zoom))),
            PType.RichTextZoomFactor.Instance);

        private static readonly string FastNavigationPlyCountDescription
            = "The number of plies (=half moves) to move forward of backward in a game for fast navigation. "
            + $"This value must be between {FastNavigationPlyCountRange.MinPlyCount} and {FastNavigationPlyCountRange.MaxPlyCount}.";

        public static readonly SettingProperty<int> FastNavigationPlyCount = new SettingProperty<int>(
            new SettingKey(SettingKey.ToSnakeCase(nameof(FastNavigationPlyCount))),
            FastNavigationPlyCountRange.Instance,
            new SettingComment(FastNavigationPlyCountDescription));

        private sealed class FastNavigationPlyCountRange : PType.Derived<PInteger, int>
        {
            public static readonly int MinPlyCount = 2;
            public static readonly int MaxPlyCount = 40;

            public static readonly FastNavigationPlyCountRange Instance = new FastNavigationPlyCountRange();

            private FastNavigationPlyCountRange() : base(new PType.RangedInteger(MinPlyCount, MaxPlyCount)) { }

            public override bool TryGetTargetValue(PInteger integer, out int targetValue)
            {
                targetValue = (int)integer.Value;
                return true;
            }

            public override PInteger GetBaseValue(int value) => new PInteger(value);
        }

        private static readonly string DarkSquareColorDescription
            = "The color of the dark squares. This value must be in the HTML color format, "
            + "for example \"#808000\" is the Olive color.";

        public static readonly SettingProperty<Color> DarkSquareColor = new SettingProperty<Color>(
            new SettingKey(SettingKey.ToSnakeCase(nameof(DarkSquareColor))),
            OpaqueColorType.Instance,
            new SettingComment(DarkSquareColorDescription));

        private static readonly string LightSquareColorDescription
            = "The color of the light squares. This value must be in the HTML color format, "
            + "for example \"#F0E68C\" is the Khaki color.";

        public static readonly SettingProperty<Color> LightSquareColor = new SettingProperty<Color>(
            new SettingKey(SettingKey.ToSnakeCase(nameof(LightSquareColor))),
            OpaqueColorType.Instance,
            new SettingComment(LightSquareColorDescription));
    }

    internal static class Settings
    {
        public static readonly SettingSchema AutoSaveSchema = CreateAutoSaveSchema();

        public static readonly SettingSchema DefaultSettingsSchema = CreateDefaultSettingsSchema();

        public static readonly SettingSchema LocalSettingsSchema = CreateLocalSettingsSchema();

        private static SettingSchema CreateAutoSaveSchema()
        {
            return new SettingSchema(
                SettingKeys.Window,
                SettingKeys.Notation,
                SettingKeys.Zoom);
        }

        private static SettingSchema CreateDefaultSettingsSchema()
        {
            return new SettingSchema(
                SettingKeys.DefaultSettingsSchemaDescription(isLocalSchema: false),
                SettingKeys.Version,
                SettingKeys.AppDataSubFolderName,
                SettingKeys.LocalPreferencesFileName,
                SettingKeys.DarkSquareColor,
                SettingKeys.LightSquareColor,
                SettingKeys.FastNavigationPlyCount);
        }

        private static SettingSchema CreateLocalSettingsSchema()
        {
            return new SettingSchema(
                SettingKeys.DefaultSettingsSchemaDescription(isLocalSchema: true),
                SettingKeys.DarkSquareColor,
                SettingKeys.LightSquareColor,
                SettingKeys.FastNavigationPlyCount);
        }

        public static SettingCopy CreateBuiltIn()
        {
            SettingCopy defaultSettings = new SettingCopy(DefaultSettingsSchema);

            defaultSettings.AddOrReplace(SettingKeys.Version, 1);
            defaultSettings.AddOrReplace(SettingKeys.AppDataSubFolderName, SettingKeys.DefaultAppDataSubFolderName);
            defaultSettings.AddOrReplace(SettingKeys.LocalPreferencesFileName, SettingKeys.DefaultLocalPreferencesFileName);
            defaultSettings.AddOrReplace(SettingKeys.DarkSquareColor, Color.LightBlue);
            defaultSettings.AddOrReplace(SettingKeys.LightSquareColor, Color.Azure);
            defaultSettings.AddOrReplace(SettingKeys.FastNavigationPlyCount, 10);

            return defaultSettings;
        }
    }
}

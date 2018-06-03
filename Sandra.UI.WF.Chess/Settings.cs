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
using SysExtensions;
using System;
using System.IO;
using System.Text;

namespace Sandra.UI.WF
{
    internal static class SettingKeys
    {
        internal const string DefaultAppDataSubFolderName = "SandraChess";

        /// <summary>
        /// Converts a Pascal case identifier to snake case for use as a key in a settings file.
        /// </summary>
        private static string ToSnakeCase(this string pascalCaseIdentifier)
        {
            // Start with converting to lower case.
            StringBuilder snakeCase = new StringBuilder(pascalCaseIdentifier.ToLowerInvariant());

            // Start at the end so the loop index doesn't need an update after insertion of an underscore.
            // Stop at index 1, to prevent underscore before the first letter.
            for (int i = pascalCaseIdentifier.Length - 1; i > 0; --i)
            {
                // Insert underscores before letters that have changed case.
                if (pascalCaseIdentifier[i] != snakeCase[i])
                {
                    snakeCase.Insert(i, '_');
                }
            }

            return snakeCase.ToString();
        }

        private static string localApplicationDataPath(bool isLocalSchema)
            => !isLocalSchema ? string.Empty :
            $" ({Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DefaultAppDataSubFolderName)})";

        internal static SettingComment DefaultSettingsSchemaDescription(bool isLocalSchema) => new SettingComment(
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

        private const string AppDataSubFolderNameDescription
            = "Subfolder of %APPDATA%/Local which should be used to store persistent data. "
            + "This includes the auto-save file, or e.g. a preferences file. "
            + "Use forward slashes to separate directories, an unrecognized escape sequence such as in \"Test\\Test\" renders the whole file unusable.";

        internal static readonly SettingProperty<string> AppDataSubFolderName = new SettingProperty<string>(
            new SettingKey(nameof(AppDataSubFolderName).ToSnakeCase()),
            SubFolderNameType.Instance,
            new SettingComment(AppDataSubFolderNameDescription));

        internal static readonly SettingProperty<PersistableFormState> Window = new SettingProperty<PersistableFormState>(
            new SettingKey(nameof(Window).ToSnakeCase()),
            PersistableFormState.Type);

        internal static readonly SettingProperty<MovesTextBox.MFOSettingValue> Notation = new SettingProperty<MovesTextBox.MFOSettingValue>(
            new SettingKey(nameof(Notation).ToSnakeCase()),
            new PType.Enumeration<MovesTextBox.MFOSettingValue>(EnumHelper<MovesTextBox.MFOSettingValue>.AllValues));

        internal static readonly SettingProperty<int> Zoom = new SettingProperty<int>(
            new SettingKey(nameof(Zoom).ToSnakeCase()),
            PType.RichTextZoomFactor.Instance);

        private const string FastNavigationPlyCountDescription
            = "The number of plies (=half moves) to move forward of backward in a game for "
            + "fast navigation. This value must be between 2 and 40.";

        internal static readonly SettingProperty<int> FastNavigationPlyCount = new SettingProperty<int>(
            new SettingKey(nameof(FastNavigationPlyCount).ToSnakeCase()),
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
    }

    internal static class Settings
    {
        public static readonly SettingSchema AutoSaveSchema = CreateAutoSaveSchema();

        public static readonly SettingSchema DefaultSettingsSchema = CreateDefaultSettingsSchema();

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
                SettingKeys.AppDataSubFolderName,
                SettingKeys.FastNavigationPlyCount);
        }

        public static SettingCopy CreateBuiltIn()
        {
            SettingCopy defaultSettings = new SettingCopy(DefaultSettingsSchema);

            defaultSettings.AddOrReplace(SettingKeys.AppDataSubFolderName, SettingKeys.DefaultAppDataSubFolderName);

            // 10 plies == 5 moves.
            defaultSettings.AddOrReplace(SettingKeys.FastNavigationPlyCount, 10);

            return defaultSettings;
        }
    }
}

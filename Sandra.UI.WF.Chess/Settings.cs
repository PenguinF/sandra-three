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
using System.Text;

namespace Sandra.UI.WF
{
    internal static class SettingKeys
    {
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

        internal static readonly SettingProperty<string> AppDataSubFolderName = new SettingProperty<string>(
            new SettingKey(nameof(AppDataSubFolderName).ToSnakeCase()),
            PType.CLR.String);

        internal static readonly SettingProperty<PersistableFormState> Window = new SettingProperty<PersistableFormState>(
            new SettingKey(nameof(Window).ToSnakeCase()),
            PersistableFormState.Type);

        internal static readonly SettingProperty<MovesTextBox.MFOSettingValue> Notation = new SettingProperty<MovesTextBox.MFOSettingValue>(
            new SettingKey(nameof(Notation).ToSnakeCase()),
            new PType.Enumeration<MovesTextBox.MFOSettingValue>(EnumHelper<MovesTextBox.MFOSettingValue>.AllValues));

        internal static readonly SettingProperty<int> Zoom = new SettingProperty<int>(
            new SettingKey(nameof(Zoom).ToSnakeCase()),
            PType.RichTextZoomFactor.Instance);
    }

    internal static class Settings
    {
        public static SettingObject CreateDefault()
        {
            SettingCopy defaultSettings = new SettingCopy();

            defaultSettings.AddOrReplace(SettingKeys.AppDataSubFolderName, "SandraChess");

            return defaultSettings.Commit();
        }
    }
}

#region License
/*********************************************************************************
 * Program.cs
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

using Eutherion;
using Eutherion.Localization;
using Eutherion.Win.AppTemplate;
using Eutherion.Win.Storage;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Sandra.UI
{
    static class Program
    {
        internal static string ExecutableFolder { get; private set; }

        internal static SettingsFile LocalSettings { get; private set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Use built-in localizer if none is provided.
            Localizer.Current = BuiltInEnglishLocalizer.Instance;

            // Store executable folder location for later use.
            ExecutableFolder = Path.GetDirectoryName(typeof(Program).Assembly.Location);

            var settingsProvider = new SettingsProvider();
            using (var session = Session.Configure(ExecutableFolder, settingsProvider))
            {
                // After creating the auto-save file, look for a local preferences file.
                // Create a working copy with correct schema first.
                SettingCopy localSettingsCopy = new SettingCopy(settingsProvider.CreateLocalSettingsSchema());

                // And then create the local settings file which can overwrite values in default settings.
                LocalSettings = SettingsFile.Create(
                    Path.Combine(session.AppDataSubFolder, session.GetDefaultSetting(SettingKeys.LocalPreferencesFileName)),
                    localSettingsCopy);

                Chess.Constants.ForceInitialize();

#if DEBUG
                // In debug mode, generate default json configuration files from hard coded settings.
                GenerateJsonConfigurationFiles(session);
#endif

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MdiContainerForm());
            }
        }

        internal static TValue GetSetting<TValue>(SettingProperty<TValue> property)
        {
            return LocalSettings.Settings.TryGetValue(property, out TValue result)
                ? result
                : Session.Current.GetDefaultSetting(property);
        }

        internal static Image LoadImage(string imageFileKey)
        {
            try
            {
                return Image.FromFile(Path.Combine(ExecutableFolder, "Images", imageFileKey + ".png"));
            }
            catch (Exception exc)
            {
                exc.Trace();
                return null;
            }
        }

#if DEBUG
        /// <summary>
        /// Generates DefaultSettings.json from the loaded default settings in memory,
        /// and Bin/Languages/en.json from the BuiltInEnglishLocalizer.
        /// </summary>
        private static void GenerateJsonConfigurationFiles(Session session)
        {
            SettingsFile.WriteToFile(
                session.DefaultSettings.Settings,
                Path.Combine(ExecutableFolder, "DefaultSettings.json"));

            var settingCopy = new SettingCopy(Localizers.CreateLanguageFileSchema());
            settingCopy.AddOrReplace(Localizers.NativeName, "English");
            settingCopy.AddOrReplace(Localizers.FlagIconFile, "flag-uk.png");
            settingCopy.AddOrReplace(Localizers.Translations, BuiltInEnglishLocalizer.Instance.Dictionary);
            SettingsFile.WriteToFile(
                settingCopy.Commit(),
                Path.Combine(ExecutableFolder, "Languages", "en.json"),
                SettingWriterOptions.SuppressSettingComments);
        }
#endif
    }
}

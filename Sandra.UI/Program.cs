﻿#region License
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
using Eutherion.Win.AppTemplate;
using Eutherion.Win.Storage;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Sandra.UI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Use built-in localizer if none is provided.
            var builtInEnglishLocalizer = new BuiltInEnglishLocalizer(
                LocalizedStringKeys.DefaultEnglishTranslations,
                LocalizedConsoleKeys.DefaultEnglishTranslations,
                SharedLocalizedStringKeys.DefaultEnglishTranslations(Session.ExecutableFileNameWithoutExtension),
                JsonErrorInfoExtensions.DefaultEnglishJsonErrorTranslations);

            using (var session = Session.Configure(new SettingsProvider(), builtInEnglishLocalizer, builtInEnglishLocalizer.Dictionary))
            {
                Chess.Constants.ForceInitialize();

#if DEBUG
                // In debug mode, generate default json configuration files from hard coded settings.
                GenerateJsonConfigurationFiles(session, builtInEnglishLocalizer);
#endif

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var mdiContainerForm = new MdiContainerForm();

                mdiContainerForm.Load += (_, __) =>
                {
                    // Enable live updates to localizers now a message loop exists.
                    session.RegisteredLocalizers.ForEach(x => x.EnableLiveUpdates());
                };

                Application.Run(mdiContainerForm);
            }
        }

        internal static Image LoadImage(string imageFileKey)
        {
            try
            {
                return Image.FromFile(Path.Combine(Session.ExecutableFolder, "Images", imageFileKey + ".png"));
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
        private static void GenerateJsonConfigurationFiles(Session session, BuiltInEnglishLocalizer builtInEnglishLocalizer)
        {
            // No exception handler for both WriteToFiles.
            SettingsFile.WriteToFile(
                session.DefaultSettings.Settings,
                Path.Combine(Session.ExecutableFolder, "DefaultSettings.json"),
                SettingWriterOptions.Default);

            var settingCopy = new SettingCopy(Localizers.CreateLanguageFileSchema());
            settingCopy.AddOrReplace(Localizers.NativeName, "English");
            settingCopy.AddOrReplace(Localizers.FlagIconFile, "flag-uk.png");
            settingCopy.AddOrReplace(Localizers.Translations, builtInEnglishLocalizer.Dictionary);
            SettingsFile.WriteToFile(
                settingCopy.Commit(),
                Path.Combine(Session.ExecutableFolder, "Languages", "en.json"),
                SettingWriterOptions.SuppressSettingComments);
        }
#endif
    }
}

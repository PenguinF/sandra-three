﻿/*********************************************************************************
 * Program.cs
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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    static class Program
    {
        internal const string DefaultSettingsFileName = "Default.settings";

        internal static string ExecutableFolder { get; private set; }

        internal static SettingsFile DefaultSettings { get; private set; }

        internal static SettingsFile LocalSettings { get; private set; }

        internal static AutoSave AutoSave { get; private set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Store executable folder location for later use.
            ExecutableFolder = Path.GetDirectoryName(typeof(Program).Assembly.Location);

            // Attempt to load default settings.
            DefaultSettings = SettingsFile.Create(
                Path.Combine(ExecutableFolder, DefaultSettingsFileName),
                Settings.CreateBuiltIn());

            // In debug mode, make sure that DefaultSettings.json matches what's read from the file.
            WriteToSourceDefaultSettingFile();

            Localizers.Register(new EnglishLocalizer(), new DutchLocalizer());

            string appDataSubFolderName = GetDefaultSetting(SettingKeys.AppDataSubFolderName);
            AutoSave = new AutoSave(appDataSubFolderName, new SettingCopy(Settings.AutoSaveSchema));

            // After creating the auto-save file, look for a local preferences file.
            // Create a working copy with correct schema first.
            SettingCopy localSettingsCopy = new SettingCopy(Settings.LocalSettingsSchema);

            // Copy from default settings.
            var defaultSettingsObject = DefaultSettings.Settings;
            foreach (var property in localSettingsCopy.Schema.AllProperties)
            {
                // Copy by property name.
                SettingProperty defaultSettingProperty;
                PValue sourceValue;
                if (defaultSettingsObject.Schema.TryGetProperty(property.Name, out defaultSettingProperty)
                    && defaultSettingsObject.TryGetPValue(defaultSettingProperty, out sourceValue))
                {
                    localSettingsCopy.AddOrReplace(property, sourceValue);
                }
            }

            // And then create the local settings file which can overwrite values in default settings.
            LocalSettings = SettingsFile.Create(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    appDataSubFolderName,
                    GetDefaultSetting(SettingKeys.LocalPreferencesFileName)),
                localSettingsCopy);

            Chess.Constants.ForceInitialize();

            Localizer localizer;
            if (TryGetAutoSaveValue(Localizers.LangSetting, out localizer))
            {
                Localizer.Current = localizer;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MdiContainerForm());

            // Wait until the auto-save background task has finished.
            AutoSave.Close();
        }

        internal static TValue GetDefaultSetting<TValue>(SettingProperty<TValue> property)
            => DefaultSettings.Settings.GetValue(property);

        internal static TValue GetSetting<TValue>(SettingProperty<TValue> property)
            => LocalSettings.Settings.GetValue(property);

        internal static bool TryGetAutoSaveValue<TValue>(SettingProperty<TValue> property, out TValue value)
            => AutoSave.CurrentSettings.TryGetValue(property, out value);

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

        [Conditional("DEBUG")]
        private static void WriteToSourceDefaultSettingFile()
        {
            DirectoryInfo exeDir = new DirectoryInfo(ExecutableFolder);
            DirectoryInfo devDir = exeDir.Parent.GetDirectories("Sandra.UI.WF.Chess", SearchOption.TopDirectoryOnly).First();

#if DEBUG
            DefaultSettings.WriteToFile(Path.Combine(devDir.FullName, "DefaultSettings.json"));
#endif
        }
    }
}

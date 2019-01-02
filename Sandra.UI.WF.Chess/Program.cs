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

using Sandra.UI.WF.Storage;
using SysExtensions;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    static class Program
    {
        internal const string DefaultSettingsFileName = "DefaultSettings.json";

        internal static string ExecutableFolder { get; private set; }

        internal static string AppDataSubFolder { get; private set; }

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

            // Save name of APPDATA subfolder for persistent files.
            var appDataSubFolderName = GetDefaultSetting(SettingKeys.AppDataSubFolderName);
            AppDataSubFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                appDataSubFolderName);

#if DEBUG
            // In debug mode, generate default json configuration files from hard coded settings.
            GenerateJsonConfigurationFiles();
#endif

            // Scan Languages subdirectory to load localizers.
            var langFolderName = GetDefaultSetting(SettingKeys.LangFolderName);
            Localizers.ScanLocalizers(Path.Combine(ExecutableFolder, langFolderName));

            AutoSave = new AutoSave(appDataSubFolderName, new SettingCopy(Settings.CreateAutoSaveSchema()));

            // After creating the auto-save file, look for a local preferences file.
            // Create a working copy with correct schema first.
            SettingCopy localSettingsCopy = new SettingCopy(Settings.CreateLocalSettingsSchema());

            // And then create the local settings file which can overwrite values in default settings.
            LocalSettings = SettingsFile.Create(
                Path.Combine(AppDataSubFolder, GetDefaultSetting(SettingKeys.LocalPreferencesFileName)),
                localSettingsCopy);

            Chess.Constants.ForceInitialize();

            if (TryGetAutoSaveValue(Localizers.LangSetting, out FileLocalizer localizer))
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
        {
            return LocalSettings.Settings.TryGetValue(property, out TValue result)
                ? result
                : GetDefaultSetting(property);
        }

        internal static bool TryGetAutoSaveValue<TValue>(SettingProperty<TValue> property, out TValue value)
            => AutoSave.CurrentSettings.TryGetValue(property, out value);

        internal static void AttachFormStateAutoSaver(
            Form targetForm,
            SettingProperty<PersistableFormState> property,
            Action noValidFormStateAction)
        {
            bool boundsInitialized = false;

            if (TryGetAutoSaveValue(property, out PersistableFormState formState))
            {
                Rectangle targetBounds = formState.Bounds;

                // If all bounds are known initialize from those.
                // Do make sure it ends up on a visible working area.
                targetBounds.Intersect(Screen.GetWorkingArea(targetBounds));
                if (targetBounds.Width >= targetForm.MinimumSize.Width && targetBounds.Height >= targetForm.MinimumSize.Height)
                {
                    targetForm.SetBounds(targetBounds.Left, targetBounds.Top, targetBounds.Width, targetBounds.Height, BoundsSpecified.All);
                    boundsInitialized = true;
                }
            }
            else
            {
                formState = new PersistableFormState(false, Rectangle.Empty);
            }

            // Allow caller to determine a window state itself if no formState was applied successfully.
            if (!boundsInitialized && noValidFormStateAction != null)
            {
                noValidFormStateAction();
            }

            // Restore maximized setting after setting the Bounds.
            if (formState.Maximized)
            {
                targetForm.WindowState = FormWindowState.Maximized;
            }

            new FormStateAutoSaver(targetForm, property, formState);
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
        private static void GenerateJsonConfigurationFiles()
        {
            SettingsFile.WriteToFile(
                DefaultSettings.Settings,
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

    internal class FormStateAutoSaver
    {
        private readonly SettingProperty<PersistableFormState> autoSaveProperty;
        private readonly PersistableFormState formState;

        public FormStateAutoSaver(
            Form targetForm,
            SettingProperty<PersistableFormState> autoSaveProperty,
            PersistableFormState formState)
        {
            this.autoSaveProperty = autoSaveProperty;
            this.formState = formState;

            // Attach only after restoring.
            formState.AttachTo(targetForm);

            // This object goes out of scope when FormState goes out of scope,
            // which is when the target Form is closed.
            formState.Changed += FormState_Changed;
        }

        private void FormState_Changed(object sender, EventArgs e)
        {
            Program.AutoSave.Persist(autoSaveProperty, formState);
        }
    }
}

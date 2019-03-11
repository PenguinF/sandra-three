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

using Eutherion.Localization;
using Eutherion.Utils;
using Eutherion.Win.Storage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Eutherion.Win.AppTemplate
{
    /// <summary>
    /// Contains all ambient state which is global to a single user session.
    /// This includes e.g. an auto-save file, settings and preferences.
    /// </summary>
    public partial class Session : IDisposable
    {
        public static readonly string DefaultSettingsFileName = "DefaultSettings.json";

        public static readonly string LangSettingKey = "lang";

        public static readonly string ExecutableFolder;

        public static readonly string ExecutableFileName;

        public static readonly string ExecutableFileNameWithoutExtension;

        static Session()
        {
            // Store executable folder/filename for later use.
            string exePath = typeof(Session).Assembly.Location;

            ExecutableFolder = Path.GetDirectoryName(exePath);
            ExecutableFileName = Path.GetFileName(exePath);
            ExecutableFileNameWithoutExtension = Path.GetFileNameWithoutExtension(exePath);
        }

        public static Session Current { get; private set; }

        public static Session Configure(ISettingsProvider settingsProvider,
                                        Localizer defaultLocalizer,
                                        Dictionary<LocalizedStringKey, string> defaultLocalizerDictionary)
            => Current = new Session(settingsProvider, defaultLocalizer, defaultLocalizerDictionary);

        private readonly Dictionary<LocalizedStringKey, string> defaultLocalizerDictionary;
        private readonly Dictionary<string, FileLocalizer> registeredLocalizers;

        private Localizer currentLocalizer;

        private Session(ISettingsProvider settingsProvider,
                        Localizer defaultLocalizer,
                        Dictionary<LocalizedStringKey, string> defaultLocalizerDictionary)
        {
            if (settingsProvider == null) throw new ArgumentNullException(nameof(settingsProvider));

            // May be null.
            this.defaultLocalizerDictionary = defaultLocalizerDictionary;

            // This depends on ExecutableFileName.
            DeveloperMode = new SettingProperty<bool>(
                new SettingKey(SettingKey.ToSnakeCase(nameof(DeveloperMode))),
                PType.CLR.Boolean,
                new SettingComment($"Enables tools which assist with {ExecutableFileNameWithoutExtension} development and debugging."));

            // Attempt to load default settings.
            DefaultSettings = SettingsFile.Create(
                Path.Combine(ExecutableFolder, DefaultSettingsFileName),
                settingsProvider.CreateBuiltIn(this));

            // Save name of APPDATA subfolder for persistent files.
            var appDataSubFolderName = GetDefaultSetting(SharedSettings.AppDataSubFolderName);
            AppDataSubFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                appDataSubFolderName);

            // Scan Languages subdirectory to load localizers.
            var langFolderName = GetDefaultSetting(SharedSettings.LangFolderName);
            registeredLocalizers = Localizers.ScanLocalizers(this, Path.Combine(ExecutableFolder, langFolderName));

            LangSetting = new SettingProperty<FileLocalizer>(
                new SettingKey(LangSettingKey),
                new PType.KeyedSet<FileLocalizer>(registeredLocalizers));

            AutoSave = new SettingsAutoSave(appDataSubFolderName, new SettingCopy(settingsProvider.CreateAutoSaveSchema(this)));

            // After creating the auto-save file, look for a local preferences file.
            // Create a working copy with correct schema first.
            SettingCopy localSettingsCopy = new SettingCopy(settingsProvider.CreateLocalSettingsSchema(this));

            // And then create the local settings file which can overwrite values in default settings.
            LocalSettings = SettingsFile.Create(
                Path.Combine(AppDataSubFolder, GetDefaultSetting(SharedSettings.LocalPreferencesFileName)),
                localSettingsCopy);

            if (TryGetAutoSaveValue(LangSetting, out FileLocalizer localizer))
            {
                currentLocalizer = localizer;
            }
            else
            {
                // Select best fit.
                currentLocalizer = Localizers.BestFit(registeredLocalizers);
            }

            // Fall back onto defaults if still null.
            currentLocalizer = currentLocalizer ?? defaultLocalizer ?? Localizer.Default;
        }

        public SettingProperty<bool> DeveloperMode { get; }

        public string AppDataSubFolder { get; }

        public SettingsFile DefaultSettings { get; }

        public SettingsFile LocalSettings { get; }

        public SettingsAutoSave AutoSave { get; }

        public SettingProperty<FileLocalizer> LangSetting { get; }

        public IEnumerable<FileLocalizer> RegisteredLocalizers => registeredLocalizers.Select(kv => kv.Value);

        /// <summary>
        /// Enables receiving <see cref="LiveTextFile"/> updates on the UI thread.
        /// </summary>
        public void SetSynchronizationContext()
        {
            DefaultSettings.SetSynchronizationContext();
            LocalSettings.SetSynchronizationContext();
            RegisteredLocalizers.ForEach(x => x.LanguageFile.SetSynchronizationContext());
        }

        private string LocalApplicationDataPath(bool isLocalSchema)
            => !isLocalSchema ? string.Empty :
            $" ({AppDataSubFolder})";

        public SettingComment DefaultSettingsSchemaDescription(bool isLocalSchema) => new SettingComment(
            "There are generally two copies of this file, one in the directory where "
            + ExecutableFileName
            + " is located ("
            + DefaultSettingsFileName
            + "), and one that lives in the local application data folder"
            + LocalApplicationDataPath(isLocalSchema)
            + ".",
            "Preferences in the latter file override those that are specified in the default. "
            + "In the majority of cases, only the latter file is changed, while the default "
            + "settings serve as a template.");

        public TValue GetDefaultSetting<TValue>(SettingProperty<TValue> property)
            => DefaultSettings.Settings.GetValue(property);

        public TValue GetSetting<TValue>(SettingProperty<TValue> property)
        {
            return LocalSettings.Settings.TryGetValue(property, out TValue result)
                ? result
                : GetDefaultSetting(property);
        }

        public bool TryGetAutoSaveValue<TValue>(SettingProperty<TValue> property, out TValue value)
            => AutoSave.CurrentSettings.TryGetValue(property, out value);

        public void AttachFormStateAutoSaver(
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

            new FormStateAutoSaver(this, targetForm, property, formState);
        }

        /// <summary>
        /// Gets or sets the current <see cref="Localizer"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// The provided new value for <see cref="CurrentLocalizer"/> is null.
        /// </exception>
        public Localizer CurrentLocalizer
        {
            get => currentLocalizer;
            set
            {
                if (currentLocalizer != value)
                {
                    currentLocalizer = value ?? throw new ArgumentNullException(nameof(value));
                    event_CurrentLocalizerChanged.Raise(null, EventArgs.Empty);
                }
            }
        }

        private readonly WeakEvent<object, EventArgs> event_CurrentLocalizerChanged = new WeakEvent<object, EventArgs>();

        /// <summary>
        /// Occurs when the value of <see cref="CurrentLocalizer"/> is updated.
        /// </summary>
        public event Action<object, EventArgs> CurrentLocalizerChanged
        {
            add => event_CurrentLocalizerChanged.AddListener(value);
            remove => event_CurrentLocalizerChanged.RemoveListener(value);
        }

        /// <summary>
        /// Notifies listeners that the translations of the given localizer were updated.
        /// </summary>
        public void NotifyCurrentLocalizerChanged()
        {
            event_CurrentLocalizerChanged.Raise(null, EventArgs.Empty);
        }

        public void Dispose()
        {
            // Wait until the auto-save background task has finished.
            AutoSave.Close();
            LocalSettings.Dispose();
            DefaultSettings.Dispose();
            registeredLocalizers.Values.ForEach(x => x.Dispose());
        }
    }

    public interface ISettingsProvider
    {
        /// <summary>
        /// Gets the built-in default settings. Its schema is used for the default settings file.
        /// </summary>
        SettingCopy CreateBuiltIn(Session session);

        /// <summary>
        /// Gets the schema to use for the local preferences file.
        /// </summary>
        SettingSchema CreateLocalSettingsSchema(Session session);

        /// <summary>
        /// Gets the schema to use for the auto-save file.
        /// </summary>
        SettingSchema CreateAutoSaveSchema(Session session);
    }
}

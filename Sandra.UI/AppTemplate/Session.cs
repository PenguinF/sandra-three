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
    public class Session : IDisposable
    {
        public static readonly string LangSettingKey = "lang";

        public static Session Current { get; private set; }

        public static Session Configure(string executableFolder, string langFolderName, string appDataSubFolderName, ISettingsProvider settingsProvider)
            => Current = new Session(executableFolder, langFolderName, appDataSubFolderName, settingsProvider);

        private readonly Dictionary<string, FileLocalizer> registeredLocalizers;

        private Session(string executableFolder, string langFolderName, string appDataSubFolderName, ISettingsProvider settingsProvider)
        {
            registeredLocalizers = Localizers.ScanLocalizers(Path.Combine(executableFolder, langFolderName));

            LangSetting = new SettingProperty<FileLocalizer>(
                new SettingKey(LangSettingKey),
                new PType.KeyedSet<FileLocalizer>(registeredLocalizers));

            AutoSave = new AutoSave(appDataSubFolderName, new SettingCopy(settingsProvider.CreateAutoSaveSchema(this)));

            if (TryGetAutoSaveValue(LangSetting, out FileLocalizer localizer))
            {
                Localizer.Current = localizer;
            }
            else
            {
                // Select best fit.
                localizer = Localizers.BestFit(registeredLocalizers);
                if (localizer != null) Localizer.Current = localizer;
            }
        }

        public SettingProperty<FileLocalizer> LangSetting { get; }

        public AutoSave AutoSave { get; }

        public IEnumerable<FileLocalizer> RegisteredLocalizers => registeredLocalizers.Select(kv => kv.Value);

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
        SettingSchema CreateAutoSaveSchema(Session session);
    }
}

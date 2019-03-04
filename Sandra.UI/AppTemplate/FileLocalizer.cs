#region License
/*********************************************************************************
 * FileLocalizer.cs
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
using Eutherion.UIActions;
using Eutherion.Utils;
using Eutherion.Win.Storage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Eutherion.Win.AppTemplate
{
    /// <summary>
    /// Apart from being a <see cref="Localizer"/>, contains properties
    /// to allow construction of <see cref="UIAction"/> bindings and interact with settings.
    /// </summary>
    public sealed class FileLocalizer : Localizer, IWeakEventTarget
    {
        /// <summary>
        /// Gets the settings file from which this localizer is loaded.
        /// </summary>
        public SettingsFile LanguageFile { get; }

        /// <summary>
        /// Gets the name of the language in the language itself, e.g. "English", "Español", "Deutsch", ...
        /// </summary>
        public string LanguageName { get; }

        /// <summary>
        /// Gets the file name without extension of the flag icon.
        /// </summary>
        public string FlagIconFileName { get; }

        public Dictionary<LocalizedStringKey, string> Dictionary { get; private set; }

        public FileLocalizer(SettingsFile languageFile)
        {
            LanguageFile = languageFile;
            LanguageName = languageFile.Settings.GetValue(Localizers.NativeName);
            FlagIconFileName = languageFile.Settings.TryGetValue(Localizers.FlagIconFile, out string flagIconFile) ? flagIconFile : string.Empty;
            UpdateDictionary();
        }

        private void UpdateDictionary()
        {
            Dictionary = LanguageFile.Settings.TryGetValue(Localizers.Translations, out Dictionary<LocalizedStringKey, string> dict) ? dict : new Dictionary<LocalizedStringKey, string>();
        }

        public override string Localize(LocalizedStringKey localizedStringKey, string[] parameters)
            => Dictionary.TryGetValue(localizedStringKey, out string displayText)
            ? StringUtilities.ConditionalFormat(displayText, parameters)
            : Default.Localize(localizedStringKey, parameters);

        private DefaultUIActionBinding switchToLangUIActionBinding;

        public DefaultUIActionBinding SwitchToLangUIActionBinding
        {
            get
            {
                if (switchToLangUIActionBinding == null)
                {
                    Image menuIcon = null;
                    try
                    {
                        if (!string.IsNullOrEmpty(FlagIconFileName))
                        {
                            menuIcon = Image.FromFile(Path.Combine(Path.GetDirectoryName(LanguageFile.AbsoluteFilePath), FlagIconFileName));
                        }
                    }
                    catch (Exception exc)
                    {
                        exc.Trace();
                    }

                    switchToLangUIActionBinding = new DefaultUIActionBinding(
                        new UIAction(nameof(FileLocalizer) + "." + LanguageName),
                        new ImplementationSet<IUIActionInterface>
                        {
                            new CombinedUIActionInterface
                            {
                                MenuCaptionKey = LocalizedStringKey.Unlocalizable(LanguageName),
                                MenuIcon = menuIcon.ToImageProvider(),
                            },
                        });
                }

                return switchToLangUIActionBinding;
            }
        }

        public UIActionState TrySwitchToLang(bool perform)
        {
            if (perform)
            {
                Current = this;
                Session.Current.AutoSave.Persist(Session.Current.LangSetting, this);
            }

            return new UIActionState(UIActionVisibility.Enabled, Current == this);
        }

        public void EnableLiveUpdates()
        {
            // Can only happen after a message loop has been started.
            LanguageFile.RegisterSettingsChangedHandler(Localizers.Translations, TranslationsChanged);
        }

        private void TranslationsChanged(object sender, EventArgs e)
        {
            UpdateDictionary();
            NotifyChanged();
        }

        // TranslationsChanged only raises another WeakEvent so this does not indirectly leak.
        // FileLocalizers do not go out of scope during the lifetime of the application.
        bool IWeakEventTarget.IsDisposed => false;
    }
}

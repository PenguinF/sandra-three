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
    public sealed class FileLocalizer : Localizer, IWeakEventTarget, IDisposable
    {
        private class LanguageMenuItemProvider : ITextProvider, IImageProvider
        {
            private readonly FileLocalizer FileLocalizer;

            public LanguageMenuItemProvider(FileLocalizer fileLocalizer)
                => FileLocalizer = fileLocalizer;

            public string GetText() => FileLocalizer.LanguageName;

            public Image GetImage()
            {
                try
                {
                    if (!string.IsNullOrEmpty(FileLocalizer.FlagIconFileName))
                    {
                        return Image.FromFile(Path.Combine(Path.GetDirectoryName(FileLocalizer.LanguageFile.AbsoluteFilePath), FileLocalizer.FlagIconFileName));
                    }
                }
                catch (Exception exc)
                {
                    exc.Trace();
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the owner <see cref="Session"/> of this localizer.
        /// </summary>
        public Session Session { get; }

        /// <summary>
        /// Gets the settings file from which this localizer is loaded.
        /// </summary>
        public SettingsFile LanguageFile { get; }

        /// <summary>
        /// Gets the name of the language in the language itself, e.g. "English", "Español", "Deutsch", ...
        /// </summary>
        public string LanguageName { get; private set; }

        /// <summary>
        /// Gets the file name without extension of the flag icon.
        /// </summary>
        public string FlagIconFileName { get; private set; }

        public Dictionary<LocalizedStringKey, string> Dictionary { get; private set; }

        public FileLocalizer(Session session, SettingsFile languageFile)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            LanguageFile = languageFile;
            UpdateFromFile();
            UpdateDictionary();

            LanguageFile.RegisterSettingsChangedHandler(Localizers.NativeName, FileChanged);
            LanguageFile.RegisterSettingsChangedHandler(Localizers.FlagIconFile, FileChanged);
            LanguageFile.RegisterSettingsChangedHandler(Localizers.Translations, TranslationsChanged);
        }

        private void UpdateFromFile()
        {
            LanguageName = LanguageFile.Settings.GetValue(Localizers.NativeName);
            FlagIconFileName = LanguageFile.Settings.TryGetValue(Localizers.FlagIconFile, out string flagIconFile) ? flagIconFile : string.Empty;
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
                    var languageMenuItemProvider = new LanguageMenuItemProvider(this);

                    switchToLangUIActionBinding = new DefaultUIActionBinding(
                        new UIAction(nameof(FileLocalizer) + "." + LanguageFile.AbsoluteFilePath),
                        new ImplementationSet<IUIActionInterface>
                        {
                            new CombinedUIActionInterface
                            {
                                MenuTextProvider = languageMenuItemProvider,
                                MenuIcon = languageMenuItemProvider,
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
                Session.CurrentLocalizer = this;
                Session.AutoSave.Persist(Session.LangSetting, this);
            }

            return new UIActionState(UIActionVisibility.Enabled, Session.CurrentLocalizer == this);
        }

        private void FileChanged(object sender, EventArgs e)
        {
            UpdateFromFile();

            // Always update, whatever the current language is.
            Session.NotifyCurrentLocalizerChanged();
        }

        private void TranslationsChanged(object sender, EventArgs e)
        {
            UpdateDictionary();

            if (Session.CurrentLocalizer == this)
            {
                // Only update if this FileLocalizer is selected as the current language.
                Session.NotifyCurrentLocalizerChanged();
            }
        }

        // TranslationsChanged only raises another WeakEvent so this does not indirectly leak.
        // FileLocalizers do not go out of scope during the lifetime of the application.
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
            LanguageFile.Dispose();
        }
    }
}

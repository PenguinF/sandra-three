#region License
/*********************************************************************************
 * Session.ToolForms.cs
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
using Eutherion.Win.UIActions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Eutherion.Win.AppTemplate
{
    /// <summary>
    /// Contains all ambient state which is global to a single user session.
    /// This includes e.g. an auto-save file, settings and preferences.
    /// </summary>
    public partial class Session : IDisposable
    {
        public const string ToolFormsUIActionPrefix = nameof(Session) + ".";

        private readonly Box<Form> localSettingsFormBox = new Box<Form>();
        private readonly Box<Form> defaultSettingsFormBox = new Box<Form>();
        private readonly Box<Form> aboutFormBox = new Box<Form>();
        private readonly Box<Form> creditsFormBox = new Box<Form>();
        private readonly Box<Form> languageFormBox = new Box<Form>();

        private void OpenOrActivateToolForm(Form owner, Box<Form> toolForm, Func<Form> toolFormConstructor)
        {
            if (toolForm.Value == null)
            {
                // Rely on exception handler in call stack, so no try-catch here.
                toolForm.Value = toolFormConstructor();

                if (toolForm.Value != null)
                {
                    toolForm.Value.Owner = owner;
                    toolForm.Value.ShowInTaskbar = false;
                    toolForm.Value.ShowIcon = false;
                    toolForm.Value.StartPosition = FormStartPosition.CenterScreen;
                    toolForm.Value.MinimumSize = new Size(144, SystemInformation.CaptionHeight * 2);
                    toolForm.Value.FormClosed += (_, __) => toolForm.Value = null;
                }
            }

            if (toolForm.Value != null && !toolForm.Value.ContainsFocus)
            {
                toolForm.Value.Visible = true;
                toolForm.Value.Activate();
            }
        }

        public readonly DefaultUIActionBinding EditPreferencesFile = new DefaultUIActionBinding(
            new UIAction(ToolFormsUIActionPrefix + nameof(EditPreferencesFile)),
            new ImplementationSet<IUIActionInterface>
            {
                new ContextMenuUIActionInterface
                {
                    MenuCaptionKey = SharedLocalizedStringKeys.EditPreferencesFile,
                    MenuIcon = SharedResources.settings,
                },
            });

        private Form CreateSettingsForm(bool isReadOnly,
                                        SettingsFile settingsFile,
                                        SettingProperty<PersistableFormState> formStateSetting,
                                        SettingProperty<int> errorHeightSetting)
            => new SettingsForm(isReadOnly, settingsFile, formStateSetting, errorHeightSetting)
            {
                ClientSize = new Size(600, 600),
            };

        public UIActionHandlerFunc TryEditPreferencesFile(Form owner) => perform =>
        {
            if (perform)
            {
                OpenOrActivateToolForm(
                    owner,
                    localSettingsFormBox,
                    () =>
                    {
                        // If the file doesn't exist yet, generate a local settings file with a commented out copy
                        // of the default settings to serve as an example, and to show which settings are available.
                        if (!File.Exists(LocalSettings.AbsoluteFilePath))
                        {
                            SettingCopy localSettingsExample = new SettingCopy(LocalSettings.Settings.Schema);

                            var defaultSettingsObject = DefaultSettings.Settings;
                            foreach (var property in localSettingsExample.Schema.AllProperties)
                            {
                                if (defaultSettingsObject.Schema.TryGetProperty(property.Name, out SettingProperty defaultSettingProperty)
                                    && defaultSettingsObject.TryGetRawValue(defaultSettingProperty, out PValue sourceValue))
                                {
                                    localSettingsExample.AddOrReplaceRaw(property, sourceValue);
                                }
                            }

                            Exception exception = SettingsFile.WriteToFile(
                                localSettingsExample.Commit(),
                                LocalSettings.AbsoluteFilePath,
                                SettingWriterOptions.CommentOutProperties);

                            if (exception != null)
                            {
                                MessageBox.Show(exception.Message);
                                return null;
                            }
                        }

                        return CreateSettingsForm(
                            false,
                            LocalSettings,
                            SharedSettings.PreferencesWindow,
                            SharedSettings.PreferencesErrorHeight);
                    });
            }

            return UIActionVisibility.Enabled;
        };

        public readonly DefaultUIActionBinding ShowDefaultSettingsFile = new DefaultUIActionBinding(
            new UIAction(ToolFormsUIActionPrefix + nameof(ShowDefaultSettingsFile)),
            new ImplementationSet<IUIActionInterface>
            {
                new ContextMenuUIActionInterface
                {
                    MenuCaptionKey = SharedLocalizedStringKeys.ShowDefaultSettingsFile,
                },
            });

        public UIActionHandlerFunc TryShowDefaultSettingsFile(Form owner) => perform =>
        {
            if (perform)
            {
                OpenOrActivateToolForm(
                    owner,
                    defaultSettingsFormBox,
                    () =>
                    {
                        if (!File.Exists(DefaultSettings.AbsoluteFilePath))
                        {
                            // Before opening the possibly non-existent file, write to it.
                            // Ignore exceptions, may be caused by insufficient access rights.
                            DefaultSettings.WriteToFile();
                        }

                        return CreateSettingsForm(
                            !GetSetting(DeveloperMode),
                            DefaultSettings,
                            SharedSettings.DefaultSettingsWindow,
                            SharedSettings.DefaultSettingsErrorHeight);
                    });
            }

            return UIActionVisibility.Enabled;
        };

        private Form CreateReadOnlyTextForm(string fileName, int width, int height)
        {
            string text;
            try
            {
                text = File.ReadAllText(fileName);
            }
            catch (Exception exception)
            {
                // Just ignore and do nothing.
                exception.Trace();
                return null;
            }

            var textBox = new RichTextBoxEx
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                DetectUrls = true,

                ForeColor = Color.FromArgb(32, 32, 32),
                BackColor = Color.LightGray,
                Font = new Font("Consolas", 10),

                Text = text,
                ReadOnly = true,
            };

            textBox.BindStandardEditUIActions();

            UIMenu.AddTo(textBox);

            // Immediately dispose the handle to the process after creating it.
            // Won't kill the process, just release its unmanaged resource in this one.
            textBox.LinkClicked += (_, e) =>
            {
                var process = Process.Start(e.LinkText);
                if (process != null) process.Dispose();
            };

            var readOnlyTextForm = new UIActionForm
            {
                ClientSize = new Size(width, height),
                Text = Path.GetFileName(fileName),
            };

            readOnlyTextForm.Controls.Add(textBox);

            return readOnlyTextForm;
        }

        public readonly DefaultUIActionBinding OpenAbout = new DefaultUIActionBinding(
            new UIAction(ToolFormsUIActionPrefix + nameof(OpenAbout)),
            new ImplementationSet<IUIActionInterface>
            {
                new ContextMenuUIActionInterface
                {
                    MenuCaptionKey = SharedLocalizedStringKeys.About,
                },
            });

        public UIActionHandlerFunc TryOpenAbout(Form owner) => perform =>
        {
            // Assume file exists, is distributed with executable.
            // File.Exists() is too expensive to call hundreds of times.
            if (perform)
            {
                OpenOrActivateToolForm(
                    owner,
                    aboutFormBox,
                    () => CreateReadOnlyTextForm("README.txt", 600, 300));
            }

            return UIActionVisibility.Enabled;
        };

        public readonly DefaultUIActionBinding ShowCredits = new DefaultUIActionBinding(
            new UIAction(ToolFormsUIActionPrefix + nameof(ShowCredits)),
            new ImplementationSet<IUIActionInterface>
            {
                new ContextMenuUIActionInterface
                {
                    MenuCaptionKey = SharedLocalizedStringKeys.Credits,
                },
            });

        public UIActionHandlerFunc TryShowCredits(Form owner) => perform =>
        {
            // Assume file exists, is distributed with executable.
            // File.Exists() is too expensive to call hundreds of times.
            if (perform)
            {
                OpenOrActivateToolForm(
                    owner,
                    creditsFormBox,
                    () => CreateReadOnlyTextForm("Credits.txt", 700, 600));
            }

            return UIActionVisibility.Enabled;
        };

        public readonly DefaultUIActionBinding EditCurrentLanguage = new DefaultUIActionBinding(
            new UIAction(ToolFormsUIActionPrefix + nameof(EditCurrentLanguage)),
            new ImplementationSet<IUIActionInterface>
            {
                new ContextMenuUIActionInterface
                {
                    MenuCaptionKey = SharedLocalizedStringKeys.EditCurrentLanguage,
                    MenuIcon = SharedResources.speech,
                },
            });

        public UIActionHandlerFunc TryEditCurrentLanguage(Form owner) => perform =>
        {
            // Only enable in developer mode.
            if (!GetSetting(DeveloperMode)) return UIActionVisibility.Hidden;

            // Cannot edit built-in localizer.
            if (!(Localizer.Current is FileLocalizer fileLocalizer)) return UIActionVisibility.Hidden;

            if (perform)
            {
                OpenOrActivateToolForm(
                    owner,
                    languageFormBox,
                    () =>
                    {
                        // Generate translations into language file if empty.
                        if (fileLocalizer.Dictionary.Count == 0)
                        {
                            var settingCopy = new SettingCopy(fileLocalizer.LanguageFile.Settings.Schema);

                            // Fill with built-in default dictionary, or if not provided, an empty dictionary.
                            settingCopy.AddOrReplace(
                                Localizers.Translations,
                                defaultLocalizerDictionary ?? new Dictionary<LocalizedStringKey, string>());

                            // And overwrite the existing language file with this.
                            // This doesn't preserve trivia such as comments, whitespace, or even the order in which properties are given.
                            SettingsFile.WriteToFile(
                                settingCopy.Commit(),
                                fileLocalizer.LanguageFile.AbsoluteFilePath,
                                SettingWriterOptions.SuppressSettingComments);
                        }

                        return CreateSettingsForm(
                            false,
                            fileLocalizer.LanguageFile,
                            SharedSettings.LanguageWindow,
                            SharedSettings.LanguageErrorHeight);
                    });
            }

            return UIActionVisibility.Enabled;
        };
    }
}

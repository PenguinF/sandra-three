#region License
/*********************************************************************************
 * ToolForms.cs
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
using Eutherion.UIActions;
using Eutherion.Utils;
using Eutherion.Win.Storage;
using Eutherion.Win.UIActions;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Sandra.UI
{
    /// <summary>
    /// Contains a number of forms which should be displayed at most once.
    /// </summary>
    static class ToolForms
    {
        public const string ToolFormsUIActionPrefix = nameof(ToolForms) + ".";

        private static readonly Box<Form> localSettingsFormBox = new Box<Form>();
        private static readonly Box<Form> defaultSettingsFormBox = new Box<Form>();
        private static readonly Box<Form> aboutFormBox = new Box<Form>();
        private static readonly Box<Form> creditsFormBox = new Box<Form>();
        private static readonly Box<Form> languageFormBox = new Box<Form>();

        private static void OpenOrActivateToolForm(Form owner, Box<Form> toolForm, Func<Form> toolFormConstructor)
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

        public static readonly DefaultUIActionBinding EditPreferencesFile = new DefaultUIActionBinding(
            new UIAction(ToolFormsUIActionPrefix + nameof(EditPreferencesFile)),
            new UIActionBinding
            {
                ContextMenuInterface = new ContextMenuUIActionInterface
                {
                    MenuCaptionKey = LocalizedStringKeys.EditPreferencesFile,
                    MenuIcon = Properties.Resources.settings,
                },
            });

        private static Form CreateSettingsForm(bool isReadOnly,
                                               SettingsFile settingsFile,
                                               SettingProperty<PersistableFormState> formStateSetting,
                                               SettingProperty<int> errorHeightSetting)
            => new SettingsForm(isReadOnly, settingsFile, formStateSetting, errorHeightSetting)
            {
                ClientSize = new Size(600, 600),
            };

        public static UIActionHandlerFunc TryEditPreferencesFile(Form owner) => perform =>
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
                        if (!File.Exists(Program.LocalSettings.AbsoluteFilePath))
                        {
                            SettingCopy localSettingsExample = new SettingCopy(Program.LocalSettings.Settings.Schema);

                            var defaultSettingsObject = Program.DefaultSettings.Settings;
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
                                Program.LocalSettings.AbsoluteFilePath,
                                SettingWriterOptions.CommentOutProperties);

                            if (exception != null)
                            {
                                MessageBox.Show(exception.Message);
                                return null;
                            }
                        }

                        return CreateSettingsForm(
                            false,
                            Program.LocalSettings,
                            SettingKeys.PreferencesWindow,
                            SettingKeys.PreferencesErrorHeight);
                    });
            }

            return UIActionVisibility.Enabled;
        };

        public static readonly DefaultUIActionBinding ShowDefaultSettingsFile = new DefaultUIActionBinding(
            new UIAction(ToolFormsUIActionPrefix + nameof(ShowDefaultSettingsFile)),
            new UIActionBinding
            {
                ContextMenuInterface = new ContextMenuUIActionInterface
                {
                    MenuCaptionKey = LocalizedStringKeys.ShowDefaultSettingsFile,
                },
            });

        public static UIActionHandlerFunc TryShowDefaultSettingsFile(Form owner) => perform =>
        {
            if (perform)
            {
                OpenOrActivateToolForm(
                    owner,
                    defaultSettingsFormBox,
                    () =>
                    {
                        if (!File.Exists(Program.DefaultSettings.AbsoluteFilePath))
                        {
                            // Before opening the possibly non-existent file, write to it.
                            // Ignore exceptions, may be caused by insufficient access rights.
                            Program.DefaultSettings.WriteToFile();
                        }

                        return CreateSettingsForm(
                            !Program.GetSetting(SettingKeys.DeveloperMode),
                            Program.DefaultSettings,
                            SettingKeys.DefaultSettingsWindow,
                            SettingKeys.DefaultSettingsErrorHeight);
                    });
            }

            return UIActionVisibility.Enabled;
        };

        private static Form CreateReadOnlyTextForm(string fileName, int width, int height)
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

        public static readonly DefaultUIActionBinding OpenAbout = new DefaultUIActionBinding(
            new UIAction(ToolFormsUIActionPrefix + nameof(OpenAbout)),
            new UIActionBinding
            {
                ContextMenuInterface = new ContextMenuUIActionInterface
                {
                    MenuCaptionKey = LocalizedStringKeys.About,
                },
            });

        public static UIActionHandlerFunc TryOpenAbout(Form owner) => perform =>
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

        public static readonly DefaultUIActionBinding ShowCredits = new DefaultUIActionBinding(
            new UIAction(ToolFormsUIActionPrefix + nameof(ShowCredits)),
            new UIActionBinding
            {
                ContextMenuInterface = new ContextMenuUIActionInterface
                {
                    MenuCaptionKey = LocalizedStringKeys.Credits,
                },
            });

        public static UIActionHandlerFunc TryShowCredits(Form owner) => perform =>
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

        public static readonly DefaultUIActionBinding EditCurrentLanguage = new DefaultUIActionBinding(
            new UIAction(ToolFormsUIActionPrefix + nameof(EditCurrentLanguage)),
            new UIActionBinding
            {
                ContextMenuInterface = new ContextMenuUIActionInterface
                {
                    MenuCaptionKey = LocalizedStringKeys.EditCurrentLanguage,
                    MenuIcon = Properties.Resources.speech,
                },
            });

        public static UIActionHandlerFunc TryEditCurrentLanguage(Form owner) => perform =>
        {
            // Only enable in developer mode.
            if (!Program.GetSetting(SettingKeys.DeveloperMode)) return UIActionVisibility.Hidden;

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

                            // Fill with built-in English translations.
                            settingCopy.AddOrReplace(Localizers.Translations, BuiltInEnglishLocalizer.Instance.Dictionary);

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
                            SettingKeys.LanguageWindow,
                            SettingKeys.LanguageErrorHeight);
                    });
            }

            return UIActionVisibility.Enabled;
        };
    }
}

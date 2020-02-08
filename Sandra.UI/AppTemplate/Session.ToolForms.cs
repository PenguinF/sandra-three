#region License
/*********************************************************************************
 * Session.ToolForms.cs
 *
 * Copyright (c) 2004-2020 Henk Nicolai
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
using Eutherion.Text.Json;
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
        public const string SessionUIActionPrefix = nameof(Session) + ".";

        private readonly Box<Form> localSettingsFormBox = new Box<Form>();
        private readonly Box<Form> defaultSettingsFormBox = new Box<Form>();
        private readonly Box<Form> aboutFormBox = new Box<Form>();
        private readonly Box<Form> creditsFormBox = new Box<Form>();
        private readonly Box<Form> languageFormBox = new Box<Form>();

        private void OpenOrActivateToolForm(Control ownerControl, Box<Form> toolForm, Func<Form> toolFormConstructor)
        {
            if (toolForm.Value == null)
            {
                // Rely on exception handler in call stack, so no try-catch here.
                toolForm.Value = toolFormConstructor();

                if (toolForm.Value != null)
                {
                    if (ownerControl?.TopLevelControl is Form ownerForm)
                    {
                        toolForm.Value.Owner = ownerForm;
                        toolForm.Value.ShowInTaskbar = false;
                    }
                    else
                    {
                        // If ShowInTaskbar = true, the task bar displays a default icon if none is provided.
                        // Icon must be set and ShowIcon must be true to override that default icon.
                        toolForm.Value.ShowInTaskbar = true;
                        toolForm.Value.Icon = ApplicationIcon;
                        toolForm.Value.ShowIcon = true;
                    }

                    toolForm.Value.StartPosition = FormStartPosition.CenterScreen;
                    toolForm.Value.MinimumSize = new Size(144, SystemInformation.CaptionHeight * 2);
                    toolForm.Value.FormClosed += (_, __) => toolForm.Value = null;
                }
            }

            if (toolForm.Value != null)
            {
                toolForm.Value.EnsureActivated();
            }
        }

        public static readonly DefaultUIActionBinding EditPreferencesFile = new DefaultUIActionBinding(
            new UIAction(SessionUIActionPrefix + nameof(EditPreferencesFile)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    MenuTextProvider = SharedLocalizedStringKeys.EditPreferencesFile.ToTextProvider(),
                    MenuIcon = SharedResources.settings.ToImageProvider(),
                },
            });

        private Form CreateSettingsForm(SyntaxEditorCodeAccessOption codeAccessOption,
                                        SettingsFile settingsFile,
                                        Func<string> initialTextGenerator,
                                        SettingProperty<PersistableFormState> formStateSetting,
                                        SettingProperty<int> errorHeightSetting,
                                        SettingProperty<AutoSaveFileNamePair> autoSaveSetting)
        {
            var syntaxDescriptor = new SettingSyntaxDescriptor(settingsFile.Settings.Schema);

            WorkingCopyTextFile codeFile;
            WorkingCopyTextFileAutoSaver autoSaver;

            if (autoSaveSetting == null)
            {
                codeFile = WorkingCopyTextFile.FromLiveTextFile(settingsFile, null);
                autoSaver = null;
            }
            else
            {
                codeFile = WorkingCopyTextFile.FromLiveTextFile(
                    settingsFile,
                    OpenAutoSaveFileStreamPair(autoSaveSetting));

                autoSaver = new WorkingCopyTextFileAutoSaver(this, autoSaveSetting, codeFile);
            }

            // Generate initial text in case the code file could not be loaded and was not auto-saved.
            if (codeFile.LoadException != null && codeFile.AutoSaveFile == null && initialTextGenerator != null)
            {
                // This pretends that the text is what's actually saved in the settings file.
                // Conceptually this is correct because the generated text is reproducible,
                // so the application's behavior is the same whether or not the file is saved.
                // Can therefore also safely disable the save action.
                codeFile.UpdateLocalCopyText(
                    initialTextGenerator() ?? string.Empty,
                    containsChanges: false);
            }

            var settingsForm = new SyntaxEditorForm<SettingSyntaxTree, IJsonSymbol, JsonErrorInfo>(
                codeAccessOption,
                syntaxDescriptor,
                codeFile,
                formStateSetting,
                errorHeightSetting,
                SharedSettings.JsonZoom)
            {
                ClientSize = new Size(600, 600),
            };

            JsonStyleSelector<SettingSyntaxTree, JsonErrorInfo>.InitializeStyles(settingsForm.SyntaxEditor);

            if (autoSaver != null) settingsForm.Disposed += (_, __) => autoSaver.Dispose();

            return settingsForm;
        }

        public UIActionHandlerFunc TryEditPreferencesFile() => perform =>
        {
            if (perform)
            {
                OpenOrActivateToolForm(
                    null,
                    localSettingsFormBox,
                    () =>
                    {
                        // If the file doesn't exist yet, generate a local settings file with a commented out copy
                        // of the default settings to serve as an example, and to show which settings are available.
                        string initialTextGenerator()
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

                            return LocalSettings.GenerateJson(
                                localSettingsExample.Commit(),
                                SettingWriterOptions.CommentOutProperties);
                        }

                        return CreateSettingsForm(
                            SyntaxEditorCodeAccessOption.FixedFile,
                            LocalSettings,
                            initialTextGenerator,
                            SharedSettings.PreferencesWindow,
                            SharedSettings.PreferencesErrorHeight,
                            SharedSettings.PreferencesAutoSave);
                    });
            }

            return UIActionVisibility.Enabled;
        };

        public static readonly DefaultUIActionBinding ShowDefaultSettingsFile = new DefaultUIActionBinding(
            new UIAction(SessionUIActionPrefix + nameof(ShowDefaultSettingsFile)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    MenuTextProvider = SharedLocalizedStringKeys.ShowDefaultSettingsFile.ToTextProvider(),
                },
            });

        public UIActionHandlerFunc TryShowDefaultSettingsFile() => perform =>
        {
            if (perform)
            {
                OpenOrActivateToolForm(
                    null,
                    defaultSettingsFormBox,
                    () => CreateSettingsForm(
                        GetSetting(DeveloperMode) ? SyntaxEditorCodeAccessOption.FixedFile : SyntaxEditorCodeAccessOption.ReadOnly,
                        DefaultSettings,
                        () => DefaultSettings.GenerateJson(DefaultSettings.Settings, SettingWriterOptions.Default),
                        SharedSettings.DefaultSettingsWindow,
                        SharedSettings.DefaultSettingsErrorHeight,
                        SharedSettings.DefaultSettingsAutoSave));
            }

            return UIActionVisibility.Enabled;
        };

        private Form CreateReadOnlyTextForm(string fileName, int width, int height)
        {
            string text;
            try
            {
                text = FileUtilities.ReadAllText(fileName);
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

            // Use a panel with padding to add some margin around the textBox.
            var fillPanel = new Panel
            {
                BackColor = Color.LightGray,
                Dock = DockStyle.Fill,
                Padding = new Padding(6),
            };

            fillPanel.Controls.Add(textBox);

            var readOnlyTextForm = new UIActionForm
            {
                ClientSize = new Size(width, height),
                Text = Path.GetFileName(fileName),
                ShowIcon = false,
            };

            readOnlyTextForm.Controls.Add(fillPanel);

            // This adds a Close menu item to the context menu of the textBox.
            textBox.BindActions(readOnlyTextForm.StandardUIActionBindings);

            return readOnlyTextForm;
        }

        public static readonly DefaultUIActionBinding OpenAbout = new DefaultUIActionBinding(
            new UIAction(SessionUIActionPrefix + nameof(OpenAbout)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    MenuTextProvider = SharedLocalizedStringKeys.About.ToTextProvider(),
                },
            });

        public UIActionHandlerFunc TryOpenAbout(Control ownerControl) => perform =>
        {
            // Assume file exists, is distributed with executable.
            // File.Exists() is too expensive to call hundreds of times.
            if (perform)
            {
                OpenOrActivateToolForm(
                    ownerControl,
                    aboutFormBox,
                    () => CreateReadOnlyTextForm("README.txt", 600, 300));
            }

            return UIActionVisibility.Enabled;
        };

        public static readonly DefaultUIActionBinding ShowCredits = new DefaultUIActionBinding(
            new UIAction(SessionUIActionPrefix + nameof(ShowCredits)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    MenuTextProvider = SharedLocalizedStringKeys.Credits.ToTextProvider(),
                },
            });

        public UIActionHandlerFunc TryShowCredits(Control ownerControl) => perform =>
        {
            // Assume file exists, is distributed with executable.
            // File.Exists() is too expensive to call hundreds of times.
            if (perform)
            {
                OpenOrActivateToolForm(
                    ownerControl,
                    creditsFormBox,
                    () => CreateReadOnlyTextForm("Credits.txt", 700, 600));
            }

            return UIActionVisibility.Enabled;
        };

        public static readonly DefaultUIActionBinding EditCurrentLanguage = new DefaultUIActionBinding(
            new UIAction(SessionUIActionPrefix + nameof(EditCurrentLanguage)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    IsFirstInGroup = true,
                    MenuTextProvider = SharedLocalizedStringKeys.EditCurrentLanguage.ToTextProvider(),
                    MenuIcon = SharedResources.speech.ToImageProvider(),
                },
            });

        public UIActionHandlerFunc TryEditCurrentLanguage() => perform =>
        {
            // Only enable in developer mode.
            if (!GetSetting(DeveloperMode)) return UIActionVisibility.Hidden;

            // Cannot edit built-in localizer.
            if (!(CurrentLocalizer is FileLocalizer fileLocalizer)) return UIActionVisibility.Hidden;

            if (perform)
            {
                OpenOrActivateToolForm(
                    null,
                    languageFormBox,
                    () =>
                    {
                        // Generate translations into language file if empty.
                        string initialTextGenerator()
                        {
                            var settingCopy = new SettingCopy(fileLocalizer.LanguageFile.Settings.Schema);

                            // Fill with built-in default dictionary, or if not provided, an empty dictionary.
                            settingCopy.AddOrReplace(
                                Localizers.Translations,
                                defaultLocalizerDictionary ?? new Dictionary<LocalizedStringKey, string>());

                            // And overwrite the existing language file with this.
                            // This doesn't preserve trivia such as comments, whitespace, or even the order in which properties are given.
                            return fileLocalizer.LanguageFile.GenerateJson(
                                settingCopy.Commit(),
                                SettingWriterOptions.SuppressSettingComments);
                        }

                        return CreateSettingsForm(
                            SyntaxEditorCodeAccessOption.FixedFile,
                            fileLocalizer.LanguageFile,
                            initialTextGenerator,
                            SharedSettings.LanguageWindow,
                            SharedSettings.LanguageErrorHeight,
                            null);
                    });
            }

            return UIActionVisibility.Enabled;
        };

        public static readonly DefaultUIActionBinding OpenLocalAppDataFolder = new DefaultUIActionBinding(
            new UIAction(SessionUIActionPrefix + nameof(OpenLocalAppDataFolder)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    MenuTextProvider = SharedLocalizedStringKeys.OpenLocalAppDataFolder.ToTextProvider(),
                },
            });

        public UIActionHandlerFunc TryOpenLocalAppDataFolder() => perform =>
        {
            // Only enable in developer mode.
            if (!GetSetting(DeveloperMode)) return UIActionVisibility.Hidden;

            if (perform) using (Process.Start(AppDataSubFolder)) { }

            return UIActionVisibility.Enabled;
        };

        public static readonly DefaultUIActionBinding OpenExecutableFolder = new DefaultUIActionBinding(
            new UIAction(SessionUIActionPrefix + nameof(OpenExecutableFolder)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    MenuTextProvider = SharedLocalizedStringKeys.OpenExecutableFolder.ToTextProvider(),
                },
            });

        public UIActionHandlerFunc TryOpenExecutableFolder() => perform =>
        {
            // Only enable in developer mode.
            if (!GetSetting(DeveloperMode)) return UIActionVisibility.Hidden;

            if (perform) using (Process.Start(ExecutableFolder)) { }

            return UIActionVisibility.Enabled;
        };
    }
}

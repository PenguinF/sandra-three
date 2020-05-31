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

namespace Eutherion.Win.MdiAppTemplate
{
    /// <summary>
    /// Contains all ambient state which is global to a single user session.
    /// This includes e.g. an auto-save file, settings and preferences.
    /// </summary>
    public partial class Session : IDisposable
    {
        public const string SessionUIActionPrefix = nameof(Session) + ".";

        private readonly Box<MenuCaptionBarForm> localSettingsFormBox = new Box<MenuCaptionBarForm>();
        private readonly Box<MenuCaptionBarForm> defaultSettingsFormBox = new Box<MenuCaptionBarForm>();

        // Indexed by language file.
        private readonly Dictionary<string, Box<MenuCaptionBarForm>> languageFormBoxes = new Dictionary<string, Box<MenuCaptionBarForm>>(StringComparer.OrdinalIgnoreCase);

        private readonly Box<MenuCaptionBarForm> aboutFormBox = new Box<MenuCaptionBarForm>();
        private readonly Box<MenuCaptionBarForm> creditsFormBox = new Box<MenuCaptionBarForm>();

        internal void OpenOrActivateToolForm(Control ownerControl, Box<MenuCaptionBarForm> toolForm, Func<MenuCaptionBarForm> toolFormConstructor)
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
                    }

                    toolForm.Value.StartPosition = FormStartPosition.CenterScreen;
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

        private MenuCaptionBarForm CreateSettingsForm(SyntaxEditorCodeAccessOption codeAccessOption,
                                                      SettingsFile settingsFile,
                                                      Func<string> initialTextGenerator,
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

            var settingsForm = new MenuCaptionBarForm<SyntaxEditor<SettingSyntaxTree, IJsonSymbol, JsonErrorInfo>>(
                new SyntaxEditor<SettingSyntaxTree, IJsonSymbol, JsonErrorInfo>(
                    codeAccessOption,
                    syntaxDescriptor,
                    codeFile,
                    SharedSettings.JsonZoom));

            // Bind SaveToFile action to the MenuCaptionBarForm to show the save button in the caption area.
            settingsForm.BindAction(SharedUIAction.SaveToFile, settingsForm.DockedControl.TrySaveToFile);

            JsonStyleSelector<SettingSyntaxTree>.InitializeStyles(settingsForm.DockedControl);

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
                        SharedSettings.DefaultSettingsAutoSave));
            }

            return UIActionVisibility.Enabled;
        };

        private class RichTextBoxExWithMargin : ContainerControl, IDockableControl
        {
            public RichTextBoxEx TextBox { get; }

            public DockProperties DockProperties { get; } = new DockProperties();

            public RichTextBoxExWithMargin(RichTextBoxEx textBox, string fileName)
            {
                Padding = new Padding(6);
                TextBox = textBox;
                Controls.Add(TextBox);

                DockProperties.CaptionHeight = 26;
                DockProperties.CaptionText = fileName;

                ActiveControl = textBox;
            }

            event Action IDockableControl.DockPropertiesChanged { add { } remove { } }
            void IDockableControl.CanClose(CloseReason closeReason, ref bool cancel) { }
        }

        private MenuCaptionBarForm CreateReadOnlyTextForm(string fileName, int width, int height)
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
            var readOnlyTextForm = new MenuCaptionBarForm<RichTextBoxExWithMargin>(
                new RichTextBoxExWithMargin(textBox, Path.GetFileName(fileName))
                {
                    BackColor = Color.LightGray,
                })
            {
                ClientSize = new Size(width, height),
            };

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
                    languageFormBoxes.GetOrAdd(fileLocalizer.LanguageFile.AbsoluteFilePath, key => new Box<MenuCaptionBarForm>()),
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

        /// <summary>
        /// Gets if an action is considered a developer tool and is therefore allowed to be hidden in a main menu.
        /// </summary>
        internal static bool IsDeveloperTool(UIAction action)
        {
            return action == EditCurrentLanguage.Action
                || action == OpenLocalAppDataFolder.Action
                || action == OpenExecutableFolder.Action;
        }
    }
}

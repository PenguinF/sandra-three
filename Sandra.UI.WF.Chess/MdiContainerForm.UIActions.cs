﻿#region License
/*********************************************************************************
 * MdiContainerForm.UIActions.cs
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
#endregion

using Sandra.UI.WF.Storage;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Main MdiContainer Form.
    /// </summary>
    public partial class MdiContainerForm
    {
        public const string MdiContainerFormUIActionPrefix = nameof(MdiContainerForm) + ".";

        public static readonly DefaultUIActionBinding Exit = new DefaultUIActionBinding(
            new UIAction(MdiContainerFormUIActionPrefix + nameof(Exit)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                IsFirstInGroup = true,
                MenuCaptionKey = LocalizedStringKeys.Exit,
                Shortcuts = new ShortcutKeys[] { new ShortcutKeys(KeyModifiers.Alt, ConsoleKey.F4), },
            });

        public UIActionState TryExit(bool perform)
        {
            if (perform) Close();
            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding EditPreferencesFile = new DefaultUIActionBinding(
            new UIAction(MdiContainerFormUIActionPrefix + nameof(EditPreferencesFile)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.EditPreferencesFile,
            });

        private Form CreateSettingsForm(bool isReadOnly,
                                        SettingsFile settingsFile,
                                        SettingProperty<PersistableFormState> formStateSetting,
                                        SettingProperty<int> errorHeightSetting)
        {
            var errorsTextBox = new RichTextBoxBase
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical,
            };

            errorsTextBox.BindActions(new UIActionBindings
            {
                { SharedUIAction.ZoomIn, errorsTextBox.TryZoomIn },
                { SharedUIAction.ZoomOut, errorsTextBox.TryZoomOut },

                { RichTextBoxBase.CutSelectionToClipBoard, errorsTextBox.TryCutSelectionToClipBoard },
                { RichTextBoxBase.CopySelectionToClipBoard, errorsTextBox.TryCopySelectionToClipBoard },
                { RichTextBoxBase.PasteSelectionFromClipBoard, errorsTextBox.TryPasteSelectionFromClipBoard },
                { RichTextBoxBase.SelectAllText, errorsTextBox.TrySelectAllText },
            });

            UIMenu.AddTo(errorsTextBox);

            var settingsTextBox = new SettingsTextBox(settingsFile, errorsTextBox)
            {
                Dock = DockStyle.Fill,
                ReadOnly = isReadOnly,
                WordWrap = false,
                ScrollBars = RichTextBoxScrollBars.Both,
            };

            settingsTextBox.BindActions(new UIActionBindings
            {
                { SettingsTextBox.SaveToFile, settingsTextBox.TrySaveToFile },

                { SharedUIAction.ZoomIn, settingsTextBox.TryZoomIn },
                { SharedUIAction.ZoomOut, settingsTextBox.TryZoomOut },

                { RichTextBoxBase.CutSelectionToClipBoard, settingsTextBox.TryCutSelectionToClipBoard },
                { RichTextBoxBase.CopySelectionToClipBoard, settingsTextBox.TryCopySelectionToClipBoard },
                { RichTextBoxBase.PasteSelectionFromClipBoard, settingsTextBox.TryPasteSelectionFromClipBoard },
                { RichTextBoxBase.SelectAllText, settingsTextBox.TrySelectAllText },
            });

            UIMenu.AddTo(settingsTextBox);

            var settingsForm = new UIActionForm()
            {
                Owner = this,
                ClientSize = new Size(600, 600),
                ShowIcon = false,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.CenterScreen,
                Text = Path.GetFileName(settingsFile.AbsoluteFilePath),
                MinimumSize = new Size(144, SystemInformation.CaptionHeight * 2),
            };

            var splitter = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterWidth = 4,
                FixedPanel = FixedPanel.Panel2,
                Orientation = Orientation.Horizontal,
            };

            // Default value shows about 2 errors at default zoom level.
            const int defaultErrorHeight = 34;

            settingsForm.Load += (_, __) =>
            {
                Program.AttachFormStateAutoSaver(settingsForm, formStateSetting, null);

                int targetErrorHeight;
                if (!Program.TryGetAutoSaveValue(errorHeightSetting, out targetErrorHeight))
                {
                    targetErrorHeight = defaultErrorHeight;
                }

                // Calculate target splitter distance which will restore the target error height exactly.
                int splitterDistance = settingsForm.ClientSize.Height - targetErrorHeight - splitter.SplitterWidth;
                if (splitterDistance >= 0) splitter.SplitterDistance = splitterDistance;

                splitter.SplitterMoved += (___, ____) => Program.AutoSave.Persist(errorHeightSetting, errorsTextBox.Height);
            };

            splitter.Panel1.Controls.Add(settingsTextBox);
            splitter.Panel2.Controls.Add(errorsTextBox);

            settingsForm.Controls.Add(splitter);

            return settingsForm;
        }

        public UIActionState TryEditPreferencesFile(bool perform)
        {
            if (perform)
            {
                if (openLocalSettingsForm == null)
                {
                    // If the file doesn't exist yet, generate a local settings file with a commented out copy
                    // of the default settings to serve as an example, and to show which settings are available.
                    Exception exception = null;
                    if (!File.Exists(Program.LocalSettings.AbsoluteFilePath))
                    {
                        SettingCopy localSettingsExample = new SettingCopy(Program.LocalSettings.Settings.Schema);

                        var defaultSettingsObject = Program.DefaultSettings.Settings;
                        foreach (var property in localSettingsExample.Schema.AllProperties)
                        {
                            // Copy by property name.
                            SettingProperty defaultSettingProperty;
                            PValue sourceValue;
                            if (defaultSettingsObject.Schema.TryGetProperty(property.Name, out defaultSettingProperty)
                                && defaultSettingsObject.TryGetRawValue(defaultSettingProperty, out sourceValue))
                            {
                                localSettingsExample.AddOrReplaceRaw(property, sourceValue);
                            }
                        }

                        exception = SettingsFile.WriteToFile(
                            localSettingsExample.Commit(),
                            Program.LocalSettings.AbsoluteFilePath,
                            commentOutProperties: true);
                    }

                    if (exception != null)
                    {
                        MessageBox.Show(exception.Message);
                    }
                    else
                    {
                        // Rely on exception handler in call stack, so no try-catch here.
                        openLocalSettingsForm = CreateSettingsForm(
                            false,
                            Program.LocalSettings,
                            SettingKeys.PreferencesWindow,
                            SettingKeys.PreferencesErrorHeight);

                        openLocalSettingsForm.FormClosed += (_, __) => openLocalSettingsForm = null;
                    }
                }

                if (openLocalSettingsForm != null && !openLocalSettingsForm.ContainsFocus)
                {
                    openLocalSettingsForm.Visible = true;
                    openLocalSettingsForm.Activate();
                }
            }

            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding ShowDefaultSettingsFile = new DefaultUIActionBinding(
            new UIAction(MdiContainerFormUIActionPrefix + nameof(ShowDefaultSettingsFile)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.ShowDefaultSettingsFile,
            });

        public UIActionState TryShowDefaultSettingsFile(bool perform)
        {
            if (perform)
            {
                if (openDefaultSettingsForm == null)
                {
                    // Before opening the possibly non-existent file, write to it.
                    // Ignore exceptions, may be caused by insufficient access rights.
                    Program.DefaultSettings.WriteToFile();

                    // Rely on exception handler in call stack, so no try-catch here.
                    openDefaultSettingsForm = CreateSettingsForm(
                        true,
                        Program.DefaultSettings,
                        SettingKeys.DefaultSettingsWindow,
                        SettingKeys.DefaultSettingsErrorHeight);

                    openDefaultSettingsForm.FormClosed += (_, __) => openDefaultSettingsForm = null;
                }

                if (!openDefaultSettingsForm.ContainsFocus)
                {
                    openDefaultSettingsForm.Visible = true;
                    openDefaultSettingsForm.Activate();
                }
            }

            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding OpenNewPlayingBoard = new DefaultUIActionBinding(
            new UIAction(MdiContainerFormUIActionPrefix + nameof(OpenNewPlayingBoard)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.NewGame,
                Shortcuts = new ShortcutKeys[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.N), },
            });

        public UIActionState TryOpenNewPlayingBoard(bool perform)
        {
            if (perform) NewPlayingBoard();
            return UIActionVisibility.Enabled;
        }
    }
}

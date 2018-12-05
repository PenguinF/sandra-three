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
using SysExtensions;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
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
            new UIActionBinding
            {
                ShowInMenu = true,
                IsFirstInGroup = true,
                MenuCaptionKey = LocalizedStringKeys.Exit,
                Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Alt, ConsoleKey.F4), },
            });

        public UIActionState TryExit(bool perform)
        {
            if (perform) Close();
            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding EditPreferencesFile = new DefaultUIActionBinding(
            new UIAction(MdiContainerFormUIActionPrefix + nameof(EditPreferencesFile)),
            new UIActionBinding
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.EditPreferencesFile,
                MenuIcon = Properties.Resources.settings,
            });

        private Form CreateSettingsForm(bool isReadOnly,
                                        SettingsFile settingsFile,
                                        SettingProperty<PersistableFormState> formStateSetting,
                                        SettingProperty<int> errorHeightSetting)
        {
            var settingsTextBox = new SettingsTextBox(settingsFile)
            {
                Dock = DockStyle.Fill,
                ReadOnly = isReadOnly,
            };

            settingsTextBox.BindActions(new UIActionBindings
            {
                { SettingsTextBox.SaveToFile, settingsTextBox.TrySaveToFile },
            });

            settingsTextBox.BindStandardEditUIActions();

            UIMenu.AddTo(settingsTextBox);

            var settingsForm = new UIActionForm
            {
                Owner = this,
                ClientSize = new Size(600, 600),
                ShowIcon = false,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.CenterScreen,
                Text = Path.GetFileName(settingsFile.AbsoluteFilePath),
                MinimumSize = new Size(144, SystemInformation.CaptionHeight * 2),
            };

            // If there is no errorsTextBox, splitter will remain null,
            // and no splitter distance needs to be restored or auto-saved.
            SplitContainer splitter = null;
            RichTextBoxEx errorsTextBox = null;

            if (settingsTextBox.ReadOnly && settingsTextBox.CurrentErrorCount == 0)
            {
                // No errors while read-only: do not display an errors textbox.
                settingsForm.Controls.Add(settingsTextBox);
            }
            else
            {
                errorsTextBox = new RichTextBoxEx
                {
                    Dock = DockStyle.Fill,
                    BorderStyle = BorderStyle.None,
                    ScrollBars = RichTextBoxScrollBars.Vertical,
                    HideSelection = false,
                    ReadOnly = true,
                };

                errorsTextBox.BindStandardEditUIActions();

                UIMenu.AddTo(errorsTextBox);

                InitializeErrorInteraction(settingsTextBox, errorsTextBox);

                splitter = new SplitContainer
                {
                    Dock = DockStyle.Fill,
                    SplitterWidth = 4,
                    FixedPanel = FixedPanel.Panel2,
                    Orientation = Orientation.Horizontal,
                };

                splitter.Panel1.Controls.Add(settingsTextBox);
                splitter.Panel2.Controls.Add(errorsTextBox);

                settingsForm.Controls.Add(splitter);
            }

            // Default value shows about 2 errors at default zoom level.
            const int defaultErrorHeight = 34;

            settingsForm.Load += (_, __) =>
            {
                Program.AttachFormStateAutoSaver(settingsForm, formStateSetting, null);

                if (splitter != null && errorsTextBox != null)
                {
                    if (!Program.TryGetAutoSaveValue(errorHeightSetting, out int targetErrorHeight))
                    {
                        targetErrorHeight = defaultErrorHeight;
                    }

                    // Calculate target splitter distance which will restore the target error height exactly.
                    int splitterDistance = settingsForm.ClientSize.Height - targetErrorHeight - splitter.SplitterWidth;
                    if (splitterDistance >= 0) splitter.SplitterDistance = splitterDistance;

                    splitter.SplitterMoved += (___, ____) => Program.AutoSave.Persist(errorHeightSetting, errorsTextBox.Height);
                }
            };

            return settingsForm;
        }

        private static readonly Font noErrorsFont = new Font("Calibri", 10, FontStyle.Italic);
        private static readonly Font errorsFont = new Font("Calibri", 10);

        /// <summary>
        /// Sets up error interaction between a syntax editor with an errors UpdatableRichTextBox.
        /// </summary>
        private void InitializeErrorInteraction(SettingsTextBox settingsTextBox, UpdatableRichTextBox errorsTextBox)
        {
            // Copy background color.
            errorsTextBox.BackColor = settingsTextBox.NoStyleBackColor;

            // Interaction between settingsTextBox and errorsTextBox.
            settingsTextBox.CurrentErrorsChanged += (_, __) => DisplayErrors(settingsTextBox, errorsTextBox);

            // Do an initial DisplayErrors() as well, because settingsTextBox might already contain errors.
            DisplayErrors(settingsTextBox, errorsTextBox);

            errorsTextBox.DoubleClick += (_, __) =>
            {
                int charIndex = errorsTextBox.GetCharIndexFromPosition(errorsTextBox.PointToClient(MousePosition));
                int lineIndex = errorsTextBox.GetLineFromCharIndex(charIndex);
                settingsTextBox.BringErrorIntoView(lineIndex);
            };

            errorsTextBox.KeyDown += (_, e) =>
            {
                if (e.KeyData == Keys.Enter)
                {
                    int charIndex = errorsTextBox.SelectionStart;
                    int lineIndex = errorsTextBox.GetLineFromCharIndex(charIndex);
                    settingsTextBox.BringErrorIntoView(lineIndex);
                }
            };
        }

        private void DisplayErrors(SettingsTextBox settingsTextBox, UpdatableRichTextBox errorsTextBox)
        {
            if (settingsTextBox.CurrentErrorCount == 0)
            {
                using (var updateToken = errorsTextBox.BeginUpdate())
                {
                    errorsTextBox.Text = "(No errors)";
                    errorsTextBox.ForeColor = settingsTextBox.LineNumberForeColor;
                    errorsTextBox.Font = noErrorsFont;
                }
            }
            else
            {
                var errors = settingsTextBox.CurrentErrors;

                using (var updateToken = errorsTextBox.BeginUpdate())
                {
                    var errorMessages = from error in errors
                                        let lineIndex = settingsTextBox.LineFromPosition(error.Start)
                                        let position = settingsTextBox.GetColumn(error.Start)
                                        select $"{error.Message} at line {lineIndex + 1}, position {position + 1}";

                    errorsTextBox.Text = string.Join("\n", errorMessages);

                    errorsTextBox.ForeColor = settingsTextBox.NoStyleForeColor;
                    errorsTextBox.Font = errorsFont;
                }
            }
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
                            if (defaultSettingsObject.Schema.TryGetProperty(property.Name, out SettingProperty defaultSettingProperty)
                                && defaultSettingsObject.TryGetRawValue(defaultSettingProperty, out PValue sourceValue))
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
            new UIActionBinding
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
            new UIActionBinding
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.NewGame,
                Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.N), },
            });

        public UIActionState TryOpenNewPlayingBoard(bool perform)
        {
            if (perform) NewPlayingBoard();
            return UIActionVisibility.Enabled;
        }

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
                Owner = this,
                ClientSize = new Size(width, height),
                ShowIcon = false,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.CenterScreen,
                Text = Path.GetFileName(fileName),
                MinimumSize = new Size(144, SystemInformation.CaptionHeight * 2),
            };

            readOnlyTextForm.Controls.Add(textBox);

            return readOnlyTextForm;
        }

        public static readonly DefaultUIActionBinding OpenAbout = new DefaultUIActionBinding(
            new UIAction(MdiContainerFormUIActionPrefix + nameof(OpenAbout)),
            new UIActionBinding
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.About,
            });

        public UIActionState TryOpenAbout(bool perform)
        {
            // Assume file exists, is distributed with executable.
            // File.Exists() is too expensive to call hundreds of times.
            if (perform)
            {
                if (openAboutForm == null)
                {
                    openAboutForm = CreateReadOnlyTextForm("README.txt", 600, 300);

                    if (openAboutForm != null)
                    {
                        openAboutForm.FormClosed += (_, __) => openAboutForm = null;
                    }
                }

                if (openAboutForm != null && !openAboutForm.ContainsFocus)
                {
                    openAboutForm.Visible = true;
                    openAboutForm.Activate();
                }
            }

            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding ShowCredits = new DefaultUIActionBinding(
            new UIAction(MdiContainerFormUIActionPrefix + nameof(ShowCredits)),
            new UIActionBinding
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.Credits,
            });

        public UIActionState TryShowCredits(bool perform)
        {
            // Assume file exists, is distributed with executable.
            // File.Exists() is too expensive to call hundreds of times.
            if (perform)
            {
                if (openCreditsForm == null)
                {
                    openCreditsForm = CreateReadOnlyTextForm("Credits.txt", 700, 600);

                    if (openCreditsForm != null)
                    {
                        openCreditsForm.FormClosed += (_, __) => openCreditsForm = null;
                    }
                }

                if (openCreditsForm != null && !openCreditsForm.ContainsFocus)
                {
                    openCreditsForm.Visible = true;
                    openCreditsForm.Activate();
                }
            }

            return UIActionVisibility.Enabled;
        }
    }
}

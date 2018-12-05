#region License
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
            => new SettingsForm(isReadOnly, settingsFile, formStateSetting, errorHeightSetting)
            {
                Owner = this,
                ClientSize = new Size(600, 600),
                ShowIcon = false,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.CenterScreen,
                MinimumSize = new Size(144, SystemInformation.CaptionHeight * 2),
            };

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

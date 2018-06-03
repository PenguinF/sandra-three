﻿/*********************************************************************************
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

        public UIActionState TryEditPreferencesFile(bool perform)
        {
            if (perform)
            {
                // Before opening the possibly non-existent file, write to it.
                // This pretty-prints it too.
                Exception exception = Program.LocalSettings.WriteToFile();
                if (exception != null)
                {
                    MessageBox.Show(exception.Message);
                }
                else
                {
                    // Immediately dispose handle after creation.
                    var process = System.Diagnostics.Process.Start(
                        "notepad.exe",
                        Program.LocalSettings.AbsoluteFilePath);
                    process.Dispose();
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
                    var settingsTextBox = new SettingsTextBox()
                    {
                        Dock = DockStyle.Fill,
                        BorderStyle = BorderStyle.None,
                        BackColor = Color.White,
                        ForeColor = Color.Black,
                        Font = new Font("Consolas", 10),
                        Text = File.ReadAllText(Program.DefaultSettings.AbsoluteFilePath),
                        ReadOnly = true,
                    };

                    settingsTextBox.BindActions(new UIActionBindings
                    {
                        { RichTextBoxBase.CopySelectionToClipBoard, settingsTextBox.TryCopySelectionToClipBoard },
                        { RichTextBoxBase.SelectAllText, settingsTextBox.TrySelectAllText },
                    });

                    UIMenu.AddTo(settingsTextBox);

                    openDefaultSettingsForm = new Form()
                    {
                        Owner = this,
                        ClientSize = new Size(600, 600),
                        ShowIcon = false,
                        ShowInTaskbar = false,
                        StartPosition = FormStartPosition.CenterScreen,
                        Text = Path.GetFileName(Program.DefaultSettings.AbsoluteFilePath),
                    };

                    openDefaultSettingsForm.Controls.Add(settingsTextBox);
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

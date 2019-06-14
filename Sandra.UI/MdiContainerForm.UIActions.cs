#region License
/*********************************************************************************
 * MdiContainerForm.UIActions.cs
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

using Eutherion.UIActions;
using Eutherion.Utils;
using Eutherion.Win;
using Eutherion.Win.AppTemplate;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sandra.UI
{
    /// <summary>
    /// Main MdiContainer Form.
    /// </summary>
    public partial class MdiContainerForm
    {
        public const string MdiContainerFormUIActionPrefix = nameof(MdiContainerForm) + ".";

        public static readonly DefaultUIActionBinding NewPGNFile = new DefaultUIActionBinding(
            new UIAction(MdiContainerFormUIActionPrefix + nameof(NewPGNFile)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control | KeyModifiers.Shift, ConsoleKey.N), },
                    IsFirstInGroup = true,
                    MenuTextProvider = LocalizedStringKeys.NewGameFile.ToTextProvider(),
                },
            });

        public UIActionState TryNewPGNFile(bool perform)
        {
            if (perform)
            {
                var syntaxDescriptor = new PGNSyntaxDescriptor();

                var pgnForm = new SyntaxEditorForm<PGNSymbol, PGNErrorInfo>(
                    SyntaxEditorCodeAccessOption.Default,
                    syntaxDescriptor,
                    WorkingCopyTextFile.Open(null, null),
                    null,
                    SettingKeys.PGNWindow,
                    SettingKeys.PGNErrorHeight,
                    SettingKeys.PGNZoom)
                {
                    MinimumSize = new Size(144, SystemInformation.CaptionHeight * 2),
                    ClientSize = new Size(400, 400),
                    ShowInTaskbar = true,
                    Icon = Session.Current.ApplicationIcon,
                    ShowIcon = true,
                };

                pgnForm.EnsureActivated();
            }

            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding OpenPGNFile = new DefaultUIActionBinding(
            new UIAction(MdiContainerFormUIActionPrefix + nameof(OpenPGNFile)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.O), },
                    MenuTextProvider = LocalizedStringKeys.OpenGameFile.ToTextProvider(),
                    OpensDialog = true,
                },
            });

        public UIActionState TryOpenPGNFile(bool perform)
        {
            if (perform)
            {
                var syntaxDescriptor = new PGNSyntaxDescriptor();

                string extension = syntaxDescriptor.FileExtension;
                var extensionLocalizedKey = syntaxDescriptor.FileExtensionLocalizedKey;

                var openFileDialog = new OpenFileDialog
                {
                    AutoUpgradeEnabled = true,
                    DereferenceLinks = true,
                    DefaultExt = extension,
                    Filter = $"{Session.Current.CurrentLocalizer.Localize(extensionLocalizedKey)} (*.{extension})|*.{extension}|{Session.Current.CurrentLocalizer.Localize(SharedLocalizedStringKeys.AllFiles)} (*.*)|*.*",
                    SupportMultiDottedExtensions = true,
                    RestoreDirectory = true,
                    Title = Session.Current.CurrentLocalizer.Localize(LocalizedStringKeys.OpenGameFile),
                    ValidateNames = true,
                    CheckFileExists = false,
                    ShowReadOnly = true,
                };

                var dialogResult = openFileDialog.ShowDialog(this);
                if (dialogResult == DialogResult.OK)
                {
                    var pgnForm = new SyntaxEditorForm<PGNSymbol, PGNErrorInfo>(
                        openFileDialog.ReadOnlyChecked ? SyntaxEditorCodeAccessOption.ReadOnly : SyntaxEditorCodeAccessOption.Default,
                        syntaxDescriptor,
                        WorkingCopyTextFile.Open(openFileDialog.FileName, null),
                        null,
                        SettingKeys.PGNWindow,
                        SettingKeys.PGNErrorHeight,
                        SettingKeys.PGNZoom)
                    {
                        MinimumSize = new Size(144, SystemInformation.CaptionHeight * 2),
                        ClientSize = new Size(400, 400),
                        ShowInTaskbar = true,
                        Icon = Session.Current.ApplicationIcon,
                        ShowIcon = true,
                    };

                    pgnForm.EnsureActivated();
                }
            }

            return UIActionVisibility.Enabled;
        }

        public UIActionState TryExit(bool perform)
        {
            if (perform) Close();
            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding OpenNewPlayingBoard = new DefaultUIActionBinding(
            new UIAction(MdiContainerFormUIActionPrefix + nameof(OpenNewPlayingBoard)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.N), },
                    MenuTextProvider = LocalizedStringKeys.NewGame.ToTextProvider(),
                },
            });

        public UIActionState TryOpenNewPlayingBoard(bool perform)
        {
            if (perform) NewPlayingBoard();
            return UIActionVisibility.Enabled;
        }
    }
}

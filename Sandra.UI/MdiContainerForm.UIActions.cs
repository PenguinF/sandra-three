#region License
/*********************************************************************************
 * MdiContainerForm.UIActions.cs
 *
 * Copyright (c) 2004-2023 Henk Nicolai
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
using Eutherion.Collections;
using Eutherion.UIActions;
using Eutherion.Win.MdiAppTemplate;
using Sandra.Chess.Pgn;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Sandra.UI
{
    using PgnEditor = SyntaxEditor<RootPgnSyntax, IPgnSymbol, PgnErrorInfo>;

    /// <summary>
    /// Main MdiContainer Form.
    /// </summary>
    public partial class MdiContainerForm
    {
        public const string MdiContainerFormUIActionPrefix = nameof(MdiContainerForm) + ".";

        public static readonly UIAction OpenNewPlayingBoard = new UIAction(
            new StringKey<UIAction>(MdiContainerFormUIActionPrefix + nameof(OpenNewPlayingBoard)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    IsFirstInGroup = true,
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.N), },
                    MenuTextProvider = LocalizedStringKeys.NewGame.ToTextProvider(),
                },
            });

        public UIActionState TryOpenNewPlayingBoard(PgnEditor pgnEditor, bool perform)
        {
            // Disable until we can modify PGN using its syntax tree.
            return UIActionVisibility.Disabled;
        }

        public static readonly UIAction OpenGame = new UIAction(
            new StringKey<UIAction>(MdiContainerFormUIActionPrefix + nameof(OpenGame)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.G), },
                    IsFirstInGroup = true,
                    MenuTextProvider = LocalizedStringKeys.OpenGame.ToTextProvider(),
                },
            });

        // First implementation: maintain a dictionary of open games.
        // This needs work; when the PGN syntax is updated all PgnGameSyntax instances are recreated.
        private readonly Dictionary<PgnGameSyntax, StandardChessBoard> OpenGames = new Dictionary<PgnGameSyntax, StandardChessBoard>();

        public UIActionState TryOpenGame(PgnEditor pgnEditor, bool perform)
        {
            var (gameSyntax, deepestPly) = pgnEditor.GameAtOrBeforePosition(pgnEditor.SelectionStart);
            if (gameSyntax == null) return UIActionVisibility.Disabled;

            if (perform)
            {
                StandardChessBoard chessBoard = OpenGames.GetOrAdd(gameSyntax, key =>
                {
                    StandardChessBoard newChessBoard = OpenChessBoard(pgnEditor, new Chess.Game(gameSyntax));
                    newChessBoard.Disposed += (_, __) => OpenGames.Remove(gameSyntax);
                    return newChessBoard;
                });

                chessBoard.Game.ActivePly = deepestPly;
                chessBoard.GameUpdated();
                chessBoard.EnsureActivated();
            }

            return UIActionVisibility.Enabled;
        }

        public static readonly UIAction NewPgnFile = new UIAction(
            new StringKey<UIAction>(MdiContainerFormUIActionPrefix + nameof(NewPgnFile)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control | KeyModifiers.Shift, ConsoleKey.N), },
                    IsFirstInGroup = true,
                    MenuTextProvider = LocalizedStringKeys.NewGameFile.ToTextProvider(),
                },
            });

        public UIActionState TryNewPgnFile(bool perform)
        {
            if (perform) OpenNewPgnEditor();
            return UIActionVisibility.Enabled;
        }

        public static readonly UIAction OpenPgnFile = new UIAction(
            new StringKey<UIAction>(MdiContainerFormUIActionPrefix + nameof(OpenPgnFile)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.O), },
                    MenuTextProvider = LocalizedStringKeys.OpenGameFile.ToTextProvider(),
                    OpensDialog = true,
                },
            });

        public UIActionState TryOpenPgnFile(bool perform)
        {
            if (perform)
            {
                string extension = PgnSyntaxDescriptor.Instance.FileExtension;
                var extensionLocalizedKey = PgnSyntaxDescriptor.Instance.FileExtensionLocalizedKey;

                var openFileDialog = new OpenFileDialog
                {
                    AutoUpgradeEnabled = true,
                    DereferenceLinks = true,
                    DefaultExt = extension,
                    Filter = $"{Session.Current.CurrentLocalizer.Format(extensionLocalizedKey)} (*.{extension})|*.{extension}|{Session.Current.CurrentLocalizer.Format(SharedLocalizedStringKeys.AllFiles)} (*.*)|*.*",
                    SupportMultiDottedExtensions = true,
                    RestoreDirectory = true,
                    Title = Session.Current.CurrentLocalizer.Format(LocalizedStringKeys.OpenGameFile),
                    ValidateNames = true,
                    CheckFileExists = false,
                    ShowReadOnly = true,
                    Multiselect = true,
                };

                var dialogResult = openFileDialog.ShowDialog(this);
                if (dialogResult == DialogResult.OK)
                {
                    OpenCommandLineArgs(openFileDialog.FileNames, isReadOnly: openFileDialog.ReadOnlyChecked);
                }
            }

            return UIActionVisibility.Enabled;
        }
    }
}

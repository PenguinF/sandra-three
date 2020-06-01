﻿#region License
/*********************************************************************************
 * MdiContainerForm.UIActions.cs
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

using Eutherion;
using Eutherion.UIActions;
using Eutherion.Utils;
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
                    IsFirstInGroup = true,
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.N), },
                    MenuTextProvider = LocalizedStringKeys.NewGame.ToTextProvider(),
                },
            });

        public UIActionState TryOpenNewPlayingBoard(PgnEditor pgnEditor, bool perform)
        {
            if (perform) OpenChessBoard(pgnEditor, new Chess.Game()).EnsureActivated();
            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding OpenGame = new DefaultUIActionBinding(
            new UIAction(MdiContainerFormUIActionPrefix + nameof(OpenGame)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.B), },
                    IsFirstInGroup = true,
                    MenuTextProvider = LocalizedStringKeys.OpenGame.ToTextProvider(),
                },
            });

        // First implementation: maintain a dictionary of open games.
        // This needs work; when the PGN syntax is updated all PgnGameSyntax instances are recreated.
        private readonly Dictionary<PgnGameSyntax, StandardChessBoard> OpenGames = new Dictionary<PgnGameSyntax, StandardChessBoard>();

        public UIActionState TryOpenGame(PgnEditor pgnEditor, bool perform)
        {
            PgnGameSyntax gameSyntax = pgnEditor.GameAtOrBeforePosition(pgnEditor.SelectionStart);
            if (gameSyntax == null) return UIActionVisibility.Disabled;

            if (perform)
            {
                StandardChessBoard chessBoard = OpenGames.GetOrAdd(gameSyntax, key =>
                {
                    StandardChessBoard newChessBoard = OpenChessBoard(pgnEditor, gameSyntax.CreateGame());
                    newChessBoard.Disposed += (_, __) => OpenGames.Remove(gameSyntax);
                    return newChessBoard;
                });

                chessBoard.EnsureActivated();
            }

            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding NewPgnFile = new DefaultUIActionBinding(
            new UIAction(MdiContainerFormUIActionPrefix + nameof(NewPgnFile)),
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

        public static readonly DefaultUIActionBinding OpenPgnFile = new DefaultUIActionBinding(
            new UIAction(MdiContainerFormUIActionPrefix + nameof(OpenPgnFile)),
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
                    Filter = $"{Session.Current.CurrentLocalizer.Localize(extensionLocalizedKey)} (*.{extension})|*.{extension}|{Session.Current.CurrentLocalizer.Localize(SharedLocalizedStringKeys.AllFiles)} (*.*)|*.*",
                    SupportMultiDottedExtensions = true,
                    RestoreDirectory = true,
                    Title = Session.Current.CurrentLocalizer.Localize(LocalizedStringKeys.OpenGameFile),
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

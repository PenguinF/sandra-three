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
using System.Linq;
using System.Text;
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

        private int LengthExcludingTrailingNewlineCharacters(ScintillaNET.Line line)
        {
            string lineText = line.Text;
            int lineLength = line.Length;
            while (lineLength > 0 && (lineText[lineLength - 1] == '\n' || lineText[lineLength - 1] == '\r')) lineLength--;
            return lineLength;
        }

        private string NewLine(ScintillaNET.Eol eol)
        {
            switch (eol)
            {
                default:
                case ScintillaNET.Eol.CrLf:
                    return "\r\n";
                case ScintillaNET.Eol.Cr:
                    return "\r";
                case ScintillaNET.Eol.Lf:
                    return "\n";
            }
        }

        public UIActionState TryOpenNewPlayingBoard(PgnEditor pgnEditor, bool perform)
        {
            if (perform)
            {
                // Determine position in the source PGN where to insert a new game.
                // When in the middle of a game, jump to the end of it.
                int insertPosition = pgnEditor.SelectionStart;
                var (gameSyntax, _) = pgnEditor.GameAtOrBeforePosition(insertPosition);
                if (gameSyntax != null) insertPosition = Math.Max(insertPosition, gameSyntax.AbsoluteStart + gameSyntax.Length);

                StringBuilder insertString = new StringBuilder();
                int insertLineIndex = pgnEditor.LineFromPosition(insertPosition);
                ScintillaNET.Line line = pgnEditor.Lines[insertLineIndex];

                // If at the end of a line and there's another line following it, jump to the start position of that line.
                int lineLength = line.Position + LengthExcludingTrailingNewlineCharacters(line);
                if (lineLength > 0
                    && insertPosition == line.Position + lineLength
                    && insertLineIndex + 1 < pgnEditor.Lines.Count)
                {
                    insertLineIndex++;
                    line = pgnEditor.Lines[insertLineIndex];
                    insertPosition = line.Position;
                }

                // If in the middle of a line, add newlines before and/or after it.
                // Respect the editor's current EolMode setting.
                string newLine = NewLine(pgnEditor.EolMode);
                if (line.Position < insertPosition) insertString.Append(newLine);
                Chess.Game.WellKnownTagNames.ForEach(x => { insertString.AppendFormat("[{0} \"\"]", x); insertString.Append(newLine); });
                if (insertPosition < line.Position + LengthExcludingTrailingNewlineCharacters(line)) insertString.Append(newLine);

                string insertStringResult = insertString.ToString();
                pgnEditor.InsertText(insertPosition, insertStringResult);
                pgnEditor.SelectionStart = insertPosition + insertStringResult.Length;

                // Parsing and selection start updates will only happen once Scintilla receives the appropriate windows messages.
                // So in order to open the game that's just been inserted, force those messages with an Update() call.
                pgnEditor.Update();
                TryOpenGame(pgnEditor, perform);
            }

            return UIActionVisibility.Enabled;
        }

        public static readonly UIAction OpenGame = new UIAction(
            new StringKey<UIAction>(MdiContainerFormUIActionPrefix + nameof(OpenGame)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.G), },
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

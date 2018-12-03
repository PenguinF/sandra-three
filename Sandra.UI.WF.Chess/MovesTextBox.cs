﻿#region License
/*********************************************************************************
 * MovesTextBox.cs
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

using Sandra.PGN;
using SysExtensions;
using SysExtensions.TextIndex;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents a read-only Windows rich text box which displays a list of chess moves.
    /// </summary>
    public partial class MovesTextBox : SyntaxEditor<PGNTerminalSymbol>
    {
        private readonly TextElementStyle defaultStyle = new TextElementStyle
        {
            HasBackColor = true,
            BackColor = Color.White,
            HasForeColor = true,
            ForeColor = Color.Black,
            Font = new Font("Candara", 10),
        };

        protected override TextElementStyle DefaultStyle => defaultStyle;

        private readonly TextElementStyle activeMoveStyle = new TextElementStyle
        {
            HasForeColor = true,
            ForeColor = Color.DarkRed,
            Font = new Font("Candara", 10, FontStyle.Bold),
        };

        public ObservableValue<int> CaretPosition { get; } = new ObservableValue<int>();

        public MovesTextBox()
        {
            BorderStyle = BorderStyle.None;
            ReadOnly = true;

            ApplyDefaultStyle();

            CaretPosition.ValueChanged += BringIntoView;
            CaretPosition.ValueChanged += caretPositionChanged;

            // DisplayTextChanged handlers are called immediately upon registration.
            // This initializes moveFormatter.
            localizedPieceSymbols.DisplayText.ValueChanged += _ =>
            {
                if (moveFormatter == null || moveFormattingOption != MoveFormattingOption.UsePGN)
                {
                    updateMoveFormatter();
                }
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                localizedPieceSymbols.Dispose();
                defaultStyle.Font.Dispose();
                activeMoveStyle.Font.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Standardized PGN notation for pieces.
        /// </summary>
        private const string PGNPieceSymbols = "NBRQK";

        private readonly LocalizedString localizedPieceSymbols = new LocalizedString(LocalizedStringKeys.PieceSymbols);

        private enum MoveFormattingOption
        {
            UseLocalizedShortAlgebraic,
            UsePGN,
            UseLocalizedLongAlgebraic,
        }

        /// <summary>
        /// Contains shorthand values for <see cref="MoveFormattingOption"/> which are stored in settings files as a string.
        /// </summary>
        internal enum MFOSettingValue
        {
            san, pgn, lan
        }

        private MoveFormattingOption moveFormattingOption;

        private Chess.IMoveFormatter moveFormatter;

        private void updateMoveFormatter()
        {
            if (moveFormatter == null)
            {
                // Initialize moveFormattingOption from settings.
                if (Program.TryGetAutoSaveValue(SettingKeys.Notation, out MFOSettingValue mfoSettingValue))
                {
                    moveFormattingOption = (MoveFormattingOption)mfoSettingValue;
                }
            }
            else
            {
                // Update setting if the formatter was already initialized.
                Program.AutoSave.Persist(SettingKeys.Notation, (MFOSettingValue)moveFormattingOption);
            }

            string pieceSymbols;
            if (moveFormattingOption == MoveFormattingOption.UsePGN)
            {
                pieceSymbols = PGNPieceSymbols;
            }
            else
            {
                pieceSymbols = localizedPieceSymbols.DisplayText.Value;
                if (pieceSymbols.Length != 5 && pieceSymbols.Length != 6)
                {
                    // Revert back to PGN.
                    pieceSymbols = PGNPieceSymbols;
                }
            }

            EnumIndexedArray<Chess.Piece, string> pgnPieceSymbolArray = EnumIndexedArray<Chess.Piece, string>.New();

            int pieceIndex = 0;
            if (pieceSymbols.Length == 6)
            {
                // Support for an optional pawn piece symbol.
                pgnPieceSymbolArray[Chess.Piece.Pawn] = pieceSymbols[pieceIndex++].ToString();
            }

            pgnPieceSymbolArray[Chess.Piece.Knight] = pieceSymbols[pieceIndex++].ToString();
            pgnPieceSymbolArray[Chess.Piece.Bishop] = pieceSymbols[pieceIndex++].ToString();
            pgnPieceSymbolArray[Chess.Piece.Rook] = pieceSymbols[pieceIndex++].ToString();
            pgnPieceSymbolArray[Chess.Piece.Queen] = pieceSymbols[pieceIndex++].ToString();
            pgnPieceSymbolArray[Chess.Piece.King] = pieceSymbols[pieceIndex++].ToString();

            if (moveFormattingOption == MoveFormattingOption.UseLocalizedLongAlgebraic)
            {
                moveFormatter = new Chess.LongAlgebraicMoveFormatter(pgnPieceSymbolArray);
            }
            else
            {
                moveFormatter = new Chess.ShortAlgebraicMoveFormatter(pgnPieceSymbolArray);
            }

            refreshText();
        }

        private InteractiveGame game;

        /// <summary>
        /// Gets or sets the chess game which contains the moves to be formatted.
        /// </summary>
        public InteractiveGame Game
        {
            get => game;
            set
            {
                if (game != value)
                {
                    game = value;
                    refreshText();
                }
            }
        }

        internal void GameUpdated()
        {
            if (!IsUpdating)
            {
                updateText();
            }
        }

        private PGNLine generatePGNLine(Chess.Game game, List<PGNPlyWithSidelines> moveList)
        {
            for (; ; )
            {
                // Remember the game's active tree because it's the starting point of side lines.
                Chess.MoveTree current = game.ActiveTree;
                int plyCount = current.PlyCount;

                List<PGNLine> sideLines = null;
                foreach (var sideLine in current.SideLines)
                {
                    if (sideLines == null) sideLines = new List<PGNLine>();

                    // Add the first ply of the side line to the list, then generate the rest of the side line.
                    sideLines.Add(generatePGNLine(game, new List<PGNPlyWithSidelines>
                    {
                        new PGNPlyWithSidelines(new PGNPly(plyCount, moveFormatter.FormatMove(game, sideLine.Move), sideLine), null)
                    }));

                    // Reset active tree after going into each side line.
                    game.SetActiveTree(current);
                }

                PGNPly ply = null;
                if (current.MainLine != null)
                {
                    ply = new PGNPly(plyCount, moveFormatter.FormatMove(game, current.MainLine.Move), current.MainLine);
                }

                if (ply != null || sideLines != null)
                {
                    moveList.Add(new PGNPlyWithSidelines(ply, sideLines));
                }

                if (current.MainLine == null)
                {
                    return new PGNLine(moveList);
                }
            }
        }

        private IEnumerable<PGNTerminalSymbol> generatePGNTerminalSymbols()
        {
            if (game != null)
            {
                // Copy the game to be able to format moves correctly without affecting game.Game.ActiveTree.
                Chess.Game copiedGame = game.Game.Copy();

                return generatePGNLine(copiedGame, new List<PGNPlyWithSidelines>()).GenerateTerminalSymbols();
            }

            return Enumerable.Empty<PGNTerminalSymbol>();
        }

        private TextElement<PGNTerminalSymbol> currentActiveMoveStyleElement;

        private void refreshText()
        {
            if (game != null)
            {
                // Clear and build the entire text anew by clearing the old element list.
                using (var updateToken = BeginUpdate())
                {
                    ReadOnly = false;
                    Clear();
                    ReadOnly = true;
                    TextIndex.Clear();
                    updateText();
                }
            }
        }

        private void updateText()
        {
            List<PGNTerminalSymbol> updatedTerminalSymbols = generatePGNTerminalSymbols().ToList();

            int existingElementCount = TextIndex.Elements.Count;
            int updatedElementCount = updatedTerminalSymbols.Count;

            PGNMoveSearcher activeTreeSearcher = new PGNMoveSearcher(game.Game.ActiveTree);
            TextElement<PGNTerminalSymbol> newActiveMoveElement = null;

            // Instead of clearing and updating the entire textbox, compare the elements one by one.
            int minLength = Math.Min(existingElementCount, updatedElementCount);
            int agreeIndex = 0;
            while (agreeIndex < minLength)
            {
                var existingElement = TextIndex.Elements[agreeIndex];
                var updatedTerminalSymbol = updatedTerminalSymbols[agreeIndex];
                if (updatedTerminalSymbol.Equals(existingElement.TerminalSymbol))
                {
                    ++agreeIndex;
                    if (newActiveMoveElement == null && activeTreeSearcher.Visit(updatedTerminalSymbol))
                    {
                        newActiveMoveElement = existingElement;
                    }
                }
                else
                {
                    // agreeIndex is now at the first index where both element lists disagree.
                    break;
                }
            }

            using (var updateToken = BeginUpdate())
            {
                // Reset any markup.
                ApplyDefaultStyle();
                currentActiveMoveStyleElement = null;

                if (agreeIndex < existingElementCount)
                {
                    // Clear existing tail part.
                    int textStart = TextIndex.Elements[agreeIndex].Start;

                    ReadOnly = false;
                    RemoveText(textStart, TextIndex.Size - textStart);
                    ReadOnly = true;

                    TextIndex.RemoveFrom(agreeIndex);
                }

                // Append new element texts.
                PGNTerminalSymbolTextGenerator textGenerator = new PGNTerminalSymbolTextGenerator();
                while (agreeIndex < updatedElementCount)
                {
                    var updatedTerminalSymbol = updatedTerminalSymbols[agreeIndex];
                    var text = textGenerator.Visit(updatedTerminalSymbol);

                    ReadOnly = false;
                    AppendText(text);
                    ReadOnly = true;

                    var newElement = TextIndex.AppendTerminalSymbol(updatedTerminalSymbol, text.Length);

                    if (newActiveMoveElement == null && activeTreeSearcher.Visit(updatedTerminalSymbol))
                    {
                        newActiveMoveElement = newElement;
                    }

                    ++agreeIndex;
                }

                if (game == null || game.Game.IsFirstMove)
                {
                    // If there's no active move, go to before the first move.
                    if (TextIndex.Elements.Count > 0)
                    {
                        CaretPosition.Value = TextIndex.Elements[0].Start;
                    }
                }
                else if (newActiveMoveElement != null)
                {
                    // Make the active move bold.
                    currentActiveMoveStyleElement = newActiveMoveElement;
                    ApplyStyle(newActiveMoveElement, activeMoveStyle);

                    // Also update the caret so the active move is in view.
                    CaretPosition.Value = newActiveMoveElement.End;
                }
            }
        }

        private void caretPositionChanged(int selectionStart)
        {
            if (game != null)
            {
                TextElement<PGNTerminalSymbol> elementBefore = TextIndex.GetElementBefore(selectionStart);
                TextElement<PGNTerminalSymbol> elementAfter = TextIndex.GetElementAfter(selectionStart);

                TextElement<PGNTerminalSymbol> activeElement = elementBefore;
                PGNPly pgnPly;

                if (activeElement == null)
                {
                    // Exceptional case to go to the initial position.
                    pgnPly = null;
                }
                else
                {
                    // Prefer to attach to a non-space element.
                    // Use assumption that the terminal symbol list nowhere contains two adjacent SpaceSymbols.
                    // Also use assumption that the terminal symbol list neither starts nor ends with a SpaceSymbol.
                    if (activeElement.TerminalSymbol is SpaceSymbol && elementAfter != null)
                    {
                        activeElement = elementAfter;
                    }

                    pgnPly = new PGNActivePlyDetector().Visit(activeElement.TerminalSymbol);
                }

                Chess.MoveTree newActiveTree = pgnPly == null ? game.Game.MoveTree : pgnPly.Variation.MoveTree;

                // Update the active move index in the game.
                if (game.Game.ActiveTree != newActiveTree)
                {
                    using (var updateToken = BeginUpdateRememberState())
                    {
                        // Reset markup of the previously active move element.
                        if (currentActiveMoveStyleElement != null)
                        {
                            ApplyStyle(currentActiveMoveStyleElement, defaultStyle);
                            currentActiveMoveStyleElement = null;
                        }

                        game.Game.SetActiveTree(newActiveTree);
                        game.ActiveMoveTreeUpdated();
                        ActionHandler.Invalidate();

                        // Search for the current active move element to set its font.
                        PGNMoveSearcher newActiveTreeSearcher = new PGNMoveSearcher(game.Game.ActiveTree);
                        foreach (TextElement<PGNTerminalSymbol> textElement in TextIndex.Elements)
                        {
                            if (newActiveTreeSearcher.Visit(textElement.TerminalSymbol))
                            {
                                currentActiveMoveStyleElement = textElement;
                                ApplyStyle(textElement, activeMoveStyle);
                                break;
                            }
                        }
                    }
                }
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (!ModifierKeys.HasFlag(Keys.Control) && game != null)
            {
                game.HandleMouseWheelEvent(e.Delta);
            }

            base.OnMouseWheel(e);
        }

        protected override void OnSelectionChanged(EventArgs e)
        {
            // Ignore updates as a result of all kinds of calls to Select()/SelectAll().
            // This is only to detect caret updates by interacting with the control.
            // Also check SelectionLength so the event is not raised for non-empty selections.
            if (!IsUpdating && SelectionLength == 0)
            {
                CaretPosition.Value = SelectionStart;
            }

            base.OnSelectionChanged(e);
        }

        private void BringIntoView(int caretPosition)
        {
            if (SelectionStart != caretPosition)
            {
                Select(caretPosition, 0);
                ScrollToCaret();
            }
        }
    }
}

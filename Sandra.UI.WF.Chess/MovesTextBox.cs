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
using Sandra.PGN;
using SysExtensions;
using SysExtensions.SyntaxRenderer;
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
    public partial class MovesTextBox : UpdatableRichTextBox, IUIActionHandlerProvider
    {
        private sealed class TextElementStyle
        {
            public bool HasBackColor { get; set; }
            public Color BackColor { get; set; }

            public bool HasForeColor { get; set; }
            public Color ForeColor { get; set; }

            public bool HasFont => Font != null;
            public Font Font { get; set; }
        }

        private readonly TextElementStyle defaultStyle = new TextElementStyle()
        {
            HasBackColor = true,
            BackColor = Color.White,
            HasForeColor = true,
            ForeColor = Color.Black,
            Font = new Font("Candara", 10),
        };

        private readonly TextElementStyle activeMoveStyle = new TextElementStyle()
        {
            HasForeColor = true,
            ForeColor = Color.DarkRed,
            Font = new Font("Candara", 10, FontStyle.Bold),
        };

        private readonly SyntaxRenderer<PGNTerminalSymbol> syntaxRenderer;

        public MovesTextBox()
        {
            BorderStyle = BorderStyle.None;
            ReadOnly = true;

            syntaxRenderer = SyntaxRenderer<PGNTerminalSymbol>.AttachTo(this);
            syntaxRenderer.CaretPositionChanged += caretPositionChanged;

            applyDefaultStyle();

            int zoomFactor;
            if (Program.AutoSave.CurrentSettings.TryGetValue(SettingKeys.Zoom, out zoomFactor))
            {
                ZoomFactor = PType.RichTextZoomFactor.FromDiscreteZoomFactor(zoomFactor);
            }

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

        private void applyDefaultStyle()
        {
            using (var updateToken = BeginUpdateRememberCaret())
            {
                BackColor = defaultStyle.BackColor;
                ForeColor = defaultStyle.ForeColor;
                Font = defaultStyle.Font;
                SelectAll();
                SelectionBackColor = defaultStyle.BackColor;
                SelectionColor = defaultStyle.ForeColor;
                SelectionFont = defaultStyle.Font;
            }
        }

        private void applyStyle(TextElement<PGNTerminalSymbol> element, TextElementStyle style)
        {
            using (var updateToken = BeginUpdateRememberCaret())
            {
                Select(element.Start, element.Length);
                if (style.HasBackColor) SelectionBackColor = style.BackColor;
                if (style.HasForeColor) SelectionColor = style.ForeColor;
                if (style.HasFont) SelectionFont = style.Font;
            }
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
        /// Gets the action handler for this control.
        /// </summary>
        public UIActionHandler ActionHandler { get; } = new UIActionHandler();

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
        /// Contains shorthand values for <see cref="MoveFormattingOption"/> for storing in settings files.
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
                MFOSettingValue mfoSettingValue;
                if (Program.AutoSave.CurrentSettings.TryGetValue(SettingKeys.Notation, out mfoSettingValue))
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
            get { return game; }
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
            for (;;)
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
                    syntaxRenderer.Clear();
                    updateText();
                }
            }
        }

        private void updateText()
        {
            List<PGNTerminalSymbol> updatedTerminalSymbols = generatePGNTerminalSymbols().ToList();

            int existingElementCount = syntaxRenderer.Elements.Count;
            int updatedElementCount = updatedTerminalSymbols.Count;

            PGNMoveSearcher activeTreeSearcher = new PGNMoveSearcher(game.Game.ActiveTree);
            TextElement<PGNTerminalSymbol> newActiveMoveElement = null;

            // Instead of clearing and updating the entire textbox, compare the elements one by one.
            int minLength = Math.Min(existingElementCount, updatedElementCount);
            int agreeIndex = 0;
            while (agreeIndex < minLength)
            {
                var existingElement = syntaxRenderer.Elements[agreeIndex];
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
                applyDefaultStyle();
                currentActiveMoveStyleElement = null;

                if (agreeIndex < existingElementCount)
                {
                    // Clear existing tail part.
                    syntaxRenderer.RemoveFrom(agreeIndex);
                }

                // Append new element texts.
                PGNTerminalSymbolTextGenerator textGenerator = new PGNTerminalSymbolTextGenerator();
                while (agreeIndex < updatedElementCount)
                {
                    var updatedTerminalSymbol = updatedTerminalSymbols[agreeIndex];
                    var newElement = syntaxRenderer.AppendTerminalSymbol(updatedTerminalSymbol,
                                                                         textGenerator.Visit(updatedTerminalSymbol));

                    if (newActiveMoveElement == null && activeTreeSearcher.Visit(updatedTerminalSymbol))
                    {
                        newActiveMoveElement = newElement;
                    }

                    ++agreeIndex;
                }

                if (game == null || game.Game.IsFirstMove)
                {
                    // If there's no active move, go to before the first move.
                    if (syntaxRenderer.Elements.Count > 0)
                    {
                        syntaxRenderer.Elements[0].BringIntoViewBefore();
                    }
                }
                else if (newActiveMoveElement != null)
                {
                    // Make the active move bold.
                    currentActiveMoveStyleElement = newActiveMoveElement;
                    applyStyle(newActiveMoveElement, activeMoveStyle);

                    // Also update the caret so the active move is in view.
                    newActiveMoveElement.BringIntoViewAfter();
                }
            }
        }

        private void caretPositionChanged(SyntaxRenderer<PGNTerminalSymbol> sender, CaretPositionChangedEventArgs<PGNTerminalSymbol> e)
        {
            if (game != null)
            {
                TextElement<PGNTerminalSymbol> activeElement = e.ElementBefore;
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
                    if (activeElement.TerminalSymbol is SpaceSymbol && e.ElementAfter != null)
                    {
                        activeElement = e.ElementAfter;
                    }

                    pgnPly = new PGNActivePlyDetector().Visit(activeElement.TerminalSymbol);
                }

                Chess.MoveTree newActiveTree = pgnPly == null ? game.Game.MoveTree : pgnPly.Variation.MoveTree;

                // Update the active move index in the game.
                if (game.Game.ActiveTree != newActiveTree)
                {
                    using (var updateToken = BeginUpdateRememberCaret())
                    {
                        // Reset markup of the previously active move element.
                        if (currentActiveMoveStyleElement != null)
                        {
                            applyStyle(currentActiveMoveStyleElement, defaultStyle);
                            currentActiveMoveStyleElement = null;
                        }

                        game.Game.SetActiveTree(newActiveTree);
                        game.ActiveMoveTreeUpdated();
                        ActionHandler.Invalidate();

                        // Search for the current active move element to set its font.
                        PGNMoveSearcher newActiveTreeSearcher = new PGNMoveSearcher(game.Game.ActiveTree);
                        foreach (TextElement<PGNTerminalSymbol> textElement in syntaxRenderer.Elements)
                        {
                            if (newActiveTreeSearcher.Visit(textElement.TerminalSymbol))
                            {
                                currentActiveMoveStyleElement = textElement;
                                applyStyle(textElement, activeMoveStyle);
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

            if (ModifierKeys.HasFlag(Keys.Control))
            {
                // ZoomFactor isn't updated yet, so predict here what it's going to be.
                autoSaveZoomFactor(PType.RichTextZoomFactor.ToDiscreteZoomFactor(ZoomFactor) + Math.Sign(e.Delta));
            }
        }

        private void autoSaveZoomFactor(int zoomFactor)
        {
            Program.AutoSave.Persist(SettingKeys.Zoom, zoomFactor);
        }
    }
}

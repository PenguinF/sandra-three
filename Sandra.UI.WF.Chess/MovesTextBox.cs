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
    public class MovesTextBox : UpdatableRichTextBox, IUIActionHandlerProvider
    {
        private sealed class TextElementStyle
        {
            public bool HasFont => Font != null;
            public Font Font { get; set; }
        }

        private readonly TextElementStyle defaultStyle = new TextElementStyle()
        {
            Font = new Font("Candara", 10),
        };

        private readonly TextElementStyle activeMoveStyle = new TextElementStyle()
        {
            Font = new Font("Candara", 10, FontStyle.Bold),
        };

        private readonly SyntaxRenderer<PGNTerminalSymbol> syntaxRenderer;

        public MovesTextBox()
        {
            BorderStyle = BorderStyle.None;
            syntaxRenderer = SyntaxRenderer<PGNTerminalSymbol>.AttachTo(this);
            BackColor = Color.White;
            ForeColor = Color.Black;
            applyDefaultStyle();
        }

        private void applyDefaultStyle()
        {
            using (var updateToken = BeginUpdateRememberCaret())
            {
                Font = defaultStyle.Font;
                SelectAll();
                SelectionFont = defaultStyle.Font;
            }
        }

        private void applyStyle(TextElement<PGNTerminalSymbol> element, TextElementStyle style)
        {
            using (var updateToken = BeginUpdateRememberCaret())
            {
                Select(element.Start, element.Length);
                if (style.HasFont) SelectionFont = style.Font;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                defaultStyle.Font.Dispose();
                activeMoveStyle.Font.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets the action handler for this control.
        /// </summary>
        public UIActionHandler ActionHandler { get; } = new UIActionHandler();

        private Chess.IMoveFormatter moveFormatter;

        /// <summary>
        /// Gets or sets the <see cref="Chess.IMoveFormatter"/> to use for formatting the moves.
        /// </summary>
        public Chess.IMoveFormatter MoveFormatter
        {
            get { return moveFormatter; }
            set
            {
                if (moveFormatter != value)
                {
                    moveFormatter = value;
                    refreshText();
                }
            }
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

        private bool hasGameAndMoveFormatter => moveFormatter != null && game != null;

        internal void GameUpdated()
        {
            if (!IsUpdating)
            {
                updateText();
            }
        }

        private IEnumerable<PGNTerminalSymbol> emitInitialBlackSideToMoveEllipsis(int plyCount)
        {
            if (plyCount % 2 == 1)
            {
                yield return new MoveCounterSymbol(plyCount / 2 + 1);
                yield return new BlackToMoveEllipsisSymbol();
            }
        }

        private IEnumerable<PGNTerminalSymbol> emitMove(Chess.Game game, Chess.Variation line, int plyCount)
        {
            if (plyCount % 2 == 0)
            {
                yield return new MoveCounterSymbol(plyCount / 2 + 1);
            }

            yield return new FormattedMoveSymbol(moveFormatter.FormatMove(game, line.Move), line);
        }

        private IEnumerable<PGNTerminalSymbol> emitMainLine(Chess.Game game, bool emitSpace)
        {
            for (;;)
            {
                Chess.MoveTree current = game.ActiveTree;
                int plyCount = current.PlyCount;

                if (current.MainLine != null)
                {
                    foreach (var element in emitMove(game, current.MainLine, plyCount)) yield return element;
                    emitSpace = true;
                }

                foreach (var sideLine in current.SideLines)
                {
                    game.SetActiveTree(current);

                    yield return new SideLineStartSymbol();

                    foreach (var element in emitInitialBlackSideToMoveEllipsis(plyCount)) yield return element;
                    foreach (var element in emitMove(game, sideLine, plyCount)) yield return element;

                    foreach (var element in emitMainLine(game, true)) yield return element;
                    yield return new SideLineEndSymbol();

                    emitSpace = true;
                }

                if (current.MainLine == null)
                {
                    yield break;
                }

                game.SetActiveTree(current.MainLine.MoveTree);
            }
        }

        private IEnumerable<PGNTerminalSymbol> emitMoveTree(Chess.Game game)
        {
            if (game.MoveTree.MainLine != null)
            {
                foreach (var element in emitInitialBlackSideToMoveEllipsis(game.MoveTree.PlyCount)) yield return element;
            }

            foreach (var element in emitMainLine(game, false)) yield return element;
        }

        private List<PGNTerminalSymbol> getUpdatedElements()
        {
            if (hasGameAndMoveFormatter)
            {
                // Copy the game to be able to format moves correctly without affecting game.Game.ActiveTree.
                Chess.Game copiedGame = game.Game.Copy();

                return new List<PGNTerminalSymbol>(emitMoveTree(copiedGame));
            }

            return new List<PGNTerminalSymbol>();
        }

        private TextElement<PGNTerminalSymbol> currentActiveMoveStyleElement;

        private void refreshText()
        {
            // Clear and build the entire text anew by clearing the old element list.
            using (var updateToken = BeginUpdate())
            {
                syntaxRenderer.Clear();
                updateText();
            }
        }

        private void updateText()
        {
            List<PGNTerminalSymbol> updatedTerminalSymbols = getUpdatedElements();

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

                if (!hasGameAndMoveFormatter || game.Game.IsFirstMove)
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

        protected override void OnSelectionChanged(EventArgs e)
        {
            if (!IsUpdating && SelectionLength == 0 && hasGameAndMoveFormatter)
            {
                int selectionStart = SelectionStart;

                int elemIndex;
                if (selectionStart == 0)
                {
                    // Go to the initial position when the caret is at the start.
                    elemIndex = -1;
                }
                else
                {
                    List<int> startIndexes = syntaxRenderer.Elements.Select(x => x.Start).ToList();

                    // Get the index of the element that contains the caret.
                    elemIndex = startIndexes.BinarySearch(selectionStart);
                    if (elemIndex < 0) elemIndex = ~elemIndex - 1;

                    // Look for an element which delimits a move.
                    while (elemIndex >= 0
                        && !(syntaxRenderer.Elements[elemIndex].TerminalSymbol is MoveCounterSymbol)
                        && !(syntaxRenderer.Elements[elemIndex].TerminalSymbol is FormattedMoveSymbol))
                    {
                        elemIndex--;
                    }
                }

                TextElement<PGNTerminalSymbol> newActiveMoveElement;
                Chess.MoveTree newActiveTree;
                if (elemIndex < 0)
                {
                    // Exceptional case to go to the initial position.
                    newActiveMoveElement = null;
                    newActiveTree = game.Game.MoveTree;
                }
                else
                {
                    // If at a MoveCounter, go forward until the actual FormattedMove.
                    while (!(syntaxRenderer.Elements[elemIndex].TerminalSymbol is FormattedMoveSymbol)) elemIndex++;

                    // Go to the position after the selected move.
                    newActiveMoveElement = syntaxRenderer.Elements[elemIndex];
                    newActiveTree = ((FormattedMoveSymbol)newActiveMoveElement.TerminalSymbol).Variation.MoveTree;
                }

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

                        if (newActiveMoveElement != null)
                        {
                            currentActiveMoveStyleElement = newActiveMoveElement;
                            applyStyle(newActiveMoveElement, activeMoveStyle);
                        }
                    }
                }
            }

            base.OnSelectionChanged(e);
        }
    }
}

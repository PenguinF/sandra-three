/*********************************************************************************
 * MovesTextBox.cs
 * 
 * Copyright (c) 2004-2017 Henk Nicolai
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

        private readonly SyntaxRenderer syntaxRenderer;

        public MovesTextBox()
        {
            ReadOnly = true;
            BorderStyle = BorderStyle.None;
            syntaxRenderer = SyntaxRenderer.AttachTo(this);
            BackColor = Color.White;
            ForeColor = Color.Black;
            AutoWordSelection = true;
            applyDefaultStyle();
        }

        private void applyDefaultStyle()
        {
            Font = defaultStyle.Font;
            SelectAll();
            SelectionFont = defaultStyle.Font;
        }

        private void applyStyle(TextElementOld element, TextElementStyle style)
        {
            Select(element.Start, element.Length);
            if (style.HasFont) SelectionFont = style.Font;
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

        /// <summary>
        /// Represents an element of formatted text displayed by a <see cref="SyntaxRenderer"/>,
        /// which maps to exactly one terminal symbol.
        /// </summary>
        public sealed class TextElement
        {
            public TextElementOld TerminalSymbol;
            public int Start { get { return TerminalSymbol.Start; } set { TerminalSymbol.Start = value; } }
            public int Length { get { return TerminalSymbol.Length; } set { TerminalSymbol.Length = value; } }
        }

        public abstract class TextElementOld
        {
            public int Start;
            public int Length;
            public abstract string GetText();

            public sealed class Space : TextElementOld
            {
                public const string SpaceText = " ";
                public override string GetText() => SpaceText;
            }

            public sealed class SideLineStart : TextElementOld
            {
                public const string SideLineStartText = "(";
                public override string GetText() => SideLineStartText;
            }

            public sealed class SideLineEnd : TextElementOld
            {
                public const string SideLineEndText = ")";
                public override string GetText() => SideLineEndText;
            }

            public sealed class InitialBlackSideToMoveEllipsis : TextElementOld
            {
                public const string EllipsisText = "..";
                public override string GetText() => EllipsisText;
            }

            public abstract class MoveDelimiter : TextElementOld { }

            public sealed class MoveCounter : MoveDelimiter
            {
                readonly int value;
                public override string GetText() => value + ".";
                public MoveCounter(int value) { this.value = value; }
            }

            public sealed class FormattedMove : MoveDelimiter
            {
                readonly string value;
                public override string GetText() => value;
                public readonly Chess.Variation Variation;
                public FormattedMove(string value, Chess.Variation variation)
                {
                    this.value = value;
                    Variation = variation;
                }
            }
        }

        private readonly List<TextElement> elements = new List<TextElement>();

        private IEnumerable<TextElementOld> emitInitialBlackSideToMoveEllipsis(int plyCount)
        {
            if (plyCount % 2 == 1)
            {
                // Add an ellipsis for the previous white move. 
                yield return new TextElementOld.MoveCounter(plyCount / 2 + 1);
                yield return new TextElementOld.InitialBlackSideToMoveEllipsis();
                yield return new TextElementOld.Space();
            }
        }

        private IEnumerable<TextElementOld> emitMove(Chess.Game game, Chess.Variation line, int plyCount)
        {
            if (plyCount % 2 == 0)
            {
                yield return new TextElementOld.MoveCounter(plyCount / 2 + 1);
                yield return new TextElementOld.Space();
            }

            yield return new TextElementOld.FormattedMove(moveFormatter.FormatMove(game, line.Move), line);
        }

        // Parametrized on emitSpace because this method may not yield anything,
        // in which case no spaces should be emitted at all.
        private IEnumerable<TextElementOld> emitMainLine(Chess.Game game, bool emitSpace)
        {
            for (;;)
            {
                // Remember the game's active tree because it's the starting point of side lines.
                Chess.MoveTree current = game.ActiveTree;
                int plyCount = current.PlyCount;

                // Emit the main move before side lines which start at the same plyCount.
                if (current.MainLine != null)
                {
                    if (emitSpace) yield return new TextElementOld.Space();
                    foreach (var element in emitMove(game, current.MainLine, plyCount)) yield return element;
                    emitSpace = true;
                }

                foreach (var sideLine in current.SideLines)
                {
                    // Reset active tree before going into each side line.
                    game.SetActiveTree(current);

                    if (emitSpace) yield return new TextElementOld.Space();
                    yield return new TextElementOld.SideLineStart();

                    // Emit first move.
                    foreach (var element in emitInitialBlackSideToMoveEllipsis(plyCount)) yield return element;
                    foreach (var element in emitMove(game, sideLine, plyCount)) yield return element;

                    // Recurse here.
                    foreach (var element in emitMainLine(game, true)) yield return element;
                    yield return new TextElementOld.SideLineEnd();

                    emitSpace = true;
                }

                if (current.MainLine == null)
                {
                    yield break;
                }

                // Goto next move in the main line.
                game.SetActiveTree(current.MainLine.MoveTree);
            }
        }

        private IEnumerable<TextElementOld> emitMoveTree(Chess.Game game)
        {
            // Possible initial black side to move ellipsis.
            if (game.MoveTree.MainLine != null)
            {
                foreach (var element in emitInitialBlackSideToMoveEllipsis(game.MoveTree.PlyCount)) yield return element;
            }

            // Treat as a regular main line.
            foreach (var element in emitMainLine(game, false)) yield return element;
        }

        private List<TextElementOld> getUpdatedElements()
        {
            if (hasGameAndMoveFormatter)
            {
                // Copy the game to be able to format moves correctly without affecting game.Game.ActiveTree.
                Chess.Game copiedGame = game.Game.Copy();

                return new List<TextElementOld>(emitMoveTree(copiedGame));
            }

            return new List<TextElementOld>();
        }

        private void refreshText()
        {
            // Clear and build the entire text anew by clearing the old element list.
            using (var updateToken = BeginUpdate())
            {
                elements.Clear();
                updateText();
            }
        }

        private void updateText()
        {
            var updated = getUpdatedElements();

            int existingElementCount = elements.Count;
            int updatedElementCount = updated.Count;

            // Instead of clearing and updating the entire textbox, compare the elements one by one.
            int minLength = Math.Min(existingElementCount, updatedElementCount);
            int agreeIndex = 0;
            while (agreeIndex < minLength)
            {
                var existingElement = elements[agreeIndex].TerminalSymbol;
                if (existingElement.GetText() == updated[agreeIndex].GetText())
                {
                    // Keep using the existing element so no derived information gets lost.
                    updated[agreeIndex] = existingElement;
                    ++agreeIndex;
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

                if (agreeIndex < existingElementCount)
                {
                    // Clear existing tail part.
                    int startDisagree = elements[agreeIndex].Start;
                    Select(startDisagree, TextLength - startDisagree);
                    // This only works if not read-only, so temporarily turn it off.
                    ReadOnly = false;
                    SelectedText = string.Empty;
                    ReadOnly = true;
                    elements.RemoveRange(agreeIndex, elements.Count - agreeIndex);
                }

                // Append new element texts.
                while (agreeIndex < updatedElementCount)
                {
                    var updatedElement = updated[agreeIndex];
                    updatedElement.Start = TextLength;
                    AppendText(updatedElement.GetText());
                    updatedElement.Length = TextLength - updatedElement.Start;
                    ++agreeIndex;
                    elements.Add(new TextElement() { TerminalSymbol = updatedElement });
                }

                // Make the active move bold.
                if (!hasGameAndMoveFormatter || game.Game.IsFirstMove)
                {
                    Select(0, 0);
                }
                else
                {
                    foreach (var formattedMoveElement in elements.Select(x => x.TerminalSymbol).OfType<TextElementOld.FormattedMove>())
                    {
                        if (formattedMoveElement.Variation.MoveTree == game.Game.ActiveTree)
                        {
                            applyStyle(formattedMoveElement, activeMoveStyle);

                            if (!ContainsFocus)
                            {
                                // Also update the caret so the active move is in view.
                                Select(formattedMoveElement.Start + formattedMoveElement.Length, 0);
                                ScrollToCaret();
                            }

                            break;
                        }
                    }
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
                    List<int> startIndexes = elements.Select(x => x.Start).ToList();

                    // Get the index of the element that contains the caret.
                    elemIndex = startIndexes.BinarySearch(selectionStart);
                    if (elemIndex < 0) elemIndex = ~elemIndex - 1;

                    // Look for an element which delimits a move.
                    while (elemIndex >= 0 && !(elements[elemIndex].TerminalSymbol is TextElementOld.MoveDelimiter))
                    {
                        elemIndex--;
                    }
                }

                TextElementOld.FormattedMove newActiveMoveElement;
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
                    while (!(elements[elemIndex].TerminalSymbol is TextElementOld.FormattedMove)) elemIndex++;

                    // Go to the position after the selected move.
                    newActiveMoveElement = (TextElementOld.FormattedMove)elements[elemIndex].TerminalSymbol;
                    newActiveTree = newActiveMoveElement.Variation.MoveTree;
                }

                // Update the active move index in the game.
                if (game.Game.ActiveTree != newActiveTree)
                {
                    using (var updateToken = BeginUpdate())
                    {
                        try
                        {
                            // Search for the current active move element to clear its font.
                            foreach (var formattedMoveElement in elements.Select(x => x.TerminalSymbol).OfType<TextElementOld.FormattedMove>())
                            {
                                if (formattedMoveElement.Variation.MoveTree == game.Game.ActiveTree)
                                {
                                    applyStyle(formattedMoveElement, defaultStyle);
                                }
                            }

                            game.Game.SetActiveTree(newActiveTree);

                            game.ActiveMoveTreeUpdated();
                            ActionHandler.Invalidate();

                            if (newActiveMoveElement != null)
                            {
                                applyStyle(newActiveMoveElement, activeMoveStyle);
                            }
                        }
                        finally
                        {
                            Select(selectionStart, 0);
                        }
                    }
                }
            }

            base.OnSelectionChanged(e);
        }
    }
}

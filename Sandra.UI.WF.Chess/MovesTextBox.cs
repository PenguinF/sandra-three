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
        readonly Font regularFont = new Font("Candara", 10);
        readonly Font lastMoveFont = new Font("Candara", 10, FontStyle.Bold);

        public MovesTextBox()
        {
            ReadOnly = true;
            BorderStyle = BorderStyle.None;
            BackColor = Color.White;
            ForeColor = Color.Black;
            Font = regularFont;
            AutoWordSelection = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                regularFont.Dispose();
                lastMoveFont.Dispose();
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

        internal void GameUpdated()
        {
            if (!IsUpdating)
            {
                updateText();
            }
        }

        private abstract class TextElement
        {
            public int Start;
            public int Length;
            public abstract string GetText();

            public sealed class Space : TextElement
            {
                public const string SpaceText = " ";
                public override string GetText() => SpaceText;
            }

            public sealed class SideLineStart : TextElement
            {
                public const string SideLineStartText = "(";
                public override string GetText() => SideLineStartText;
            }

            public sealed class SideLineEnd : TextElement
            {
                public const string SideLineEndText = ")";
                public override string GetText() => SideLineEndText;
            }

            public sealed class InitialBlackSideToMoveEllipsis : TextElement
            {
                public const string EllipsisText = "..";
                public override string GetText() => EllipsisText;
            }

            public sealed class MoveCounter : TextElement
            {
                readonly int value;
                public override string GetText() => value + ".";
                public MoveCounter(int value) { this.value = value; }
            }

            public sealed class FormattedMove : TextElement
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

        private List<TextElement> elements;

        private void updateFont(TextElement element, Font newFont)
        {
            Select(element.Start, element.Length);
            SelectionFont = newFont;
        }

        private IEnumerable<TextElement> emitInitialBlackSideToMoveEllipsis(int plyCount)
        {
            if (plyCount % 2 == 1)
            {
                // Add an ellipsis for the previous white move. 
                yield return new TextElement.MoveCounter(plyCount / 2 + 1);
                yield return new TextElement.InitialBlackSideToMoveEllipsis();
                yield return new TextElement.Space();
            }
        }

        private IEnumerable<TextElement> emitMove(Chess.Game game, Chess.Variation line, int plyCount)
        {
            if (plyCount % 2 == 0)
            {
                yield return new TextElement.MoveCounter(plyCount / 2 + 1);
                yield return new TextElement.Space();
            }

            yield return new TextElement.FormattedMove(moveFormatter.FormatMove(game, line.Move), line);
        }

        // Parametrized on emitSpace because this method may not yield anything,
        // in which case no spaces should be emitted at all.
        private IEnumerable<TextElement> emitMainLine(Chess.Game game, bool emitSpace)
        {
            for (;;)
            {
                // Remember the game's active tree because it's the starting point of side lines.
                Chess.MoveTree current = game.ActiveTree;
                int plyCount = current.PlyCount;

                // Emit the main move before side lines which start at the same plyCount.
                if (current.MainLine != null)
                {
                    if (emitSpace) yield return new TextElement.Space();
                    foreach (var element in emitMove(game, current.MainLine, plyCount)) yield return element;
                    emitSpace = true;
                }

                foreach (var sideLine in current.SideLines)
                {
                    // Reset active tree before going into each side line.
                    game.SetActiveTree(current);

                    if (emitSpace) yield return new TextElement.Space();
                    yield return new TextElement.SideLineStart();

                    // Emit first move.
                    foreach (var element in emitInitialBlackSideToMoveEllipsis(plyCount)) yield return element;
                    foreach (var element in emitMove(game, sideLine, plyCount)) yield return element;

                    // Recurse here.
                    foreach (var element in emitMainLine(game, true)) yield return element;
                    yield return new TextElement.SideLineEnd();

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

        private IEnumerable<TextElement> emitMoveTree(Chess.Game game)
        {
            // Possible initial black side to move ellipsis.
            if (game.MoveTree.MainLine != null)
            {
                foreach (var element in emitInitialBlackSideToMoveEllipsis(game.MoveTree.PlyCount)) yield return element;
            }

            // Treat as a regular main line.
            foreach (var element in emitMainLine(game, false)) yield return element;
        }

        private List<TextElement> getUpdatedElements()
        {
            if (moveFormatter != null && game != null)
            {
                // Copy the game to be able to format moves correctly without affecting game.Game.ActiveTree.
                Chess.Game copiedGame = game.Game.Copy();

                return new List<TextElement>(emitMoveTree(copiedGame));
            }

            return null;
        }

        private void refreshText()
        {
            // Clear and build the entire text anew by resetting the old element lists.
            elements = null;
            updateText();
        }

        private void updateText()
        {
            // Block OnSelectionChanged() which will be raised as a side effect of this method.
            BeginUpdate();
            try
            {
                var updated = getUpdatedElements();

                int existingElementCount = elements == null
                                         ? 0
                                         : elements.Count;

                int updatedElementCount = updated == null
                                        ? 0
                                        : updated.Count;


                // Instead of clearing and updating the entire textbox, compare the elements one by one.
                int minLength = Math.Min(existingElementCount, updatedElementCount);
                int agreeIndex = 0;
                while (agreeIndex < minLength)
                {
                    var existingElement = elements[agreeIndex];
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

                if (agreeIndex < existingElementCount)
                {
                    // Clear existing tail part.
                    int startDisagree = elements[agreeIndex].Start;
                    Select(startDisagree, TextLength - startDisagree);
                    // This only works if not read-only, so temporarily turn it off.
                    ReadOnly = false;
                    SelectedText = string.Empty;
                    ReadOnly = true;
                }

                // Append new element texts.
                while (agreeIndex < updatedElementCount)
                {
                    var updatedElement = updated[agreeIndex];
                    updatedElement.Start = TextLength;
                    AppendText(updatedElement.GetText());
                    updatedElement.Length = TextLength - updatedElement.Start;
                    ++agreeIndex;
                }

                elements = updated;

                // Reset all markup.
                SelectAll();
                SelectionFont = regularFont;

                // Make the active move bold.
                if (elements != null)
                {
                    foreach (var formattedMoveElement in elements.OfType<TextElement.FormattedMove>())
                    {
                        if (formattedMoveElement.Variation.MoveTree == game.Game.ActiveTree)
                        {
                            updateFont(formattedMoveElement, lastMoveFont);

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
            finally
            {
                EndUpdate();
            }
        }

        protected override void OnSelectionChanged(EventArgs e)
        {
            if (!IsUpdating && elements != null && SelectionLength == 0)
            {
                int selectionStart = SelectionStart;

                List<int> startIndexes = elements.Select(x => x.Start).ToList();

                // Get the index of the element that contains the caret.
                int elemIndex = startIndexes.BinarySearch(selectionStart);
                if (elemIndex < 0) elemIndex = ~elemIndex - 1;

                // Look for a FormattedMove element.
                while (elemIndex >= 0 && !(elements[elemIndex] is TextElement.FormattedMove))
                {
                    elemIndex--;
                }

                TextElement.FormattedMove newActiveMoveElement;
                Chess.MoveTree newActiveTree;
                if (elemIndex < 0)
                {
                    // Exceptional case to go to the initial position.
                    newActiveMoveElement = null;
                    newActiveTree = game.Game.MoveTree;
                }
                else
                {
                    // Go to the position after the selected move.
                    newActiveMoveElement = (TextElement.FormattedMove)elements[elemIndex];
                    newActiveTree = newActiveMoveElement.Variation.MoveTree;
                }

                // Update the active move index in the game.
                if (game.Game.ActiveTree != newActiveTree)
                {
                    BeginUpdate();
                    try
                    {
                        // Search for the current active move element to clear its font.
                        foreach (var formattedMoveElement in elements.OfType<TextElement.FormattedMove>())
                        {
                            if (formattedMoveElement.Variation.MoveTree == game.Game.ActiveTree)
                            {
                                updateFont(formattedMoveElement, regularFont);
                            }
                        }

                        game.Game.SetActiveTree(newActiveTree);
                        ActionHandler.Invalidate();
                        if (newActiveMoveElement != null)
                        {
                            updateFont(newActiveMoveElement, lastMoveFont);
                        }
                    }
                    finally
                    {
                        Select(selectionStart, 0);
                        EndUpdate();
                    }
                }
            }

            base.OnSelectionChanged(e);
        }
    }
}

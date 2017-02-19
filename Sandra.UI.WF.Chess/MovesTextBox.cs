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
                public readonly Chess.MoveIndex MoveIndex;
                public FormattedMove(string value, Chess.MoveIndex moveIndex)
                {
                    this.value = value;
                    MoveIndex = moveIndex;
                }
            }
        }

        private List<TextElement> elements;
        private List<TextElement.FormattedMove> moveElements;

        private void updateFont(TextElement element, Font newFont)
        {
            Select(element.Start, element.Length);
            SelectionFont = newFont;
        }

        private List<TextElement> getUpdatedElements()
        {
            if (moveFormatter != null && game != null)
            {
                var updated = new List<TextElement>();

                // Simulate a game to be able to format moves correctly.
                Chess.Game simulatedGame = new Chess.Game(game.Game.InitialPosition);

                bool first = true;
                int plyCounter = 0;

                Chess.Variation current = game.Game.MoveTree.Main;
                while (current != null)
                {
                    if (first)
                    {
                        if (simulatedGame.InitialSideToMove == Chess.Color.Black)
                        {
                            // Adjust plyCounter and add an ellipsis for the previous white move. 
                            updated.Add(new TextElement.MoveCounter(plyCounter / 2 + 1));
                            updated.Add(new TextElement.InitialBlackSideToMoveEllipsis());
                            updated.Add(new TextElement.Space());
                            plyCounter++;
                        }

                        first = false;
                    }
                    else
                    {
                        updated.Add(new TextElement.Space());
                    }

                    if (plyCounter % 2 == 0)
                    {
                        updated.Add(new TextElement.MoveCounter(plyCounter / 2 + 1));
                        updated.Add(new TextElement.Space());
                    }

                    updated.Add(new TextElement.FormattedMove(moveFormatter.FormatMove(simulatedGame, current.Move), current.MoveIndex));

                    ++plyCounter;
                    current = current.MoveTree.Main;
                }

                return updated;
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

                // Update moveElements as well.
                if (elements == null)
                {
                    moveElements = null;
                }
                else
                {
                    moveElements = new List<TextElement.FormattedMove>(elements.OfType<TextElement.FormattedMove>());

                    foreach (var formattedMoveElement in moveElements)
                    {
                        if (formattedMoveElement.MoveIndex.EqualTo(game.Game.ActiveMoveIndex))
                        {
                            // Make the active move bold.
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

                List<int> startIndexes = moveElements.Select(x => x.Start).ToList();

                // Get the index of the element that contains the caret.
                int elemIndex = startIndexes.BinarySearch(selectionStart);
                if (elemIndex < 0) elemIndex = ~elemIndex - 1;

                TextElement.FormattedMove newActiveMoveElement;
                Chess.MoveIndex newActiveMoveIndex;
                if (elemIndex < 0)
                {
                    // Exceptional case to go to the initial position.
                    newActiveMoveElement = null;
                    newActiveMoveIndex = Chess.MoveIndex.BeforeFirstMove;
                }
                else
                {
                    // Go to the position after the selected move.
                    newActiveMoveElement = moveElements[elemIndex];
                    newActiveMoveIndex = newActiveMoveElement.MoveIndex;
                }

                // Update the active move index in the game.
                if (!game.Game.ActiveMoveIndex.EqualTo(newActiveMoveIndex))
                {
                    BeginUpdate();
                    try
                    {
                        // Search for the current active move element to clear its font.
                        foreach (var formattedMoveElement in moveElements)
                        {
                            if (formattedMoveElement.MoveIndex.EqualTo(game.Game.ActiveMoveIndex))
                            {
                                updateFont(formattedMoveElement, regularFont);
                            }
                        }

                        game.Game.SetActiveMoveIndex(newActiveMoveIndex);
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

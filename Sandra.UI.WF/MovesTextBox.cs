/*********************************************************************************
 * MovesTextBox.cs
 * 
 * Copyright (c) 2004-2016 Henk Nicolai
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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents a read-only Windows rich text box which displays a list of chess moves.
    /// </summary>
    public class MovesTextBox : RichTextBox
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

        private Chess.AbstractMoveFormatter moveFormatter;

        /// <summary>
        /// Gets or sets the <see cref="Chess.AbstractMoveFormatter"/> to use for formatting the moves.
        /// </summary>
        public Chess.AbstractMoveFormatter MoveFormatter
        {
            get { return moveFormatter; }
            set
            {
                if (moveFormatter != value)
                {
                    moveFormatter = value;
                    updateText();
                }
            }
        }

        private Chess.Game game;

        /// <summary>
        /// Gets or sets the chess game which contains the moves to be formatted.
        /// </summary>
        public Chess.Game Game
        {
            get { return game; }
            set
            {
                if (game != value)
                {
                    if (game != null) game.MoveMade -= game_MoveMade;
                    game = value;
                    if (game != null) game.MoveMade += game_MoveMade;
                    updateText();
                }
            }
        }

        private void game_MoveMade(object sender, Chess.MoveMadeEventArgs e)
        {
            updateText();
        }

        private abstract class TextElement
        {
            public int Start;
            public abstract string Text { get; }

            public sealed class Space : TextElement
            {
                public const string SpaceText = " ";
                public override string Text => SpaceText;
            }

            public sealed class InitialBlackSideToMoveEllipsis : TextElement
            {
                public const string EllipsisText = "1...";
                public override string Text => EllipsisText;
            }

            public sealed class MoveCounter : TextElement
            {
                readonly int value;
                public override string Text => value + ".";
                public MoveCounter(int value) { this.value = value; }
            }

            public sealed class FormattedMove : TextElement
            {
                readonly string value;
                public override string Text => value;
                public FormattedMove(string value) { this.value = value; }
            }
        }

        private List<TextElement> elements;
        private List<TextElement.FormattedMove> moveElements;

        private void updateText()
        {
            elements = null;
            moveElements = null;
            Clear();

            if (moveFormatter != null && game != null)
            {
                elements = new List<TextElement>();
                moveElements = new List<TextElement.FormattedMove>();

                // Simulate a game to be able to format moves correctly.
                Chess.Game simulatedGame = new Chess.Game(game.InitialPosition);

                foreach (Chess.Move move in game.Moves)
                {
                    int plyCounter = simulatedGame.MoveCount;
                    if (plyCounter > 0) elements.Add(new TextElement.Space());

                    if (simulatedGame.InitialSideToMove == Chess.Color.Black)
                    {
                        if (plyCounter == 0)
                        {
                            elements.Add(new TextElement.InitialBlackSideToMoveEllipsis());
                            elements.Add(new TextElement.Space());
                        }
                        ++plyCounter;
                    }

                    if (plyCounter % 2 == 0)
                    {
                        elements.Add(new TextElement.MoveCounter(plyCounter / 2 + 1));
                        elements.Add(new TextElement.Space());
                    }

                    var formattedMoveElement = new TextElement.FormattedMove(moveFormatter.FormatMove(simulatedGame, move));
                    elements.Add(formattedMoveElement);
                    moveElements.Add(formattedMoveElement);
                }
            }

            if (elements != null)
            {
                elements.ForEach(element =>
                {
                    element.Start = TextLength;
                    AppendText(element.Text);
                });

                // Make the last move bold. This is the move before, not after ActiveMoveIndex.
                if (game.ActiveMoveIndex > 0)
                {
                    var lastMoveElement = moveElements[game.ActiveMoveIndex - 1];
                    Select(lastMoveElement.Start, lastMoveElement.Text.Length);
                    SelectionFont = lastMoveFont;
                    Select(lastMoveElement.Text.Length, 0);
                }
            }
        }
    }
}

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
using System.Drawing;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents a read-only Windows rich text box which displays a list of chess moves.
    /// </summary>
    public class MovesTextBox : RichTextBox
    {
        public MovesTextBox()
        {
            ReadOnly = true;
            BorderStyle = BorderStyle.None;
            BackColor = Color.White;
            ForeColor = Color.Black;
            Font = new Font("Candara", 10);
            AutoWordSelection = true;
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

        private void updateText()
        {
            Clear();

            if (moveFormatter != null && game != null)
            {
                Chess.Game simulatedGame = new Chess.Game(game.InitialPosition);

                bool first = true;
                int plyCounter = simulatedGame.SideToMove == Chess.Color.White ? 0 : 1;

                foreach (Chess.Move move in game.Moves)
                {
                    if (!first) AppendText(" ");

                    bool even = plyCounter % 2 == 0;
                    if (even || first)
                    {
                        AppendText((plyCounter / 2 + 1) + ".");
                        if (!even) AppendText("..");
                        AppendText(" ");
                    }

                    var moveInfo = simulatedGame.TryMakeMove(move, true);
                    AppendText(moveFormatter.FormatMove(simulatedGame, move, moveInfo));

                    first = false;
                    ++plyCounter;
                }
            }
        }
    }
}

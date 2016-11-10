﻿/*********************************************************************************
 * StandardChessBoardForm.cs
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

namespace Sandra.UI.WF
{
    /// <summary>
    /// Form which contains a chess board on which a standard game of chess is played.
    /// </summary>
    public class StandardChessBoardForm : PlayingBoardForm
    {
        public StandardChessBoardForm()
        {
            PlayingBoard.MouseEnterSquare += playingBoard_MouseEnterSquare;
            PlayingBoard.MouseLeaveSquare += playingBoard_MouseLeaveSquare;
        }

        private void playingBoard_MouseEnterSquare(object sender, SquareEventArgs e)
        {
            PlayingBoard playingBoard = (PlayingBoard)sender;
            if (!playingBoard.IsDraggingImage)
            {
                playingBoard.SetIsImageHighLighted(e.X, e.Y, true);
            }
        }

        private void playingBoard_MouseLeaveSquare(object sender, SquareEventArgs e)
        {
            PlayingBoard playingBoard = (PlayingBoard)sender;
            if (!playingBoard.IsDraggingImage)
            {
                playingBoard.SetIsImageHighLighted(e.X, e.Y, false);
            }
        }
    }
}

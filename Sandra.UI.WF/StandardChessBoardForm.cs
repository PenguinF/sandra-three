/*********************************************************************************
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

using System.Drawing;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Form which contains a chess board on which a standard game of chess is played.
    /// </summary>
    public class StandardChessBoardForm : PlayingBoardForm
    {
        public StandardChessBoardForm()
        {
            ShowIcon = false;
            MaximizeBox = false;

            PlayingBoard.MouseEnterSquare += playingBoard_MouseEnterSquare;
            PlayingBoard.MouseLeaveSquare += playingBoard_MouseLeaveSquare;
            PlayingBoard.MoveCancel += playingBoard_MoveCancel;
            PlayingBoard.MoveCommit += playingBoard_MoveCommit;
        }

        private void playingBoard_MouseEnterSquare(object sender, SquareEventArgs e)
        {
            if (PlayingBoard.IsMoving)
            {
                PlayingBoard.SetSquareOverlayColor(e.Location.X, e.Location.Y, Color.FromArgb(80, 255, 255, 255));
            }
            else
            {
                PlayingBoard.SetIsImageHighLighted(e.Location.X, e.Location.Y, true);
            }
        }

        private void playingBoard_MouseLeaveSquare(object sender, SquareEventArgs e)
        {
            if (PlayingBoard.IsMoving)
            {
                PlayingBoard.SetSquareOverlayColor(e.Location.X, e.Location.Y, Color.FromArgb(48, 255, 190, 0));
            }
            else
            {
                PlayingBoard.SetIsImageHighLighted(e.Location.X, e.Location.Y, false);
            }
        }

        private void resetMoveStartSquareHighlight(MoveEventArgs e)
        {
            var hoverSquare = PlayingBoard.HoverSquare;
            if (hoverSquare == null || hoverSquare.X != e.Start.X || hoverSquare.Y != e.Start.Y)
            {
                PlayingBoard.SetIsImageHighLighted(e.Start.X, e.Start.Y, false);
            }
            if (hoverSquare != null)
            {
                PlayingBoard.SetIsImageHighLighted(hoverSquare.X, hoverSquare.Y, true);
            }
            for (int x = 0; x < PlayingBoard.BoardSize; ++x)
            {
                for (int y = 0; y < PlayingBoard.BoardSize; ++y)
                {
                    PlayingBoard.SetSquareOverlayColor(x, y, new Color());
                }
            }
        }

        private void playingBoard_MoveCommit(object sender, MoveCommitEventArgs e)
        {
            resetMoveStartSquareHighlight(e);

            // Move piece from source to destination.
            if (e.Start.X != e.Target.X || e.Start.Y != e.Target.Y)
            {
                PlayingBoard.SetForegroundImage(e.Target.X, e.Target.Y, PlayingBoard.GetForegroundImage(e.Start.X, e.Start.Y));
                PlayingBoard.SetForegroundImage(e.Start.X, e.Start.Y, null);
            }
        }

        private void playingBoard_MoveCancel(object sender, MoveEventArgs e)
        {
            resetMoveStartSquareHighlight(e);
        }
    }
}

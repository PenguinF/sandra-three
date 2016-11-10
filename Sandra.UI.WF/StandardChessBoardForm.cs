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
            ShowIcon = false;
            MaximizeBox = false;

            PlayingBoard.MouseEnterSquare += playingBoard_MouseEnterSquare;
            PlayingBoard.MouseLeaveSquare += playingBoard_MouseLeaveSquare;
            PlayingBoard.DragImageCancel += playingBoard_DragImageCancel;
            PlayingBoard.DragImageDrop += playingBoard_DragImageDrop;
        }

        private SquareEventArgs hoverSquare;

        private void playingBoard_MouseEnterSquare(object sender, SquareEventArgs e)
        {
            hoverSquare = e;
            if (PlayingBoard.IsDraggingImage)
            {
                PlayingBoard.SetIsSquareHighLighted(e.X, e.Y, false);
            }
            else
            {
                PlayingBoard.SetIsImageHighLighted(e.X, e.Y, true);
            }
        }

        private void playingBoard_MouseLeaveSquare(object sender, SquareEventArgs e)
        {
            hoverSquare = null;
            if (PlayingBoard.IsDraggingImage)
            {
                PlayingBoard.SetIsSquareHighLighted(e.X, e.Y, true);
            }
            else
            {
                PlayingBoard.SetIsImageHighLighted(e.X, e.Y, false);
            }
        }

        private void resetDragStartSquareHighlight(DragSquareEventArgs e)
        {
            if (hoverSquare == null || hoverSquare.X != e.StartX || hoverSquare.Y != e.StartY)
            {
                PlayingBoard.SetIsImageHighLighted(e.StartX, e.StartY, false);
            }
            if (hoverSquare != null)
            {
                PlayingBoard.SetIsImageHighLighted(hoverSquare.X, hoverSquare.Y, true);
            }
            for (int x = 0; x < PlayingBoard.BoardSize; ++x)
            {
                for (int y = 0; y < PlayingBoard.BoardSize; ++y)
                {
                    PlayingBoard.SetIsSquareHighLighted(x, y, false);
                }
            }
        }

        private void playingBoard_DragImageDrop(object sender, DragOverSquareEventArgs e)
        {
            resetDragStartSquareHighlight(e);

            // Move piece from source to destination.
            if (e.StartX != e.DragX || e.StartY != e.DragY)
            {
                PlayingBoard.SetForegroundImage(e.DragX, e.DragY, PlayingBoard.GetForegroundImage(e.StartX, e.StartY));
                PlayingBoard.SetForegroundImage(e.StartX, e.StartY, null);
            }
        }

        private void playingBoard_DragImageCancel(object sender, DragSquareEventArgs e)
        {
            resetDragStartSquareHighlight(e);
        }
    }
}

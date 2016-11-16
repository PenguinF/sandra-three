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
        public EnumIndexedArray<Chess.NonEmptyColoredPiece, Image> PieceImages { get; private set; }

        public void UpdatePieceImages(EnumIndexedArray<Chess.NonEmptyColoredPiece, Image> pieceImages)
        {
            PieceImages = pieceImages;
        }

        public StandardChessBoardForm()
        {
            ShowIcon = false;
            MaximizeBox = false;

            PlayingBoard.BoardWidth = Chess.Constants.SquareCount;
            PlayingBoard.BoardHeight = Chess.Constants.SquareCount;

            PlayingBoard.MouseEnterSquare += playingBoard_MouseEnterSquare;
            PlayingBoard.MouseLeaveSquare += playingBoard_MouseLeaveSquare;
            PlayingBoard.MoveCancel += playingBoard_MoveCancel;
            PlayingBoard.MoveCommit += playingBoard_MoveCommit;
        }

        private SquareEventArgs hoverSquare;

        private void playingBoard_MouseEnterSquare(object sender, SquareEventArgs e)
        {
            hoverSquare = e;
            if (PlayingBoard.IsMoving)
            {
                PlayingBoard.SetSquareOverlayColor(e.X, e.Y, Color.FromArgb(80, 255, 255, 255));
            }
            else
            {
                PlayingBoard.SetIsImageHighLighted(e.X, e.Y, true);
            }
        }

        private void playingBoard_MouseLeaveSquare(object sender, SquareEventArgs e)
        {
            hoverSquare = null;
            if (PlayingBoard.IsMoving)
            {
                PlayingBoard.SetSquareOverlayColor(e.X, e.Y, Color.FromArgb(48, 255, 190, 0));
            }
            else
            {
                PlayingBoard.SetIsImageHighLighted(e.X, e.Y, false);
            }
        }

        private void resetMoveStartSquareHighlight(MoveEventArgs e)
        {
            if (hoverSquare == null || hoverSquare.X != e.StartX || hoverSquare.Y != e.StartY)
            {
                PlayingBoard.SetIsImageHighLighted(e.StartX, e.StartY, false);
            }
            if (hoverSquare != null)
            {
                PlayingBoard.SetIsImageHighLighted(hoverSquare.X, hoverSquare.Y, true);
            }
            for (int x = 0; x < PlayingBoard.BoardWidth; ++x)
            {
                for (int y = 0; y < PlayingBoard.BoardHeight; ++y)
                {
                    PlayingBoard.SetSquareOverlayColor(x, y, new Color());
                }
            }
        }

        private void playingBoard_MoveCommit(object sender, MoveCommitEventArgs e)
        {
            resetMoveStartSquareHighlight(e);

            // Move piece from source to destination.
            if (e.StartX != e.TargetX || e.StartY != e.TargetY)
            {
                PlayingBoard.SetForegroundImage(e.TargetX, e.TargetY, PlayingBoard.GetForegroundImage(e.StartX, e.StartY));
                PlayingBoard.SetForegroundImage(e.StartX, e.StartY, null);
            }
        }

        private void playingBoard_MoveCancel(object sender, MoveEventArgs e)
        {
            resetMoveStartSquareHighlight(e);
        }

        private void clearBoard()
        {
            for (int y = 0; y < PlayingBoard.BoardHeight; ++y)
            {
                for (int x = 0; x < PlayingBoard.BoardWidth; ++x)
                {
                    PlayingBoard.SetForegroundImage(x, y, null);
                }
            }
        }

        public void InitializeStartPosition()
        {
            clearBoard();

            var startPosition = EnumIndexedArray<Chess.NonEmptyColoredPiece, ulong>.New();

            startPosition[Chess.NonEmptyColoredPiece.WhitePawn] = Chess.Constants.WhiteInStartPosition & Chess.Constants.PawnsInStartPosition;
            startPosition[Chess.NonEmptyColoredPiece.WhiteKnight] = Chess.Constants.WhiteInStartPosition & Chess.Constants.KnightsInStartPosition;
            startPosition[Chess.NonEmptyColoredPiece.WhiteBishop] = Chess.Constants.WhiteInStartPosition & Chess.Constants.BishopsInStartPosition;
            startPosition[Chess.NonEmptyColoredPiece.WhiteRook] = Chess.Constants.WhiteInStartPosition & Chess.Constants.RooksInStartPosition;
            startPosition[Chess.NonEmptyColoredPiece.WhiteQueen] = Chess.Constants.WhiteInStartPosition & Chess.Constants.QueensInStartPosition;
            startPosition[Chess.NonEmptyColoredPiece.WhiteKing] = Chess.Constants.WhiteInStartPosition & Chess.Constants.KingsInStartPosition;
            startPosition[Chess.NonEmptyColoredPiece.BlackPawn] = Chess.Constants.BlackInStartPosition & Chess.Constants.PawnsInStartPosition;
            startPosition[Chess.NonEmptyColoredPiece.BlackKnight] = Chess.Constants.BlackInStartPosition & Chess.Constants.KnightsInStartPosition;
            startPosition[Chess.NonEmptyColoredPiece.BlackBishop] = Chess.Constants.BlackInStartPosition & Chess.Constants.BishopsInStartPosition;
            startPosition[Chess.NonEmptyColoredPiece.BlackRook] = Chess.Constants.BlackInStartPosition & Chess.Constants.RooksInStartPosition;
            startPosition[Chess.NonEmptyColoredPiece.BlackQueen] = Chess.Constants.BlackInStartPosition & Chess.Constants.QueensInStartPosition;
            startPosition[Chess.NonEmptyColoredPiece.BlackKing] = Chess.Constants.BlackInStartPosition & Chess.Constants.KingsInStartPosition;

            foreach (Chess.NonEmptyColoredPiece chessPieceImage in EnumHelper<Chess.NonEmptyColoredPiece>.AllValues)
            {
                foreach (var square in startPosition[chessPieceImage].AllSquares())
                {
                    PlayingBoard.SetForegroundImage(square.X(), 7 - square.Y(), PieceImages[chessPieceImage]);
                }
            }
        }
    }
}

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
using System.Windows.Forms;

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
            PlayingBoard.BoardWidth = Chess.Constants.SquareCount;
            PlayingBoard.BoardHeight = Chess.Constants.SquareCount;

            PlayingBoard.MouseMove += playingBoard_MouseMove;
            PlayingBoard.MouseEnterSquare += playingBoard_MouseEnterSquare;
            PlayingBoard.MouseLeaveSquare += playingBoard_MouseLeaveSquare;
            PlayingBoard.MoveCancel += playingBoard_MoveCancel;
            PlayingBoard.MoveCommit += playingBoard_MoveCommit;

            PlayingBoard.Paint += playingBoard_Paint;
        }

        /// <summary>
        /// Identifies a quadrant of a square.
        /// </summary>
        private enum SquareQuadrant
        {
            /// <summary>
            /// Either there is no active quadrant, or the square size is too small to subdivide into non-empty quadrants.
            /// </summary>
            Indeterminate,

            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
        }

        private Chess.NonEmptyColoredPiece getPromoteToPiece(SquareQuadrant quadrant, Chess.Color promoteColor)
        {
            switch (promoteColor)
            {
                case Chess.Color.White:
                    switch (quadrant)
                    {
                        case SquareQuadrant.TopLeft:
                            return Chess.NonEmptyColoredPiece.WhiteRook;
                        case SquareQuadrant.TopRight:
                            return Chess.NonEmptyColoredPiece.WhiteBishop;
                        case SquareQuadrant.BottomRight:
                            return Chess.NonEmptyColoredPiece.WhiteKnight;
                        case SquareQuadrant.BottomLeft:
                        case SquareQuadrant.Indeterminate:
                        default:
                            return Chess.NonEmptyColoredPiece.WhiteQueen;
                    }
                case Chess.Color.Black:
                default:
                    switch (quadrant)
                    {
                        case SquareQuadrant.BottomLeft:
                            return Chess.NonEmptyColoredPiece.BlackRook;
                        case SquareQuadrant.BottomRight:
                            return Chess.NonEmptyColoredPiece.BlackBishop;
                        case SquareQuadrant.TopRight:
                            return Chess.NonEmptyColoredPiece.BlackKnight;
                        case SquareQuadrant.TopLeft:
                        case SquareQuadrant.Indeterminate:
                        default:
                            return Chess.NonEmptyColoredPiece.BlackQueen;
                    }
            }
        }

        private SquareQuadrant hoverQuadrant;

        private void updateHoverQuadrant(SquareQuadrant value, Chess.Color promoteColor)
        {
            if (hoverQuadrant != value)
            {
                hoverQuadrant = value;
                if (value == SquareQuadrant.Indeterminate)
                {
                    PlayingBoard.MovingImage = null;
                }
                else
                {
                    PlayingBoard.MovingImage = PieceImages[getPromoteToPiece(value, promoteColor)];
                }
            }
        }

        private bool isPromoting(SquareLocation location, out Chess.Color promoteColor)
        {
            if (PlayingBoard.IsMoving && location != null && PlayingBoard.MoveStartSquare != location)
            {
                if (location.Y == 0)
                {
                    promoteColor = Chess.Color.White;
                    return PlayingBoard.GetForegroundImage(PlayingBoard.MoveStartSquare) == PieceImages[Chess.NonEmptyColoredPiece.WhitePawn];
                }
                else if (location.Y == 7)
                {
                    promoteColor = Chess.Color.Black;
                    return PlayingBoard.GetForegroundImage(PlayingBoard.MoveStartSquare) == PieceImages[Chess.NonEmptyColoredPiece.BlackPawn];
                }
            }
            promoteColor = default(Chess.Color);
            return false;
        }

        private void playingBoard_MouseMove(object sender, MouseEventArgs e)
        {
            Chess.Color promoteColor;
            if (isPromoting(PlayingBoard.HoverSquare, out promoteColor))
            {
                Rectangle hoverSquareRectangle = PlayingBoard.GetSquareRectangle(PlayingBoard.HoverSquare);
                SquareQuadrant hitQuadrant = SquareQuadrant.Indeterminate;
                if (hoverSquareRectangle.Contains(e.X, e.Y))
                {
                    int squareSize = PlayingBoard.SquareSize;
                    if (squareSize >= 2)
                    {
                        int halfSquareSize = (squareSize + 1) / 2;
                        if (e.X - hoverSquareRectangle.Left < halfSquareSize)
                        {
                            hitQuadrant = e.Y - hoverSquareRectangle.Top < halfSquareSize
                                        ? SquareQuadrant.TopLeft
                                        : SquareQuadrant.BottomLeft;
                        }
                        else
                        {
                            hitQuadrant = e.Y - hoverSquareRectangle.Top < halfSquareSize
                                        ? SquareQuadrant.TopRight
                                        : SquareQuadrant.BottomRight;
                        }
                    }
                }
                updateHoverQuadrant(hitQuadrant, promoteColor);
            }
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
            updateHoverQuadrant(SquareQuadrant.Indeterminate, default(Chess.Color));
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
            foreach (var squareLocation in PlayingBoard.AllSquareLocations)
            {
                PlayingBoard.SetSquareOverlayColor(squareLocation, new Color());
            }
        }

        private void playingBoard_MoveCommit(object sender, MoveCommitEventArgs e)
        {
            resetMoveStartSquareHighlight(e);

            // Move piece from source to destination.
            if (e.Start.X != e.Target.X || e.Start.Y != e.Target.Y)
            {
                Chess.Color promoteColor;
                if (isPromoting(e.Target, out promoteColor))
                {
                    PlayingBoard.SetForegroundImage(e.Target.X, e.Target.Y, PieceImages[getPromoteToPiece(hoverQuadrant, promoteColor)]);
                }
                else
                {
                    PlayingBoard.SetForegroundImage(e.Target.X, e.Target.Y, PlayingBoard.GetForegroundImage(e.Start.X, e.Start.Y));
                }
                PlayingBoard.SetForegroundImage(e.Start.X, e.Start.Y, null);
            }
        }

        private void playingBoard_MoveCancel(object sender, MoveEventArgs e)
        {
            resetMoveStartSquareHighlight(e);
        }

        private void playingBoard_Paint(object sender, PaintEventArgs e)
        {
            var hoverSquare = PlayingBoard.HoverSquare;

            Chess.Color promoteColor;
            if (isPromoting(hoverSquare, out promoteColor))
            {
                int squareSize = PlayingBoard.SquareSize;
                if (squareSize >= 2)
                {
                    Rectangle rect = PlayingBoard.GetSquareRectangle(hoverSquare.X, hoverSquare.Y);
                    int halfSquareSize = (squareSize + 1) / 2;
                    int otherHalfSquareSize = squareSize - halfSquareSize;

                    e.Graphics.DrawImage(PieceImages[getPromoteToPiece(SquareQuadrant.TopLeft, promoteColor)],
                                         rect.X, rect.Y, halfSquareSize, halfSquareSize);
                    e.Graphics.DrawImage(PieceImages[getPromoteToPiece(SquareQuadrant.TopRight, promoteColor)],
                                         rect.X + halfSquareSize, rect.Y, otherHalfSquareSize, halfSquareSize);
                    e.Graphics.DrawImage(PieceImages[getPromoteToPiece(SquareQuadrant.BottomLeft, promoteColor)],
                                         rect.X, rect.Y + halfSquareSize, halfSquareSize, otherHalfSquareSize);
                    e.Graphics.DrawImage(PieceImages[getPromoteToPiece(SquareQuadrant.BottomRight, promoteColor)],
                                         rect.X + halfSquareSize, rect.Y + halfSquareSize, otherHalfSquareSize, otherHalfSquareSize);
                }
            }
        }

        private void clearBoard()
        {
            foreach (var squareLocation in PlayingBoard.AllSquareLocations)
            {
                PlayingBoard.SetForegroundImage(squareLocation, null);
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

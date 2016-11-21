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
        private EnumIndexedArray<Chess.NonEmptyColoredPiece, Image> pieceImages;

        /// <summary>
        /// Gets or sets the image set to use for the playing board.
        /// </summary>
        public EnumIndexedArray<Chess.NonEmptyColoredPiece, Image> PieceImages
        {
            get
            {
                return pieceImages;
            }
            set
            {
                pieceImages = value;
                copyPositionToBoard();
            }
        }

        private Chess.Position currentPosition;

        private Chess.Square toSquare(SquareLocation squareLocation)
        {
            // Reverse y-index, because square A1 is at y == 7.
            return ((Chess.File)squareLocation.X).Combine((Chess.Rank)7 - squareLocation.Y);
        }

        private void copyPositionToBoard()
        {
            // Copy all pieces.
            foreach (Chess.NonEmptyColoredPiece coloredPiece in EnumHelper<Chess.NonEmptyColoredPiece>.AllValues)
            {
                foreach (var square in currentPosition.GetVector(coloredPiece).AllSquares())
                {
                    PlayingBoard.SetForegroundImage(square.X(), 7 - square.Y(), PieceImages[coloredPiece]);
                }
            }

            // Clear all squares that are empty.
            foreach (var square in currentPosition.GetVector(Chess.ColoredPiece.Empty).AllSquares())
            {
                PlayingBoard.SetForegroundImage(square.X(), 7 - square.Y(), null);
            }
        }

        private bool canPieceBeMoved(SquareLocation squareLocation)
        {
            // Check if location is a member of all squares where a piece sits of the current colour.
            ulong allowed = currentPosition.GetVector(currentPosition.SideToMove);
            return (allowed & toSquare(squareLocation).ToVector()) != 0;
        }

        public StandardChessBoardForm()
        {
            currentPosition = Chess.Position.GetInitialPosition();

            PlayingBoard.BoardWidth = Chess.Constants.SquareCount;
            PlayingBoard.BoardHeight = Chess.Constants.SquareCount;

            PlayingBoard.MouseMove += playingBoard_MouseMove;
            PlayingBoard.MouseEnterSquare += playingBoard_MouseEnterSquare;
            PlayingBoard.MouseLeaveSquare += playingBoard_MouseLeaveSquare;

            PlayingBoard.MoveStart += playingBoard_MoveStart;
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

        private bool isPromoting(SquareLocation location)
        {
            if (PlayingBoard.IsMoving && location != null)
            {
                // Create fake illegal promotion move to check what TryMakeMove() returns.
                Chess.Move move = new Chess.Move()
                {
                    SourceSquare = toSquare(PlayingBoard.MoveStartSquare),
                    TargetSquare = toSquare(location),
                };
                return currentPosition.TryMakeMove(move, false).HasFlag(Chess.MoveCheckResult.IllegalPromotion);
            }
            return false;
        }

        private void playingBoard_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPromoting(PlayingBoard.HoverSquare))
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
                updateHoverQuadrant(hitQuadrant, currentPosition.SideToMove);
            }
        }

        private void setMoveStartHighlight(SquareLocation squareLocation)
        {
            if (!PlayingBoard.IsMoving && canPieceBeMoved(squareLocation))
            {
                PlayingBoard.SetIsImageHighLighted(squareLocation.X, squareLocation.Y, true);
            }
        }

        private void playingBoard_MouseEnterSquare(object sender, SquareEventArgs e)
        {
            setMoveStartHighlight(e.Location);
        }

        private void playingBoard_MouseLeaveSquare(object sender, SquareEventArgs e)
        {
            updateHoverQuadrant(SquareQuadrant.Indeterminate, default(Chess.Color));
            if (!PlayingBoard.IsMoving)
            {
                PlayingBoard.SetIsImageHighLighted(e.Location.X, e.Location.Y, false);
            }
        }

        private void playingBoard_MoveStart(object sender, CancellableMoveEventArgs e)
        {
            if (canPieceBeMoved(e.Start))
            {
                // Move is allowed, now enumerate possible target squares and ask currentPosition if that's possible.
                Chess.Move move = new Chess.Move()
                {
                    SourceSquare = toSquare(e.Start),
                };

                foreach (var square in EnumHelper<Chess.Square>.AllValues)
                {
                    move.TargetSquare = square;
                    var moveCheckResult = currentPosition.TryMakeMove(move, false);
                    if (moveCheckResult == Chess.MoveCheckResult.OK || moveCheckResult == Chess.MoveCheckResult.IllegalPromotion)
                    {
                        // Highlight each found square.
                        PlayingBoard.SetSquareOverlayColor(square.X(), 7 - square.Y(), Color.FromArgb(48, 240, 90, 90));
                    }
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void resetMoveStartSquareHighlight(MoveEventArgs e)
        {
            var hoverSquare = PlayingBoard.HoverSquare;
            if (hoverSquare == null || hoverSquare.X != e.Start.X || hoverSquare.Y != e.Start.Y)
            {
                PlayingBoard.SetIsImageHighLighted(e.Start.X, e.Start.Y, false);
            }
            foreach (var squareLocation in PlayingBoard.AllSquareLocations)
            {
                PlayingBoard.SetSquareOverlayColor(squareLocation, new Color());
            }
        }

        private void playingBoard_MoveCommit(object sender, MoveCommitEventArgs e)
        {
            // Move piece from source to destination.
            Chess.Move move = new Chess.Move()
            {
                SourceSquare = toSquare(e.Start),
                TargetSquare = toSquare(e.Target),
            };

            if (isPromoting(e.Target))
            {
                move.MoveType = Chess.MoveType.Promotion;
                move.PromoteTo = getPromoteToPiece(hoverQuadrant, currentPosition.SideToMove).GetPiece();
            }

            if (currentPosition.TryMakeMove(move, true) == Chess.MoveCheckResult.OK)
            {
                copyPositionToBoard();
            }

            resetMoveStartSquareHighlight(e);
        }

        private void playingBoard_MoveCancel(object sender, MoveEventArgs e)
        {
            resetMoveStartSquareHighlight(e);
        }

        private void playingBoard_Paint(object sender, PaintEventArgs e)
        {
            var hoverSquare = PlayingBoard.HoverSquare;

            // Draw subtle corners just inside the edges of a legal target square.
            if (hoverSquare != null && PlayingBoard.IsMoving && !PlayingBoard.GetSquareOverlayColor(hoverSquare).IsEmpty)
            {
                Rectangle hoverRect = PlayingBoard.GetSquareRectangle(hoverSquare);
                e.Graphics.ExcludeClip(Rectangle.Inflate(hoverRect, -10, 0));
                e.Graphics.ExcludeClip(Rectangle.Inflate(hoverRect, 0, -10));
                e.Graphics.DrawRectangle(Pens.Gray, new Rectangle(hoverRect.X, hoverRect.Y, hoverRect.Width - 1, hoverRect.Height - 1));
                e.Graphics.ResetClip();
            }

            if (isPromoting(hoverSquare))
            {
                int squareSize = PlayingBoard.SquareSize;
                if (squareSize >= 2)
                {
                    Rectangle rect = PlayingBoard.GetSquareRectangle(hoverSquare.X, hoverSquare.Y);
                    int halfSquareSize = (squareSize + 1) / 2;
                    int otherHalfSquareSize = squareSize - halfSquareSize;

                    Chess.Color promoteColor = currentPosition.SideToMove;
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
    }
}

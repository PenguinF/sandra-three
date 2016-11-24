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
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Form which contains a chess board on which a standard game of chess is played.
    /// </summary>
    public class StandardChessBoardForm : PlayingBoardForm
    {
        private EnumIndexedArray<Chess.ColoredPiece, Image> pieceImages;

        /// <summary>
        /// Gets or sets the image set to use for the playing board.
        /// </summary>
        public EnumIndexedArray<Chess.ColoredPiece, Image> PieceImages
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

        private SquareLocation toSquareLocation(Chess.Square square)
        {
            return new SquareLocation(square.X(), 7 - square.Y());
        }

        private void copyPositionToBoard()
        {
            // Copy all pieces.
            foreach (Chess.ColoredPiece coloredPiece in EnumHelper<Chess.ColoredPiece>.AllValues)
            {
                foreach (var square in currentPosition.GetVector(coloredPiece).AllSquares())
                {
                    PlayingBoard.SetForegroundImage(toSquareLocation(square), PieceImages[coloredPiece]);
                }
            }

            // Clear all squares that are empty.
            foreach (var square in currentPosition.GetEmptyVector().AllSquares())
            {
                PlayingBoard.SetForegroundImage(toSquareLocation(square), null);
            }
        }

        private bool canPieceBeMoved(SquareLocation squareLocation)
        {
            // Check if location is a member of all squares where a piece sits of the current colour.
            ulong allowed = currentPosition.GetVector(currentPosition.SideToMove);
            return allowed.Test(toSquare(squareLocation).ToVector());
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

            dotPen = new Pen(Color.DimGray)
            {
                DashStyle = DashStyle.Dot,
                Width = 2,
                Alignment = PenAlignment.Center,
                StartCap = LineCap.RoundAnchor,
                EndCap = LineCap.Round,
            };
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

        private Chess.ColoredPiece getPromoteToPiece(SquareQuadrant quadrant, Chess.Color promoteColor)
        {
            switch (promoteColor)
            {
                case Chess.Color.White:
                    switch (quadrant)
                    {
                        case SquareQuadrant.TopLeft:
                            return Chess.ColoredPiece.WhiteRook;
                        case SquareQuadrant.TopRight:
                            return Chess.ColoredPiece.WhiteBishop;
                        case SquareQuadrant.BottomRight:
                            return Chess.ColoredPiece.WhiteKnight;
                        case SquareQuadrant.BottomLeft:
                        case SquareQuadrant.Indeterminate:
                        default:
                            return Chess.ColoredPiece.WhiteQueen;
                    }
                case Chess.Color.Black:
                default:
                    switch (quadrant)
                    {
                        case SquareQuadrant.BottomLeft:
                            return Chess.ColoredPiece.BlackRook;
                        case SquareQuadrant.BottomRight:
                            return Chess.ColoredPiece.BlackBishop;
                        case SquareQuadrant.TopRight:
                            return Chess.ColoredPiece.BlackKnight;
                        case SquareQuadrant.TopLeft:
                        case SquareQuadrant.Indeterminate:
                        default:
                            return Chess.ColoredPiece.BlackQueen;
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

        private SquareLocation currentSquareWithPromoteEffect;

        private void displayPromoteEffect(Chess.Square promoteSquare)
        {
            currentSquareWithPromoteEffect = toSquareLocation(promoteSquare);
        }

        private void stopDisplayPromoteEffect()
        {
            if (currentSquareWithPromoteEffect != null)
            {
                updateHoverQuadrant(SquareQuadrant.Indeterminate, default(Chess.Color));
                currentSquareWithPromoteEffect = null;
            }
        }

        private SquareLocation currentSquareWithEnPassantEffect;

        private void displayEnPassantEffect(Chess.Square enPassantCaptureSquare)
        {
            currentSquareWithEnPassantEffect = toSquareLocation(enPassantCaptureSquare);
            PlayingBoard.SetForegroundImageAttribute(currentSquareWithEnPassantEffect, ForegroundImageAttribute.HalfTransparent);
        }

        private void stopDisplayEnPassantEffect()
        {
            if (currentSquareWithEnPassantEffect != null)
            {
                PlayingBoard.SetForegroundImageAttribute(currentSquareWithEnPassantEffect, ForegroundImageAttribute.Default);
                currentSquareWithEnPassantEffect = null;
            }
        }

        private SquareLocation rookSquareWithCastlingEffect;
        private SquareLocation rookTargetSquareWithCastlingEffect;

        private void displayCastlingEffect(Chess.Square rookSquare, Chess.Square rookTargetSquare)
        {
            rookSquareWithCastlingEffect = toSquareLocation(rookSquare);
            rookTargetSquareWithCastlingEffect = toSquareLocation(rookTargetSquare);
            PlayingBoard.SetForegroundImageAttribute(rookSquareWithCastlingEffect, ForegroundImageAttribute.HalfTransparent);
            PlayingBoard.SetForegroundImage(rookTargetSquareWithCastlingEffect,
                                            PlayingBoard.GetForegroundImage(rookSquareWithCastlingEffect));
        }

        private void stopDisplayCastlingEffect()
        {
            if (rookSquareWithCastlingEffect != null)
            {
                PlayingBoard.SetForegroundImageAttribute(rookSquareWithCastlingEffect, ForegroundImageAttribute.Default);
                PlayingBoard.SetForegroundImage(rookTargetSquareWithCastlingEffect, null);
                rookSquareWithCastlingEffect = null;
                rookTargetSquareWithCastlingEffect = null;
            }
        }

        private void playingBoard_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentSquareWithPromoteEffect != null)
            {
                Rectangle hoverSquareRectangle = PlayingBoard.GetSquareRectangle(currentSquareWithPromoteEffect);
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

        private void highlightHoverSquare()
        {
            var hoverSquare = PlayingBoard.HoverSquare;
            if (hoverSquare != null && canPieceBeMoved(hoverSquare))
            {
                PlayingBoard.SetForegroundImageAttribute(hoverSquare, ForegroundImageAttribute.Highlight);
            }
        }

        private void playingBoard_MouseEnterSquare(object sender, SquareEventArgs e)
        {
            if (PlayingBoard.IsMoving)
            {
                // If a legal move, display any piece about to be captured with a half-transparent effect.
                // Also display additional effects for special moves, detectable by the returned MoveCheckResult.
                Chess.Move move = new Chess.Move()
                {
                    SourceSquare = toSquare(PlayingBoard.MoveStartSquare),
                    TargetSquare = toSquare(e.Location),
                };

                var moveCheckResult = currentPosition.TryMakeMove(move, false);
                if (moveCheckResult.IsLegalMove())
                {
                    if (moveCheckResult == Chess.MoveCheckResult.MissingEnPassant)
                    {
                        displayEnPassantEffect(currentPosition.EnPassantCaptureVector.GetSingleSquare());
                    }
                    else if (moveCheckResult == Chess.MoveCheckResult.MissingCastleQueenside)
                    {
                        displayCastlingEffect(move.SourceSquare - 4, move.SourceSquare - 1);
                    }
                    else if (moveCheckResult == Chess.MoveCheckResult.MissingCastleKingside)
                    {
                        displayCastlingEffect(move.SourceSquare + 3, move.SourceSquare + 1);
                    }
                    else
                    {
                        if (moveCheckResult == Chess.MoveCheckResult.MissingPromotionInformation)
                        {
                            displayPromoteEffect(move.TargetSquare);
                        }
                        PlayingBoard.SetForegroundImageAttribute(e.Location, ForegroundImageAttribute.HalfTransparent);
                    }
                }
            }
            else
            {
                highlightHoverSquare();
            }
        }

        private void playingBoard_MouseLeaveSquare(object sender, SquareEventArgs e)
        {
            stopDisplayPromoteEffect();
            stopDisplayEnPassantEffect();
            stopDisplayCastlingEffect();
            if (!PlayingBoard.IsMoving || e.Location != PlayingBoard.MoveStartSquare)
            {
                PlayingBoard.SetForegroundImageAttribute(e.Location, ForegroundImageAttribute.Default);
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
                    if (moveCheckResult.IsLegalMove())
                    {
                        // Highlight each found square.
                        PlayingBoard.SetSquareOverlayColor(toSquareLocation(square), Color.FromArgb(48, 240, 90, 90));
                    }
                }

                PlayingBoard.SetForegroundImageAttribute(e.Start, ForegroundImageAttribute.HalfTransparent);
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void resetMoveEffects(MoveEventArgs e)
        {
            stopDisplayPromoteEffect();
            stopDisplayEnPassantEffect();
            stopDisplayCastlingEffect();
            foreach (var squareLocation in PlayingBoard.AllSquareLocations)
            {
                PlayingBoard.SetForegroundImageAttribute(squareLocation, ForegroundImageAttribute.Default);
                PlayingBoard.SetSquareOverlayColor(squareLocation, new Color());
            }
            highlightHoverSquare();
        }

        private void playingBoard_MoveCommit(object sender, MoveCommitEventArgs e)
        {
            // Move piece from source to destination.
            Chess.Move move = new Chess.Move()
            {
                SourceSquare = toSquare(e.Start),
                TargetSquare = toSquare(e.Target),
            };

            if (currentSquareWithEnPassantEffect != null)
            {
                // Must specify this MoveType to commit it.
                move.MoveType = Chess.MoveType.EnPassant;
            }
            else if (rookSquareWithCastlingEffect != null)
            {
                if (rookTargetSquareWithCastlingEffect.X > rookSquareWithCastlingEffect.X)
                {
                    // Rook moves to the right.
                    move.MoveType = Chess.MoveType.CastleQueenside;
                }
                else
                {
                    move.MoveType = Chess.MoveType.CastleKingside;
                }
            }
            else if (currentSquareWithPromoteEffect != null)
            {
                move.MoveType = Chess.MoveType.Promotion;
                move.PromoteTo = getPromoteToPiece(hoverQuadrant, currentPosition.SideToMove).GetPiece();
            }

            resetMoveEffects(e);

            if (currentPosition.TryMakeMove(move, true) == Chess.MoveCheckResult.OK)
            {
                copyPositionToBoard();
                lastCommittedMove = e;
            }
        }

        private void playingBoard_MoveCancel(object sender, MoveEventArgs e)
        {
            resetMoveEffects(e);
        }

        MoveCommitEventArgs lastCommittedMove;

        Pen dotPen;

        private void playingBoard_Paint(object sender, PaintEventArgs e)
        {
            var hoverSquare = PlayingBoard.HoverSquare;

            // Draw a dotted line between the centers of the squares of the last move.
            if (lastCommittedMove != null)
            {
                Rectangle startSquareRect = PlayingBoard.GetSquareRectangle(lastCommittedMove.Start);
                int startSquareCenterX = startSquareRect.X + startSquareRect.Width / 2;
                int startSquareCenterY = startSquareRect.Y + startSquareRect.Height / 2;
                Rectangle targetSquareRect = PlayingBoard.GetSquareRectangle(lastCommittedMove.Target);
                int targetSquareCenterX = targetSquareRect.X + targetSquareRect.Width / 2;
                int targetSquareCenterY = targetSquareRect.Y + targetSquareRect.Height / 2;

                // Cut off the last segment over the target square.
                int distance = Math.Max(
                    Math.Abs(lastCommittedMove.Start.X - lastCommittedMove.Target.X),
                    Math.Abs(lastCommittedMove.Start.Y - lastCommittedMove.Target.Y));

                int endPointX = targetSquareCenterX - (targetSquareCenterX - startSquareCenterX) / distance * 3 / 8;
                int endPointY = targetSquareCenterY - (targetSquareCenterY - startSquareCenterY) / distance * 3 / 8;

                e.Graphics.DrawLine(dotPen,
                                    startSquareCenterX, startSquareCenterY,
                                    endPointX, endPointY);

                e.Graphics.FillEllipse(Brushes.DimGray, new RectangleF(endPointX - 3.5f, endPointY - 3.5f, 7, 7));
            }

            // Draw subtle corners just inside the edges of a legal target square.
            if (hoverSquare != null && PlayingBoard.IsMoving && !PlayingBoard.GetSquareOverlayColor(hoverSquare).IsEmpty)
            {
                Rectangle hoverRect = PlayingBoard.GetSquareRectangle(hoverSquare);
                e.Graphics.ExcludeClip(Rectangle.Inflate(hoverRect, -10, 0));
                e.Graphics.ExcludeClip(Rectangle.Inflate(hoverRect, 0, -10));
                e.Graphics.DrawRectangle(Pens.Gray, new Rectangle(hoverRect.X, hoverRect.Y, hoverRect.Width - 1, hoverRect.Height - 1));
                e.Graphics.ResetClip();
            }

            if (currentSquareWithPromoteEffect != null)
            {
                int squareSize = PlayingBoard.SquareSize;
                if (squareSize >= 2)
                {
                    Rectangle rect = PlayingBoard.GetSquareRectangle(currentSquareWithPromoteEffect);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dotPen.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

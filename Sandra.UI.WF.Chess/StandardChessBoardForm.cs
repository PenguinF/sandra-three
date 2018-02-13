﻿/*********************************************************************************
 * StandardChessBoardForm.cs
 * 
 * Copyright (c) 2004-2018 Henk Nicolai
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
    /// Maintains its aspect ratio while resizing.
    /// </summary>
    public partial class StandardChessBoardForm : SnappingMdiChildForm
    {
        /// <summary>
        /// Gets a reference to the playing board control on this form.
        /// </summary>
        public PlayingBoard PlayingBoard { get; }

        private EnumIndexedArray<Chess.ColoredPiece, Image> pieceImages = EnumIndexedArray<Chess.ColoredPiece, Image>.New();

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

        private InteractiveGame game;

        public InteractiveGame Game
        {
            get { return game; }
            set
            {
                if (game != value)
                {
                    game = value;
                    copyPositionToBoard();
                }
            }
        }

        private bool isBoardFlipped;

        /// <summary>
        /// Gets or sets if the board is flipped.
        /// </summary>
        public bool IsBoardFlipped
        {
            get { return isBoardFlipped; }
            set
            {
                if (isBoardFlipped != value)
                {
                    isBoardFlipped = value;
                    copyPositionToBoard();
                }
            }
        }

        private Chess.Square toSquare(SquareLocation squareLocation)
        {
            return isBoardFlipped
                 ? (7 - (Chess.File)squareLocation.X).Combine((Chess.Rank)squareLocation.Y)
                 : ((Chess.File)squareLocation.X).Combine((Chess.Rank)7 - squareLocation.Y);
        }

        private SquareLocation toSquareLocation(Chess.Square square)
        {
            return isBoardFlipped
                 ? new SquareLocation(7 - square.X(), square.Y())
                 : new SquareLocation(square.X(), 7 - square.Y());
        }

        private void copyPositionToBoard()
        {
            if (game != null)
            {
                // Copy all pieces and clear all squares that are empty.
                foreach (var square in EnumHelper<Chess.Square>.AllValues)
                {
                    Chess.ColoredPiece? coloredPiece = game.Game.GetColoredPiece(square);
                    if (coloredPiece == null)
                    {
                        PlayingBoard.SetForegroundImage(toSquareLocation(square), null);
                    }
                    else
                    {
                        PlayingBoard.SetForegroundImage(toSquareLocation(square), PieceImages[(Chess.ColoredPiece)coloredPiece]);
                    }
                }
            }
            else
            {
                // Just clear all squares.
                foreach (var squareLocation in PlayingBoard.AllSquareLocations)
                {
                    PlayingBoard.SetForegroundImage(squareLocation, null);
                }
            }
        }

        public StandardChessBoardForm()
        {
            PlayingBoard = new PlayingBoard
            {
                Location = new Point(0, 0),
                Dock = DockStyle.Fill,
                BoardWidth = Chess.Constants.SquareCount,
                BoardHeight = Chess.Constants.SquareCount,
            };

            PlayingBoard.MouseMove += playingBoard_MouseMove;
            PlayingBoard.MouseEnterSquare += playingBoard_MouseEnterSquare;
            PlayingBoard.MouseLeaveSquare += playingBoard_MouseLeaveSquare;
            PlayingBoard.MouseWheel += playingBoard_MouseWheel;

            PlayingBoard.MoveStart += playingBoard_MoveStart;
            PlayingBoard.MoveCancel += playingBoard_MoveCancel;
            PlayingBoard.MoveCommit += playingBoard_MoveCommit;

            PlayingBoard.Paint += playingBoard_Paint;

            Controls.Add(PlayingBoard);

            ShowIcon = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.SizableToolWindow;

            lastMoveArrowPen = new Pen(Color.DimGray)
            {
                DashStyle = DashStyle.Dot,
                Width = 2,
                Alignment = PenAlignment.Center,
                StartCap = LineCap.Round,
                EndCap = LineCap.RoundAnchor,
            };
        }

        private void playingBoard_MouseWheel(object sender, MouseEventArgs e)
        {
            if (game != null)
            {
                game.HandleMouseWheelEvent(e.Delta);
            }
        }

        private SquareLocation moveStartSquare;

        private bool canPieceBeMoved(SquareLocation squareLocation)
        {
            if (game == null) return false;

            // Check if location is a member of all squares where a piece sits of the current color.
            Chess.Square square = toSquare(squareLocation);
            Chess.ColoredPiece? coloredPiece = game.Game.GetColoredPiece(square);
            if (coloredPiece != null)
            {
                return ((Chess.ColoredPiece)coloredPiece).GetColor() == game.Game.SideToMove;
            }
            return false;
        }

        /// <summary>
        /// Gets the position of the mouse relative to the top left corner of the playing board when dragging started.
        /// </summary>
        private Point dragStartPosition;

        private Cursor dragCursor;

        private void updateDragImage(Image newImage, SquareLocation startSquare, Point dragStartPosition)
        {
            Cursor newDragCursor = null;
            if (newImage != null && startSquare != null)
            {
                Rectangle imageRectangle = PlayingBoard.GetRelativeForegroundImageRectangle();
                if (imageRectangle.Width > 0 && imageRectangle.Height > 0)
                {
                    Point dragStartSquareLocation = PlayingBoard.GetSquareRectangle(startSquare).Location;
                    int hotSpotX = dragStartPosition.X - dragStartSquareLocation.X - imageRectangle.X;
                    int hotSpotY = dragStartPosition.Y - dragStartSquareLocation.Y - imageRectangle.Y;

                    newDragCursor = DragUtils.CreateDragCursorFromImage(newImage,
                                                                        imageRectangle.Size,
                                                                        Cursors.Default,
                                                                        new Point(hotSpotX, hotSpotY));
                }
            }

            Cursor oldDragCursor = dragCursor;

            if (newDragCursor != null || oldDragCursor != null)
            {
                dragCursor = newDragCursor;
                Cursor.Current = dragCursor ?? Cursors.Default;
            }

            if (oldDragCursor != null) oldDragCursor.Dispose();
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
            if (isBoardFlipped)
            {
                switch (promoteColor)
                {
                    case Chess.Color.White:
                        switch (quadrant)
                        {
                            case SquareQuadrant.BottomLeft:
                                return Chess.ColoredPiece.WhiteRook;
                            case SquareQuadrant.BottomRight:
                                return Chess.ColoredPiece.WhiteBishop;
                            case SquareQuadrant.TopRight:
                                return Chess.ColoredPiece.WhiteKnight;
                            case SquareQuadrant.TopLeft:
                            case SquareQuadrant.Indeterminate:
                            default:
                                return Chess.ColoredPiece.WhiteQueen;
                        }
                    case Chess.Color.Black:
                    default:
                        switch (quadrant)
                        {
                            case SquareQuadrant.TopLeft:
                                return Chess.ColoredPiece.BlackRook;
                            case SquareQuadrant.TopRight:
                                return Chess.ColoredPiece.BlackBishop;
                            case SquareQuadrant.BottomRight:
                                return Chess.ColoredPiece.BlackKnight;
                            case SquareQuadrant.BottomLeft:
                            case SquareQuadrant.Indeterminate:
                            default:
                                return Chess.ColoredPiece.BlackQueen;
                        }
                }
            }
            else
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
        }

        private SquareQuadrant hoverQuadrant;

        private void updateHoverQuadrant(SquareQuadrant value, Chess.Color promoteColor)
        {
            if (hoverQuadrant != value)
            {
                hoverQuadrant = value;
                if (value == SquareQuadrant.Indeterminate)
                {
                    updateDragImage(moveStartSquare != null ? PlayingBoard.GetForegroundImage(moveStartSquare) : null,
                                    moveStartSquare,
                                    dragStartPosition);
                }
                else
                {
                    updateDragImage(PieceImages[getPromoteToPiece(value, promoteColor)],
                                    moveStartSquare,
                                    dragStartPosition);
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
                updateHoverQuadrant(hitQuadrant, game.Game.SideToMove);
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

        private void playingBoard_MouseEnterSquare(PlayingBoard sender, SquareEventArgs e)
        {
            if (moveStartSquare != null)
            {
                // If a legal move, display any piece about to be captured with a half-transparent effect.
                // Also display additional effects for special moves, detectable by the returned MoveCheckResult.
                Chess.MoveInfo moveInfo = new Chess.MoveInfo()
                {
                    SourceSquare = toSquare(moveStartSquare),
                    TargetSquare = toSquare(e.Location),
                };

                game.Game.TryMakeMove(ref moveInfo, false);

                var moveCheckResult = moveInfo.Result;
                if (moveCheckResult.IsLegalMove())
                {
                    if (moveCheckResult == Chess.MoveCheckResult.MissingEnPassant)
                    {
                        displayEnPassantEffect(game.Game.EnPassantCaptureSquare);
                    }
                    else if (moveCheckResult == Chess.MoveCheckResult.MissingCastleQueenside)
                    {
                        displayCastlingEffect(moveInfo.SourceSquare - 4, moveInfo.SourceSquare - 1);
                    }
                    else if (moveCheckResult == Chess.MoveCheckResult.MissingCastleKingside)
                    {
                        displayCastlingEffect(moveInfo.SourceSquare + 3, moveInfo.SourceSquare + 1);
                    }
                    else
                    {
                        if (moveCheckResult == Chess.MoveCheckResult.MissingPromotionInformation)
                        {
                            displayPromoteEffect(moveInfo.TargetSquare);
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

        private void playingBoard_MouseLeaveSquare(PlayingBoard sender, SquareEventArgs e)
        {
            stopDisplayPromoteEffect();
            stopDisplayEnPassantEffect();
            stopDisplayCastlingEffect();
            if (moveStartSquare == null || e.Location != moveStartSquare)
            {
                PlayingBoard.SetForegroundImageAttribute(e.Location, ForegroundImageAttribute.Default);
            }
        }

        private void playingBoard_MoveStart(PlayingBoard sender, CancellableMoveEventArgs e)
        {
            if (canPieceBeMoved(e.Start))
            {
                // Move is allowed, now enumerate possible target squares and ask currentPosition if that's possible.
                Chess.MoveInfo moveInfo = new Chess.MoveInfo()
                {
                    SourceSquare = toSquare(e.Start),
                };

                foreach (var square in EnumHelper<Chess.Square>.AllValues)
                {
                    moveInfo.TargetSquare = square;
                    game.Game.TryMakeMove(ref moveInfo, false);
                    var moveCheckResult = moveInfo.Result;
                    if (moveCheckResult.IsLegalMove())
                    {
                        // Highlight each found square.
                        PlayingBoard.SetSquareOverlayColor(toSquareLocation(square), Color.FromArgb(48, 240, 90, 90));
                    }
                }

                PlayingBoard.SetForegroundImageAttribute(e.Start, ForegroundImageAttribute.HalfTransparent);
                updateDragImage(PlayingBoard.GetForegroundImage(e.Start), e.Start, e.MouseStartPosition);

                moveStartSquare = e.Start;
                dragStartPosition = e.MouseStartPosition;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void resetMoveEffects(MoveEventArgs e)
        {
            updateDragImage(null, null, Point.Empty);
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

        private void playingBoard_MoveCommit(PlayingBoard sender, MoveCommitEventArgs e)
        {
            if (moveStartSquare != null)
            {
                // Move piece from source to destination.
                Chess.MoveInfo moveInfo = new Chess.MoveInfo()
                {
                    SourceSquare = toSquare(moveStartSquare),
                    TargetSquare = toSquare(e.Target),
                };

                if (currentSquareWithEnPassantEffect != null)
                {
                    // Must specify this MoveType to commit it.
                    moveInfo.MoveType = Chess.MoveType.EnPassant;
                }
                else if (rookSquareWithCastlingEffect != null)
                {
                    if (rookTargetSquareWithCastlingEffect.X > rookSquareWithCastlingEffect.X)
                    {
                        // Rook moves to the right.
                        moveInfo.MoveType = Chess.MoveType.CastleQueenside;
                    }
                    else
                    {
                        moveInfo.MoveType = Chess.MoveType.CastleKingside;
                    }
                }
                else if (currentSquareWithPromoteEffect != null)
                {
                    moveInfo.MoveType = Chess.MoveType.Promotion;
                    moveInfo.PromoteTo = getPromoteToPiece(hoverQuadrant, game.Game.SideToMove).GetPiece();
                }

                resetMoveEffects(e);

                game.Game.TryMakeMove(ref moveInfo, true);

                game.ActiveMoveTreeUpdated();
                PlayingBoard.ActionHandler.Invalidate();

                moveStartSquare = null;
            }
        }

        internal void GameUpdated()
        {
            copyPositionToBoard();
        }

        private void playingBoard_MoveCancel(PlayingBoard sender, MoveEventArgs e)
        {
            if (moveStartSquare != null)
            {
                resetMoveEffects(e);
                moveStartSquare = null;
            }
        }

        Pen lastMoveArrowPen;

        private void drawLastMoveArrow(Graphics g, SquareLocation start, SquareLocation target)
        {
            Rectangle startSquareRect = PlayingBoard.GetSquareRectangle(start);
            int startSquareCenterX = startSquareRect.X + startSquareRect.Width / 2;
            int startSquareCenterY = startSquareRect.Y + startSquareRect.Height / 2;

            Rectangle targetSquareRect = PlayingBoard.GetSquareRectangle(target);
            int targetSquareCenterX = targetSquareRect.X + targetSquareRect.Width / 2;
            int targetSquareCenterY = targetSquareRect.Y + targetSquareRect.Height / 2;

            int deltaX = start.X - target.X;
            int deltaY = start.Y - target.Y;

            // Cut off the last segment over the target square.
            int distance = Math.Max(Math.Abs(deltaX), Math.Abs(deltaY));

            int endPointX = targetSquareCenterX - (targetSquareCenterX - startSquareCenterX) / distance * 3 / 8;
            int endPointY = targetSquareCenterY - (targetSquareCenterY - startSquareCenterY) / distance * 3 / 8;

            g.DrawLine(lastMoveArrowPen,
                       endPointX, endPointY,
                       startSquareCenterX, startSquareCenterY);

            // Draw two lines from the end point at a 30 degrees angle to make an arrow.
            double phi = Math.Atan2(deltaY, deltaX);

            double arrow1Phi = phi - Math.PI / 6;
            double arrow2Phi = phi + Math.PI / 6;

            double targetLength = PlayingBoard.SquareSize / 4f;
            double arrow1EndX = endPointX + Math.Cos(arrow1Phi) * targetLength;
            double arrow1EndY = endPointY + Math.Sin(arrow1Phi) * targetLength;
            double arrow2EndX = endPointX + Math.Cos(arrow2Phi) * targetLength;
            double arrow2EndY = endPointY + Math.Sin(arrow2Phi) * targetLength;

            lastMoveArrowPen.DashStyle = DashStyle.Solid;
            lastMoveArrowPen.EndCap = LineCap.Round;
            g.DrawLine(lastMoveArrowPen,
                       endPointX, endPointY,
                       (float)arrow1EndX, (float)arrow1EndY);
            g.DrawLine(lastMoveArrowPen,
                       endPointX, endPointY,
                       (float)arrow2EndX, (float)arrow2EndY);
            lastMoveArrowPen.DashStyle = DashStyle.Dot;
            lastMoveArrowPen.EndCap = LineCap.RoundAnchor;
        }

        private void playingBoard_Paint(object sender, PaintEventArgs e)
        {
            // Draw a dotted line between the centers of the squares of the last move.
            if (game != null && !game.Game.IsFirstMove)
            {
                Chess.Move lastCommittedMove = game.Game.PreviousMove();
                drawLastMoveArrow(e.Graphics,
                                  toSquareLocation(lastCommittedMove.SourceSquare),
                                  toSquareLocation(lastCommittedMove.TargetSquare));
            }

            // Draw subtle corners just inside the edges of a legal target square.
            var hoverSquare = PlayingBoard.HoverSquare;

            if (hoverSquare != null && moveStartSquare != null && !PlayingBoard.GetSquareOverlayColor(hoverSquare).IsEmpty)
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

                    Chess.Color promoteColor = game.Game.SideToMove;
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

        /// <summary>
        /// Manually updates the size of the form to a value which it would have snapped to had it been resized by WM_SIZING messages.
        /// </summary>
        public void PerformAutoFit()
        {
            startResize();

            // Simulate OnResizing event.
            // No need to create a RECT with coordinates relative to the screen, since only the size may be affected.
            RECT windowRect = new RECT()
            {
                Left = Left,
                Right = Right,
                Top = Top,
                Bottom = Bottom,
            };

            performAutoFit(ref windowRect, ResizeMode.BottomRight);

            SetBoundsCore(windowRect.Left, windowRect.Top,
                          windowRect.Right - windowRect.Left, windowRect.Bottom - windowRect.Top,
                          BoundsSpecified.Size);

            PlayingBoard.Size = PlayingBoard.GetClosestAutoFitSize(ClientSize);
        }

        int widthDifference;
        int heightDifference;

        protected override void OnResizeBegin(EventArgs e)
        {
            base.OnResizeBegin(e);

            startResize();
        }

        private void startResize()
        {
            // Cache difference in size between the window and the client rectangle.
            widthDifference = Bounds.Width - ClientRectangle.Width;
            heightDifference = Bounds.Height - ClientRectangle.Height;
        }

        private void performAutoFit(ref RECT resizeRect, ResizeMode resizeMode)
        {
            Size maxBounds;
            switch (resizeMode)
            {
                case ResizeMode.Left:
                case ResizeMode.Right:
                    // Unrestricted vertical growth.
                    maxBounds = new Size(resizeRect.Right - resizeRect.Left - widthDifference,
                                         int.MaxValue);
                    break;
                case ResizeMode.Top:
                case ResizeMode.Bottom:
                    // Unrestricted horizontal growth.
                    maxBounds = new Size(int.MaxValue,
                                         resizeRect.Bottom - resizeRect.Top - heightDifference);
                    break;
                default:
                    maxBounds = new Size(resizeRect.Right - resizeRect.Left - widthDifference,
                                         resizeRect.Bottom - resizeRect.Top - heightDifference);
                    break;
            }

            // Calculate closest auto fit size given the client height and width that would result from performing the given resize.
            Size targetSize = PlayingBoard.GetClosestAutoFitSize(maxBounds);

            // Left/right.
            switch (resizeMode)
            {
                case ResizeMode.Left:
                case ResizeMode.TopLeft:
                case ResizeMode.BottomLeft:
                    // Adjust left edge.
                    resizeRect.Left = resizeRect.Right - targetSize.Width - widthDifference;
                    break;
                default:
                    // Adjust right edge.
                    resizeRect.Right = resizeRect.Left + targetSize.Width + widthDifference;
                    break;
            }

            // Top/bottom.
            switch (resizeMode)
            {
                case ResizeMode.Top:
                case ResizeMode.TopLeft:
                case ResizeMode.TopRight:
                    // Adjust top edge.
                    resizeRect.Top = resizeRect.Bottom - targetSize.Height - heightDifference;
                    break;
                default:
                    // Adjust bottom edge.
                    resizeRect.Bottom = resizeRect.Top + targetSize.Height + heightDifference;
                    break;
            }
        }

        protected override void OnResizing(ref RECT resizeRect, ResizeMode resizeMode)
        {
            // Snap to auto-fit.
            performAutoFit(ref resizeRect, resizeMode);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lastMoveArrowPen.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

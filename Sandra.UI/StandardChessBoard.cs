#region License
/*********************************************************************************
 * StandardChessBoard.cs
 *
 * Copyright (c) 2004-2023 Henk Nicolai
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
**********************************************************************************/
#endregion

using Eutherion;
using Eutherion.Collections;
using Eutherion.Win.DragDrop;
using Eutherion.Win.Forms;
using Eutherion.Win.MdiAppTemplate;
using Eutherion.Win.Native;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Sandra.UI
{
    /// <summary>
    /// Contains a chess board on which a standard game of chess is played.
    /// Maintains its aspect ratio while resizing.
    /// </summary>
    public partial class StandardChessBoard : ContainerControl, IDockableControl, IWeakEventTarget
    {
        public DockProperties DockProperties { get; } = new DockProperties();

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
            get => pieceImages;
            set
            {
                pieceImages = value;
                CopyPositionToBoard();
            }
        }

        private Chess.Game game;

        public Chess.Game Game
        {
            get => game;
            set
            {
                if (game != value)
                {
                    game = value;
                    CopyPositionToBoard();
                }
            }
        }

        private bool isBoardFlipped;

        /// <summary>
        /// Gets or sets if the board is flipped.
        /// </summary>
        public bool IsBoardFlipped
        {
            get => isBoardFlipped;
            set
            {
                if (isBoardFlipped != value)
                {
                    if (moveStatus != MoveStatus.None)
                    {
                        StopDisplayLegalTargetSquaresEffect();
                        PlayingBoard.SetForegroundImageAttribute(ToSquareLocation(moveStartSquare), ForegroundImageAttribute.Default);
                    }
                    if (PlayingBoard.HoverSquare != null)
                    {
                        LeaveSquareEffects(PlayingBoard.HoverSquare);
                    }

                    isBoardFlipped = value;
                    CopyPositionToBoard();

                    if (moveStatus != MoveStatus.None)
                    {
                        DisplayLegalTargetSquaresEffect();
                        UpdateMoveStartSquareEffect();
                    }
                    if (PlayingBoard.HoverSquare != null)
                    {
                        EnterSquareEffects(PlayingBoard.HoverSquare);
                    }

                    MouseMoveEffects(PointToClient(MousePosition));
                }
            }
        }

        private Chess.Square ToSquare(SquareLocation squareLocation)
            => isBoardFlipped ? (7 - (Chess.File)squareLocation.X).Combine((Chess.Rank)squareLocation.Y)
                              : ((Chess.File)squareLocation.X).Combine((Chess.Rank)7 - squareLocation.Y);

        private SquareLocation ToSquareLocation(Chess.Square square)
            => isBoardFlipped ? new SquareLocation(7 - square.X(), square.Y())
                              : new SquareLocation(square.X(), 7 - square.Y());

        private void CopyPositionToBoard()
        {
            if (game != null)
            {
                // Copy all pieces and clear all squares that are empty.
                foreach (var square in EnumValues<Chess.Square>.List)
                {
                    Chess.ColoredPiece? coloredPiece = game.CurrentPosition.GetColoredPiece(square);
                    if (coloredPiece == null)
                    {
                        PlayingBoard.SetForegroundImage(ToSquareLocation(square), null);
                    }
                    else
                    {
                        PlayingBoard.SetForegroundImage(ToSquareLocation(square), PieceImages[(Chess.ColoredPiece)coloredPiece]);
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

        public StandardChessBoard()
        {
            PlayingBoard = new PlayingBoard
            {
                Location = new Point(0, 0),
                Dock = DockStyle.Fill,
                BoardWidth = Chess.Constants.SquareCount,
                BoardHeight = Chess.Constants.SquareCount,
                DarkSquareColor = Session.Current.GetSetting(SettingKeys.DarkSquareColor),
                LightSquareColor = Session.Current.GetSetting(SettingKeys.LightSquareColor),
            };

            Session.Current.LocalSettings.RegisterSettingsChangedHandler(SettingKeys.DarkSquareColor, DarkSquareColorChanged);
            Session.Current.LocalSettings.RegisterSettingsChangedHandler(SettingKeys.LightSquareColor, LightSquareColorChanged);
            Session.Current.LocalSettings.RegisterSettingsChangedHandler(SettingKeys.LastMoveArrowColor, LastMoveArrowColorChanged);
            Session.Current.LocalSettings.RegisterSettingsChangedHandler(SettingKeys.DisplayLegalTargetSquares, DisplayLegalTargetSquaresChanged);
            Session.Current.LocalSettings.RegisterSettingsChangedHandler(SettingKeys.LegalTargetSquaresColor, DisplayLegalTargetSquaresChanged);

            PlayingBoard.MouseMove += PlayingBoard_MouseMove;
            PlayingBoard.MouseEnterSquare += PlayingBoard_MouseEnterSquare;
            PlayingBoard.MouseLeaveSquare += PlayingBoard_MouseLeaveSquare;
            PlayingBoard.MouseWheel += PlayingBoard_MouseWheel;

            PlayingBoard.SquareMouseDown += PlayingBoard_SquareMouseDown;
            PlayingBoard.SquareMouseUp += PlayingBoard_SquareMouseUp;

            PlayingBoard.Paint += PlayingBoard_Paint;

            Controls.Add(PlayingBoard);

            UpdateLastMoveArrowPen();

            ActiveControl = PlayingBoard;
        }

        private void UpdateLastMoveArrowPen()
        {
            lastMoveArrowPen = new Pen(Session.Current.GetSetting(SettingKeys.LastMoveArrowColor))
            {
                DashStyle = DashStyle.Dot,
                Width = 2,
                Alignment = PenAlignment.Center,
                StartCap = LineCap.Round,
                EndCap = LineCap.RoundAnchor,
            };
        }

        private void DarkSquareColorChanged(object sender, EventArgs e)
        {
            PlayingBoard.DarkSquareColor = Session.Current.GetSetting(SettingKeys.DarkSquareColor);
        }

        private void LightSquareColorChanged(object sender, EventArgs e)
        {
            PlayingBoard.LightSquareColor = Session.Current.GetSetting(SettingKeys.LightSquareColor);
        }

        private void LastMoveArrowColorChanged(object sender, EventArgs e)
        {
            lastMoveArrowPen.Dispose();
            UpdateLastMoveArrowPen();
            PlayingBoard.Invalidate();
        }

        private void DisplayLegalTargetSquaresChanged(object sender, EventArgs e)
        {
            if (moveStatus != MoveStatus.None)
            {
                DisplayLegalTargetSquaresEffect();
            }
        }

        private void PlayingBoard_MouseWheel(object sender, MouseEventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                // Zoom in or out.
                PerformAutoFit(PlayingBoard.SquareSize + e.Delta / 120);
            }
            else if (game != null)
            {
                var delta = e.Delta;
                if (delta > 0)
                {
                    (delta / 120).Times(game.Backward);
                    GameUpdated();
                }
                else if (delta < 0)
                {
                    (-delta / 120).Times(game.Forward);
                    GameUpdated();
                }
            }
        }

        private enum MoveStatus
        {
            None, Moving, Dragging
        }

        private MoveStatus moveStatus;

        private Chess.Square moveStartSquare;

        private bool drawFocusMoveStartSquare;

        private bool CanPieceBeMoved(SquareLocation squareLocation)
        {
            if (game != null && squareLocation != null)
            {
                // Check if location is a member of all squares where a piece sits of the current color.
                Chess.Square square = ToSquare(squareLocation);
                Chess.ColoredPiece? coloredPiece = game.CurrentPosition.GetColoredPiece(square);
                if (coloredPiece != null)
                {
                    return ((Chess.ColoredPiece)coloredPiece).GetColor() == game.CurrentPosition.SideToMove;
                }
            }

            return false;
        }

        private void UpdateMoveStartSquareEffect()
        {
            if (moveStatus != MoveStatus.None)
            {
                SquareLocation moveStartSquareLocation = ToSquareLocation(moveStartSquare);
                if (moveStatus == MoveStatus.Dragging && dragCursor != null)
                {
                    PlayingBoard.SetForegroundImageAttribute(moveStartSquareLocation, ForegroundImageAttribute.HalfTransparent);
                }
                else
                {
                    PlayingBoard.SetForegroundImageAttribute(moveStartSquareLocation, ForegroundImageAttribute.Default);
                }
            }
        }

        /// <summary>
        /// Gets the position of the mouse relative to the top left corner of the move start square when dragging started.
        /// </summary>
        private Point dragStartPosition;

        private CursorFromHandle dragCursor;

        private void UpdateDragCursor(Image newImage, Point dragStartPosition)
        {
            CursorFromHandle newDragCursor = null;

            if (newImage != null)
            {
                Rectangle imageRectangle = PlayingBoard.GetRelativeForegroundImageRectangle();
                if (imageRectangle.Width > 0 && imageRectangle.Height > 0)
                {
                    int hotSpotX = dragStartPosition.X - imageRectangle.X;
                    int hotSpotY = dragStartPosition.Y - imageRectangle.Y;

                    try
                    {
                        newDragCursor = DragUtilities.CreateDragCursorFromImage(
                            newImage,
                            imageRectangle.Size,
                            Cursors.Default,
                            new Point(hotSpotX, hotSpotY));
                    }
                    catch (Exception exc)
                    {
                        // Creating a HIcon may fail in exceptional circumstances.
                        // Can still use the normal cursor, but do trace the exception.
                        exc.Trace();
                    }
                }
            }

            UpdateDragCursor(newDragCursor);
        }

        private void UpdateDragCursor(CursorFromHandle newDragCursor)
        {
            CursorFromHandle oldDragCursor = dragCursor;

            if (newDragCursor != null || oldDragCursor != null)
            {
                dragCursor = newDragCursor;
                Cursor.Current = dragCursor != null ? dragCursor.Cursor : Cursors.Default;
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

        private Chess.ColoredPiece GetPromoteToPiece(SquareQuadrant quadrant, Chess.Color promoteColor)
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

        private void UpdateHoverQuadrant(SquareQuadrant value, Chess.Color promoteColor)
        {
            if (hoverQuadrant != value)
            {
                hoverQuadrant = value;

                if (moveStatus == MoveStatus.Dragging)
                {
                    if (value == SquareQuadrant.Indeterminate)
                    {
                        UpdateDragCursor(PlayingBoard.GetForegroundImage(ToSquareLocation(moveStartSquare)), dragStartPosition);
                    }
                    else
                    {
                        UpdateDragCursor(PieceImages[GetPromoteToPiece(value, promoteColor)], dragStartPosition);
                    }
                }
                else
                {
                    UpdateDragCursor(null);
                }

                // Also invalidate the PlayingBoard so the promotion effect will be redrawn.
                PlayingBoard.Invalidate();
            }
        }

        private Rectangle GetSquareQuadrantRectangle(ref Rectangle squareRectangle, SquareQuadrant squareQuadrant)
        {
            int squareSize = squareRectangle.Width;
            int halfSquareSize = (squareSize + 1) / 2;
            int otherHalfSquareSize = squareSize - halfSquareSize;
            if (squareQuadrant == SquareQuadrant.TopLeft)
            {
                return new Rectangle(squareRectangle.X, squareRectangle.Y, halfSquareSize, halfSquareSize);
            }
            else if (squareQuadrant == SquareQuadrant.TopRight)
            {
                return new Rectangle(squareRectangle.X + halfSquareSize, squareRectangle.Y, otherHalfSquareSize, halfSquareSize);
            }
            else if (squareQuadrant == SquareQuadrant.BottomLeft)
            {
                return new Rectangle(squareRectangle.X, squareRectangle.Y + halfSquareSize, halfSquareSize, otherHalfSquareSize);
            }
            else  // squareQuadrant == SquareQuadrant.BottomRight
            {
                return new Rectangle(squareRectangle.X + halfSquareSize, squareRectangle.Y + halfSquareSize, otherHalfSquareSize, otherHalfSquareSize);
            }
        }

        private SquareLocation currentSquareWithCaptureEffect;

        private void DisplayCaptureEffect(Chess.Square captureSquare)
        {
            currentSquareWithCaptureEffect = ToSquareLocation(captureSquare);
            PlayingBoard.SetForegroundImageAttribute(currentSquareWithCaptureEffect, ForegroundImageAttribute.HalfTransparent);
        }

        private void StopDisplayCaptureEffect()
        {
            if (currentSquareWithCaptureEffect != null)
            {
                PlayingBoard.SetForegroundImageAttribute(currentSquareWithCaptureEffect, ForegroundImageAttribute.Default);
                currentSquareWithCaptureEffect = null;
            }
        }

        private SquareLocation currentSquareWithPromoteEffect;

        private void DisplayPromoteEffect(Chess.Square promoteSquare)
        {
            currentSquareWithPromoteEffect = ToSquareLocation(promoteSquare);
        }

        private void StopDisplayPromoteEffect()
        {
            if (currentSquareWithPromoteEffect != null)
            {
                UpdateHoverQuadrant(SquareQuadrant.Indeterminate, default);
                currentSquareWithPromoteEffect = null;
            }
        }

        private SquareLocation currentSquareWithEnPassantEffect;

        private void DisplayEnPassantEffect(Chess.Square enPassantCaptureSquare)
        {
            currentSquareWithEnPassantEffect = ToSquareLocation(enPassantCaptureSquare);
            PlayingBoard.SetForegroundImageAttribute(currentSquareWithEnPassantEffect, ForegroundImageAttribute.HalfTransparent);
        }

        private void StopDisplayEnPassantEffect()
        {
            if (currentSquareWithEnPassantEffect != null)
            {
                PlayingBoard.SetForegroundImageAttribute(currentSquareWithEnPassantEffect, ForegroundImageAttribute.Default);
                currentSquareWithEnPassantEffect = null;
            }
        }

        private SquareLocation rookSquareWithCastlingEffect;
        private SquareLocation rookTargetSquareWithCastlingEffect;

        private void DisplayCastlingEffect(Chess.Square rookSquare, Chess.Square rookTargetSquare)
        {
            rookSquareWithCastlingEffect = ToSquareLocation(rookSquare);
            rookTargetSquareWithCastlingEffect = ToSquareLocation(rookTargetSquare);
            PlayingBoard.SetForegroundImageAttribute(rookSquareWithCastlingEffect, ForegroundImageAttribute.HalfTransparent);
            PlayingBoard.SetForegroundImage(rookTargetSquareWithCastlingEffect,
                                            PlayingBoard.GetForegroundImage(rookSquareWithCastlingEffect));
        }

        private void StopDisplayCastlingEffect()
        {
            if (rookSquareWithCastlingEffect != null)
            {
                PlayingBoard.SetForegroundImageAttribute(rookSquareWithCastlingEffect, ForegroundImageAttribute.Default);
                PlayingBoard.SetForegroundImage(rookTargetSquareWithCastlingEffect, null);
                rookSquareWithCastlingEffect = null;
                rookTargetSquareWithCastlingEffect = null;
            }
        }

        private void DisplayLegalTargetSquaresEffect()
        {
            Color overlayColor = Session.Current.GetSetting(SettingKeys.DisplayLegalTargetSquares)
                ? Color.FromArgb(48, Session.Current.GetSetting(SettingKeys.LegalTargetSquaresColor))
                : Color.Empty;

            // Move is allowed, now enumerate possible target squares and ask currentPosition if that's possible.
            Chess.MoveInfo moveInfo = new Chess.MoveInfo
            {
                SourceSquare = moveStartSquare,
            };

            foreach (var square in EnumValues<Chess.Square>.List)
            {
                moveInfo.TargetSquare = square;
                var moveCheckResult = game.CurrentPosition.TestMove(moveInfo);
                if (moveCheckResult.IsLegalMove())
                {
                    // Highlight each found square.
                    PlayingBoard.SetSquareOverlayColor(ToSquareLocation(square), overlayColor);
                }
            }
        }

        private void StopDisplayLegalTargetSquaresEffect()
        {
            foreach (var squareLocation in PlayingBoard.AllSquareLocations)
            {
                PlayingBoard.SetSquareOverlayColor(squareLocation, Color.Empty);
            }
        }

        private void MouseMoveEffects(Point mouseLocation)
        {
            if (currentSquareWithPromoteEffect != null)
            {
                Rectangle hoverSquareRectangle = PlayingBoard.GetSquareRectangle(currentSquareWithPromoteEffect);
                SquareQuadrant hitQuadrant = SquareQuadrant.Indeterminate;
                if (hoverSquareRectangle.Contains(mouseLocation.X, mouseLocation.Y))
                {
                    int squareSize = PlayingBoard.SquareSize;
                    if (squareSize >= 2)
                    {
                        int halfSquareSize = (squareSize + 1) / 2;
                        if (mouseLocation.X - hoverSquareRectangle.Left < halfSquareSize)
                        {
                            hitQuadrant = mouseLocation.Y - hoverSquareRectangle.Top < halfSquareSize
                                        ? SquareQuadrant.TopLeft
                                        : SquareQuadrant.BottomLeft;
                        }
                        else
                        {
                            hitQuadrant = mouseLocation.Y - hoverSquareRectangle.Top < halfSquareSize
                                        ? SquareQuadrant.TopRight
                                        : SquareQuadrant.BottomRight;
                        }
                    }
                }
                UpdateHoverQuadrant(hitQuadrant, game.CurrentPosition.SideToMove);
            }

            if (moveStatus == MoveStatus.Dragging && dragCursor == null)
            {
                UpdateDragCursor(PlayingBoard.GetForegroundImage(ToSquareLocation(moveStartSquare)), dragStartPosition);
            }

            UpdateMoveStartSquareEffect();
        }

        private void PlayingBoard_MouseMove(object sender, MouseEventArgs e) => MouseMoveEffects(e.Location);

        private void HighlightHoverSquare()
        {
            var hoverSquare = PlayingBoard.HoverSquare;

            if (moveStatus != MoveStatus.Dragging
                && CanPieceBeMoved(hoverSquare)
                && (moveStatus == MoveStatus.None || ToSquare(hoverSquare) != moveStartSquare))
            {
                PlayingBoard.SetForegroundImageAttribute(hoverSquare, ForegroundImageAttribute.Highlight);
            }
        }

        private void EnterSquareEffects(SquareLocation location)
        {
            if (moveStatus != MoveStatus.None)
            {
                // If a legal move, display any piece about to be captured with a half-transparent effect.
                // Also display additional effects for special moves, detectable by the returned MoveCheckResult.
                Chess.MoveInfo moveInfo = new Chess.MoveInfo()
                {
                    SourceSquare = moveStartSquare,
                    TargetSquare = ToSquare(location),
                };

                var moveCheckResult = game.CurrentPosition.TestMove(moveInfo);

                if (moveCheckResult.IsLegalMove())
                {
                    if (moveCheckResult == Chess.MoveCheckResult.MissingEnPassant)
                    {
                        DisplayEnPassantEffect(game.CurrentPosition.EnPassantCaptureSquare);
                    }
                    else if (moveCheckResult == Chess.MoveCheckResult.MissingCastleQueenside)
                    {
                        DisplayCastlingEffect(moveInfo.SourceSquare - 4, moveInfo.SourceSquare - 1);
                    }
                    else if (moveCheckResult == Chess.MoveCheckResult.MissingCastleKingside)
                    {
                        DisplayCastlingEffect(moveInfo.SourceSquare + 3, moveInfo.SourceSquare + 1);
                    }
                    else
                    {
                        if (moveCheckResult == Chess.MoveCheckResult.MissingPromotionInformation)
                        {
                            DisplayPromoteEffect(moveInfo.TargetSquare);
                        }
                        DisplayCaptureEffect(moveInfo.TargetSquare);
                    }
                }
            }

            HighlightHoverSquare();
        }

        private void LeaveSquareEffects(SquareLocation location)
        {
            StopDisplayPromoteEffect();
            StopDisplayEnPassantEffect();
            StopDisplayCastlingEffect();
            StopDisplayCaptureEffect();

            if (moveStatus != MoveStatus.Dragging
                && CanPieceBeMoved(location)
                && (moveStatus == MoveStatus.None || ToSquare(location) != moveStartSquare))
            {
                PlayingBoard.SetForegroundImageAttribute(location, ForegroundImageAttribute.Default);
            }
        }

        private void PlayingBoard_MouseEnterSquare(PlayingBoard sender, SquareEventArgs e) => EnterSquareEffects(e.Location);

        private void PlayingBoard_MouseLeaveSquare(PlayingBoard sender, SquareEventArgs e) => LeaveSquareEffects(e.Location);

        private bool CommitOrCancelMove(SquareLocation targetSquare)
        {
            if (targetSquare != null)
            {
                // Move piece from source to destination.
                Chess.MoveInfo moveInfo = new Chess.MoveInfo
                {
                    SourceSquare = moveStartSquare,
                    TargetSquare = ToSquare(targetSquare),
                };

                if (currentSquareWithEnPassantEffect != null)
                {
                    // Must specify this MoveType to commit it.
                    moveInfo.MoveType = Chess.MoveType.EnPassant;
                }
                else if (rookSquareWithCastlingEffect != null)
                {
                    if (ToSquare(rookTargetSquareWithCastlingEffect).X() > ToSquare(rookSquareWithCastlingEffect).X())
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
                    moveInfo.PromoteTo = GetPromoteToPiece(hoverQuadrant, game.CurrentPosition.SideToMove).GetPiece();
                }

                ResetMoveEffects();

                game.TryMakeMove(ref moveInfo, true);

                if (moveInfo.Result == Chess.MoveCheckResult.OK)
                {
                    GameUpdated();
                    PlayingBoard.ActionHandler.Invalidate();
                    moveStatus = MoveStatus.None;
                    return true;
                }
            }
            else
            {
                ResetMoveEffects();
            }

            moveStatus = MoveStatus.None;
            return false;
        }

        private void PlayingBoard_SquareMouseDown(PlayingBoard sender, SquareMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (moveStatus == MoveStatus.None || e.Location == null || ToSquare(e.Location) != moveStartSquare)
                {
                    if (drawFocusMoveStartSquare)
                    {
                        drawFocusMoveStartSquare = false;
                        if (moveStatus != MoveStatus.None)
                        {
                            PlayingBoard.Invalidate();
                        }
                    }
                }

                // Commit or cancel a started move first, before checking e.Location again.
                bool moveMade = moveStatus != MoveStatus.None && CommitOrCancelMove(e.Location);

                // Only recheck if no move was made.
                if (!moveMade && CanPieceBeMoved(e.Location))
                {
                    moveStatus = MoveStatus.Dragging;
                    moveStartSquare = ToSquare(e.Location);
                    DisplayLegalTargetSquaresEffect();

                    // Immediately remove the highlight.
                    UpdateMoveStartSquareEffect();

                    Point dragStartSquareLocation = PlayingBoard.GetSquareRectangle(e.Location).Location;
                    dragStartPosition = new Point
                    {
                        X = e.MouseLocation.X - dragStartSquareLocation.X,
                        Y = e.MouseLocation.Y - dragStartSquareLocation.Y
                    };
                }
            }
        }

        private void ResetMoveEffects()
        {
            StopDisplayPromoteEffect();
            StopDisplayEnPassantEffect();
            StopDisplayCastlingEffect();
            StopDisplayCaptureEffect();
            StopDisplayLegalTargetSquaresEffect();

            foreach (var squareLocation in PlayingBoard.AllSquareLocations)
            {
                PlayingBoard.SetForegroundImageAttribute(squareLocation, ForegroundImageAttribute.Default);
            }

            HighlightHoverSquare();
        }

        private void PlayingBoard_SquareMouseUp(PlayingBoard sender, SquareMouseEventArgs e)
        {
            if (moveStatus == MoveStatus.Dragging)
            {
                // Always reset the half-transparency.
                PlayingBoard.SetForegroundImageAttribute(ToSquareLocation(moveStartSquare), ForegroundImageAttribute.Default);

                // Only commit or cancel a move if the piece was dropped onto a different square.
                if (e.Location == null || moveStartSquare != ToSquare(e.Location))
                {
                    CommitOrCancelMove(e.Location);
                }
                else
                {
                    drawFocusMoveStartSquare = true;
                    PlayingBoard.Invalidate();
                    moveStatus = MoveStatus.Moving;
                }

                UpdateMoveStartSquareEffect();
                UpdateDragCursor(null);
            }
        }

        internal void GameUpdated()
        {
            // Cancel any pending moves.
            ResetMoveEffects();
            moveStatus = MoveStatus.None;

            // Then copy the position.
            CopyPositionToBoard();
        }

        Pen lastMoveArrowPen;

        private void DrawLastMoveArrow(Graphics g, SquareLocation start, SquareLocation target)
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

        private Color GetDarkerGrayColor(Chess.Square square)
        {
            Color baseColor = ((square.X() + square.Y()) % 2) == 0
                            ? PlayingBoard.LightSquareColor
                            : PlayingBoard.DarkSquareColor;

            // Convert to grayscale, halve brightness. Convert.ToInt32() rounds.
            int targetBrightness = Convert.ToInt32(baseColor.GetBrightness() * 128);
            return Color.FromArgb(targetBrightness, targetBrightness, targetBrightness);
        }

        private void PlayingBoard_Paint(object sender, PaintEventArgs e)
        {
            // Draw a dotted line between the centers of the squares of the last move.
            if (game != null && !game.IsFirstMove)
            {
                Chess.Move lastCommittedMove = game.PreviousMove();
                DrawLastMoveArrow(e.Graphics,
                                  ToSquareLocation(lastCommittedMove.SourceSquare),
                                  ToSquareLocation(lastCommittedMove.TargetSquare));
            }

            // Draw subtle corners just inside the edges of a legal target square.
            var hoverSquare = PlayingBoard.HoverSquare;

            if (hoverSquare != null && moveStatus != MoveStatus.None && !PlayingBoard.GetSquareOverlayColor(hoverSquare).IsEmpty)
            {
                Rectangle hoverRect = PlayingBoard.GetSquareRectangle(hoverSquare);
                e.Graphics.ExcludeClip(Rectangle.Inflate(hoverRect, -10, 0));
                e.Graphics.ExcludeClip(Rectangle.Inflate(hoverRect, 0, -10));

                using (var darkerGrayPen = new Pen(GetDarkerGrayColor(ToSquare(hoverSquare)), 1f))
                {
                    e.Graphics.DrawRectangle(darkerGrayPen, new Rectangle(hoverRect.X, hoverRect.Y, hoverRect.Width - 1, hoverRect.Height - 1));
                }

                e.Graphics.ResetClip();
            }

            if (currentSquareWithPromoteEffect != null)
            {
                int squareSize = PlayingBoard.SquareSize;
                if (squareSize >= 2)
                {
                    Rectangle rect = PlayingBoard.GetSquareRectangle(currentSquareWithPromoteEffect);

                    Chess.Color promoteColor = game.CurrentPosition.SideToMove;

                    SquareQuadrant[] allQuadrants = { SquareQuadrant.TopLeft, SquareQuadrant.TopRight, SquareQuadrant.BottomLeft, SquareQuadrant.BottomRight };
                    allQuadrants.ForEach(quadrant =>
                    {
                        if (quadrant == hoverQuadrant)
                        {
                            Image image = PieceImages[GetPromoteToPiece(quadrant, promoteColor)];
                            e.Graphics.DrawImage(PieceImages[GetPromoteToPiece(quadrant, promoteColor)],
                                                 GetSquareQuadrantRectangle(ref rect, quadrant),
                                                 0, 0, image.Width, image.Height,
                                                 GraphicsUnit.Pixel,
                                                 PlayingBoard.HighlightImageAttributes);
                        }
                        else
                        {
                            e.Graphics.DrawImage(PieceImages[GetPromoteToPiece(quadrant, promoteColor)],
                                                 GetSquareQuadrantRectangle(ref rect, quadrant));
                        }
                    });
                }
            }

            // Draw a kind of focus rectangle around the moveStartSquare if not dragging.
            if (moveStatus != MoveStatus.None && drawFocusMoveStartSquare)
            {
                Rectangle activeRect = PlayingBoard.GetSquareRectangle(ToSquareLocation(moveStartSquare));

                using (Pen darkerGrayPen = new Pen(GetDarkerGrayColor(moveStartSquare), 2f))
                {
                    e.Graphics.DrawRectangle(darkerGrayPen, activeRect.X, activeRect.Y, activeRect.Width - 1, activeRect.Height - 1);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lastMoveArrowPen.Dispose();
            }

            base.Dispose(disposing);
        }

        private void PerformAutoFit(int? targetSquareSize)
        {
            // Don't expect a StandardChessBoard to be docked somewhere else, but also don't throw.
            if (FindForm() is MenuCaptionBarForm<StandardChessBoard> chessBoardForm)
            {
                PerformAutoFit(chessBoardForm, targetSquareSize);
            }
        }

        public static void ConstrainClientSize(MenuCaptionBarForm<StandardChessBoard> chessBoardForm)
        {
            chessBoardForm.Resizing += ChessBoardForm_Resizing;
            PerformAutoFit(chessBoardForm, null);

            // Only snap while moving.
            OwnedFormSnapHelper snapHelper = OwnedFormSnapHelper.AttachTo(chessBoardForm);
            snapHelper.SnapWhileResizing = false;
        }

        /// <summary>
        /// Manually updates the size of the form to a value which it would have snapped to had it been resized by WM_SIZING messages.
        /// </summary>
        /// <param name="targetSquareSize">
        /// IF not-null and greater than 0, will resize the form so the playing board will have the desired square size.
        /// </param>
        private static void PerformAutoFit(MenuCaptionBarForm<StandardChessBoard> chessBoardForm, int? targetSquareSize)
        {
            // Simulate OnResizing event.
            // No need to create a RECT with coordinates relative to the screen, since only the size may be affected.
            RECT windowRect;
            if (targetSquareSize == null || targetSquareSize.Value <= 0)
            {
                windowRect = new RECT
                {
                    Left = chessBoardForm.Left,
                    Right = chessBoardForm.Right,
                    Top = chessBoardForm.Top,
                    Bottom = chessBoardForm.Bottom,
                };
            }
            else
            {
                Size targetSize = chessBoardForm.DockedControl.PlayingBoard.GetExactAutoFitSize(targetSquareSize.Value);
                Size clientAreaSize = chessBoardForm.ClientAreaSize;
                targetSize.Width += chessBoardForm.ClientSize.Width - clientAreaSize.Width;
                targetSize.Height += chessBoardForm.ClientSize.Height - clientAreaSize.Height;

                windowRect = new RECT
                {
                    Left = chessBoardForm.Left,
                    Right = chessBoardForm.Left + targetSize.Width,
                    Top = chessBoardForm.Top,
                    Bottom = chessBoardForm.Top + targetSize.Height,
                };
            }

            PerformAutoFit(chessBoardForm, ref windowRect, ResizeMode.BottomRight);

            chessBoardForm.SetBounds(windowRect.Left, windowRect.Top,
                                     windowRect.Right - windowRect.Left, windowRect.Bottom - windowRect.Top,
                                     BoundsSpecified.Size);

            chessBoardForm.DockedControl.PlayingBoard.Size = chessBoardForm.DockedControl.PlayingBoard.GetClosestAutoFitSize(chessBoardForm.DockedControl.ClientSize);
        }

        private static void PerformAutoFit(MenuCaptionBarForm<StandardChessBoard> chessBoardForm, ref RECT resizeRect, ResizeMode resizeMode)
        {
            Size clientAreaSize = chessBoardForm.ClientAreaSize;
            int widthDifference = chessBoardForm.ClientSize.Width - clientAreaSize.Width;
            int heightDifference = chessBoardForm.ClientSize.Height - clientAreaSize.Height;

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
            Size targetSize = chessBoardForm.DockedControl.PlayingBoard.GetClosestAutoFitSize(maxBounds);

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

        private static void ChessBoardForm_Resizing(object sender, ResizeEventArgs e)
        {
            // Snap to auto-fit.
            PerformAutoFit((MenuCaptionBarForm<StandardChessBoard>)sender, ref e.MoveResizeRect, e.ResizeMode);
        }

        event Action IDockableControl.DockPropertiesChanged { add { } remove { } }
        void IDockableControl.CanClose(CloseReason closeReason, ref bool cancel) { }
    }
}

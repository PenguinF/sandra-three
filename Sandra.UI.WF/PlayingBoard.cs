/*********************************************************************************
 * PlayingBoard.cs
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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents a playing board of squares occupied by pieces represented by foreground images.
    /// Square coordinate (0, 0) is the top left square.
    /// </summary>
    public class PlayingBoard : Control
    {
        public PlayingBoard()
        {
            // Styles appropriate for a graphics-heavy control.
            SetStyle(ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.UserMouse
                | ControlStyles.Selectable
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.FixedHeight
                | ControlStyles.FixedWidth
                | ControlStyles.Opaque, true);

            updateBackgroundBrush();
            updateBorderBrush();
            updateDarkSquareBrush();
            updateLightSquareBrush();
            updateSquareArrays();

            // Highlight by setting a gamma smaller than 1.
            var highlight = new ImageAttributes();
            highlight.SetGamma(0.6f);
            highlightImgAttributes = highlight;

            // Half-transparent foreground image at the source square, when moving.
            var halfTransparent = new ImageAttributes();
            ColorMatrix halfTransparentMatrix = new ColorMatrix(new float[][]
            {
                new float[] {1, 0, 0, 0, 0},
                new float[] {0, 1, 0, 0, 0},
                new float[] {0, 0, 1, 0, 0},
                new float[] {0, 0, 0, 0.4f, 0},
                new float[] {0, 0, 0, 0, 0}
            });
            halfTransparent.SetColorMatrix(halfTransparentMatrix);
            moveSourceImageAttributes = halfTransparent;
        }

        private readonly ImageAttributes highlightImgAttributes;
        private readonly ImageAttributes moveSourceImageAttributes;

        private readonly PropertyStore propertyStore = new PropertyStore
        {
            { nameof(BoardSize), DefaultBoardSize },
            { nameof(BorderColor), DefaultBorderColor },
            { nameof(BorderWidth), DefaultBorderWidth },
            { nameof(DarkSquareColor), DefaultDarkSquareColor },
            { nameof(ForegroundImagePadding), DefaultForegroundImagePadding },
            { nameof(ForegroundImageRelativeSize), DefaultForegroundImageRelativeSize },
            { nameof(InnerSpacing), DefaultInnerSpacing },
            { nameof(LightSquareColor), DefaultLightSquareColor },
            { nameof(SizeToFit), DefaultSizeToFit },
            { nameof(SquareSize), DefaultSquareSize },

            { nameof(backgroundBrush), null },
            { nameof(borderBrush), null },
            { nameof(darkSquareBrush), null },
            { nameof(lightSquareBrush), null },
        };


        /// <summary>
        /// Gets the default value for the <see cref="BoardSize"/> property.
        /// </summary>
        public const int DefaultBoardSize = 8;

        /// <summary>
        /// Gets or sets the number of squares in a row or file. The minimum value is 1.
        /// The default value is <see cref="DefaultBoardSize"/> (8).
        /// </summary>
        [DefaultValue(DefaultBoardSize)]
        public int BoardSize
        {
            get { return propertyStore.Get<int>(nameof(BoardSize)); }
            set
            {
                // A board size of 0 will cause a division by zero error if SizeToFit is true.
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(BoardSize), value, "Board size must be 1 or higher.");
                }
                if (propertyStore.Set(nameof(BoardSize), value))
                {
                    updateSquareArrays();
                    verifySizeToFit();
                    Invalidate();
                }
            }
        }


        /// <summary>
        /// Gets the default value for the <see cref="BorderColor"/> property.
        /// </summary>
        public static Color DefaultBorderColor { get { return Color.Black; } }

        /// <summary>
        /// Gets or sets the color of the border area.
        /// The default value is <see cref="DefaultBorderColor"/> (<see cref="Color.Black"/>).
        /// </summary>
        public Color BorderColor
        {
            get { return propertyStore.Get<Color>(nameof(BorderColor)); }
            set
            {
                if (propertyStore.Set(nameof(BorderColor), value))
                {
                    updateBorderBrush();
                    Invalidate();
                }
            }
        }


        /// <summary>
        /// Gets the default value for the <see cref="BorderWidth"/> property.
        /// </summary>
        public const int DefaultBorderWidth = 0;

        /// <summary>
        /// Gets or sets the width of the border around the playing board.
        /// The default value is <see cref="DefaultBorderWidth"/> (0).
        /// </summary>
        [DefaultValue(DefaultBorderWidth)]
        public int BorderWidth
        {
            get { return propertyStore.Get<int>(nameof(BorderWidth)); }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(BorderWidth), value, "Border width must be 0 or higher.");
                }
                if (propertyStore.Set(nameof(BorderWidth), value))
                {
                    verifySizeToFit();
                    Invalidate();
                }
            }
        }


        /// <summary>
        /// Gets the default value for the <see cref="DarkSquareColor"/> property.
        /// </summary>
        public static Color DefaultDarkSquareColor { get { return Color.Azure; } }

        /// <summary>
        /// Gets or sets the color of dark squares.
        /// The default value is <see cref="DefaultDarkSquareColor"/> (<see cref="Color.Azure"/>).
        /// </summary>
        public Color DarkSquareColor
        {
            get { return propertyStore.Get<Color>(nameof(DarkSquareColor)); }
            set
            {
                if (propertyStore.Set(nameof(DarkSquareColor), value))
                {
                    updateDarkSquareBrush();
                    Invalidate();
                }
            }
        }


        /// <summary>
        /// Gets the default value for the <see cref="ForegroundImagePadding"/> property.
        /// </summary>
        public static Padding DefaultForegroundImagePadding { get { return new Padding(0); } }

        /// <summary>
        /// Gets or sets the absolute padding between a foreground image and the edge of the square in which it is shown.
        /// The default value is <see cref="DefaultForegroundImagePadding"/>, which is zero padding.
        /// </summary>
        public Padding ForegroundImagePadding
        {
            get { return propertyStore.Get<Padding>(nameof(ForegroundImagePadding)); }
            set
            {
                if (value.Left < 0 || value.Right < 0 || value.Top < 0 || value.Bottom < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(ForegroundImagePadding), value, "The foreground image padding must contain values that are 0 or higher.");
                }
                if (propertyStore.Set(nameof(ForegroundImagePadding), value))
                {
                    Invalidate();
                }
            }
        }


        /// <summary>
        /// Gets the default value for the <see cref="ForegroundImageRelativeSize"/> property.
        /// </summary>
        public const double DefaultForegroundImageRelativeSize = 1;

        /// <summary>
        /// Gets or sets the size of a foreground image as a factor of the size of the square in which it is shown.
        /// The default value is <see cref="DefaultForegroundImageRelativeSize"/> (1.0).
        /// </summary>
        [DefaultValue(DefaultForegroundImageRelativeSize)]
        public double ForegroundImageRelativeSize
        {
            get { return propertyStore.Get<double>(nameof(ForegroundImageRelativeSize)); }
            set
            {
                // Do not allow cases where negative relative size and padding create a positive image size.
                // This has the weird effect of foreground images growing as the square size shrinks,
                // and appearing in the top left corner of the square in the negative padding area.
                if (value < 0.0)
                {
                    throw new ArgumentOutOfRangeException(nameof(ForegroundImageRelativeSize), value, "Relative size must be 0.0 or higher.");
                }
                if (propertyStore.Set(nameof(ForegroundImageRelativeSize), value))
                {
                    Invalidate();
                }
            }
        }


        /// <summary>
        /// Gets the default value for the <see cref="InnerSpacing"/> property.
        /// </summary>
        public const int DefaultInnerSpacing = 0;

        /// <summary>
        /// Gets or sets the amount of spacing between squares inside the playing board.
        /// The default value is <see cref="DefaultInnerSpacing"/> (0).
        /// </summary>
        [DefaultValue(DefaultInnerSpacing)]
        public int InnerSpacing
        {
            get { return propertyStore.Get<int>(nameof(InnerSpacing)); }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(InnerSpacing), value, "Inner spacing must be 0 or higher.");
                }
                if (propertyStore.Set(nameof(InnerSpacing), value))
                {
                    verifySizeToFit();
                    Invalidate();
                }
            }
        }


        /// <summary>
        /// Gets if an image is currently being moved.
        /// </summary>
        [Browsable(false)]
        public bool IsMoving
        {
            get
            {
                return isMoving;
            }
        }


        /// <summary>
        /// Gets the default value for the <see cref="LightSquareColor"/> property.
        /// </summary>
        public static Color DefaultLightSquareColor { get { return Color.LightBlue; } }

        /// <summary>
        /// Gets or sets the color of light squares.
        /// The default value is <see cref="DefaultLightSquareColor"/> (<see cref="Color.LightBlue"/>).
        /// </summary>
        public Color LightSquareColor
        {
            get { return propertyStore.Get<Color>(nameof(LightSquareColor)); }
            set
            {
                if (propertyStore.Set(nameof(LightSquareColor), value))
                {
                    updateLightSquareBrush();
                    Invalidate();
                }
            }
        }


        /// <summary>
        /// Gets the default value for the <see cref="SizeToFit"/> property.
        /// </summary>
        public const bool DefaultSizeToFit = true;

        /// <summary>
        /// Gets or sets if <see cref="SquareSize"/> is automatically adjusted to fit the control's client area.
        /// The default value is <see cref="DefaultSizeToFit"/> (true).
        /// </summary>
        [DefaultValue(DefaultSizeToFit)]
        public bool SizeToFit
        {
            get { return propertyStore.Get<bool>(nameof(SizeToFit)); }
            set
            {
                if (propertyStore.Set(nameof(SizeToFit), value))
                {
                    verifySizeToFit();
                    Invalidate();
                }
            }
        }


        /// <summary>
        /// Gets the default value for the <see cref="SquareSize"/> property.
        /// </summary>
        public const int DefaultSquareSize = 44;

        /// <summary>
        /// Gets or sets the size of a single square on the board in pixels.
        /// The default value is <see cref="DefaultSquareSize"/> (44).
        /// </summary>
        [DefaultValue(DefaultSquareSize)]
        public int SquareSize
        {
            get { return propertyStore.Get<int>(nameof(SquareSize)); }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(SquareSize), value, "Square size must be 0 or higher.");
                }
                // No effect if SizeToFit.
                if (SizeToFit)
                {
                    return;
                }
                if (propertyStore.Set(nameof(SquareSize), value))
                {
                    Invalidate();
                }
            }
        }


        private Brush backgroundBrush
        {
            get { return propertyStore.Get<Brush>(nameof(backgroundBrush)); }
            set { propertyStore.Set(nameof(backgroundBrush), value); }
        }

        private void updateBackgroundBrush()
        {
            backgroundBrush = new SolidBrush(BackColor);
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            updateBackgroundBrush();
            Invalidate();
            base.OnBackColorChanged(e);
        }


        private Brush borderBrush
        {
            get { return propertyStore.Get<Brush>(nameof(borderBrush)); }
            set { propertyStore.Set(nameof(borderBrush), value); }
        }

        private void updateBorderBrush()
        {
            borderBrush = new SolidBrush(BorderColor);
        }


        private Brush darkSquareBrush
        {
            get { return propertyStore.Get<Brush>(nameof(darkSquareBrush)); }
            set { propertyStore.Set(nameof(darkSquareBrush), value); }
        }

        private void updateDarkSquareBrush()
        {
            darkSquareBrush = new SolidBrush(DarkSquareColor);
        }


        private Brush lightSquareBrush
        {
            get { return propertyStore.Get<Brush>(nameof(lightSquareBrush)); }
            set { propertyStore.Set(nameof(lightSquareBrush), value); }
        }

        private void updateLightSquareBrush()
        {
            lightSquareBrush = new SolidBrush(LightSquareColor);
        }


        private Image[] foregroundImages;

        /// <summary>
        /// Gets the <see cref="Image"/> on position (x, y).
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="x"/> or <paramref name="y"/> are smaller than 0 or greater than or equal to <see cref="BoardSize"/>.
        /// </exception>
        public Image GetForegroundImage(int x, int y)
        {
            int index = getIndex(x, y);
            return foregroundImages[index];
        }

        /// <summary>
        /// Sets the <see cref="Image"/> on position (x, y).
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="x"/> or <paramref name="y"/> are smaller than 0 or greater than or equal to <see cref="BoardSize"/>.
        /// </exception>
        public void SetForegroundImage(int x, int y, Image value)
        {
            int index = getIndex(x, y);
            if (foregroundImages[index] != value)
            {
                foregroundImages[index] = value;
                Invalidate();
            }
        }


        private bool[] isImageHighlighted;

        /// <summary>
        /// Gets if the <see cref="Image"/> on position (x, y) is highlighted or not.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="x"/> or <paramref name="y"/> are smaller than 0 or greater than or equal to <see cref="BoardSize"/>.
        /// </exception>
        public bool GetIsImageHighLighted(int x, int y)
        {
            int index = getIndex(x, y);
            return isImageHighlighted[index];
        }

        /// <summary>
        /// Sets if the <see cref="Image"/> on position (x, y) is highlighted or not.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="x"/> or <paramref name="y"/> are smaller than 0 or greater than or equal to <see cref="BoardSize"/>.
        /// </exception>
        public void SetIsImageHighLighted(int x, int y, bool value)
        {
            int index = getIndex(x, y);
            if (isImageHighlighted[index] != value)
            {
                isImageHighlighted[index] = value;
                Invalidate();
            }
        }


        private Color[] squareOverlayColors;

        /// <summary>
        /// Gets an overlay color for the square on position (x, y).
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="x"/> or <paramref name="y"/> are smaller than 0 or greater than or equal to <see cref="BoardSize"/>.
        /// </exception>
        public Color GetSquareOverlayColor(int x, int y)
        {
            int index = getIndex(x, y);
            return squareOverlayColors[index];
        }

        /// <summary>
        /// Sets an overlay color for the square on position (x, y).
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="x"/> or <paramref name="y"/> are smaller than 0 or greater than or equal to <see cref="BoardSize"/>.
        /// </exception>
        public void SetSquareOverlayColor(int x, int y, Color value)
        {
            int index = getIndex(x, y);
            if (squareOverlayColors[index] != value)
            {
                squareOverlayColors[index] = value;
                Invalidate();
            }
        }


        private void updateSquareArrays()
        {
            int newArrayLength = BoardSize * BoardSize;
            foregroundImages = new Image[newArrayLength];
            isImageHighlighted = new bool[newArrayLength];
            squareOverlayColors = new Color[newArrayLength];
        }


        /// <summary>
        /// Occurs when the mouse pointer enters a square.
        /// </summary>
        public event EventHandler<SquareEventArgs> MouseEnterSquare;

        /// <summary>
        /// Raises the <see cref="MouseEnterSquare"/> event. 
        /// </summary>
        protected virtual void OnMouseEnterSquare(SquareEventArgs e)
        {
            MouseEnterSquare?.Invoke(this, e);
        }

        protected void RaiseMouseEnterSquare(int squareIndex)
        {
            OnMouseEnterSquare(new SquareEventArgs(getX(squareIndex), getY(squareIndex)));
        }


        /// <summary>
        /// Occurs when the mouse pointer leaves a square.
        /// </summary>
        public event EventHandler<SquareEventArgs> MouseLeaveSquare;

        /// <summary>
        /// Raises the <see cref="MouseLeaveSquare"/> event. 
        /// </summary>
        protected virtual void OnMouseLeaveSquare(SquareEventArgs e)
        {
            MouseLeaveSquare?.Invoke(this, e);
        }

        protected void RaiseMouseLeaveSquare(int squareIndex)
        {
            OnMouseLeaveSquare(new SquareEventArgs(getX(squareIndex), getY(squareIndex)));
        }


        /// <summary>
        /// Occurs when an image stops being moved, and is not dropped onto another square.
        /// </summary>
        public event EventHandler<MoveEventArgs> MoveCancel;

        /// <summary>
        /// Raises the <see cref="MoveCancel"/> event. 
        /// </summary>
        protected virtual void OnMoveCancel(MoveEventArgs e)
        {
            MoveCancel?.Invoke(this, e);
        }

        protected void RaiseMoveCancel(int squareIndex)
        {
            OnMoveCancel(new MoveEventArgs(getX(squareIndex), getY(squareIndex)));
        }


        /// <summary>
        /// Occurs when an image is being moved and dropped onto another square.
        /// </summary>
        public event EventHandler<MoveCommitEventArgs> MoveCommit;

        /// <summary>
        /// Raises the <see cref="MoveCommit"/> event. 
        /// </summary>
        protected virtual void OnMoveCommit(MoveCommitEventArgs e)
        {
            MoveCommit?.Invoke(this, e);
        }

        protected void RaiseMoveCommit(int sourceSquareIndex, int targetSquareIndex)
        {
            OnMoveCommit(new MoveCommitEventArgs(getX(sourceSquareIndex),
                                                 getY(sourceSquareIndex),
                                                 getX(targetSquareIndex),
                                                 getY(targetSquareIndex)));
        }


        /// <summary>
        /// Occurs when an image occupying a square starts being moved.
        /// </summary>
        public event EventHandler<CancellableSquareEventArgs> MoveStart;

        /// <summary>
        /// Raises the <see cref="MoveStart"/> event. 
        /// </summary>
        protected virtual void OnMoveStart(CancellableSquareEventArgs e)
        {
            MoveStart?.Invoke(this, e);
        }

        protected bool RaiseMoveStart(int squareIndex)
        {
            var e = new CancellableSquareEventArgs(getX(squareIndex), getY(squareIndex));
            OnMoveStart(e);
            return !e.Cancel;
        }


        private int getX(int index) { return index % BoardSize; }
        private int getY(int index) { return index / BoardSize; }

        private int getIndex(int x, int y)
        {
            if (x < 0 || x >= BoardSize) throw new IndexOutOfRangeException(nameof(x));
            if (y < 0 || y >= BoardSize) throw new IndexOutOfRangeException(nameof(y));
            return y * BoardSize + x;
        }

        private Point getLocationFromIndex(int index)
        {
            if (index < 0 || index >= BoardSize * BoardSize)
            {
                return Point.Empty;
            }

            int x = getX(index),
                y = getY(index),
                delta = SquareSize + InnerSpacing;
            int px = BorderWidth + x * delta,
                py = BorderWidth + y * delta;

            return new Point(px, py);
        }

        private Rectangle getRelativeForegroundImageRectangle()
        {
            // Returns the rectangle of a foreground image relative to its containing square.
            if (SquareSize > 0)
            {
                int foregroundImageSize = (int)Math.Floor(SquareSize * ForegroundImageRelativeSize);
                var padding = ForegroundImagePadding;
                int imageOffset = (SquareSize - foregroundImageSize) / 2;
                int left = imageOffset + padding.Left;
                int top = imageOffset + padding.Top;
                int width = foregroundImageSize - padding.Horizontal;
                int height = foregroundImageSize - padding.Vertical;
                if (width > 0 && height > 0)
                {
                    return new Rectangle(left, top, width, height);
                }
            }
            // Default empty rectangle.
            return new Rectangle();
        }

        private int squareSizeFromClientSize(int clientSize)
        {
            int result = (clientSize - InnerSpacing * (BoardSize - 1) - BorderWidth * 2) / BoardSize;
            return Math.Max(result, 0);
        }

        private void performSizeToFit()
        {
            // Resize the squares so that it is as large as possible while still fitting in the client area.
            int minSize = Math.Min(ClientSize.Height, ClientSize.Width);
            int newSquareSize = squareSizeFromClientSize(minSize);
            // Store directly in the property store, to bypass SizeToFit check.
            if (propertyStore.Set(nameof(SquareSize), newSquareSize))
            {
                Invalidate();
            }
        }

        private void verifySizeToFit()
        {
            // Only conditionally perform size-to-fit.
            if (SizeToFit) performSizeToFit();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            verifySizeToFit();
            base.OnLayout(levent);
        }

        /// <summary>
        /// Returns the size closest to the given size which will allow the board to fit exactly.
        /// </summary>
        public int GetClosestAutoFitSize(int clientSize)
        {
            int squareSize = squareSizeFromClientSize(clientSize);
            // Go back to a client size by inverting squareSizeFromClientSize().
            return squareSize * BoardSize + InnerSpacing * (BoardSize - 1) + BorderWidth * 2;
        }


        private Point lastKnownMouseMovePoint = new Point(-1, -1);

        private int hoveringSquareIndex = -1;

        private bool isMoving;
        private Point moveStartPosition;
        private Point moveCurrentPosition;
        private int moveStartSquareIndex;

        private int hitTest(Point clientLocation)
        {
            int squareSize = SquareSize;

            if (squareSize == 0)
            {
                // No square can contain the point.
                // Short-circuit exit here to prevent division by zeroes.
                return -1;
            }

            int boardSize = BoardSize;
            int borderWidth = BorderWidth;

            int px = clientLocation.X - borderWidth,
                py = clientLocation.Y - borderWidth,
                delta = squareSize + InnerSpacing;
            int x = px / delta,
                y = py / delta,
                remainderX = px % delta,
                remainderY = py % delta;

            int hit;
            if (x < 0 || x >= boardSize || y < 0 || y >= boardSize || remainderX >= squareSize || remainderY >= squareSize)
            {
                // Either outside of the actual board, or hitting a border.
                hit = -1;
            }
            else
            {
                // The location is inside a square.
                hit = y * boardSize + x;
            }

            // Update hovering information.
            if (hoveringSquareIndex != hit)
            {
                if (hoveringSquareIndex >= 0)
                {
                    RaiseMouseLeaveSquare(hoveringSquareIndex);
                }
                hoveringSquareIndex = hit;
                if (hoveringSquareIndex >= 0)
                {
                    RaiseMouseEnterSquare(hoveringSquareIndex);
                }
            }

            return hit;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (lastKnownMouseMovePoint.X >= 0 && lastKnownMouseMovePoint.Y >= 0)
            {
                hitTest(lastKnownMouseMovePoint);
            }
            base.OnMouseEnter(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            int hit = hitTest(e.Location);

            // Start moving?
            if (e.Button == MouseButtons.Left && !isMoving)
            {
                // Only start when a square is hit.
                if (hit >= 0 && foregroundImages[hit] != null)
                {
                    if (RaiseMoveStart(hit))
                    {
                        isMoving = true;
                        moveStartPosition = new Point(-e.Location.X, -e.Location.Y);
                        moveStartPosition.Offset(getLocationFromIndex(hit));
                        Rectangle imageRect = getRelativeForegroundImageRectangle();
                        moveStartPosition.Offset(imageRect.Location);
                        moveCurrentPosition = e.Location;
                        moveStartSquareIndex = hit;
                        Invalidate();
                    }
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            // Do a hit test, which updates hover information.
            int hit = hitTest(e.Location);

            // Update moving information.
            if (isMoving)
            {
                moveCurrentPosition = e.Location;
                Invalidate();
            }

            // Remember position for mouse-enters without mouse-leaves.
            lastKnownMouseMovePoint = e.Location;

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (isMoving)
            {
                // End of move.
                isMoving = false;

                if (hoveringSquareIndex >= 0)
                {
                    RaiseMoveCommit(moveStartSquareIndex, hoveringSquareIndex);
                }
                else
                {
                    RaiseMoveCancel(moveStartSquareIndex);
                }

                Invalidate();
            }

            base.OnMouseUp(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            // Hit test a position outside of the control to reset the hover square index and raise proper events.
            lastKnownMouseMovePoint = new Point(-1, -1);
            hitTest(lastKnownMouseMovePoint);

            base.OnMouseLeave(e);
        }


        protected override void OnPaint(PaintEventArgs pe)
        {
            Graphics g = pe.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // First cache some property values needed for painting so they don't get typecast repeatedly out of the property store.
            int boardSize = BoardSize;
            int squareSize = SquareSize;
            int innerSpacing = InnerSpacing;
            int delta = squareSize + innerSpacing;
            int borderWidth = BorderWidth;
            int totalBoardSize = delta * boardSize - innerSpacing;
            int totalSize = borderWidth * 2 + totalBoardSize;

            Rectangle clipRectangle = pe.ClipRectangle;
            Rectangle boardRectangle = new Rectangle(borderWidth, borderWidth, totalBoardSize, totalBoardSize);
            Rectangle boardWithBorderRectangle = new Rectangle(0, 0, totalSize, totalSize);

            // Draw the background area not covered by the playing board.
            g.ExcludeClip(boardWithBorderRectangle);
            if (!g.IsVisibleClipEmpty) g.FillRectangle(backgroundBrush, ClientRectangle);
            g.ResetClip();

            // Draw the background light and dark squares.
            if (squareSize > 0 && clipRectangle.IntersectsWith(boardRectangle))
            {
                // Draw dark squares over the entire board.
                g.FillRectangle(darkSquareBrush, boardRectangle);

                // Draw light squares by excluding the dark squares, and then filling up what's left.
                int doubleDelta = delta * 2;
                int y = borderWidth;
                for (int yIndex = boardSize - 1; yIndex >= 0; --yIndex)
                {
                    // Create block pattern by starting at logical coordinate 0 or 1 depending on the y-index.
                    int x = borderWidth + (yIndex & 1) * delta;
                    for (int xIndex = (boardSize - 1) / 2; xIndex >= 0; --xIndex)
                    {
                        g.ExcludeClip(new Rectangle(x, y, squareSize, squareSize));
                        x += doubleDelta;
                    }
                    y += delta;
                }
                g.FillRectangle(lightSquareBrush, boardRectangle);
                g.ResetClip();
            }

            // Draw borders.
            if ((borderWidth > 0 || innerSpacing > 0) && clipRectangle.IntersectsWith(boardWithBorderRectangle))
            {
                // Clip to borders.
                if (innerSpacing == 0)
                {
                    g.ExcludeClip(boardRectangle);
                }
                else
                {
                    // Exclude all squares one by one.
                    int y = borderWidth;
                    for (int j = 0; j < boardSize; ++j)
                    {
                        int x = borderWidth;
                        for (int k = 0; k < boardSize; ++k)
                        {
                            g.ExcludeClip(new Rectangle(x, y, squareSize, squareSize));
                            x += delta;
                        }
                        y += delta;
                    }
                }

                // And draw.
                g.FillRectangle(borderBrush, boardWithBorderRectangle);
                g.ResetClip();
            }

            if (squareSize > 0 && clipRectangle.IntersectsWith(boardRectangle))
            {
                // Draw foreground images.
                // Determine the image size and the amount of space around a foreground image within a square.
                Rectangle imgRect = getRelativeForegroundImageRectangle();
                int sizeH = imgRect.Width,
                    sizeV = imgRect.Height;

                if (sizeH > 0 && sizeV > 0)
                {
                    int hOffset = borderWidth + imgRect.Left,
                        vOffset = borderWidth + imgRect.Top;

                    // Loop over foreground images and draw them.
                    int y = vOffset;
                    int index = 0;
                    for (int j = 0; j < boardSize; ++j)
                    {
                        int x = hOffset;
                        for (int k = 0; k < boardSize; ++k)
                        {
                            // Select picture.
                            Image currentImg = foregroundImages[index];
                            if (currentImg != null)
                            {
                                // Draw current image - but use a color transformation if the current square was
                                // used to start moving from, or if the image must be highlighted.
                                if (isMoving && index == moveStartSquareIndex)
                                {
                                    // Half-transparent.
                                    g.DrawImage(currentImg,
                                                new Rectangle(x, y, sizeH, sizeV),
                                                0, 0, currentImg.Width, currentImg.Height,
                                                GraphicsUnit.Pixel,
                                                moveSourceImageAttributes);
                                }
                                else if (isImageHighlighted[index])
                                {
                                    // Highlight piece.
                                    g.DrawImage(currentImg,
                                                new Rectangle(x, y, sizeH, sizeV),
                                                0, 0, currentImg.Width, currentImg.Height,
                                                GraphicsUnit.Pixel,
                                                highlightImgAttributes);
                                }
                                else
                                {
                                    // Default case.
                                    g.DrawImage(currentImg, new Rectangle(x, y, sizeH, sizeV));
                                }
                            }
                            x += delta;
                            ++index;
                        }
                        y += delta;
                    }
                }

                // Apply square highlights.
                for (int index = 0; index < boardSize * boardSize; ++index)
                {
                    if (!squareOverlayColors[index].IsEmpty)
                    {
                        Point offset = getLocationFromIndex(index);
                        // Draw overlay color on the square, with the already drawn foreground image.
                        using (var overlayBrush = new SolidBrush(squareOverlayColors[index]))
                        {
                            g.FillRectangle(overlayBrush, offset.X, offset.Y, squareSize, squareSize);
                        }
                    }
                }

                if (sizeH > 0 && sizeV > 0 && isMoving)
                {
                    // Draw moving image on top of the rest.
                    Image currentImg = foregroundImages[moveStartSquareIndex];
                    if (currentImg != null)
                    {
                        Point location = moveCurrentPosition;
                        location.Offset(moveStartPosition);

                        // Make sure the piece looks exactly the same as when it was still on its source square.
                        if (isImageHighlighted[moveStartSquareIndex])
                        {
                            // Highlight piece.
                            g.DrawImage(currentImg,
                                        new Rectangle(location.X, location.Y, sizeH, sizeV),
                                        0, 0, currentImg.Width, currentImg.Height,
                                        GraphicsUnit.Pixel,
                                        highlightImgAttributes);
                        }
                        else
                        {
                            // Default case.
                            g.DrawImage(currentImg, new Rectangle(location.X, location.Y, sizeH, sizeV));
                        }
                    }
                }
            }

            base.OnPaint(pe);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                highlightImgAttributes.Dispose();
                moveSourceImageAttributes.Dispose();

                // To dispose of stored disposable values such as brushes.
                propertyStore.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Provides data for the <see cref="PlayingBoard.MouseEnterSquare"/>
    /// or <see cref="PlayingBoard.MouseLeaveSquare"/> event.
    /// </summary>
    [DebuggerDisplay("(x = {X}, y = {Y})")]
    public class SquareEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the X-coordinate of the square.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Gets the Y-coordinate of the square.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SquareEventArgs"/> class.
        /// </summary>
        /// <param name="x">
        /// The X-coordinate of the square.
        /// </param>
        /// <param name="y">
        /// The Y-coordinate of the square.
        /// </param>
        public SquareEventArgs(int x, int y)
        {
            X = x; Y = y;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="PlayingBoard.MoveStart"/> event.
    /// </summary>
    public class CancellableSquareEventArgs : SquareEventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether the event should be canceled.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CancellableSquareEventArgs"/> class.
        /// </summary>
        /// <param name="x">
        /// The X-coordinate of the square.
        /// </param>
        /// <param name="y">
        /// The Y-coordinate of the square.
        /// </param>
        public CancellableSquareEventArgs(int x, int y) : base(x, y)
        {
        }
    }

    /// <summary>
    /// Provides data for the <see cref="PlayingBoard.MoveCancel"/> event.
    /// </summary>
    [DebuggerDisplay("From (x = {StartX}, y = {StartY})")]
    public class MoveEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the X-coordinate of the square where moving started.
        /// </summary>
        public int StartX { get; }

        /// <summary>
        /// Gets the Y-coordinate of the square where moving started.
        /// </summary>
        public int StartY { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveEventArgs"/> class.
        /// </summary>
        /// <param name="startX">
        /// The X-coordinate of the square where moving started.
        /// </param>
        /// <param name="startY">
        /// The Y-coordinate of the square where moving started.
        /// </param>
        public MoveEventArgs(int startX, int startY)
        {
            StartX = startX; StartY = startY;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="PlayingBoard.MoveCommit"/> event.
    /// </summary>
    [DebuggerDisplay("From (x = {StartX}, y = {StartY}) to (x = {TargetX}, y = {TargetY})")]
    public class MoveCommitEventArgs : MoveEventArgs
    {
        /// <summary>
        /// Gets the X-coordinate of the square where the mouse cursor currently is.
        /// </summary>
        public int TargetX { get; }

        /// <summary>
        /// Gets the Y-coordinate of the square where the mouse cursor currently is.
        /// </summary>
        public int TargetY { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoveCommitEventArgs"/> class.
        /// </summary>
        /// <param name="startX">
        /// The X-coordinate of the square where moving started.
        /// </param>
        /// <param name="startY">
        /// The Y-coordinate of the square where moving started.
        /// </param>
        /// <param name="targetX">
        /// The X-coordinate of the square where the mouse cursor currently is.
        /// </param>
        /// <param name="targetY">
        /// The Y-coordinate of the square where the mouse cursor currently is.
        /// </param>
        public MoveCommitEventArgs(int startX, int startY, int targetX, int targetY) : base(startX, startY)
        {
            TargetX = targetX; TargetY = targetY;
        }
    }
}

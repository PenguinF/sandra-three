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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
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
            updateForegroundImages();
        }

        private readonly PropertyStore propertyStore = new PropertyStore
        {
            { nameof(BoardSize), DefaultBoardSize },
            { nameof(BorderColor), DefaultBorderColor },
            { nameof(BorderWidth), DefaultBorderWidth },
            { nameof(DarkSquareColor), DefaultDarkSquareColor },
            { nameof(ForegroundImagePadding), DefaultForegroundImagePadding },
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
                    updateForegroundImages();
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
        public static Color DefaultDarkSquareColor { get { return Color.Brown; } }

        /// <summary>
        /// Gets or sets the color of dark squares.
        /// The default value is <see cref="DefaultDarkSquareColor"/> (<see cref="Color.Brown"/>).
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
                if (propertyStore.Set(nameof(ForegroundImagePadding), value))
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
        /// Gets the default value for the <see cref="LightSquareColor"/> property.
        /// </summary>
        public static Color DefaultLightSquareColor { get { return Color.SandyBrown; } }

        /// <summary>
        /// Gets or sets the color of light squares.
        /// The default value is <see cref="DefaultLightSquareColor"/> (<see cref="Color.SandyBrown"/>).
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
            base.OnBackColorChanged(e);
            updateBackgroundBrush();
            Invalidate();
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

        private void updateForegroundImages()
        {
            int oldArrayLength = foregroundImages == null ? 0 : foregroundImages.Length,
                newArrayLength = BoardSize * BoardSize;

            Image[] newForegroundImages = new Image[newArrayLength];
            int min = Math.Min(newArrayLength, oldArrayLength);
            if (min > 0)
            {
                Array.Copy(foregroundImages, newForegroundImages, min);
            }
            foregroundImages = newForegroundImages;
        }

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

        private int getIndex(int x, int y)
        {
            int boardSize = BoardSize;
            if (x < 0 || x >= boardSize) throw new IndexOutOfRangeException(nameof(x));
            if (y < 0 || y >= boardSize) throw new IndexOutOfRangeException(nameof(y));
            return y * boardSize + x;
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

        protected override void OnLayout(LayoutEventArgs args)
        {
            base.OnLayout(args);
            verifySizeToFit();
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


        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            Graphics g = pe.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // First cache some property values needed for painting so they don't get typecast repeatedly out of the property store.
            int boardSize = BoardSize;
            int boardSizeMinusOne = boardSize - 1;
            int squareSize = SquareSize;
            int innerBorderWidth = InnerSpacing;
            int delta = squareSize + innerBorderWidth;
            int borderWidth = BorderWidth;
            int totalBoardSize = delta * boardSizeMinusOne + squareSize;
            int totalSize = borderWidth * 2 + totalBoardSize;
            Rectangle clipRectangle = pe.ClipRectangle;
            Rectangle boardRectangle = new Rectangle(borderWidth, borderWidth, totalBoardSize, totalBoardSize);
            Rectangle boardWithBorderRectangle = new Rectangle(0, 0, totalSize, totalSize);

            // Draw the background area not covered by the playing board.
            g.ExcludeClip(boardWithBorderRectangle);
            if (!g.IsVisibleClipEmpty) g.FillRectangle(backgroundBrush, ClientRectangle);
            g.ResetClip();

            // Draw borders.
            if ((borderWidth > 0 || innerBorderWidth > 0) && clipRectangle.IntersectsWith(boardWithBorderRectangle))
            {
                g.FillRectangle(borderBrush, boardWithBorderRectangle);
            }

            // Draw the background light and dark squares.
            if (squareSize > 0 && clipRectangle.IntersectsWith(boardRectangle))
            {
                // Draw dark squares over the entire board.
                g.FillRectangle(darkSquareBrush, boardRectangle);

                // Draw light squares by excluding the dark squares, and then filling up what's left.
                int doubleDelta = delta * 2;
                int y = borderWidth;
                for (int yIndex = boardSizeMinusOne; yIndex >= 0; --yIndex)
                {
                    // Create block pattern by starting at logical coordinate 0 or 1 depending on the y-index.
                    int x = borderWidth + (yIndex & 1) * delta;
                    for (int xIndex = boardSizeMinusOne / 2; xIndex >= 0; --xIndex)
                    {
                        g.ExcludeClip(new Rectangle(x, y, squareSize, squareSize));
                        x += doubleDelta;
                    }
                    y += delta;
                }
                g.FillRectangle(lightSquareBrush, boardRectangle);
                g.ResetClip();

                // Draw foreground images.
                // Use ForegroundImagePadding to determine the amount of space around a foreground image within a square.
                var padding = ForegroundImagePadding;
                int sizeH = squareSize - padding.Horizontal,
                    sizeV = squareSize - padding.Vertical;

                if (sizeH > 0 && sizeV > 0)
                {
                    int hOffset = borderWidth + padding.Left,
                        vOffset = borderWidth + padding.Top;

                    // Loop over foreground images and draw them.
                    y = vOffset;
                    int index = 0;
                    for (int j = boardSizeMinusOne; j >= 0; --j)
                    {
                        int x = hOffset;
                        for (int k = boardSizeMinusOne; k >= 0; --k)
                        {
                            // Select picture.
                            Image currentImg = foregroundImages[index];
                            if (currentImg != null)
                            {
                                // Copy image to the graphics.
                                g.DrawImage(currentImg, new Rectangle(x, y, sizeH, sizeV));
                            }
                            x += delta;
                            ++index;
                        }
                        y += delta;
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // To dispose of stored disposable values such as brushes.
                propertyStore.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

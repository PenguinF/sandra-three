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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    public class PlayingBoard
        : Control
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
            updateDarkSquareBrush();
            updateLightSquareBrush();
        }

        private readonly Dictionary<string, object> propertyStore = new Dictionary<string, object>
        {
            { nameof(BoardSize), DefaultBoardSize },
            { nameof(DarkSquareColor), DefaultDarkSquareColor },
            { nameof(LightSquareColor), DefaultLightSquareColor },
            { nameof(SizeToFit), DefaultSizeToFit },
            { nameof(SquareSize), DefaultSquareSize },
        };


        /// <summary>
        /// Gets the default value for the <see cref="BoardSize"/> property.
        /// </summary>
        public const int DefaultBoardSize = 8;

        /// <summary>
        /// Gets or sets the number of squares in a row and file.
        /// The default value is <see cref="DefaultBoardSize"/> (8).
        /// </summary>
        [DefaultValue(DefaultBoardSize)]
        public int BoardSize
        {
            get { return (int)propertyStore[nameof(BoardSize)]; }
            set
            {
                // A board size of 0 will cause a division by zero error if SizeToFit is true.
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(BoardSize), value, "Board size must be 1 or higher.");
                }
                if (value != BoardSize)
                {
                    propertyStore[nameof(BoardSize)] = value;
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
        /// The default value is <see cref="Color.Brown"/>.
        /// </summary>
        public Color DarkSquareColor
        {
            get { return (Color)propertyStore[nameof(DarkSquareColor)]; }
            set
            {
                if (DarkSquareColor != value)
                {
                    propertyStore[nameof(DarkSquareColor)] = value;
                    updateDarkSquareBrush();
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
        /// The default value is <see cref="Color.SandyBrown"/>.
        /// </summary>
        public Color LightSquareColor
        {
            get { return (Color)propertyStore[nameof(LightSquareColor)]; }
            set
            {
                if (LightSquareColor != value)
                {
                    propertyStore[nameof(LightSquareColor)] = value;
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
        /// Gets or sets if <see cref="SquareSize"/> is automatically adjusted to fit the control's client area for as much as possible.
        /// The default value is <see cref="DefaultSizeToFit"/> (true).
        /// </summary>
        [DefaultValue(DefaultSizeToFit)]
        public bool SizeToFit
        {
            get { return (bool)propertyStore[nameof(SizeToFit)]; }
            set
            {
                if (value != SizeToFit)
                {
                    propertyStore[nameof(SizeToFit)] = value;
                    if (value) performSizeToFit();
                    Invalidate();
                }
            }
        }


        /// <summary>
        /// Gets the default value for the <see cref="SquareSize"/> property.
        /// </summary>
        public const int DefaultSquareSize = 44;

        /// <summary>
        /// Gets or sets the size of either side of a single square on the board.
        /// The default value is <see cref="DefaultSquareSize"/> (44).
        /// </summary>
        [DefaultValue(DefaultSquareSize)]
        public int SquareSize
        {
            get { return (int)propertyStore[nameof(SquareSize)]; }
            set
            {
                // Never allow zero square sizes - will cause division by zero errors.
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(SquareSize), value, "Square size must be 1 or higher.");
                }
                if (value != SquareSize)
                {
                    propertyStore[nameof(SquareSize)] = value;
                    Invalidate();
                }
            }
        }


        private Brush backgroundBrush;

        private void resetBackgroundBrush()
        {
            if (backgroundBrush != null) backgroundBrush.Dispose();
            backgroundBrush = null;
        }

        private void updateBackgroundBrush()
        {
            resetBackgroundBrush();
            backgroundBrush = new SolidBrush(BackColor);
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            updateBackgroundBrush();
            Invalidate();
        }


        private Brush darkSquareBrush;

        private void resetDarkSquareBrush()
        {
            if (darkSquareBrush != null) darkSquareBrush.Dispose();
            darkSquareBrush = null;
        }

        private void updateDarkSquareBrush()
        {
            resetDarkSquareBrush();
            darkSquareBrush = new SolidBrush(DarkSquareColor);
        }


        private Brush lightSquareBrush;

        private void resetLightSquareBrush()
        {
            if (lightSquareBrush != null) lightSquareBrush.Dispose();
            lightSquareBrush = null;
        }

        private void updateLightSquareBrush()
        {
            if (lightSquareBrush != null) lightSquareBrush.Dispose();
            lightSquareBrush = new SolidBrush(LightSquareColor);
        }


        private int squareSizeFromClientSize(int clientSize)
        {
            return clientSize / BoardSize;
        }

        private void performSizeToFit()
        {
            // Resize the board so that it is as large as possible while still fitting on the window.
            // Do this by adjusting the square size.
            int minSize = Math.Min(ClientSize.Height, ClientSize.Width);
            int newSquareSize = squareSizeFromClientSize(minSize);
            SquareSize = Math.Max(1, newSquareSize);
        }

        protected override void OnLayout(LayoutEventArgs args)
        {
            base.OnLayout(args);

            // Choose client size.
            if (SizeToFit) performSizeToFit();
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
            int totalBoardSize = squareSize * boardSize;
            Rectangle clipRectangle = pe.ClipRectangle;
            Rectangle boardRectangle = new Rectangle(0, 0, totalBoardSize, totalBoardSize);

            g.ExcludeClip(boardRectangle);
            if (!g.IsVisibleClipEmpty) g.FillRectangle(backgroundBrush, ClientRectangle);
            g.ResetClip();

            if (clipRectangle.IntersectsWith(boardRectangle))
            {
                // Draw dark squares over the entire board.
                g.FillRectangle(darkSquareBrush, boardRectangle);

                // Draw light squares by excluding the dark squares, and then filling up what's left.
                int doubleDelta = squareSize * 2;
                int y = 0;
                for (int yIndex = boardSizeMinusOne; yIndex >= 0; --yIndex)
                {
                    // Create block pattern by starting at logical coordinate 0 or 1 depending on the y-index.
                    int x = (yIndex & 1) * squareSize;
                    for (int xIndex = boardSizeMinusOne / 2; xIndex >= 0; --xIndex)
                    {
                        g.ExcludeClip(new Rectangle(x, y, squareSize, squareSize));
                        x += doubleDelta;
                    }
                    y += squareSize;
                }
                g.FillRectangle(lightSquareBrush, boardRectangle);
                g.ResetClip();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                resetBackgroundBrush();
                resetLightSquareBrush();
                resetDarkSquareBrush();
            }
            base.Dispose(disposing);
        }
    }
}

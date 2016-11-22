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
            { nameof(BoardHeight), DefaultBoardHeight },
            { nameof(BoardWidth), DefaultBoardWidth },
            { nameof(BorderColor), DefaultBorderColor },
            { nameof(BorderWidth), DefaultBorderWidth },
            { nameof(DarkSquareColor), DefaultDarkSquareColor },
            { nameof(DarkSquareImage), null },
            { nameof(ForegroundImagePadding), DefaultForegroundImagePadding },
            { nameof(ForegroundImageRelativeSize), DefaultForegroundImageRelativeSize },
            { nameof(InnerSpacing), DefaultInnerSpacing },
            { nameof(LightSquareColor), DefaultLightSquareColor },
            { nameof(LightSquareImage), null },
            { nameof(MovingImage), null },
            { nameof(SizeToFit), DefaultSizeToFit },
            { nameof(SquareSize), DefaultSquareSize },

            { nameof(backgroundBrush), null },
            { nameof(borderBrush), null },
            { nameof(darkSquareBrush), null },
            { nameof(lightSquareBrush), null },
        };


        /// <summary>
        /// Gets the default value for the <see cref="BoardHeight"/> property.
        /// </summary>
        public const int DefaultBoardHeight = 8;

        /// <summary>
        /// Gets or sets the number of squares in a file. The minimum value is 1.
        /// The default value is <see cref="DefaultBoardHeight"/> (8).
        /// </summary>
        [DefaultValue(DefaultBoardHeight)]
        public int BoardHeight
        {
            get { return propertyStore.Get<int>(nameof(BoardHeight)); }
            set
            {
                // A board height of 0 will cause a division by zero error if SizeToFit is true.
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(BoardHeight), value, "Board height must be 1 or higher.");
                }
                if (propertyStore.Set(nameof(BoardHeight), value))
                {
                    updateSquareArrays();
                    verifySizeToFit();
                    Invalidate();
                }
            }
        }


        /// <summary>
        /// Gets the default value for the <see cref="BoardWidth"/> property.
        /// </summary>
        public const int DefaultBoardWidth = 8;

        /// <summary>
        /// Gets or sets the number of squares in a rank. The minimum value is 1.
        /// The default value is <see cref="DefaultBoardWidth"/> (8).
        /// </summary>
        [DefaultValue(DefaultBoardWidth)]
        public int BoardWidth
        {
            get { return propertyStore.Get<int>(nameof(BoardWidth)); }
            set
            {
                // A board width of 0 will cause a division by zero error if SizeToFit is true.
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(BoardWidth), value, "Board width must be 1 or higher.");
                }
                if (propertyStore.Set(nameof(BoardWidth), value))
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
        public static Color DefaultDarkSquareColor { get { return Color.LightBlue; } }

        /// <summary>
        /// Gets or sets the color of dark squares.
        /// The default value is <see cref="DefaultDarkSquareColor"/> (<see cref="Color.LightBlue"/>).
        /// If an image is specified for <see cref="DarkSquareImage"/>, this property is ignored.
        /// </summary>
        public Color DarkSquareColor
        {
            get { return propertyStore.Get<Color>(nameof(DarkSquareColor)); }
            set
            {
                if (propertyStore.Set(nameof(DarkSquareColor), value))
                {
                    if (DarkSquareImage == null)
                    {
                        updateDarkSquareBrush();
                        Invalidate();
                    }
                }
            }
        }


        /// <summary>
        /// Gets or sets the image background for dark squares.
        /// </summary>
        public Image DarkSquareImage
        {
            get { return propertyStore.Get<Image>(nameof(DarkSquareImage)); }
            set
            {
                if (propertyStore.Set(nameof(DarkSquareImage), value))
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
        /// Gets the default value for the <see cref="LightSquareColor"/> property.
        /// </summary>
        public static Color DefaultLightSquareColor { get { return Color.Azure; } }

        /// <summary>
        /// Gets or sets the color of light squares.
        /// The default value is <see cref="DefaultLightSquareColor"/> (<see cref="Color.Azure"/>).
        /// If an image is specified for <see cref="LightSquareImage"/>, this property is ignored.
        /// </summary>
        public Color LightSquareColor
        {
            get { return propertyStore.Get<Color>(nameof(LightSquareColor)); }
            set
            {
                if (propertyStore.Set(nameof(LightSquareColor), value))
                {
                    if (LightSquareImage == null)
                    {
                        updateLightSquareBrush();
                        Invalidate();
                    }
                }
            }
        }


        /// <summary>
        /// Gets or sets the image background for light squares.
        /// </summary>
        public Image LightSquareImage
        {
            get { return propertyStore.Get<Image>(nameof(LightSquareImage)); }
            set
            {
                if (propertyStore.Set(nameof(LightSquareImage), value))
                {
                    updateLightSquareBrush();
                    Invalidate();
                }
            }
        }


        /// <summary>
        /// Gets or sets the image to display under the mouse pointer when moving.
        /// A null value (Nothing in Visual Basic) means that the image of the move's start square is used.
        /// </summary>
        public Image MovingImage
        {
            get { return propertyStore.Get<Image>(nameof(MovingImage)); }
            set
            {
                if (propertyStore.Set(nameof(MovingImage), value))
                {
                    if (IsMoving) Invalidate();
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
            get { return propertyStore.GetOwnedDisposable<Brush>(nameof(backgroundBrush)); }
            set { propertyStore.SetOwnedDisposable(nameof(backgroundBrush), value); }
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
            get { return propertyStore.GetOwnedDisposable<Brush>(nameof(borderBrush)); }
            set { propertyStore.SetOwnedDisposable(nameof(borderBrush), value); }
        }

        private void updateBorderBrush()
        {
            borderBrush = new SolidBrush(BorderColor);
        }


        private Brush darkSquareBrush
        {
            get { return propertyStore.GetOwnedDisposable<Brush>(nameof(darkSquareBrush)); }
            set { propertyStore.SetOwnedDisposable(nameof(darkSquareBrush), value); }
        }

        private void updateDarkSquareBrush()
        {
            if (DarkSquareImage != null)
            {
                darkSquareBrush = new TextureBrush(DarkSquareImage, WrapMode.Tile);
            }
            else
            {
                darkSquareBrush = new SolidBrush(DarkSquareColor);
            }
        }


        private Brush lightSquareBrush
        {
            get { return propertyStore.GetOwnedDisposable<Brush>(nameof(lightSquareBrush)); }
            set { propertyStore.SetOwnedDisposable(nameof(lightSquareBrush), value); }
        }

        private void updateLightSquareBrush()
        {
            if (LightSquareImage != null)
            {
                lightSquareBrush = new TextureBrush(LightSquareImage, WrapMode.Tile);
            }
            else
            {
                lightSquareBrush = new SolidBrush(LightSquareColor);
            }
        }


        private Image[] foregroundImages;

        /// <summary>
        /// Gets the <see cref="Image"/> on position (x, y).
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="x"/> or <paramref name="y"/> are smaller than 0 or greater than or equal to <see cref="BoardWidth"/> or <see cref="BoardHeight"/> respectively.
        /// </exception>
        public Image GetForegroundImage(int x, int y)
        {
            int index = getIndex(x, y);
            return foregroundImages[index];
        }

        /// <summary>
        /// Gets the <see cref="Image"/> on position (x, y).
        /// </summary>
        /// <param name="squareLocation">
        /// The location (x, y) of the square.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="squareLocation"/> is null (Nothing in Visual Basic).
        /// </exception>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="squareLocation.X"/> or <paramref name="squareLocation.Y"/> are smaller than 0 or greater than or equal to <see cref="BoardWidth"/> or <see cref="BoardHeight"/> respectively.
        /// </exception>
        public Image GetForegroundImage(SquareLocation squareLocation)
        {
            throwIfNull(squareLocation);
            return GetForegroundImage(squareLocation.X, squareLocation.Y);
        }

        /// <summary>
        /// Sets the <see cref="Image"/> on position (x, y).
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="x"/> or <paramref name="y"/> are smaller than 0 or greater than or equal to <see cref="BoardWidth"/> or <see cref="BoardHeight"/> respectively.
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

        /// <summary>
        /// Sets the <see cref="Image"/> on position (x, y).
        /// </summary>
        /// <param name="squareLocation">
        /// The location (x, y) of the square.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="squareLocation"/> is null (Nothing in Visual Basic).
        /// </exception>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="squareLocation.X"/> or <paramref name="squareLocation.Y"/> are smaller than 0 or greater than or equal to <see cref="BoardWidth"/> or <see cref="BoardHeight"/> respectively.
        /// </exception>
        public void SetForegroundImage(SquareLocation squareLocation, Image value)
        {
            throwIfNull(squareLocation);
            SetForegroundImage(squareLocation.X, squareLocation.Y, value);
        }


        private bool[] isImageHighlighted;

        /// <summary>
        /// Gets if the <see cref="Image"/> on position (x, y) is highlighted or not.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="x"/> or <paramref name="y"/> are smaller than 0 or greater than or equal to <see cref="BoardWidth"/> or <see cref="BoardHeight"/> respectively.
        /// </exception>
        public bool GetIsImageHighLighted(int x, int y)
        {
            int index = getIndex(x, y);
            return isImageHighlighted[index];
        }

        /// <summary>
        /// Gets if the <see cref="Image"/> on position (x, y) is highlighted or not.
        /// </summary>
        /// <param name="squareLocation">
        /// The location (x, y) of the square.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="squareLocation"/> is null (Nothing in Visual Basic).
        /// </exception>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="squareLocation.X"/> or <paramref name="squareLocation.Y"/> are smaller than 0 or greater than or equal to <see cref="BoardWidth"/> or <see cref="BoardHeight"/> respectively.
        /// </exception>
        public bool GetIsImageHighLighted(SquareLocation squareLocation)
        {
            throwIfNull(squareLocation);
            return GetIsImageHighLighted(squareLocation.X, squareLocation.Y);
        }

        /// <summary>
        /// Sets if the <see cref="Image"/> on position (x, y) is highlighted or not.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="x"/> or <paramref name="y"/> are smaller than 0 or greater than or equal to <see cref="BoardWidth"/> or <see cref="BoardHeight"/> respectively.
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

        /// <summary>
        /// Sets if the <see cref="Image"/> on position (x, y) is highlighted or not.
        /// </summary>
        /// <param name="squareLocation">
        /// The location (x, y) of the square.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="squareLocation"/> is null (Nothing in Visual Basic).
        /// </exception>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="squareLocation.X"/> or <paramref name="squareLocation.Y"/> are smaller than 0 or greater than or equal to <see cref="BoardWidth"/> or <see cref="BoardHeight"/> respectively.
        /// </exception>
        public void SetIsImageHighLighted(SquareLocation squareLocation, bool value)
        {
            throwIfNull(squareLocation);
            SetIsImageHighLighted(squareLocation.X, squareLocation.Y, value);
        }


        private Color[] squareOverlayColors;

        /// <summary>
        /// Gets an overlay color for the square on position (x, y).
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="x"/> or <paramref name="y"/> are smaller than 0 or greater than or equal to <see cref="BoardWidth"/> or <see cref="BoardHeight"/> respectively.
        /// </exception>
        public Color GetSquareOverlayColor(int x, int y)
        {
            int index = getIndex(x, y);
            return squareOverlayColors[index];
        }

        /// <summary>
        /// Gets an overlay color for the square on position (x, y).
        /// </summary>
        /// <param name="squareLocation">
        /// The location (x, y) of the square.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="squareLocation"/> is null (Nothing in Visual Basic).
        /// </exception>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="squareLocation.X"/> or <paramref name="squareLocation.Y"/> are smaller than 0 or greater than or equal to <see cref="BoardWidth"/> or <see cref="BoardHeight"/> respectively.
        /// </exception>
        public Color GetSquareOverlayColor(SquareLocation squareLocation)
        {
            throwIfNull(squareLocation);
            return GetSquareOverlayColor(squareLocation.X, squareLocation.Y);
        }

        /// <summary>
        /// Sets an overlay color for the square on position (x, y).
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="x"/> or <paramref name="y"/> are smaller than 0 or greater than or equal to <see cref="BoardWidth"/> or <see cref="BoardHeight"/> respectively.
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

        /// <summary>
        /// Sets an overlay color for the square on position (x, y).
        /// </summary>
        /// <param name="squareLocation">
        /// The location (x, y) of the square.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="squareLocation"/> is null (Nothing in Visual Basic).
        /// </exception>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="squareLocation.X"/> or <paramref name="squareLocation.Y"/> are smaller than 0 or greater than or equal to <see cref="BoardWidth"/> or <see cref="BoardHeight"/> respectively.
        /// </exception>
        public void SetSquareOverlayColor(SquareLocation squareLocation, Color value)
        {
            throwIfNull(squareLocation);
            SetSquareOverlayColor(squareLocation.X, squareLocation.Y, value);
        }


        private void updateSquareArrays()
        {
            int newArrayLength = BoardWidth * BoardHeight;
            foregroundImages = new Image[newArrayLength];
            isImageHighlighted = new bool[newArrayLength];
            squareOverlayColors = new Color[newArrayLength];
        }


        /// <summary>
        /// Enumerates all available <see cref="SquareLocation"/>s on the board.
        /// </summary>
        [Browsable(false)]
        public IEnumerable<SquareLocation> AllSquareLocations
        {
            get
            {
                for (int x = 0; x < BoardWidth; ++x)
                {
                    for (int y = 0; y < BoardHeight; ++y)
                    {
                        yield return new SquareLocation(x, y);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the location of the square the mouse pointer is located,
        /// or null (Nothing in Visual Basic) if the mouse pointer is located outside the control's bounds or above a border.
        /// </summary>
        [Browsable(false)]
        public SquareLocation HoverSquare
        {
            get
            {
                return getSquareLocation(hoveringSquareIndex);
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
                return moveStartSquareIndex >= 0;
            }
        }

        /// <summary>
        /// Gets the <see cref="Rectangle"/> for the square on position (x, y) in coordinates relative to the top left corner of the control.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="x"/> or <paramref name="y"/> are smaller than 0 or greater than or equal to <see cref="BoardWidth"/> or <see cref="BoardHeight"/> respectively.
        /// </exception>
        public Rectangle GetSquareRectangle(int x, int y)
        {
            throwIfOutOfRange(x, y);
            int delta = SquareSize + InnerSpacing;
            int px = BorderWidth + x * delta,
                py = BorderWidth + y * delta;
            return new Rectangle(px, py, SquareSize, SquareSize);
        }

        /// <summary>
        /// Gets the <see cref="Rectangle"/> for the square on position (x, y) in coordinates relative to the top left corner of the control.
        /// </summary>
        /// <param name="squareLocation">
        /// The location (x, y) of the square.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="squareLocation"/> is null (Nothing in Visual Basic).
        /// </exception>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="squareLocation.X"/> or <paramref name="squareLocation.Y"/> are smaller than 0 or greater than or equal to <see cref="BoardWidth"/> or <see cref="BoardHeight"/> respectively.
        /// </exception>
        public Rectangle GetSquareRectangle(SquareLocation squareLocation)
        {
            throwIfNull(squareLocation);
            return GetSquareRectangle(squareLocation.X, squareLocation.Y);
        }

        /// <summary>
        /// Gets the location of the square where the current move started if <see cref="IsMoving"/> is true,
        /// or null (Nothing in Visual Basic) if no move is currently being performed.
        /// </summary>
        [Browsable(false)]
        public SquareLocation MoveStartSquare
        {
            get
            {
                return getSquareLocation(moveStartSquareIndex);
            }
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
            OnMouseEnterSquare(new SquareEventArgs(getSquareLocation(squareIndex)));
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
            OnMouseLeaveSquare(new SquareEventArgs(getSquareLocation(squareIndex)));
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
            OnMoveCancel(new MoveEventArgs(getSquareLocation(squareIndex)));
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
            OnMoveCommit(new MoveCommitEventArgs(getSquareLocation(sourceSquareIndex),
                                                 getSquareLocation(targetSquareIndex)));
        }


        /// <summary>
        /// Occurs when an image occupying a square starts being moved.
        /// </summary>
        public event EventHandler<CancellableMoveEventArgs> MoveStart;

        /// <summary>
        /// Raises the <see cref="MoveStart"/> event. 
        /// </summary>
        protected virtual void OnMoveStart(CancellableMoveEventArgs e)
        {
            MoveStart?.Invoke(this, e);
        }

        protected bool RaiseMoveStart(int squareIndex)
        {
            var e = new CancellableMoveEventArgs(getSquareLocation(squareIndex));
            OnMoveStart(e);
            return !e.Cancel;
        }


        private int getX(int index) { return index % BoardWidth; }
        private int getY(int index) { return index / BoardWidth; }

        private SquareLocation getSquareLocation(int index)
        {
            if (index < 0) return null;
            return new SquareLocation(getX(index), getY(index));
        }

        private void throwIfNull(SquareLocation squareLocation)
        {
            if (squareLocation == null) throw new ArgumentNullException(nameof(squareLocation));
        }

        private void throwIfOutOfRange(int x, int y)
        {
            if (x < 0 || x >= BoardWidth) throw new IndexOutOfRangeException(nameof(x));
            if (y < 0 || y >= BoardHeight) throw new IndexOutOfRangeException(nameof(y));
        }

        private int getIndex(int x, int y)
        {
            throwIfOutOfRange(x, y);
            return y * BoardWidth + x;
        }

        private Point getLocationFromIndex(int index)
        {
            if (index < 0 || index >= BoardWidth * BoardHeight)
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

        private int maxSquareSize(Size clientSize)
        {
            int totalBorderWidth = BorderWidth * 2;
            int squareSizeHrz = (clientSize.Width - InnerSpacing * (BoardWidth - 1) - totalBorderWidth) / BoardWidth;
            int squareSizeVrt = (clientSize.Height - InnerSpacing * (BoardHeight - 1) - totalBorderWidth) / BoardHeight;
            return Math.Max(Math.Min(squareSizeHrz, squareSizeVrt), 0);
        }

        private void performSizeToFit()
        {
            // Resize the squares so that it is as large as possible while still fitting in the client area.
            int newSquareSize = maxSquareSize(ClientSize);
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
        public Size GetClosestAutoFitSize(Size maxBounds)
        {
            int squareSize = maxSquareSize(maxBounds);

            // Go back to a client size by inverting squareSizeFromClientSize().
            int targetWidth = squareSize * BoardWidth + InnerSpacing * (BoardWidth - 1) + BorderWidth * 2;
            int targetHeight = squareSize * BoardHeight + InnerSpacing * (BoardHeight - 1) + BorderWidth * 2;
            return new Size(targetWidth, targetHeight);
        }


        private Point lastKnownMouseMovePoint = new Point(-1, -1);

        private int hoveringSquareIndex = -1;

        private Point moveStartPosition;
        private Point moveCurrentPosition;
        private int moveStartSquareIndex = -1;

        private int hitTest(Point clientLocation)
        {
            int squareSize = SquareSize;

            if (squareSize == 0)
            {
                // No square can contain the point.
                // Short-circuit exit here to prevent division by zeroes.
                return -1;
            }

            int borderWidth = BorderWidth;

            int px = clientLocation.X - borderWidth,
                py = clientLocation.Y - borderWidth,
                delta = squareSize + InnerSpacing;

            // Need to use a conditional expression because e.g. -1/2 == 0.
            int x = px < 0 ? -1 : px / delta,
                y = py < 0 ? -1 : py / delta,
                remainderX = px % delta,
                remainderY = py % delta;

            int hit;
            if (x < 0 || x >= BoardWidth || y < 0 || y >= BoardHeight || remainderX >= squareSize || remainderY >= squareSize)
            {
                // Either outside of the actual board, or hitting a border.
                hit = -1;
            }
            else
            {
                // The location is inside a square.
                hit = y * BoardWidth + x;
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
            if (e.Button == MouseButtons.Left && !IsMoving)
            {
                // Only start when a square is hit.
                if (hit >= 0 && foregroundImages[hit] != null)
                {
                    if (RaiseMoveStart(hit))
                    {
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
            hitTest(e.Location);

            // Update moving information.
            if (IsMoving)
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
            if (moveStartSquareIndex >= 0)
            {
                if (hoveringSquareIndex >= 0)
                {
                    RaiseMoveCommit(moveStartSquareIndex, hoveringSquareIndex);
                }
                else
                {
                    RaiseMoveCancel(moveStartSquareIndex);
                }

                // End of move.
                moveStartSquareIndex = -1;

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
            int boardWidth = BoardWidth;
            int boardHeight = BoardHeight;
            int squareSize = SquareSize;
            int innerSpacing = InnerSpacing;
            int delta = squareSize + innerSpacing;
            int borderWidth = BorderWidth;
            int totalBoardWidth = delta * boardWidth - innerSpacing;
            int totalBoardHeight = delta * boardHeight - innerSpacing;

            Rectangle clipRectangle = pe.ClipRectangle;
            Rectangle boardRectangle = new Rectangle(borderWidth, borderWidth, totalBoardWidth, totalBoardHeight);
            Rectangle boardWithBorderRectangle = new Rectangle(0, 0, borderWidth * 2 + totalBoardWidth, borderWidth * 2 + totalBoardHeight);

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
                for (int yIndex = boardHeight - 1; yIndex >= 0; --yIndex)
                {
                    // Create block pattern by starting at logical coordinate 0 or 1 depending on the y-index.
                    int x = borderWidth + (yIndex & 1) * delta;
                    for (int xIndex = (boardWidth - 1) / 2; xIndex >= 0; --xIndex)
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
                    for (int j = 0; j < boardHeight; ++j)
                    {
                        int x = borderWidth;
                        for (int k = 0; k < boardWidth; ++k)
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
                    for (int j = 0; j < boardHeight; ++j)
                    {
                        int x = hOffset;
                        for (int k = 0; k < boardWidth; ++k)
                        {
                            // Select picture.
                            Image currentImg = foregroundImages[index];
                            if (currentImg != null)
                            {
                                // Draw current image - but use a color transformation if the current square was
                                // used to start moving from, or if the image must be highlighted.
                                if (index == moveStartSquareIndex)
                                {
                                    // Half-transparent.
                                    g.DrawImage(currentImg,
                                                new Rectangle(x, y, sizeH, sizeV),
                                                0, 0, currentImg.Width, currentImg.Height,
                                                GraphicsUnit.Pixel,
                                                moveSourceImageAttributes);
                                }
                                else
                                {
                                    drawForegroundImage(g, currentImg,
                                                        new Rectangle(x, y, sizeH, sizeV),
                                                        isImageHighlighted[index]);
                                }
                            }
                            x += delta;
                            ++index;
                        }
                        y += delta;
                    }
                }

                // Apply square highlights.
                for (int index = 0; index < boardWidth * boardHeight; ++index)
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

                base.OnPaint(pe);

                if (sizeH > 0 && sizeV > 0 && moveStartSquareIndex >= 0)
                {
                    // Draw moving image on top of the rest.
                    Image currentImg = MovingImage ?? foregroundImages[moveStartSquareIndex];
                    if (currentImg != null)
                    {
                        Point location = moveCurrentPosition;
                        location.Offset(moveStartPosition);
                        g.DrawImage(currentImg, new Rectangle(location.X, location.Y, sizeH, sizeV));
                    }
                }
            }
            else
            {
                base.OnPaint(pe);
            }
        }

        private void drawForegroundImage(Graphics g, Image image, Rectangle destinationRectangle, bool highlight)
        {
            if (highlight)
            {
                // Highlight piece.
                g.DrawImage(image,
                            destinationRectangle,
                            0, 0, image.Width, image.Height,
                            GraphicsUnit.Pixel,
                            highlightImgAttributes);
            }
            else
            {
                // Default case.
                g.DrawImage(image, destinationRectangle);
            }
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
    /// Represents the location of a square on a <see cref="PlayingBoard"/>. 
    /// </summary>
    public sealed class SquareLocation
    {
        /// <summary>
        /// Gets the x-coordinate of the square.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Gets the y-coordinate of the square.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Initializes a new instance of a square location.
        /// </summary>
        public SquareLocation(int x, int y)
        {
            X = x; Y = y;
        }

        private bool equals(SquareLocation other)
        {
            if (other == null) return false;
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return equals(obj as SquareLocation);
        }

        public override int GetHashCode()
        {
            // Rely on hash code generation of the built-in .NET library.
            return new Tuple<int, int>(X, Y).GetHashCode();
        }

        public static bool operator ==(SquareLocation left, SquareLocation right)
        {
            if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
            return left.equals(right);
        }

        public static bool operator !=(SquareLocation left, SquareLocation right)
        {
            if (ReferenceEquals(left, null)) return !ReferenceEquals(right, null);
            return !left.equals(right);
        }
    }
}

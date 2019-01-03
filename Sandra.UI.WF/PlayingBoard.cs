#region License
/*********************************************************************************
 * PlayingBoard.cs
 *
 * Copyright (c) 2004-2019 Henk Nicolai
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

using System;
using System.Collections.Generic;
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
    public class PlayingBoard : Control, IUIActionHandlerProvider
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

            UpdateSquareArrays();

            // Highlight by setting a gamma smaller than 1.
            var highlight = new ImageAttributes();
            highlight.SetGamma(0.6f);
            HighlightImageAttributes = highlight;

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
            HalfTransparentImageAttributes = halfTransparent;
        }

        /// <summary>
        /// Gets a reference to the <see cref="ImageAttributes"/> used for the <see cref="ForegroundImageAttribute.Highlight"/> effect.
        /// </summary>
        public ImageAttributes HighlightImageAttributes { get; }

        /// <summary>
        /// Gets a reference to the <see cref="ImageAttributes"/> used for the <see cref="ForegroundImageAttribute.HalfTransparent"/> effect.
        /// </summary>
        public ImageAttributes HalfTransparentImageAttributes { get; }

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
            { nameof(SizeToFit), DefaultSizeToFit },
            { nameof(SquareSize), DefaultSquareSize },
        };


        /// <summary>
        /// Gets the action handler for this control.
        /// </summary>
        public UIActionHandler ActionHandler { get; } = new UIActionHandler();


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
                    UpdateSquareArrays();
                    VerifySizeToFit();
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
                    UpdateSquareArrays();
                    VerifySizeToFit();
                    Invalidate();
                }
            }
        }


        /// <summary>
        /// Gets the default value for the <see cref="BorderColor"/> property.
        /// </summary>
        public static Color DefaultBorderColor => Color.Black;

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
                    VerifySizeToFit();
                    Invalidate();
                }
            }
        }


        /// <summary>
        /// Gets the default value for the <see cref="DarkSquareColor"/> property.
        /// </summary>
        public static Color DefaultDarkSquareColor => Color.LightBlue;

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
                    Invalidate();
                }
            }
        }


        /// <summary>
        /// Gets the default value for the <see cref="ForegroundImagePadding"/> property.
        /// </summary>
        public static Padding DefaultForegroundImagePadding => new Padding(0);

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
                    VerifySizeToFit();
                    Invalidate();
                }
            }
        }


        /// <summary>
        /// Gets the default value for the <see cref="LightSquareColor"/> property.
        /// </summary>
        public static Color DefaultLightSquareColor => Color.Azure;

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
                    VerifySizeToFit();
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


        protected override void OnBackColorChanged(EventArgs e)
        {
            Invalidate();
            base.OnBackColorChanged(e);
        }


        private Image[] foregroundImages;

        /// <summary>
        /// Gets the <see cref="Image"/> on position (x, y).
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="x"/> or <paramref name="y"/> are smaller than 0 or greater than or equal to <see cref="BoardWidth"/> or <see cref="BoardHeight"/> respectively.
        /// </exception>
        public Image GetForegroundImage(int x, int y) => foregroundImages[GetIndex(x, y)];

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
            ThrowIfNull(squareLocation);
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
            int index = GetIndex(x, y);
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
            ThrowIfNull(squareLocation);
            SetForegroundImage(squareLocation.X, squareLocation.Y, value);
        }


        private ForegroundImageAttribute[] foregroundImageAttributes;

        /// <summary>
        /// Gets the current <see cref="ForegroundImageAttribute"/> for the <see cref="Image"/> on position (x, y).
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="x"/> or <paramref name="y"/> are smaller than 0 or greater than or equal to <see cref="BoardWidth"/> or <see cref="BoardHeight"/> respectively.
        /// </exception>
        public ForegroundImageAttribute GetForegroundImageAttribute(int x, int y) => foregroundImageAttributes[GetIndex(x, y)];

        /// <summary>
        /// Gets the current <see cref="ForegroundImageAttribute"/> for the <see cref="Image"/> on position (x, y).
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
        public ForegroundImageAttribute GetForegroundImageAttribute(SquareLocation squareLocation)
        {
            ThrowIfNull(squareLocation);
            return GetForegroundImageAttribute(squareLocation.X, squareLocation.Y);
        }

        /// <summary>
        /// Sets the current <see cref="ForegroundImageAttribute"/> for the <see cref="Image"/> on position (x, y).
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="x"/> or <paramref name="y"/> are smaller than 0 or greater than or equal to <see cref="BoardWidth"/> or <see cref="BoardHeight"/> respectively.
        /// </exception>
        public void SetForegroundImageAttribute(int x, int y, ForegroundImageAttribute value)
        {
            int index = GetIndex(x, y);
            if (foregroundImageAttributes[index] != value)
            {
                foregroundImageAttributes[index] = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Sets the current <see cref="ForegroundImageAttribute"/> for the <see cref="Image"/> on position (x, y).
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
        public void SetForegroundImageAttribute(SquareLocation squareLocation, ForegroundImageAttribute value)
        {
            ThrowIfNull(squareLocation);
            SetForegroundImageAttribute(squareLocation.X, squareLocation.Y, value);
        }


        private Color[] squareOverlayColors;

        /// <summary>
        /// Gets an overlay color for the square on position (x, y).
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="x"/> or <paramref name="y"/> are smaller than 0 or greater than or equal to <see cref="BoardWidth"/> or <see cref="BoardHeight"/> respectively.
        /// </exception>
        public Color GetSquareOverlayColor(int x, int y) => squareOverlayColors[GetIndex(x, y)];

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
            ThrowIfNull(squareLocation);
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
            int index = GetIndex(x, y);
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
            ThrowIfNull(squareLocation);
            SetSquareOverlayColor(squareLocation.X, squareLocation.Y, value);
        }


        private void UpdateSquareArrays()
        {
            int newArrayLength = BoardWidth * BoardHeight;
            foregroundImages = new Image[newArrayLength];
            foregroundImageAttributes = new ForegroundImageAttribute[newArrayLength];
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
        public SquareLocation HoverSquare => GetSquareLocation(hoveringSquareIndex);

        /// <summary>
        /// Gets the <see cref="Rectangle"/> for the square on position (x, y) in coordinates relative to the top left corner of the control.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when either <paramref name="x"/> or <paramref name="y"/> are smaller than 0 or greater than or equal to <see cref="BoardWidth"/> or <see cref="BoardHeight"/> respectively.
        /// </exception>
        public Rectangle GetSquareRectangle(int x, int y)
        {
            ThrowIfOutOfRange(x, y);
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
            ThrowIfNull(squareLocation);
            return GetSquareRectangle(squareLocation.X, squareLocation.Y);
        }

        /// <summary>
        /// Returns the square that is located at the specified coordinates.
        /// </summary>
        /// <param name="point">
        /// A <see cref="Point"/> containing the coordinates relative to the upper left corner of the control.
        /// </param>
        /// <returns>
        /// The <see cref="SquareLocation"/> at the specified point, or null if there is none.
        /// </returns>
        public SquareLocation GetSquareLocation(Point point) => GetSquareLocation(GetSquareIndexFromLocation(point));


        /// <summary>
        /// Occurs when the mouse pointer enters a square.
        /// </summary>
        public event Action<PlayingBoard, SquareEventArgs> MouseEnterSquare;

        /// <summary>
        /// Raises the <see cref="MouseEnterSquare"/> event. 
        /// </summary>
        protected virtual void OnMouseEnterSquare(SquareEventArgs e) => MouseEnterSquare?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="MouseEnterSquare"/> event. 
        /// </summary>
        protected void RaiseMouseEnterSquare(int squareIndex) => OnMouseEnterSquare(new SquareEventArgs(GetSquareLocation(squareIndex)));


        /// <summary>
        /// Occurs when the mouse pointer leaves a square.
        /// </summary>
        public event Action<PlayingBoard, SquareEventArgs> MouseLeaveSquare;

        /// <summary>
        /// Raises the <see cref="MouseLeaveSquare"/> event. 
        /// </summary>
        protected virtual void OnMouseLeaveSquare(SquareEventArgs e) => MouseLeaveSquare?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="MouseLeaveSquare"/> event. 
        /// </summary>
        protected void RaiseMouseLeaveSquare(int squareIndex) => OnMouseLeaveSquare(new SquareEventArgs(GetSquareLocation(squareIndex)));


        /// <summary>
        /// Occurs when the mouse is over this control and one of its buttons is released.
        /// </summary>
        public event Action<PlayingBoard, SquareMouseEventArgs> SquareMouseUp;

        /// <summary>
        /// Raises the <see cref="SquareMouseUp"/> event. 
        /// </summary>
        protected virtual void OnSquareMouseUp(SquareMouseEventArgs e) => SquareMouseUp?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="SquareMouseUp"/> event. 
        /// </summary>
        protected void RaiseSquareMouseUp(int squareIndex, MouseButtons button, Point mouseLocation)
        {
            OnSquareMouseUp(new SquareMouseEventArgs(GetSquareLocation(squareIndex), button, mouseLocation));
        }


        /// <summary>
        /// Occurs when the mouse is over this control and one of its buttons is pressed.
        /// </summary>
        public event Action<PlayingBoard, SquareMouseEventArgs> SquareMouseDown;

        /// <summary>
        /// Raises the <see cref="SquareMouseDown"/> event. 
        /// </summary>
        protected virtual void OnSquareMouseDown(SquareMouseEventArgs e) => SquareMouseDown?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="SquareMouseDown"/> event. 
        /// </summary>
        protected void RaiseSquareMouseDown(int squareIndex, MouseButtons button, Point mouseLocation)
        {
            OnSquareMouseDown(new SquareMouseEventArgs(GetSquareLocation(squareIndex), button, mouseLocation));
        }


        private int GetX(int index) => index % BoardWidth;
        private int GetY(int index) => index / BoardWidth;

        private SquareLocation GetSquareLocation(int index) => index < 0 ? null
                                                             : new SquareLocation(GetX(index), GetY(index));

        private void ThrowIfNull(SquareLocation squareLocation)
        {
            if (squareLocation == null) throw new ArgumentNullException(nameof(squareLocation));
        }

        private void ThrowIfOutOfRange(int x, int y)
        {
            if (x < 0 || x >= BoardWidth) throw new IndexOutOfRangeException(nameof(x));
            if (y < 0 || y >= BoardHeight) throw new IndexOutOfRangeException(nameof(y));
        }

        private int GetIndex(int x, int y)
        {
            ThrowIfOutOfRange(x, y);
            return y * BoardWidth + x;
        }

        private Point GetLocationFromIndex(int index)
        {
            if (index < 0 || index >= BoardWidth * BoardHeight)
            {
                return Point.Empty;
            }

            int x = GetX(index),
                y = GetY(index),
                delta = SquareSize + InnerSpacing;
            int px = BorderWidth + x * delta,
                py = BorderWidth + y * delta;

            return new Point(px, py);
        }

        /// <summary>
        /// Returns the rectangle of a foreground image relative to its containing square.
        /// </summary>
        public Rectangle GetRelativeForegroundImageRectangle()
        {
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

        private int MaxSquareSize(Size clientSize)
        {
            int totalBorderWidth = BorderWidth * 2;
            int squareSizeHrz = (clientSize.Width - InnerSpacing * (BoardWidth - 1) - totalBorderWidth) / BoardWidth;
            int squareSizeVrt = (clientSize.Height - InnerSpacing * (BoardHeight - 1) - totalBorderWidth) / BoardHeight;
            return Math.Max(Math.Min(squareSizeHrz, squareSizeVrt), 0);
        }

        private void PerformSizeToFit()
        {
            // Resize the squares so that it is as large as possible while still fitting in the client area.
            int newSquareSize = MaxSquareSize(ClientSize);
            // Store directly in the property store, to bypass SizeToFit check.
            if (propertyStore.Set(nameof(SquareSize), newSquareSize))
            {
                Invalidate();
            }
        }

        private void VerifySizeToFit()
        {
            // Only conditionally perform size-to-fit.
            if (SizeToFit) PerformSizeToFit();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            VerifySizeToFit();
            base.OnLayout(levent);
        }

        /// <summary>
        /// Returns the size closest to the given size which will allow the board to fit exactly.
        /// </summary>
        public Size GetClosestAutoFitSize(Size maxBounds)
            => GetExactAutoFitSize(MaxSquareSize(maxBounds));

        /// <summary>
        /// Given a square size, returns the <see cref="Size"/> which will allow the board to fit exactly.
        /// </summary>
        public Size GetExactAutoFitSize(int squareSize)
        {
            int targetWidth = squareSize * BoardWidth + InnerSpacing * (BoardWidth - 1) + BorderWidth * 2;
            int targetHeight = squareSize * BoardHeight + InnerSpacing * (BoardHeight - 1) + BorderWidth * 2;
            return new Size(targetWidth, targetHeight);
        }


        private Point lastKnownMouseMovePoint = new Point(-1, -1);

        private int hoveringSquareIndex = -1;

        private int GetSquareIndexFromLocation(Point clientLocation)
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

            return hit;
        }

        private int HitTest(Point clientLocation)
        {
            int hit = GetSquareIndexFromLocation(clientLocation);

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
                HitTest(lastKnownMouseMovePoint);
            }

            base.OnMouseEnter(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            RaiseSquareMouseDown(HitTest(e.Location), e.Button, e.Location);

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            // Do a hit test, which updates hover information.
            HitTest(e.Location);

            // Remember position for mouse-enters without mouse-leaves.
            lastKnownMouseMovePoint = e.Location;

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            RaiseSquareMouseUp(HitTest(e.Location), e.Button, e.Location);

            base.OnMouseUp(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            // Hit test a position outside of the control to reset the hover square index and raise proper events.
            lastKnownMouseMovePoint = new Point(-1, -1);
            HitTest(lastKnownMouseMovePoint);

            base.OnMouseLeave(e);
        }

        /// <summary>
        /// Holds all GDI objects used during a single Paint event of this <see cref="PlayingBoard"/>.
        /// </summary>
        private sealed class GDIPaintResources : IDisposable
        {
            public Brush BackgroundBrush;
            public Brush BorderBrush;

            public Brush DarkSquareBrush;
            public Brush LightSquareBrush;

            private void ReleaseUnmanagedResources()
            {
                // Unmanaged resources: also dispose when finalizing.
                if (BackgroundBrush != null) BackgroundBrush.Dispose();
                if (BorderBrush != null) BorderBrush.Dispose();

                if (DarkSquareBrush != null) DarkSquareBrush.Dispose();
                if (LightSquareBrush != null) LightSquareBrush.Dispose();
            }

            public void Dispose()
            {
                ReleaseUnmanagedResources();
                GC.SuppressFinalize(this);
            }

            ~GDIPaintResources()
            {
                ReleaseUnmanagedResources();
            }
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

            using (GDIPaintResources gdi = new GDIPaintResources())
            {
                // Draw the background area not covered by the playing board.
                g.ExcludeClip(boardWithBorderRectangle);
                if (!g.IsVisibleClipEmpty)
                {
                    gdi.BackgroundBrush = new SolidBrush(BackColor);
                    g.FillRectangle(gdi.BackgroundBrush, ClientRectangle);
                }
                g.ResetClip();

                // Draw the background light and dark squares in a block pattern.
                // Use SmoothingMode.None so crisp edges are drawn for the squares.
                g.SmoothingMode = SmoothingMode.None;
                if (squareSize > 0 && clipRectangle.IntersectsWith(boardRectangle))
                {
                    Image darkSquareImage = DarkSquareImage;
                    Image lightSquareImage = LightSquareImage;

                    if (darkSquareImage == null) gdi.DarkSquareBrush = new SolidBrush(DarkSquareColor);
                    if (lightSquareImage == null) gdi.LightSquareBrush = new SolidBrush(LightSquareColor);

                    int y = borderWidth;
                    bool startWithDarkSquare = false;

                    for (int yIndex = 0; yIndex < boardHeight; ++yIndex)
                    {
                        bool drawDarkSquare = startWithDarkSquare;
                        int x = borderWidth;

                        for (int xIndex = 0; xIndex < boardWidth; ++xIndex)
                        {
                            // Draw either a light or a dark square depending on its location.
                            if (drawDarkSquare)
                            {
                                if (darkSquareImage != null)
                                {
                                    g.DrawImage(darkSquareImage, x, y, squareSize, squareSize);
                                }
                                else
                                {
                                    g.FillRectangle(gdi.DarkSquareBrush, x, y, squareSize, squareSize);
                                }
                            }
                            else
                            {
                                if (lightSquareImage != null)
                                {
                                    g.DrawImage(lightSquareImage, x, y, squareSize, squareSize);
                                }
                                else
                                {
                                    g.FillRectangle(gdi.LightSquareBrush, x, y, squareSize, squareSize);
                                }
                            }

                            drawDarkSquare = !drawDarkSquare;
                            x += delta;
                        }

                        startWithDarkSquare = !startWithDarkSquare;
                        y += delta;
                    }
                }
                g.SmoothingMode = SmoothingMode.AntiAlias;

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
                    gdi.BorderBrush = new SolidBrush(BorderColor);
                    g.FillRectangle(gdi.BorderBrush, boardWithBorderRectangle);
                    g.ResetClip();
                }

                if (squareSize > 0 && clipRectangle.IntersectsWith(boardRectangle))
                {
                    // Draw foreground images.
                    // Determine the image size and the amount of space around a foreground image within a square.
                    Rectangle imgRect = GetRelativeForegroundImageRectangle();
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
                                    DrawForegroundImage(g, currentImg,
                                                        new Rectangle(x, y, sizeH, sizeV),
                                                        foregroundImageAttributes[index]);
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
                            Point offset = GetLocationFromIndex(index);
                            // Draw overlay color on the square, with the already drawn foreground image.
                            using (var overlayBrush = new SolidBrush(squareOverlayColors[index]))
                            {
                                g.FillRectangle(overlayBrush, offset.X, offset.Y, squareSize, squareSize);
                            }
                        }
                    }
                }
            }

            base.OnPaint(pe);
        }

        private void DrawForegroundImage(Graphics g, Image image, Rectangle destinationRectangle, ForegroundImageAttribute imgAttribute)
        {
            if (imgAttribute == ForegroundImageAttribute.HalfTransparent)
            {
                // Half-transparent.
                g.DrawImage(image,
                            destinationRectangle,
                            0, 0, image.Width, image.Height,
                            GraphicsUnit.Pixel,
                            HalfTransparentImageAttributes);
            }
            else if (imgAttribute == ForegroundImageAttribute.Highlight)
            {
                // Highlight piece.
                g.DrawImage(image,
                            destinationRectangle,
                            0, 0, image.Width, image.Height,
                            GraphicsUnit.Pixel,
                            HighlightImageAttributes);
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
                HighlightImageAttributes.Dispose();
                HalfTransparentImageAttributes.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Enumerates options in which to draw a foreground image on the playing board.
    /// </summary>
    public enum ForegroundImageAttribute
    {
        /// <summary>
        /// The foreground image is drawn as is.
        /// </summary>
        Default,
        /// <summary>
        /// The foreground image is drawn with a highlight.
        /// </summary>
        Highlight,
        /// <summary>
        /// The foreground image is drawn half transparently.
        /// </summary>
        HalfTransparent,
    }

    /// <summary>
    /// Represents the location of a square on a <see cref="PlayingBoard"/>. 
    /// </summary>
    [DebuggerDisplay("x = {X}, y = {Y}")]
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

        private bool EqualTo(SquareLocation other) => other != null && X == other.X && Y == other.Y;

        public override bool Equals(object obj) => EqualTo(obj as SquareLocation);

        // Rely on hash code generation of the built-in .NET library.
        public override int GetHashCode() => new Tuple<int, int>(X, Y).GetHashCode();

        public static bool operator ==(SquareLocation left, SquareLocation right)
            => ReferenceEquals(left, null) ? ReferenceEquals(right, null)
             : left.EqualTo(right);

        public static bool operator !=(SquareLocation left, SquareLocation right)
            => ReferenceEquals(left, null) ? !ReferenceEquals(right, null)
             : !left.EqualTo(right);
    }
}

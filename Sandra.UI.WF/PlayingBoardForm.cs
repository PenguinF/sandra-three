/*********************************************************************************
 * PlayingBoardForm.cs
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
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Form which contains a playing board control, and which maintains its aspect ratio while resizing.
    /// </summary>
    public class PlayingBoardForm : SnappingMdiChildForm
    {
        /// <summary>
        /// Gets a reference to the playing board control on this form.
        /// </summary>
        public PlayingBoard PlayingBoard { get; }

        public PlayingBoardForm()
        {
            PlayingBoard = new PlayingBoard();
            PlayingBoard.Dock = DockStyle.Fill;
            PlayingBoard.Visible = true;
            Controls.Add(PlayingBoard);
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

            OnResizing(ref windowRect, ResizeMode.BottomRight);

            SetBoundsCore(windowRect.Left, windowRect.Top,
                          windowRect.Right - windowRect.Left, windowRect.Bottom - windowRect.Top,
                          BoundsSpecified.Size);
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
            // Calculate closest auto fit size given the client height and width that would result from performing the given resize.
            int targetWidth = PlayingBoard.GetClosestAutoFitSize(resizeRect.Right - resizeRect.Left - widthDifference);
            int targetHeight = PlayingBoard.GetClosestAutoFitSize(resizeRect.Bottom - resizeRect.Top - heightDifference);

            // Select target size and extra direction in which to grow/shrink for straight directions.
            int targetSize;
            switch (resizeMode)
            {
                case ResizeMode.Top:
                    resizeMode = ResizeMode.TopRight;
                    targetSize = targetHeight;
                    break;
                case ResizeMode.Bottom:
                    resizeMode = ResizeMode.BottomRight;
                    targetSize = targetHeight;
                    break;
                case ResizeMode.Left:
                    resizeMode = ResizeMode.BottomLeft;
                    targetSize = targetWidth;
                    break;
                case ResizeMode.Right:
                    resizeMode = ResizeMode.BottomRight;
                    targetSize = targetWidth;
                    break;
                default:
                    targetSize = Math.Min(targetHeight, targetWidth);
                    break;
            }

            // Left/right.
            switch (resizeMode)
            {
                case ResizeMode.TopLeft:
                case ResizeMode.BottomLeft:
                    // Adjust left edge.
                    resizeRect.Left = resizeRect.Right - targetSize - widthDifference;
                    break;
                case ResizeMode.TopRight:
                case ResizeMode.BottomRight:
                    // Adjust right edge.
                    resizeRect.Right = resizeRect.Left + targetSize + widthDifference;
                    break;
            }

            // Top/bottom.
            switch (resizeMode)
            {
                case ResizeMode.TopLeft:
                case ResizeMode.TopRight:
                    // Adjust top edge.
                    resizeRect.Top = resizeRect.Bottom - targetSize - heightDifference;
                    break;
                case ResizeMode.BottomLeft:
                case ResizeMode.BottomRight:
                    // Adjust bottom edge.
                    resizeRect.Bottom = resizeRect.Top + targetSize + heightDifference;
                    break;
            }
        }

        protected override void OnResizing(ref RECT resizeRect, ResizeMode resizeMode)
        {
            if (PlayingBoard.SizeToFit)
            {
                // Snap to auto-fit, instead of to other MDI children.
                performAutoFit(ref resizeRect, resizeMode);
            }
            else
            {
                base.OnResizing(ref resizeRect, resizeMode);
            }
        }
    }
}

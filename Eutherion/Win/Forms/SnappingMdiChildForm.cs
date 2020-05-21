#region License
/*********************************************************************************
 * SnappingMdiChildForm.cs
 *
 * Copyright (c) 2004-2020 Henk Nicolai
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

using Eutherion.Win.Native;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Eutherion.Win.Forms
{
    /// <summary>
    /// Windows <see cref="Form"/> which, if it's an MDI child, snaps to other MDI children and the edges of its parent MDI client area.
    /// </summary>
    public class SnappingMdiChildForm : ConstrainedMoveResizeForm
    {
        /// <summary>
        /// Gets the default value for the <see cref="MaxSnapDistance"/> property.
        /// </summary>
        public const int DefaultMaxSnapDistance = 4;

        /// <summary>
        /// Gets or sets the maximum distance between form edges within which they will be sensitive to snapping together. The default value is <see cref="DefaultMaxSnapDistance"/> (4).
        /// </summary>
        [DefaultValue(DefaultMaxSnapDistance)]
        public int MaxSnapDistance { get; set; } = DefaultMaxSnapDistance;

        /// <summary>
        /// Gets the default value for the <see cref="InsensitiveBorderEndLength"/> property.
        /// </summary>
        public const int DefaultInsensitiveBorderEndLength = 16;

        /// <summary>
        /// Gets or sets the length of the ends of the borders of this form that are insensitive to snapping. The default value is <see cref="DefaultInsensitiveBorderEndLength"/> (16).
        /// </summary>
        [DefaultValue(DefaultInsensitiveBorderEndLength)]
        public int InsensitiveBorderEndLength { get; set; } = DefaultInsensitiveBorderEndLength;

        // Size/move precalculated information.
        Rectangle m_currentMdiClientScreenRectangle;   // Current bounds of the MDI client rectangle. Changes during sizing/moving when scrollbars are shown or hidden.
        SnapGrid m_snapGrid;
        RECT m_rectangleBeforeSizeMove;                // Initial bounds of this window before sizing/moving was started. Used to preserve sizes or positions.

        /// <summary>
        /// Precalculates and caches line segments that this form can snap onto.
        /// </summary>
        SnapGrid PrepareSizeMove(Control.ControlCollection mdiChildren)
        {
            // Ignore the possibility that the MDI client rectangle is empty.
            RECT mdiClientRectangle = new RECT
            {
                Left = m_currentMdiClientScreenRectangle.Left,
                Right = m_currentMdiClientScreenRectangle.Right,
                Top = m_currentMdiClientScreenRectangle.Top,
                Bottom = m_currentMdiClientScreenRectangle.Bottom,
            };

            // Create a list of MDI child rectangles sorted by their z-order. (MDI child on top has index 0.)
            List<RECT> mdiChildRectangles = new List<RECT>();
            int mdiChildCount = mdiChildren.Count;
            for (int mdiChildIndex = 0; mdiChildIndex < mdiChildCount; ++mdiChildIndex)
            {
                Control child = mdiChildren[mdiChildIndex];
                if (child is Form mdiChildForm && mdiChildForm.Visible && mdiChildForm.WindowState == FormWindowState.Normal)
                {
                    // Convert the bounds of this MDI child to screen coordinates.
                    Rectangle mdiChildBounds = mdiChildForm.Bounds;
                    mdiChildBounds.Offset(mdiClientRectangle.Left, mdiClientRectangle.Top);
                    RECT mdiChildRectangle = new RECT
                    {
                        Left = mdiChildBounds.Left,
                        Right = mdiChildBounds.Right,
                        Top = mdiChildBounds.Top,
                        Bottom = mdiChildBounds.Bottom,
                    };

                    if (this != mdiChildForm)
                    {
                        // Intersect the MDI child rectangle with the MDI client rectangle, so this form does not snap to edges outside of the visible MDI client rectangle.
                        if (mdiChildRectangle.Left < mdiClientRectangle.Left) mdiChildRectangle.Left = mdiClientRectangle.Left;
                        if (mdiChildRectangle.Right > mdiClientRectangle.Right) mdiChildRectangle.Right = mdiClientRectangle.Right;
                        if (mdiChildRectangle.Top < mdiClientRectangle.Top) mdiChildRectangle.Top = mdiClientRectangle.Top;
                        if (mdiChildRectangle.Bottom > mdiClientRectangle.Bottom) mdiChildRectangle.Bottom = mdiClientRectangle.Bottom;

                        // Only add non-empty rectangles.
                        if (mdiChildRectangle.Left < mdiChildRectangle.Right && mdiChildRectangle.Top < mdiChildRectangle.Bottom) mdiChildRectangles.Add(mdiChildRectangle);
                    }
                    else
                    {
                        // Save original rectangle, so its properties can be used to keep positions or sizes constant during sizing and/or moving.
                        m_rectangleBeforeSizeMove = mdiChildRectangle;
                    }
                }
            }

            // Calculate snappable segments and save them to arrays which can be used efficiently from within the WndProc() override.
            List<LineSegment> verticalSegments = SnapGrid.GetVerticalEdges(ref mdiClientRectangle, 0);
            List<LineSegment> horizontalSegments = SnapGrid.GetHorizontalEdges(ref mdiClientRectangle, 0);

            // Add snap line segments for each MDI child.
            SnapGrid.AddVisibleSegments(verticalSegments, mdiChildRectangles, true, InsensitiveBorderEndLength);
            SnapGrid.AddVisibleSegments(horizontalSegments, mdiChildRectangles, false, InsensitiveBorderEndLength);

            return new SnapGrid(verticalSegments.ToArray(), horizontalSegments.ToArray());
        }

        /// <summary>
        /// Checks if line segments that this form can snap onto should be precalculated (again).
        /// </summary>
        void CheckUpdateSizeMove()
        {
            Form parentForm = MdiParent;
            if (parentForm != null)
            {
                foreach (Control c in parentForm.Controls)
                {
                    if (c is MdiClient)
                    {
                        // Get the bounds of the MDI client relative to the screen coordinates, because that is also how the size/move rectangles will be passed into WndProc().
                        Rectangle currentMdiClientScreenRectangle = c.RectangleToScreen(c.ClientRectangle);
                        if (m_snapGrid == null || currentMdiClientScreenRectangle != m_currentMdiClientScreenRectangle)
                        {
                            m_currentMdiClientScreenRectangle = currentMdiClientScreenRectangle;
                            m_snapGrid = PrepareSizeMove(c.Controls);
                        }
                        // No need to check 'other' MdiClients, so stop looping.
                        return;
                    }
                }
            }
        }

        protected override void OnResizeBegin(EventArgs e)
        {
            base.OnResizeBegin(e);

            // Precalculate size/move line segments.
            CheckUpdateSizeMove();
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            m_snapGrid = null;
            base.OnResizeEnd(e);
        }

        protected override void OnMoving(ref RECT moveRect)
        {
            if (m_snapGrid != null)
            {
                // Check if the MDI client rectangle changed during move/resize, because the client area may show or hide scrollbars during sizing/moving of this window.
                CheckUpdateSizeMove();

                // Evaluate left/right borders, then top/bottom borders.

                // Create line segments for each border of this window.
                LineSegment leftBorder = SnapGrid.LeftEdge(ref moveRect, InsensitiveBorderEndLength);
                LineSegment rightBorder = SnapGrid.RightEdge(ref moveRect, InsensitiveBorderEndLength);

                if (null != leftBorder && null != rightBorder)
                {
                    // Initialize snap threshold.
                    int snapThresholdX = MaxSnapDistance + 1;

                    // Preserve original width of the window.
                    int originalWidth = m_rectangleBeforeSizeMove.Right - m_rectangleBeforeSizeMove.Left;

                    // Check vertical segments to snap against.
                    foreach (LineSegment verticalSegment in m_snapGrid.VerticalSegments)
                    {
                        if (leftBorder.SnapSensitive(ref snapThresholdX, verticalSegment))
                        {
                            // Snap left border, preserve original width.
                            moveRect.Left = verticalSegment.Position;
                            moveRect.Right = verticalSegment.Position + originalWidth;
                        }
                        if (rightBorder.SnapSensitive(ref snapThresholdX, verticalSegment))
                        {
                            // Snap right border, preserve original width.
                            moveRect.Left = verticalSegment.Position - originalWidth;
                            moveRect.Right = verticalSegment.Position;
                        }
                    }
                }

                // Create line segments for each border of this window.
                LineSegment topBorder = SnapGrid.TopEdge(ref moveRect, InsensitiveBorderEndLength);
                LineSegment bottomBorder = SnapGrid.BottomEdge(ref moveRect, InsensitiveBorderEndLength);

                if (null != topBorder && null != bottomBorder)
                {
                    // Initialize snap threshold.
                    int snapThresholdY = MaxSnapDistance + 1;

                    // Preserve original height of the window.
                    int originalHeight = m_rectangleBeforeSizeMove.Bottom - m_rectangleBeforeSizeMove.Top;

                    // Check horizontal segments to snap against.
                    foreach (LineSegment horizontalSegment in m_snapGrid.HorizontalSegments)
                    {
                        if (topBorder.SnapSensitive(ref snapThresholdY, horizontalSegment))
                        {
                            // Snap top border, preserve original height.
                            moveRect.Top = horizontalSegment.Position;
                            moveRect.Bottom = horizontalSegment.Position + originalHeight;
                        }
                        if (bottomBorder.SnapSensitive(ref snapThresholdY, horizontalSegment))
                        {
                            // Snap bottom border, preserve original height.
                            moveRect.Top = horizontalSegment.Position - originalHeight;
                            moveRect.Bottom = horizontalSegment.Position;
                        }
                    }
                }
            }
        }

        protected override void OnResizing(ref RECT resizeRect, ResizeMode resizeMode)
        {
            if (m_snapGrid != null)
            {
                // Check if the MDI client rectangle changed during move/resize, because the client area may show or hide scrollbars during sizing/moving of this window.
                CheckUpdateSizeMove();

                // Evaluate left/right borders, then top/bottom borders.

                // Initialize snap threshold.
                int snapThresholdX = MaxSnapDistance + 1;

                switch (resizeMode)
                {
                    case ResizeMode.Left:
                    case ResizeMode.TopLeft:
                    case ResizeMode.BottomLeft:
                        LineSegment leftBorder = SnapGrid.LeftEdge(ref resizeRect, InsensitiveBorderEndLength);
                        if (null != leftBorder)
                        {
                            foreach (LineSegment verticalSegment in m_snapGrid.VerticalSegments)
                            {
                                if (leftBorder.SnapSensitive(ref snapThresholdX, verticalSegment))
                                {
                                    // Snap left border, preserve original location of right border of the window.
                                    resizeRect.Left = verticalSegment.Position;
                                    resizeRect.Right = m_rectangleBeforeSizeMove.Right;
                                }
                            }
                        }
                        break;
                    case ResizeMode.Right:
                    case ResizeMode.TopRight:
                    case ResizeMode.BottomRight:
                        LineSegment rightBorder = SnapGrid.RightEdge(ref resizeRect, InsensitiveBorderEndLength);
                        if (null != rightBorder)
                        {
                            foreach (LineSegment verticalSegment in m_snapGrid.VerticalSegments)
                            {
                                if (rightBorder.SnapSensitive(ref snapThresholdX, verticalSegment))
                                {
                                    // Snap right border, preserve original location of left border of the window.
                                    resizeRect.Left = m_rectangleBeforeSizeMove.Left;
                                    resizeRect.Right = verticalSegment.Position;
                                }
                            }
                        }
                        break;
                }

                // Initialize snap threshold.
                int snapThresholdY = MaxSnapDistance + 1;

                switch (resizeMode)
                {
                    case ResizeMode.Top:
                    case ResizeMode.TopLeft:
                    case ResizeMode.TopRight:
                        LineSegment topBorder = SnapGrid.TopEdge(ref resizeRect, InsensitiveBorderEndLength);
                        if (null != topBorder)
                        {
                            foreach (LineSegment horizontalSegment in m_snapGrid.HorizontalSegments)
                            {
                                if (topBorder.SnapSensitive(ref snapThresholdY, horizontalSegment))
                                {
                                    // Snap top border, preserve original location of bottom border of the window.
                                    resizeRect.Top = horizontalSegment.Position;
                                    resizeRect.Bottom = m_rectangleBeforeSizeMove.Bottom;
                                }
                            }
                        }
                        break;
                    case ResizeMode.Bottom:
                    case ResizeMode.BottomLeft:
                    case ResizeMode.BottomRight:
                        LineSegment bottomBorder = SnapGrid.BottomEdge(ref resizeRect, InsensitiveBorderEndLength);
                        if (null != bottomBorder)
                        {
                            foreach (LineSegment horizontalSegment in m_snapGrid.HorizontalSegments)
                            {
                                if (bottomBorder.SnapSensitive(ref snapThresholdY, horizontalSegment))
                                {
                                    // Snap bottom border, preserve original location of top border of the window.
                                    resizeRect.Top = m_rectangleBeforeSizeMove.Top;
                                    resizeRect.Bottom = horizontalSegment.Position;
                                }
                            }
                        }
                        break;
                }
            }
        }
    }
}

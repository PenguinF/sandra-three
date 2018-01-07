/*********************************************************************************
 * SnappingMdiChildForm.cs
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Windows <see cref="Form"/> which, if it's an MDI child, snaps to other MDI children and the edges of its parent MDI client area.
    /// </summary>
    public class SnappingMdiChildForm : ConstrainedMoveResizeForm
    {
        LineSegment createIfNonEmpty(int position, int min, int max)
        {
            if (min < max) return new LineSegment(position, min, max);
            return null;
        }

        LineSegment getLeftBorder(ref RECT rectangle, int cutoff)
        {
            // Cut off the given length and only yield the left border if it's non-empty.
            return createIfNonEmpty(rectangle.Left, rectangle.Top + cutoff, rectangle.Bottom - cutoff);
        }

        LineSegment getRightBorder(ref RECT rectangle, int cutoff)
        {
            // Cut off the given length and only yield the right border if it's non-empty.
            return createIfNonEmpty(rectangle.Right, rectangle.Top + cutoff, rectangle.Bottom - cutoff);
        }

        LineSegment getTopBorder(ref RECT rectangle, int cutoff)
        {
            // Cut off the given length and only yield the top border if it's non-empty.
            return createIfNonEmpty(rectangle.Top, rectangle.Left + cutoff, rectangle.Right - cutoff);
        }

        LineSegment getBottomBorder(ref RECT rectangle, int cutoff)
        {
            // Cut off the given length and only yield the bpttom border if it's non-empty.
            return createIfNonEmpty(rectangle.Bottom, rectangle.Left + cutoff, rectangle.Right - cutoff);
        }

        void addIfNonEmpty(List<LineSegment> list, LineSegment segment)
        {
            if (segment != null) list.Add(segment);
        }

        List<LineSegment> getVerticalBorders(ref RECT rectangle, int cutoff)
        {
            // Cut off the given length and only add left and right borders if they are non-empty.
            List<LineSegment> verticalBorders = new List<LineSegment>();
            addIfNonEmpty(verticalBorders, getLeftBorder(ref rectangle, cutoff));
            addIfNonEmpty(verticalBorders, getRightBorder(ref rectangle, cutoff));
            return verticalBorders;
        }

        List<LineSegment> getHorizontalBorders(ref RECT rectangle, int cutoff)
        {
            // Cut off the given length and only add top and bottom borders if they are non-empty.
            List<LineSegment> horizontalBorders = new List<LineSegment>();
            addIfNonEmpty(horizontalBorders, getTopBorder(ref rectangle, cutoff));
            addIfNonEmpty(horizontalBorders, getBottomBorder(ref rectangle, cutoff));
            return horizontalBorders;
        }

        /// <summary>
        /// Gets the default value for the <see cref="MaxSnapDistance"/> property.
        /// </summary>
        public const int DefaultMaxSnapDistance = 4;

        /// <summary>
        /// Gets or sets the maximum distance between form edges within which they will be sensitive to snapping together. The default value is <see cref="DefaultMaxSnapDistance"/> (4).
        /// </summary>
        [DefaultValue(DefaultMaxSnapDistance)]
        public int MaxSnapDistance { get { return m_maxSnapDistance; } set { m_maxSnapDistance = value; } }
        int m_maxSnapDistance = DefaultMaxSnapDistance;

        /// <summary>
        /// Gets the default value for the <see cref="InsensitiveBorderEndLength"/> property.
        /// </summary>
        public const int DefaultInsensitiveBorderEndLength = 16;

        /// <summary>
        /// Gets or sets the length of the ends of the borders of this form that are insensitive to snapping. The default value is <see cref="DefaultInsensitiveBorderEndLength"/> (16).
        /// </summary>
        [DefaultValue(DefaultInsensitiveBorderEndLength)]
        public int InsensitiveBorderEndLength { get { return m_insensitiveBorderEndLength; } set { m_insensitiveBorderEndLength = value; } }
        int m_insensitiveBorderEndLength = DefaultInsensitiveBorderEndLength;

        // Size/move precalculated information.
        bool m_canSnap;                                // Guard boolean, which is only true if the window is sizing/moving and has an MDI parent.
        Rectangle m_currentMdiClientScreenRectangle;   // Current bounds of the MDI client rectangle. Changes during sizing/moving when scrollbars are shown or hidden.
        LineSegment[] m_verticalSegments;              // Precalculated array of vertical line segments the sizing/moving window can snap onto.
        LineSegment[] m_horizontalSegments;            // Precalculated array of horizontal line segments the sizing/moving window can snap onto.
        RECT m_rectangleBeforeSizeMove;                // Initial bounds of this window before sizing/moving was started. Used to preserve sizes or positions.

        /// <summary>
        /// Calculates an array of visible vertical or horizontal segments, given a list of possibly overlapping rectangles.
        /// </summary>
        LineSegment[] calculateSegments(ref RECT mdiClientRectangle, List<RECT> mdiChildRectangles, bool isVertical)
        {
            // Create snap line segments for each MDI child, and add extra segments for the MDI client rectangle.
            List<LineSegment> segments = isVertical
                ? getVerticalBorders(ref mdiClientRectangle, 0)
                : getHorizontalBorders(ref mdiClientRectangle, 0);

            // Loop over MDI child rectangles, to cut away hidden sections of segments.
            for (int mdiChildRectangleIndex = mdiChildRectangles.Count - 1; mdiChildRectangleIndex >= 0; --mdiChildRectangleIndex)
            {
                RECT mdiChildRectangle = mdiChildRectangles[mdiChildRectangleIndex];

                // Calculate segments for this MDI child, and start with initial full segments.
                List<LineSegment> childSegments = isVertical
                    ? getVerticalBorders(ref mdiChildRectangle, m_insensitiveBorderEndLength)
                    : getHorizontalBorders(ref mdiChildRectangle, m_insensitiveBorderEndLength);

                // Loop over MDI child rectangles that are higher in the z-order, since they can overlap.
                for (int overlappingRectangleIndex = mdiChildRectangleIndex - 1; overlappingRectangleIndex >= 0; --overlappingRectangleIndex)
                {
                    // Cut away whatever is hidden by the MDI child higher in the z-order.
                    RECT overlappingRectangle = mdiChildRectangles[overlappingRectangleIndex];

                    // Calculate inflated rectangle coordinates.
                    int overlappingRectanglePositionMin, overlappingRectanglePositionMax, minInflated, maxInflated;
                    if (isVertical)
                    {
                        overlappingRectanglePositionMin = overlappingRectangle.Left;
                        overlappingRectanglePositionMax = overlappingRectangle.Right;
                        minInflated = overlappingRectangle.Top - m_insensitiveBorderEndLength;
                        maxInflated = overlappingRectangle.Bottom + m_insensitiveBorderEndLength;
                    }
                    else
                    {
                        overlappingRectanglePositionMin = overlappingRectangle.Top;
                        overlappingRectanglePositionMax = overlappingRectangle.Bottom;
                        minInflated = overlappingRectangle.Left - m_insensitiveBorderEndLength;
                        maxInflated = overlappingRectangle.Right + m_insensitiveBorderEndLength;
                    }

                    // Start with the end of the list and work back to the beginning, because segments that are split in two by this overlapping rectangle don't need to be checked again.
                    for (int index = childSegments.Count - 1; index >= 0; --index)
                    {
                        LineSegment segment = childSegments[index];

                        // Is the segment somewhere between the left and right edges of the inflated overlapping rectangle?
                        if (segment.Near < segment.Far && overlappingRectanglePositionMin < segment.Position && segment.Position < overlappingRectanglePositionMax)
                        {
                            if (minInflated <= segment.Near)
                            {
                                // Cut off the top of the segment.
                                if (segment.Near < maxInflated) segment.Near = maxInflated;
                            }
                            else if (segment.Far <= maxInflated)
                            {
                                // Cut off the bottom of the segment.
                                if (minInflated < segment.Far) segment.Far = minInflated;
                            }
                            else
                            {
                                // Split the segment in two.
                                childSegments.Add(new LineSegment(segment.Position, segment.Near, minInflated));
                                segment.Near = maxInflated;
                            }
                        }
                    }
                }

                // Only keep those segments that are non-empty.
                foreach (LineSegment segment in childSegments)
                {
                    if (segment.Near < segment.Far) segments.Add(segment);
                }
            }

            return segments.ToArray();
        }

        /// <summary>
        /// Precalculates and caches line segments that this form can snap onto.
        /// </summary>
        void prepareSizeMove(Control.ControlCollection mdiChildren)
        {
            // Ignore the possibility that the MDI client rectangle is empty.
            RECT mdiClientRectangle = new RECT()
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
                Form mdiChildForm = child as Form;
                if (mdiChildForm != null && mdiChildForm.Visible && mdiChildForm.WindowState == FormWindowState.Normal)
                {
                    // Convert the bounds of this MDI child to screen coordinates.
                    Rectangle mdiChildBounds = mdiChildForm.Bounds;
                    mdiChildBounds.Offset(mdiClientRectangle.Left, mdiClientRectangle.Top);
                    RECT mdiChildRectangle = new RECT()
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
            m_verticalSegments = calculateSegments(ref mdiClientRectangle, mdiChildRectangles, true);
            m_horizontalSegments = calculateSegments(ref mdiClientRectangle, mdiChildRectangles, false);
        }

        /// <summary>
        /// Checks if line segments that this form can snap onto should be precalculated (again).
        /// </summary>
        void checkUpdateSizeMove()
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
                        if (!m_canSnap || currentMdiClientScreenRectangle != m_currentMdiClientScreenRectangle)
                        {
                            m_canSnap = true;
                            m_currentMdiClientScreenRectangle = currentMdiClientScreenRectangle;
                            prepareSizeMove(c.Controls);
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
            checkUpdateSizeMove();
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            m_canSnap = false;
            m_verticalSegments = null;
            m_horizontalSegments = null;
            base.OnResizeEnd(e);
        }

        protected override void OnMoving(ref RECT moveRect)
        {
            // Only if an MDI child.
            if (!m_canSnap) return;

            // Check if the MDI client rectangle changed during move/resize, because the client area may show or hide scrollbars during sizing/moving of this window.
            checkUpdateSizeMove();

            // Evaluate left/right borders, then top/bottom borders.

            // Create line segments for each border of this window.
            LineSegment leftBorder = getLeftBorder(ref moveRect, m_insensitiveBorderEndLength);
            LineSegment rightBorder = getRightBorder(ref moveRect, m_insensitiveBorderEndLength);

            if (null != leftBorder && null != rightBorder)
            {
                // Initialize snap threshold.
                int snapThresholdX = m_maxSnapDistance + 1;

                // Preserve original width of the window.
                int originalWidth = m_rectangleBeforeSizeMove.Right - m_rectangleBeforeSizeMove.Left;

                // Check vertical segments to snap against.
                foreach (LineSegment verticalSegment in m_verticalSegments)
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
            LineSegment topBorder = getTopBorder(ref moveRect, m_insensitiveBorderEndLength);
            LineSegment bottomBorder = getBottomBorder(ref moveRect, m_insensitiveBorderEndLength);

            if (null != topBorder && null != bottomBorder)
            {
                // Initialize snap threshold.
                int snapThresholdY = m_maxSnapDistance + 1;

                // Preserve original height of the window.
                int originalHeight = m_rectangleBeforeSizeMove.Bottom - m_rectangleBeforeSizeMove.Top;

                // Check horizontal segments to snap against.
                foreach (LineSegment horizontalSegment in m_horizontalSegments)
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

        protected override void OnResizing(ref RECT resizeRect, ResizeMode resizeMode)
        {
            // Only if an MDI child.
            if (!m_canSnap) return;

            // Check if the MDI client rectangle changed during move/resize, because the client area may show or hide scrollbars during sizing/moving of this window.
            checkUpdateSizeMove();

            // Evaluate left/right borders, then top/bottom borders.

            // Initialize snap threshold.
            int snapThresholdX = m_maxSnapDistance + 1;

            switch (resizeMode)
            {
                case ResizeMode.Left:
                case ResizeMode.TopLeft:
                case ResizeMode.BottomLeft:
                    LineSegment leftBorder = getLeftBorder(ref resizeRect, m_insensitiveBorderEndLength);
                    if (null != leftBorder)
                    {
                        foreach (LineSegment verticalSegment in m_verticalSegments)
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
                    LineSegment rightBorder = getRightBorder(ref resizeRect, m_insensitiveBorderEndLength);
                    if (null != rightBorder)
                    {
                        foreach (LineSegment verticalSegment in m_verticalSegments)
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
            int snapThresholdY = m_maxSnapDistance + 1;

            switch (resizeMode)
            {
                case ResizeMode.Top:
                case ResizeMode.TopLeft:
                case ResizeMode.TopRight:
                    LineSegment topBorder = getTopBorder(ref resizeRect, m_insensitiveBorderEndLength);
                    if (null != topBorder)
                    {
                        foreach (LineSegment horizontalSegment in m_horizontalSegments)
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
                    LineSegment bottomBorder = getBottomBorder(ref resizeRect, m_insensitiveBorderEndLength);
                    if (null != bottomBorder)
                    {
                        foreach (LineSegment horizontalSegment in m_horizontalSegments)
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

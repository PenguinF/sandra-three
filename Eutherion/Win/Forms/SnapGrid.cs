#region License
/*********************************************************************************
 * SnapGrid.cs
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
using System.Drawing;

namespace Eutherion.Win.Forms
{
    /// <summary>
    /// Contains a collection of <see cref="LineSegment"/> instances a <see cref="ConstrainedMoveResizeForm"/>
    /// can snap to while it's being resized or moved.
    /// </summary>
    public class SnapGrid
    {
        private static LineSegment CreateIfNonEmpty(int position, int min, int max)
        {
            if (min < max) return new LineSegment(position, min, max);
            return null;
        }

        /// <summary>
        /// Creates a <see cref="LineSegment"/> representing the left edge of a rectangle, or null if it's empty.
        /// </summary>
        /// <param name="rectangle">
        /// The rectangle for which to derive the left edge <see cref="LineSegment"/>.
        /// </param>
        /// <param name="cutoff">
        /// The length to cut off both ends of the line segment.
        /// </param>
        /// <returns>
        /// A <see cref="LineSegment"/> representing the left edge of the rectangle, or null if it's empty.
        /// </returns>
        public static LineSegment LeftEdge(ref Rectangle rectangle, int cutoff)
        {
            // Cut off the given length and only yield the left border if it's non-empty.
            return CreateIfNonEmpty(rectangle.Left, rectangle.Top + cutoff, rectangle.Bottom - cutoff);
        }

        /// <summary>
        /// Creates a <see cref="LineSegment"/> representing the left edge of a rectangle, or null if it's empty.
        /// </summary>
        /// <param name="rectangle">
        /// The rectangle for which to derive the left edge <see cref="LineSegment"/>.
        /// </param>
        /// <param name="cutoff">
        /// The length to cut off both ends of the line segment.
        /// </param>
        /// <returns>
        /// A <see cref="LineSegment"/> representing the left edge of the rectangle, or null if it's empty.
        /// </returns>
        public static LineSegment LeftEdge(ref RECT rectangle, int cutoff)
        {
            // Cut off the given length and only yield the left border if it's non-empty.
            return CreateIfNonEmpty(rectangle.Left, rectangle.Top + cutoff, rectangle.Bottom - cutoff);
        }

        /// <summary>
        /// Creates a <see cref="LineSegment"/> representing the right edge of a rectangle, or null if it's empty.
        /// </summary>
        /// <param name="rectangle">
        /// The rectangle for which to derive the right edge <see cref="LineSegment"/>.
        /// </param>
        /// <param name="cutoff">
        /// The length to cut off both ends of the line segment.
        /// </param>
        /// <returns>
        /// A <see cref="LineSegment"/> representing the right edge of the rectangle, or null if it's empty.
        /// </returns>
        public static LineSegment RightEdge(ref Rectangle rectangle, int cutoff)
        {
            // Cut off the given length and only yield the right border if it's non-empty.
            return CreateIfNonEmpty(rectangle.Right, rectangle.Top + cutoff, rectangle.Bottom - cutoff);
        }

        /// <summary>
        /// Creates a <see cref="LineSegment"/> representing the right edge of a rectangle, or null if it's empty.
        /// </summary>
        /// <param name="rectangle">
        /// The rectangle for which to derive the right edge <see cref="LineSegment"/>.
        /// </param>
        /// <param name="cutoff">
        /// The length to cut off both ends of the line segment.
        /// </param>
        /// <returns>
        /// A <see cref="LineSegment"/> representing the right edge of the rectangle, or null if it's empty.
        /// </returns>
        public static LineSegment RightEdge(ref RECT rectangle, int cutoff)
        {
            // Cut off the given length and only yield the right border if it's non-empty.
            return CreateIfNonEmpty(rectangle.Right, rectangle.Top + cutoff, rectangle.Bottom - cutoff);
        }

        /// <summary>
        /// Creates a <see cref="LineSegment"/> representing the top edge of a rectangle, or null if it's empty.
        /// </summary>
        /// <param name="rectangle">
        /// The rectangle for which to derive the top edge <see cref="LineSegment"/>.
        /// </param>
        /// <param name="cutoff">
        /// The length to cut off both ends of the line segment.
        /// </param>
        /// <returns>
        /// A <see cref="LineSegment"/> representing the top edge of the rectangle, or null if it's empty.
        /// </returns>
        public static LineSegment TopEdge(ref Rectangle rectangle, int cutoff)
        {
            // Cut off the given length and only yield the top border if it's non-empty.
            return CreateIfNonEmpty(rectangle.Top, rectangle.Left + cutoff, rectangle.Right - cutoff);
        }

        /// <summary>
        /// Creates a <see cref="LineSegment"/> representing the top edge of a rectangle, or null if it's empty.
        /// </summary>
        /// <param name="rectangle">
        /// The rectangle for which to derive the top edge <see cref="LineSegment"/>.
        /// </param>
        /// <param name="cutoff">
        /// The length to cut off both ends of the line segment.
        /// </param>
        /// <returns>
        /// A <see cref="LineSegment"/> representing the top edge of the rectangle, or null if it's empty.
        /// </returns>
        public static LineSegment TopEdge(ref RECT rectangle, int cutoff)
        {
            // Cut off the given length and only yield the top border if it's non-empty.
            return CreateIfNonEmpty(rectangle.Top, rectangle.Left + cutoff, rectangle.Right - cutoff);
        }

        /// <summary>
        /// Creates a <see cref="LineSegment"/> representing the bottom edge of a rectangle, or null if it's empty.
        /// </summary>
        /// <param name="rectangle">
        /// The rectangle for which to derive the bottom edge <see cref="LineSegment"/>.
        /// </param>
        /// <param name="cutoff">
        /// The length to cut off both ends of the line segment.
        /// </param>
        /// <returns>
        /// A <see cref="LineSegment"/> representing the bottom edge of the rectangle, or null if it's empty.
        /// </returns>
        public static LineSegment BottomEdge(ref Rectangle rectangle, int cutoff)
        {
            // Cut off the given length and only yield the bpttom border if it's non-empty.
            return CreateIfNonEmpty(rectangle.Bottom, rectangle.Left + cutoff, rectangle.Right - cutoff);
        }

        /// <summary>
        /// Creates a <see cref="LineSegment"/> representing the bottom edge of a rectangle, or null if it's empty.
        /// </summary>
        /// <param name="rectangle">
        /// The rectangle for which to derive the bottom edge <see cref="LineSegment"/>.
        /// </param>
        /// <param name="cutoff">
        /// The length to cut off both ends of the line segment.
        /// </param>
        /// <returns>
        /// A <see cref="LineSegment"/> representing the bottom edge of the rectangle, or null if it's empty.
        /// </returns>
        public static LineSegment BottomEdge(ref RECT rectangle, int cutoff)
        {
            // Cut off the given length and only yield the bpttom border if it's non-empty.
            return CreateIfNonEmpty(rectangle.Bottom, rectangle.Left + cutoff, rectangle.Right - cutoff);
        }

        private static void AddIfNonEmpty(List<LineSegment> list, LineSegment segment)
        {
            if (segment != null) list.Add(segment);
        }

        /// <summary>
        /// Creates a list of non-empty line segments representing the left and right edges of a rectangle.
        /// </summary>
        /// <param name="rectangle">
        /// The rectangle for which to derive the vertical edges.
        /// </param>
        /// <param name="cutoff">
        /// The length to cut off both ends of all line segments.
        /// </param>
        /// <returns>
        /// A list of non-empty line segments representing the left and right edges of the rectangle.
        /// </returns>
        public static List<LineSegment> GetVerticalEdges(ref Rectangle rectangle, int cutoff)
        {
            // Cut off the given length and only add left and right borders if they are non-empty.
            List<LineSegment> verticalBorders = new List<LineSegment>(16);
            AddIfNonEmpty(verticalBorders, LeftEdge(ref rectangle, cutoff));
            AddIfNonEmpty(verticalBorders, RightEdge(ref rectangle, cutoff));
            return verticalBorders;
        }

        /// <summary>
        /// Creates a list of non-empty line segments representing the top and bottom edges of a rectangle.
        /// </summary>
        /// <param name="rectangle">
        /// The rectangle for which to derive the horizontal edges.
        /// </param>
        /// <param name="cutoff">
        /// The length to cut off both ends of all line segments.
        /// </param>
        /// <returns>
        /// A list of non-empty line segments representing the top and bottom edges of the rectangle.
        /// </returns>
        public static List<LineSegment> GetHorizontalEdges(ref Rectangle rectangle, int cutoff)
        {
            // Cut off the given length and only add top and bottom borders if they are non-empty.
            List<LineSegment> horizontalBorders = new List<LineSegment>(16);
            AddIfNonEmpty(horizontalBorders, TopEdge(ref rectangle, cutoff));
            AddIfNonEmpty(horizontalBorders, BottomEdge(ref rectangle, cutoff));
            return horizontalBorders;
        }

        private static void AddVisibleSegments(List<LineSegment> segments, List<Rectangle> overlappingRectangles, bool isVertical, int cutoff)
        {
            // Loop over rectangles, to cut away hidden sections of segments.
            for (int rectangleIndex = overlappingRectangles.Count - 1; rectangleIndex >= 0; --rectangleIndex)
            {
                Rectangle rectangle = overlappingRectangles[rectangleIndex];

                // Calculate segments for this rectangle, and start with initial full segments.
                List<LineSegment> rectangleSegments = isVertical
                    ? GetVerticalEdges(ref rectangle, cutoff)
                    : GetHorizontalEdges(ref rectangle, cutoff);

                // Loop over rectangles that are higher in the z-order (so lower index), since they can overlap.
                for (int overlappingRectangleIndex = rectangleIndex - 1; overlappingRectangleIndex >= 0; --overlappingRectangleIndex)
                {
                    // Cut away whatever is hidden by the rectangle higher in the z-order.
                    Rectangle overlappingRectangle = overlappingRectangles[overlappingRectangleIndex];

                    // Calculate inflated rectangle coordinates.
                    int overlappingRectanglePositionMin, overlappingRectanglePositionMax, minInflated, maxInflated;
                    if (isVertical)
                    {
                        overlappingRectanglePositionMin = overlappingRectangle.Left;
                        overlappingRectanglePositionMax = overlappingRectangle.Right;
                        minInflated = overlappingRectangle.Top - cutoff;
                        maxInflated = overlappingRectangle.Bottom + cutoff;
                    }
                    else
                    {
                        overlappingRectanglePositionMin = overlappingRectangle.Top;
                        overlappingRectanglePositionMax = overlappingRectangle.Bottom;
                        minInflated = overlappingRectangle.Left - cutoff;
                        maxInflated = overlappingRectangle.Right + cutoff;
                    }

                    // Start with the end of the list and work back to the beginning, so segments that are split in two by this overlapping rectangle will not be checked again.
                    for (int index = rectangleSegments.Count - 1; index >= 0; --index)
                    {
                        LineSegment segment = rectangleSegments[index];

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
                                rectangleSegments.Add(new LineSegment(segment.Position, segment.Near, minInflated));
                                segment.Near = maxInflated;
                            }
                        }
                    }
                }

                // Only keep those segments that are non-empty.
                foreach (LineSegment segment in rectangleSegments)
                {
                    if (segment.Near < segment.Far) segments.Add(segment);
                }
            }
        }

        /// <summary>
        /// Gets the list of vertical line segments the resizing/moving window can snap onto.
        /// </summary>
        public readonly ReadOnlyList<LineSegment> VerticalSegments;

        /// <summary>
        /// Gets the list of horizontal line segments the resizing/moving window can snap onto.
        /// </summary>
        public readonly ReadOnlyList<LineSegment> HorizontalSegments;

        /// <summary>
        /// Initializes a new instance of <see cref="SnapGrid"/>.
        /// </summary>
        /// <param name="verticalSegments">
        /// A list of vertical line segments the resizing/moving window can snap onto.
        /// </param>
        /// <param name="horizontalSegments">
        /// A list of horizontal line segments the resizing/moving window can snap onto.
        /// </param>
        /// <param name="overlappingRectangles">
        /// A list of overlapping rectangles in z-order. Rectangles with a lower index overlap rectangles with a higher index.
        /// </param>
        /// <param name="cutoff">
        /// The length to cut off both ends of all line segments.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="verticalSegments"/> and/or <paramref name="horizontalSegments"/> and/or <paramref name="overlappingRectangles"/> is null.
        /// </exception>
        public SnapGrid(
            List<LineSegment> verticalSegments,
            List<LineSegment> horizontalSegments,
            List<Rectangle> overlappingRectangles,
            int cutoff)
        {
            if (verticalSegments == null) throw new ArgumentNullException(nameof(verticalSegments));
            if (horizontalSegments == null) throw new ArgumentNullException(nameof(horizontalSegments));
            if (overlappingRectangles == null) throw new ArgumentNullException(nameof(overlappingRectangles));

            AddVisibleSegments(verticalSegments, overlappingRectangles, true, cutoff);
            AddVisibleSegments(horizontalSegments, overlappingRectangles, false, cutoff);

            VerticalSegments = ReadOnlyList<LineSegment>.Create(verticalSegments);
            HorizontalSegments = ReadOnlyList<LineSegment>.Create(horizontalSegments);
        }

        /// <summary>
        /// Modifies a <see cref="MoveResizeEventArgs"/> so a window will snap to segments
        /// defined in this <see cref="SnapGrid"/> while it's being moved.
        /// </summary>
        /// <param name="e">
        /// The <see cref="MoveResizeEventArgs"/> to modify.
        /// </param>
        /// <param name="rectangleBeforeSizeMove">
        /// The bounds of the rectangle of the window before it was being moved.
        /// </param>
        /// <param name="maxSnapDistance">
        /// The maximum distance from a line segment within which the window will snap to a line segment.
        /// </param>
        /// <param name="cutoff">
        /// The length to cut off both ends of line segments representing the edges of the window being moved.
        /// </param>
        public void SnapWhileMoving(MoveResizeEventArgs e, ref Rectangle rectangleBeforeSizeMove, int maxSnapDistance, int cutoff)
        {
            // Evaluate left/right borders, then top/bottom borders.

            // Create line segments for each border of the rectangle.
            LineSegment leftBorder = LeftEdge(ref e.MoveResizeRect, cutoff);
            LineSegment rightBorder = RightEdge(ref e.MoveResizeRect, cutoff);

            if (null != leftBorder && null != rightBorder)
            {
                // Initialize snap threshold.
                int snapThresholdX = maxSnapDistance + 1;

                // Preserve original width of the rectangle.
                int originalWidth = rectangleBeforeSizeMove.Width;

                // Check vertical segments to snap against.
                foreach (LineSegment verticalSegment in VerticalSegments)
                {
                    if (leftBorder.SnapSensitive(ref snapThresholdX, verticalSegment))
                    {
                        // Snap left border, preserve original width.
                        e.MoveResizeRect.Left = verticalSegment.Position;
                        e.MoveResizeRect.Right = verticalSegment.Position + originalWidth;
                    }
                    if (rightBorder.SnapSensitive(ref snapThresholdX, verticalSegment))
                    {
                        // Snap right border, preserve original width.
                        e.MoveResizeRect.Left = verticalSegment.Position - originalWidth;
                        e.MoveResizeRect.Right = verticalSegment.Position;
                    }
                }
            }

            // Create line segments for each border of the rectangle.
            LineSegment topBorder = TopEdge(ref e.MoveResizeRect, cutoff);
            LineSegment bottomBorder = BottomEdge(ref e.MoveResizeRect, cutoff);

            if (null != topBorder && null != bottomBorder)
            {
                // Initialize snap threshold.
                int snapThresholdY = maxSnapDistance + 1;

                // Preserve original height of the rectangle.
                int originalHeight = rectangleBeforeSizeMove.Height;

                // Check horizontal segments to snap against.
                foreach (LineSegment horizontalSegment in HorizontalSegments)
                {
                    if (topBorder.SnapSensitive(ref snapThresholdY, horizontalSegment))
                    {
                        // Snap top border, preserve original height.
                        e.MoveResizeRect.Top = horizontalSegment.Position;
                        e.MoveResizeRect.Bottom = horizontalSegment.Position + originalHeight;
                    }
                    if (bottomBorder.SnapSensitive(ref snapThresholdY, horizontalSegment))
                    {
                        // Snap bottom border, preserve original height.
                        e.MoveResizeRect.Top = horizontalSegment.Position - originalHeight;
                        e.MoveResizeRect.Bottom = horizontalSegment.Position;
                    }
                }
            }
        }

        /// <summary>
        /// Modifies a <see cref="ResizeEventArgs"/> so a window will snap to segments
        /// defined in this <see cref="SnapGrid"/> while it's being resized.
        /// </summary>
        /// <param name="e">
        /// The <see cref="ResizeEventArgs"/> to modify.
        /// </param>
        /// <param name="rectangleBeforeSizeMove">
        /// The bounds of the rectangle of the window before it was being resized.
        /// </param>
        /// <param name="maxSnapDistance">
        /// The maximum distance from a line segment within which the window will snap to a line segment.
        /// </param>
        /// <param name="cutoff">
        /// The length to cut off both ends of line segments representing the edges of the window being resized.
        /// </param>
        public void SnapWhileResizing(ResizeEventArgs e, ref Rectangle rectangleBeforeSizeMove, int maxSnapDistance, int cutoff)
        {
            // Evaluate left/right borders, then top/bottom borders.

            // Initialize snap threshold.
            int snapThresholdX = maxSnapDistance + 1;

            switch (e.ResizeMode)
            {
                case ResizeMode.Left:
                case ResizeMode.TopLeft:
                case ResizeMode.BottomLeft:
                    LineSegment leftBorder = LeftEdge(ref e.MoveResizeRect, cutoff);
                    if (null != leftBorder)
                    {
                        foreach (LineSegment verticalSegment in VerticalSegments)
                        {
                            if (leftBorder.SnapSensitive(ref snapThresholdX, verticalSegment))
                            {
                                // Snap left border, preserve original location of right border of the rectangle.
                                e.MoveResizeRect.Left = verticalSegment.Position;
                                e.MoveResizeRect.Right = rectangleBeforeSizeMove.Right;
                            }
                        }
                    }
                    break;
                case ResizeMode.Right:
                case ResizeMode.TopRight:
                case ResizeMode.BottomRight:
                    LineSegment rightBorder = RightEdge(ref e.MoveResizeRect, cutoff);
                    if (null != rightBorder)
                    {
                        foreach (LineSegment verticalSegment in VerticalSegments)
                        {
                            if (rightBorder.SnapSensitive(ref snapThresholdX, verticalSegment))
                            {
                                // Snap right border, preserve original location of left border of the rectangle.
                                e.MoveResizeRect.Left = rectangleBeforeSizeMove.Left;
                                e.MoveResizeRect.Right = verticalSegment.Position;
                            }
                        }
                    }
                    break;
            }

            // Initialize snap threshold.
            int snapThresholdY = maxSnapDistance + 1;

            switch (e.ResizeMode)
            {
                case ResizeMode.Top:
                case ResizeMode.TopLeft:
                case ResizeMode.TopRight:
                    LineSegment topBorder = TopEdge(ref e.MoveResizeRect, cutoff);
                    if (null != topBorder)
                    {
                        foreach (LineSegment horizontalSegment in HorizontalSegments)
                        {
                            if (topBorder.SnapSensitive(ref snapThresholdY, horizontalSegment))
                            {
                                // Snap top border, preserve original location of bottom border of the rectangle.
                                e.MoveResizeRect.Top = horizontalSegment.Position;
                                e.MoveResizeRect.Bottom = rectangleBeforeSizeMove.Bottom;
                            }
                        }
                    }
                    break;
                case ResizeMode.Bottom:
                case ResizeMode.BottomLeft:
                case ResizeMode.BottomRight:
                    LineSegment bottomBorder = BottomEdge(ref e.MoveResizeRect, cutoff);
                    if (null != bottomBorder)
                    {
                        foreach (LineSegment horizontalSegment in HorizontalSegments)
                        {
                            if (bottomBorder.SnapSensitive(ref snapThresholdY, horizontalSegment))
                            {
                                // Snap bottom border, preserve original location of top border of the rectangle.
                                e.MoveResizeRect.Top = rectangleBeforeSizeMove.Top;
                                e.MoveResizeRect.Bottom = horizontalSegment.Position;
                            }
                        }
                    }
                    break;
            }
        }
    }
}

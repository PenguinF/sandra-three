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
using System.Collections.Generic;

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
        public static List<LineSegment> GetVerticalEdges(ref RECT rectangle, int cutoff)
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
        public static List<LineSegment> GetHorizontalEdges(ref RECT rectangle, int cutoff)
        {
            // Cut off the given length and only add top and bottom borders if they are non-empty.
            List<LineSegment> horizontalBorders = new List<LineSegment>(16);
            AddIfNonEmpty(horizontalBorders, TopEdge(ref rectangle, cutoff));
            AddIfNonEmpty(horizontalBorders, BottomEdge(ref rectangle, cutoff));
            return horizontalBorders;
        }

        /// <summary>
        /// Derives visible line segments from a list of overlapping rectangles in z-order, and adds them to a list.
        /// </summary>
        /// <param name="segments">
        /// The list of segments to add to.
        /// </param>
        /// <param name="overlappingRectangles">
        /// The list of overlapping rectangles in z-order. Rectangles with a lower index overlap rectangles with a higher index.
        /// </param>
        /// <param name="isVertical">
        /// True to consider vertical edges, false to consider horizontal edges.
        /// </param>
        /// <param name="cutoff">
        /// The length to cut off both ends of all line segments.
        /// </param>
        public static void AddVisibleSegments(List<LineSegment> segments, List<RECT> overlappingRectangles, bool isVertical, int cutoff)
        {
            // Loop over rectangles, to cut away hidden sections of segments.
            for (int rectangleIndex = overlappingRectangles.Count - 1; rectangleIndex >= 0; --rectangleIndex)
            {
                RECT rectangle = overlappingRectangles[rectangleIndex];

                // Calculate segments for this rectangle, and start with initial full segments.
                List<LineSegment> rectangleSegments = isVertical
                    ? GetVerticalEdges(ref rectangle, cutoff)
                    : GetHorizontalEdges(ref rectangle, cutoff);

                // Loop over rectangles that are higher in the z-order (so lower index), since they can overlap.
                for (int overlappingRectangleIndex = rectangleIndex - 1; overlappingRectangleIndex >= 0; --overlappingRectangleIndex)
                {
                    // Cut away whatever is hidden by the rectangle higher in the z-order.
                    RECT overlappingRectangle = overlappingRectangles[overlappingRectangleIndex];

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
        /// The enumeration of vertical line segments the resizing/moving window can snap onto.
        /// </param>
        /// <param name="horizontalSegments">
        /// The enumeration of horizontal line segments the resizing/moving window can snap onto.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="verticalSegments"/> and/or <paramref name="horizontalSegments"/> is null.
        /// </exception>
        public SnapGrid(IEnumerable<LineSegment> verticalSegments, IEnumerable<LineSegment> horizontalSegments)
        {
            VerticalSegments = ReadOnlyList<LineSegment>.Create(verticalSegments);
            HorizontalSegments = ReadOnlyList<LineSegment>.Create(horizontalSegments);
        }
    }
}

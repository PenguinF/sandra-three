/*********************************************************************************
 * LineSegment.cs
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

namespace Sandra.UI.WF
{
    /// <summary>
    /// Helper class to represent a segment of a horizontal or vertical line.
    /// </summary>
    public class LineSegment
    {
        /// <summary>
        /// The fixed coordinate of the line segment, which is the X-coordinate for vertical line segments, and the Y-coordinate for horizontal segments.
        /// </summary>
        public readonly int Position;

        /// <summary>
        /// The coordinate of the near endpoint of the line segment, which is the minimum Y-coordinate for vertical line segments, and the minimum X-coordinate for horizontal segments.
        /// </summary>
        public int Near;

        /// <summary>
        /// The coordinate of the far endpoint of the line segment, which is the maximum Y-coordinate for vertical line segments, and the maximum X-coordinate for horizontal segments.
        /// </summary>
        public int Far;

        /// <summary>
        /// Constructs a new instance of a <see cref="LineSegment"/>.
        /// </summary>
        /// <param name="position">
        /// The initial fixed coordinate of the line segment.
        /// </param>
        /// <param name="near">
        /// The initial coordinate of the near endpoint of the line segment.
        /// </param>
        /// <param name="far">
        /// The initial coordinate of the far endpoint of the line segment.
        /// </param>
        public LineSegment(int position, int near, int far)
        {
            Position = position;
            Near = near;
            Far = far;
        }

        /// <summary>
        /// Calculates whether another line segment is close enough to snap together with this one.
        /// If it does, as a side effect the threshold is lowered to the distance to the other segment.
        /// </summary>
        public bool SnapSensitive(ref int threshold, LineSegment segment)
        {
            if (Far < segment.Near || segment.Far < Near) return false;
            int distance = Math.Abs(Position - segment.Position);
            if (distance >= threshold) return false;
            threshold = distance;
            return true;
        }
    }
}

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

namespace Eutherion.Win.Forms
{
    /// <summary>
    /// Contains a collection of <see cref="LineSegment"/> instances a <see cref="ConstrainedMoveResizeForm"/>
    /// can snap to while it's being resized or moved.
    /// </summary>
    public class SnapGrid
    {
        /// <summary>
        /// Gets the array of vertical line segments the resizing/moving window can snap onto.
        /// </summary>
        public readonly LineSegment[] VerticalSegments;

        /// <summary>
        /// Gets the array of horizontal line segments the resizing/moving window can snap onto.
        /// </summary>
        public readonly LineSegment[] HorizontalSegments;

        public SnapGrid(LineSegment[] verticalSegments, LineSegment[] horizontalSegments)
        {
            VerticalSegments = verticalSegments;
            HorizontalSegments = horizontalSegments;
        }
    }
}

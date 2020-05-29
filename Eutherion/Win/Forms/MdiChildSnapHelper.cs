#region License
/*********************************************************************************
 * MdiChildSnapHelper.cs
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Eutherion.Win.Forms
{
    /// <summary>
    /// Modifies a MDI child Form's behavior, such that it snaps to other MDI children and the edges of its parent MDI client area.
    /// </summary>
    public class MdiChildSnapHelper : SnapHelper
    {
        /// <summary>
        /// Initializes a new <see cref="MdiChildSnapHelper"/> for the MDI child and attaches snap behavior to it.
        /// </summary>
        /// <param name="mdiChild">
        /// The <see cref="ConstrainedMoveResizeForm"/> MDI child to modify.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="mdiChild"/> is null.
        /// </exception>
        public static MdiChildSnapHelper AttachTo(ConstrainedMoveResizeForm mdiChild)
        {
            if (mdiChild == null) throw new ArgumentNullException(nameof(mdiChild));
            return new MdiChildSnapHelper(mdiChild);
        }

        // Size/move precalculated information.
        private Rectangle m_currentMdiClientScreenRectangle;   // Current bounds of the MDI client rectangle. Changes during sizing/moving when scrollbars are shown or hidden.
        private Rectangle m_rectangleBeforeSizeMove;           // Initial bounds of this window before sizing/moving was started. Used to preserve sizes or positions.
        private SnapGrid m_snapGrid;

        private MdiChildSnapHelper(ConstrainedMoveResizeForm mdiChild)
            : base(mdiChild)
        {
            Form.ResizeBegin += MdiChild_ResizeBegin;
            Form.ResizeEnd += MdiChild_ResizeEnd;
        }

        /// <summary>
        /// Precalculates and caches line segments that the form can snap onto.
        /// </summary>
        private SnapGrid PrepareSizeMove(Control.ControlCollection mdiChildren)
        {
            // Ignore the possibility that the MDI client rectangle is empty.
            // Create a list of MDI child rectangles sorted by their z-order. (MDI child on top has index 0.)
            List<Rectangle> mdiChildRectangles = new List<Rectangle>();
            int mdiChildCount = mdiChildren.Count;
            for (int mdiChildIndex = 0; mdiChildIndex < mdiChildCount; ++mdiChildIndex)
            {
                if (mdiChildren[mdiChildIndex] is Form mdiChildForm && mdiChildForm.Visible && mdiChildForm.WindowState == FormWindowState.Normal)
                {
                    // Convert the bounds of this MDI child to screen coordinates.
                    Rectangle mdiChildRectangle = mdiChildForm.Bounds;
                    mdiChildRectangle.Offset(m_currentMdiClientScreenRectangle.Left, m_currentMdiClientScreenRectangle.Top);

                    if (Form != mdiChildForm)
                    {
                        // Intersect the MDI child rectangle with the MDI client rectangle, so the form does not snap to edges outside of the visible MDI client rectangle.
                        mdiChildRectangle = Rectangle.Intersect(mdiChildRectangle, m_currentMdiClientScreenRectangle);

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

            // Calculate snappable segments and save them to arrays which can be used efficiently from within the event handlers.
            List<LineSegment> verticalSegments = SnapGrid.GetVerticalEdges(ref m_currentMdiClientScreenRectangle, 0);
            List<LineSegment> horizontalSegments = SnapGrid.GetHorizontalEdges(ref m_currentMdiClientScreenRectangle, 0);

            return new SnapGrid(verticalSegments, horizontalSegments, mdiChildRectangles, InsensitiveBorderEndLength);
        }

        /// <summary>
        /// Checks if line segments that the form can snap onto should be precalculated (again).
        /// </summary>
        private void CheckUpdateSizeMove()
        {
            Form parentForm = Form.MdiParent;
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

        private void MdiChild_ResizeBegin(object sender, EventArgs e)
        {
            // Precalculate size/move line segments.
            CheckUpdateSizeMove();

            if (m_snapGrid != null)
            {
                if (SnapWhileMoving) Form.Moving += MdiChild_Moving;
                if (SnapWhileResizing) Form.Resizing += MdiChild_Resizing;
            }
        }

        private void MdiChild_ResizeEnd(object sender, EventArgs e)
        {
            Form.Moving -= MdiChild_Moving;
            Form.Resizing -= MdiChild_Resizing;
            m_snapGrid = null;
        }

        private void MdiChild_Moving(object sender, MoveResizeEventArgs e)
        {
            if (m_snapGrid != null)
            {
                // Check if the MDI client rectangle changed during move/resize, because the client area may show or hide scrollbars during sizing/moving of the window.
                CheckUpdateSizeMove();

                m_snapGrid.SnapWhileMoving(e, ref m_rectangleBeforeSizeMove, MaxSnapDistance, InsensitiveBorderEndLength);
            }
        }

        private void MdiChild_Resizing(object sender, ResizeEventArgs e)
        {
            if (m_snapGrid != null)
            {
                // Check if the MDI client rectangle changed during move/resize, because the client area may show or hide scrollbars during sizing/moving of the window.
                CheckUpdateSizeMove();

                m_snapGrid.SnapWhileResizing(e, ref m_rectangleBeforeSizeMove, MaxSnapDistance, InsensitiveBorderEndLength);
            }
        }
    }
}

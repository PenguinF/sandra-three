#region License
/*********************************************************************************
 * OwnedFormSnapHelper.cs
 *
 * Copyright (c) 2004-2023 Henk Nicolai
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
using System.Linq;
using System.Windows.Forms;

namespace Eutherion.Win.Forms
{
    /// <summary>
    /// Modifies an owned Form's behavior, such that it snaps to sibling owned forms, its owner form, and the edges of the screen.
    /// </summary>
    public class OwnedFormSnapHelper : SnapHelper
    {
        /// <summary>
        /// Initializes a new <see cref="OwnedFormSnapHelper"/> for the owned form and attaches snap behavior to it.
        /// </summary>
        /// <param name="ownedForm">
        /// The <see cref="ConstrainedMoveResizeForm"/> to modify.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="ownedForm"/> is null.
        /// </exception>
        public static OwnedFormSnapHelper AttachTo(ConstrainedMoveResizeForm ownedForm)
        {
            if (ownedForm == null) throw new ArgumentNullException(nameof(ownedForm));
            return new OwnedFormSnapHelper(ownedForm);
        }

        // Initial bounds of the window before sizing/moving was started. Used to preserve sizes or positions.
        private Rectangle m_rectangleBeforeSizeMove;
        private SnapGrid m_snapGrid;

        private OwnedFormSnapHelper(ConstrainedMoveResizeForm ownedForm)
            : base(ownedForm)
        {
            Form.ResizeBegin += OwnedForm_ResizeBegin;
            Form.ResizeEnd += OwnedForm_ResizeEnd;
        }

        private void OwnedForm_ResizeBegin(object sender, EventArgs e)
        {
            // Precalculate size/move line segments.
            Form parentForm = Form.Owner;
            if (parentForm != null)
            {
                m_rectangleBeforeSizeMove = Form.Bounds;

                // Enumerate all siblings and the parent form in z-order.
                Form[] snapTargetForms = parentForm.OwnedForms.Concat(new SingleElementEnumerable<Form>(parentForm)).ToArray();
                IntPtr[] handles = snapTargetForms.Where(x => x.IsHandleCreated).Select(x => x.Handle).ToArray();
                int handleCount = handles.Length;
                List<Form> formsInZOrder = new List<Form>(handleCount);

                NativeMethods.EnumWindows(
                    new EnumThreadWindowsCallback((handle, lParam) =>
                    {
                        int handleIndex = Array.IndexOf(handles, handle);
                        if (handleIndex >= 0)
                        {
                            formsInZOrder.Add(snapTargetForms[handleIndex]);
                            handles[handleIndex] = IntPtr.Zero;
                            handleCount--;
                        }
                        return handleCount > 0;
                    }), IntPtr.Zero);

                // Now build the snap grid from all working areas edges, then the sibling rectangles.
                var verticalSegments = new List<LineSegment>();
                var horizontalSegments = new List<LineSegment>();

                foreach (Screen screen in Screen.AllScreens)
                {
                    Rectangle workingArea = screen.WorkingArea;
                    verticalSegments.AddRange(SnapGrid.GetVerticalEdges(ref workingArea, InsensitiveBorderEndLength));
                    horizontalSegments.AddRange(SnapGrid.GetHorizontalEdges(ref workingArea, InsensitiveBorderEndLength));
                }

                // Add snap line segments for each sibling.
                List<Rectangle> siblingRectangles = new List<Rectangle>();
                foreach (Form form in formsInZOrder)
                {
                    if (form != Form && form.Visible && form.WindowState == FormWindowState.Normal)
                    {
                        Rectangle bounds = form.Bounds;

                        // Only add non-empty rectangles.
                        if (bounds.Left < bounds.Right && bounds.Top < bounds.Bottom) siblingRectangles.Add(bounds);
                    }
                }

                m_snapGrid = new SnapGrid(verticalSegments, horizontalSegments, siblingRectangles, InsensitiveBorderEndLength);

                if (SnapWhileMoving) Form.Moving += OwnedForm_Moving;
                if (SnapWhileResizing) Form.Resizing += OwnedForm_Resizing;
            }
        }

        private void OwnedForm_ResizeEnd(object sender, EventArgs e)
        {
            Form.Moving -= OwnedForm_Moving;
            Form.Resizing -= OwnedForm_Resizing;
            m_snapGrid = null;
        }

        private void OwnedForm_Moving(object sender, MoveResizeEventArgs e)
        {
            if (m_snapGrid != null)
            {
                m_snapGrid.SnapWhileMoving(e, ref m_rectangleBeforeSizeMove, MaxSnapDistance, InsensitiveBorderEndLength);
            }
        }

        private void OwnedForm_Resizing(object sender, ResizeEventArgs e)
        {
            if (m_snapGrid != null)
            {
                m_snapGrid.SnapWhileResizing(e, ref m_rectangleBeforeSizeMove, MaxSnapDistance, InsensitiveBorderEndLength);
            }
        }
    }
}

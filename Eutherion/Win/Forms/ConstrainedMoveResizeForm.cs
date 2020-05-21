#region License
/*********************************************************************************
 * ConstrainedMoveResizeForm.cs
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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Eutherion.Win.Forms
{
    /// <summary>
    /// Windows <see cref="Form"/> with extra <see cref="OnMoving(ref RECT)"/> and <see cref="OnResizing(ref RECT, ResizeMode)"/> methods,
    /// which can be overridden to restrict the window rectangle while the user is moving and resizing the form.
    /// </summary>
    public class ConstrainedMoveResizeForm : Form
    {
        /// <summary>
        /// Expected displacement of the RECT parameter in WndProc(), resulting from continuous RECT updates while moving.
        /// </summary>
        Point m_displacement;

        // This method is called from the base WndProc() on the WM_ENTERSIZEMOVE message, i.e. always after the window starts sizing or moving.
        protected override void OnResizeBegin(EventArgs e)
        {
            base.OnResizeBegin(e);
            m_displacement.X = 0;
            m_displacement.Y = 0;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM.SIZING)
            {
                // Marshal the size/move rectangle from the message.
                RECT rc = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT));

                var e = new ResizeEventArgs(rc, (ResizeMode)m.WParam.ToInt32());
                OnResizing(e);

                // Marshal back the result.
                Marshal.StructureToPtr(e.MoveResizeRect, m.LParam, true);
            }
            else if (m.Msg == WM.MOVING)
            {
                // Marshal the size/move rectangle from the message.
                RECT rc = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT));

                // Add displacement resulting from continuous updates by OnMoving().
                rc.Left += m_displacement.X;
                rc.Top += m_displacement.Y;
                rc.Right += m_displacement.X;
                rc.Bottom += m_displacement.Y;

                var e = new MoveResizeEventArgs(rc);
                OnMoving(e);

                // Calculate new displacement for the next WndProc() iteration.
                m_displacement.X = rc.Left - e.MoveResizeRect.Left;
                m_displacement.Y = rc.Top - e.MoveResizeRect.Top;

                // Marshal back the result.
                Marshal.StructureToPtr(e.MoveResizeRect, m.LParam, true);
            }

            base.WndProc(ref m);
        }

        // This method is called from the base WndProc() on the WM_EXITSIZEMOVE message, i.e. always after the window stops sizing or moving.
        protected override void OnResizeEnd(EventArgs e)
        {
            m_displacement.X = 0;
            m_displacement.Y = 0;
            base.OnResizeEnd(e);
        }

        /// <summary>
        /// Occurs when the form is being moved.
        /// </summary>
        public event EventHandler<MoveResizeEventArgs> Moving;

        /// <summary>
        /// Raises the <see cref="Moving"/> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="MoveResizeEventArgs"/> containing the event data.
        /// </param>
        protected virtual void OnMoving(MoveResizeEventArgs e) => Moving?.Invoke(this, e);

        /// <summary>
        /// Occurs when the form is being resized.
        /// </summary>
        public event EventHandler<ResizeEventArgs> Resizing;

        /// <summary>
        /// Raises the <see cref="Resizing"/> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="ResizeEventArgs"/> containing the event data.
        /// </param>
        protected virtual void OnResizing(ResizeEventArgs e) => Resizing?.Invoke(this, e);
    }
}

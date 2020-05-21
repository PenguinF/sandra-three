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
            const int WM_SIZING = 0x214;
            const int WM_MOVING = 0x216;

            if (m.Msg == WM_SIZING)
            {
                // Marshal the size/move rectangle from the message.
                RECT rc = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT));

                OnResizing(ref rc, (ResizeMode)m.WParam.ToInt32());

                // Marshal back the result.
                Marshal.StructureToPtr(rc, m.LParam, true);
            }
            else if (m.Msg == WM_MOVING)
            {
                // Marshal the size/move rectangle from the message.
                RECT rc = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT));

                // Add displacement resulting from continuous updates by OnMoving().
                rc.Left += m_displacement.X;
                rc.Top += m_displacement.Y;
                rc.Right += m_displacement.X;
                rc.Bottom += m_displacement.Y;

                // Store result rectangle in a different variable, to be able to calculate the displacement later.
                RECT newRC = rc;

                OnMoving(ref newRC);

                // Calculate new displacement for the next WndProc() iteration.
                m_displacement.X = rc.Left - newRC.Left;
                m_displacement.Y = rc.Top - newRC.Top;

                // Marshal back the result.
                Marshal.StructureToPtr(newRC, m.LParam, true);
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
        /// Called when the form is being moved.
        /// </summary>
        /// <param name="moveRect">
        /// The bounding rectangle of the form, relative to the edges of the screen.
        /// </param>
        protected virtual void OnMoving(ref RECT moveRect)
        {
        }

        /// <summary>
        /// Called when the form is being resized.
        /// </summary>
        /// <param name="resizeRect">
        /// The bounding rectangle of the form, relative to the edges of the screen.
        /// </param>
        /// <param name="resizeMode">
        /// The mode in which the form is being resized.
        /// </param>
        protected virtual void OnResizing(ref RECT resizeRect, ResizeMode resizeMode)
        {
        }
    }
}

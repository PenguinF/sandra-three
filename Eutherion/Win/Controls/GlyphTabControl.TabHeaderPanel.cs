#region License
/*********************************************************************************
 * GlyphTabControl.TabHeaderPanel.cs
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
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Eutherion.Win.Controls
{
    public partial class GlyphTabControl
    {
        private class TabHeaderPanel : Control
        {
            private readonly GlyphTabControl OwnerTabControl;

            private int CurrentWidth;
            private int CurrentHeight;
            private float CurrentTabWidth;

            public TabHeaderPanel(GlyphTabControl ownerTabControl)
            {
                OwnerTabControl = ownerTabControl;

                SetStyle(ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.UserPaint
                    | ControlStyles.UserMouse
                    | ControlStyles.OptimizedDoubleBuffer
                    | ControlStyles.Opaque, true);

                SetStyle(ControlStyles.Selectable, false);
            }

            /// <summary>
            /// Called when the positions and sizes of tab headers are expected to change.
            /// </summary>
            public void UpdateMetrics()
            {
                CurrentWidth = ClientSize.Width;
                CurrentHeight = ClientSize.Height;
                int tabCount = OwnerTabControl.TabPages.Count;
                CurrentTabWidth = tabCount > 0 ? (float)CurrentWidth / tabCount : 0;
                if (OwnerTabControl.TabWidth < CurrentTabWidth) CurrentTabWidth = OwnerTabControl.TabWidth;

                Invalidate();
            }

            /// <summary>
            /// Called when the header must be redrawn, but the positions and sizes of tab headers has not changed.
            /// </summary>
            public void UpdateNonMetrics()
            {
                Invalidate();
            }

            protected override void OnLayout(LayoutEventArgs e) => UpdateMetrics();

            private Rectangle TabHeaderTextRectangle(int index) => new Rectangle(
                (int)Math.Round(index * CurrentTabWidth),
                0,
                (int)Math.Round(CurrentTabWidth),
                CurrentHeight);

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                Rectangle clientRectangle = ClientRectangle;

                // Block out the entire client area.
                using (var inactiveAreaBrush = new SolidBrush(OwnerTabControl.BackColor))
                {
                    g.FillRectangle(inactiveAreaBrush, clientRectangle);
                }

                // Then draw each tab page.
                for (int tabIndex = 0; tabIndex < OwnerTabControl.TabPages.Count; tabIndex++)
                {
                    TabPage tabPage = OwnerTabControl.TabPages[tabIndex];

                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    TextRenderer.DrawText(
                        g,
                        tabPage.Text,
                        OwnerTabControl.Font,
                        TabHeaderTextRectangle(tabIndex),
                        Color.Black,
                        OwnerTabControl.BackColor,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                }
            }
        }
    }
}

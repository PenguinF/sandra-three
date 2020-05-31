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
using System.Drawing.Drawing2D;
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
            private float CurrentHorizontalTabTextMargin;

            private Point LastKnownMouseMovePoint = new Point(-1, -1);

            private int HoverTabIndex = -1;

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
                CurrentHorizontalTabTextMargin = OwnerTabControl.HorizontalTabTextMargin;
                if (CurrentHorizontalTabTextMargin * 2 > CurrentTabWidth) CurrentHorizontalTabTextMargin = CurrentTabWidth / 2;

                // Must hit test again after updating the metrics.
                HitTest(MousePosition);

                Invalidate();
            }

            /// <summary>
            /// Called when the header must be redrawn, but the positions and sizes of tab headers has not changed.
            /// </summary>
            public void UpdateNonMetrics()
            {
                Invalidate();
            }

            private void HitTest(Point clientLocation)
            {
                int tabIndex = -1;

                if (clientLocation.Y >= 0 && clientLocation.Y < ClientSize.Height && CurrentTabWidth > 0)
                {
                    tabIndex = (int)Math.Floor(clientLocation.X / CurrentTabWidth);
                    if (tabIndex >= OwnerTabControl.TabPages.Count) tabIndex = -1;
                }

                if (HoverTabIndex != tabIndex)
                {
                    HoverTabIndex = tabIndex;
                    Invalidate();
                }
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                if (LastKnownMouseMovePoint.X >= 0 && LastKnownMouseMovePoint.Y >= 0)
                {
                    HitTest(LastKnownMouseMovePoint);
                }

                base.OnMouseEnter(e);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                HitTest(e.Location);

                if (HoverTabIndex >= 0 && e.Button == MouseButtons.Left)
                {
                    OwnerTabControl.TabHeaderClicked(HoverTabIndex);
                }

                base.OnMouseDown(e);
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                // Do a hit test, which updates hover information.
                HitTest(e.Location);

                // Remember position for mouse-enters without mouse-leaves.
                LastKnownMouseMovePoint = e.Location;

                base.OnMouseMove(e);
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                HitTest(e.Location);
                base.OnMouseUp(e);
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                // Hit test a position outside of the control to reset the hover tab index and raise proper events.
                LastKnownMouseMovePoint = new Point(-1, -1);
                HitTest(LastKnownMouseMovePoint);

                base.OnMouseLeave(e);
            }

            protected override void OnLayout(LayoutEventArgs e) => UpdateMetrics();

            private Rectangle TabHeaderTextRectangle(int index) => new Rectangle(
                (int)Math.Round(index * CurrentTabWidth + CurrentHorizontalTabTextMargin),
                0,
                (int)Math.Round(CurrentTabWidth - CurrentHorizontalTabTextMargin * 2),
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

                    // Remember some things for drawing text later.
                    Color tabBackColor;
                    Color tabForeColor;

                    if (tabIndex == OwnerTabControl.ActiveTabPageIndex)
                    {
                        using (var activeTabHeaderBrush = new SolidBrush(tabPage.ActiveBackColor))
                        {
                            g.FillRectangle(activeTabHeaderBrush, new RectangleF(tabIndex * CurrentTabWidth, 0, CurrentTabWidth, CurrentHeight));
                        }

                        tabBackColor = tabPage.ActiveBackColor;
                        tabForeColor = tabPage.ActiveForeColor;
                    }
                    else if (tabIndex == HoverTabIndex)
                    {
                        using (var hoverBrush = new SolidBrush(OwnerTabControl.InactiveTabHeaderHoverColor))
                        {
                            g.FillRectangle(hoverBrush, tabIndex * CurrentTabWidth, 0, CurrentTabWidth, CurrentHeight);
                        }

                        // Drawing rectangles with a Pen includes the right border, so subtract 1 from the width.
                        using (var hoverBorderPen = new Pen(OwnerTabControl.InactiveTabHeaderHoverBorderColor, 1))
                        {
                            g.DrawRectangle(hoverBorderPen, tabIndex * CurrentTabWidth, 0, CurrentTabWidth - 1, CurrentHeight);
                        }

                        tabBackColor = OwnerTabControl.InactiveTabHeaderHoverColor;
                        tabForeColor = OwnerTabControl.ForeColor;
                    }
                    else
                    {
                        tabBackColor = OwnerTabControl.BackColor;
                        tabForeColor = OwnerTabControl.ForeColor;
                    }

                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    TextRenderer.DrawText(
                        g,
                        tabPage.Text,
                        OwnerTabControl.Font,
                        TabHeaderTextRectangle(tabIndex),
                        tabForeColor,
                        tabBackColor,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                    g.SmoothingMode = SmoothingMode.None;
                }
            }
        }
    }
}

﻿#region License
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
            private static readonly string CloseButtonGlyph = "×";

            // This more or less puts the '×' in the center, with the used font family.
            private static readonly float GlyphFontToHeightRatio = 2.2f;

            // If e.g. the glyph takes up 12 horizontal pixels, about a tenth of it is expected to be a unit of padding,
            // with one padding unit to the left, and 2 to the right.
            // This means that the character is estimated to be around 8.4 pixels wide.
            private static readonly float EstimatedHorizontalGlyphPaddingRatio = 10f;

            private readonly GlyphTabControl OwnerTabControl;
            private readonly ToolTip ToolTip;

            private int CurrentWidth;
            private int CurrentHeight;
            private float CurrentTabWidth;
            private float CurrentHorizontalTabTextMargin;
            private int CurrentTextAreaWidthIncludeGlyph;

            private Font CurrentCloseButtonGlyphFont;
            private bool NeedNewGlyphSizeMeasurement = true;
            private Size MeasuredGlyphSize;

            private Point LastKnownMouseMovePoint = new Point(-1, -1);

            private int HoverTabIndex = -1;
            private bool HoverOverGlyph;

            public TabHeaderPanel(GlyphTabControl ownerTabControl)
            {
                OwnerTabControl = ownerTabControl;
                ToolTip = new ToolTip();

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
                int previousHeight = CurrentHeight;
                CurrentWidth = ClientSize.Width;
                CurrentHeight = ClientSize.Height;
                int tabCount = OwnerTabControl.TabPages.Count;
                CurrentTabWidth = tabCount > 0 ? (float)CurrentWidth / tabCount : 0;
                if (OwnerTabControl.TabWidth < CurrentTabWidth) CurrentTabWidth = OwnerTabControl.TabWidth;
                CurrentHorizontalTabTextMargin = OwnerTabControl.HorizontalTabTextMargin;
                if (CurrentHorizontalTabTextMargin * 2 > CurrentTabWidth) CurrentHorizontalTabTextMargin = CurrentTabWidth / 2;

                // Calculate text area width for each tab page.
                CurrentTextAreaWidthIncludeGlyph = (int)Math.Round(CurrentTabWidth - CurrentHorizontalTabTextMargin * 2);

                // Glyph font height little less than half the tab height.
                if (previousHeight != CurrentHeight || CurrentCloseButtonGlyphFont == null)
                {
                    CurrentCloseButtonGlyphFont?.Dispose();
                    CurrentCloseButtonGlyphFont = new Font("Segoe UI", CurrentHeight / GlyphFontToHeightRatio, FontStyle.Bold, GraphicsUnit.Point);

                    // Without the Graphics device context which is available in the Paint event handler, we cannot measure
                    // the glyph size accurately here. Unfortunately, having an updated glyph size is necessary for
                    // the follow up hit test, as the metrics can change after e.g. closing a tab page.
                    // Can reasonably assume that the tab header height doesn't change, so just take an educated guess.
                    // Hit testing can only be done after a first Paint, so if the previous height was 0 we can ignore it.
                    NeedNewGlyphSizeMeasurement = true;
                    if (previousHeight > 0 && CurrentHeight > 0 && MeasuredGlyphSize.Width > 0 && MeasuredGlyphSize.Height > 0)
                    {
                        // Scale by the height growth.
                        MeasuredGlyphSize = new Size(
                            MeasuredGlyphSize.Width * CurrentHeight / previousHeight,
                            MeasuredGlyphSize.Height * CurrentHeight / previousHeight);
                    }
                }

                // Must hit test again after updating the metrics.
                HitTest(MousePosition);

                Invalidate();
            }

            /// <summary>
            /// Called when the header must be redrawn, but the positions and sizes of tab headers has not changed.
            /// </summary>
            public void UpdateNonMetrics()
            {
                ToolTip.SetToolTip(this, HoverTabIndex >= 0 ? OwnerTabControl.TabPages[HoverTabIndex].Text : null);
                Invalidate();
            }

            private void HitTest(Point clientLocation)
            {
                int tabIndex = -1;
                bool overGlyph = false;

                if (clientLocation.Y >= 0 && clientLocation.Y < ClientSize.Height && CurrentTabWidth > 0)
                {
                    tabIndex = (int)Math.Floor(clientLocation.X / CurrentTabWidth);

                    if (tabIndex < OwnerTabControl.TabPages.Count)
                    {
                        float relativeX = clientLocation.X - tabIndex * CurrentTabWidth;

                        // Between the right edge of the text and the edge of where the margin starts.
                        overGlyph
                            = CurrentTabWidth - CurrentHorizontalTabTextMargin - MeasuredGlyphSize.Width < relativeX
                            && relativeX < CurrentTabWidth - CurrentHorizontalTabTextMargin;
                    }
                    else
                    {
                        tabIndex = -1;
                    }
                }

                if (HoverTabIndex != tabIndex || HoverOverGlyph != overGlyph)
                {
                    if (HoverTabIndex != tabIndex) ToolTip.SetToolTip(this, tabIndex >= 0 ? OwnerTabControl.TabPages[tabIndex].Text : null);

                    HoverTabIndex = tabIndex;
                    HoverOverGlyph = overGlyph;
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
                    OwnerTabControl.TabHeaderClicked(HoverTabIndex, HoverOverGlyph);
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

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                Rectangle clientRectangle = ClientRectangle;

                if (NeedNewGlyphSizeMeasurement)
                {
                    MeasuredGlyphSize = TextRenderer.MeasureText(
                        g,
                        CloseButtonGlyph,
                        CurrentCloseButtonGlyphFont,
                        Size.Empty,
                        TextFormatFlags.NoPadding);
                    NeedNewGlyphSizeMeasurement = false;
                }

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
                    Color glyphForeColor;
                    bool drawCloseButtonGlyph;

                    // True if hovering over glyph of the current tab page to draw.
                    bool hoverOverThisTabPageGlyph = tabIndex == HoverTabIndex && HoverOverGlyph;

                    if (tabIndex == OwnerTabControl.ActiveTabPageIndex)
                    {
                        using (var activeTabHeaderBrush = new SolidBrush(tabPage.ActiveBackColor))
                        {
                            g.FillRectangle(activeTabHeaderBrush, new RectangleF(tabIndex * CurrentTabWidth, 0, CurrentTabWidth, CurrentHeight));
                        }

                        tabBackColor = tabPage.ActiveBackColor;
                        tabForeColor = tabPage.ActiveForeColor;

                        // Only show glyph for active and hover tabs.
                        drawCloseButtonGlyph = true;
                        glyphForeColor = tabPage.GlyphForeColor.A == 0
                            ? tabForeColor
                            : tabPage.GlyphForeColor;

                        // Highlight when hovering.
                        if (hoverOverThisTabPageGlyph)
                        {
                            // Default to a lighter version if no color given.
                            glyphForeColor = tabPage.GlyphHoverColor.A == 0
                                ? ControlPaint.Light(glyphForeColor)
                                : tabPage.GlyphHoverColor;
                        }
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

                        // Only show glyph for active and hover tabs.
                        drawCloseButtonGlyph = true;
                        glyphForeColor = OwnerTabControl.InactiveTabHeaderGlyphForeColor.A == 0
                            ? tabForeColor
                            : OwnerTabControl.InactiveTabHeaderGlyphForeColor;

                        // Highlight when hovering.
                        if (hoverOverThisTabPageGlyph)
                        {
                            // Default to a lighter version if no color given.
                            glyphForeColor = OwnerTabControl.InactiveTabHeaderGlyphHoverColor.A == 0
                                ? ControlPaint.Light(glyphForeColor)
                                : OwnerTabControl.InactiveTabHeaderGlyphHoverColor;
                        }
                    }
                    else
                    {
                        tabBackColor = OwnerTabControl.BackColor;
                        tabForeColor = OwnerTabControl.ForeColor;
                        drawCloseButtonGlyph = false;
                        glyphForeColor = tabForeColor;  // Not used.
                    }

                    int textAreaLeftOffset = (int)Math.Floor(tabIndex * CurrentTabWidth + CurrentHorizontalTabTextMargin);
                    int textAreaWidth = CurrentTextAreaWidthIncludeGlyph;
                    if (drawCloseButtonGlyph) textAreaWidth -= MeasuredGlyphSize.Width;

                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    TextRenderer.DrawText(
                        g,
                        tabPage.Text,
                        OwnerTabControl.Font,
                        new Rectangle(textAreaLeftOffset, 0, textAreaWidth, CurrentHeight),
                        tabForeColor,
                        tabBackColor,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

                    if (drawCloseButtonGlyph)
                    {
                        if (!hoverOverThisTabPageGlyph && tabPage.IsModified)
                        {
                            // Subtract the padding, one from the left, two from the right side.
                            float hrzPadding = MeasuredGlyphSize.Width / EstimatedHorizontalGlyphPaddingRatio;
                            float diameter = MeasuredGlyphSize.Width - 3 * hrzPadding;

                            RectangleF glyphRectangle = new RectangleF(
                                textAreaLeftOffset + textAreaWidth + hrzPadding,
                                (CurrentHeight - diameter) / 2,
                                diameter,
                                diameter);

                            // Draw a circle where otherwise the '×' would be.
                            using (var ellipseBrush = new SolidBrush(glyphForeColor))
                            {
                                g.FillEllipse(ellipseBrush, glyphRectangle);
                            }
                        }
                        else
                        {
                            // Make rectangle 2 pixels less high to not interfere with hover border.
                            Rectangle glyphRectangle = new Rectangle(
                                textAreaLeftOffset + textAreaWidth,
                                1,
                                MeasuredGlyphSize.Width,
                                MeasuredGlyphSize.Height - 2);

                            TextRenderer.DrawText(
                                g,
                                CloseButtonGlyph,
                                CurrentCloseButtonGlyphFont,
                                glyphRectangle,
                                glyphForeColor,
                                tabBackColor,
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                        }
                    }

                    g.SmoothingMode = SmoothingMode.None;
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    ToolTip.Dispose();
                    CurrentCloseButtonGlyphFont?.Dispose();
                }

                base.Dispose(disposing);
            }
        }
    }
}

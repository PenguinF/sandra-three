#region License
/*********************************************************************************
 * GlyphTabControl.Drawing.cs
 *
 * Copyright (c) 2004-2022 Henk Nicolai
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
        /// <summary>
        /// Contains the result of a call to <see cref="HitTest(Point)"/>.
        /// </summary>
        public struct HitTestResult
        {
            /// <summary>
            /// True if the client location is over the header area, otherwise false.
            /// </summary>
            public bool OverHeaderArea;

            /// <summary>
            /// If there's tab header under the client location, contains the index of that tab header. Is -1 otherwise.
            /// </summary>
            public int TabIndex;

            /// <summary>
            /// True if <see cref="TabIndex"/> is 0 or greater and the given client location is over the tab header's glyph
            /// with that index, otherwise false.
            /// </summary>
            public bool OverGlyph;
        }

        private static readonly string CloseButtonGlyph = "×";

        // This more or less puts the '×' in the center, with the used font family.
        private static readonly float GlyphFontToHeightRatio = 2.2f;

        // If e.g. the glyph takes up 12 horizontal pixels, about a tenth of it is expected to be a unit of padding,
        // with one padding unit to the left, and 2 to the right.
        // This means that the character is estimated to be around 8.4 pixels wide.
        private static readonly float EstimatedHorizontalGlyphPaddingRatio = 10f;

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
        private int GlyphPressedIndex = -1;

        /// <summary>
        /// Called when the positions and sizes of tab headers are expected to change.
        /// </summary>
        private void UpdateMetrics()
        {
            int previousHeight = CurrentHeight;
            CurrentWidth = ClientSize.Width;
            CurrentHeight = TabHeaderHeight;
            int tabCount = TabPages.Count;
            CurrentTabWidth = tabCount > 0 ? (float)CurrentWidth / tabCount : 0;
            if (TabWidth < CurrentTabWidth) CurrentTabWidth = TabWidth;
            CurrentHorizontalTabTextMargin = HorizontalTabTextMargin;
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
            ProcessHitTest(MousePosition);

            Invalidate();
        }

        /// <summary>
        /// Called when the header must be redrawn, but the positions and sizes of tab headers has not changed.
        /// </summary>
        private void UpdateNonMetrics()
        {
            ToolTip.SetToolTip(this, HoverTabIndex >= 0 ? TabPages[HoverTabIndex].Text : null);
            Invalidate();
        }

        /// <summary>
        /// Returns information about a location relative to the top left corner of this control.
        /// </summary>
        /// <param name="clientLocation">
        /// The location to examine.
        /// </param>
        /// <returns>
        /// The <see cref="HitTestResult"/> which contains information about whether the location is over the tab header area,
        /// which tab header it is over, and whether or not it is over a tab header's glyph.
        /// </returns>
        public HitTestResult HitTest(Point clientLocation)
        {
            HitTestResult result = new HitTestResult { TabIndex = -1 };

            if (clientLocation.Y >= 0 && clientLocation.Y < TabHeaderHeight && CurrentTabWidth > 0)
            {
                result.OverHeaderArea = true;
                result.TabIndex = (int)Math.Floor(clientLocation.X / CurrentTabWidth);

                if (result.TabIndex < TabPages.Count)
                {
                    float relativeX = clientLocation.X - result.TabIndex * CurrentTabWidth;

                    // Between the right edge of the text and the edge of where the margin starts.
                    result.OverGlyph
                       = CurrentTabWidth - CurrentHorizontalTabTextMargin - MeasuredGlyphSize.Width < relativeX
                       && relativeX < CurrentTabWidth - CurrentHorizontalTabTextMargin;
                }
                else
                {
                    result.TabIndex = -1;
                }
            }

            return result;
        }

        private void ProcessHitTest(Point clientLocation)
        {
            HitTestResult result = HitTest(clientLocation);

            if (HoverTabIndex != result.TabIndex || HoverOverGlyph != result.OverGlyph)
            {
                if (HoverTabIndex != result.TabIndex) ToolTip.SetToolTip(this, result.TabIndex >= 0 ? TabPages[result.TabIndex].Text : null);

                HoverTabIndex = result.TabIndex;
                HoverOverGlyph = result.OverGlyph;
                Invalidate();
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (LastKnownMouseMovePoint.X >= 0 && LastKnownMouseMovePoint.Y >= 0)
            {
                ProcessHitTest(LastKnownMouseMovePoint);
            }

            base.OnMouseEnter(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            ProcessHitTest(e.Location);

            if (HoverTabIndex >= 0 && e.Button == MouseButtons.Left)
            {
                if (!HoverOverGlyph)
                {
                    // Default is to activate the tab.
                    ActivateTab(HoverTabIndex);
                }
                else
                {
                    // Remember which glyph is being clicked.
                    GlyphPressedIndex = HoverTabIndex;
                    Invalidate();
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            // Do a hit test, which updates hover information.
            ProcessHitTest(e.Location);

            // Remember position for mouse-enters without mouse-leaves.
            LastKnownMouseMovePoint = e.Location;

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            ProcessHitTest(e.Location);

            if (e.Button == MouseButtons.Left && GlyphPressedIndex >= 0)
            {
                // Close the tab page if releasing the mouse over the same glyph.
                if (HoverTabIndex == GlyphPressedIndex && HoverOverGlyph)
                {
                    CloseTab(GlyphPressedIndex);
                }

                GlyphPressedIndex = -1;
                Invalidate();
            }

            base.OnMouseUp(e);
        }

        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            if (!Capture)
            {
                // If an actual mouse-up happens, OnMouseUp is called before OnMouseCaptureChanged.
                // Otherwise, something else captured the mouse while moving, which means mouse-down info should be reset.
                if (GlyphPressedIndex >= 0)
                {
                    GlyphPressedIndex = -1;
                    Invalidate();
                }

                ProcessHitTest(MousePosition);
            }

            base.OnMouseCaptureChanged(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            // Hit test a position outside of the control to reset the hover tab index and raise proper events.
            LastKnownMouseMovePoint = new Point(-1, -1);
            ProcessHitTest(LastKnownMouseMovePoint);

            base.OnMouseLeave(e);
        }

        /// <summary>
        /// Raises the <see cref="Control.Layout"/> event.
        /// </summary>
        /// <param name="e">
        /// A <see cref="LayoutEventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLayout(LayoutEventArgs e)
        {
            UpdateMetrics();

            var clientSize = ClientSize;

            foreach (var tabPage in TabPages)
            {
                tabPage.ClientControl.SetBounds(0, TabHeaderHeight, clientSize.Width, clientSize.Height - TabHeaderHeight);
            }

            base.OnLayout(e);
        }

        /// <summary>
        /// Paints the background of the control.
        /// </summary>
        /// <param name="e">
        /// A <see cref="PaintEventArgs"/> that contains the event data.
        /// </param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (ActiveTabPageIndex < 0)
            {
                // Default behavior if there is no control to display in the tab client area.
                var clientSize = ClientSize;

                e.Graphics.FillRectangle(Brushes.Black, new Rectangle(
                    0, TabHeaderHeight, clientSize.Width, clientSize.Height - TabHeaderHeight));
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

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
            using (var inactiveAreaBrush = new SolidBrush(BackColor))
            {
                g.FillRectangle(inactiveAreaBrush, new Rectangle(0, 0, ClientSize.Width, TabHeaderHeight));
            }

            // Then draw each tab page.
            for (int tabIndex = 0; tabIndex < TabPages.Count; tabIndex++)
            {
                TabPage tabPage = TabPages[tabIndex];

                // Remember some things for drawing text later.
                Color tabBackColor;
                Color tabForeColor;
                Color glyphForeColor;
                Color glyphPressedBackColor;
                bool drawModifiedGlyph;
                bool drawCloseButtonGlyph;

                bool hoverOverThisTabPageGlyph = tabIndex == HoverTabIndex && HoverOverGlyph;
                bool drawGlyphPressed = hoverOverThisTabPageGlyph && tabIndex == GlyphPressedIndex;
                bool drawGlyphHighlight = hoverOverThisTabPageGlyph && GlyphPressedIndex == -1 || tabIndex == GlyphPressedIndex;

                if (tabIndex == ActiveTabPageIndex)
                {
                    using (var activeTabHeaderBrush = new SolidBrush(tabPage.ActiveBackColor))
                    {
                        g.FillRectangle(activeTabHeaderBrush, new RectangleF(tabIndex * CurrentTabWidth, 0, CurrentTabWidth, TabHeaderHeight));
                    }

                    tabBackColor = tabPage.ActiveBackColor;
                    tabForeColor = tabPage.ActiveForeColor;
                    glyphPressedBackColor = tabPage.ActiveBackColor.GetBrightness() >= 0.5f
                        ? ControlPaint.Dark(tabPage.ActiveBackColor)
                        : ControlPaint.Light(tabPage.ActiveBackColor);

                    glyphForeColor = tabPage.GlyphForeColor.A == 0
                        ? tabForeColor
                        : tabPage.GlyphForeColor;

                    if (drawGlyphHighlight)
                    {
                        // Highlight when hovering or the glyph button is pressed.
                        // Default to a lighter version if no color given.
                        glyphForeColor = tabPage.GlyphHoverColor.A == 0
                            ? ControlPaint.Light(glyphForeColor)
                            : tabPage.GlyphHoverColor;

                        drawModifiedGlyph = false;
                        drawCloseButtonGlyph = true;
                    }
                    else
                    {
                        drawModifiedGlyph = tabPage.IsModified;
                        drawCloseButtonGlyph = !tabPage.IsModified;
                    }
                }
                else
                {
                    if (tabIndex == HoverTabIndex)
                    {
                        using (var hoverBrush = new SolidBrush(InactiveTabHeaderHoverColor))
                        {
                            g.FillRectangle(hoverBrush, tabIndex * CurrentTabWidth, 0, CurrentTabWidth, TabHeaderHeight);
                        }

                        // Drawing rectangles with a Pen includes the right border, so subtract 1 from the width.
                        using (var hoverBorderPen = new Pen(InactiveTabHeaderHoverBorderColor, 1))
                        {
                            g.DrawRectangle(hoverBorderPen, tabIndex * CurrentTabWidth, 0, CurrentTabWidth - 1, TabHeaderHeight);
                        }

                        tabBackColor = InactiveTabHeaderHoverColor;
                    }
                    else
                    {
                        tabBackColor = BackColor;
                    }

                    tabForeColor = ForeColor;
                    glyphPressedBackColor = BackColor;

                    glyphForeColor = InactiveTabHeaderGlyphForeColor.A == 0
                        ? tabForeColor
                        : InactiveTabHeaderGlyphForeColor;

                    if (drawGlyphHighlight)
                    {
                        // Highlight when hovering or the glyph button is pressed.
                        // Default to a lighter version if no color given.
                        glyphForeColor = InactiveTabHeaderGlyphHoverColor.A == 0
                            ? ControlPaint.Light(glyphForeColor)
                            : InactiveTabHeaderGlyphHoverColor;

                        drawModifiedGlyph = false;
                        drawCloseButtonGlyph = true;
                    }
                    else
                    {
                        drawModifiedGlyph = tabPage.IsModified;
                        drawCloseButtonGlyph = !drawModifiedGlyph && tabIndex == HoverTabIndex;
                    }
                }

                int textAreaLeftOffset = (int)Math.Floor(tabIndex * CurrentTabWidth + CurrentHorizontalTabTextMargin);
                int textAreaWidth = CurrentTextAreaWidthIncludeGlyph;
                if (drawModifiedGlyph || drawCloseButtonGlyph) textAreaWidth -= MeasuredGlyphSize.Width;

                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                TextRenderer.DrawText(
                    g,
                    tabPage.Text,
                    Font,
                    new Rectangle(textAreaLeftOffset, 0, textAreaWidth, TabHeaderHeight),
                    tabForeColor,
                    tabBackColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

                if (drawModifiedGlyph || drawCloseButtonGlyph)
                {
                    // Subtract the padding, one from the left, two from the right side.
                    float hrzPadding = MeasuredGlyphSize.Width / EstimatedHorizontalGlyphPaddingRatio;
                    float diameter = MeasuredGlyphSize.Width - 3 * hrzPadding;

                    // Prevent drawing outside the glyph rectangle.
                    float targetHeight = Math.Min(MeasuredGlyphSize.Height + hrzPadding * 2, TabHeaderHeight - hrzPadding * 2);

                    RectangleF backgroundGlyphRectangle = new RectangleF(
                        textAreaLeftOffset + textAreaWidth - hrzPadding,
                        (TabHeaderHeight - targetHeight) / 2,
                        MeasuredGlyphSize.Width + hrzPadding * 2,
                        targetHeight);

                    g.SetClip(backgroundGlyphRectangle);

                    if (drawModifiedGlyph)
                    {
                        RectangleF modifiedGlyphRectangle = new RectangleF(
                            textAreaLeftOffset + textAreaWidth + hrzPadding,
                            (TabHeaderHeight - diameter) / 2,
                            diameter,
                            diameter);

                        // Draw a circle where otherwise the '×' would be.
                        using (var ellipseBrush = new SolidBrush(glyphForeColor))
                        {
                            g.FillEllipse(ellipseBrush, modifiedGlyphRectangle);
                        }
                    }
                    else if (drawCloseButtonGlyph)
                    {
                        if (drawGlyphPressed)
                        {
                            g.SmoothingMode = SmoothingMode.None;

                            using (var backgroundBrush = new SolidBrush(glyphPressedBackColor))
                            {
                                g.FillRectangle(backgroundBrush, backgroundGlyphRectangle);
                            }

                            g.SmoothingMode = SmoothingMode.AntiAlias;
                        }

                        // Make rectangle 2 pixels less high to not interfere with hover border.
                        Rectangle glyphTextRectangle = new Rectangle(
                            textAreaLeftOffset + textAreaWidth,
                            1,
                            MeasuredGlyphSize.Width,
                            MeasuredGlyphSize.Height - 2);

                        TextRenderer.DrawText(
                            g,
                            CloseButtonGlyph,
                            CurrentCloseButtonGlyphFont,
                            glyphTextRectangle,
                            glyphForeColor,
                            drawGlyphPressed ? glyphPressedBackColor : tabBackColor,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.PreserveGraphicsClipping);
                    }

                    g.ResetClip();
                }

                g.SmoothingMode = SmoothingMode.None;
            }
        }
    }
}

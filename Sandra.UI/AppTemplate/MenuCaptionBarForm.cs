#region License
/*********************************************************************************
 * MenuCaptionBarForm.cs
 *
 * Copyright (c) 2004-2019 Henk Nicolai
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

using Eutherion.Win.Controls;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace Eutherion.Win.AppTemplate
{
    /// <summary>
    /// <see cref="UIActionForm"/> which displays a main menu inside a custom caption bar.
    /// It is advisable to give the main menu strip a back color so it does not display
    /// a gradient which clashes with the custom drawn caption bar area.
    /// </summary>
    public class MenuCaptionBarForm : UIActionForm
    {
        private const int MainMenuHorizontalMargin = 8;

        private const int buttonOuterRightMargin = 12;
        private const int captionButtonSize = 24;

        private readonly NonSelectableButton minimizeButton;
        private readonly NonSelectableButton maximizeButton;

        public MenuCaptionBarForm()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.UserMouse
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.FixedHeight
                | ControlStyles.FixedWidth
                | ControlStyles.ResizeRedraw
                | ControlStyles.Opaque, true);

            ControlBox = false;
            FormBorderStyle = FormBorderStyle.None;

            minimizeButton = CreateCaptionButton(SharedResources.minimize);
            minimizeButton.Click += (_, __) => WindowState = FormWindowState.Minimized;

            maximizeButton = CreateCaptionButton(SharedResources.maximize);

            SuspendLayout();

            Controls.Add(minimizeButton);
            Controls.Add(maximizeButton);

            ResumeLayout();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                // Enables borders regardless of ControlBox and FormBorderStyle settings.
                const int WS_SIZEBOX = 0x40000;
                CreateParams cp = base.CreateParams;
                cp.Style |= WS_SIZEBOX;
                return cp;
            }
        }

        private NonSelectableButton CreateCaptionButton(Image icon)
        {
            var button = new NonSelectableButton
            {
                Image = icon,
                ImageAlign = ContentAlignment.MiddleCenter,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0),
                Padding = new Padding(0),
                TabStop = false,
            };

            button.FlatAppearance.BorderSize = 0;

            return button;
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            if (e.Control == MainMenuStrip)
            {
                MainMenuStrip.BackColorChanged += MainMenuStrip_BackColorChanged;
                UpdateCaptionAreaButtonsBackColor();
            }
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            if (e.Control == MainMenuStrip)
            {
                MainMenuStrip.BackColorChanged -= MainMenuStrip_BackColorChanged;
            }

            base.OnControlRemoved(e);
        }

        private void MainMenuStrip_BackColorChanged(object sender, EventArgs e) => UpdateCaptionAreaButtonsBackColor();

        private void UpdateCaptionAreaButtonsBackColor()
        {
            minimizeButton.BackColor = MainMenuStrip.BackColor;
            maximizeButton.BackColor = MainMenuStrip.BackColor;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            if (MainMenuStrip != null && MainMenuStrip.Items.Count > 0)
            {
                MainMenuStrip.Width = MainMenuHorizontalMargin +
                    MainMenuStrip.Items
                    .OfType<ToolStripItem>()
                    .Where(x => x.Visible)
                    .Select(x => x.Width)
                    .Sum();

                // Calculate top edge position for all caption buttons: 1 pixel above center.
                int topEdge = MainMenuStrip.Height - captionButtonSize - 2;
                topEdge = topEdge < 0 ? 0 : topEdge / 2;

                // Use a vertical edge variable so buttons can be placed from right to left.
                int currentVerticalEdge = Width - captionButtonSize - buttonOuterRightMargin;

                maximizeButton.SetBounds(
                    currentVerticalEdge,
                    topEdge,
                    captionButtonSize,
                    captionButtonSize);

                currentVerticalEdge = currentVerticalEdge - captionButtonSize;

                minimizeButton.SetBounds(
                    currentVerticalEdge,
                    topEdge,
                    captionButtonSize,
                    captionButtonSize);
            }
            else
            {
                // Don't mess with visibility, so put buttons outside of the client rectangle.
                minimizeButton.SetBounds(-2, -2, 1, 1);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (MainMenuStrip != null && MainMenuStrip.Items.Count > 0)
            {
                var g = e.Graphics;
                int width = Width;
                int mainMenuWidth = MainMenuStrip.Width;

                using (var captionAreaColorBrush = new SolidBrush(MainMenuStrip.BackColor))
                {
                    g.FillRectangle(captionAreaColorBrush, new Rectangle(mainMenuWidth, 0, width - mainMenuWidth, MainMenuStrip.Height));
                }

                string text = Text;

                if (!string.IsNullOrWhiteSpace(text))
                {
                    // Take Y and Height from first menu item so text can be aligned with it.
                    var firstMenuItemBounds = MainMenuStrip.Items[0].Bounds;
                    var textAreaRectangle = new Rectangle(0, firstMenuItemBounds.Y, width, firstMenuItemBounds.Height);

                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    TextRenderer.DrawText(
                        g,
                        text,
                        MainMenuStrip.Font,
                        textAreaRectangle,
                        MainMenuStrip.ForeColor,
                        MainMenuStrip.BackColor,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;

            const int HTCAPTION = 2;

            if (m.Msg == WM_NCHITTEST && MainMenuStrip != null && MainMenuStrip.Items.Count > 0)
            {
                Point position = PointToClient(new Point(m.LParam.ToInt32()));

                if (position.Y >= 0
                    && position.Y < MainMenuStrip.Height
                    && position.X >= 0
                    && position.X < ClientSize.Width)
                {
                    // This is the draggable 'caption' area.
                    m.Result = (IntPtr)HTCAPTION;
                    return;
                }
            }

            base.WndProc(ref m);
        }
    }
}

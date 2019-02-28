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

using System.Drawing;
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
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (MainMenuStrip != null && MainMenuStrip.Items.Count > 0)
            {
                var g = e.Graphics;
                var mainMenuWidth = MainMenuStrip.Width;

                using (var captionAreaColorBrush = new SolidBrush(MainMenuStrip.BackColor))
                {
                    g.FillRectangle(captionAreaColorBrush, new Rectangle(mainMenuWidth, 0, Width - MainMenuStrip.Width, MainMenuStrip.Height));
                }
            }
        }
    }
}

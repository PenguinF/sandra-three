﻿#region License
/*********************************************************************************
 * MenuCaptionBarFormStyle.cs
 *
 * Copyright (c) 2004-2021 Henk Nicolai
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

using Eutherion.Win.Utils;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Eutherion.Win.MdiAppTemplate
{
    /// <summary>
    /// Holds the current style properties of a <see cref="MenuCaptionBarForm"/>, and exposes an event to signal
    /// a change, for example if the system user preferences changed, or the window was activated.
    /// </summary>
    public class MenuCaptionBarFormStyle : IDisposable, IWeakEventTarget
    {
        public bool InDarkMode { get; private set; }
        public Color BackColor { get; private set; }
        public Color ForeColor { get; private set; }

        private bool isActive;
        public bool IsActive { get => isActive; set { if (isActive != value) { isActive = value; Recalculate(); } } }

        public Color HoverColor { get; private set; }
        public Color HoverBorderColor { get; private set; }

        public Color SuggestedTransparencyKey { get; private set; }

        public Font Font { get; private set; }

        public bool IsDisposed { get; private set; }

        public MenuCaptionBarFormStyle()
        {
            ThemeHelper.UserPreferencesChanged += ThemeHelper_UserPreferencesChanged;

            // Initial calculation to get into a valid state; won't raise the events.
            Recalculate();
        }

        public event EventHandler NotifyChange;

        private void Recalculate()
        {
            BackColor = ThemeHelper.GetDwmAccentColor(isActive);
            InDarkMode = BackColor.GetBrightness() < 0.5f;
            ForeColor = !isActive ? SystemColors.GrayText : InDarkMode ? Color.White : Color.Black;

            Font = SystemFonts.IconTitleFont;

            // Create a dummy MainMenuStrip just so we can borrow the system defined color table.
            using (var menuStrip = new MenuStrip())
            {
                if (menuStrip.Renderer is ToolStripProfessionalRenderer professionalRenderer)
                {
                    HoverColor = professionalRenderer.ColorTable.ButtonSelectedHighlight;
                    HoverBorderColor = professionalRenderer.ColorTable.ButtonSelectedBorder;
                }
            }

            // Choose a transparency key different from all the calculated colors.
            // We need 5 candidates, one of them is different from the 4 it's compared to.
            var candidateTransparencyKeys = new[] { Color.Red, Color.Green, Color.Blue, Color.Orange };
            SuggestedTransparencyKey = Color.Yellow;
            foreach (var candidate in candidateTransparencyKeys)
            {
                if (candidate != BackColor && candidate != ForeColor && candidate != HoverBorderColor && candidate != HoverColor)
                {
                    SuggestedTransparencyKey = candidate;
                    break;
                }
            }

            NotifyChange?.Invoke(this, EventArgs.Empty);
        }

        private void ThemeHelper_UserPreferencesChanged(_void sender, EventArgs e) => Recalculate();

        public void Dispose()
        {
            // Don't raise any events after a dispose.
            NotifyChange = null;
            ThemeHelper.UserPreferencesChanged -= ThemeHelper_UserPreferencesChanged;
            IsDisposed = true;
        }
    }
}

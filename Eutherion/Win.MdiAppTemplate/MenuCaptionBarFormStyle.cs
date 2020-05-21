#region License
/*********************************************************************************
 * MenuCaptionBarFormStyle.cs
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

using Eutherion.Utils;
using Eutherion.Win.Utils;
using System;
using System.Drawing;

namespace Eutherion.Win.MdiAppTemplate
{
    /// <summary>
    /// Holds the current style properties of a <see cref="MenuCaptionBarForm"/>, and exposes an event to signal
    /// a change, for example if the system user preferences changed, or the window was activated.
    /// </summary>
    public class MenuCaptionBarFormStyle : IDisposable, IWeakEventTarget
    {
        private int blockNotifyChangeCounter;

        public bool InDarkMode { get; private set; }
        public Color BackColor { get; private set; }
        public Color ForeColor { get; private set; }

        private bool isActive;
        public bool IsActive { get => isActive; set { if (isActive != value) { isActive = value; Recalculate(); } } }

        private Color hoverColor;
        public Color HoverColor { get => hoverColor; set { if (hoverColor != value) { hoverColor = value; Recalculate(); } } }

        private Color hoverBorderColor;
        public Color HoverBorderColor { get => hoverBorderColor; set { if (hoverBorderColor != value) { hoverBorderColor = value; Recalculate(); } } }

        public Color SuggestedTransparencyKey { get; private set; }

        private Font font = new Font("Segoe UI", 9, FontStyle.Regular, GraphicsUnit.Point);
        public Font Font { get => font; set { if (font != value) { font = value; Recalculate(); } } }

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
            if (blockNotifyChangeCounter == 0)
            {
                BackColor = ThemeHelper.GetDwmAccentColor(isActive);
                InDarkMode = BackColor.GetBrightness() < 0.5f;
                ForeColor = !isActive ? SystemColors.GrayText : InDarkMode ? Color.White : Color.Black;

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
        }

        private void ThemeHelper_UserPreferencesChanged(_void sender, EventArgs e) => Recalculate();

        public void Update(Action updateAction)
        {
            blockNotifyChangeCounter++;

            try
            {
                updateAction();
            }
            finally
            {
                blockNotifyChangeCounter--;
                Recalculate();
            }
        }

        public void Dispose()
        {
            ThemeHelper.UserPreferencesChanged -= ThemeHelper_UserPreferencesChanged;
            IsDisposed = true;
        }
    }
}

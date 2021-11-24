#region License
/*********************************************************************************
 * ThemeHelper.cs
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

using Eutherion.Win.Native;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Eutherion.Win.Utils
{
    public static class ThemeHelper
    {
        private static SafeLazy<(Color, Color)> accentColors;

        static ThemeHelper()
        {
            try
            {
                SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
            }
            catch
            {
                // Just ignore changes if this event cannot be registered.
            }

            ResetAccentColors();
        }

        /// <summary>
        /// Gets if DWN composition is currently enabled.
        /// </summary>
        public static bool DwmEnabled
        {
            get
            {
                try
                {
                    Version osVersion = Environment.OSVersion.Version;

                    // DWM cannot be turned off anymore in Windows 10.
                    if (osVersion.Major >= 10) return true;

                    // DWM is off in terminal sessions before Windows 10.
                    if (SystemInformation.TerminalServerSession) return false;

                    NativeMethods.DwmIsCompositionEnabled(out bool enabled);
                    return enabled;
                }
                catch
                {
                    return false;
                }
            }
        }

        private static void ResetAccentColors()
        {
            accentColors = new SafeLazy<(Color, Color)>(GetDwmAccentColors);
        }

        /// <summary>
        /// Gets the current DWM accent color, as determined by OS user preferences.
        /// </summary>
        public static Color GetDwmAccentColor(bool isActive)
            => isActive ? accentColors.Value.Item1 : accentColors.Value.Item2;

        private static (Color, Color) GetDwmAccentColors()
        {
            Color accentActiveColor = Color.White;
            Color accentInactiveColor = Color.White;

            if (DwmEnabled
                && RegistryHelper.GetRegistryValue(Registry.CurrentUser, "Software\\Microsoft\\Windows\\DWM", "ColorPrevalence", 0) != 0)
            {
                Maybe<int> maybeAccentActiveColor = RegistryHelper.GetRegistryValue<int>(Registry.CurrentUser, "Software\\Microsoft\\Windows\\DWM", "AccentColor");
                if (maybeAccentActiveColor.IsJust(out int accentActiveColorIntValue))
                {
                    // Use alpha == 255. Format is ABGR.
                    accentActiveColor = Color.FromArgb(
                        accentActiveColorIntValue & 0xff,
                        (accentActiveColorIntValue & 0xff00) >> 8,
                        (accentActiveColorIntValue & 0xff0000) >> 16);
                }

                Maybe<int> maybeAccentInctiveColor = RegistryHelper.GetRegistryValue<int>(Registry.CurrentUser, "Software\\Microsoft\\Windows\\DWM", "AccentColorInactive");
                if (maybeAccentInctiveColor.IsJust(out int accentInactiveColorIntValue))
                {
                    // Use alpha == 255. Format is ABGR.
                    accentInactiveColor = Color.FromArgb(
                        accentInactiveColorIntValue & 0xff,
                        (accentInactiveColorIntValue & 0xff00) >> 8,
                        (accentInactiveColorIntValue & 0xff0000) >> 16);
                }
            }

            // Default is Color.White even when dark theme is on.
            return (accentActiveColor, accentInactiveColor);
        }

        // Using a weak event for static event declarations, so memory leaks are less likely.
        private static readonly WeakEvent<_void, EventArgs> event_UserPreferencesChanged = new WeakEvent<_void, EventArgs>();

        /// <summary>
        /// <see cref="WeakEvent{TSender, TEventArgs}"/> which occurs when the system user preferences changed.
        /// </summary>
        public static event Action<_void, EventArgs> UserPreferencesChanged
        {
            add { event_UserPreferencesChanged.AddListener(value); }
            remove { event_UserPreferencesChanged.RemoveListener(value); }
        }

        private static void SystemEvents_UserPreferenceChanged(object sender, EventArgs e)
        {
            ResetAccentColors();
            event_UserPreferencesChanged.Raise(default, EventArgs.Empty);
        }
    }
}

#region License
/*********************************************************************************
 * KeyUtilities.cs
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

using Eutherion.Win.UIActions;
using System.Windows.Forms;

namespace Eutherion.Win.Utils
{
    public static class KeyUtilities
    {
        /// <summary>
        /// Determines if a given shortcut matches a <see cref="ShortcutKeys"/> definition.
        /// This takes into account alternative shortcut keys which are generally considered equivalent.
        /// An example is Ctrl+., where there are usually two keys that map to the '.' character.
        /// </summary>
        public static bool IsMatch(ShortcutKeys shortcutKeys, Keys shortcut)
        {
            if (shortcutKeys.IsEmpty) return false;

            Keys equivalentShortcut = ToKeys(shortcutKeys);

            if (shortcut == equivalentShortcut) return true;

            Keys keyCode = equivalentShortcut & Keys.KeyCode;
            Keys modifiers = equivalentShortcut & Keys.Modifiers;
            bool shift = equivalentShortcut.HasFlag(Keys.Shift);

            if (keyCode >= Keys.D0 && keyCode <= Keys.D9)
            {
                if (shortcut == equivalentShortcut - Keys.D0 + Keys.NumPad0) return true;
            }
            else if (keyCode >= Keys.NumPad0 && keyCode <= Keys.NumPad9)
            {
                if (shortcut == equivalentShortcut - Keys.NumPad0 + Keys.D0) return true;
            }

            else if (keyCode == Keys.Add)
            {
                if (!shift && shortcut == (modifiers | Keys.Shift | Keys.Oemplus)) return true;
            }
            else if (keyCode == Keys.Oemplus)
            {
                if (shift && shortcut == (modifiers | Keys.Add)) return true;
            }

            else if (keyCode == Keys.Subtract)
            {
                if (shortcut == (modifiers | Keys.OemMinus)) return true;
            }
            else if (keyCode == Keys.OemMinus)
            {
                if (shortcut == (modifiers | Keys.Subtract)) return true;
            }

            else if (keyCode == Keys.Multiply)
            {
                if (!shift && shortcut == (modifiers | Keys.Shift | Keys.D8)) return true;
            }
            else if (keyCode == Keys.D8)
            {
                if (shift && shortcut == (modifiers | Keys.Multiply)) return true;
            }

            else if (keyCode == Keys.Divide)
            {
                if (shortcut == (modifiers | Keys.OemQuestion)) return true;
            }
            else if (keyCode == Keys.OemQuestion)
            {
                if (shortcut == (modifiers | Keys.Divide)) return true;
            }

            return false;
        }

        public static Keys ToKeys(ShortcutKeys shortcutKeys)
        {
            Keys equivalentShortcut = (Keys)shortcutKeys.Key;

            KeyModifiers code = shortcutKeys.Modifiers;
            if (code.HasFlag(KeyModifiers.Shift)) equivalentShortcut |= Keys.Shift;
            if (code.HasFlag(KeyModifiers.Control)) equivalentShortcut |= Keys.Control;
            if (code.HasFlag(KeyModifiers.Alt)) equivalentShortcut |= Keys.Alt;

            return equivalentShortcut;
        }

        /// <summary>
        /// Searches for an <see cref="UIActionHandler"/> to convert a user command key to a <see cref="UIAction"/>
        /// and then perform the action.
        /// </summary>
        /// <param name="shortcut">
        /// The shortcut key pressed by the user.
        /// </param>
        /// <param name="bottomLevelControl">
        /// The <see cref="Control"/> from which to initiate searching for a handler for the shortcut key.
        /// Generally this is the control which has the keyboard focus.
        /// </param>
        /// <returns>
        /// Whether or not a <see cref="UIActionHandler"/> was found which processed the key successfully.
        /// </returns>
        public static bool TryExecute(Keys shortcut, Control bottomLevelControl)
        {
            foreach (UIActionHandler actionHandler in UIActionHandler.EnumerateUIActionHandlers(bottomLevelControl))
            {
                // Try to find an action with given shortcut.
                foreach (var mapping in actionHandler.KeyMappings)
                {
                    // If the shortcut matches, then try to perform the action.
                    // If the handler does not return UIActionVisibility.Parent, then swallow the key by returning true.
                    if (IsMatch(mapping.Shortcut, shortcut)
                        && actionHandler.TryPerformAction(mapping.Action, true).UIActionVisibility != UIActionVisibility.Parent)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}

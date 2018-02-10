/*********************************************************************************
 * KeyUIActionMapping.cs
 * 
 * Copyright (c) 2004-2018 Henk Nicolai
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
 *********************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    [Flags]
    public enum KeyModifiers
    {
        None = 0,
        Shift = 1,
        Control = 2,
        Alt = 4,
    }

    /// <summary>
    /// Represents a shortcut key combination for a <see cref="UIAction"/>.
    /// </summary>
    public struct ShortcutKeys
    {
        public KeyModifiers Modifiers;
        public ConsoleKey Key;

        public ShortcutKeys(KeyModifiers modifierCode, ConsoleKey key)
        {
            Modifiers = modifierCode;
            Key = key;
        }

        public ShortcutKeys(ConsoleKey key) : this(KeyModifiers.None, key)
        {
        }

        public bool IsEmpty => Key == 0;

        public IEnumerable<LocalizedStringKey> DisplayStringParts()
        {
            if (IsEmpty) yield break;

            if (Modifiers.HasFlag(KeyModifiers.Control)) yield return LocalizedConsoleKeys.ConsoleKeyCtrl;
            if (Modifiers.HasFlag(KeyModifiers.Shift)) yield return LocalizedConsoleKeys.ConsoleKeyShift;
            if (Modifiers.HasFlag(KeyModifiers.Alt)) yield return LocalizedConsoleKeys.ConsoleKeyAlt;

            if (Key >= ConsoleKey.D0 && Key <= ConsoleKey.D9) yield return LocalizedStringKey.Unlocalizable(Convert.ToString((int)Key - (int)ConsoleKey.D0));
            else if (Key == ConsoleKey.Add) yield return LocalizedStringKey.Unlocalizable("+");
            else if (Key == ConsoleKey.Subtract) yield return LocalizedStringKey.Unlocalizable("-");
            else if (Key == ConsoleKey.Multiply) yield return LocalizedStringKey.Unlocalizable("*");
            else if (Key == ConsoleKey.Divide) yield return LocalizedStringKey.Unlocalizable("/");
            else if (Key == ConsoleKey.Delete) yield return LocalizedConsoleKeys.ConsoleKeyDelete;
            else if (Key == ConsoleKey.LeftArrow) yield return LocalizedConsoleKeys.ConsoleKeyLeftArrow;
            else if (Key == ConsoleKey.RightArrow) yield return LocalizedConsoleKeys.ConsoleKeyRightArrow;
            else if (Key == ConsoleKey.UpArrow) yield return LocalizedConsoleKeys.ConsoleKeyUpArrow;
            else if (Key == ConsoleKey.DownArrow) yield return LocalizedConsoleKeys.ConsoleKeyDownArrow;
            else yield return LocalizedStringKey.Unlocalizable(Key.ToString());
        }
    }

    public static class LocalizedConsoleKeys
    {
        public static readonly LocalizedStringKey ConsoleKeyAlt = new LocalizedStringKey(nameof(ConsoleKeyAlt));
        public static readonly LocalizedStringKey ConsoleKeyCtrl = new LocalizedStringKey(nameof(ConsoleKeyCtrl));
        public static readonly LocalizedStringKey ConsoleKeyDelete = new LocalizedStringKey(nameof(ConsoleKeyDelete));
        public static readonly LocalizedStringKey ConsoleKeyDownArrow = new LocalizedStringKey(nameof(ConsoleKeyDownArrow));
        public static readonly LocalizedStringKey ConsoleKeyLeftArrow = new LocalizedStringKey(nameof(ConsoleKeyLeftArrow));
        public static readonly LocalizedStringKey ConsoleKeyRightArrow = new LocalizedStringKey(nameof(ConsoleKeyRightArrow));
        public static readonly LocalizedStringKey ConsoleKeyShift = new LocalizedStringKey(nameof(ConsoleKeyShift));
        public static readonly LocalizedStringKey ConsoleKeyUpArrow = new LocalizedStringKey(nameof(ConsoleKeyUpArrow));
    }

    /// <summary>
    /// Represents a mapping between a shortcut key and a <see cref="UIAction"/>.
    /// </summary>
    public struct KeyUIActionMapping
    {
        public KeyUIActionMapping(ShortcutKeys shortcut, UIAction action)
        {
            Shortcut = shortcut;
            Action = action;
        }

        /// <summary>
        /// Gets the shortcut key for this mapping.
        /// </summary>
        public ShortcutKeys Shortcut { get; }

        /// <summary>
        /// Gets the <see cref="UIAction"/> for this mapping.
        /// </summary>
        public UIAction Action { get; }
    }

    public static class KeyUtils
    {
        /// <summary>
        /// Determines if a given shortcut matches a <see cref="ShortcutKeys"/> definition.
        /// This takes into account alternative shortcut keys which are generally considered equivalent.
        /// An example is Ctrl+., where there are usually two keys that map to the '.' character.
        /// </summary>
        public static bool IsMatch(ShortcutKeys shortcutKeys, Keys shortcut)
        {
            if (shortcutKeys.IsEmpty) return false;

            Keys equivalentShortcut = (Keys)shortcutKeys.Key;

            KeyModifiers code = shortcutKeys.Modifiers;
            if (code.HasFlag(KeyModifiers.Shift)) equivalentShortcut |= Keys.Shift;
            if (code.HasFlag(KeyModifiers.Control)) equivalentShortcut |= Keys.Control;
            if (code.HasFlag(KeyModifiers.Alt)) equivalentShortcut |= Keys.Alt;

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

        /// <summary>
        /// Tries to convert a user command key to a <see cref="UIAction"/> and perform it on the currently focused .NET control.
        /// </summary>
        /// <param name="shortcut">
        /// The shortcut key pressed by the user.
        /// </param>
        /// <returns>
        /// Whether or not the key was processed.
        /// </returns>
        public static bool TryExecute(Keys shortcut)
        {
            foreach (UIActionHandler actionHandler in UIActionHandler.EnumerateUIActionHandlers(FocusHelper.GetFocusedControl()))
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

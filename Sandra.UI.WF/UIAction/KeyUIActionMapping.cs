/*********************************************************************************
 * KeyUIActionMapping.cs
 * 
 * Copyright (c) 2004-2017 Henk Nicolai
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

        IEnumerable<string> displayStringParts()
        {
            if (IsEmpty) yield break;

            if (Modifiers.HasFlag(KeyModifiers.Control)) yield return "Ctrl";
            if (Modifiers.HasFlag(KeyModifiers.Shift)) yield return "Shift";
            if (Modifiers.HasFlag(KeyModifiers.Alt)) yield return "Alt";

            if (Key >= ConsoleKey.D0 && Key <= ConsoleKey.D9) yield return Convert.ToString((int)Key - (int)ConsoleKey.D0);
            else if (Key == ConsoleKey.Add) yield return "+";
            else if (Key == ConsoleKey.Subtract) yield return "-";
            else if (Key == ConsoleKey.Multiply) yield return "*";
            else if (Key == ConsoleKey.Divide) yield return "/";
            else if (Key == ConsoleKey.Delete) yield return "Del";
            else if (Key == ConsoleKey.LeftArrow) yield return "Left Arrow";
            else if (Key == ConsoleKey.RightArrow) yield return "Right Arrow";
            else if (Key == ConsoleKey.UpArrow) yield return "Up Arrow";
            else if (Key == ConsoleKey.DownArrow) yield return "Down Arrow";
            else yield return Key.ToString();
        }

        public string DisplayString => string.Join("+", displayStringParts());
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
        /// Helper method to make sure ampersand characters are shown in menu items, instead of giving rise to a mnemonic.
        /// </summary>
        public static string EscapeAmpersand(string caption)
        {
            return caption.Replace("&", "&&");
        }

        /// <summary>
        /// Converts a <see cref="ShortcutKeys"/> key combination to a <see cref="Keys"/>.
        /// </summary>
        public static Keys ConvertToKeys(ShortcutKeys shortcut)
        {
            if (shortcut.IsEmpty) return Keys.None;

            Keys result = (Keys)shortcut.Key;

            KeyModifiers code = shortcut.Modifiers;
            if (code.HasFlag(KeyModifiers.Shift)) result |= Keys.Shift;
            if (code.HasFlag(KeyModifiers.Control)) result |= Keys.Control;
            if (code.HasFlag(KeyModifiers.Alt)) result |= Keys.Alt;

            return result;
        }

        /// <summary>
        /// For a given shortcut key, enumerates all alternative shortcut keys which are generally considered equivalent.
        /// An example is Ctrl+., where there are usually two keys that map to the '.' character.
        /// </summary>
        public static IEnumerable<Keys> EnumerateEquivalentKeys(Keys shortcut)
        {
            Keys keyCode = shortcut & Keys.KeyCode;

            if (keyCode == Keys.None) yield break;

            yield return shortcut;

            Keys modifiers = shortcut & Keys.Modifiers;
            bool shift = shortcut.HasFlag(Keys.Shift);
            if (keyCode >= Keys.D0 && keyCode <= Keys.D9)
            {
                yield return shortcut - Keys.D0 + Keys.NumPad0;
            }
            else if (keyCode >= Keys.NumPad0 && keyCode <= Keys.NumPad9)
            {
                yield return shortcut - Keys.NumPad0 + Keys.D0;
            }
            else if (keyCode == Keys.Add)
            {
                if (!shift) yield return modifiers | Keys.Shift | Keys.Oemplus;
            }
            else if (keyCode == Keys.Oemplus)
            {
                if (shift) yield return modifiers | Keys.Add;
            }
            else if (keyCode == Keys.Subtract)
            {
                yield return modifiers | Keys.OemMinus;
            }
            else if (keyCode == Keys.OemMinus)
            {
                yield return modifiers | Keys.Subtract;
            }
            else if (keyCode == Keys.Multiply)
            {
                if (!shift) yield return modifiers | Keys.Shift | Keys.D8;
            }
            else if (keyCode == Keys.D8)
            {
                if (shift) yield return modifiers | Keys.Multiply;
            }
            else if (keyCode == Keys.Divide)
            {
                yield return modifiers | Keys.OemQuestion;
            }
            else if (keyCode == Keys.OemQuestion)
            {
                yield return modifiers | Keys.Divide;
            }
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
            try
            {
                Control control = FocusHelper.GetFocusedControl();
                while (control != null)
                {
                    IUIActionHandlerProvider provider = control as IUIActionHandlerProvider;
                    if (provider != null && provider.ActionHandler != null)
                    {
                        // Try to find an action with given shortcut.
                        foreach (var mapping in provider.ActionHandler.KeyMappings)
                        {
                            foreach (var mappedShortcut in EnumerateEquivalentKeys(ConvertToKeys(mapping.Shortcut)))
                            {
                                // If the shortcut matches, then try to perform the action.
                                // If the handler does not return UIActionVisibility.Parent, then swallow the key by returning true.
                                if (mappedShortcut == shortcut
                                    && provider.ActionHandler.TryPerformAction(mapping.Action, true).UIActionVisibility != UIActionVisibility.Parent)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    control = control.Parent;
                }
                return false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return true;
            }
        }
    }
}

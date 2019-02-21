#region License
/*********************************************************************************
 * ShortcutKeys.cs
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

using Eutherion.Localization;
using System;
using System.Collections.Generic;

namespace Eutherion.UIActions
{
    /// <summary>
    /// Specifies a general set of key modifiers.
    /// </summary>
    [Flags]
    public enum KeyModifiers
    {
        /// <summary>
        /// No modifiers.
        /// </summary>
        None = 0,

        /// <summary>
        /// The SHIFT key modifier.
        /// </summary>
        Shift = 1,

        /// <summary>
        /// The CONTROL key modifier.
        /// </summary>
        Control = 2,

        /// <summary>
        /// The ALT key modifier.
        /// </summary>
        Alt = 4,
    }

    /// <summary>
    /// Represents a shortcut key combination for a <see cref="UIAction"/>.
    /// </summary>
    public struct ShortcutKeys
    {
        /// <summary>
        /// Gets or sets the key modifiers for this shortcut.
        /// </summary>
        public KeyModifiers Modifiers { get; set; }

        /// <summary>
        /// Gets or sets the console key for this shortcut.
        /// </summary>
        public ConsoleKey Key { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ShortcutKeys"/>.
        /// </summary>
        /// <param name="modifiers">
        /// The key modifiers for this shortcut.
        /// </param>
        /// <param name="key">
        /// The console key for this shortcut.
        /// </param>
        public ShortcutKeys(KeyModifiers modifiers, ConsoleKey key)
        {
            Modifiers = modifiers;
            Key = key;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ShortcutKeys"/> without modifier keys.
        /// </summary>
        /// <param name="key">
        /// The console key for this shortcut.
        /// </param>
        public ShortcutKeys(ConsoleKey key) : this(KeyModifiers.None, key)
        {
        }

        /// <summary>
        /// Returns if this shortcut key is empty. This ignores the key modifiers.
        /// </summary>
        public bool IsEmpty => Key == 0;

        /// <summary>
        /// Enumerates the <see cref="LocalizedStringKey"/>s which combined construct a localized display string for this shortcut.
        /// </summary>
        /// <returns>
        /// The <see cref="LocalizedStringKey"/>s enumerable which combined construct a localized display string for this shortcut.
        /// </returns>
        public IEnumerable<LocalizedStringKey> DisplayStringParts()
        {
            if (IsEmpty) yield break;

            if (Modifiers.HasFlag(KeyModifiers.Control)) yield return LocalizedConsoleKeys.ConsoleKeyCtrl;
            if (Modifiers.HasFlag(KeyModifiers.Shift)) yield return LocalizedConsoleKeys.ConsoleKeyShift;
            if (Modifiers.HasFlag(KeyModifiers.Alt)) yield return LocalizedConsoleKeys.ConsoleKeyAlt;

            if (Key >= ConsoleKey.D0 && Key <= ConsoleKey.D9) yield return LocalizedStringKey.Unlocalizable(Convert.ToString((int)Key - (int)ConsoleKey.D0));
            else
            {
                switch (Key)
                {
                    case ConsoleKey.Add:
                        yield return LocalizedStringKey.Unlocalizable("+");
                        break;
                    case ConsoleKey.Subtract:
                        yield return LocalizedStringKey.Unlocalizable("-");
                        break;
                    case ConsoleKey.Multiply:
                        yield return LocalizedStringKey.Unlocalizable("*");
                        break;
                    case ConsoleKey.Divide:
                        yield return LocalizedStringKey.Unlocalizable("/");
                        break;
                    case ConsoleKey.Delete:
                        yield return LocalizedConsoleKeys.ConsoleKeyDelete;
                        break;
                    case ConsoleKey.LeftArrow:
                        yield return LocalizedConsoleKeys.ConsoleKeyLeftArrow;
                        break;
                    case ConsoleKey.RightArrow:
                        yield return LocalizedConsoleKeys.ConsoleKeyRightArrow;
                        break;
                    case ConsoleKey.UpArrow:
                        yield return LocalizedConsoleKeys.ConsoleKeyUpArrow;
                        break;
                    case ConsoleKey.DownArrow:
                        yield return LocalizedConsoleKeys.ConsoleKeyDownArrow;
                        break;
                    case ConsoleKey.Home:
                        yield return LocalizedConsoleKeys.ConsoleKeyHome;
                        break;
                    case ConsoleKey.End:
                        yield return LocalizedConsoleKeys.ConsoleKeyEnd;
                        break;
                    case ConsoleKey.PageUp:
                        yield return LocalizedConsoleKeys.ConsoleKeyPageUp;
                        break;
                    case ConsoleKey.PageDown:
                        yield return LocalizedConsoleKeys.ConsoleKeyPageDown;
                        break;
                    default:
                        yield return LocalizedStringKey.Unlocalizable(Key.ToString());
                        break;
                }
            }
        }
    }
}

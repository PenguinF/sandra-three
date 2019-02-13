#region License
/*********************************************************************************
 * KeyUIActionMapping.cs
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

namespace Eutherion.Win.UIActions
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

    public static class LocalizedConsoleKeys
    {
        public static readonly LocalizedStringKey ConsoleKeyAlt = new LocalizedStringKey(nameof(ConsoleKeyAlt));
        public static readonly LocalizedStringKey ConsoleKeyCtrl = new LocalizedStringKey(nameof(ConsoleKeyCtrl));
        public static readonly LocalizedStringKey ConsoleKeyDelete = new LocalizedStringKey(nameof(ConsoleKeyDelete));
        public static readonly LocalizedStringKey ConsoleKeyDownArrow = new LocalizedStringKey(nameof(ConsoleKeyDownArrow));
        public static readonly LocalizedStringKey ConsoleKeyEnd = new LocalizedStringKey(nameof(ConsoleKeyEnd));
        public static readonly LocalizedStringKey ConsoleKeyHome = new LocalizedStringKey(nameof(ConsoleKeyHome));
        public static readonly LocalizedStringKey ConsoleKeyLeftArrow = new LocalizedStringKey(nameof(ConsoleKeyLeftArrow));
        public static readonly LocalizedStringKey ConsoleKeyPageDown = new LocalizedStringKey(nameof(ConsoleKeyPageDown));
        public static readonly LocalizedStringKey ConsoleKeyPageUp = new LocalizedStringKey(nameof(ConsoleKeyPageUp));
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
}

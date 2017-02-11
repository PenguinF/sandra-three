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
    /// Represents the shortcut key combination for a <see cref="UIAction"/>.
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
}

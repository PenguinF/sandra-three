#region License
/*********************************************************************************
 * ShortcutKeys.cs
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

using System;

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
    }
}

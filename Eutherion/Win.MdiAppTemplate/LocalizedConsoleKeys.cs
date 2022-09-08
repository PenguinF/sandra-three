#region License
/*********************************************************************************
 * LocalizedConsoleKeys.cs
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

using Eutherion.Text;
using System.Collections.Generic;

namespace Eutherion.Win.MdiAppTemplate
{
    /// <summary>
    /// Contains a collection of <see cref="StringKey{T}"/>s of <see cref="ForFormattedText"/> which generate localized display strings
    /// for keyboard shortcuts (<see cref="Eutherion.UIActions.ShortcutKeys"/>).
    /// </summary>
    public static class LocalizedConsoleKeys
    {
        /// <summary>
        /// Gets the <see cref="StringKey{T}"/> of <see cref="ForFormattedText"/> for <see cref="Eutherion.UIActions.KeyModifiers.Alt"/>.
        /// </summary>
        public static readonly StringKey<ForFormattedText> ConsoleKeyAlt = new StringKey<ForFormattedText>(nameof(ConsoleKeyAlt));

        /// <summary>
        /// Gets the <see cref="StringKey{T}"/> of <see cref="ForFormattedText"/> for <see cref="Eutherion.UIActions.KeyModifiers.Control"/>.
        /// </summary>
        public static readonly StringKey<ForFormattedText> ConsoleKeyCtrl = new StringKey<ForFormattedText>(nameof(ConsoleKeyCtrl));

        /// <summary>
        /// Gets the <see cref="StringKey{T}"/> of <see cref="ForFormattedText"/> for <see cref="System.ConsoleKey.Delete"/>.
        /// </summary>
        public static readonly StringKey<ForFormattedText> ConsoleKeyDelete = new StringKey<ForFormattedText>(nameof(ConsoleKeyDelete));

        /// <summary>
        /// Gets the <see cref="StringKey{T}"/> of <see cref="ForFormattedText"/> for <see cref="System.ConsoleKey.DownArrow"/>.
        /// </summary>
        public static readonly StringKey<ForFormattedText> ConsoleKeyDownArrow = new StringKey<ForFormattedText>(nameof(ConsoleKeyDownArrow));

        /// <summary>
        /// Gets the <see cref="StringKey{T}"/> of <see cref="ForFormattedText"/> for <see cref="System.ConsoleKey.End"/>.
        /// </summary>
        public static readonly StringKey<ForFormattedText> ConsoleKeyEnd = new StringKey<ForFormattedText>(nameof(ConsoleKeyEnd));

        /// <summary>
        /// Gets the <see cref="StringKey{T}"/> of <see cref="ForFormattedText"/> for <see cref="System.ConsoleKey.Home"/>.
        /// </summary>
        public static readonly StringKey<ForFormattedText> ConsoleKeyHome = new StringKey<ForFormattedText>(nameof(ConsoleKeyHome));

        /// <summary>
        /// Gets the <see cref="StringKey{T}"/> of <see cref="ForFormattedText"/> for <see cref="System.ConsoleKey.LeftArrow"/>.
        /// </summary>
        public static readonly StringKey<ForFormattedText> ConsoleKeyLeftArrow = new StringKey<ForFormattedText>(nameof(ConsoleKeyLeftArrow));

        /// <summary>
        /// Gets the <see cref="StringKey{T}"/> of <see cref="ForFormattedText"/> for <see cref="System.ConsoleKey.PageDown"/>.
        /// </summary>
        public static readonly StringKey<ForFormattedText> ConsoleKeyPageDown = new StringKey<ForFormattedText>(nameof(ConsoleKeyPageDown));

        /// <summary>
        /// Gets the <see cref="StringKey{T}"/> of <see cref="ForFormattedText"/> for <see cref="System.ConsoleKey.PageUp"/>.
        /// </summary>
        public static readonly StringKey<ForFormattedText> ConsoleKeyPageUp = new StringKey<ForFormattedText>(nameof(ConsoleKeyPageUp));

        /// <summary>
        /// Gets the <see cref="StringKey{T}"/> of <see cref="ForFormattedText"/> for <see cref="System.ConsoleKey.RightArrow"/>.
        /// </summary>
        public static readonly StringKey<ForFormattedText> ConsoleKeyRightArrow = new StringKey<ForFormattedText>(nameof(ConsoleKeyRightArrow));

        /// <summary>
        /// Gets the <see cref="StringKey{T}"/> of <see cref="ForFormattedText"/> for <see cref="Eutherion.UIActions.KeyModifiers.Shift"/>.
        /// </summary>
        public static readonly StringKey<ForFormattedText> ConsoleKeyShift = new StringKey<ForFormattedText>(nameof(ConsoleKeyShift));

        /// <summary>
        /// Gets the <see cref="StringKey{T}"/> of <see cref="ForFormattedText"/> for <see cref="System.ConsoleKey.UpArrow"/>.
        /// </summary>
        public static readonly StringKey<ForFormattedText> ConsoleKeyUpArrow = new StringKey<ForFormattedText>(nameof(ConsoleKeyUpArrow));

        /// <summary>
        /// Enumerates all <see cref="StringKey{T}"/>s of <see cref="ForFormattedText"/> in this class with a suggested default English translation.
        /// </summary>
        public static IEnumerable<KeyValuePair<StringKey<ForFormattedText>, string>> DefaultEnglishTranslations => new Dictionary<StringKey<ForFormattedText>, string>
        {
            { ConsoleKeyCtrl, "Ctrl" },
            { ConsoleKeyShift, "Shift" },
            { ConsoleKeyAlt, "Alt" },

            { ConsoleKeyLeftArrow, "Left Arrow" },
            { ConsoleKeyRightArrow, "Right Arrow" },
            { ConsoleKeyUpArrow, "Up Arrow" },
            { ConsoleKeyDownArrow, "Down Arrow" },

            { ConsoleKeyDelete, "Del" },
            { ConsoleKeyHome, "Home" },
            { ConsoleKeyEnd, "End" },
            { ConsoleKeyPageDown, "PageDown" },
            { ConsoleKeyPageUp, "PageUp" },
        };
    }
}

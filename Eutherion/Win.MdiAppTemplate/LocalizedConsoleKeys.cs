#region License
/*********************************************************************************
 * LocalizedConsoleKeys.cs
 *
 * Copyright (c) 2004-2025 Henk Nicolai
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
    /// Contains a collection of <see cref="StringKey{T}"/>s of <see cref="Localization"/> which generate localized display strings
    /// for keyboard shortcuts (<see cref="Eutherion.UIActions.ShortcutKeys"/>).
    /// </summary>
    public static class LocalizedConsoleKeys
    {
        /// <summary>
        /// Gets the <see cref="StringKey{T}"/> of <see cref="Localization"/> for <see cref="Eutherion.UIActions.KeyModifiers.Alt"/>.
        /// </summary>
        public static readonly StringKey<Localization> ConsoleKeyAlt = new StringKey<Localization>(nameof(ConsoleKeyAlt));

        /// <summary>
        /// Gets the <see cref="StringKey{T}"/> of <see cref="Localization"/> for <see cref="Eutherion.UIActions.KeyModifiers.Control"/>.
        /// </summary>
        public static readonly StringKey<Localization> ConsoleKeyCtrl = new StringKey<Localization>(nameof(ConsoleKeyCtrl));

        /// <summary>
        /// Gets the <see cref="StringKey{T}"/> of <see cref="Localization"/> for <see cref="System.ConsoleKey.Delete"/>.
        /// </summary>
        public static readonly StringKey<Localization> ConsoleKeyDelete = new StringKey<Localization>(nameof(ConsoleKeyDelete));

        /// <summary>
        /// Gets the <see cref="StringKey{T}"/> of <see cref="Localization"/> for <see cref="System.ConsoleKey.DownArrow"/>.
        /// </summary>
        public static readonly StringKey<Localization> ConsoleKeyDownArrow = new StringKey<Localization>(nameof(ConsoleKeyDownArrow));

        /// <summary>
        /// Gets the <see cref="StringKey{T}"/> of <see cref="Localization"/> for <see cref="System.ConsoleKey.End"/>.
        /// </summary>
        public static readonly StringKey<Localization> ConsoleKeyEnd = new StringKey<Localization>(nameof(ConsoleKeyEnd));

        /// <summary>
        /// Gets the <see cref="StringKey{T}"/> of <see cref="Localization"/> for <see cref="System.ConsoleKey.Home"/>.
        /// </summary>
        public static readonly StringKey<Localization> ConsoleKeyHome = new StringKey<Localization>(nameof(ConsoleKeyHome));

        /// <summary>
        /// Gets the <see cref="StringKey{T}"/> of <see cref="Localization"/> for <see cref="System.ConsoleKey.LeftArrow"/>.
        /// </summary>
        public static readonly StringKey<Localization> ConsoleKeyLeftArrow = new StringKey<Localization>(nameof(ConsoleKeyLeftArrow));

        /// <summary>
        /// Gets the <see cref="StringKey{T}"/> of <see cref="Localization"/> for <see cref="System.ConsoleKey.PageDown"/>.
        /// </summary>
        public static readonly StringKey<Localization> ConsoleKeyPageDown = new StringKey<Localization>(nameof(ConsoleKeyPageDown));

        /// <summary>
        /// Gets the <see cref="StringKey{T}"/> of <see cref="Localization"/> for <see cref="System.ConsoleKey.PageUp"/>.
        /// </summary>
        public static readonly StringKey<Localization> ConsoleKeyPageUp = new StringKey<Localization>(nameof(ConsoleKeyPageUp));

        /// <summary>
        /// Gets the <see cref="StringKey{T}"/> of <see cref="Localization"/> for <see cref="System.ConsoleKey.RightArrow"/>.
        /// </summary>
        public static readonly StringKey<Localization> ConsoleKeyRightArrow = new StringKey<Localization>(nameof(ConsoleKeyRightArrow));

        /// <summary>
        /// Gets the <see cref="StringKey{T}"/> of <see cref="Localization"/> for <see cref="Eutherion.UIActions.KeyModifiers.Shift"/>.
        /// </summary>
        public static readonly StringKey<Localization> ConsoleKeyShift = new StringKey<Localization>(nameof(ConsoleKeyShift));

        /// <summary>
        /// Gets the <see cref="StringKey{T}"/> of <see cref="Localization"/> for <see cref="System.ConsoleKey.UpArrow"/>.
        /// </summary>
        public static readonly StringKey<Localization> ConsoleKeyUpArrow = new StringKey<Localization>(nameof(ConsoleKeyUpArrow));

        /// <summary>
        /// Enumerates all <see cref="StringKey{T}"/>s of <see cref="Localization"/> in this class with a suggested default English translation.
        /// </summary>
        public static IEnumerable<KeyValuePair<StringKey<Localization>, string>> DefaultEnglishTranslations => new Dictionary<StringKey<Localization>, string>
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

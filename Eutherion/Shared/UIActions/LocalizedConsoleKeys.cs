#region License
/*********************************************************************************
 * LocalizedConsoleKeys.cs
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

namespace Eutherion.UIActions
{
    /// <summary>
    /// Contains a collection of <see cref="LocalizedStringKey"/>s which generate localized display strings
    /// for keyboard shortcuts (<see cref="ShortcutKeys"/>).
    /// </summary>
    public static class LocalizedConsoleKeys
    {
        /// <summary>
        /// Gets the <see cref="LocalizedStringKey"/> for <see cref="KeyModifiers.Alt"/>.
        /// </summary>
        public static readonly LocalizedStringKey ConsoleKeyAlt = new LocalizedStringKey(nameof(ConsoleKeyAlt));

        /// <summary>
        /// Gets the <see cref="LocalizedStringKey"/> for <see cref="KeyModifiers.Control"/>.
        /// </summary>
        public static readonly LocalizedStringKey ConsoleKeyCtrl = new LocalizedStringKey(nameof(ConsoleKeyCtrl));

        /// <summary>
        /// Gets the <see cref="LocalizedStringKey"/> for <see cref="System.ConsoleKey.Delete"/>.
        /// </summary>
        public static readonly LocalizedStringKey ConsoleKeyDelete = new LocalizedStringKey(nameof(ConsoleKeyDelete));

        /// <summary>
        /// Gets the <see cref="LocalizedStringKey"/> for <see cref="System.ConsoleKey.DownArrow"/>.
        /// </summary>
        public static readonly LocalizedStringKey ConsoleKeyDownArrow = new LocalizedStringKey(nameof(ConsoleKeyDownArrow));

        /// <summary>
        /// Gets the <see cref="LocalizedStringKey"/> for <see cref="System.ConsoleKey.End"/>.
        /// </summary>
        public static readonly LocalizedStringKey ConsoleKeyEnd = new LocalizedStringKey(nameof(ConsoleKeyEnd));

        /// <summary>
        /// Gets the <see cref="LocalizedStringKey"/> for <see cref="System.ConsoleKey.Home"/>.
        /// </summary>
        public static readonly LocalizedStringKey ConsoleKeyHome = new LocalizedStringKey(nameof(ConsoleKeyHome));

        /// <summary>
        /// Gets the <see cref="LocalizedStringKey"/> for <see cref="System.ConsoleKey.LeftArrow"/>.
        /// </summary>
        public static readonly LocalizedStringKey ConsoleKeyLeftArrow = new LocalizedStringKey(nameof(ConsoleKeyLeftArrow));

        /// <summary>
        /// Gets the <see cref="LocalizedStringKey"/> for <see cref="System.ConsoleKey.PageDown"/>.
        /// </summary>
        public static readonly LocalizedStringKey ConsoleKeyPageDown = new LocalizedStringKey(nameof(ConsoleKeyPageDown));

        /// <summary>
        /// Gets the <see cref="LocalizedStringKey"/> for <see cref="System.ConsoleKey.PageUp"/>.
        /// </summary>
        public static readonly LocalizedStringKey ConsoleKeyPageUp = new LocalizedStringKey(nameof(ConsoleKeyPageUp));

        /// <summary>
        /// Gets the <see cref="LocalizedStringKey"/> for <see cref="System.ConsoleKey.RightArrow"/>.
        /// </summary>
        public static readonly LocalizedStringKey ConsoleKeyRightArrow = new LocalizedStringKey(nameof(ConsoleKeyRightArrow));

        /// <summary>
        /// Gets the <see cref="LocalizedStringKey"/> for <see cref="KeyModifiers.Shift"/>.
        /// </summary>
        public static readonly LocalizedStringKey ConsoleKeyShift = new LocalizedStringKey(nameof(ConsoleKeyShift));

        /// <summary>
        /// Gets the <see cref="LocalizedStringKey"/> for <see cref="System.ConsoleKey.UpArrow"/>.
        /// </summary>
        public static readonly LocalizedStringKey ConsoleKeyUpArrow = new LocalizedStringKey(nameof(ConsoleKeyUpArrow));
    }
}

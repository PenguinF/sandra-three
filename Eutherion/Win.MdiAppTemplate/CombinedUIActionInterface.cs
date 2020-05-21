#region License
/*********************************************************************************
 * CombinedUIActionInterface.cs
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

using Eutherion.Localization;
using Eutherion.UIActions;
using Eutherion.Win.UIActions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Eutherion.Win.MdiAppTemplate
{
    /// <summary>
    /// Defines how a <see cref="UIAction"/> can be invoked by a keyboard shortcut and how it is shown in a context menu.
    /// </summary>
    public sealed class CombinedUIActionInterface : IShortcutKeysUIActionInterface, IContextMenuUIActionInterface
    {
        /// <summary>
        /// Array of shortcut keys which will invoke the action. The first non-empty shortcut is shown in e.g. the context menu.
        /// </summary>
        public ShortcutKeys[] Shortcuts { get; set; }

        /// <summary>
        /// Gets or sets if this action is the first in a group of actions.
        /// This will result in a separator generated above the menu item generated for this binding.
        /// </summary>
        public bool IsFirstInGroup { get; set; }

        /// <summary>
        /// Defines the text provider used to generate the display text for the menu item.
        /// </summary>
        public ITextProvider MenuTextProvider { get; set; }

        /// <summary>
        /// Defines the image to display for the generated menu item.
        /// </summary>
        public IImageProvider MenuIcon { get; set; }

        /// <summary>
        /// Indicates if a modal dialog will be displayed if the action is invoked.
        /// If true, the display text of the menu item is followed by "...".
        /// </summary>
        public bool OpensDialog { get; set; }

        /// <summary>
        /// Enumerates the <see cref="LocalizedStringKey"/>s which combined construct a localized display string
        /// for the shortcut of this <see cref="CombinedUIActionInterface"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="LocalizedStringKey"/>s enumerable which combined construct a localized display string
        /// for the shortcut of this <see cref="CombinedUIActionInterface"/>.
        /// </returns>
        public IEnumerable<ITextProvider> DisplayShortcutKeys
        {
            get
            {
                if (Shortcuts == null) yield break;
                if (!Shortcuts.Any(x => !x.IsEmpty, out ShortcutKeys shortcut)) yield break;

                if (shortcut.Modifiers.HasFlag(KeyModifiers.Control)) yield return LocalizedConsoleKeys.ConsoleKeyCtrl.ToTextProvider();
                if (shortcut.Modifiers.HasFlag(KeyModifiers.Shift)) yield return LocalizedConsoleKeys.ConsoleKeyShift.ToTextProvider();
                if (shortcut.Modifiers.HasFlag(KeyModifiers.Alt)) yield return LocalizedConsoleKeys.ConsoleKeyAlt.ToTextProvider();

                if (shortcut.Key >= ConsoleKey.D0 && shortcut.Key <= ConsoleKey.D9)
                {
                    yield return Convert.ToString((int)shortcut.Key - (int)ConsoleKey.D0).ToTextProvider();
                }
                else
                {
                    switch (shortcut.Key)
                    {
                        case ConsoleKey.Add:
                            yield return "+".ToTextProvider();
                            break;
                        case ConsoleKey.Subtract:
                            yield return "-".ToTextProvider();
                            break;
                        case ConsoleKey.Multiply:
                            yield return "*".ToTextProvider();
                            break;
                        case ConsoleKey.Divide:
                            yield return "/".ToTextProvider();
                            break;
                        case ConsoleKey.Delete:
                            yield return LocalizedConsoleKeys.ConsoleKeyDelete.ToTextProvider();
                            break;
                        case ConsoleKey.LeftArrow:
                            yield return LocalizedConsoleKeys.ConsoleKeyLeftArrow.ToTextProvider();
                            break;
                        case ConsoleKey.RightArrow:
                            yield return LocalizedConsoleKeys.ConsoleKeyRightArrow.ToTextProvider();
                            break;
                        case ConsoleKey.UpArrow:
                            yield return LocalizedConsoleKeys.ConsoleKeyUpArrow.ToTextProvider();
                            break;
                        case ConsoleKey.DownArrow:
                            yield return LocalizedConsoleKeys.ConsoleKeyDownArrow.ToTextProvider();
                            break;
                        case ConsoleKey.Home:
                            yield return LocalizedConsoleKeys.ConsoleKeyHome.ToTextProvider();
                            break;
                        case ConsoleKey.End:
                            yield return LocalizedConsoleKeys.ConsoleKeyEnd.ToTextProvider();
                            break;
                        case ConsoleKey.PageUp:
                            yield return LocalizedConsoleKeys.ConsoleKeyPageUp.ToTextProvider();
                            break;
                        case ConsoleKey.PageDown:
                            yield return LocalizedConsoleKeys.ConsoleKeyPageDown.ToTextProvider();
                            break;
                        default:
                            yield return shortcut.Key.ToString().ToTextProvider();
                            break;
                    }
                }
            }
        }
    }

    public static class CombinedUIActionInterfaceExtensions
    {
        public static ITextProvider ToTextProvider(this string displayText)
            => new ConstantTextProvider(displayText);

        public static ITextProvider ToTextProvider(this LocalizedStringKey key)
            => key == null ? null : new LocalizedTextProvider(key);

        public static IImageProvider ToImageProvider(this Image image)
            => image == null ? null : new ConstantImageProvider(image);
    }
}

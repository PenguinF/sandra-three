#region License
/*********************************************************************************
 * CombinedUIActionInterface.cs
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
using Eutherion.UIActions;
using Eutherion.Win.UIActions;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Eutherion.Win.AppTemplate
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
        /// Defines the caption to display for the generated menu item.
        /// </summary>
        public LocalizedStringKey MenuCaptionKey { get; set; }

        /// <summary>
        /// Defines the image to display for the generated menu item.
        /// </summary>
        public IImageProvider MenuIcon { get; set; }

        /// <summary>
        /// Indicates if a modal dialog will be displayed if the action is invoked.
        /// If true, the display text of the menu item is followed by "...".
        /// </summary>
        public bool OpensDialog { get; set; }

        IEnumerable<LocalizedStringKey> IContextMenuUIActionInterface.DisplayShortcutKeys
            => Shortcuts == null ? null
            : Shortcuts.FirstOrDefault(x => !x.IsEmpty).DisplayStringParts();
    }

    public static class CombinedUIActionInterfaceExtensions
    {
        public static IImageProvider ToImageProvider(this Image image)
            => image == null ? null : new ConstantImageProvider(image);
    }
}

#region License
/*********************************************************************************
 * UIActionInterfaces.cs
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
using System.Collections.Generic;
using System.Drawing;

namespace Eutherion.Win.UIActions
{
    /// <summary>
    /// Defines how a <see cref="UIAction"/> can be invoked by a keyboard shortcut.
    /// </summary>
    public interface IShortcutKeysUIActionInterface : IUIActionInterface
    {
        /// <summary>
        /// Array of shortcut keys which will invoke the action. The first non-empty shortcut is shown in e.g. the context menu.
        /// </summary>
        ShortcutKeys[] Shortcuts { get; }
    }

    /// <summary>
    /// Defines how a <see cref="UIAction"/> is shown in a context menu.
    /// </summary>
    public interface IContextMenuUIActionInterface : IUIActionInterface
    {
        /// <summary>
        /// Gets if this action is the first in a group of actions.
        /// This will result in a separator generated above the menu item generated for this binding.
        /// </summary>
        bool IsFirstInGroup { get; }

        /// <summary>
        /// Defines the caption to display for the generated menu item.
        /// </summary>
        LocalizedStringKey MenuCaptionKey { get; }

        /// <summary>
        /// Defines the image to display for the generated menu item.
        /// </summary>
        Image MenuIcon { get; }

        /// <summary>
        /// Defines the shortcut key to display in the menu item.
        /// If the enumeration is null or empty, no shortcut key will be shown.
        /// </summary>
        IEnumerable<LocalizedStringKey> DisplayShortcutKeys { get; }

        /// <summary>
        /// Indicates if a modal dialog will be displayed if the action is invoked.
        /// If true, the display text of the menu item is followed by "...".
        /// </summary>
        bool OpensDialog { get; }
    }
}

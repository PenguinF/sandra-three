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
using System.Drawing;

namespace Eutherion.Win.UIActions
{
    /// <summary>
    /// Defines how a <see cref="UIAction"/> can be invoked by a keyboard shortcut.
    /// </summary>
    public sealed class ShortcutKeysUIActionInterface : IUIActionInterface
    {
        /// <summary>
        /// Array of shortcut keys which will invoke the action. The first non-empty shortcut is shown in e.g. the context menu.
        /// </summary>
        public ShortcutKeys[] Shortcuts;
    }

    /// <summary>
    /// Defines how a <see cref="UIAction"/> is shown in a context menu.
    /// </summary>
    public sealed class ContextMenuUIActionInterface : IUIActionInterface
    {
        /// <summary>
        /// Defines the container in which a menu item must be generated for this binding.
        /// If this is null, the root node is used.
        /// </summary>
        public UIMenuNode.Container MenuContainer;

        /// <summary>
        /// Gets or sets if this action is the first in a group of actions.
        /// This will result in a separator generated above the menu item generated for this binding.
        /// </summary>
        public bool IsFirstInGroup;

        /// <summary>
        /// Defines the caption to display for the generated menu item.
        /// </summary>
        public LocalizedStringKey MenuCaptionKey;

        /// <summary>
        /// Defines the image to display for the generated menu item.
        /// </summary>
        public Image MenuIcon;
    }
}

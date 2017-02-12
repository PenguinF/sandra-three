/*********************************************************************************
 * UIActionBinding.cs
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
using System.Collections.Generic;
using System.Drawing;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Groups a set of optional properties to allow for one shared definition of a <see cref="UIAction"/> 
    /// across several components and ways of exposing those actions (menu, shortcut key, buttons, etc).
    /// </summary>
    public struct UIActionBinding
    {
        /// <summary>
        /// Shortcut which is displayed in menu items, as well as used for creating a <see cref="KeyUIActionMapping"/>.
        /// </summary>
        public ShortcutKeys MainShortcut;

        /// <summary>
        /// List of alternative shortcut keys which will invoke the action as well, but are not shown in e.g. the context menu.
        /// </summary>
        public List<ShortcutKeys> AlternativeShortcuts;

        /// <summary>
        /// Whether or not the action is shown in the context menu.
        /// </summary>
        public bool ShowInMenu;

        /// <summary>
        /// If <see cref="ShowInMenu"/> is true, defines the container in which a menu item must be generated for this binding.
        /// If this is null, the root node is used.
        /// </summary>
        public UIMenuNode.Container MenuContainer;

        /// <summary>
        /// If <see cref="ShowInMenu"/> is true, defines the caption to display for the generated menu item.
        /// </summary>
        public string MenuCaption;

        /// <summary>
        /// If <see cref="ShowInMenu"/> is true, defines the image to display for the generated menu item.
        /// </summary>
        public Image MenuIcon;
    }
}

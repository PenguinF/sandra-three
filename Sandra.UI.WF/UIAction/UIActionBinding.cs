/*********************************************************************************
 * UIActionBinding.cs
 * 
 * Copyright (c) 2004-2018 Henk Nicolai
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
using System.Collections;
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
        /// Array of shortcut keys which will invoke the action. The first non-empty shortcut is shown in e.g. the context menu.
        /// </summary>
        public ShortcutKeys[] Shortcuts;

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
        /// Gets or sets if this action is the first in a group of actions.
        /// This will result in a separator generated above the menu item generated for this binding.
        /// </summary>
        public bool IsFirstInGroup;

        /// <summary>
        /// If <see cref="ShowInMenu"/> is true, defines the caption to display for the generated menu item.
        /// </summary>
        public LocalizedStringKey MenuCaption;

        /// <summary>
        /// If <see cref="ShowInMenu"/> is true, defines the image to display for the generated menu item.
        /// </summary>
        public Image MenuIcon;
    }

    /// <summary>
    /// Helper class which encapsulates a <see cref="IUIActionHandlerProvider"/>'s default suggested binding for a <see cref="UIAction"/>.
    /// </summary>
    public sealed class DefaultUIActionBinding
    {
        /// <summary>
        /// Gets the <see cref="UIAction"/> to bind.
        /// </summary>
        public UIAction Action { get; }

        /// <summary>
        /// Gets the <see cref="UIActionBinding"/> which contains the default parameters that define how the action is exposed to the user interface.
        /// </summary>
        public UIActionBinding DefaultBinding { get; }

        public DefaultUIActionBinding(UIAction action, UIActionBinding defaultBinding)
        {
            Action = action;
            DefaultBinding = defaultBinding;
        }
    }

    /// <summary>
    /// Defines a <see cref="UIActionHandlerFunc"/> for a given <see cref="DefaultUIActionBinding"/>.
    /// </summary>
    public sealed class BindingHandlerPair
    {
        public readonly DefaultUIActionBinding Binding;
        public readonly UIActionHandlerFunc Handler;

        public BindingHandlerPair(DefaultUIActionBinding binding, UIActionHandlerFunc handler)
        {
            Binding = binding;
            Handler = handler;
        }
    }

    /// <summary>
    /// Enumerates a collection of handlers for a set of <see cref="UIAction"/> bindings.
    /// Instances of this class can be declared with a collection initializer.
    /// </summary>
    public sealed class UIActionBindings : IEnumerable<BindingHandlerPair>
    {
        readonly List<BindingHandlerPair> added = new List<BindingHandlerPair>();

        public void Add(DefaultUIActionBinding key, UIActionHandlerFunc value) => added.Add(new BindingHandlerPair(key, value));

        IEnumerator IEnumerable.GetEnumerator() => added.GetEnumerator();
        IEnumerator<BindingHandlerPair> IEnumerable<BindingHandlerPair>.GetEnumerator() => added.GetEnumerator();
    }
}

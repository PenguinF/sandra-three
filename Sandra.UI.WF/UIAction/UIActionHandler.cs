/*********************************************************************************
 * UIActionHandler.cs
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Delegate which verifies if an action can be performed, and optionally performs it.
    /// </summary>
    /// <param name="perform">
    /// Whether or not to actually perform the action.
    /// </param>
    /// <returns>
    /// A complete <see cref="UIActionState"/> if <paramref name="perform"/> is false,
    /// or a <see cref="UIActionState"/> indicating whether or not the action was performed successfully, if <paramref name="perform"/> is true.
    /// </returns>
    public delegate UIActionState UIActionHandlerFunc(bool perform);

    /// <summary>
    /// Responsible for managing a set of <see cref="UIAction"/>s and their associated handlers.
    /// </summary>
    public class UIActionHandler
    {
        private readonly Dictionary<UIAction, UIActionHandlerFunc> handlers = new Dictionary<UIAction, UIActionHandlerFunc>();
        private readonly List<KeyUIActionMapping> keyMappings = new List<KeyUIActionMapping>();
        private readonly UIMenuNode.Container rootMenuNode = new UIMenuNode.Container(null);

        /// <summary>
        /// Enumerates all non-empty <see cref="ShortcutKeys"/> which are bound to this handler.
        /// </summary>
        public IEnumerable<KeyUIActionMapping> KeyMappings => keyMappings;

        /// <summary>
        /// Gets the top level node of a <see cref="UIMenuNode"/> tree.
        /// </summary>
        public UIMenuNode.Container RootMenuNode => rootMenuNode;

        /// <summary>
        /// Binds a handler function for a <see cref="UIAction"/> to this <see cref="UIActionHandler"/>,
        /// and specifies how this <see cref="UIAction"/> is exposed to the user interface.
        /// </summary>
        /// <param name="action">
        /// The <see cref="UIAction"/> to bind.
        /// </param>
        /// <param name="handler">
        /// The handler function used to perform the <see cref="UIAction"/> and determine its <see cref="UIActionState"/>.
        /// </param>
        /// <param name="binding">
        /// Structure containing parameters that define how the action is exposed to the user interface.
        /// </param>
        public void BindAction(UIAction action, UIActionHandlerFunc handler, UIActionBinding binding)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            handlers.Add(action, handler);

            if (!binding.MainShortcut.IsEmpty)
            {
                keyMappings.Add(new KeyUIActionMapping(binding.MainShortcut, action));
            }

            if (binding.AlternativeShortcuts != null)
            {
                foreach (var alternativeShortcut in binding.AlternativeShortcuts.Where(x => !x.IsEmpty))
                {
                    keyMappings.Add(new KeyUIActionMapping(alternativeShortcut, action));
                }
            }

            if (binding.ShowInMenu)
            {
                (binding.MenuContainer ?? rootMenuNode).Nodes.Add(new UIMenuNode.Element(action, binding));
            }
        }

        /// <summary>
        /// Verifies if an action can be performed, and optionally performs it.
        /// </summary>
        /// <param name="action">
        /// The action to perform.
        /// </param>
        /// <param name="perform">
        /// Whether or not to perform the action.
        /// </param>
        /// <returns>
        /// A complete <see cref="UIActionState"/> if <paramref name="perform"/> is false,
        /// or a <see cref="UIActionState"/> indicating whether or not the action was performed successfully, if <paramref name="perform"/> is true.
        /// </returns>
        public UIActionState TryPerformAction(UIAction action, bool perform)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            UIActionHandlerFunc handler;
            if (handlers.TryGetValue(action, out handler))
            {
                // Call the handler.
                return handler(perform);
            }

            // Default is to look at parent controls for unsupported actions.
            return default(UIActionState);
        }
    }

    /// <summary>
    /// Interface implemented by <see cref="System.Windows.Forms.Control"/> subclasses to hook into the <see cref="UIAction"/> framework.
    /// </summary>
    public interface IUIActionHandlerProvider
    {
        UIActionHandler ActionHandler { get; }
    }

    public static class UIActionProviderExtensions
    {
        /// <summary>
        /// Binds a handler function for a <see cref="UIAction"/> to a <see cref="IUIActionHandlerProvider"/>,
        /// and specifies how this <see cref="UIAction"/> is exposed to the user interface.
        /// </summary>
        /// <param name="provider">
        /// The <see cref="System.Windows.Forms.Control"/> which allows binding of actions by implementing the <see cref="IUIActionHandlerProvider"/> interface.
        /// </param>
        /// <param name="action">
        /// The <see cref="UIAction"/> to bind.
        /// </param>
        /// <param name="handler">
        /// The handler function used to perform the <see cref="UIAction"/> and determine its <see cref="UIActionState"/>.
        /// </param>
        /// <param name="binding">
        /// Structure containing parameters that define how the action is exposed to the user interface.
        /// </param>
        public static void BindAction(this IUIActionHandlerProvider provider, UIAction action, UIActionHandlerFunc handler, UIActionBinding binding)
        {
            if (provider != null && provider.ActionHandler != null)
            {
                provider.ActionHandler.BindAction(action, handler, binding);
            }
        }
    }
}

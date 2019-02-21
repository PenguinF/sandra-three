#region License
/*********************************************************************************
 * UIActionHandler.cs
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

using Eutherion.UIActions;
using Eutherion.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Eutherion.Win.UIActions
{
    /// <summary>
    /// Responsible for managing a set of <see cref="UIAction"/>s and their associated handlers.
    /// </summary>
    public class UIActionHandler
    {
        private readonly Dictionary<UIAction, UIActionHandlerFunc> handlers = new Dictionary<UIAction, UIActionHandlerFunc>();
        private readonly List<KeyUIActionMapping> keyMappings = new List<KeyUIActionMapping>();

        /// <summary>
        /// Enumerates all non-empty <see cref="ShortcutKeys"/> which are bound to this handler.
        /// </summary>
        public IEnumerable<KeyUIActionMapping> KeyMappings => keyMappings.Enumerate();

        /// <summary>
        /// Gets the top level node of a <see cref="UIMenuNode"/> tree.
        /// </summary>
        public UIMenuNode.Container RootMenuNode { get; } = new UIMenuNode.Container(null);

        /// <summary>
        /// Binds a handler function for a <see cref="UIAction"/> to this <see cref="UIActionHandler"/>,
        /// and specifies how this <see cref="UIAction"/> is exposed to the user interface.
        /// </summary>
        /// <param name="action">
        /// The <see cref="UIAction"/> to bind.
        /// </param>
        /// <param name="binding">
        /// <see cref="UIActionBinding"/> structure containing parameters that define how the <see cref="UIAction"/> is exposed to the user interface.
        /// </param>
        /// <param name="handler">
        /// The handler function used to perform the <see cref="UIAction"/> and determine its <see cref="UIActionState"/>.
        /// </param>
        public void BindAction(UIAction action, ImplementationSet<IUIActionInterface> binding, UIActionHandlerFunc handler)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            handlers.Add(action, handler);

            if (binding.TryGet(out ShortcutKeysUIActionInterface shortcutKeysInterface) && shortcutKeysInterface.Shortcuts != null)
            {
                foreach (var shortcut in shortcutKeysInterface.Shortcuts.Where(x => !x.IsEmpty))
                {
                    keyMappings.Add(new KeyUIActionMapping(shortcut, action));
                }
            }

            if (binding.TryGet(out ContextMenuUIActionInterface contextMenuInterface))
            {
                RootMenuNode.Nodes.Add(new UIMenuNode.Element(action, shortcutKeysInterface, contextMenuInterface));
            }
        }

        /// <summary>
        /// Occurs when all actions have been invalidated.
        /// </summary>
        public event Action<UIActionHandler> UIActionsInvalidated;

        /// <summary>
        /// Raises the <see cref="UIActionsInvalidated"/> event. 
        /// </summary>
        protected virtual void OnUIActionsInvalidated()
        {
            UIActionsInvalidated?.Invoke(this);
        }

        /// <summary>
        /// Verifies if an action can be performed, and optionally performs it.
        /// </summary>
        /// <param name="action">
        /// The <see cref="UIAction"/> to perform.
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

            if (handlers.TryGetValue(action, out UIActionHandlerFunc handler))
            {
                // Call the handler.
                UIActionState result = handler(perform);

                // Raise event if an action has been performed successfully.
                if (perform && result.UIActionVisibility == UIActionVisibility.Enabled)
                {
                    Invalidate();
                }

                return result;
            }

            // Default is to look at parent controls for unsupported actions.
            return default(UIActionState);
        }

        /// <summary>
        /// Invalidates this <see cref="UIActionHandler"/> manually.
        /// </summary>
        public void Invalidate()
        {
            OnUIActionsInvalidated();
        }
    }

    /// <summary>
    /// Interface implemented by <see cref="Control"/> subclasses to hook into the <see cref="UIAction"/> framework.
    /// </summary>
    public interface IUIActionHandlerProvider
    {
        /// <summary>
        /// Returns the <see cref="UIActionHandler"/> for the <see cref="Control"/>,
        /// to which handlers for <see cref="UIAction"/>s can be bound.
        /// </summary>
        UIActionHandler ActionHandler { get; }
    }

    public static class UIActionProviderExtensions
    {
        /// <summary>
        /// Binds a handler function for a <see cref="UIAction"/> to a <see cref="IUIActionHandlerProvider"/>,
        /// and specifies how this <see cref="UIAction"/> is exposed to the user interface.
        /// </summary>
        /// <param name="provider">
        /// The <see cref="Control"/> which allows binding of actions by implementing the <see cref="IUIActionHandlerProvider"/> interface.
        /// </param>
        /// <param name="action">
        /// The <see cref="UIAction"/> to bind.
        /// </param>
        /// <param name="binding">
        /// <see cref="UIActionBinding"/> structure containing parameters that define how the <see cref="UIAction"/> is exposed to the user interface.
        /// </param>
        /// <param name="handler">
        /// The handler function used to perform the <see cref="UIAction"/> and determine its <see cref="UIActionState"/>.
        /// </param>
        public static void BindAction(this IUIActionHandlerProvider provider, UIAction action, ImplementationSet<IUIActionInterface> binding, UIActionHandlerFunc handler)
        {
            if (provider != null && provider.ActionHandler != null)
            {
                provider.ActionHandler.BindAction(action, binding, handler);
            }
        }

        /// <summary>
        /// Binds a handler function for a <see cref="UIAction"/> to a <see cref="IUIActionHandlerProvider"/>,
        /// and specifies how this <see cref="UIAction"/> is exposed to the user interface.
        /// </summary>
        /// <param name="provider">
        /// The <see cref="Control"/> which allows binding of actions by implementing the <see cref="IUIActionHandlerProvider"/> interface.
        /// </param>
        /// <param name="binding">
        /// Contains the <see cref="UIAction"/> with default <see cref="UIActionBinding"/>.
        /// </param>
        /// <param name="handler">
        /// The handler function used to perform the <see cref="UIAction"/> and determine its <see cref="UIActionState"/>.
        /// </param>
        public static void BindAction(this IUIActionHandlerProvider provider, DefaultUIActionBinding binding, UIActionHandlerFunc handler)
        {
            if (provider != null && provider.ActionHandler != null)
            {
                provider.ActionHandler.BindAction(binding.Action, binding.DefaultInterfaces, handler);
            }
        }

        /// <summary>
        /// Binds a collection of handlers for <see cref="UIAction"/>s to a <see cref="IUIActionHandlerProvider"/>.
        /// </summary>
        /// <param name="provider">
        /// The <see cref="Control"/> which allows binding of actions by implementing the <see cref="IUIActionHandlerProvider"/> interface.
        /// </param>
        /// <param name="bindings">
        /// A collection of triples of a <see cref="UIAction"/> to bind, a <see cref="UIActionBinding"/> that defines how the action
        /// is exposed to the user interface, and a handler function used to perform the <see cref="UIAction"/> and determine its <see cref="UIActionState"/>.
        /// </param>
        public static void BindActions(this IUIActionHandlerProvider provider, UIActionBindings bindings)
        {
            if (provider != null && provider.ActionHandler != null)
            {
                foreach (var bindingHandlerPair in bindings)
                {
                    provider.ActionHandler.BindAction(bindingHandlerPair.Binding.Action,
                                                      bindingHandlerPair.Binding.DefaultInterfaces,
                                                      bindingHandlerPair.Handler);
                }
            }
        }
    }
}

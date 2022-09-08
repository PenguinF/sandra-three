#region License
/*********************************************************************************
 * UIActionHandler.cs
 *
 * Copyright (c) 2004-2021 Henk Nicolai
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

using Eutherion.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Eutherion.UIActions
{
    /// <summary>
    /// Responsible for managing a set of <see cref="UIAction"/>s and their associated handlers.
    /// </summary>
    public class UIActionHandler
    {
        private readonly Dictionary<UIAction, UIActionHandlerFunc> handlers = new Dictionary<UIAction, UIActionHandlerFunc>();
        private readonly List<(ImplementationSet<IUIActionInterface>, UIAction)> interfaceSets = new List<(ImplementationSet<IUIActionInterface>, UIAction)>();

        /// <summary>
        /// Enumerates all sets of interfaces which map to a invokable <see cref="UIAction"/> of this handler.
        /// </summary>
        public IEnumerable<(ImplementationSet<IUIActionInterface>, UIAction)> InterfaceSets => interfaceSets.Enumerate();

        /// <summary>
        /// Binds a handler function for a <see cref="UIAction"/> to this <see cref="UIActionHandler"/>,
        /// and specifies how this <see cref="UIAction"/> is exposed to the user interface.
        /// </summary>
        /// <param name="binding">
        /// The <see cref="UIActionBinding"/> containing the <see cref="UIAction"/> to bind, its interfaces, and its handler.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="binding"/> is null.
        /// </exception>
        public void BindAction(UIActionBinding binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));

            handlers.Add(binding.Action, binding.Handler);
            interfaceSets.Add((binding.Interfaces, binding.Action));

            Invalidate();
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
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action"/> is null.
        /// </exception>
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
            return default;
        }

        /// <summary>
        /// Invalidates this <see cref="UIActionHandler"/> manually.
        /// </summary>
        public void Invalidate()
        {
            OnUIActionsInvalidated();
        }
    }
}

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
using System.Windows.Forms;

namespace Eutherion.Win.UIActions
{
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
                provider.ActionHandler.BindAction(new UIActionBinding(action, binding, handler));
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
            BindAction(provider, binding.Action, binding.DefaultInterfaces, handler);
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
            foreach (var bindingHandlerPair in bindings)
            {
                BindAction(provider,
                           bindingHandlerPair.Binding.Action,
                           bindingHandlerPair.Binding.DefaultInterfaces,
                           bindingHandlerPair.Handler);
            }
        }
    }
}

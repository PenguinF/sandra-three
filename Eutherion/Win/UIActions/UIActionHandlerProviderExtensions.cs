#region License
/*********************************************************************************
 * UIActionHandlerProviderExtensions.cs
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

using Eutherion.Utils;
using Eutherion.Win.UIActions;
using System;
using System.Linq;
using System.Windows.Forms;

// Same namespace as IUIActionHandlerProvider.
namespace Eutherion.UIActions
{
    public static class UIActionHandlerProviderExtensions
    {
        /// <summary>
        /// Binds a handler function for a <see cref="UIAction"/> to this <see cref="IUIActionHandlerProvider"/>,
        /// and specifies how this <see cref="UIAction"/> is exposed to the user interface.
        /// </summary>
        /// <typeparam name="T">
        /// The type of <paramref name="provider"/>.
        /// </typeparam>
        /// <summary>
        /// <param name="provider">
        /// The <see cref="Control"/> which allows binding of actions by implementing the <see cref="IUIActionHandlerProvider"/> interface.
        /// </param>
        /// <param name="binding">
        /// The <see cref="UIActionBinding"/> containing the <see cref="UIAction"/> to bind, its interfaces, and its handler.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="binding"/> is null.
        /// </exception>
        public static void BindAction<T>(this T provider, UIActionBinding binding)
            where T : Control, IUIActionHandlerProvider
        {
            if (provider != null && provider.ActionHandler != null)
            {
                provider.ActionHandler.BindAction(binding);
            }
        }

        /// <summary>
        /// Binds a handler function for a <see cref="UIAction"/> to a <see cref="IUIActionHandlerProvider"/>,
        /// and specifies how this <see cref="UIAction"/> is exposed to the user interface.
        /// </summary>
        /// <typeparam name="T">
        /// The type of <paramref name="provider"/>.
        /// </typeparam>
        /// <param name="provider">
        /// The <see cref="Control"/> which allows binding of actions by implementing the <see cref="IUIActionHandlerProvider"/> interface.
        /// </param>
        /// <param name="action">
        /// The <see cref="UIAction"/> to bind.
        /// </param>
        /// <param name="interfaces">
        /// The <see cref="IUIActionInterface"/> set which defines how the action is exposed to the user interface.
        /// </param>
        /// <param name="handler">
        /// The handler function used to perform the <see cref="UIAction"/> and determine its <see cref="UIActionState"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action"/> and/or <paramref name="interfaces"/> and/or <paramref name="handler"/> are null.
        /// </exception>
        public static void BindAction<T>(this T provider, UIAction action, ImplementationSet<IUIActionInterface> interfaces, UIActionHandlerFunc handler)
            where T : Control, IUIActionHandlerProvider
            => BindAction(provider, new UIActionBinding(action, interfaces, handler));

        /// <summary>
        /// Binds a handler function for a <see cref="UIAction"/> to a <see cref="IUIActionHandlerProvider"/>,
        /// and specifies how this <see cref="UIAction"/> is exposed to the user interface.
        /// </summary>
        /// <typeparam name="T">
        /// The type of <paramref name="provider"/>.
        /// </typeparam>
        /// <param name="provider">
        /// The <see cref="Control"/> which allows binding of actions by implementing the <see cref="IUIActionHandlerProvider"/> interface.
        /// </param>
        /// <param name="binding">
        /// Contains the <see cref="UIAction"/> with its suggested default <see cref="IUIActionInterface"/> set.
        /// </param>
        /// <param name="handler">
        /// The handler function used to perform the <see cref="UIAction"/> and determine its <see cref="UIActionState"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="binding"/> and/or <paramref name="handler"/> are null.
        /// </exception>
        public static void BindAction<T>(this T provider, DefaultUIActionBinding binding, UIActionHandlerFunc handler)
            where T : Control, IUIActionHandlerProvider
            => BindAction(provider, new UIActionBinding(binding, handler));

        /// <summary>
        /// Binds a collection of handlers for <see cref="UIAction"/>s to a <see cref="IUIActionHandlerProvider"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of <paramref name="provider"/>.
        /// </typeparam>
        /// <param name="provider">
        /// The <see cref="Control"/> which allows binding of actions by implementing the <see cref="IUIActionHandlerProvider"/> interface.
        /// </param>
        /// <param name="bindings">
        /// A collection of triples of a <see cref="UIAction"/> to bind, a <see cref="IUIActionInterface"/> set that defines how the action
        /// is exposed to the user interface, and a handler function used to perform the <see cref="UIAction"/> and determine its <see cref="UIActionState"/>.
        /// </param>
        public static void BindActions<T>(this T provider, UIActionBindings bindings)
            where T : Control, IUIActionHandlerProvider
            => bindings.ForEach(binding => BindAction(provider, binding));
    }
}

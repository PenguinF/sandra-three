#region License
/*********************************************************************************
 * UIActionBinding.cs
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
using System.Collections;
using System.Collections.Generic;

namespace Eutherion.Win.UIActions
{
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
        /// Gets the suggested default <see cref="ImplementationSet{TInterface}"/> which defines how the action is exposed to the user interface.
        /// </summary>
        public ImplementationSet<IUIActionInterface> DefaultInterfaces { get; }

        public DefaultUIActionBinding(UIAction action, ImplementationSet<IUIActionInterface> defaultInterfaces)
        {
            Action = action;
            DefaultInterfaces = defaultInterfaces;
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

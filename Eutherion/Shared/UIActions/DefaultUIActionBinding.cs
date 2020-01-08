#region License
/*********************************************************************************
 * DefaultUIActionBinding.cs
 *
 * Copyright (c) 2004-2020 Henk Nicolai
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
using System;

namespace Eutherion.UIActions
{
    /// <summary>
    /// Helper class which encapsulates a <see cref="IUIActionHandlerProvider"/>'s default suggested interfaces for a <see cref="UIAction"/>.
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

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultUIActionBinding"/>.
        /// </summary>
        /// <param name="action">
        /// The <see cref="UIAction"/> to bind to a <see cref="UIActionHandler"/>.
        /// </param>
        /// <param name="defaultInterfaces">
        /// The suggested default <see cref="ImplementationSet{TInterface}"/> which defines how the action is exposed to the user interface.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action"/> and/or <paramref name="defaultInterfaces"/> are null.
        /// </exception>
        public DefaultUIActionBinding(UIAction action, ImplementationSet<IUIActionInterface> defaultInterfaces)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));
            DefaultInterfaces = defaultInterfaces ?? throw new ArgumentNullException(nameof(defaultInterfaces));
        }
    }
}

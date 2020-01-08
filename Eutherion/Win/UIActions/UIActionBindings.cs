#region License
/*********************************************************************************
 * UIActionBindings.cs
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

using Eutherion.UIActions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Eutherion.Win.UIActions
{
    /// <summary>
    /// Enumerates a collection of handlers for a set of <see cref="UIAction"/> bindings.
    /// Instances of this class can be declared with a collection initializer.
    /// </summary>
    public sealed class UIActionBindings : IEnumerable<UIActionBinding>
    {
        private readonly List<UIActionBinding> added = new List<UIActionBinding>();

        /// <summary>
        /// Initializes a new empty instance of <see cref="UIActionBindings"/>.
        /// </summary>
        public UIActionBindings() { }

        /// <summary>
        /// Initializes a new instance of <see cref="UIActionBindings"/> containing a collection of bindings.
        /// </summary>
        /// <param name="bindings">
        /// The bindings to add.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="bindings"/> is null - or at least one of the elements of the enumeration is null.
        /// </exception>
        public UIActionBindings(IEnumerable<UIActionBinding> bindings)
            => AddRange(bindings);

        /// <summary>
        /// Adds a binding.
        /// </summary>
        /// <param name="binding">
        /// The binding to add.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="binding"/> is null.
        /// </exception>
        public void Add(UIActionBinding binding)
            => added.Add(binding ?? throw new ArgumentNullException(nameof(binding)));

        /// <summary>
        /// Adds a binding.
        /// </summary>
        /// <param name="binding">
        /// The <see cref="DefaultUIActionBinding"/> to add.
        /// </param>
        /// <param name="handler">
        /// The handler to add for the binding.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="binding"/> and/or <paramref name="handler"/> are null.
        /// </exception>
        public void Add(DefaultUIActionBinding binding, UIActionHandlerFunc handler)
            => added.Add(new UIActionBinding(binding, handler));

        /// <summary>
        /// Adds a collection of bindings.
        /// </summary>
        /// <param name="bindings">
        /// The bindings to add.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="bindings"/> is null - or at least one of the elements of the enumeration is null.
        /// </exception>
        public void AddRange(IEnumerable<UIActionBinding> bindings)
            => (bindings ?? throw new ArgumentNullException(nameof(bindings))).ForEach(Add);

        /// <summary>
        /// Gets an enumerator that iterates through the bindings of this collection.
        /// </summary>
        /// <returns>
        /// The enumerator that iterates through the bindings of this collection.
        /// </returns>
        public IEnumerator<UIActionBinding> GetEnumerator() => added.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => added.GetEnumerator();
    }
}

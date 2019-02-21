﻿#region License
/*********************************************************************************
 * UIActionBindings.cs
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
using System.Collections;
using System.Collections.Generic;

namespace Eutherion.Win.UIActions
{
    /// <summary>
    /// Enumerates a collection of handlers for a set of <see cref="UIAction"/> bindings.
    /// Instances of this class can be declared with a collection initializer.
    /// </summary>
    public sealed class UIActionBindings : IEnumerable<UIActionBinding>
    {
        private readonly List<UIActionBinding> added = new List<UIActionBinding>();

        public void Add(DefaultUIActionBinding key, UIActionHandlerFunc value) => added.Add(new UIActionBinding(key, value));

        IEnumerator IEnumerable.GetEnumerator() => added.GetEnumerator();
        IEnumerator<UIActionBinding> IEnumerable<UIActionBinding>.GetEnumerator() => added.GetEnumerator();
    }
}
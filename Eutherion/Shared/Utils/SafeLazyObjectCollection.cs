#region License
/*********************************************************************************
 * SafeLazyObjectCollection.cs
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

using System;

namespace Eutherion.Utils
{
    /// <summary>
    /// Represents a collection of objects which are initialized lazily with thread-safety.
    /// </summary>
    /// <typeparam name="TObject">
    /// The type of objects to create.
    /// </typeparam>
    public class SafeLazyObjectCollection<TObject>
    {
        public TObject[] Arr { get; }

        /// <summary>
        /// Gets the number of objects in this collection.
        /// </summary>
        public int Count => Arr.Length;

        /// <summary>
        /// Initializes a new instance of <see cref="SafeLazyObjectCollection{TObject}"/>.
        /// </summary>
        /// <param name="count">
        /// The number of objects in the collection.
        /// </param>
        public SafeLazyObjectCollection(int count)
        {
            Arr = count > 0 ? new TObject[count] : Array.Empty<TObject>();
        }
    }
}

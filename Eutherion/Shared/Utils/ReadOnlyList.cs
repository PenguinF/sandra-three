#region License
/*********************************************************************************
 * ReadOnlyList.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SysExtensions
{
    /// <summary>
    /// Represents a read-only list of elements that can be accessed by index.
    /// </summary>
    /// <typeparam name="T">
    /// The type of elements in the read-only list.
    /// </typeparam>
    public class ReadOnlyList<T> : IReadOnlyList<T>
    {
        // Prevent repeated allocations of empty arrays.
        private static readonly T[] emptyArray = new T[0];

        private readonly T[] array;

        private static T[] GetArray(IEnumerable<T> elements)
        {
            // Use ICollection.CopyTo() if possible, to ensure only one array is allocated.
            if (elements != null)
            {
                if (elements is ICollection<T> collection)
                {
                    var length = collection.Count;
                    if (length > 0)
                    {
                        T[] array = new T[length];
                        collection.CopyTo(array, 0);
                        return array;
                    }
                }
                else if (elements is IReadOnlyCollection<T> readOnlyCollection)
                {
                    var length = readOnlyCollection.Count;
                    if (length > 0)
                    {
                        T[] array = new T[length];
                        int index = 0;
                        foreach (var element in readOnlyCollection)
                        {
                            array[index] = element;

                            // Don't check if index >= length, assume that readOnlyCollection
                            // satisfies the contract that the number of enumerated elements is always equal to Count.
                            index++;
                        }
                        return array;
                    }
                }
                else if (elements.Any())
                {
                    return elements.ToArray();
                }
            }

            // Default case if null or empty.
            return emptyArray;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ReadOnlyList{T}"/>.
        /// </summary>
        /// <param name="elements">
        /// The elements of the list.
        /// </param>
        public ReadOnlyList(IEnumerable<T> elements) => array = GetArray(elements);

        /// <summary>
        /// Gets the element at the specified index in the read-only list.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the element to get.
        /// </param>
        /// <returns>
        /// The element at the specified index in the read-only list.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// <paramref name="index"/>is less than 0 or greater than or equal to <see cref="Count"/>.
        /// </exception>
        public T this[int index] => array[index];

        /// <summary>
        /// Gets the number of elements in the list.
        /// </summary>
        public int Count => array.Length;

        /// <summary>
        /// Returns an enumerator that iterates through the list.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the list.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (T element in array) yield return element;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

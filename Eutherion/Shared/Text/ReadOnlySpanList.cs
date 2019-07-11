#region License
/*********************************************************************************
 * ReadOnlySpanList.cs
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

namespace Eutherion.Text
{
    /// <summary>
    /// Represents a read-only list of spanned elements that can be accessed by index.
    /// </summary>
    /// <typeparam name="TSpan">
    /// The type of spanned elements in the read-only list.
    /// </typeparam>
    public class ReadOnlySpanList<TSpan> : IReadOnlyList<TSpan>, ISpan where TSpan : ISpan
    {
        /// <summary>
        /// Gets the empty <see cref="ReadOnlySpanList{T}"/>.
        /// </summary>
        public static readonly ReadOnlySpanList<TSpan> Empty = new ReadOnlySpanList<TSpan>(Array.Empty<TSpan>());

        /// <summary>
        /// Initializes a new instance of <see cref="ReadOnlySpanList{T}"/>.
        /// </summary>
        /// <param name="source">
        /// The elements of the list.
        /// </param>
        /// <returns>
        /// The initialized <see cref="ReadOnlySpanList{T}"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// One or more elements in <paramref name="source"/> are null.
        /// </exception>
        public static ReadOnlySpanList<TSpan> Create(IEnumerable<TSpan> source)
        {
            if (source is ReadOnlySpanList<TSpan> readOnlySpanList) return readOnlySpanList;
            var array = source.ToArrayEx();
            return array.Length == 0 ? Empty : new ReadOnlySpanList<TSpan>(array);
        }

        private readonly TSpan[] array;

        private ReadOnlySpanList(TSpan[] source)
        {
            int length = 0;
            for (int i = 0; i < source.Length; i++)
            {
                TSpan arrayElement = source[i];
                if (arrayElement == null) throw new ArgumentException(nameof(source));
                length += arrayElement.Length;
            }

            array = source;
            Length = length;
        }

        /// <summary>
        /// Gets the length of this <see cref="ReadOnlySpanList{TSpan}"/>.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets the spanned element at the specified index in the read-only list.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the spanned element to get.
        /// </param>
        /// <returns>
        /// The spanned element at the specified index in the read-only list.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// <paramref name="index"/>is less than 0 or greater than or equal to <see cref="Count"/>.
        /// </exception>
        public TSpan this[int index] => array[index];

        /// <summary>
        /// Gets the number of spanned elements in the list.
        /// </summary>
        public int Count => array.Length;

        /// <summary>
        /// Returns an enumerator that iterates through the list.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the list.
        /// </returns>
        public IEnumerator<TSpan> GetEnumerator()
        {
            foreach (TSpan element in array) yield return element;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

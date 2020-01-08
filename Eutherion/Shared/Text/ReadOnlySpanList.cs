#region License
/*********************************************************************************
 * ReadOnlySpanList.cs
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
    public abstract class ReadOnlySpanList<TSpan> : IReadOnlyList<TSpan>, ISpan where TSpan : ISpan
    {
        private class ZeroElements : ReadOnlySpanList<TSpan>
        {
            public override int Length => 0;

            public override TSpan this[int index] => throw new IndexOutOfRangeException();

            public override int Count => 0;

            public override IEnumerator<TSpan> GetEnumerator() => EmptyEnumerator<TSpan>.Instance;

            public override int GetElementOffset(int index) => throw new IndexOutOfRangeException();
        }

        private class OneElement : ReadOnlySpanList<TSpan>
        {
            private readonly TSpan element;

            public OneElement(TSpan source)
            {
                if (source == null) throw new ArgumentException(nameof(source));
                element = source;
            }

            public override int Length => element.Length;

            public override TSpan this[int index] => index == 0 ? element : throw new IndexOutOfRangeException();

            public override int Count => 1;

            public override IEnumerator<TSpan> GetEnumerator() => new SingleElementEnumerator<TSpan>(element);

            public override int GetElementOffset(int index) => index == 0 ? 0 : throw new IndexOutOfRangeException();
        }

        private class TwoOrMoreElements : ReadOnlySpanList<TSpan>
        {
            private readonly TSpan[] array;
            private readonly int[] arrayElementOffsets;

            public TwoOrMoreElements(TSpan[] source)
            {
                if (source[0] == null) throw new ArgumentException(nameof(source));
                int length = source[0].Length;
                arrayElementOffsets = new int[source.Length - 1];

                for (int i = 1; i < source.Length; i++)
                {
                    TSpan arrayElement = source[i];
                    if (arrayElement == null) throw new ArgumentException(nameof(source));
                    arrayElementOffsets[i - 1] = length;
                    length += arrayElement.Length;
                }

                array = source;
                Length = length;
            }

            public override int Length { get; }

            public override TSpan this[int index] => array[index];

            public override int Count => array.Length;

            public override IEnumerator<TSpan> GetEnumerator() => ((ICollection<TSpan>)array).GetEnumerator();

            public override int GetElementOffset(int index) => index == 0 ? 0 : arrayElementOffsets[index - 1];
        }

        /// <summary>
        /// Gets the empty <see cref="ReadOnlySpanList{TSpan}"/>.
        /// </summary>
        public static readonly ReadOnlySpanList<TSpan> Empty = new ZeroElements();

        /// <summary>
        /// Initializes a new instance of <see cref="ReadOnlySpanList{TSpan}"/>.
        /// </summary>
        /// <param name="source">
        /// The elements of the list.
        /// </param>
        /// <returns>
        /// The initialized <see cref="ReadOnlySpanList{TSpan}"/>.
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
            if (array.Length == 0) return Empty;
            if (array.Length == 1) return new OneElement(array[0]);
            return new TwoOrMoreElements(array);
        }

        private ReadOnlySpanList() { }

        /// <summary>
        /// Gets the length of this <see cref="ReadOnlySpanList{TSpan}"/>.
        /// </summary>
        public abstract int Length { get; }

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
        public abstract TSpan this[int index] { get; }

        /// <summary>
        /// Gets the number of spanned elements in the list.
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Returns an enumerator that iterates through the list.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the list.
        /// </returns>
        public abstract IEnumerator<TSpan> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Gets the start position of the spanned element at the specified index
        /// relative to the start position of the first element.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the spanned element.
        /// </param>
        /// <returns>
        /// The start position of the spanned element relative to the start position of the first element.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// <paramref name="index"/>is less than 0 or greater than or equal to <see cref="Count"/>.
        /// </exception>
        public abstract int GetElementOffset(int index);
    }
}

#region License
/*********************************************************************************
 * ReadOnlySeparatedSpanList.cs
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Eutherion.Text
{
    /// <summary>
    /// Represents a read-only list of spanned elements that can be accessed by index and that are separated by a separator.
    /// </summary>
    /// <typeparam name="TSpan">
    /// The type of spanned elements in the read-only list.
    /// </typeparam>
    /// <typeparam name="TSeparator">
    /// The type of separator between successive elements in the read-only list.
    /// </typeparam>
    public abstract class ReadOnlySeparatedSpanList<TSpan, TSeparator> : IReadOnlyList<TSpan>, ISpan where TSpan : ISpan where TSeparator : ISpan
    {
        private class ZeroElements : ReadOnlySeparatedSpanList<TSpan, TSeparator>
        {
            public override int Length => 0;

            public override TSpan this[int index] => throw new IndexOutOfRangeException();

            public override int Count => 0;

            public override IEnumerator<TSpan> GetEnumerator() => EmptyEnumerator<TSpan>.Instance;

            public override int AllElementCount => 0;

            public override IEnumerable<Union<TSpan, TSeparator>> AllElements => EmptyEnumerable<Union<TSpan, TSeparator>>.Instance;

            public override int GetElementOffset(int index) => throw new IndexOutOfRangeException();

            public override int GetSeparatorOffset(int index) => throw new IndexOutOfRangeException();

            public override int GetElementOrSeparatorOffset(int index) => throw new IndexOutOfRangeException();
        }

        private class OneElement : ReadOnlySeparatedSpanList<TSpan, TSeparator>
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

            public override int AllElementCount => 1;

            public override IEnumerable<Union<TSpan, TSeparator>> AllElements => new SingleElementEnumerable<Union<TSpan, TSeparator>>(element);

            public override int GetElementOffset(int index) => index == 0 ? 0 : throw new IndexOutOfRangeException();

            public override int GetSeparatorOffset(int index) => throw new IndexOutOfRangeException();

            public override int GetElementOrSeparatorOffset(int index) => index == 0 ? 0 : throw new IndexOutOfRangeException();
        }

        private class TwoOrMoreElements : ReadOnlySeparatedSpanList<TSpan, TSeparator>
        {
            private readonly TSpan[] array;
            private readonly TSeparator separator;
            private readonly int[] arrayElementOffsets;

            public TwoOrMoreElements(TSpan[] source, TSeparator separator)
            {
                if (source[0] == null) throw new ArgumentException(nameof(source));
                int length = source[0].Length;
                int separatorLength = separator.Length;
                arrayElementOffsets = new int[source.Length - 1];

                for (int i = 1; i < source.Length; i++)
                {
                    TSpan arrayElement = source[i];
                    if (arrayElement == null) throw new ArgumentException(nameof(source));
                    length += separatorLength;
                    arrayElementOffsets[i - 1] = length;
                    length += arrayElement.Length;
                }

                array = source;
                this.separator = separator;
                Length = length;
            }

            public override int Length { get; }

            public override TSpan this[int index] => array[index];

            public override int Count => array.Length;

            public override IEnumerator<TSpan> GetEnumerator() => ((ICollection<TSpan>)array).GetEnumerator();

            public override int AllElementCount => array.Length * 2 - 1;

            public override IEnumerable<Union<TSpan, TSeparator>> AllElements
            {
                get
                {
                    yield return array[0];

                    for (int i = 1; i < array.Length; i++)
                    {
                        yield return separator;
                        yield return array[i];
                    }
                }
            }

            public override int GetElementOffset(int index) => index == 0 ? 0 : arrayElementOffsets[index - 1];

            public override int GetSeparatorOffset(int index) => arrayElementOffsets[index] - separator.Length;

            public override int GetElementOrSeparatorOffset(int index)
            {
                if (index == 0) return 0;
                int offset = arrayElementOffsets[(index - 1) >> 1];
                if ((index & 1) != 0) offset -= separator.Length;
                return offset;
            }
        }

        /// <summary>
        /// Gets the empty <see cref="ReadOnlySeparatedSpanList{TSpan, TSeparator}"/>.
        /// </summary>
        public static readonly ReadOnlySeparatedSpanList<TSpan, TSeparator> Empty = new ZeroElements();

        /// <summary>
        /// Initializes a new instance of <see cref="ReadOnlySeparatedSpanList{TSpan, TSeparator}"/>.
        /// </summary>
        /// <param name="source">
        /// The elements of the list.
        /// </param>
        /// <param name="separator">
        /// The separator between successive elements of the list.
        /// </param>
        /// <returns>
        /// The initialized <see cref="ReadOnlySeparatedSpanList{TSpan, TSeparator}"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> and/or <paramref name="separator"/> are null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// One or more elements in <paramref name="source"/> are null.
        /// </exception>
        public static ReadOnlySeparatedSpanList<TSpan, TSeparator> Create(IEnumerable<TSpan> source, TSeparator separator)
        {
            var array = source is ReadOnlyList<TSpan> readOnlyList ? readOnlyList.ReadOnlyArray : source.ToArrayEx();
            if (separator == null) throw new ArgumentNullException(nameof(separator));
            if (array.Length == 0) return Empty;
            if (array.Length == 1) return new OneElement(array[0]);
            return new TwoOrMoreElements(array, separator);
        }

        private ReadOnlySeparatedSpanList() { }

        /// <summary>
        /// Gets the length of this <see cref="ReadOnlySeparatedSpanList{TSpan, TSeparator}"/>.
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
        /// Gets the number of spanned elements in the list, excluding the separators.
        /// See also: <seealso cref="AllElementCount"/>.
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
        /// Gets the number of spanned elements in the list, including the separators.
        /// See also: <seealso cref="Count"/>.
        /// </summary>
        public abstract int AllElementCount { get; }

        /// <summary>
        /// Enumerates all elements of the list, including separators.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerable{T}"/> that enumerates all elements of the list, including separators.
        /// </returns>
        public abstract IEnumerable<Union<TSpan, TSeparator>> AllElements { get; }

        /// <summary>
        /// Gets the start position of the spanned element at the specified index
        /// relative to the start position of the first element.
        /// See also: <seealso cref="GetSeparatorOffset"/>, <seealso cref="GetElementOrSeparatorOffset"/>.
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

        /// <summary>
        /// Gets the start position of the separator at the specified index
        /// relative to the start position of the first element.
        /// The number of separators is always one less than the number of elements.
        /// See also: <seealso cref="GetElementOffset"/>, <seealso cref="GetElementOrSeparatorOffset"/>.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the separator.
        /// </param>
        /// <returns>
        /// The start position of the separator relative to the start position of the first element.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// <paramref name="index"/>is less than 0 or greater than or equal to <see cref="Count"/> - 1.
        /// </exception>
        public abstract int GetSeparatorOffset(int index);

        /// <summary>
        /// Gets the start position of the spanned element or separator at the specified index
        /// relative to the start position of the first element.
        /// See also: <seealso cref="GetElementOffset"/>, <seealso cref="GetSeparatorOffset"/>.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the spanned element or separator.
        /// </param>
        /// <returns>
        /// The start position of the spanned element or separator relative to the start position of the first element.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// <paramref name="index"/>is less than 0 or greater than or equal to <see cref="AllElementCount"/>.
        /// </exception>
        public abstract int GetElementOrSeparatorOffset(int index);
    }
}

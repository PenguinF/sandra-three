#region License
/*********************************************************************************
 * LinqExtensions.cs
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

using System.Collections.Generic;

namespace System.Linq
{
    public static class LinqExtensions
    {
        /// <summary>
        /// Determines whether there is any element in a sequence, and returns that element.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source"/>.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IEnumerable{TSource}"/> whose elements to check.
        /// </param>
        /// <param name="value">
        /// Returns the found element if true was returned, otherwise a default value.
        /// </param>
        /// <returns>
        /// true if the source sequence contains an element, otherwise false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> is null (Nothing in Visual Basic).
        /// </exception>
        public static bool Any<TSource>(this IEnumerable<TSource> source, out TSource value)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            foreach (var element in source)
            {
                value = element;
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Determines whether any element of a sequence satisfies a condition, and returns such an element.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source"/>.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IEnumerable{TSource}"/> whose elements to apply the predicate to.
        /// </param>
        /// <param name="predicate">
        /// A function to test each element for a condition.
        /// </param>
        /// <param name="value">
        /// Returns the found element if true was returned, otherwise a default value.
        /// </param>
        /// <returns>
        /// true if any elements in the source sequence pass the test in the specified predicate, otherwise false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> or <paramref name="predicate"/> is null (Nothing in Visual Basic).
        /// </exception>
        public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, out TSource value)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            foreach (var element in source)
            {
                if (predicate(element))
                {
                    value = element;
                    return true;
                }
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Enumerates each element of a sequence. This is useful to protect references to mutable collections
        /// from being leaked. Instead, only the elements of a mutable collection are enumerated, and casts
        /// to mutable destination collection types will fail.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source"/>.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of values.
        /// </param>
        /// <returns>
        /// A <see cref="IEnumerable{T}"/> whose elements are the same as the elements in <paramref name="source"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> is null.
        /// </exception>
        public static IEnumerable<TSource> Enumerate<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            foreach (var element in source)
            {
                yield return element;
            }
        }

        /// <summary>
        /// Performs an action on each element of a sequence.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source"/>.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of elements.
        /// </param>
        /// <param name="action">
        /// The action to perform on each element of the sequence.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> or <paramref name="action"/> is null.
        /// </exception>
        public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            foreach (var element in source)
            {
                action(element);
            }
        }

        /// <summary>
        /// Creates an array from a <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source"/>.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of elements.
        /// </param>
        /// <returns>
        /// An array that contains the elements from the input sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> is null.
        /// </exception>
        /// <remarks>
        /// This is similar to the regular <see cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})"/>,
        /// but contains more code targeted to input sequences of a specific type.
        /// </remarks>
        public static TSource[] ToArrayEx<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            // Use ICollection.CopyTo() if possible, to ensure only one array is allocated.
            TSource[] array;
            int length;
            switch (source)
            {
                // This covers the TSource[] too; expression (ICollection<TSource>)(new TSource[0]) is valid.
                case ICollection<TSource> collection:
                    length = collection.Count;
                    if (length > 0)
                    {
                        array = new TSource[length];
                        collection.CopyTo(array, 0);
                        return array;
                    }
                    break;
                case IReadOnlyCollection<TSource> readOnlyCollection:
                    length = readOnlyCollection.Count;
                    if (length > 0)
                    {
                        array = new TSource[length];
                        int index = 0;

                        try
                        {
                            foreach (var element in readOnlyCollection)
                            {
                                array[index] = element;

                                // Don't check if index >= length, assume that readOnlyCollection
                                // satisfies the contract that the number of enumerated elements is always equal to Count.
                                // Do catch IndexOutOfRangeExceptions in case the readOnlyCollection returns more
                                // elements than it should.
                                index++;
                            }
                        }
                        catch (IndexOutOfRangeException)
                        {
                            // Ignore this exception, the IReadOnlyCollection does not honor its contract.
                        }

                        return array;
                    }
                    break;
                default:
                    array = source.ToArray();
                    if (array.Length > 0) return array;
                    break;
            }

            // Default case if empty.
            return Array.Empty<TSource>();
        }
    }
}

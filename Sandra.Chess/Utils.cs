/*********************************************************************************
 * Utils.cs
 * 
 * Copyright (c) 2004-2016 Henk Nicolai
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
 *********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandra
{
    /// <summary>
    /// Contains utility extension methods.
    /// </summary>
    public static class UtilityExtensions
    {
        /// <summary>
        /// Sets a single value at each index of the array.
        /// </summary>
        public static void Fill<T>(this T[] array, T value)
        {
            for (int i = array.Length - 1; i >= 0; --i)
            {
                array[i] = value;
            }
        }
    }

    /// <summary>
    /// Contains general helper methods for enumerations.
    /// </summary>
    /// <remarks>
    /// Declaring an <see cref="EnumHelper{TEnum}"/> with a non-enumeration type
    /// results in a <see cref="TypeInitializationException"/> being thrown.
    /// </remarks>
    public static class EnumHelper<TEnum> where TEnum : struct
    {
        static readonly TEnum[] distinctValues;

        static EnumHelper()
        {
            TEnum[] values = (TEnum[])Enum.GetValues(typeof(TEnum));
            distinctValues = values.Distinct().OrderBy(x => x).ToArray();
            EnumCount = distinctValues.Length;
        }

        /// <summary>
        /// Gets the number of distinct values in the enumeration.
        /// </summary>
        public static readonly int EnumCount;

        /// <summary>
        /// Enumerates all distinct values in <typeparamref name="TEnum"/>.
        /// </summary>
        public static IEnumerable<TEnum> AllValues
        {
            get
            {
                foreach (var value in distinctValues)
                {
                    yield return value;
                }
            }
        }
    }
}

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
        /// true if any elements in the source sequence pass the test in the specified predicate, otherwise false.
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
            value = default(TSource);
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
            value = default(TSource);
            return false;
        }
    }
}

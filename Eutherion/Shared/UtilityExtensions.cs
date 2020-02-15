#region License
/*********************************************************************************
 * UtilityExtensions.cs
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;

namespace Eutherion
{
    /// <summary>
    /// Contains utility extension methods.
    /// </summary>
    public static class UtilityExtensions
    {
        /// <summary>
        /// Returns a locale invariant representation of an integer.
        /// </summary>
        /// <param name="value">
        /// The integer to convert.
        /// </param>
        /// <returns>
        /// The locale invariant representation
        /// </returns>
        public static string ToStringInvariant(this int value)
            => value.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Returns a locale invariant representation of an integer.
        /// </summary>
        /// <param name="value">
        /// The integer to convert.
        /// </param>
        /// <returns>
        /// The locale invariant representation
        /// </returns>
        public static string ToStringInvariant(this BigInteger value)
            => value.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Sets a single value at each index of the array.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the elements of the array.
        /// </typeparam>
        /// <param name="array">
        /// The one-dimensional, zero-based array to fill.
        /// </param>
        /// <param name="value">
        /// The value to set at each index of the array.
        /// </param>
        public static void Fill<T>(this T[] array, T value)
        {
            for (int i = array.Length - 1; i >= 0; --i)
            {
                array[i] = value;
            }
        }

        /// <summary>
        /// Iterates an action a fixed number of times.
        /// If the number of iterations is zero or lower, the action won't get executed at all.
        /// </summary>
        public static void Times(this int numberOfIterations, Action action)
        {
            for (int i = numberOfIterations; i > 0; --i) action();
        }

        /// <summary>
        /// Adds a value to a <see cref="Dictionary{TKey, TValue}" /> if the key does not already exist.
        /// </summary>
        /// <typeparam name="TKey">
        /// The type of keys in the dictionary.
        /// </typeparam>
        /// <typeparam name="TValue">
        /// The type of values in the dictionary.
        /// </typeparam>
        /// <param name="dictionary">
        /// The dictionary in which to add the value.
        /// </param>
        /// <param name="key">
        /// The key of the value to add.
        /// </param>
        /// <param name="constructor">
        /// The function used to generate a value for the key if it does not exist.
        /// </param>
        /// <returns>
        /// The value for the key. This will be either the existing value for the key if the key is already in the dictionary,
        /// or the new value for the key as returned by <paramref name="constructor" /> if the key was not in the dictionary.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dictionary"/> and/or <paramref name="key"/> and/or <paramref name="constructor"/> are null.
        /// </exception>
        /// <remarks>
        /// The key of the value returned from <paramref name="constructor" /> is assumed to be equal to the passed in <paramref name="key" /> parameter.
        /// </remarks>
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> constructor)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            if (constructor == null) throw new ArgumentNullException(nameof(constructor));

            if (dictionary.TryGetValue(key, out TValue value)) return value;
            value = constructor(key);
            dictionary.Add(key, value);
            return value;
        }

        /// <summary>
        /// Adds the specified key-value pair to the dictionary.
        /// </summary>
        /// <typeparam name="TKey">
        /// The type of keys in the dictionary.
        /// </typeparam>
        /// <typeparam name="TValue">
        /// The type of values in the dictionary.
        /// </typeparam>
        /// <param name="dictionary">
        /// The dictionary in which to add the key-value pair.
        /// </param>
        /// <param name="keyValuePair">
        /// The key and value of the element to add.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dictionary"/> is null -or- <paramref name="keyValuePair"/>.Key is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// An element with the same key already exists in the dictionary.
        /// </exception>
        public static void Add<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, KeyValuePair<TKey, TValue> keyValuePair)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

            dictionary.Add(keyValuePair.Key, keyValuePair.Value);
        }

        /// <summary>
        /// Writes an <see cref="Exception"/> to <see cref="Debug"/>.  
        /// </summary>
        /// <param name="exception">
        /// The <see cref="Exception"/> to trace.
        /// </param>
        public static void Trace(this Exception exception)
        {
            if (exception != null)
            {
                Debug.WriteLine($"{exception.GetType().FullName}: {exception.Message}");
            }
        }
    }
}

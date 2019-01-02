#region License
/*********************************************************************************
 * PMap.cs
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

namespace Sandra.UI.WF.Storage
{
    /// <summary>
    /// Represents a read-only map of string keys onto <see cref="PValue"/>s.
    /// </summary>
    public class PMap : IReadOnlyDictionary<string, PValue>, PValue
    {
        // Prevent repeated allocations of empty dictionaries.
        private static readonly Dictionary<string, PValue> emptyMap = new Dictionary<string, PValue>();

        /// <summary>
        /// Represents the empty <see cref="PMap"/>, which contains no key-value pairs.
        /// </summary>
        public static readonly PMap Empty = new PMap(null);

        private readonly Dictionary<string, PValue> map;

        /// <summary>
        /// Initializes a new instance of <see cref="PMap"/>.
        /// </summary>
        /// <param name="map">
        /// The map which contains the key-value pairs to construct this <see cref="PMap"/> with.
        /// </param>
        /// <exception cref="ArgumentException">
        /// <paramref name="map"/> contains one or more duplicate keys.
        /// </exception>
        public PMap(IDictionary<string, PValue> map)
        {
            this.map = map != null && map.Count > 0
                ? new Dictionary<string, PValue>(map)
                : emptyMap;
        }

        /// <summary>
        /// Gets the <see cref="PValue"/> associated with the specified key.
        /// </summary>
        /// <param name="key">
        /// The key to locate.
        /// </param>
        /// <returns>
        /// The value associated with the specified key.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is null.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        /// The key does not exist.
        /// </exception>
        public PValue this[string key] => map[key];

        /// <summary>
        /// Gets the number of key-value pairs in this <see cref="PMap"/>.
        /// </summary>
        public int Count => map.Count;

        /// <summary>
        /// Enumerates all keys in this <see cref="PMap"/>.
        /// </summary>
        public IEnumerable<string> Keys => map.Keys;

        /// <summary>
        /// Enumerates all values in this <see cref="PMap"/>.
        /// </summary>
        public IEnumerable<PValue> Values => map.Values;

        /// <summary>
        /// Determines whether this <see cref="PMap"/> contains a <see cref="PValue"/> with the specified key.
        /// </summary>
        /// <param name="key">
        /// The key to locate.
        /// </param>
        /// <returns>
        /// true if this <see cref="PMap"/> contains a <see cref="PValue"/> with the specified key; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is null.
        /// </exception>
        public bool ContainsKey(string key) => map.ContainsKey(key);

        /// <summary>
        /// Enumerates all key-value pairs in this <see cref="PMap"/>.
        /// </summary>
        public IEnumerator<KeyValuePair<string, PValue>> GetEnumerator() => map.GetEnumerator();

        /// <summary>
        /// Gets the <see cref="PValue"/> that is associated with the specified key.
        /// </summary>
        /// <param name="key">
        /// The key to locate.
        /// </param>
        /// <param name="value">
        /// When this method returns, contains the <see cref="PValue"/> associated with the specified key, if the key is found;
        /// otherwise, the default <see cref="PValue"/> value.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// true if this <see cref="PMap"/> contains a <see cref="PValue"/> with the specified key; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is null.
        /// </exception>
        public bool TryGetValue(string key, out PValue value) => map.TryGetValue(key, out value);

        /// <summary>
        /// Compares this <see cref="PMap"/> with another and returns if they are equal.
        /// </summary>
        /// <param name="other">
        /// The <see cref="PMap"/> to compare with.
        /// </param>
        /// <returns>
        /// true if both <see cref="PMap"/> instances are equal; otherwise false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="other"/> is null.
        /// </exception>
        public bool EqualTo(PMap other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            // Compare Count properties for a fast exit if they are different.
            if (map.Count != other.map.Count) return false;

            // Both key sets need to match exactly, but if Counts are equal a unidirectional check is sufficient.
            PValueEqualityComparer eq = PValueEqualityComparer.Instance;
            return map.All(kv => other.map.TryGetValue(kv.Key, out PValue otherValue)
                              && eq.AreEqual(kv.Value, otherValue));
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void PValue.Accept(PValueVisitor visitor) => visitor.VisitMap(this);
        TResult PValue.Accept<TResult>(PValueVisitor<TResult> visitor) => visitor.VisitMap(this);
        TResult PValue.Accept<T, TResult>(PValueVisitor<T, TResult> visitor, T arg) => visitor.VisitMap(this, arg);
    }
}

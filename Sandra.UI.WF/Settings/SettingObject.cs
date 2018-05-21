/*********************************************************************************
 * SettingObject.cs
 * 
 * Copyright (c) 2004-2018 Henk Nicolai
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents a read-only collection of setting values (<see cref="ISettingValue"/>) indexed by <see cref="SettingKey"/>.
    /// </summary>
    public class SettingObject : IReadOnlyDictionary<SettingKey, ISettingValue>
    {
        internal readonly Dictionary<SettingKey, ISettingValue> Mapping;

        internal SettingObject()
        {
            Mapping = new Dictionary<SettingKey, ISettingValue>();
        }

        /// <summary>
        /// Constructs a new <see cref="SettingObject"/> from a working copy.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="workingCopy"/> is null.
        /// </exception>
        public SettingObject(SettingCopy workingCopy)
        {
            if (workingCopy == null) throw new ArgumentNullException(nameof(workingCopy));

            Mapping = new Dictionary<SettingKey, ISettingValue>(workingCopy.KeyValueMapping);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
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
        public ISettingValue this[SettingKey key] => Mapping[key];

        /// <summary>
        /// Gets the number of key-value pairs in this <see cref="SettingObject"/>.
        /// </summary>
        public int Count => Mapping.Count;

        /// <summary>
        /// Enumerates all keys in this <see cref="SettingObject"/>.
        /// </summary>
        public IEnumerable<SettingKey> Keys => Mapping.Keys;

        /// <summary>
        /// Enumerates all values in this <see cref="SettingObject"/>.
        /// </summary>
        public IEnumerable<ISettingValue> Values => Mapping.Values;

        /// <summary>
        /// Determines whether this <see cref="SettingObject"/> contains a value with the specified key.
        /// </summary>
        /// <param name="key">
        /// The key to locate.
        /// </param>
        /// <returns>
        /// true if this <see cref="SettingObject"/> contains a value with the specified key; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is null.
        /// </exception>
        public bool ContainsKey(SettingKey key) => Mapping.ContainsKey(key);

        /// <summary>
        /// Enumerates all key-value pairs in this <see cref="SettingObject"/>.
        /// </summary>
        public IEnumerator<KeyValuePair<SettingKey, ISettingValue>> GetEnumerator() => Mapping.GetEnumerator();

        /// <summary>
        /// Gets the value that is associated with the specified key.
        /// </summary>
        /// <param name="key">
        /// The key to locate.
        /// </param>
        /// <param name="value">
        /// When this method returns, contains the value associated with the specified key, if the key is found;
        /// otherwise, the default value for the type of the value parameter.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// true if this <see cref="SettingObject"/> contains a value with the specified key; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is null.
        /// </exception>
        public bool TryGetValue(SettingKey key, out ISettingValue value) => Mapping.TryGetValue(key, out value);

        /// <summary>
        /// Creates a working <see cref="SettingCopy"/> based on this <see cref="SettingObject"/>.
        /// </summary>
        public SettingCopy CreateWorkingCopy()
        {
            var copy = new SettingCopy();
            copy.Revert(this);
            return copy;
        }

        /// <summary>
        /// Compares this <see cref="SettingObject"/> with another and returns if they are equal.
        /// </summary>
        /// <param name="other">
        /// The <see cref="SettingObject"/> to compare with.
        /// </param>
        /// <returns>
        /// true if both <see cref="SettingObject"/> instances are equal; otherwise false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="other"/> is null.
        /// </exception>
        public bool EqualTo(SettingObject other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            // Compare Count properties for a fast exit if they are different.
            if (Mapping.Count != other.Mapping.Count) return false;

            // Both key sets need to match exactly, but if Counts are equal a unidirectional check is sufficient.
            SettingValueEqualityComparer eq = SettingValueEqualityComparer.Instance;
            ISettingValue otherValue;
            return Mapping.All(kv => other.Mapping.TryGetValue(kv.Key, out otherValue)
                                  && eq.AreEqual(kv.Value, otherValue));
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

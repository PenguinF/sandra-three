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
using System.Collections;
using System.Collections.Generic;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents a read-only collection of setting values (<see cref="PValue"/>) indexed by <see cref="SettingKey"/>.
    /// </summary>
    public class SettingObject : IReadOnlyDictionary<SettingKey, PValue>
    {
        internal readonly Dictionary<SettingKey, PValue> Map;

        internal SettingObject(SettingCopy workingCopy)
        {
            Map = new Dictionary<SettingKey, PValue>(workingCopy.KeyValueMapping);
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
        public PValue this[SettingKey key] => Map[key];

        /// <summary>
        /// Gets the number of key-value pairs in this <see cref="SettingObject"/>.
        /// </summary>
        public int Count => Map.Count;

        /// <summary>
        /// Enumerates all keys in this <see cref="SettingObject"/>.
        /// </summary>
        public IEnumerable<SettingKey> Keys => Map.Keys;

        /// <summary>
        /// Enumerates all values in this <see cref="SettingObject"/>.
        /// </summary>
        public IEnumerable<PValue> Values => Map.Values;

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
        public bool ContainsKey(SettingKey key) => Map.ContainsKey(key);

        /// <summary>
        /// Enumerates all key-value pairs in this <see cref="SettingObject"/>.
        /// </summary>
        public IEnumerator<KeyValuePair<SettingKey, PValue>> GetEnumerator() => Map.GetEnumerator();

        /// <summary>
        /// Gets the value that is associated with the specified key.
        /// </summary>
        /// <param name="key">
        /// The key to locate.
        /// </param>
        /// <param name="value">
        /// When this method returns, contains the value associated with the specified key, if the key is found;
        /// otherwise, the default <see cref="PValue"/> value.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// true if this <see cref="SettingObject"/> contains a value with the specified key; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is null.
        /// </exception>
        public bool TryGetValue(SettingKey key, out PValue value) => Map.TryGetValue(key, out value);

        /// <summary>
        /// Creates a working <see cref="SettingCopy"/> based on this <see cref="SettingObject"/>.
        /// </summary>
        public SettingCopy CreateWorkingCopy()
        {
            var copy = new SettingCopy();
            copy.Revert(this);
            return copy;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

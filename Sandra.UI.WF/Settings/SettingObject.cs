﻿/*********************************************************************************
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
using System.Numerics;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents a read-only collection of setting values (<see cref="PValue"/>) indexed by <see cref="SettingKey"/>.
    /// </summary>
    public class SettingObject : IReadOnlyDictionary<SettingKey, PValue>
    {
        internal readonly Dictionary<SettingKey, PValue> Mapping;

        internal SettingObject()
        {
            Mapping = new Dictionary<SettingKey, PValue>();
        }

        internal SettingObject(SettingCopy workingCopy)
        {
            Mapping = new Dictionary<SettingKey, PValue>(workingCopy.KeyValueMapping);
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
        public PValue this[SettingKey key] => Mapping[key];

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
        public IEnumerable<PValue> Values => Mapping.Values;

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
        public IEnumerator<KeyValuePair<SettingKey, PValue>> GetEnumerator() => Mapping.GetEnumerator();

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
        public bool TryGetValue(SettingKey key, out PValue value) => Mapping.TryGetValue(key, out value);

        /// <summary>
        /// Gets the value that is associated with the specified key if it is a <see cref="bool"/>.
        /// </summary>
        /// <param name="key">
        /// The key to locate.
        /// </param>
        /// <param name="value">
        /// When this method returns, contains the value associated with the specified key, if the key is found
        /// and the value associated with it is a <see cref="bool"/>; otherwise, the default <see cref="bool"/> value.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// true if this <see cref="SettingObject"/> contains a value with the specified key
        /// and the value associated with it is a <see cref="bool"/>; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is null.
        /// </exception>
        public bool TryGetValue(SettingKey key, out bool value)
        {
            PValue settingValue;
            if (Mapping.TryGetValue(key, out settingValue) && settingValue is PBoolean)
            {
                value = ((PBoolean)settingValue).Value;
                return true;
            }

            value = default(bool);
            return false;
        }

        /// <summary>
        /// Gets the value that is associated with the specified key if it's an <see cref="int"/>.
        /// </summary>
        /// <param name="key">
        /// The key to locate.
        /// </param>
        /// <param name="value">
        /// When this method returns, contains the value associated with the specified key, if the key is found
        /// and the value associated with it is an <see cref="int"/>; otherwise, the default <see cref="int"/> value.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// true if this <see cref="SettingObject"/> contains a value with the specified key
        /// and the value associated with it is a <see cref="int"/>; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is null.
        /// </exception>
        public bool TryGetValue(SettingKey key, out int value)
        {
            PValue settingValue;
            if (Mapping.TryGetValue(key, out settingValue) && settingValue is PInteger)
            {
                BigInteger bigInteger = ((PInteger)settingValue).Value;
                if (int.MinValue <= bigInteger && bigInteger <= int.MaxValue)
                {
                    value = (int)bigInteger;
                    return true;
                }
            }

            value = default(int);
            return false;
        }

        /// <summary>
        /// Gets the value that is associated with the specified key if it is a <see cref="string"/>.
        /// </summary>
        /// <param name="key">
        /// The key to locate.
        /// </param>
        /// <param name="value">
        /// When this method returns, contains the value associated with the specified key, if the key is found
        /// and the value associated with it is a <see cref="string"/>; otherwise, the default <see cref="string"/> value.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// true if this <see cref="SettingObject"/> contains a value with the specified key
        /// and the value associated with it is a <see cref="string"/>; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is null.
        /// </exception>
        public bool TryGetValue(SettingKey key, out string value)
        {
            PValue settingValue;
            if (Mapping.TryGetValue(key, out settingValue) && settingValue is PString)
            {
                value = ((PString)settingValue).Value;
                return true;
            }

            value = default(string);
            return false;
        }

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
        /// <remarks>
        /// This is not the same as complete equality, in particular this method returns true from the following expression:
        /// <code>
        /// workingCopy.Commit().EqualTo(workingCopy)
        /// </code>
        /// where workingCopy is a <see cref="SettingCopy"/>. Or even:
        /// <code>
        /// workingCopy.Commit().CreateWorkingCopy().EqualTo(workingCopy)
        /// </code>
        /// </remarks>
        public bool EqualTo(SettingObject other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            Dictionary<string, PValue> temp1 = new Dictionary<string, PValue>();
            foreach (var kv in this) temp1.Add(kv.Key.Key, kv.Value);
            PMap map1 = new PMap(temp1);

            Dictionary<string, PValue> temp2 = new Dictionary<string, PValue>();
            foreach (var kv in other) temp2.Add(kv.Key.Key, kv.Value);
            PMap map2 = new PMap(temp2);

            return map1.EqualTo(map2);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

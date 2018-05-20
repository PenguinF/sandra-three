/*********************************************************************************
 * SettingKey.cs
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
using System.Diagnostics;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents a key for a setting value.
    /// </summary>
    [DebuggerDisplay("{Key}")]
    public sealed class SettingKey : IEquatable<SettingKey>
    {
        private readonly string Key;

        /// <summary>
        /// Initializes a new instance of <see cref="SettingKey"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="key"/> is <see cref="string.Empty"/>, or contains a double quote character (").
        /// </exception>
        public SettingKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (key.Length == 0)
            {
                throw new ArgumentException($"{nameof(key)} is string.Empty.", nameof(key));
            }

            if (key.Contains("\""))
            {
                throw new ArgumentException($"{nameof(key)} contains a double quote character (\").", nameof(key));
            }

            Key = key;
        }

        public bool Equals(SettingKey other) => other != null && Key == other.Key;

        public override bool Equals(object obj) => Equals(obj as SettingKey);

        public override int GetHashCode() => Key.GetHashCode();

        public static bool operator ==(SettingKey first, SettingKey second)
        {
            if (ReferenceEquals(null, first)) return ReferenceEquals(null, second);
            if (ReferenceEquals(null, second)) return false;
            return first.Key == second.Key;
        }

        public static bool operator !=(SettingKey first, SettingKey second)
        {
            if (ReferenceEquals(null, first)) return !ReferenceEquals(null, second);
            if (ReferenceEquals(null, second)) return true;
            return first.Key != second.Key;
        }
    }
}

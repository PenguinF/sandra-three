#region License
/*********************************************************************************
 * LocalizedStringKey.cs
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
using System.Diagnostics;

namespace Eutherion.Localization
{
    /// <summary>
    /// Represents an immutable identifier for a localized string.
    /// </summary>
    [DebuggerDisplay("{Key}")]
    public sealed class LocalizedStringKey : IEquatable<LocalizedStringKey>
    {
        /// <summary>
        /// Gets the string representation of this <see cref="LocalizedStringKey"/>. 
        /// </summary>
        public readonly string Key;

        /// <summary>
        /// Constructs a new instance of <see cref="LocalizedStringKey"/>,
        /// in which the provided string key is used for equality comparison and hashcode generation.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is null.
        /// </exception>
        public LocalizedStringKey(string key) => Key = key ?? throw new ArgumentNullException(nameof(key));

        public bool Equals(LocalizedStringKey other) => other != null
                                                     && Key == other.Key;

        public override bool Equals(object obj) => Equals(obj as LocalizedStringKey);

        public override int GetHashCode() => Key.GetHashCode();

        public static bool operator ==(LocalizedStringKey first, LocalizedStringKey second)
        {
            if (first is null) return second is null;
            if (second is null) return false;
            return first.Key == second.Key;
        }

        public static bool operator !=(LocalizedStringKey first, LocalizedStringKey second)
        {
            if (first is null) return !(second is null);
            if (second is null) return true;
            return first.Key != second.Key;
        }
    }
}

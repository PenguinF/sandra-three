/*********************************************************************************
 * UIAction.cs
 * 
 * Copyright (c) 2004-2017 Henk Nicolai
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

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents an immutable, opaque atom for a user action.
    /// </summary>
    public sealed class UIAction : IEquatable<UIAction>
    {
        private readonly string Key;

        /// <summary>
        /// Constructs a new instance of <see cref="UIAction"/>, in which the provided string key is used for equality comparison and hashcode generation.
        /// </summary>
        public UIAction(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            Key = key;
        }

        public bool Equals(UIAction other)
        {
            return other != null && Key == other.Key;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as UIAction);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        public static bool operator ==(UIAction first, UIAction second)
        {
            if (ReferenceEquals(null, first)) return ReferenceEquals(null, second);
            if (ReferenceEquals(null, second)) return false;
            return first.Key == second.Key;
        }

        public static bool operator !=(UIAction first, UIAction second)
        {
            if (ReferenceEquals(null, first)) return !ReferenceEquals(null, second);
            if (ReferenceEquals(null, second)) return true;
            return first.Key != second.Key;
        }
    }
}

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

    /// <summary>
    /// Represents one of three types of access to a <see cref="UIAction"/>.
    /// </summary>
    public enum UIActionAccessType
    {
        /// <summary>
        /// The <see cref="UIAction"/> is currently not visible.
        /// </summary>
        Hidden,
        /// <summary>
        /// The <see cref="UIAction"/> is currently visible, but disabled.
        /// </summary>
        Disabled,
        /// <summary>
        /// The <see cref="UIAction"/> is currently visible and enabled.
        /// </summary>
        Enabled,
    }

    /// <summary>
    /// Encodes a current state of a <see cref="UIAction"/> in how it is represented in e.g. menu items.
    /// </summary>
    public struct UIActionState
    {
        /// <summary>
        /// Gets the current type of access to the <see cref="UIAction"/>.
        /// </summary>
        public UIActionAccessType UIActionAccessType { get; }

        /// <summary>
        /// Gets if the <see cref="UIAction"/> is currently in a checked state.
        /// </summary>
        public bool Checked { get; }

        /// <summary>
        /// Gets if the <see cref="UIAction"/> is currently visible.
        /// </summary>
        public bool Visible => UIActionAccessType != UIActionAccessType.Hidden;

        /// <summary>
        /// Gets if the <see cref="UIAction"/> is currently enabled.
        /// </summary>
        public bool Enabled => UIActionAccessType == UIActionAccessType.Enabled;

        public UIActionState(UIActionAccessType accessType)
        {
            UIActionAccessType = accessType;
            Checked = false;
        }

        public UIActionState(UIActionAccessType accessType, bool isChecked)
        {
            UIActionAccessType = accessType;
            Checked = isChecked;
        }

        public static implicit operator UIActionState(UIActionAccessType accessType) => new UIActionState(accessType);
    }
}

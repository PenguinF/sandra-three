#region License
/*********************************************************************************
 * UIAction.cs
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

namespace Eutherion.UIActions
{
    /// <summary>
    /// Represents an immutable, opaque atom for a user action.
    /// </summary>
    [DebuggerDisplay("{Key}")]
    public sealed class UIAction : IEquatable<UIAction>
    {
        private readonly string Key;

        /// <summary>
        /// Constructs a new instance of <see cref="UIAction"/>,
        /// in which the provided string key is used for equality comparison and hashcode generation.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is null.
        /// </exception>
        public UIAction(string key) => Key = key ?? throw new ArgumentNullException(nameof(key));

        public bool Equals(UIAction other) => other != null
                                           && Key == other.Key;

        public override bool Equals(object obj) => Equals(obj as UIAction);

        public override int GetHashCode() => Key.GetHashCode();

        public static bool operator ==(UIAction first, UIAction second)
        {
            if (first is null) return second is null;
            if (second is null) return false;
            return first.Key == second.Key;
        }

        public static bool operator !=(UIAction first, UIAction second)
        {
            if (first is null) return !(second is null);
            if (second is null) return true;
            return first.Key != second.Key;
        }
    }

    /// <summary>
    /// Represents one of three types of visibility to a <see cref="UIAction"/>.
    /// </summary>
    public enum UIActionVisibility
    {
        /// <summary>
        /// The visibility of a <see cref="UIAction"/> is determined by some parent control.
        /// </summary>
        Parent,
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
        /// Gets the current visibility of the <see cref="UIAction"/>.
        /// </summary>
        public UIActionVisibility UIActionVisibility { get; }

        /// <summary>
        /// Gets if the <see cref="UIAction"/> is currently in a checked state.
        /// </summary>
        public bool Checked { get; }

        /// <summary>
        /// Gets if the <see cref="UIAction"/> is currently visible.
        /// </summary>
        public bool Visible => UIActionVisibility == UIActionVisibility.Disabled || Enabled;

        /// <summary>
        /// Gets if the <see cref="UIAction"/> is currently enabled.
        /// </summary>
        public bool Enabled => UIActionVisibility == UIActionVisibility.Enabled;

        /// <summary>
        /// Initializes a new instance of <see cref="UIActionState"/> with a given <see cref="UIActions.UIActionVisibility"/>
        /// and which is unchecked.
        /// </summary>
        /// <param name="visibility">
        /// The <see cref="UIActions.UIActionVisibility"/> of this <see cref="UIActionState"/>.
        /// </param>
        public UIActionState(UIActionVisibility visibility)
        {
            UIActionVisibility = visibility;
            Checked = false;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UIActionState"/> with a given <see cref="UIActions.UIActionVisibility"/>
        /// and checked state.
        /// </summary>
        /// <param name="visibility">
        /// The <see cref="UIActions.UIActionVisibility"/> of this <see cref="UIActionState"/>.
        /// </param>
        /// <param name="isChecked">
        /// The checked state of this <see cref="UIActionState"/>.
        /// </param>
        public UIActionState(UIActionVisibility visibility, bool isChecked)
        {
            UIActionVisibility = visibility;
            Checked = isChecked;
        }

        public static implicit operator UIActionState(UIActionVisibility visibility) => new UIActionState(visibility);
    }
}

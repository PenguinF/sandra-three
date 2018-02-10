﻿/*********************************************************************************
 * LocalizedString.cs
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
    /// Represents an immutable identifier for a <see cref="LocalizedString"/>.
    /// </summary>
    [DebuggerDisplay("{Key}")]
    public sealed class LocalizedStringKey : IEquatable<LocalizedStringKey>
    {
        /// <summary>
        /// Creates a <see cref="LocalizedStringKey"/> that serves as a placeholder key for strings that cannot be localized.
        /// </summary>
        public static LocalizedStringKey Unlocalizable(string displayText) => new LocalizedStringKey(null, displayText);

        /// <summary>
        /// Gets the string representation of this <see cref="LocalizedStringKey"/>. 
        /// </summary>
        internal readonly string Key;

        /// <summary>
        /// For untranslatable keys, returns the display text.
        /// </summary>
        internal readonly string DisplayText;

        private LocalizedStringKey(string key, string displayText)
        {
            if (displayText == null) throw new ArgumentNullException(nameof(displayText));
            Key = key;
            DisplayText = displayText;
        }

        /// <summary>
        /// Constructs a new instance of <see cref="LocalizedStringKey"/>.
        /// </summary>
        public LocalizedStringKey(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            Key = key;
        }

        public bool Equals(LocalizedStringKey other) => other != null
                                                     && Key == other.Key
                                                     && DisplayText == other.DisplayText;

        public override bool Equals(object obj) => Equals(obj as LocalizedStringKey);

        public override int GetHashCode() => Key != null ? Key.GetHashCode() : DisplayText.GetHashCode();

        public static bool operator ==(LocalizedStringKey first, LocalizedStringKey second)
        {
            if (ReferenceEquals(null, first)) return ReferenceEquals(null, second);
            if (ReferenceEquals(null, second)) return false;
            return first.Key == second.Key && first.DisplayText == second.DisplayText;
        }

        public static bool operator !=(LocalizedStringKey first, LocalizedStringKey second)
        {
            if (ReferenceEquals(null, first)) return !ReferenceEquals(null, second);
            if (ReferenceEquals(null, second)) return true;
            return first.Key != second.Key || first.DisplayText != second.DisplayText;
        }
    }

    /// <summary>
    /// Represents a localized string, which is updated on a change to <see cref="Localizer.Current"/>.
    /// </summary>
    public sealed class LocalizedString : IDisposable, IWeakEventTarget
    {
        /// <summary>
        /// Gets the key for this <see cref="LocalizedString"/>.
        /// </summary>
        public readonly LocalizedStringKey Key;

        /// <summary>
        /// Gets the current localized display text.
        /// </summary>
        public string DisplayText { get; private set; }

        private event Action<string> displayTextChanged;

        /// <summary>
        /// Occurs when the value of <see cref="DisplayText"/> changed.
        /// </summary>
        public event Action<string> DisplayTextChanged
        {
            add
            {
                displayTextChanged += value;
                value(DisplayText);
            }
            remove
            {
                displayTextChanged -= value;
            }
        }

        public LocalizedString(LocalizedStringKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            Key = key;
            DisplayText = Localizer.Current.Localize(key);

            Localizer.CurrentChanged += Localizer_CurrentChanged;
        }

        private void Localizer_CurrentChanged(object sender, EventArgs e)
        {
            string newDisplayText = Localizer.Current.Localize(Key);
            if (DisplayText != newDisplayText)
            {
                DisplayText = newDisplayText;
                displayTextChanged?.Invoke(DisplayText);
            }
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}

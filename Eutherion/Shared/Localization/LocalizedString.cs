#region License
/*********************************************************************************
 * LocalizedString.cs
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

using Eutherion.Utils;
using System;
using System.Diagnostics;

namespace Eutherion.Localization
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
        public readonly string Key;

        /// <summary>
        /// For untranslatable keys, returns the display text.
        /// </summary>
        internal readonly string DisplayText;

        private LocalizedStringKey(string key, string displayText)
        {
            Key = key;
            DisplayText = displayText ?? throw new ArgumentNullException(nameof(displayText));
        }

        /// <summary>
        /// Constructs a new instance of <see cref="LocalizedStringKey"/>.
        /// </summary>
        public LocalizedStringKey(string key)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
        }

        public bool Equals(LocalizedStringKey other) => other != null
                                                     && Key == other.Key
                                                     && DisplayText == other.DisplayText;

        public override bool Equals(object obj) => Equals(obj as LocalizedStringKey);

        public override int GetHashCode() => Key != null ? Key.GetHashCode() : DisplayText.GetHashCode();

        public static bool operator ==(LocalizedStringKey first, LocalizedStringKey second)
        {
            if (first is null) return second is null;
            if (second is null) return false;
            return first.Key == second.Key && first.DisplayText == second.DisplayText;
        }

        public static bool operator !=(LocalizedStringKey first, LocalizedStringKey second)
        {
            if (first is null) return !(second is null);
            if (second is null) return true;
            return first.Key != second.Key || first.DisplayText != second.DisplayText;
        }
    }

    /// <summary>
    /// Represents a localized string, which is updated on a change to <see cref="Localizer.Current"/>.
    /// </summary>
    public sealed class LocalizedString : IDisposable, IWeakEventTarget
    {
        /// <summary>
        /// Conditionally formats a string, based on whether or not it has parameters.
        /// </summary>
        public static string ConditionalFormat(string localizedString, string[] parameters)
        {
            if (parameters == null || parameters.Length == 0) return localizedString;

            try
            {
                return string.Format(localizedString, parameters);
            }
            catch (FormatException)
            {
                // The provided localized format string is in an incorrect format, and/or it contains
                // more parameter substitution locations than there are parameters provided.
                // Examples:
                // string.Format("Test with parameters {invalid parameter}", parameters)
                // string.Format("Test with parameters {0}, {1} and {2}", new string[] { "0", "1" })
                return $"{localizedString} {StringUtilities.ToDefaultParameterListDisplayString(parameters)}";
            }
        }

        /// <summary>
        /// Gets the key for this <see cref="LocalizedString"/>.
        /// </summary>
        public readonly LocalizedStringKey Key;

        /// <summary>
        /// Gets the current localized display text.
        /// </summary>
        public readonly ObservableValue<string> DisplayText = new ObservableValue<string>(StringComparer.Ordinal);

        public LocalizedString(LocalizedStringKey key)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            DisplayText.Value = Localizer.Current.Localize(Key);

            Localizer.CurrentChanged += Localizer_CurrentChanged;
        }

        private void Localizer_CurrentChanged(object sender, EventArgs e)
        {
            DisplayText.Value = Localizer.Current.Localize(Key);
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}

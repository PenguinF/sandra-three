#region License
/*********************************************************************************
 * PTypeErrorBuilder.cs
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

using Eutherion.Localization;

namespace Eutherion.Win.Storage
{
    /// <summary>
    /// Contains information to build an error message caused by a value being of a different type than expected.
    /// </summary>
    public class PTypeErrorBuilder : ITypeErrorBuilder
    {
        /// <summary>
        /// Gets the translation key for when there are no legal values.
        /// </summary>
        public static readonly LocalizedStringKey NoLegalValues = new LocalizedStringKey(nameof(NoLegalValues));

        /// <summary>
        /// Gets the translation key for concatenating a list of values.
        /// </summary>
        public static readonly LocalizedStringKey EnumerateWithOr = new LocalizedStringKey(nameof(EnumerateWithOr));

        /// <summary>
        /// Surrounds a string value with double quote characters.
        /// </summary>
        /// <param name="stringValue">
        /// The string value to surround.
        /// </param>
        /// <returns>
        /// The string value surrounded with double quote characters.
        /// </returns>
        public static string QuoteStringValue(string stringValue) => $"\"{stringValue}\"";

        /// <summary>
        /// Surrounds a value with single quote characters.
        /// </summary>
        /// <param name="value">
        /// The value to surround.
        /// </param>
        /// <returns>
        /// The value surrounded with single quote characters.
        /// </returns>
        public static string QuoteValue(string value) => $"'{value}'";

        /// <summary>
        /// Gets the translation key for this error message.
        /// </summary>
        // This is intentionally not a LocalizedString, because it has a dependency on the Localizer.CurrentChanged event.
        // Instead, GetLocalizedTypeErrorMessage() handles the localization.
        public LocalizedStringKey LocalizedMessageKey { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="PTypeErrorBuilder"/>.
        /// </summary>
        /// <param name="localizedMessageKey">
        /// The translation key for this error message.
        /// </param>
        public PTypeErrorBuilder(LocalizedStringKey localizedMessageKey) => LocalizedMessageKey = localizedMessageKey;

        /// <summary>
        /// Gets the localized, context sensitive message for this error.
        /// </summary>
        /// <param name="localizer">
        /// The localizer to use.
        /// </param>
        /// <param name="propertyKey">
        /// The property key for which the error occurred, or null if there was none.
        /// </param>
        /// <param name="valueString">
        /// A string representation of the value in the source code.
        /// </param>
        /// <returns>
        /// The localized error message.
        /// </returns>
        public string GetLocalizedTypeErrorMessage(Localizer localizer, string propertyKey, string valueString)
            => localizer.Localize(LocalizedMessageKey, new[] { propertyKey, valueString });
    }
}

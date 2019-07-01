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
using Eutherion.Text.Json;
using System;

namespace Eutherion.Win.Storage
{
    /// <summary>
    /// Contains information to build an error message caused by a value being of a different type than expected.
    /// </summary>
    public class PTypeErrorBuilder : ITypeErrorBuilder
    {
        /// <summary>
        /// Gets the translation key for concatenating a list of values.
        /// </summary>
        public static readonly LocalizedStringKey EnumerateWithOr = new LocalizedStringKey(nameof(EnumerateWithOr));

        /// <summary>
        /// Gets the translation key for property keys that are not recognized.
        /// </summary>
        public static readonly LocalizedStringKey UnrecognizedPropertyKeyTypeError = new LocalizedStringKey(nameof(UnrecognizedPropertyKeyTypeError));

        /// <summary>
        /// Gets the translation key for a generic json value type error.
        /// Parameters: 0 = description of expected type, 1 = actual value
        /// Example: "expected an integer value, but found 'false'"
        ///          "expected _______{0}______, but found __{1}__"
        /// </summary>
        public static readonly LocalizedStringKey GenericJsonTypeError = new LocalizedStringKey(nameof(GenericJsonTypeError));

        /// <summary>
        /// Gets the translation key for a generic json value type error.
        /// Parameters: 0 = description of expected type, 1 = location, 2 = actual value
        /// Example: "expected an integer value at index 3, but found 'false'"
        ///          "expected _______{0}______ ____{1}___, but found __{2}__"
        /// </summary>
        public static readonly LocalizedStringKey GenericJsonTypeErrorSomewhere = new LocalizedStringKey(nameof(GenericJsonTypeErrorSomewhere));

        /// <summary>
        /// Gets the translation key for when there are no legal values.
        /// </summary>
        public static readonly LocalizedStringKey NoLegalValues = new LocalizedStringKey(nameof(NoLegalValues));

        /// <summary>
        /// Gets the translation key for <see cref="PType.TupleTypeBase{T}"/> type check failure error messages when one or more tuple elements have the wrong type.
        /// </summary>
        public static readonly LocalizedStringKey TupleItemTypeMismatchError = new LocalizedStringKey(nameof(TupleItemTypeMismatchError));

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
        /// Gets the display string for a property key.
        /// </summary>
        /// <param name="keyNode">
        /// The key node that contains the property key for which the error is generated.
        /// </param>
        /// <param name="json">
        /// The source json on which the <paramref name="keyNode"/> is based.
        /// </param>
        /// <returns>
        /// The display string.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="keyNode"/> and/or <paramref name="json"/> are null.
        /// </exception>
        public static string GetPropertyKeyDisplayString(JsonStringLiteralSyntax keyNode, string json)
        {
            if (keyNode == null) throw new ArgumentNullException(nameof(keyNode));
            if (json == null) throw new ArgumentNullException(nameof(json));

            // Do a Substring rather than keyNode.Value because the property key may contain escaped characters.
            return json.Substring(keyNode.Start, keyNode.Length);
        }

        /// <summary>
        /// Gets the display string for a json value.
        /// </summary>
        /// <param name="valueNode">
        /// The value node.
        /// </param>
        /// <param name="json">
        /// The source json on which the <paramref name="valueNode"/> is based.
        /// </param>
        /// <returns>
        /// The display string.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="valueNode"/> and/or <paramref name="json"/> are null.
        /// </exception>
        public static string GetValueDisplayString(JsonSyntaxNode valueNode, string json)
        {
            if (valueNode == null) throw new ArgumentNullException(nameof(valueNode));
            if (json == null) throw new ArgumentNullException(nameof(json));

            const int maxLength = 30;
            const string ellipsis = "...";
            const int ellipsisLength = 3;

            switch (valueNode)
            {
                case JsonMissingValueSyntax _:
                    // Missing values.
                    return null;
                case JsonStringLiteralSyntax _:
                    // 2 quote characters.
                    if (valueNode.Length <= maxLength)
                    {
                        // QuoteStringValue not necessary, already quoted.
                        return json.Substring(valueNode.Start, valueNode.Length);
                    }
                    else
                    {
                        // Remove quotes, add ellipsis to inner string value, then quote again.
                        // Somehow it looks like placing the ellipsis inside the string quotes makes more sense.
                        return QuoteStringValue(json.Substring(valueNode.Start + 1, maxLength - ellipsisLength - 2) + ellipsis);
                    }
                default:
                    if (valueNode.Length <= maxLength)
                    {
                        return QuoteValue(json.Substring(valueNode.Start, valueNode.Length));
                    }
                    else
                    {
                        return QuoteValue(json.Substring(valueNode.Start, maxLength - ellipsisLength) + ellipsis);
                    }
            }
        }

        /// <summary>
        /// Gets the translation key which describes the type of expected value.
        /// </summary>
        // This is intentionally not a LocalizedString, because it has a dependency on the Localizer.CurrentChanged event.
        // Instead, GetLocalizedTypeErrorMessage() handles the localization.
        public LocalizedStringKey ExpectedTypeDescriptionKey { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="PTypeErrorBuilder"/>.
        /// </summary>
        /// <param name="expectedTypeDescriptionKey">
        /// The translation key which describes the type of expected value.
        /// </param>
        public PTypeErrorBuilder(LocalizedStringKey expectedTypeDescriptionKey)
            => ExpectedTypeDescriptionKey = expectedTypeDescriptionKey;

        /// <summary>
        /// Gets the localized, context sensitive message for this error.
        /// </summary>
        /// <param name="localizer">
        /// The localizer to use.
        /// </param>
        /// <param name="actualValueString">
        /// A string representation of the value in the source code.
        /// </param>
        /// <returns>
        /// The localized error message.
        /// </returns>
        public string GetLocalizedTypeErrorMessage(Localizer localizer, string actualValueString)
            => localizer.Localize(
                GenericJsonTypeError,
                new[]
                {
                    localizer.Localize(ExpectedTypeDescriptionKey),
                    actualValueString,
                });

        /// <summary>
        /// Gets the localized, context sensitive message for this error.
        /// </summary>
        /// <param name="localizer">
        /// The localizer to use.
        /// </param>
        /// <param name="actualValueString">
        /// A string representation of the value in the source code.
        /// </param>
        /// <param name="propertyKey">
        /// The property key for which the error occurred.
        /// </param>
        /// <returns>
        /// The localized error message.
        /// </returns>
        public string GetLocalizedTypeErrorAtPropertyKeyMessage(Localizer localizer, string actualValueString, string propertyKey)
            => localizer.Localize(
                ExpectedTypeDescriptionKey,
                new[]
                {
                    propertyKey,
                    actualValueString,
                });
    }
}

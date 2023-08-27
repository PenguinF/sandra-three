﻿#region License
/*********************************************************************************
 * PTypeErrorBuilder.cs
 *
 * Copyright (c) 2004-2023 Henk Nicolai
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

using Eutherion.Text;
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
        public static readonly StringKey<ForFormattedText> EnumerateWithOr = new StringKey<ForFormattedText>(nameof(EnumerateWithOr));

        /// <summary>
        /// Gets the translation key for duplicate property keys.
        /// </summary>
        public static readonly StringKey<ForFormattedText> DuplicatePropertyKeyWarning = new StringKey<ForFormattedText>(nameof(DuplicatePropertyKeyWarning));

        /// <summary>
        /// Gets the translation key for property keys that are not recognized.
        /// </summary>
        public static readonly StringKey<ForFormattedText> UnrecognizedPropertyKeyWarning = new StringKey<ForFormattedText>(nameof(UnrecognizedPropertyKeyWarning));

        /// <summary>
        /// Gets the translation key for a generic json value type error.
        /// Parameters: 0 = description of expected type, 1 = actual value
        /// Example: "expected an integer value, but found 'false'"
        ///          "expected _______{0}______, but found __{1}__"
        /// See also: <seealso cref="GetLocalizedTypeErrorMessage"/>.
        /// </summary>
        public static readonly StringKey<ForFormattedText> GenericJsonTypeError = new StringKey<ForFormattedText>(nameof(GenericJsonTypeError));

        /// <summary>
        /// Gets the translation key for a generic json value type error.
        /// Parameters: 0 = description of expected type, 1 = actual value, 2 = location
        /// Example: "expected an integer value at index 3, but found 'false'"
        ///          "expected _______{0}______ ____{2}___, but found __{1}__"
        /// The location could be:
        /// - a key error location (KeyErrorLocation)
        /// - an index error location (IndexErrorLocation)
        /// See also: <seealso cref="GetLocalizedTypeErrorSomewhereMessage"/>.
        /// </summary>
        public static readonly StringKey<ForFormattedText> GenericJsonTypeErrorSomewhere = new StringKey<ForFormattedText>(nameof(GenericJsonTypeErrorSomewhere));

        /// <summary>
        /// Gets the translation key for displaying an error in the context of a property key.
        /// </summary>
        public static readonly StringKey<ForFormattedText> KeyErrorLocation = new StringKey<ForFormattedText>(nameof(KeyErrorLocation));

        /// <summary>
        /// Gets the translation key for displaying an error in the context of an item index in an array.
        /// </summary>
        public static readonly StringKey<ForFormattedText> IndexErrorLocation = new StringKey<ForFormattedText>(nameof(IndexErrorLocation));

        /// <summary>
        /// Gets the translation key for when there are no legal values.
        /// </summary>
        public static readonly StringKey<ForFormattedText> NoLegalValuesError = new StringKey<ForFormattedText>(nameof(NoLegalValuesError));

        /// <summary>
        /// Gets the translation key for when there are no legal values.
        /// </summary>
        public static readonly StringKey<ForFormattedText> NoLegalValuesErrorSomewhere = new StringKey<ForFormattedText>(nameof(NoLegalValuesErrorSomewhere));

        /// <summary>
        /// Gets the translation key for <see cref="PType.TupleTypeBase{T}"/> type check failure error messages when one or more tuple elements have the wrong type.
        /// </summary>
        public static readonly StringKey<ForFormattedText> TupleItemTypeMismatchError = new StringKey<ForFormattedText>(nameof(TupleItemTypeMismatchError));

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
        /// <returns>
        /// The display string.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="keyNode"/> is <see langword="null"/>.
        /// </exception>
        public static string GetPropertyKeyDisplayString(JsonStringLiteralSyntax keyNode)
        {
            if (keyNode == null) throw new ArgumentNullException(nameof(keyNode));

            // Do a Substring rather than keyNode.Value because the property key may contain escaped characters.
            return keyNode.Root.Json.Substring(keyNode.AbsoluteStart, keyNode.Length);
        }

        private class ValueDisplayStringRenderer : JsonValueSyntaxVisitor<string>
        {
            const int maxLength = 31;
            const string ellipsis = "...";
            const int ellipsisLength = 3;
            const int halfLength = (maxLength - ellipsisLength) / 2;

            public static ValueDisplayStringRenderer Instance { get; } = new ValueDisplayStringRenderer();

            private ValueDisplayStringRenderer() { }

            public override string VisitMissingValueSyntax(JsonMissingValueSyntax valueNode, _void _)
            {
                // Missing values.
                return null;
            }

            public override string VisitStringLiteralSyntax(JsonStringLiteralSyntax valueNode, _void _)
            {
                int valueNodeStart = valueNode.AbsoluteStart;
                string json = valueNode.Root.Json;

                // 2 quote characters.
                if (valueNode.Length <= maxLength)
                {
                    // QuoteStringValue not necessary, already quoted.
                    return json.Substring(valueNodeStart, valueNode.Length);
                }
                else
                {
                    // Remove quotes, add ellipsis to inner string value, then quote again.
                    return QuoteStringValue(
                        json.Substring(valueNodeStart + 1, halfLength - 1)
                        + ellipsis
                        + json.Substring(valueNodeStart + valueNode.Length - halfLength + 1, halfLength - 1));
                }
            }

            public override string DefaultVisit(JsonValueSyntax valueNode, _void _)
            {
                int valueNodeStart = valueNode.AbsoluteStart;
                string json = valueNode.Root.Json;

                if (valueNode.Length <= maxLength)
                {
                    return QuoteValue(json.Substring(valueNodeStart, valueNode.Length));
                }
                else
                {
                    return QuoteValue(
                        json.Substring(valueNodeStart, halfLength)
                        + ellipsis
                        + json.Substring(valueNodeStart + valueNode.Length - halfLength, halfLength));
                }
            }
        }

        /// <summary>
        /// Gets the display string for a json value.
        /// </summary>
        /// <param name="valueNode">
        /// The value node.
        /// </param>
        /// <returns>
        /// The display string.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="valueNode"/> is <see langword="null"/>.
        /// </exception>
        public static string GetValueDisplayString(JsonValueSyntax valueNode)
        {
            if (valueNode == null) throw new ArgumentNullException(nameof(valueNode));

            return ValueDisplayStringRenderer.Instance.Visit(valueNode);
        }

        /// <summary>
        /// Gets the localized error message for a generic json value type error. 
        /// </summary>
        /// <param name="localizer">
        /// The localizer to use.
        /// </param>
        /// <param name="localizedExpectedTypeDescription">
        /// A localized description of the type of value that is expected.
        /// </param>
        /// <param name="actualValueString">
        /// A string representation of the value in the source code.
        /// </param>
        /// <returns>
        /// The localized error message.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="localizer"/> is <see langword="null"/>.
        /// </exception>
        public static string GetLocalizedTypeErrorMessage(
            TextFormatter localizer,
            string localizedExpectedTypeDescription,
            string actualValueString)
            => (localizer ?? throw new ArgumentNullException(nameof(localizer))).Format(
                GenericJsonTypeError,
                localizedExpectedTypeDescription,
                actualValueString);

        /// <summary>
        /// Gets the localized description of an error for a value located at a property key.
        /// </summary>
        /// <param name="localizer">
        /// The localizer to use.
        /// </param>
        /// <param name="propertyKey">
        /// The property key for which the error occurred.
        /// </param>
        /// <returns>
        /// The localized description of the location of the error.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="localizer"/> is <see langword="null"/>.
        /// </exception>
        public static string GetLocatedAtPropertyKeyMessage(TextFormatter localizer, string propertyKey)
            => (localizer ?? throw new ArgumentNullException(nameof(localizer))).Format(KeyErrorLocation, propertyKey);

        /// <summary>
        /// Gets the localized description of an error for a value which is an element of an array.
        /// </summary>
        /// <param name="localizer">
        /// The localizer to use.
        /// </param>
        /// <param name="itemIndex">
        /// The index of the array where the error occurred.
        /// </param>
        /// <returns>
        /// The localized description of the location of the error.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="localizer"/> is <see langword="null"/>.
        /// </exception>
        public static string GetLocatedAtItemIndexMessage(TextFormatter localizer, int itemIndex)
            => (localizer ?? throw new ArgumentNullException(nameof(localizer))).Format(IndexErrorLocation, itemIndex.ToStringInvariant());

        /// <summary>
        /// Gets the localized error message for a generic json value type error. 
        /// </summary>
        /// <param name="localizer">
        /// The localizer to use.
        /// </param>
        /// <param name="localizedExpectedTypeDescription">
        /// A localized description of the type of value that is expected.
        /// </param>
        /// <param name="actualValueString">
        /// A string representation of the value in the source code.
        /// </param>
        /// <param name="somewhere">
        /// The location where the error occurred.
        /// </param>
        /// <returns>
        /// The localized error message.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="localizer"/> is <see langword="null"/>.
        /// </exception>
        public static string GetLocalizedTypeErrorSomewhereMessage(
            TextFormatter localizer,
            string localizedExpectedTypeDescription,
            string actualValueString,
            string somewhere)
            => (localizer ?? throw new ArgumentNullException(nameof(localizer))).Format(
                GenericJsonTypeErrorSomewhere,
                localizedExpectedTypeDescription,
                actualValueString,
                somewhere);

        /// <summary>
        /// Gets the translation key which describes the type of expected value.
        /// </summary>
        // This is intentionally not a LocalizedString, because it has a dependency on the Localizer.CurrentChanged event.
        // Instead, GetLocalizedTypeErrorMessage() handles the localization.
        public StringKey<ForFormattedText> ExpectedTypeDescriptionKey { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="PTypeErrorBuilder"/>.
        /// </summary>
        /// <param name="expectedTypeDescriptionKey">
        /// The translation key which describes the type of expected value.
        /// </param>
        public PTypeErrorBuilder(StringKey<ForFormattedText> expectedTypeDescriptionKey)
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
        /// <exception cref="ArgumentNullException">
        /// <paramref name="localizer"/> is <see langword="null"/>.
        /// </exception>
        public string GetLocalizedTypeErrorMessage(TextFormatter localizer, string actualValueString)
            => GetLocalizedTypeErrorMessage(
                localizer,
                (localizer ?? throw new ArgumentNullException(nameof(localizer))).Format(ExpectedTypeDescriptionKey),
                actualValueString);

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
        /// <exception cref="ArgumentNullException">
        /// <paramref name="localizer"/> is <see langword="null"/>.
        /// </exception>
        public string GetLocalizedTypeErrorAtPropertyKeyMessage(TextFormatter localizer, string actualValueString, string propertyKey)
            => GetLocalizedTypeErrorSomewhereMessage(
                localizer,
                (localizer ?? throw new ArgumentNullException(nameof(localizer))).Format(ExpectedTypeDescriptionKey),
                actualValueString,
                GetLocatedAtPropertyKeyMessage(localizer, propertyKey));

        /// <summary>
        /// Gets the localized, context sensitive message for this error.
        /// </summary>
        /// <param name="localizer">
        /// The localizer to use.
        /// </param>
        /// <param name="actualValueString">
        /// A string representation of the value in the source code.
        /// </param>
        /// <param name="itemIndex">
        /// The index of the array where the error occurred.
        /// </param>
        /// <returns>
        /// The localized error message.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="localizer"/> is <see langword="null"/>.
        /// </exception>
        public string GetLocalizedTypeErrorAtItemIndexMessage(TextFormatter localizer, string actualValueString, int itemIndex)
            => GetLocalizedTypeErrorSomewhereMessage(
                localizer,
                (localizer ?? throw new ArgumentNullException(nameof(localizer))).Format(ExpectedTypeDescriptionKey),
                actualValueString,
                GetLocatedAtItemIndexMessage(localizer, itemIndex));
    }
}

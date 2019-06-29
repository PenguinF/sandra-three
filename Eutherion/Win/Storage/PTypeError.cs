#region License
/*********************************************************************************
 * PTypeError.cs
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
    /// Represents a semantic type error caused by a failed typecheck in any of the <see cref="PType{T}"/> subclasses.
    /// </summary>
    public abstract class PTypeError : JsonErrorInfo
    {
        public PTypeError(int start, int length)
            : base(JsonErrorCode.Custom, start, length)
        {
        }

        /// <summary>
        /// Gets the localized, context sensitive message for this error.
        /// </summary>
        /// <param name="localizer">
        /// The localizer to use.
        /// </param>
        /// <returns>
        /// The localized error message.
        /// </returns>
        public abstract string GetLocalizedMessage(Localizer localizer);
    }

    /// <summary>
    /// Represents an error caused by an unknown property key within a schema.
    /// </summary>
    public class UnrecognizedPropertyKeyTypeError : PTypeError
    {
        /// <summary>
        /// Gets the property key for which this error occurred.
        /// </summary>
        public string PropertyKey { get; }

        /// <summary>
        /// Gets the localized, context sensitive message for this error.
        /// </summary>
        /// <param name="localizer">
        /// The localizer to use.
        /// </param>
        /// <returns>
        /// The localized error message.
        /// </returns>
        public override string GetLocalizedMessage(Localizer localizer)
            => localizer.Localize(
                PTypeErrorBuilder.UnrecognizedPropertyKeyTypeError,
                new[] { PropertyKey });

        private UnrecognizedPropertyKeyTypeError(string propertyKey, int start, int length)
            : base(start, length)
        {
            PropertyKey = propertyKey;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UnrecognizedPropertyKeyTypeError"/>.
        /// </summary>
        /// <param name="keyNode">
        /// The property key for which the error is generated.
        /// </param>
        /// <param name="json">
        /// The source json which contains the type error.
        /// </param>
        /// <returns>
        /// A <see cref="UnrecognizedPropertyKeyTypeError"/> instance which generates a localized error message.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="keyNode"/> and/or <paramref name="json"/> are null.
        /// </exception>
        public static UnrecognizedPropertyKeyTypeError Create(JsonStringLiteralSyntax keyNode, string json)
            => new UnrecognizedPropertyKeyTypeError(
                PTypeErrorBuilder.GetPropertyKeyDisplayString(keyNode, json),
                keyNode.Start,
                keyNode.Length);
    }

    /// <summary>
    /// Represents an error caused by a value being of a different type than expected.
    /// </summary>
    public class ValueTypeErrorAtPropertyKey : PTypeError
    {
        /// <summary>
        /// Gets the context insensitive information for this error message.
        /// </summary>
        public ITypeErrorBuilder TypeErrorBuilder { get; }

        /// <summary>
        /// Gets the property key for which this error occurred, or null if there was none.
        /// </summary>
        public string PropertyKey { get; }

        /// <summary>
        /// Gets the string representation of the value for which this error occurred.
        /// </summary>
        public string ValueString { get; }

        /// <summary>
        /// Gets the localized, context sensitive message for this error.
        /// </summary>
        /// <param name="localizer">
        /// The localizer to use.
        /// </param>
        /// <returns>
        /// The localized error message.
        /// </returns>
        public override string GetLocalizedMessage(Localizer localizer)
            => TypeErrorBuilder.GetLocalizedTypeErrorMessage(
                localizer,
                PropertyKey,
                ValueString ?? localizer.Localize(PType.JsonUndefinedValue));

        private ValueTypeErrorAtPropertyKey(ITypeErrorBuilder typeErrorBuilder, string propertyKey, string valueString, int start, int length)
            : base(start, length)
        {
            TypeErrorBuilder = typeErrorBuilder;
            PropertyKey = propertyKey;
            ValueString = valueString;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ValueTypeErrorAtPropertyKey"/>.
        /// </summary>
        /// <param name="typeErrorBuilder">
        /// The context insensitive information for this error message.
        /// </param>
        /// <param name="keyNode">
        /// The property key for which the error is generated.
        /// </param>
        /// <param name="valueNode">
        /// The value node corresponding to the value that was typechecked.
        /// </param>
        /// <param name="json">
        /// The source json which contains the type error.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTypeErrorAtPropertyKey"/> instance which generates a localized error message.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="typeErrorBuilder"/> and/or <paramref name="keyNode"/> and/or <paramref name="valueNode"/> and/or <paramref name="json"/> are null.
        /// </exception>
        public static ValueTypeErrorAtPropertyKey Create(ITypeErrorBuilder typeErrorBuilder, JsonStringLiteralSyntax keyNode, JsonSyntaxNode valueNode, string json)
        {
            if (typeErrorBuilder == null) throw new ArgumentNullException(nameof(typeErrorBuilder));
            if (valueNode == null) throw new ArgumentNullException(nameof(valueNode));

            const int maxLength = 30;
            const string ellipsis = "...";
            const int ellipsisLength = 3;

            string valueString;
            if (valueNode is JsonMissingValueSyntax)
            {
                // Missing values.
                valueString = null;
            }
            else if (valueNode.Length <= maxLength)
            {
                valueString = json.Substring(valueNode.Start, valueNode.Length);

                if (!(valueNode is JsonStringLiteralSyntax))
                {
                    valueString = PTypeErrorBuilder.QuoteValue(valueString);
                }
            }
            else
            {
                valueString = PTypeErrorBuilder.QuoteValue(json.Substring(valueNode.Start, maxLength - ellipsisLength) + ellipsis);
            }

            return new ValueTypeErrorAtPropertyKey(
                typeErrorBuilder,
                PTypeErrorBuilder.GetPropertyKeyDisplayString(keyNode, json),
                valueString,
                valueNode.Start,
                valueNode.Length);
        }
    }
}

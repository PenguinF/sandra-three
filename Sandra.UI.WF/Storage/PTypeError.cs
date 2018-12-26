#region License
/*********************************************************************************
 * PTypeError.cs
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
#endregion

using SysExtensions.Text.Json;
using System;

namespace Sandra.UI.WF.Storage
{
    /// <summary>
    /// Represents an error caused by a value being of a different type than expected.
    /// </summary>
    public class PTypeError : JsonErrorInfo
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
        public string GetLocalizedMessage(Localizer localizer) => TypeErrorBuilder.GetLocalizedTypeErrorMessage(
            localizer,
            PropertyKey,
            ValueString ?? localizer.Localize(PType.JsonUndefinedValue));

        private PTypeError(ITypeErrorBuilder typeErrorBuilder, string propertyKey, string valueString, int start, int length)
            : base(JsonErrorCode.Custom, start, length)
        {
            TypeErrorBuilder = typeErrorBuilder;
            PropertyKey = propertyKey;
            ValueString = valueString;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PTypeError"/>.
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
        /// A <see cref="PTypeError"/> instance which generates a localized error message.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="typeErrorBuilder"/> is null.
        /// </exception>
        public static PTypeError Create(ITypeErrorBuilder typeErrorBuilder, JsonStringLiteralSyntax keyNode, JsonSyntaxNode valueNode, string json)
        {
            if (typeErrorBuilder == null) throw new ArgumentNullException(nameof(typeErrorBuilder));

            string valueString;
            if (valueNode is JsonMissingValueSyntax)
            {
                // Missing values.
                valueString = null;
            }
            else
            {
                valueString = json.Substring(valueNode.Start, valueNode.Length);

                if (!(valueNode is JsonStringLiteralSyntax))
                {
                    valueString = PTypeErrorBuilder.QuoteValue(valueString);
                }
            }

            return new PTypeError(
                typeErrorBuilder,
                // Do a Substring because the property key may contain escaped characters.
                keyNode == null ? null : json.Substring(keyNode.Start, keyNode.Length),
                valueString,
                valueNode.Start,
                valueNode.Length);
        }
    }
}

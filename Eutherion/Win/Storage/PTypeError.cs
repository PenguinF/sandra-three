﻿#region License
/*********************************************************************************
 * PTypeError.cs
 *
 * Copyright (c) 2004-2020 Henk Nicolai
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

        public PTypeError(int start, int length, JsonErrorLevel errorLevel)
            : base(JsonErrorCode.Custom, errorLevel, start, length)
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
            : base(start, length, JsonErrorLevel.Warning)
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
        /// <param name="keyNodeStart">
        /// The start position of the key node in the source json.
        /// </param>
        /// <returns>
        /// A <see cref="UnrecognizedPropertyKeyTypeError"/> instance which generates a localized error message.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="keyNode"/> and/or <paramref name="json"/> are null.
        /// </exception>
        public static UnrecognizedPropertyKeyTypeError Create(GreenJsonStringLiteralSyntax keyNode, string json, int keyNodeStart)
            => new UnrecognizedPropertyKeyTypeError(
                PTypeErrorBuilder.GetPropertyKeyDisplayString(keyNode, json, keyNodeStart),
                keyNodeStart,
                keyNode.Length);
    }

    /// <summary>
    /// Represents an error caused by a value being of a different type than expected.
    /// </summary>
    public class ValueTypeError : PTypeError
    {
        /// <summary>
        /// Gets the context insensitive information for this error message.
        /// </summary>
        public ITypeErrorBuilder TypeErrorBuilder { get; }

        /// <summary>
        /// Gets the string representation of the value for which this error occurred.
        /// </summary>
        public string ActualValueString { get; }

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
                ActualValueString ?? localizer.Localize(PType.JsonUndefinedValue));

        internal ValueTypeError(ITypeErrorBuilder typeErrorBuilder, string actualValueString, int start, int length)
            : base(start, length)
        {
            TypeErrorBuilder = typeErrorBuilder;
            ActualValueString = actualValueString;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ValueTypeError"/>.
        /// </summary>
        /// <param name="typeErrorBuilder">
        /// The context insensitive information for this error message.
        /// </param>
        /// <param name="valueNode">
        /// The value node corresponding to the value that was typechecked.
        /// </param>
        /// <param name="json">
        /// The source json which contains the type error.
        /// </param>
        /// <param name="valueNodeStart">
        /// The start position of the value node in the source json.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTypeError"/> instance which generates a localized error message.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="typeErrorBuilder"/> and/or <paramref name="valueNode"/> and/or <paramref name="json"/> are null.
        /// </exception>
        public static ValueTypeError Create(ITypeErrorBuilder typeErrorBuilder, GreenJsonValueSyntax valueNode, string json, int valueNodeStart)
        {
            if (typeErrorBuilder == null) throw new ArgumentNullException(nameof(typeErrorBuilder));

            return new ValueTypeError(
                typeErrorBuilder,
                PTypeErrorBuilder.GetValueDisplayString(valueNode, json, valueNodeStart),
                valueNodeStart,
                valueNode.Length);
        }
    }

    /// <summary>
    /// Represents an error caused by a value at a property key being of a different type than expected.
    /// </summary>
    public class ValueTypeErrorAtPropertyKey : ValueTypeError
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
            => TypeErrorBuilder.GetLocalizedTypeErrorAtPropertyKeyMessage(
                localizer,
                ActualValueString ?? localizer.Localize(PType.JsonUndefinedValue),
                PropertyKey);

        private ValueTypeErrorAtPropertyKey(
            ITypeErrorBuilder typeErrorBuilder,
            string propertyKey,
            string actualValueString,
            int start,
            int length)
            : base(typeErrorBuilder, actualValueString, start, length)
        {
            PropertyKey = propertyKey;
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
        /// <param name="keyNodeStart">
        /// The start position of the key node in the source json.
        /// </param>
        /// <param name="valueNodeStart">
        /// The start position of the value node in the source json.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTypeErrorAtPropertyKey"/> instance which generates a localized error message.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="typeErrorBuilder"/> and/or <paramref name="keyNode"/> and/or <paramref name="valueNode"/> and/or <paramref name="json"/> are null.
        /// </exception>
        public static ValueTypeErrorAtPropertyKey Create(
            ITypeErrorBuilder typeErrorBuilder,
            GreenJsonStringLiteralSyntax keyNode,
            GreenJsonValueSyntax valueNode,
            string json,
            int keyNodeStart,
            int valueNodeStart)
        {
            if (typeErrorBuilder == null) throw new ArgumentNullException(nameof(typeErrorBuilder));

            return new ValueTypeErrorAtPropertyKey(
                typeErrorBuilder,
                PTypeErrorBuilder.GetPropertyKeyDisplayString(keyNode, json, keyNodeStart),
                PTypeErrorBuilder.GetValueDisplayString(valueNode, json, valueNodeStart),
                valueNodeStart,
                valueNode.Length);
        }
    }
}

﻿#region License
/*********************************************************************************
 * PTypeError.cs
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
using Eutherion.Threading;
using System;

namespace Eutherion.Win.Storage
{
    /// <summary>
    /// Represents a semantic type error caused by a failed typecheck in any of the <see cref="PType{T}"/> subclasses.
    /// </summary>
    public abstract class PTypeError : ISpan
    {
        /// <summary>
        /// Gets the start position of the text span where the error occurred.
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// Gets the length of the text span where the error occurred.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="PTypeError"/>.
        /// </summary>
        /// <param name="start">
        /// The start position of the text span where the type error occurred.
        /// </param>
        /// <param name="length">
        /// The length of the text span where the type error occurred.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Either <paramref name="start"/> or <paramref name="length"/>, or both are negative.
        /// </exception>
        public PTypeError(int start, int length)
        {
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));

            Start = start;
            Length = length;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PTypeError"/>.
        /// </summary>
        /// <param name="syntaxNode">
        /// The syntax node where the type error occurred.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Either <paramref name="syntaxNode"/> is <see langword="null"/>.
        /// </exception>
        public PTypeError(JsonSyntax syntaxNode)
        {
            if (syntaxNode == null) throw new ArgumentNullException(nameof(syntaxNode));

            Start = syntaxNode.AbsoluteStart;
            Length = syntaxNode.Length;
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
        public abstract string GetLocalizedMessage(TextFormatter localizer);
    }

    /// <summary>
    /// Represents a warning caused by a duplicate property key within a JSON object.
    /// </summary>
    public class DuplicatePropertyKeyWarning : PTypeError
    {
        /// <summary>
        /// Gets the property key for which this error occurred.
        /// </summary>
        public JsonStringLiteralSyntax KeyNode { get; }

        private readonly SafeLazy<string> PropertyKeyDisplayString;

        /// <summary>
        /// Gets the localized, context sensitive message for this error.
        /// </summary>
        /// <param name="localizer">
        /// The localizer to use.
        /// </param>
        /// <returns>
        /// The localized error message.
        /// </returns>
        public override string GetLocalizedMessage(TextFormatter localizer)
            => localizer.Format(
                PTypeErrorBuilder.DuplicatePropertyKeyWarning,
                PropertyKeyDisplayString.Value);

        /// <summary>
        /// Initializes a new instance of <see cref="DuplicatePropertyKeyWarning"/>.
        /// </summary>
        /// <param name="keyNode">
        /// The property key for which the error is generated.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="keyNode"/> is <see langword="null"/>.
        /// </exception>
        public DuplicatePropertyKeyWarning(JsonStringLiteralSyntax keyNode)
            : base(keyNode)
        {
            KeyNode = keyNode;
            PropertyKeyDisplayString = new SafeLazy<string>(() => PTypeErrorBuilder.GetPropertyKeyDisplayString(KeyNode));
        }
    }

    /// <summary>
    /// Represents a warning caused by an unknown property key within a schema.
    /// </summary>
    public class UnrecognizedPropertyKeyWarning : PTypeError
    {
        /// <summary>
        /// Gets the property key syntax node for which this error occurred.
        /// </summary>
        public JsonStringLiteralSyntax KeyNode { get; }

        private readonly SafeLazy<string> PropertyKeyDisplayString;

        /// <summary>
        /// Gets the localized, context sensitive message for this error.
        /// </summary>
        /// <param name="localizer">
        /// The localizer to use.
        /// </param>
        /// <returns>
        /// The localized error message.
        /// </returns>
        public override string GetLocalizedMessage(TextFormatter localizer)
            => localizer.Format(
                PTypeErrorBuilder.UnrecognizedPropertyKeyWarning,
                PropertyKeyDisplayString.Value);

        /// <summary>
        /// Initializes a new instance of <see cref="UnrecognizedPropertyKeyWarning"/>.
        /// </summary>
        /// <param name="keyNode">
        /// The property key for which the error is generated.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="keyNode"/> is <see langword="null"/>.
        /// </exception>
        public UnrecognizedPropertyKeyWarning(JsonStringLiteralSyntax keyNode)
            : base(keyNode)
        {
            KeyNode = keyNode;
            PropertyKeyDisplayString = new SafeLazy<string>(() => PTypeErrorBuilder.GetPropertyKeyDisplayString(KeyNode));
        }
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
        public override string GetLocalizedMessage(TextFormatter localizer)
            => TypeErrorBuilder.GetLocalizedTypeErrorMessage(
                localizer,
                ActualValueString ?? localizer.Format(PType.JsonUndefinedValue));

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
        /// <returns>
        /// A <see cref="ValueTypeError"/> instance which generates a localized error message.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="typeErrorBuilder"/> and/or <paramref name="valueNode"/> are <see langword="null"/>.
        /// </exception>
        public static ValueTypeError Create(ITypeErrorBuilder typeErrorBuilder, JsonValueSyntax valueNode)
        {
            if (typeErrorBuilder == null) throw new ArgumentNullException(nameof(typeErrorBuilder));

            return new ValueTypeError(
                typeErrorBuilder,
                PTypeErrorBuilder.GetValueDisplayString(valueNode),
                valueNode.AbsoluteStart,
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
        public override string GetLocalizedMessage(TextFormatter localizer)
            => TypeErrorBuilder.GetLocalizedTypeErrorAtPropertyKeyMessage(
                localizer,
                ActualValueString ?? localizer.Format(PType.JsonUndefinedValue),
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
        /// <returns>
        /// A <see cref="ValueTypeErrorAtPropertyKey"/> instance which generates a localized error message.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="typeErrorBuilder"/> and/or <paramref name="keyNode"/> and/or <paramref name="valueNode"/> are <see langword="null"/>.
        /// </exception>
        public static ValueTypeErrorAtPropertyKey Create(
            ITypeErrorBuilder typeErrorBuilder,
            JsonStringLiteralSyntax keyNode,
            JsonValueSyntax valueNode)
        {
            if (typeErrorBuilder == null) throw new ArgumentNullException(nameof(typeErrorBuilder));

            return new ValueTypeErrorAtPropertyKey(
                typeErrorBuilder,
                PTypeErrorBuilder.GetPropertyKeyDisplayString(keyNode),
                PTypeErrorBuilder.GetValueDisplayString(valueNode),
                valueNode.AbsoluteStart,
                valueNode.Length);
        }
    }

    /// <summary>
    /// Represents an error caused by an array value being of a different type than expected.
    /// </summary>
    public class ValueTypeErrorAtItemIndex : ValueTypeError
    {
        /// <summary>
        /// Gets the index of the array where the error occurred.
        /// </summary>
        public int ItemIndex { get; }

        /// <summary>
        /// Gets the localized, context sensitive message for this error.
        /// </summary>
        /// <param name="localizer">
        /// The localizer to use.
        /// </param>
        /// <returns>
        /// The localized error message.
        /// </returns>
        public override string GetLocalizedMessage(TextFormatter localizer)
            => TypeErrorBuilder.GetLocalizedTypeErrorAtItemIndexMessage(
                localizer,
                ActualValueString ?? localizer.Format(PType.JsonUndefinedValue),
                ItemIndex);

        private ValueTypeErrorAtItemIndex(
            ITypeErrorBuilder typeErrorBuilder,
            int itemIndex,
            string actualValueString,
            int start,
            int length)
            : base(typeErrorBuilder, actualValueString, start, length)
        {
            ItemIndex = itemIndex;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ValueTypeErrorAtItemIndex"/>.
        /// </summary>
        /// <param name="typeErrorBuilder">
        /// The context insensitive information for this error message.
        /// </param>
        /// <param name="itemIndex">
        /// The index of the array where the error occurred.
        /// </param>
        /// <param name="valueNode">
        /// The value node corresponding to the value that was typechecked.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTypeErrorAtItemIndex"/> instance which generates a localized error message.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="typeErrorBuilder"/> and/or <paramref name="valueNode"/> are <see langword="null"/>.
        /// </exception>
        public static ValueTypeErrorAtItemIndex Create(
            ITypeErrorBuilder typeErrorBuilder,
            int itemIndex,
            JsonValueSyntax valueNode)
        {
            if (typeErrorBuilder == null) throw new ArgumentNullException(nameof(typeErrorBuilder));

            return new ValueTypeErrorAtItemIndex(
                typeErrorBuilder,
                itemIndex,
                PTypeErrorBuilder.GetValueDisplayString(valueNode),
                valueNode.AbsoluteStart,
                valueNode.Length);
        }
    }
}

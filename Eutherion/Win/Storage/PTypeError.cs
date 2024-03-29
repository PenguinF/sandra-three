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
        /// Gets the syntax node where the error occurred.
        /// </summary>
        public JsonSyntax SyntaxNode { get; }

        /// <summary>
        /// Gets the start position of the text span where the error occurred.
        /// </summary>
        public int Start => SyntaxNode.AbsoluteStart;

        /// <summary>
        /// Gets the length of the text span where the error occurred.
        /// </summary>
        public int Length => SyntaxNode.Length;

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
            => SyntaxNode = syntaxNode ?? throw new ArgumentNullException(nameof(syntaxNode));

        /// <summary>
        /// Gets the formatted, context sensitive message for this error.
        /// </summary>
        /// <param name="formatter">
        /// The formatter to use.
        /// </param>
        /// <returns>
        /// The formatted error message.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="formatter"/> is <see langword="null"/>.
        /// </exception>
        public abstract string FormatMessage(TextFormatter formatter);
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
        /// Gets the formatted, context sensitive message for this error.
        /// </summary>
        /// <param name="formatter">
        /// The formatter to use.
        /// </param>
        /// <returns>
        /// The formatted error message.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="formatter"/> is <see langword="null"/>.
        /// </exception>
        public override string FormatMessage(TextFormatter formatter)
            => (formatter ?? throw new ArgumentNullException(nameof(formatter))).Format(
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
        /// Gets the formatted, context sensitive message for this error.
        /// </summary>
        /// <param name="formatter">
        /// The formatter to use.
        /// </param>
        /// <returns>
        /// The formatted error message.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="formatter"/> is <see langword="null"/>.
        /// </exception>
        public override string FormatMessage(TextFormatter formatter)
            => (formatter ?? throw new ArgumentNullException(nameof(formatter))).Format(
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
        /// Gets the value syntax node for which this error occurred.
        /// </summary>
        public JsonValueSyntax ValueNode { get; }

        private readonly SafeLazy<string> ActualValueString;

        /// <summary>
        /// Gets the string representation of the value for which this error occurred.
        /// </summary>
        /// <param name="formatter">
        /// The formatter to use.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="formatter"/> is <see langword="null"/>.
        /// </exception>
        public string GenerateValueString(TextFormatter formatter)
            => ActualValueString.Value
            ?? (formatter ?? throw new ArgumentNullException(nameof(formatter))).Format(PType.JsonUndefinedValue);

        /// <summary>
        /// Gets the formatted, context sensitive message for this error.
        /// </summary>
        /// <param name="formatter">
        /// The formatter to use.
        /// </param>
        /// <returns>
        /// The formatted error message.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="formatter"/> is <see langword="null"/>.
        /// </exception>
        public override string FormatMessage(TextFormatter formatter)
            => TypeErrorBuilder.FormatTypeErrorMessage(
                formatter,
                GenerateValueString(formatter));

        /// <summary>
        /// Initializes a new instance of <see cref="ValueTypeError"/>.
        /// </summary>
        /// <param name="typeErrorBuilder">
        /// The context insensitive information for this error message.
        /// </param>
        /// <param name="valueNode">
        /// The value node corresponding to the value that was typechecked.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="typeErrorBuilder"/> and/or <paramref name="valueNode"/> are <see langword="null"/>.
        /// </exception>
        public ValueTypeError(ITypeErrorBuilder typeErrorBuilder, JsonValueSyntax valueNode)
            : base(valueNode)
        {
            TypeErrorBuilder = typeErrorBuilder ?? throw new ArgumentNullException(nameof(typeErrorBuilder));
            ValueNode = valueNode;
            ActualValueString = new SafeLazy<string>(() => PTypeErrorBuilder.GetValueDisplayString(ValueNode));
        }
    }

    /// <summary>
    /// Represents an error caused by a value at a property key being of a different type than expected.
    /// </summary>
    public class ValueTypeErrorAtPropertyKey : ValueTypeError
    {
        /// <summary>
        /// Gets the property key syntax node for which this error occurred.
        /// </summary>
        public JsonStringLiteralSyntax KeyNode { get; }

        private readonly SafeLazy<string> PropertyKeyDisplayString;

        /// <summary>
        /// Gets the formatted, context sensitive message for this error.
        /// </summary>
        /// <param name="formatter">
        /// The formatter to use.
        /// </param>
        /// <returns>
        /// The formatted error message.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="formatter"/> is <see langword="null"/>.
        /// </exception>
        public override string FormatMessage(TextFormatter formatter)
            => TypeErrorBuilder.FormatTypeErrorAtPropertyKeyMessage(
                formatter,
                GenerateValueString(formatter),
                PropertyKeyDisplayString.Value);

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
        /// <exception cref="ArgumentNullException">
        /// <paramref name="typeErrorBuilder"/> and/or <paramref name="keyNode"/> and/or <paramref name="valueNode"/> are <see langword="null"/>.
        /// </exception>
        public ValueTypeErrorAtPropertyKey(ITypeErrorBuilder typeErrorBuilder, JsonStringLiteralSyntax keyNode, JsonValueSyntax valueNode)
            : base(typeErrorBuilder, valueNode)
        {
            KeyNode = keyNode ?? throw new ArgumentNullException(nameof(keyNode));
            PropertyKeyDisplayString = new SafeLazy<string>(() => PTypeErrorBuilder.GetPropertyKeyDisplayString(KeyNode));
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
        /// Gets the formatted, context sensitive message for this error.
        /// </summary>
        /// <param name="formatter">
        /// The formatter to use.
        /// </param>
        /// <returns>
        /// The formatted error message.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="formatter"/> is <see langword="null"/>.
        /// </exception>
        public override string FormatMessage(TextFormatter formatter)
            => TypeErrorBuilder.FormatTypeErrorAtItemIndexMessage(
                formatter,
                GenerateValueString(formatter),
                ItemIndex);

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
        /// <exception cref="ArgumentNullException">
        /// <paramref name="typeErrorBuilder"/> and/or <paramref name="valueNode"/> are <see langword="null"/>.
        /// </exception>
        public ValueTypeErrorAtItemIndex(ITypeErrorBuilder typeErrorBuilder, int itemIndex, JsonValueSyntax valueNode)
            : base(typeErrorBuilder, valueNode)
        {
            ItemIndex = itemIndex;
        }
    }
}

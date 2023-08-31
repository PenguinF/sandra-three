#region License
/*********************************************************************************
 * PType.Base.cs
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
using System.Collections.Generic;
using System.Numerics;

namespace Eutherion.Win.Storage
{
    public static partial class PType
    {
        /// <summary>
        /// Gets the translation key for referring to a json boolean.
        /// </summary>
        public static readonly StringKey<ForFormattedText> JsonBoolean = new StringKey<ForFormattedText>(nameof(JsonBoolean));

        /// <summary>
        /// Gets the translation key for referring to a json integer.
        /// </summary>
        public static readonly StringKey<ForFormattedText> JsonInteger = new StringKey<ForFormattedText>(nameof(JsonInteger));

        /// <summary>
        /// Gets the translation key for referring to a json integer within a specific range.
        /// </summary>
        public static readonly StringKey<ForFormattedText> RangedJsonInteger = new StringKey<ForFormattedText>(nameof(RangedJsonInteger));

        /// <summary>
        /// Gets the translation key for referring to a json string.
        /// </summary>
        public static readonly StringKey<ForFormattedText> JsonString = new StringKey<ForFormattedText>(nameof(JsonString));

        /// <summary>
        /// Gets the translation key for referring to a general json array (list).
        /// </summary>
        public static readonly StringKey<ForFormattedText> JsonArray = new StringKey<ForFormattedText>(nameof(JsonArray));

        /// <summary>
        /// Gets the translation key for referring to a general json object (map).
        /// </summary>
        public static readonly StringKey<ForFormattedText> JsonObject = new StringKey<ForFormattedText>(nameof(JsonObject));

        /// <summary>
        /// Gets the translation key for referring to an undefined value.
        /// </summary>
        public static readonly StringKey<ForFormattedText> JsonUndefinedValue = new StringKey<ForFormattedText>(nameof(JsonUndefinedValue));

        /// <summary>
        /// Gets the standard <see cref="PType"/> for <see cref="PBoolean"/> values.
        /// </summary>
        public static readonly PType<PBoolean> Boolean = new BaseType<PBoolean>(JsonBoolean, new ToBoolConverter());

        /// <summary>
        /// Gets the standard <see cref="PType"/> for <see cref="PInteger"/> values.
        /// </summary>
        public static readonly PType<PInteger> Integer = new BaseType<PInteger>(JsonInteger, new ToIntConverter());

        /// <summary>
        /// Gets the standard <see cref="PType"/> for <see cref="PString"/> values.
        /// </summary>
        public static readonly PType<PString> String = new BaseType<PString>(JsonString, new ToStringConverter());

        private class JsonConverter<T> : JsonValueSyntaxVisitor<Maybe<T>>
        {
            public override Maybe<T> DefaultVisit(JsonValueSyntax node, _void arg) => Maybe<T>.Nothing;
        }

        private class ToBoolConverter : JsonConverter<PBoolean>
        {
            public override Maybe<PBoolean> VisitBooleanLiteralSyntax(JsonBooleanLiteralSyntax value, _void arg) => value.Value ? PConstantValue.True : PConstantValue.False;
        }

        private class ToIntConverter : JsonConverter<PInteger>
        {
            public override Maybe<PInteger> VisitIntegerLiteralSyntax(JsonIntegerLiteralSyntax value, _void arg) => new PInteger(value.Value);
        }

        private class ToStringConverter : JsonConverter<PString>
        {
            public override Maybe<PString> VisitStringLiteralSyntax(JsonStringLiteralSyntax value, _void arg) => new PString(value.Value);
        }

        private sealed class BaseType<TValue> : PType<TValue>
            where TValue : PValue
        {
            private readonly PTypeErrorBuilder typeError;
            private readonly JsonValueSyntaxVisitor<Maybe<TValue>> converter;

            public BaseType(StringKey<ForFormattedText> expectedTypeDescriptionKey, JsonValueSyntaxVisitor<Maybe<TValue>> converter)
            {
                typeError = new PTypeErrorBuilder(expectedTypeDescriptionKey);
                this.converter = converter;
            }

            internal override Union<ITypeErrorBuilder, TValue> TryCreateValue(JsonValueSyntax valueNode, ArrayBuilder<PTypeError> errors)
                => converter.Visit(valueNode).IsJust(out TValue convertedValue)
                ? convertedValue
                : Union<ITypeErrorBuilder, TValue>.Option1(typeError);

            public override PValue ConvertToPValue(TValue value) => value;
        }

        /// <summary>
        /// Abstract base class for all types which depend on a base <see cref="PType{TBase}"/>
        /// and then apply a further restriction or conversion.
        /// </summary>
        /// <typeparam name="TBase">
        /// The .NET target <see cref="Type"/> of the base <see cref="PType{TBase}"/>.
        /// </typeparam>
        /// <typeparam name="T">
        /// The .NET target <see cref="Type"/> to convert to and from.
        /// </typeparam>
        public abstract class Derived<TBase, T> : PType<T>
        {
            /// <summary>
            /// Gets the base <see cref="PType{TBase}"/>.
            /// </summary>
            public PType<TBase> BaseType { get; }

            /// <summary>
            /// Initializes a new instance of <see cref="Derived{TBase, T}"/> with a base <see cref="PType{TBase}"/>.
            /// </summary>
            /// <param name="baseType">
            /// The base <see cref="PType{TBase}"/> to depend on.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="baseType"/> is <see langword="null"/>.
            /// </exception>
            protected Derived(PType<TBase> baseType)
                => BaseType = baseType ?? throw new ArgumentNullException(nameof(baseType));

            /// <summary>
            /// Helper method to indicate a failed conversion in <see cref="TryGetTargetValue(TBase)"/>.
            /// </summary>
            /// <param name="typeError">
            /// The type error generated by the failed conversion.
            /// </param>
            /// <returns>
            /// The type error, indicating a failed conversion.
            /// </returns>
            protected Union<ITypeErrorBuilder, T> InvalidValue(ITypeErrorBuilder typeError)
                => Union<ITypeErrorBuilder, T>.Option1(typeError);

            internal override sealed Union<ITypeErrorBuilder, T> TryCreateValue(JsonValueSyntax valueNode, ArrayBuilder<PTypeError> errors)
                => BaseType.TryCreateValue(valueNode, errors).Match(
                    whenOption1: Union<ITypeErrorBuilder, T>.Option1,
                    whenOption2: TryGetTargetValue);

            public override sealed PValue ConvertToPValue(T value) => BaseType.ConvertToPValue(ConvertToBaseValue(value));

            /// <summary>
            /// Attempts to convert a <see cref="TBase"/> value to the target .NET type <typeparamref name="T"/>.
            /// </summary>
            /// <param name="value">
            /// The value to convert from.
            /// </param>
            /// <returns>
            /// The target value to convert to, if conversion succeeds, or a type error, if conversion fails.
            /// </returns>
            public abstract Union<ITypeErrorBuilder, T> TryGetTargetValue(TBase value);

            /// <summary>
            /// Converts a value of the target .NET type <typeparamref name="T"/> to a <see cref="TBase"/> value.
            /// Assumed is that this is the reverse operation of <see cref="TryGetTargetValue(TBase)"/>.
            /// </summary>
            /// <param name="value">
            /// The value to convert from.
            /// </param>
            /// <returns>
            /// The converted base value.
            /// </returns>
            public abstract TBase ConvertToBaseValue(T value);
        }

        /// <summary>
        /// <see cref="Derived{T, T}"/> type which filters values of the target type.
        /// </summary>
        /// <typeparam name="T">
        /// The .NET target <see cref="Type"/> to filter.
        /// </typeparam>
        public abstract class Filter<T> : Derived<T, T>
        {
            /// <summary>
            /// Helper method to indicate a failed conversion in <see cref="IsValid(T, out ITypeErrorBuilder)"/>.
            /// </summary>
            /// <param name="convertTypeError">
            /// The type error generated by the failed conversion.
            /// </param>
            /// <param name="typeError">
            /// Always returns <paramref name="convertTypeError"/>.
            /// </param>
            /// <returns>
            /// Always returns <see langword="false"/>.
            /// </returns>
            protected bool InvalidValue(ITypeErrorBuilder convertTypeError, out ITypeErrorBuilder typeError)
            {
                typeError = convertTypeError;
                return false;
            }

            /// <summary>
            /// Helper method to indicate a successful conversion in <see cref="IsValid(T, out ITypeErrorBuilder)"/>.
            /// </summary>
            /// <param name="typeError">
            /// Always returns <see langword="null"/>.
            /// </param>
            /// <returns>
            /// Always returns <see langword="true"/>.
            /// </returns>
            protected bool ValidValue(out ITypeErrorBuilder typeError)
            {
                typeError = null;
                return true;
            }

            protected Filter(PType<T> baseType) : base(baseType) { }

            /// <summary>
            /// Returns if a value of the target type is a member of this <see cref="PType"/>.
            /// </summary>
            /// <param name="candidateValue">
            /// The candidate value to check.
            /// </param>
            /// <param name="typeError">
            /// A type error, if the candidate value is invalid.
            /// </param>
            /// <returns>
            /// <see langword="true"/> if the candidate value is valid; otherwise <see langword="false"/>.
            /// </returns>
            public abstract bool IsValid(T candidateValue, out ITypeErrorBuilder typeError);

            public override sealed Union<ITypeErrorBuilder, T> TryGetTargetValue(T candidateValue)
                => IsValid(candidateValue, out ITypeErrorBuilder typeError)
                ? candidateValue
                : InvalidValue(typeError);

            public override sealed T ConvertToBaseValue(T value)
            {
                if (!IsValid(value, out _))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        value,
                        $"Value is not a member of {ToString()}.");
                }

                return value;
            }
        }

        /// <summary>
        /// Aliases a base type.
        /// </summary>
        /// <typeparam name="T">
        /// The .NET target <see cref="Type"/> to alias.
        /// </typeparam>
        public abstract class Alias<T> : Filter<T>
        {
            protected Alias(PType<T> baseType) : base(baseType) { }

            public sealed override bool IsValid(T candidateValue, out ITypeErrorBuilder typeError) { typeError = default; return true; }
        }

        public sealed class RangedInteger : Filter<PInteger>, ITypeErrorBuilder
        {
            /// <summary>
            /// Gets the minimum value which is allowed for values of this type.
            /// </summary>
            public BigInteger MinValue { get; }

            /// <summary>
            /// Gets the maximum value which is allowed for values of this type.
            /// </summary>
            public BigInteger MaxValue { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="RangedInteger"/> type.
            /// </summary>
            /// <param name="minValue">
            /// The minimum allowed value.
            /// </param>
            /// <param name="maxValue">
            /// The maximum allowed value.
            /// </param>
            public RangedInteger(BigInteger minValue, BigInteger maxValue) : base(Integer)
            {
                MinValue = minValue;
                MaxValue = maxValue;
            }

            public override bool IsValid(PInteger candidateValue, out ITypeErrorBuilder typeError)
                => MinValue <= candidateValue.Value
                && candidateValue.Value <= MaxValue
                ? ValidValue(out typeError)
                : InvalidValue(this, out typeError);

            private static string FormatExpectedTypeDescription(TextFormatter formatter, BigInteger minValue, BigInteger maxValue)
                => formatter.Format(
                    RangedJsonInteger,
                    minValue.ToStringInvariant(),
                    maxValue.ToStringInvariant());

            public static string FormatTypeErrorMessage(
                TextFormatter formatter,
                string actualValueString,
                BigInteger minValue,
                BigInteger maxValue)
                => PTypeErrorBuilder.FormatTypeErrorMessage(
                    formatter,
                    FormatExpectedTypeDescription(formatter, minValue, maxValue),
                    actualValueString);

            public static string FormatTypeErrorAtPropertyKeyMessage(
                TextFormatter formatter,
                string actualValueString,
                string propertyKey,
                BigInteger minValue,
                BigInteger maxValue)
                => PTypeErrorBuilder.FormatTypeErrorSomewhereMessage(
                    formatter,
                    FormatExpectedTypeDescription(formatter, minValue, maxValue),
                    actualValueString,
                    PTypeErrorBuilder.FormatLocatedAtPropertyKeyMessage(formatter, propertyKey));

            public static string FormatTypeErrorAtItemIndexMessage(
                TextFormatter formatter,
                string actualValueString,
                int itemIndex,
                BigInteger minValue,
                BigInteger maxValue)
                => PTypeErrorBuilder.FormatTypeErrorSomewhereMessage(
                    formatter,
                    FormatExpectedTypeDescription(formatter, minValue, maxValue),
                    actualValueString,
                    PTypeErrorBuilder.FormatLocatedAtItemIndexMessage(formatter, itemIndex));

            public string FormatTypeErrorMessage(TextFormatter formatter, string actualValueString)
                => FormatTypeErrorMessage(formatter, actualValueString, MinValue, MaxValue);

            public string FormatTypeErrorAtPropertyKeyMessage(TextFormatter formatter, string actualValueString, string propertyKey)
                => FormatTypeErrorAtPropertyKeyMessage(formatter, actualValueString, propertyKey, MinValue, MaxValue);

            public string FormatTypeErrorAtItemIndexMessage(TextFormatter formatter, string actualValueString, int itemIndex)
                => FormatTypeErrorAtItemIndexMessage(formatter, actualValueString, itemIndex, MinValue, MaxValue);

            public override string ToString()
                => $"{nameof(RangedInteger)}[{MinValue}..{MaxValue}]";
        }
    }
}

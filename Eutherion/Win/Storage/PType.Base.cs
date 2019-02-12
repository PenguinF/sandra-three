﻿#region License
/*********************************************************************************
 * PType.Base.cs
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
using System.Globalization;
using System.Numerics;

namespace Sandra.UI.WF.Storage
{
    public static partial class PType
    {
        /// <summary>
        /// Gets the translation key for referring to a general json array (list).
        /// </summary>
        public static readonly LocalizedStringKey JsonArray = new LocalizedStringKey(nameof(JsonArray));

        /// <summary>
        /// Gets the translation key for referring to a general json object (map).
        /// </summary>
        public static readonly LocalizedStringKey JsonObject = new LocalizedStringKey(nameof(JsonObject));

        /// <summary>
        /// Gets the translation key for referring to an undefined value.
        /// </summary>
        public static readonly LocalizedStringKey JsonUndefinedValue = new LocalizedStringKey(nameof(JsonUndefinedValue));

        public static readonly PTypeErrorBuilder BooleanTypeError
            = new PTypeErrorBuilder(new LocalizedStringKey(nameof(BooleanTypeError)));

        public static readonly PTypeErrorBuilder IntegerTypeError
            = new PTypeErrorBuilder(new LocalizedStringKey(nameof(IntegerTypeError)));

        public static readonly PTypeErrorBuilder MapTypeError
            = new PTypeErrorBuilder(new LocalizedStringKey(nameof(MapTypeError)));

        public static readonly PTypeErrorBuilder StringTypeError
            = new PTypeErrorBuilder(new LocalizedStringKey(nameof(StringTypeError)));

        /// <summary>
        /// Gets the standard <see cref="PType"/> for <see cref="PBoolean"/> values.
        /// </summary>
        public static readonly PType<PBoolean> Boolean = new BaseType<PBoolean>(BooleanTypeError);

        /// <summary>
        /// Gets the standard <see cref="PType"/> for <see cref="PInteger"/> values.
        /// </summary>
        public static readonly PType<PInteger> Integer = new BaseType<PInteger>(IntegerTypeError);

        /// <summary>
        /// Gets the standard <see cref="PType"/> for <see cref="PMap"/> values.
        /// </summary>
        public static readonly PType<PMap> Map = new BaseType<PMap>(MapTypeError);

        /// <summary>
        /// Gets the standard <see cref="PType"/> for <see cref="PString"/> values.
        /// </summary>
        public static readonly PType<PString> String = new BaseType<PString>(StringTypeError);

        private sealed class BaseType<TValue> : PType<TValue>
            where TValue : PValue
        {
            public PTypeErrorBuilder TypeError { get; }

            public BaseType(PTypeErrorBuilder typeError) => TypeError = typeError;

            public override Union<ITypeErrorBuilder, TValue> TryGetValidValue(PValue value)
                => value is TValue targetValue
                ? ValidValue(targetValue)
                : InvalidValue(TypeError);

            public override PValue GetPValue(TValue value) => value;
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
            /// <paramref name="baseType"/> is null.
            /// </exception>
            protected Derived(PType<TBase> baseType)
                => BaseType = baseType ?? throw new ArgumentNullException(nameof(baseType));

            public override sealed Union<ITypeErrorBuilder, T> TryGetValidValue(PValue value)
                => BaseType.TryGetValidValue(value).Match(InvalidValue, TryGetTargetValue);

            public override sealed PValue GetPValue(T value) => BaseType.GetPValue(GetBaseValue(value));

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
            public abstract TBase GetBaseValue(T value);
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
            /// Always returns false.
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
            /// Always returns null.
            /// </param>
            /// <returns>
            /// Always returns true.
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
            /// True if the candidate value is valid; otherwise false.
            /// </returns>
            public abstract bool IsValid(T candidateValue, out ITypeErrorBuilder typeError);

            public override sealed Union<ITypeErrorBuilder, T> TryGetTargetValue(T candidateValue)
                => IsValid(candidateValue, out ITypeErrorBuilder typeError)
                ? ValidValue(candidateValue)
                : InvalidValue(typeError);

            public override sealed T GetBaseValue(T value)
            {
                if (!IsValid(value, out ITypeErrorBuilder _))
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
        /// Gets the translation key for <see cref="RangedInteger"/> type check failure error messages.
        /// </summary>
        public static readonly LocalizedStringKey RangedIntegerTypeError = new LocalizedStringKey(nameof(RangedIntegerTypeError));

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

            /// <summary>
            /// Gets the localized, context sensitive message for this error.
            /// </summary>
            public string GetLocalizedTypeErrorMessage(Localizer localizer, string propertyKey, string valueString)
                => localizer.Localize(RangedIntegerTypeError, new[]
                {
                    propertyKey,
                    valueString,
                    MinValue.ToString(CultureInfo.InvariantCulture),
                    MaxValue.ToString(CultureInfo.InvariantCulture)
                });

            public override string ToString()
                => $"{nameof(RangedInteger)}[{MinValue}..{MaxValue}]";
        }
    }
}
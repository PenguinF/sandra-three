#region License
/*********************************************************************************
 * PType.Common.cs
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Eutherion.Win.Storage
{
    public static partial class PType
    {
        /// <summary>
        /// Contains <see cref="PType"/>s which convert to and from common .NET data types.
        /// </summary>
        public static class CLR
        {
            public static readonly PType<int> Int32 = new Int32CLRType();
            public static readonly PType<uint> UInt32 = new UInt32CLRType();

            private sealed class Int32CLRType : Derived<BigInteger, int>
            {
                public Int32CLRType() : base(new RangedInteger(int.MinValue, int.MaxValue)) { }

                public override Union<ITypeErrorBuilder, int> TryGetTargetValue(BigInteger integer) => (int)integer;

                public override BigInteger ConvertToBaseValue(int value) => value;
            }

            private sealed class UInt32CLRType : Derived<BigInteger, uint>
            {
                public UInt32CLRType() : base(new RangedInteger(uint.MinValue, uint.MaxValue)) { }

                public override Union<ITypeErrorBuilder, uint> TryGetTargetValue(BigInteger integer) => (uint)integer;

                public override BigInteger ConvertToBaseValue(uint value) => value;
            }

            /// <summary>
            /// Converts to and from <see cref="int"/> values within a specified valid range.
            /// </summary>
            public sealed class RangedInt32 : Filter<int>, ITypeErrorBuilder
            {
                /// <summary>
                /// Gets the minimum value which is allowed for values of this type.
                /// </summary>
                public int MinValue { get; }

                /// <summary>
                /// Gets the maximum value which is allowed for values of this type.
                /// </summary>
                public int MaxValue { get; }

                /// <summary>
                /// Initializes a new instance of the <see cref="RangedInt32"/> type.
                /// </summary>
                /// <param name="minValue">
                /// The minimum allowed value.
                /// </param>
                /// <param name="maxValue">
                /// The maximum allowed value.
                /// </param>
                public RangedInt32(int minValue, int maxValue) : base(Int32)
                {
                    MinValue = minValue;
                    MaxValue = maxValue;
                }

                public override bool IsValid(int candidateValue, out ITypeErrorBuilder typeError)
                    => MinValue <= candidateValue
                    && candidateValue <= MaxValue
                    ? ValidValue(out typeError)
                    : InvalidValue(this, out typeError);

                public string FormatTypeErrorMessage(TextFormatter formatter, string actualValueString)
                    => RangedInteger.FormatTypeErrorMessage(formatter, actualValueString, MinValue, MaxValue);

                public string FormatTypeErrorAtPropertyKeyMessage(TextFormatter formatter, string actualValueString, string propertyKey)
                    => RangedInteger.FormatTypeErrorAtPropertyKeyMessage(formatter, actualValueString, propertyKey, MinValue, MaxValue);

                public string FormatTypeErrorAtItemIndexMessage(TextFormatter formatter, string actualValueString, int itemIndex)
                    => RangedInteger.FormatTypeErrorAtItemIndexMessage(formatter, actualValueString, itemIndex, MinValue, MaxValue);

                public override string ToString()
                    => $"{nameof(RangedInt32)}[{MinValue}..{MaxValue}]";
            }
        }

        public sealed class Enumeration<TEnum> : Derived<string, TEnum>, ITypeErrorBuilder where TEnum : struct
        {
            private readonly Dictionary<TEnum, string> enumToString = new Dictionary<TEnum, string>();
            private readonly Dictionary<string, TEnum> stringToEnum = new Dictionary<string, TEnum>();

            /// <summary>
            /// Initializes a new instance of an <see cref="Enumeration{TEnum}"/> <see cref="PType"/>.
            /// </summary>
            /// <param name="enumValues">
            /// The list of distinct enumeration values.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="enumValues"/> is <see langword="null"/>.
            /// </exception>
            public Enumeration(IEnumerable<TEnum> enumValues) : base(String)
            {
                if (enumValues == null) throw new ArgumentNullException(nameof(enumValues));

                Type enumType = typeof(TEnum);
                foreach (var enumValue in enumValues)
                {
                    string name = Enum.GetName(enumType, enumValue);
                    enumToString.Add(enumValue, name);
                    stringToEnum.Add(name, enumValue);
                }
            }

            public override Union<ITypeErrorBuilder, TEnum> TryGetTargetValue(string stringValue)
                => stringToEnum.TryGetValue(stringValue, out TEnum targetValue)
                ? targetValue
                : InvalidValue(this);

            public override string ConvertToBaseValue(TEnum value) => enumToString[value];

            private string GenericTypeErrorMessage(TextFormatter formatter, string actualValueString, Maybe<string> maybeSomewhere)
            {
                if (stringToEnum.Count == 0)
                {
                    return maybeSomewhere.Match(
                        whenNothing: () => formatter.Format(
                            PTypeErrorBuilder.NoLegalValuesError,
                            actualValueString),
                        whenJust: somewhere => formatter.Format(
                            PTypeErrorBuilder.NoLegalValuesErrorSomewhere,
                            actualValueString,
                            somewhere));
                }

                string formattedValueList;
                if (stringToEnum.Count == 1)
                {
                    formattedValueList = PTypeErrorBuilder.QuoteStringValue(stringToEnum.Keys.First());
                }
                else
                {
                    IEnumerable<string> enumValues = stringToEnum.Keys.Take(stringToEnum.Count - 1).Select(PTypeErrorBuilder.QuoteStringValue);
                    var lastEnumValue = PTypeErrorBuilder.QuoteStringValue(stringToEnum.Keys.Last());
                    formattedValueList = formatter.Format(
                        PTypeErrorBuilder.EnumerateWithOr,
                        string.Join(", ", enumValues),
                        lastEnumValue);
                }

                return maybeSomewhere.Match(
                    whenNothing: () => PTypeErrorBuilder.FormatTypeErrorMessage(
                        formatter,
                        formattedValueList,
                        actualValueString),
                    whenJust: somewhere => PTypeErrorBuilder.FormatTypeErrorSomewhereMessage(
                        formatter,
                        formattedValueList,
                        actualValueString,
                        somewhere));
            }

            public string FormatTypeErrorMessage(TextFormatter formatter, string actualValueString)
                => GenericTypeErrorMessage(formatter, actualValueString, Maybe<string>.Nothing);

            public string FormatTypeErrorAtPropertyKeyMessage(TextFormatter formatter, string actualValueString, string propertyKey)
                => GenericTypeErrorMessage(formatter, actualValueString, PTypeErrorBuilder.FormatLocatedAtPropertyKeyMessage(formatter, propertyKey));

            public string FormatTypeErrorAtItemIndexMessage(TextFormatter formatter, string actualValueString, int itemIndex)
                => GenericTypeErrorMessage(formatter, actualValueString, PTypeErrorBuilder.FormatLocatedAtItemIndexMessage(formatter, itemIndex));
        }

        public sealed class KeyedSet<T> : Derived<string, T>, ITypeErrorBuilder where T : class
        {
            private readonly Dictionary<string, T> stringToTarget = new Dictionary<string, T>();

            /// <summary>
            /// Initializes a new instance of a <see cref="KeyedSet{T}"/> <see cref="PType"/>.
            /// </summary>
            /// <param name="keyedValues">
            /// The mapping which maps distinct keys to values of type <typeparamref name="T"/>.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="keyedValues"/> is <see langword="null"/>.
            /// </exception>
            public KeyedSet(IEnumerable<KeyValuePair<string, T>> keyedValues) : base(String)
            {
                if (keyedValues == null) throw new ArgumentNullException(nameof(keyedValues));
                keyedValues.ForEach(stringToTarget.Add);
            }

            public override Union<ITypeErrorBuilder, T> TryGetTargetValue(string stringValue)
                => stringToTarget.TryGetValue(stringValue, out T targetValue)
                ? targetValue
                : InvalidValue(this);

            public override string ConvertToBaseValue(T value)
            {
                foreach (var kv in stringToTarget)
                {
                    if (kv.Value == value) return kv.Key;
                }

                throw new ArgumentException("Target value not found.");
            }

            private string GenericTypeErrorMessage(TextFormatter formatter, string actualValueString, Maybe<string> maybeSomewhere)
            {
                if (stringToTarget.Count == 0)
                {
                    return maybeSomewhere.Match(
                        whenNothing: () => formatter.Format(
                            PTypeErrorBuilder.NoLegalValuesError,
                            actualValueString),
                        whenJust: somewhere => formatter.Format(
                            PTypeErrorBuilder.NoLegalValuesErrorSomewhere,
                            actualValueString,
                            somewhere));
                }

                string formattedKeysList;
                if (stringToTarget.Count == 1)
                {
                    formattedKeysList = PTypeErrorBuilder.QuoteStringValue(stringToTarget.Keys.First());
                }
                else
                {
                    // TODO: escape characters in KeyedSet keys.
                    IEnumerable<string> keys = stringToTarget.Keys.Take(stringToTarget.Count - 1).Select(PTypeErrorBuilder.QuoteStringValue);
                    var lastKey = PTypeErrorBuilder.QuoteStringValue(stringToTarget.Keys.Last());
                    formattedKeysList = formatter.Format(
                        PTypeErrorBuilder.EnumerateWithOr,
                        string.Join(", ", keys),
                        lastKey);
                }

                return maybeSomewhere.Match(
                    whenNothing: () => PTypeErrorBuilder.FormatTypeErrorMessage(
                        formatter,
                        formattedKeysList,
                        actualValueString),
                    whenJust: somewhere => PTypeErrorBuilder.FormatTypeErrorSomewhereMessage(
                        formatter,
                        formattedKeysList,
                        actualValueString,
                        somewhere));
            }

            public string FormatTypeErrorMessage(TextFormatter formatter, string actualValueString)
                => GenericTypeErrorMessage(formatter, actualValueString, Maybe<string>.Nothing);

            public string FormatTypeErrorAtPropertyKeyMessage(TextFormatter formatter, string actualValueString, string propertyKey)
                => GenericTypeErrorMessage(formatter, actualValueString, PTypeErrorBuilder.FormatLocatedAtPropertyKeyMessage(formatter, propertyKey));

            public string FormatTypeErrorAtItemIndexMessage(TextFormatter formatter, string actualValueString, int itemIndex)
                => GenericTypeErrorMessage(formatter, actualValueString, PTypeErrorBuilder.FormatLocatedAtItemIndexMessage(formatter, itemIndex));
        }
    }
}

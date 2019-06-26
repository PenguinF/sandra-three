#region License
/*********************************************************************************
 * PType.Common.cs
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
using Eutherion.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Eutherion.Win.Storage
{
    public static partial class PType
    {
        /// <summary>
        /// Contains <see cref="PType"/>s which convert to and from common .NET data types.
        /// </summary>
        public static class CLR
        {
            public static readonly PType<bool> Boolean = new _BooleanCLRType();
            public static readonly PType<int> Int32 = new _Int32CLRType();
            public static readonly PType<uint> UInt32 = new _UInt32CLRType();
            public static readonly PType<string> String = new _StringCLRType();

            private sealed class _BooleanCLRType : Derived<PBoolean, bool>
            {
                public _BooleanCLRType() : base(PType.Boolean) { }

                public override Union<ITypeErrorBuilder, bool> TryGetTargetValue(PBoolean boolean)
                    => boolean.Value;

                public override PBoolean GetBaseValue(bool value)
                    => new PBoolean(value);
            }

            private sealed class _Int32CLRType : Derived<PInteger, int>
            {
                public _Int32CLRType() : base(new RangedInteger(int.MinValue, int.MaxValue)) { }

                public override Union<ITypeErrorBuilder, int> TryGetTargetValue(PInteger integer)
                    => (int)integer.Value;

                public override PInteger GetBaseValue(int value)
                    => new PInteger(value);
            }

            private sealed class _UInt32CLRType : Derived<PInteger, uint>
            {
                public _UInt32CLRType() : base(new RangedInteger(uint.MinValue, uint.MaxValue)) { }

                public override Union<ITypeErrorBuilder, uint> TryGetTargetValue(PInteger integer)
                    => (uint)integer.Value;

                public override PInteger GetBaseValue(uint value)
                    => new PInteger(value);
            }

            private sealed class _StringCLRType : Derived<PString, string>
            {
                public _StringCLRType() : base(PType.String) { }

                public override Union<ITypeErrorBuilder, string> TryGetTargetValue(PString stringValue)
                    => stringValue.Value;

                public override PString GetBaseValue(string value)
                    => new PString(value);
            }
        }

        /// <summary>
        /// Gets the translation key for a typecheck error message of <see cref="PType.Enumeration{TEnum}"/>.
        /// </summary>
        public static readonly LocalizedStringKey EnumerationTypeError = new LocalizedStringKey(nameof(EnumerationTypeError));

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
            /// <paramref name="enumValues"/> is null.
            /// </exception>
            public Enumeration(IEnumerable<TEnum> enumValues) : base(CLR.String)
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

            public override string GetBaseValue(TEnum value) => enumToString[value];

            /// <summary>
            /// Gets the localized, context sensitive message for this error.
            /// </summary>
            public string GetLocalizedTypeErrorMessage(Localizer localizer, string propertyKey, string valueString)
            {
                if (stringToEnum.Count == 0)
                {
                    return localizer.Localize(PTypeErrorBuilder.NoLegalValues, new[]
                    {
                        propertyKey,
                        valueString
                    });
                }

                string localizedValueList;
                if (stringToEnum.Count == 1)
                {
                    localizedValueList = PTypeErrorBuilder.QuoteStringValue(stringToEnum.Keys.First());
                }
                else
                {
                    IEnumerable<string> enumValues = stringToEnum.Keys.Take(stringToEnum.Count - 1).Select(PTypeErrorBuilder.QuoteStringValue);
                    var lastEnumValue = PTypeErrorBuilder.QuoteStringValue(stringToEnum.Keys.Last());
                    localizedValueList = localizer.Localize(PTypeErrorBuilder.EnumerateWithOr, new[]
                    {
                        string.Join(", ", enumValues),
                        lastEnumValue
                    });
                }

                return localizer.Localize(EnumerationTypeError, new[]
                {
                    propertyKey,
                    valueString,
                    localizedValueList
                });
            }
        }

        /// <summary>
        /// Gets the translation key for a typecheck error message of <see cref="PType.KeyedSet{T}"/>.
        /// </summary>
        public static readonly LocalizedStringKey KeyedSetTypeError = new LocalizedStringKey(nameof(KeyedSetTypeError));

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
            /// <paramref name="keyedValues"/> is null.
            /// </exception>
            public KeyedSet(IEnumerable<KeyValuePair<string, T>> keyedValues) : base(CLR.String)
            {
                if (keyedValues == null) throw new ArgumentNullException(nameof(keyedValues));
                keyedValues.ForEach(stringToTarget.Add);
            }

            public override Union<ITypeErrorBuilder, T> TryGetTargetValue(string stringValue)
                => stringToTarget.TryGetValue(stringValue, out T targetValue)
                ? targetValue
                : InvalidValue(this);

            public override string GetBaseValue(T value)
            {
                foreach (var kv in stringToTarget)
                {
                    if (kv.Value == value) return kv.Key;
                }

                throw new ArgumentException("Target value not found.");
            }

            /// <summary>
            /// Gets the localized, context sensitive message for this error.
            /// </summary>
            public string GetLocalizedTypeErrorMessage(Localizer localizer, string propertyKey, string valueString)
            {
                if (stringToTarget.Count == 0)
                {
                    return localizer.Localize(PTypeErrorBuilder.NoLegalValues, new[]
                    {
                        propertyKey,
                        valueString
                    });
                }

                string localizedKeysList;
                if (stringToTarget.Count == 1)
                {
                    localizedKeysList = PTypeErrorBuilder.QuoteStringValue(stringToTarget.Keys.First());
                }
                else
                {
                    // TODO: escape characters in KeyedSet keys.
                    IEnumerable<string> keys = stringToTarget.Keys.Take(stringToTarget.Count - 1).Select(PTypeErrorBuilder.QuoteStringValue);
                    var lastKey = PTypeErrorBuilder.QuoteStringValue(stringToTarget.Keys.Last());
                    localizedKeysList = localizer.Localize(PTypeErrorBuilder.EnumerateWithOr, new[]
                    {
                        string.Join(", ", keys),
                        lastKey
                    });
                }

                return localizer.Localize(KeyedSetTypeError, new[]
                {
                    propertyKey,
                    valueString,
                    localizedKeysList
                });
            }
        }
    }
}

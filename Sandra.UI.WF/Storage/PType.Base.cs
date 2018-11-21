#region License
/*********************************************************************************
 * PType.Base.cs
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

using System;
using System.Numerics;

namespace Sandra.UI.WF.Storage
{
    public static partial class PType
    {
        /// <summary>
        /// Gets the standard <see cref="PType"/> for <see cref="PBoolean"/> values.
        /// </summary>
        public static readonly PType<PBoolean> Boolean = new BaseType<PBoolean>();

        /// <summary>
        /// Gets the standard <see cref="PType"/> for <see cref="PInteger"/> values.
        /// </summary>
        public static readonly PType<PInteger> Integer = new BaseType<PInteger>();

        /// <summary>
        /// Gets the standard <see cref="PType"/> for <see cref="PMap"/> values.
        /// </summary>
        public static readonly PType<PMap> Map = new BaseType<PMap>();

        /// <summary>
        /// Gets the standard <see cref="PType"/> for <see cref="PString"/> values.
        /// </summary>
        public static readonly PType<PString> String = new BaseType<PString>();

        private sealed class BaseType<TValue> : PType<TValue>
            where TValue : PValue
        {
            public override bool TryGetValidValue(PValue value, out TValue targetValue)
            {
                if (value is TValue)
                {
                    targetValue = (TValue)value;
                    return true;
                }

                targetValue = default(TValue);
                return false;
            }

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
            {
                BaseType = baseType ?? throw new ArgumentNullException(nameof(baseType));
            }

            public override sealed bool TryGetValidValue(PValue value, out T targetValue)
            {
                if (BaseType.TryGetValidValue(value, out TBase baseValue)
                    && TryGetTargetValue(baseValue, out targetValue))
                {
                    return true;
                }

                targetValue = default(T);
                return false;
            }

            public override sealed PValue GetPValue(T value) => BaseType.GetPValue(GetBaseValue(value));

            /// <summary>
            /// Attempts to convert a <see cref="TBase"/> value to the target .NET type <typeparamref name="T"/>.
            /// </summary>
            /// <param name="value">
            /// The value to convert from.
            /// </param>
            /// <param name="targetValue">
            /// The target value to convert to, if conversion succeeds.
            /// </param>
            /// <returns>
            /// Whether or not conversion succeeded.
            /// </returns>
            public abstract bool TryGetTargetValue(TBase value, out T targetValue);

            /// <summary>
            /// Converts a value of the target .NET type <typeparamref name="T"/> to a <see cref="TBase"/> value.
            /// Assumed is that this is the reverse operation of <see cref="GetTargetValue(TBase)"/>.
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
            protected Filter(PType<T> baseType) : base(baseType) { }

            /// <summary>
            /// Returns if a value of the target type is a member of this <see cref="PType"/>.
            /// </summary>
            public abstract bool IsValid(T candidateValue);

            public override sealed bool TryGetTargetValue(T candidateValue, out T targetValue)
            {
                if (IsValid(candidateValue))
                {
                    targetValue = candidateValue;
                    return true;
                }

                targetValue = default(T);
                return false;
            }

            public override sealed T GetBaseValue(T value)
            {
                if (!IsValid(value))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        value,
                        $"Value is not a member of {ToString()}.");
                }

                return value;
            }
        }

        public sealed class RangedInteger : Filter<PInteger>
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

            public override bool IsValid(PInteger candidateValue)
                => MinValue <= candidateValue.Value
                && candidateValue.Value <= MaxValue;

            public override string ToString()
                => $"{nameof(RangedInteger)}[{MinValue}..{MaxValue}]";
        }
    }
}

﻿#region License
/*********************************************************************************
 * PType.cs
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

namespace Sandra.UI.WF.Storage
{
    /// <summary>
    /// Represents a type of <see cref="PValue"/>, which controls the range of values that are possible.
    /// </summary>
    /// <typeparam name="T">
    /// The .NET target <see cref="System.Type"/> to convert to and from.
    /// </typeparam>
    public abstract class PType<T>
    {
        /// <summary>
        /// Helper method to indicate a failed conversion in <see cref="TryGetValidValue(PValue)"/>.
        /// </summary>
        /// <param name="typeError">
        /// The type error generated by the failed conversion.
        /// </param>
        /// <returns>
        /// The type error, indicating a failed conversion.
        /// </returns>
        protected Union<ITypeErrorBuilder, T> InvalidValue(ITypeErrorBuilder typeError)
            => Union<ITypeErrorBuilder, T>.Option1(typeError);

        /// <summary>
        /// Helper method to indicate a successful conversion in <see cref="TryGetValidValue(PValue)"/>.
        /// </summary>
        /// <param name="targetValue">
        /// The successfully converted value.
        /// </param>
        /// <returns>
        /// The target value to convert to, indicating a successful conversion.
        /// </returns>
        protected Union<ITypeErrorBuilder, T> ValidValue(T targetValue)
            => Union<ITypeErrorBuilder, T>.Option2(targetValue);

        /// <summary>
        /// Attempts to convert a raw <see cref="PValue"/> to the target .NET type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="value">
        /// The value to convert from.
        /// </param>
        /// <returns>
        /// The target value to convert to, if conversion succeeds, or a type error, if conversion fails.
        /// </returns>
        public abstract Union<ITypeErrorBuilder, T> TryGetValidValue(PValue value);

        /// <summary>
        /// Converts a value of the target .NET type <typeparamref name="T"/> to a <see cref="PValue"/>.
        /// Assumed is that this is the reverse operation of <see cref="TryGetValidValue(PValue)"/>, i.e.:
        /// <code>
        /// if (TryGetValidValue(value).IsOption2(out targetValue))
        /// {
        ///     PValue convertedValue = GetPValue(targetValue);
        ///     Debug.Assert(PValueEqualityComparer.Instance.AreEqual(value, convertedValue), "This should always succeed.");
        /// }
        /// </code>
        /// And vice versa.
        /// </summary>
        /// <param name="value">
        /// The value to convert from.
        /// </param>
        /// <returns>
        /// The converted target value.
        /// </returns>
        public abstract PValue GetPValue(T value);
    }
}
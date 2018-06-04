/*********************************************************************************
 * PType.cs
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
        /// Attempts to convert a raw <see cref="PValue"/> to the target .NET type <typeparamref name="T"/>.
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
        public abstract bool TryGetValidValue(PValue value, out T targetValue);

        /// <summary>
        /// Converts a value of the target .NET type <typeparamref name="T"/> to a <see cref="PValue"/>.
        /// Assumed is that this is the reverse operation of <see cref="TryGetValidValue(PValue, out T)"/>, i.e.:
        /// <code>
        /// if (TryGetValidValue(value, out targetValue))
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

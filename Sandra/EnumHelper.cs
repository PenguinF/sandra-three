/*********************************************************************************
 * EnumHelper.cs
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandra
{
    /// <summary>
    /// Contains general helper methods for enumerations.
    /// </summary>
    /// <remarks>
    /// Declaring an <see cref="EnumHelper{TEnum}"/> with a non-enumeration type
    /// results in a <see cref="TypeInitializationException"/> being thrown.
    /// </remarks>
    public static class EnumHelper<TEnum> where TEnum : struct
    {
        static readonly TEnum[] distinctValues;

        static EnumHelper()
        {
            TEnum[] values = (TEnum[])Enum.GetValues(typeof(TEnum));
            distinctValues = values.Distinct().OrderBy(x => x).ToArray();
            EnumCount = distinctValues.Length;
        }

        /// <summary>
        /// Gets the number of distinct values in the enumeration.
        /// </summary>
        public static readonly int EnumCount;

        /// <summary>
        /// Enumerates all distinct values in <typeparamref name="TEnum"/>.
        /// </summary>
        public static IEnumerable<TEnum> AllValues => distinctValues.Enumerate();
    }
}

/*********************************************************************************
 * EnumIndexedArray.cs
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
using System.Linq;

namespace Sandra
{
    /// <summary>
    /// Contains an array which is indexed by an enumeration.
    /// Always initialize such an array with the <see cref="New"/> method, e.g.:
    /// <code>
    /// var array = EnumIndexedArray&lt;StringComparison, StringComparer&gt;.New();
    /// </code>
    /// </summary>
    /// <remarks>
    /// Declaring an <see cref="EnumIndexedArray{TEnum, TValue}"/> with a non-enumeration key type
    /// results in a <see cref="TypeInitializationException"/> being thrown.
    /// </remarks>
    public struct EnumIndexedArray<TEnum, TValue> where TEnum : struct
    {
        static EnumIndexedArray()
        {
            // Examine the enumeration.
            TEnum[] values = EnumHelper<TEnum>.AllValues.ToArray();
            for (int i = values.Length - 1; i >= 0; --i)
            {
                if ((int)(object)values[i] != i)
                {
                    throw new NotSupportedException("EnumIndexedArray<TEnum, TValue> does not support discontinuous enumerations, or enumerations that have a non-zero lower bound.");
                }
            }
        }

        private TValue[] arr;

        private void init()
        {
            if (arr == null) arr = new TValue[Length];
        }

        /// <summary>
        /// Initializes an empty array with default values.
        /// </summary>
        public static EnumIndexedArray<TEnum, TValue> New()
        {
            EnumIndexedArray<TEnum, TValue> wrapped = default(EnumIndexedArray<TEnum, TValue>);
            wrapped.init();
            return wrapped;
        }

        /// <summary>
        /// Gets the number of keys in the array, which is equal to the number of members in the enumeration.
        /// </summary>
        public int Length => EnumHelper<TEnum>.EnumCount;

        public TValue this[TEnum index]
        {
            get
            {
                try
                {
                    return arr[(int)(object)index];
                }
                catch (NullReferenceException)
                {
                    init();
                    return arr[(int)(object)index];
                }
            }
            set
            {
                try
                {
                    arr[(int)(object)index] = value;
                }
                catch (NullReferenceException)
                {
                    init();
                    arr[(int)(object)index] = value;
                }
            }
        }

        /// <summary>
        /// Creates a copy of this array and returns it.
        /// </summary>
        public EnumIndexedArray<TEnum, TValue> Copy()
        {
            var copy = New();
            try
            {
                Array.Copy(arr, copy.arr, Length);
                return copy;
            }
            catch (NullReferenceException)
            {
                init();
                Array.Copy(arr, copy.arr, Length);
                return copy;
            }
        }
    }
}

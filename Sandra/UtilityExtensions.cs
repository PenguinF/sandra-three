/*********************************************************************************
 * UtilityExtensions.cs
 * 
 * Copyright (c) 2004-2017 Henk Nicolai
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

namespace Sandra
{
    /// <summary>
    /// Contains utility extension methods.
    /// </summary>
    public static class UtilityExtensions
    {
        /// <summary>
        /// Sets a single value at each index of the array.
        /// </summary>
        public static void Fill<T>(this T[] array, T value)
        {
            for (int i = array.Length - 1; i >= 0; --i)
            {
                array[i] = value;
            }
        }

        /// <summary>
        /// Iterates an action a number of times.
        /// If the number of iterations is zero or lower, the action won't get executed at all.
        /// </summary>
        public static void Times(this int numberOfIterations, Action action)
        {
            for (int i = numberOfIterations; i > 0; --i) action();
        }
    }
}

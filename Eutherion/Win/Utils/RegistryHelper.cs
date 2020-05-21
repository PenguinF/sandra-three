#region License
/*********************************************************************************
 * RegistryHelper.cs
 *
 * Copyright (c) 2004-2020 Henk Nicolai
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

using Microsoft.Win32;
using System;
using System.IO;
using System.Security;

namespace Eutherion.Win.Utils
{
    /// <summary>
    /// Contains utility methods to interface with the Windows registry.
    /// </summary>
    public static class RegistryHelper
    {
        /// <summary>
        /// Retrieves a value of a specific type from the registry.
        /// </summary>
        /// <typeparam name="TResult">
        /// The expected type of value in the registry.
        /// </typeparam>
        /// <param name="registryKey">
        /// The registry key in which to look for the sub key.
        /// </param>
        /// <param name="subKey">
        /// The path to the sub key with the registry value.
        /// </param>
        /// <param name="valueName">
        /// The name of the registry value.
        /// </param>
        /// <returns>
        /// The registry value, if it exists and is of the expected type, otherwise <see cref="Maybe{TResult}.Nothing"/>.
        /// </returns>
        public static Maybe<TResult> GetRegistryValue<TResult>(RegistryKey registryKey, string subKey, string valueName)
        {
            try
            {
                using (RegistryKey key = registryKey.OpenSubKey(subKey))
                {
                    if (key != null && key.GetValue(valueName) is TResult value)
                    {
                        return Maybe<TResult>.Just(value);
                    }
                }
            }
            catch (SecurityException) { }
            catch (UnauthorizedAccessException) { }
            catch (IOException) { }

            return Maybe<TResult>.Nothing;
        }

        /// <summary>
        /// Retrieves a value of a specific type from the registry, or a default value if it does not exist.
        /// </summary>
        /// <typeparam name="TResult">
        /// The expected type of value in the registry.
        /// </typeparam>
        /// <param name="registryKey">
        /// The registry key in which to look for the sub key.
        /// </param>
        /// <param name="subKey">
        /// The path to the sub key with the registry value.
        /// </param>
        /// <param name="valueName">
        /// The name of the registry value.
        /// </param>
        /// <param name="defaultValue">
        /// The default value in case the registry value does not exist or is not of the expected type.
        /// </param>
        /// <returns>
        /// The registry value, if it exists and is of the expected type, otherwise <paramref name="defaultValue"/>.
        /// </returns>
        public static TResult GetRegistryValue<TResult>(RegistryKey registryKey, string subKey, string valueName, TResult defaultValue)
            => GetRegistryValue<TResult>(registryKey, subKey, valueName).Match(
                whenNothing: () => defaultValue,
                whenJust: value => value);
    }
}

#region License
/*********************************************************************************
 * PropertyStore.cs
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

using System.Collections.Generic;

namespace Sandra.UI
{
    /// <summary>
    /// Helper class which stores values for a fixed set of properties.
    /// </summary>
    public class PropertyStore : Dictionary<string, object>
    {
        /// <summary>
        /// Gets a value from the store.
        /// </summary>
        /// <typeparam name="T">
        /// Expected type of the stored value.
        /// </typeparam>
        /// <param name="propertyKey">
        /// Key under which the value is stored.
        /// </param>
        public T Get<T>(string propertyKey)
        {
            return (T)base[propertyKey];
        }

        /// <summary>
        /// Updates a value in the store and returns if the value changed.
        /// </summary>
        /// <typeparam name="T">
        /// Expected type of the stored value.
        /// </typeparam>
        /// <param name="propertyKey">
        /// Key under which the value is stored.
        /// </param>
        /// <param name="value">
        /// The new value to store for the property key.
        /// </param>
        /// <returns>
        /// True if the value changed, otherwise false.
        /// </returns>
        public bool Set<T>(string propertyKey, T value)
        {
            T oldValue = Get<T>(propertyKey);
            if (!EqualityComparer<T>.Default.Equals(oldValue, value))
            {
                base[propertyKey] = value;
                return true;
            }
            return false;
        }
    }
}

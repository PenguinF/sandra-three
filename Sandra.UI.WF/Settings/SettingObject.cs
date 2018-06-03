/*********************************************************************************
 * SettingObject.cs
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

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents a read-only collection of setting values (<see cref="PValue"/>) indexed by <see cref="SettingKey"/>.
    /// </summary>
    public sealed class SettingObject
    {
        /// <summary>
        /// Gets the schema for this <see cref="SettingObject"/>.
        /// </summary>
        public readonly SettingSchema Schema;

        internal readonly PMap Map;

        internal SettingObject(SettingCopy workingCopy)
        {
            Schema = workingCopy.Schema;
            Map = workingCopy.ToPMap();
        }

        /// <summary>
        /// Gets the value that is associated with the specified property.
        /// </summary>
        /// <param name="property">
        /// The property to locate.
        /// </param>
        /// <param name="value">
        /// When this method returns, contains the value associated with the specified property,
        /// if the property is found and its value is of the correct <see cref="PType"/>;
        /// otherwise, the default <see cref="PValue"/> value.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// true if this <see cref="SettingObject"/> contains a value of the correct <see cref="PType"/>
        /// for the specified property; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="property"/> is null.
        /// </exception>
        public bool TryGetValue<TValue>(SettingProperty<TValue> property, out TValue value)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            PValue pValue;
            if (Map.TryGetValue(property.Name.Key, out pValue)
                && property.TryGetValidValue(pValue, out value))
            {
                return true;
            }
            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Gets the value that is associated with the specified property.
        /// </summary>
        /// <param name="property">
        /// The property to locate.
        /// </param>
        /// <returns>
        /// The value associated with the specified property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="property"/> is null.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        /// The property does not exist.
        /// </exception>
        public TValue GetValue<TValue>(SettingProperty<TValue> property)
        {
            TValue value;
            if (!TryGetValue(property, out value))
            {
                throw new KeyNotFoundException($"Key {property.Name} does not exist in the {nameof(SettingObject)}.");
            }
            return value;
        }

        /// <summary>
        /// Creates a working <see cref="SettingCopy"/> based on this <see cref="SettingObject"/>.
        /// </summary>
        public SettingCopy CreateWorkingCopy()
        {
            var copy = new SettingCopy(Schema);
            copy.Revert(this);
            return copy;
        }
    }
}

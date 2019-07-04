#region License
/*********************************************************************************
 * SettingObject.cs
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

using System;
using System.Collections.Generic;

namespace Eutherion.Win.Storage
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

        internal SettingObject(SettingSchema schema, PMap map)
        {
            Schema = schema;
            Map = map;
        }

        internal SettingObject(SettingCopy workingCopy)
            : this(workingCopy.Schema, workingCopy.ToPMap())
        {
        }

        /// <summary>
        /// Gets the <see cref="PValue"/> that is associated with the specified property.
        /// </summary>
        /// <param name="property">
        /// The property to locate.
        /// </param>
        /// <param name="value">
        /// When this method returns, contains the value associated with the specified property,
        /// if the property is found; otherwise, the default <see cref="PValue"/> value.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// true if this <see cref="SettingObject"/> contains a value for the specified property;
        /// otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="property"/> is null.
        /// </exception>
        public bool TryGetRawValue(SettingProperty property, out PValue value)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            if (Schema.ContainsProperty(property)
                && Map.TryGetValue(property.Name.Key, out value))
            {
                return true;
            }
            value = default;
            return false;
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
            if (TryGetRawValue(property, out PValue pValue)
                && property.PType.TryConvert(pValue).IsJust(out value))
            {
                return true;
            }
            value = default;
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
        /// <exception cref="ArgumentException">
        /// The value associated with the specified property is not of the target type.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        /// The property does not exist.
        /// </exception>
        public TValue GetValue<TValue>(SettingProperty<TValue> property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            if (!Map.ContainsKey(property.Name.Key))
            {
                throw new KeyNotFoundException($"Key {property.Name} does not exist in the {nameof(SettingObject)}.");
            }

            if (!TryGetValue(property, out TValue value))
            {
                throw new ArgumentException($"The value of {property.Name} is not of the target type {typeof(TValue).FullName}.");
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

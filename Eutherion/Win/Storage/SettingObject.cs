#region License
/*********************************************************************************
 * SettingObject.cs
 *
 * Copyright (c) 2004-2023 Henk Nicolai
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
    /// Represents a read-only collection of type checked JSON values indexed by property key
    /// (<see cref="StringKey{T}"/> of <see cref="SettingProperty"/>).
    /// </summary>
    public sealed class SettingObject
    {
        /// <summary>
        /// Creates an empty <see cref="SettingObject"/> with a given schema.
        /// </summary>
        /// <param name="schema">
        /// The <see cref="SettingSchema"/> to use.
        /// </param>
        /// <returns>
        /// A <see cref="SettingObject"/> with undefined values for each of the members of the schema.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="schema"/> is <see langword="null"/>.
        /// </exception>
        public static SettingObject CreateEmpty(SettingSchema schema) => new SettingCopy(schema).Commit();

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
        /// Gets the value that is associated with the specified property.
        /// </summary>
        /// <param name="property">
        /// The property to locate.
        /// </param>
        /// <param name="value">
        /// When this method returns, contains the value associated with the specified property,
        /// if the property is found; otherwise, the default value.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if this <see cref="SettingObject"/> contains a value for the specified property;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="property"/> is <see langword="null"/>.
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
        /// <see langword="true"/> if this <see cref="SettingObject"/> contains a value of the correct <see cref="PType"/>
        /// for the specified property; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="property"/> is <see langword="null"/>.
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
        /// Creates a copy of this <see cref="SettingObject"/> with an added or replaced value associated with a property.
        /// </summary>
        /// <typeparam name="TValue">
        /// The target .NET type of the property.
        /// </typeparam>
        /// <param name="property">
        /// The property for which to add or replace the value.
        /// </param>
        /// <param name="value">
        /// The new value to associate with the property.
        /// </param>
        /// <returns>
        /// The new <see cref="SettingObject"/> with the requested change.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="property"/> and/or <paramref name="value"/> are <see langword="null"/>.
        /// </exception>
        public SettingObject Set<TValue>(SettingProperty<TValue> property, TValue value)
        {
            SettingCopy workingCopy = CreateWorkingCopy();
            workingCopy.Set(property, value);
            return workingCopy.Commit();
        }

        /// <summary>
        /// Creates a copy of this <see cref="SettingObject"/> with a removed value associated with a property.
        /// </summary>
        /// <typeparam name="TValue">
        /// The target .NET type of the property.
        /// </typeparam>
        /// <param name="property">
        /// The property for which to remove the value.
        /// </param>
        /// <returns>
        /// The new <see cref="SettingObject"/> with the requested change.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="property"/> is <see langword="null"/>.
        /// </exception>
        public SettingObject Unset<TValue>(SettingProperty<TValue> property)
        {
            SettingCopy workingCopy = CreateWorkingCopy();
            workingCopy.Unset(property);
            return workingCopy.Commit();
        }

        /// <summary>
        /// Creates a mutable <see cref="SettingCopy"/> based on this <see cref="SettingObject"/>.
        /// </summary>
        public SettingCopy CreateWorkingCopy()
        {
            Dictionary<string, PValue> keyValueMapping = new Dictionary<string, PValue>();

            // No need to copy values if they can be assumed read-only or are structs.
            foreach (var kv in Map)
            {
                keyValueMapping.Add(kv.Key, kv.Value);
            }

            return new SettingCopy(Schema, keyValueMapping);
        }

        /// <summary>
        /// Converts this <see cref="SettingObject"/> into a <see cref="PMap"/> suitable for serialization to JSON.
        /// </summary>
        public PMap ConvertToMap() => Schema.ConvertToPMap(this);
    }
}

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
    /// (<see cref="StringKey{T}"/> of <see cref="SettingSchema.Member"/>).
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
        public SettingSchema Schema { get; }

        /// <summary>
        /// Gets the mapping between keys and values.
        /// </summary>
        internal readonly Dictionary<string, object> KeyValueMapping;

        internal SettingObject(SettingSchema schema, Dictionary<string, object> keyValueMapping)
        {
            Schema = schema;
            KeyValueMapping = keyValueMapping;
        }

        /// <summary>
        /// Returns if this <see cref="SettingObject"/> contains a defined value for a given <see cref="SettingProperty"/>.
        /// </summary>
        /// <param name="member">
        /// The <see cref="SettingSchema.Member"/> to check.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if this <see cref="SettingObject"/> contains a defined value for the given member;
        /// otherwise <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="member"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="SchemaMismatchException">
        /// <paramref name="member"/> has a different schema.
        /// </exception>
        public bool IsSet(SettingSchema.Member member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));

            Schema.ThrowIfNonMatchingSchema(member.OwnerSchema);
            return KeyValueMapping.ContainsKey(member.Name.Key);
        }

        internal bool TryGetUntypedValue(SettingSchema.Member member, out object value)
        {
            // This ensures basic security that the passed in property also performed the type-check.
            Schema.ThrowIfNonMatchingSchema(member.OwnerSchema);

            if (KeyValueMapping.TryGetValue(member.Name.Key, out value))
            {
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Gets the value that is associated with the specified member.
        /// </summary>
        /// <param name="member">
        /// The member to locate.
        /// </param>
        /// <param name="value">
        /// When this method returns, contains the value associated with the specified member,
        /// if it is defined for the given member; otherwise, the default value.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if this <see cref="SettingObject"/> contains a defined value for the given member;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="member"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="SchemaMismatchException">
        /// <paramref name="member"/> has a different schema.
        /// </exception>
        public bool TryGetValue<TValue>(SettingSchema.Member<TValue> member, out TValue value)
        {
            if (TryGetUntypedValue(member, out object untypedValue))
            {
                value = (TValue)untypedValue;
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
        /// otherwise, the default value.
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
            if (Schema.TryGetMember(property.Name, out SettingSchema.Member member)
                && member is SettingSchema.Member<TValue> typedMember
                && typedMember.PType == property.PType
                && TryGetValue(typedMember, out value))
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
        public SettingCopy CreateWorkingCopy() => new SettingCopy(Schema, new Dictionary<string, object>(KeyValueMapping));

        /// <summary>
        /// Converts this <see cref="SettingObject"/> into a <see cref="PMap"/> suitable for serialization to JSON.
        /// </summary>
        public PMap ConvertToMap() => Schema.ConvertToPMap(this);
    }
}

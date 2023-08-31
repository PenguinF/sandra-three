﻿#region License
/*********************************************************************************
 * SettingSchema.cs
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

using Eutherion.Text.Json;
using System;
using System.Collections.Generic;

namespace Eutherion.Win.Storage
{
    public class SettingSchema : PType.MapBase<SettingObject>
    {
        /// <summary>
        /// Contains the declaration of a setting property, but doesn't specify its type.
        /// </summary>
        public abstract class Member
        {
            /// <summary>
            /// Gets the schema that owns this member.
            /// </summary>
            public SettingSchema OwnerSchema { get; }

            /// <summary>
            /// Gets the name of this member.
            /// </summary>
            public StringKey<SettingProperty> Name { get; }

            internal Member(SettingSchema ownerSchema, StringKey<SettingProperty> name)
            {
                OwnerSchema = ownerSchema;
                Name = name;
            }

            /// <summary>
            /// Type-checks a JSON value syntax node.
            /// </summary>
            /// <param name="valueNode">
            /// The value node to type-check.
            /// </param>
            /// <param name="errors">
            /// The list of inner errors to which new type errors can be added.
            /// </param>
            /// <returns>
            /// A type error (not added to <paramref name="errors"/>) for this value if the type check failed,
            /// or the converted <see cref="PValue"/> if the type check succeeded.
            /// </returns>
            internal abstract Union<ITypeErrorBuilder, object> TryCreateValue(JsonValueSyntax valueNode, ArrayBuilder<PTypeError> errors);

            internal abstract PValue ConvertToPValue(object untypedValue);
        }

        /// <summary>
        /// Describes a key-value pair in a JSON object.
        /// </summary>
        /// <typeparam name="T">
        /// The .NET target <see cref="Type"/> to convert to and from.
        /// </typeparam>
        public sealed class Member<T> : Member
        {
            /// <summary>
            /// Gets the type of value that it contains.
            /// </summary>
            public PType<T> PType { get; }

            internal Member(SettingSchema ownerSchema, StringKey<SettingProperty> name, PType<T> pType)
                : base(ownerSchema, name)
                => PType = pType;

            internal sealed override Union<ITypeErrorBuilder, object> TryCreateValue(JsonValueSyntax valueNode, ArrayBuilder<PTypeError> errors)
                => PType.TryCreateValue(valueNode, errors).Match(
                    whenOption1: Union<ITypeErrorBuilder, object>.Option1,
                    whenOption2: value => value);

            internal override PValue ConvertToPValue(object untypedValue)
                => PType.ConvertToPValue((T)untypedValue);
        }

        private readonly Dictionary<string, SettingProperty> Members;

        private readonly Dictionary<string, SettingComment> MemberDescriptions;

        /// <summary>
        /// Gets the built-in description of the schema in a settings file.
        /// </summary>
        public SettingComment Description { get; }

        /// <summary>
        /// Initializes a new instance of a <see cref="SettingSchema"/>.
        /// </summary>
        /// <param name="properties">
        /// The set of properties with unique keys to support.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Two or more properties have the same key.
        /// </exception>
        public SettingSchema(params SettingProperty[] properties)
            : this((IEnumerable<SettingProperty>)properties)
        {
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="SettingSchema"/>.
        /// </summary>
        /// <param name="description">
        /// The built-in description of the schema in a settings file.
        /// </param>
        /// <param name="properties">
        /// The set of properties with unique keys to support.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Two or more properties have the same key.
        /// </exception>
        public SettingSchema(SettingComment description, params SettingProperty[] properties)
            : this(properties, description)
        {
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="SettingSchema"/>.
        /// </summary>
        /// <param name="properties">
        /// The set of properties with unique keys to support.
        /// </param>
        /// <param name="description">
        /// The built-in description of the schema in a settings file.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Two or more properties have the same key; or one of the properties is <see langword="null"/>.
        /// </exception>
        public SettingSchema(IEnumerable<SettingProperty> properties, SettingComment description = null)
        {
            this.Members = new Dictionary<string, SettingProperty>();
            MemberDescriptions = new Dictionary<string, SettingComment>();

            if (properties != null)
            {
                foreach (var property in properties)
                {
                    if (property == null) throw new ArgumentException("One of the properties is null.", nameof(properties));
                    this.Members.Add(property.Name.Key, property);
                    if (property.Description != null) MemberDescriptions.Add(property.Name.Key, property.Description);
                }
            }

            Description = description;
        }

        /// <summary>
        /// Gets the <see cref="SettingProperty"/> that is associated with the specified key.
        /// </summary>
        /// <param name="key">
        /// The key to locate.
        /// </param>
        /// <param name="property">
        /// When this method returns, contains the <see cref="SettingProperty"/> associated with the specified key, if the key is found;
        /// otherwise, the default <see cref="SettingProperty"/> value.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if this <see cref="SettingSchema"/> contains a <see cref="SettingProperty"/> with the specified key;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is <see langword="null"/>.
        /// </exception>
        public bool TryGetProperty(StringKey<SettingProperty> key, out SettingProperty property)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            if (Members.TryGetValue(key.Key, out property)) return true;
            property = default;
            return false;
        }

        /// <summary>
        /// Gets if a <see cref="SettingProperty"/> is contained in this schema.
        /// </summary>
        /// <param name="property">
        /// The <see cref="SettingProperty"/> to locate.
        /// </param>
        /// <returns>
        /// Whether or not the property is contained in this schema.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="property"/> is <see langword="null"/>.
        /// </exception>
        public bool ContainsProperty(SettingProperty property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            return Members.TryGetValue(property.Name.Key, out SettingProperty propertyInDictionary)
                && property == propertyInDictionary;
        }

        /// <summary>
        /// Gets the <see cref="SettingComment"/> that is associated with the specified key.
        /// </summary>
        /// <param name="key">
        /// The key to locate.
        /// </param>
        /// <returns>
        /// A <see cref="SettingComment"/> that describes a member, or <see cref="Maybe{T}.Nothing"/> is none was found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is null.
        /// </exception>
        public Maybe<SettingComment> TryGetDescription(StringKey<SettingProperty> key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            if (MemberDescriptions.TryGetValue(key.Key, out SettingComment description))
            {
                return description;
            }

            return Maybe<SettingComment>.Nothing;
        }

        /// <summary>
        /// Enumerates all properties in this schema.
        /// </summary>
        public IEnumerable<SettingProperty> AllProperties => Members.Values;

        internal override Union<ITypeErrorBuilder, SettingObject> TryCreateFromMap(JsonMapSyntax jsonMapSyntax, ArrayBuilder<PTypeError> errors)
        {
            var mapBuilder = new Dictionary<string, object>();

            // Report errors on duplicate keys (case sensitive).
            HashSet<string> foundKeys = new HashSet<string>();

            // Analyze values with this schema while building the PMap.
            foreach (var (keyNode, valueNode) in jsonMapSyntax.DefinedKeyValuePairs())
            {
                if (foundKeys.Contains(keyNode.Value))
                {
                    errors.Add(new DuplicatePropertyKeyWarning(keyNode));
                }
                else
                {
                    foundKeys.Add(keyNode.Value);
                }

                if (TryGetProperty(new StringKey<SettingProperty>(keyNode.Value), out SettingProperty property))
                {
                    var valueOrError = property.CreateSchemaMember(this).TryCreateValue(valueNode, errors);

                    if (valueOrError.IsOption2(out object convertedItemValue))
                    {
                        mapBuilder.Add(keyNode.Value, convertedItemValue);
                    }
                    else
                    {
                        ITypeErrorBuilder typeError = valueOrError.ToOption1();
                        errors.Add(new ValueTypeErrorAtPropertyKey(typeError, keyNode, valueNode));
                    }
                }
                else
                {
                    errors.Add(new UnrecognizedPropertyKeyWarning(keyNode));
                }
            }

            return new SettingObject(this, mapBuilder);
        }

        public override Maybe<SettingObject> TryConvertFromMap(PMap map)
            => throw new Exception();

        public override PMap ConvertToPMap(SettingObject value)
        {
            var mapBuilder = new Dictionary<string, PValue>();

            foreach (var property in value.Schema.AllProperties)
            {
                if (value.KeyValueMapping.TryGetValue(property.Name.Key, out object untypedValue))
                {
                    mapBuilder.Add(property.Name.Key, property.CreateSchemaMember(this).ConvertToPValue(untypedValue));
                }
            }

            return new PMap(mapBuilder);
        }
    }
}

#region License
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
        private readonly Dictionary<string, SettingProperty> properties;

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
        /// Two or more properties have the same key; or one of the properties is null.
        /// </exception>
        public SettingSchema(IEnumerable<SettingProperty> properties, SettingComment description = null)
        {
            this.properties = new Dictionary<string, SettingProperty>();

            if (properties != null)
            {
                foreach (var property in properties)
                {
                    if (property == null) throw new ArgumentException("One of the properties is null.", nameof(properties));
                    this.properties.Add(property.Name.Key, property);
                }
            }

            Description = description;
        }

        /// <summary>
        /// Gets the <see cref="SettingProperty"/> that is associated with the specified key.
        /// </summary>
        /// <param name="settingKey">
        /// The key to locate.
        /// </param>
        /// <param name="property">
        /// When this method returns, contains the <see cref="SettingProperty"/> associated with the specified key, if the key is found;
        /// otherwise, the default <see cref="SettingProperty"/> value.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// true if this <see cref="SettingSchema"/> contains a <see cref="SettingProperty"/> with the specified key; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="settingKey"/> is null.
        /// </exception>
        public bool TryGetProperty(StringKey<SettingProperty> settingKey, out SettingProperty property)
        {
            if (settingKey == null) throw new ArgumentNullException(nameof(settingKey));

            if (properties.TryGetValue(settingKey.Key, out property)) return true;
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
        /// <paramref name="property"/> is null.
        /// </exception>
        public bool ContainsProperty(SettingProperty property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            return properties.TryGetValue(property.Name.Key, out SettingProperty propertyInDictionary)
                && property == propertyInDictionary;
        }

        /// <summary>
        /// Enumerates all properties in this schema.
        /// </summary>
        public IEnumerable<SettingProperty> AllProperties => properties.Values;

        internal override Union<ITypeErrorBuilder, PValue> TryCreateFromMap(
            JsonMapSyntax jsonMapSyntax,
            out SettingObject convertedValue,
            ArrayBuilder<PTypeError> errors)
        {
            var mapBuilder = new Dictionary<string, PValue>();

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
                    var valueOrError = property.TryCreateValue(valueNode, errors);

                    if (valueOrError.IsOption2(out PValue convertedItemValue))
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

            var map = new PMap(mapBuilder);
            convertedValue = new SettingObject(this, map);
            return map;
        }

        public override Maybe<SettingObject> TryConvertFromMap(PMap map)
            => new SettingObject(this, map);

        public override PMap ConvertToPMap(SettingObject value)
            => value.Map;
    }
}

#region License
/*********************************************************************************
 * SettingCopy.cs
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

using Eutherion.Text.Json;
using System;
using System.Collections.Generic;

namespace Eutherion.Win.Storage
{
    /// <summary>
    /// Represents the mutable working copy of a <see cref="SettingObject"/>.
    /// </summary>
    public class SettingCopy
    {
        /// <summary>
        /// Gets the schema for this <see cref="SettingCopy"/>.
        /// </summary>
        public readonly SettingSchema Schema;

        /// <summary>
        /// Gets the mutable mapping between keys and values.
        /// </summary>
        private readonly Dictionary<SettingKey, PValue> KeyValueMapping;

        /// <summary>
        /// Initializes a new instance of <see cref="SettingCopy"/>.
        /// </summary>
        /// <param name="schema">
        /// The schema to use.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="schema"/> is null.
        /// </exception>
        public SettingCopy(SettingSchema schema)
        {
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            KeyValueMapping = new Dictionary<SettingKey, PValue>();
        }

        /// <summary>
        /// Adds or replaces a value associated with a property.
        /// </summary>
        /// <param name="property">
        /// The property for which to add or replace the value.
        /// </param>
        /// <param name="value">
        /// The new value to associate with the property.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="property"/> and/or <paramref name="value"/> are null.
        /// </exception>
        public void AddOrReplace<TValue>(SettingProperty<TValue> property, TValue value)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (value == null) throw new ArgumentNullException(nameof(value));

            AddOrReplaceRaw(property, property.PType.GetPValue(value));
        }

        /// <summary>
        /// Adds or replaces a value from a source <see cref="SettingObject"/> with a different schema.
        /// </summary>
        /// <param name="property">
        /// The property for which to add or replace the value.
        /// </param>
        /// <param name="value">
        /// The new value to associate with the property.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="property"/> and/or <paramref name="value"/> are null.
        /// </exception>
        public void AddOrReplaceRaw(SettingProperty property, PValue value)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (Schema.ContainsProperty(property) && property.IsValidValue(value))
            {
                KeyValueMapping[property.Name] = value;
            }
        }

        /// <summary>
        /// Removes a value associated with a property.
        /// </summary>
        /// <param name="property">
        /// The property for which to remove the value.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="property"/> is null.
        /// </exception>
        public void Remove<TValue>(SettingProperty<TValue> property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            if (Schema.ContainsProperty(property))
            {
                KeyValueMapping.Remove(property.Name);
            }
        }

        /// <summary>
        /// Reverts to the state of a <see cref="SettingObject"/>.
        /// </summary>
        /// <param name="settingObject">
        /// The <see cref="SettingObject"/> to revert to.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="settingObject"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="settingObject"/> does not have the same schema.
        /// </exception>
        public void Revert(SettingObject settingObject)
        {
            if (settingObject == null) throw new ArgumentNullException(nameof(settingObject));
            if (settingObject.Schema != Schema) throw new ArgumentException($"Cannot revert to a {nameof(SettingObject)} with a different schema.");

            // Clear out the mapping before copying key-value pairs.
            KeyValueMapping.Clear();

            // No need to copy values if they can be assumed read-only or are structs.
            foreach (var kv in settingObject.Map)
            {
                KeyValueMapping.Add(new SettingKey(kv.Key), kv.Value);
            }
        }

        /// <summary>
        /// Commits this working <see cref="SettingCopy"/> to a new <see cref="SettingObject"/>.
        /// </summary>
        public SettingObject Commit() => new SettingObject(this);

        /// <summary>
        /// Compares this <see cref="SettingCopy"/> with a <see cref="SettingObject"/> and returns if they are equal.
        /// </summary>
        /// <param name="other">
        /// The <see cref="SettingObject"/> to compare with.
        /// </param>
        /// <returns>
        /// true if both <see cref="SettingObject"/> instances are equal; otherwise false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="other"/> is null.
        /// </exception>
        /// <remarks>
        /// This is not the same as complete equality, in particular this method returns true from the following expression:
        /// <code>
        /// workingCopy.Commit().EqualTo(workingCopy)
        /// </code>
        /// where workingCopy is a <see cref="SettingCopy"/>. Or even:
        /// <code>
        /// workingCopy.Commit().CreateWorkingCopy().EqualTo(workingCopy)
        /// </code>
        /// </remarks>
        public bool EqualTo(SettingObject other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            return ToPMap().EqualTo(other.Map);
        }

        internal PMap ToPMap()
        {
            Dictionary<string, PValue> mapBuilder = new Dictionary<string, PValue>();
            foreach (var kv in KeyValueMapping) mapBuilder.Add(kv.Key.Key, kv.Value);
            return new PMap(mapBuilder);
        }

        /// <summary>
        /// Loads settings from text.
        /// </summary>
        internal List<JsonErrorInfo> TryLoadFromText(string json)
        {
            var parser = new SettingReader(json);

            if (parser.TryParse(Schema, out PMap map, out List<JsonErrorInfo> errors))
            {
                foreach (var kv in map)
                {
                    if (Schema.TryGetProperty(new SettingKey(kv.Key), out SettingProperty property))
                    {
                        AddOrReplaceRaw(property, kv.Value);
                    }
                }
            }

            return errors;
        }
    }
}

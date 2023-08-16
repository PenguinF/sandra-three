#region License
/*********************************************************************************
 * SettingProperty.cs
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
    /// <summary>
    /// Contains the declaration of a setting property, but doesn't specify its type.
    /// </summary>
    public abstract class SettingProperty
    {
        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public StringKey<SettingProperty> Name { get; }

        /// <summary>
        /// Gets the built-in description of the property in a settings file.
        /// </summary>
        public SettingComment Description { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="SettingProperty"/>.
        /// </summary>
        /// <param name="name">
        /// The name of the property.
        /// </param>
        /// <param name="description">
        /// The built-in description of the property in a settings file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> is null.
        /// </exception>
        public SettingProperty(StringKey<SettingProperty> name, SettingComment description)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
        }

        /// <summary>
        /// Returns if a raw <see cref="PValue"/> can be converted to the target .NET type of this property.
        /// </summary>
        /// <param name="value">
        /// The value to convert from.
        /// </param>
        /// <returns>
        /// Whether or not conversion will succeed.
        /// </returns>
        public abstract bool IsValidValue(PValue value);

        /// <summary>
        /// Type-checks a context free value syntax while parsing json.
        /// Parameters other than <paramref name="valueNode"/> are given for type error construction.
        /// </summary>
        /// <param name="json">
        /// The parsed json.
        /// </param>
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
        internal abstract Union<ITypeErrorBuilder, PValue> TryCreateValue(
            string json,
            JsonValueSyntax valueNode,
            ArrayBuilder<PTypeError> errors);
    }

    /// <summary>
    /// Contains the declaration of a setting property, i.e. its name and the type of value that it takes.
    /// </summary>
    /// <typeparam name="T">
    /// The .NET target <see cref="Type"/> to convert to and from.
    /// </typeparam>
    public class SettingProperty<T> : SettingProperty
    {
        /// <summary>
        /// Gets the type of value that it contains.
        /// </summary>
        public PType<T> PType { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="SettingProperty"/>.
        /// </summary>
        /// <param name="name">
        /// The name of the property.
        /// </param>
        /// <param name="pType">
        /// The type of value that it contains.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> and/or <paramref name="pType"/> are null.
        /// </exception>
        public SettingProperty(StringKey<SettingProperty> name, PType<T> pType) : this(name, pType, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SettingProperty"/>.
        /// </summary>
        /// <param name="name">
        /// The name of the property.
        /// </param>
        /// <param name="pType">
        /// The type of value that it contains.
        /// </param>
        /// <param name="description">
        /// The built-in description of the property in a settings file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> and/or <paramref name="pType"/> are null.
        /// </exception>
        public SettingProperty(StringKey<SettingProperty> name, PType<T> pType, SettingComment description) : base(name, description)
        {
            PType = pType ?? throw new ArgumentNullException(nameof(pType));
        }

        public sealed override bool IsValidValue(PValue value)
            => !PType.TryConvert(value).IsNothing;

        internal sealed override Union<ITypeErrorBuilder, PValue> TryCreateValue(
            string json,
            JsonValueSyntax valueNode,
            ArrayBuilder<PTypeError> errors)
            => PType.TryCreateValue(json, valueNode, out _, errors);
    }
}

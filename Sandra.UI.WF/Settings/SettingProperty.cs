/*********************************************************************************
 * SettingProperty.cs
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

namespace Sandra.UI.WF
{
    /// <summary>
    /// Contains the declaration of a setting property, i.e. its name and the type of value that it takes.
    /// </summary>
    /// <typeparam name="T">
    /// The .NET target <see cref="Type"/> to convert to and from.
    /// </typeparam>
    public class SettingProperty<T>
    {
        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public SettingKey Name { get; }

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
        public SettingProperty(SettingKey name, PType<T> pType)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (pType == null) throw new ArgumentNullException(nameof(pType));

            Name = name;
            PType = pType;
        }
    }
}

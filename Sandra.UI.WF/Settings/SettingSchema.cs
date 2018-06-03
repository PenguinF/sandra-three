/*********************************************************************************
 * SettingSchema.cs
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
using System.Collections.Generic;
using System.Linq;

namespace Sandra.UI.WF
{
    public class SettingSchema
    {
        // Prevent repeated allocations of empty dictionaries.
        private static readonly Dictionary<string, SettingProperty> emptyProperties = new Dictionary<string, SettingProperty>();

        /// <summary>
        /// Represents the empty <see cref="SettingSchema"/>, which contains no properties.
        /// </summary>
        public static readonly SettingSchema Empty = new SettingSchema(null);

        private readonly Dictionary<string, SettingProperty> properties;

        /// <summary>
        /// Initializes a new instance of a <see cref="SettingSchema"/>.
        /// </summary>
        /// <param name="properties">
        /// The set of properties with unique keys to support.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// Two or more properties have the same key.
        /// </exception>
        public SettingSchema(IEnumerable<SettingProperty> properties)
        {
            this.properties = properties != null && properties.Any()
                ? new Dictionary<string, SettingProperty>()
                : emptyProperties;

            if (properties != null)
            {
                foreach (var property in properties)
                {
                    this.properties.Add(property.Name.Key, property);
                }
            }
        }
    }
}

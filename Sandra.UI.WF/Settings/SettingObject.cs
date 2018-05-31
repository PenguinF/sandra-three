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

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents a read-only collection of setting values (<see cref="PValue"/>) indexed by <see cref="SettingKey"/>.
    /// </summary>
    public class SettingObject
    {
        internal readonly PMap Map;

        internal SettingObject(SettingCopy workingCopy)
        {
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
                && property.PType.TryGetValidValue(pValue, out value))
            {
                return true;
            }
            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Creates a working <see cref="SettingCopy"/> based on this <see cref="SettingObject"/>.
        /// </summary>
        public SettingCopy CreateWorkingCopy()
        {
            var copy = new SettingCopy();
            copy.Revert(this);
            return copy;
        }
    }
}

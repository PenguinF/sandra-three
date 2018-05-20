/*********************************************************************************
 * SettingCopy.cs
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
using System.Collections.Generic;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents the mutable working copy of a <see cref="SettingObject"/>.
    /// </summary>
    public class SettingCopy : SettingObject
    {
        /// <summary>
        /// Gets the mutable mapping between keys and values.
        /// </summary>
        public Dictionary<SettingKey, ISettingValue> KeyValueMapping => Mapping;

        /// <summary>
        /// Reverts to the state of a <see cref="SettingObject"/>.
        /// </summary>
        /// <param name="settingObject">
        /// The <see cref="SettingObject"/> to revert to.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="settingObject"/> is null.
        /// </exception>
        public void Revert(SettingObject settingObject)
        {
            if (settingObject == null) throw new ArgumentNullException(nameof(settingObject));

            // Clear out the mapping before copying key-value pairs.
            KeyValueMapping.Clear();

            // No need to copy values if they can be assumed read-only or are structs.
            foreach (var kv in settingObject)
            {
                KeyValueMapping.Add(kv.Key, kv.Value);
            }
        }
    }
}

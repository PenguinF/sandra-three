/*********************************************************************************
 * SettingUpdateOperation.cs
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

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents an update operation to a setting object.
    /// </summary>
    public class SettingUpdateOperation
    {
        /// <summary>
        /// Dictionary of settings to update.
        /// </summary>
        private readonly Dictionary<string, ISettingValue> updates = new Dictionary<string, ISettingValue>();

        /// <summary>
        /// Registers that the value for a setting key must be added or replaced.
        /// </summary>
        /// <param name="settingKey">
        /// The key of the setting to add or replace.
        /// </param>
        /// <param name="value">
        /// The new value of the setting to add or replace.
        /// </param>
        /// <returns>
        /// This instance. See also: https://en.wikipedia.org/wiki/Builder_pattern
        /// </returns>
        public SettingUpdateOperation AddOrReplace(string settingKey, bool value)
        {
            updates[settingKey] = new BooleanSettingValue() { Value = value };
            return this;
        }

        /// <summary>
        /// Registers that the value for a setting key must be added or replaced.
        /// </summary>
        /// <param name="settingKey">
        /// The key of the setting to add or replace.
        /// </param>
        /// <param name="value">
        /// The new value of the setting to add or replace.
        /// </param>
        /// <returns>
        /// This instance. See also: https://en.wikipedia.org/wiki/Builder_pattern
        /// </returns>
        public SettingUpdateOperation AddOrReplace(string settingKey, int value)
        {
            updates[settingKey] = new Int32SettingValue() { Value = value };
            return this;
        }

        /// <summary>
        /// Commits and persists the update operation.
        /// </summary>
        public void Persist()
        {
        }
    }
}

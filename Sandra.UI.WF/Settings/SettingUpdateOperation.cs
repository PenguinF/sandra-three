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
namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents an update operation to a persisted <see cref="SettingObject"/>.
    /// </summary>
    public class SettingUpdateOperation
    {
        private readonly AutoSave owner;

        /// <summary>
        /// Dictionary of settings to update.
        /// </summary>
        internal readonly SettingCopy WorkingCopy;

        internal SettingUpdateOperation(AutoSave owner)
        {
            this.owner = owner;
            WorkingCopy = owner.CurrentSettings.CreateWorkingCopy();
        }

        private SettingUpdateOperation AddOrReplace(SettingKey settingKey, ISettingValue value)
        {
            WorkingCopy.KeyValueMapping[settingKey] = value;
            return this;
        }

        /// <summary>
        /// Adds or replaces a value for a setting key.
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
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="settingKey"/> is null.
        /// </exception>
        public SettingUpdateOperation AddOrReplace(SettingKey settingKey, bool value)
            => AddOrReplace(settingKey, new BooleanSettingValue() { Value = value });

        /// <summary>
        /// Adds or replaces a value for a setting key.
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
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="settingKey"/> is null.
        /// </exception>
        public SettingUpdateOperation AddOrReplace(SettingKey settingKey, int value)
            => AddOrReplace(settingKey, new Int32SettingValue() { Value = value });

        /// <summary>
        /// Removes a key and its associated value.
        /// </summary>
        /// <param name="settingKey">
        /// The key of the setting to remove.
        /// </param>
        /// <returns>
        /// This instance. See also: https://en.wikipedia.org/wiki/Builder_pattern
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="settingKey"/> is null.
        /// </exception>
        public SettingUpdateOperation Remove(SettingKey settingKey)
        {
            WorkingCopy.KeyValueMapping.Remove(settingKey);
            return this;
        }

        /// <summary>
        /// Commits and persists the update operation.
        /// </summary>
        public void Persist()
        {
            owner.Persist(WorkingCopy);
        }
    }
}

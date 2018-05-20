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
            WorkingCopy.Mapping[settingKey] = value;
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
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="settingKey"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="settingKey"/> is <see cref="string.Empty"/>, or contains a double quote character (").
        /// </exception>
        public SettingUpdateOperation AddOrReplace(string settingKey, bool value)
            => AddOrReplace(new SettingKey(settingKey), new BooleanSettingValue() { Value = value });

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
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="settingKey"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="settingKey"/> is <see cref="string.Empty"/>, or contains a double quote character (").
        /// </exception>
        public SettingUpdateOperation AddOrReplace(string settingKey, int value)
            => AddOrReplace(new SettingKey(settingKey), new Int32SettingValue() { Value = value });

        /// <summary>
        /// Commits and persists the update operation.
        /// </summary>
        public void Persist()
        {
            owner.Persist(this);
        }
    }
}

#region License
/*********************************************************************************
 * Localizer.cs
 *
 * Copyright (c) 2004-2021 Henk Nicolai
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

namespace Eutherion.Localization
{
    /// <summary>
    /// Defines an abstract method to generate a localized string given a <see cref="LocalizedStringKey"/>.
    /// </summary>
    public abstract class Localizer
    {
        /// <summary>
        /// Generates a localized string given a <see cref="LocalizedStringKey"/>.
        /// </summary>
        public string Localize(LocalizedStringKey localizedStringKey) => Localize(localizedStringKey, null);

        /// <summary>
        /// Generates and formats a localized string given a <see cref="LocalizedStringKey"/> and an array of parameters.
        /// </summary>
        public abstract string Localize(LocalizedStringKey localizedStringKey, string[] parameters);

        private sealed class DefaultLocalizer : Localizer
        {
            public override string Localize(LocalizedStringKey localizedStringKey, string[] parameters)
            {
                if (localizedStringKey == null) return null;
                return "{" + localizedStringKey.Key + StringUtilities.ToDefaultParameterListDisplayString(parameters) + "}";
            }
        }

        /// <summary>
        /// Gets a reference to the default <see cref="Localizer"/>,
        /// which provides a default localized string for each <see cref="LocalizedStringKey"/>.
        /// </summary>
        public static readonly Localizer Default = new DefaultLocalizer();
    }
}

#region License
/*********************************************************************************
 * BuiltInEnglishLocalizer.cs
 *
 * Copyright (c) 2004-2020 Henk Nicolai
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

using Eutherion.Localization;
using Eutherion.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Eutherion.Win.AppTemplate
{
    public sealed class BuiltInEnglishLocalizer : Localizer
    {
        public readonly Dictionary<LocalizedStringKey, string> Dictionary;

        public override string Localize(LocalizedStringKey localizedStringKey, string[] parameters)
            => Dictionary.TryGetValue(localizedStringKey, out string displayText)
            ? StringUtilities.ConditionalFormat(displayText, parameters)
            : Default.Localize(localizedStringKey, parameters);

        public BuiltInEnglishLocalizer(params IEnumerable<KeyValuePair<LocalizedStringKey, string>>[] subDictionaries)
        {
            Dictionary = new Dictionary<LocalizedStringKey, string>();

            if (subDictionaries != null)
            {
                subDictionaries.ForEach(kv => kv.ForEach(Dictionary.Add));
            }
        }
    }
}

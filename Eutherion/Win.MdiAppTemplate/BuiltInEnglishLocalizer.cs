#region License
/*********************************************************************************
 * BuiltInEnglishLocalizer.cs
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

using Eutherion.Text;
using System.Collections.Generic;
using System.Linq;

namespace Eutherion.Win.MdiAppTemplate
{
    public sealed class BuiltInEnglishLocalizer : TextFormatter
    {
        public readonly Dictionary<StringKey<ForFormattedText>, string> Dictionary;

        public override string Format(StringKey<ForFormattedText> localizedStringKey, string[] parameters)
            => Dictionary.TryGetValue(localizedStringKey, out string displayText)
            ? FormatUtilities.SoftFormat(displayText, parameters)
            : Default.Format(localizedStringKey, parameters);

        public BuiltInEnglishLocalizer(params IEnumerable<KeyValuePair<StringKey<ForFormattedText>, string>>[] subDictionaries)
        {
            Dictionary = new Dictionary<StringKey<ForFormattedText>, string>();

            if (subDictionaries != null)
            {
                subDictionaries.ForEach(kv => kv.ForEach(Dictionary.Add));
            }
        }
    }
}

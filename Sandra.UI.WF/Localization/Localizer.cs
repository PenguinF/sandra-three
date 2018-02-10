/*********************************************************************************
 * Localizer.cs
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
    public abstract class Localizer
    {
        public abstract string Localize(LocalizedStringKey localizedStringKey);

        private sealed class DefaultLocalizer : Localizer
        {
            public override string Localize(LocalizedStringKey localizedStringKey)
            {
                if (localizedStringKey == null) return null;
                if (localizedStringKey.Key == null) return localizedStringKey.DisplayText;
                return "{" + localizedStringKey.Key + "}";
            }
        }

        private static Localizer current;

        public static Localizer Current
        {
            get { return current; }
            set
            {
                if (current != value)
                {
                    if (value == null) throw new ArgumentNullException(nameof(value));
                    current = value;
                }
            }
        }

        public static readonly Localizer Default = new DefaultLocalizer();

        static Localizer()
        {
            current = Default;
        }
    }
}

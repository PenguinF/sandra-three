/*********************************************************************************
 * LocalizedString.cs
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
    /// Represents an immutable identifier for a <see cref="LocalizedString"/>.
    /// </summary>
    public sealed class LocalizedStringKey
    {
        public readonly string DisplayText;

        /// <summary>
        /// Constructs a new instance of <see cref="LocalizedStringKey"/>.
        /// </summary>
        public LocalizedStringKey(string displayText)
        {
            if (displayText == null) throw new ArgumentNullException(nameof(displayText));
            DisplayText = displayText;
        }
    }
}

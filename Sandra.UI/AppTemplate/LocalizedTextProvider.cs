#region License
/*********************************************************************************
 * LocalizedTextProvider.cs
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
using System;

namespace Eutherion.Win.AppTemplate
{
    /// <summary>
    /// <see cref="ITextProvider"/> which provides a localized text to a UI element.
    /// </summary>
    public class LocalizedTextProvider : ITextProvider
    {
        /// <summary>
        /// Gets the key for this <see cref="LocalizedTextProvider"/>.
        /// </summary>
        public readonly LocalizedStringKey Key;

        /// <summary>
        /// Gets the current localized display text.
        /// </summary>
        public string GetText() => Session.Current.CurrentLocalizer.Localize(Key);

        /// <summary>
        /// Initializes a new instance of <see cref="LocalizedTextProvider"/> with a specified <see cref="LocalizedStringKey"/>.
        /// </summary>
        /// <param name="key">
        /// The <see cref="LocalizedStringKey"/> for the <see cref="LocalizedTextProvider"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is null.
        /// </exception>
        public LocalizedTextProvider(LocalizedStringKey key)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
        }
    }
}

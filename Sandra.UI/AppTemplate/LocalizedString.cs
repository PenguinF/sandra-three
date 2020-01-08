#region License
/*********************************************************************************
 * LocalizedString.cs
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
using System;

namespace Eutherion.Win.AppTemplate
{
    /// <summary>
    /// Represents a localized string, which is updated on a change to <see cref="Session.CurrentLocalizer"/>.
    /// </summary>
    public class LocalizedString : LocalizedTextProvider, IDisposable, IWeakEventTarget
    {
        /// <summary>
        /// Gets the current localized display text.
        /// </summary>
        public readonly ObservableValue<string> DisplayText = new ObservableValue<string>(StringComparer.Ordinal);

        public LocalizedString(LocalizedStringKey key)
            : base(key)
        {
            DisplayText.Value = GetText();
            Session.Current.CurrentLocalizerChanged += Localizer_CurrentChanged;
        }

        private void Localizer_CurrentChanged(object sender, EventArgs e)
        {
            DisplayText.Value = GetText();
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}

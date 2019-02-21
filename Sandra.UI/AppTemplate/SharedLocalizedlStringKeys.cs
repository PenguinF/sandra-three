#region License
/*********************************************************************************
 * SharedLocalizedStringKeys.cs
 *
 * Copyright (c) 2004-2019 Henk Nicolai
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

namespace Eutherion.Win.AppTemplate
{
    public static class SharedLocalizedStringKeys
    {
        internal static readonly LocalizedStringKey About = new LocalizedStringKey(nameof(About));
        internal static readonly LocalizedStringKey Copy = new LocalizedStringKey(nameof(Copy));
        internal static readonly LocalizedStringKey Credits = new LocalizedStringKey(nameof(Credits));
        internal static readonly LocalizedStringKey Cut = new LocalizedStringKey(nameof(Cut));
        internal static readonly LocalizedStringKey EditCurrentLanguage = new LocalizedStringKey(nameof(EditCurrentLanguage));
        internal static readonly LocalizedStringKey EditPreferencesFile = new LocalizedStringKey(nameof(EditPreferencesFile));
        internal static readonly LocalizedStringKey GoToNextLocation = new LocalizedStringKey(nameof(GoToNextLocation));
        internal static readonly LocalizedStringKey GoToPreviousLocation = new LocalizedStringKey(nameof(GoToPreviousLocation));
        internal static readonly LocalizedStringKey Paste = new LocalizedStringKey(nameof(Paste));
        internal static readonly LocalizedStringKey Redo = new LocalizedStringKey(nameof(Redo));
        internal static readonly LocalizedStringKey SelectAll = new LocalizedStringKey(nameof(SelectAll));
        internal static readonly LocalizedStringKey ShowDefaultSettingsFile = new LocalizedStringKey(nameof(ShowDefaultSettingsFile));
        internal static readonly LocalizedStringKey Undo = new LocalizedStringKey(nameof(Undo));
        internal static readonly LocalizedStringKey ZoomIn = new LocalizedStringKey(nameof(ZoomIn));
        internal static readonly LocalizedStringKey ZoomOut = new LocalizedStringKey(nameof(ZoomOut));
    }
}

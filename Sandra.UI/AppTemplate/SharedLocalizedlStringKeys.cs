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
using System.Collections.Generic;

namespace Eutherion.Win.AppTemplate
{
    public static class SharedLocalizedStringKeys
    {
        public static readonly LocalizedStringKey About = new LocalizedStringKey(nameof(About));
        public static readonly LocalizedStringKey Copy = new LocalizedStringKey(nameof(Copy));
        public static readonly LocalizedStringKey Credits = new LocalizedStringKey(nameof(Credits));
        public static readonly LocalizedStringKey Cut = new LocalizedStringKey(nameof(Cut));
        public static readonly LocalizedStringKey Edit = new LocalizedStringKey(nameof(Edit));
        public static readonly LocalizedStringKey EditCurrentLanguage = new LocalizedStringKey(nameof(EditCurrentLanguage));
        public static readonly LocalizedStringKey EditPreferencesFile = new LocalizedStringKey(nameof(EditPreferencesFile));
        public static readonly LocalizedStringKey ErrorLocation = new LocalizedStringKey(nameof(ErrorLocation));
        public static readonly LocalizedStringKey Exit = new LocalizedStringKey(nameof(Exit));
        public static readonly LocalizedStringKey File = new LocalizedStringKey(nameof(File));
        public static readonly LocalizedStringKey GoToNextLocation = new LocalizedStringKey(nameof(GoToNextLocation));
        public static readonly LocalizedStringKey GoToPreviousLocation = new LocalizedStringKey(nameof(GoToPreviousLocation));
        public static readonly LocalizedStringKey Help = new LocalizedStringKey(nameof(Help));
        public static readonly LocalizedStringKey NoErrorsMessage = new LocalizedStringKey(nameof(NoErrorsMessage));
        public static readonly LocalizedStringKey Paste = new LocalizedStringKey(nameof(Paste));
        public static readonly LocalizedStringKey Redo = new LocalizedStringKey(nameof(Redo));
        public static readonly LocalizedStringKey Save = new LocalizedStringKey(nameof(Save));
        public static readonly LocalizedStringKey SelectAll = new LocalizedStringKey(nameof(SelectAll));
        public static readonly LocalizedStringKey ShowDefaultSettingsFile = new LocalizedStringKey(nameof(ShowDefaultSettingsFile));
        public static readonly LocalizedStringKey Tools = new LocalizedStringKey(nameof(Tools));
        public static readonly LocalizedStringKey Undo = new LocalizedStringKey(nameof(Undo));
        public static readonly LocalizedStringKey View = new LocalizedStringKey(nameof(View));
        public static readonly LocalizedStringKey ZoomIn = new LocalizedStringKey(nameof(ZoomIn));
        public static readonly LocalizedStringKey ZoomOut = new LocalizedStringKey(nameof(ZoomOut));

        public static IEnumerable<KeyValuePair<LocalizedStringKey, string>> DefaultEnglishTranslations => new Dictionary<LocalizedStringKey, string>
        {
            { About, "About SandraChess" },
            { Copy, "Copy" },
            { Credits, "Show credits" },
            { Cut, "Cut" },
            { Edit, "Edit" },
            { EditCurrentLanguage, "Edit current language" },
            { EditPreferencesFile, "Edit preferences" },
            { ErrorLocation, "{0} at line {1}, position {2}" },
            { Exit, "Exit" },
            { File, "File" },
            { GoToNextLocation, "Go to next location" },
            { GoToPreviousLocation, "Go to previous location" },
            { Help, "Help" },
            { NoErrorsMessage, "(No errors)" },
            { Paste, "Paste" },
            { Redo, "Redo" },
            { Save, "Save" },
            { SelectAll, "Select All" },
            { ShowDefaultSettingsFile, "Show default settings" },
            { Tools, "Tools" },
            { Undo, "Undo" },
            { View, "View" },
            { ZoomIn, "Zoom in" },
            { ZoomOut, "Zoom out" },
        };
    }
}

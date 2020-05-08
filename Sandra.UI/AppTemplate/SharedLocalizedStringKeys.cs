#region License
/*********************************************************************************
 * SharedLocalizedStringKeys.cs
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
using System.Collections.Generic;

namespace Eutherion.Win.AppTemplate
{
    public static class SharedLocalizedStringKeys
    {
        public static readonly LocalizedStringKey About = new LocalizedStringKey(nameof(About));
        public static readonly LocalizedStringKey AllFiles = new LocalizedStringKey(nameof(AllFiles));
        public static readonly LocalizedStringKey Close = new LocalizedStringKey(nameof(Close));
        public static readonly LocalizedStringKey Copy = new LocalizedStringKey(nameof(Copy));
        public static readonly LocalizedStringKey Credits = new LocalizedStringKey(nameof(Credits));
        public static readonly LocalizedStringKey Cut = new LocalizedStringKey(nameof(Cut));
        public static readonly LocalizedStringKey Edit = new LocalizedStringKey(nameof(Edit));
        public static readonly LocalizedStringKey EditCurrentLanguage = new LocalizedStringKey(nameof(EditCurrentLanguage));
        public static readonly LocalizedStringKey EditPreferencesFile = new LocalizedStringKey(nameof(EditPreferencesFile));
        public static readonly LocalizedStringKey ErrorLocation = new LocalizedStringKey(nameof(ErrorLocation));
        public static readonly LocalizedStringKey Exit = new LocalizedStringKey(nameof(Exit));
        public static readonly LocalizedStringKey File = new LocalizedStringKey(nameof(File));
        public static readonly LocalizedStringKey GoToNextError = new LocalizedStringKey(nameof(GoToNextError));
        public static readonly LocalizedStringKey GoToPreviousError = new LocalizedStringKey(nameof(GoToPreviousError));
        public static readonly LocalizedStringKey Help = new LocalizedStringKey(nameof(Help));
        public static readonly LocalizedStringKey JsonFiles = new LocalizedStringKey(nameof(JsonFiles));
        public static readonly LocalizedStringKey NoErrorsMessage = new LocalizedStringKey(nameof(NoErrorsMessage));
        public static readonly LocalizedStringKey OpenExecutableFolder = new LocalizedStringKey(nameof(OpenExecutableFolder));
        public static readonly LocalizedStringKey OpenLocalAppDataFolder = new LocalizedStringKey(nameof(OpenLocalAppDataFolder));
        public static readonly LocalizedStringKey Paste = new LocalizedStringKey(nameof(Paste));
        public static readonly LocalizedStringKey Redo = new LocalizedStringKey(nameof(Redo));
        public static readonly LocalizedStringKey Save = new LocalizedStringKey(nameof(Save));
        public static readonly LocalizedStringKey SaveAs = new LocalizedStringKey(nameof(SaveAs));
        public static readonly LocalizedStringKey SaveChangesQuery = new LocalizedStringKey(nameof(SaveChangesQuery));
        public static readonly LocalizedStringKey SelectAll = new LocalizedStringKey(nameof(SelectAll));
        public static readonly LocalizedStringKey ShowDefaultSettingsFile = new LocalizedStringKey(nameof(ShowDefaultSettingsFile));
        public static readonly LocalizedStringKey ShowErrorPane = new LocalizedStringKey(nameof(ShowErrorPane));
        public static readonly LocalizedStringKey Tools = new LocalizedStringKey(nameof(Tools));
        public static readonly LocalizedStringKey Undo = new LocalizedStringKey(nameof(Undo));
        public static readonly LocalizedStringKey UnsavedChangesTitle = new LocalizedStringKey(nameof(UnsavedChangesTitle));
        public static readonly LocalizedStringKey Untitled = new LocalizedStringKey(nameof(Untitled));
        public static readonly LocalizedStringKey View = new LocalizedStringKey(nameof(View));
        public static readonly LocalizedStringKey ZoomIn = new LocalizedStringKey(nameof(ZoomIn));
        public static readonly LocalizedStringKey ZoomOut = new LocalizedStringKey(nameof(ZoomOut));

        public static IEnumerable<KeyValuePair<LocalizedStringKey, string>> DefaultEnglishTranslations(string appName)
            => new Dictionary<LocalizedStringKey, string>
            {
                { About, $"About {appName}" },
                { AllFiles, "All files" },
                { Close, "Close" },
                { Copy, "Copy" },
                { Credits, "Show credits" },
                { Cut, "Cut" },
                { Edit, "Edit" },
                { EditCurrentLanguage, "Edit current language" },
                { EditPreferencesFile, "Edit preferences" },
                { ErrorLocation, "{0} at line {1}, position {2}" },
                { Exit, "Exit" },
                { File, "File" },
                { GoToNextError, "Go to next message" },
                { GoToPreviousError, "Go to previous message" },
                { Help, "Help" },
                { JsonFiles, "Json files" },
                { NoErrorsMessage, "(No errors)" },
                { OpenExecutableFolder, "Open executable folder" },
                { OpenLocalAppDataFolder, "Open local application data folder" },
                { Paste, "Paste" },
                { Redo, "Redo" },
                { Save, "Save" },
                { SaveAs, "Save as" },
                { SaveChangesQuery, "'{0}' contains unsaved changes which will be lost if this window is closed. Save changes?" },
                { SelectAll, "Select All" },
                { ShowDefaultSettingsFile, "Show default settings" },
                { ShowErrorPane, "Show messages" },
                { Tools, "Tools" },
                { Undo, "Undo" },
                { UnsavedChangesTitle, "Save changes" },
                { Untitled, "Untitled" },
                { View, "View" },
                { ZoomIn, "Zoom in" },
                { ZoomOut, "Zoom out" },
            };
    }
}

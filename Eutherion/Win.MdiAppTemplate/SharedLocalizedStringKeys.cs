#region License
/*********************************************************************************
 * SharedLocalizedStringKeys.cs
 *
 * Copyright (c) 2004-2025 Henk Nicolai
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

namespace Eutherion.Win.MdiAppTemplate
{
    public static class SharedLocalizedStringKeys
    {
        public static readonly StringKey<Localization> About = new StringKey<Localization>(nameof(About));
        public static readonly StringKey<Localization> AllFiles = new StringKey<Localization>(nameof(AllFiles));
        public static readonly StringKey<Localization> Close = new StringKey<Localization>(nameof(Close));
        public static readonly StringKey<Localization> Copy = new StringKey<Localization>(nameof(Copy));
        public static readonly StringKey<Localization> Credits = new StringKey<Localization>(nameof(Credits));
        public static readonly StringKey<Localization> Cut = new StringKey<Localization>(nameof(Cut));
        public static readonly StringKey<Localization> Edit = new StringKey<Localization>(nameof(Edit));
        public static readonly StringKey<Localization> EditCurrentLanguage = new StringKey<Localization>(nameof(EditCurrentLanguage));
        public static readonly StringKey<Localization> EditPreferencesFile = new StringKey<Localization>(nameof(EditPreferencesFile));
        public static readonly StringKey<Localization> ErrorLocation = new StringKey<Localization>(nameof(ErrorLocation));
        public static readonly StringKey<Localization> ErrorPaneTitle = new StringKey<Localization>(nameof(ErrorPaneTitle));
        public static readonly StringKey<Localization> Exit = new StringKey<Localization>(nameof(Exit));
        public static readonly StringKey<Localization> File = new StringKey<Localization>(nameof(File));
        public static readonly StringKey<Localization> GoToNextError = new StringKey<Localization>(nameof(GoToNextError));
        public static readonly StringKey<Localization> GoToPreviousError = new StringKey<Localization>(nameof(GoToPreviousError));
        public static readonly StringKey<Localization> Help = new StringKey<Localization>(nameof(Help));
        public static readonly StringKey<Localization> JsonFiles = new StringKey<Localization>(nameof(JsonFiles));
        public static readonly StringKey<Localization> NoErrorsMessage = new StringKey<Localization>(nameof(NoErrorsMessage));
        public static readonly StringKey<Localization> OpenExecutableFolder = new StringKey<Localization>(nameof(OpenExecutableFolder));
        public static readonly StringKey<Localization> OpenLocalAppDataFolder = new StringKey<Localization>(nameof(OpenLocalAppDataFolder));
        public static readonly StringKey<Localization> Paste = new StringKey<Localization>(nameof(Paste));
        public static readonly StringKey<Localization> Redo = new StringKey<Localization>(nameof(Redo));
        public static readonly StringKey<Localization> Save = new StringKey<Localization>(nameof(Save));
        public static readonly StringKey<Localization> SaveAs = new StringKey<Localization>(nameof(SaveAs));
        public static readonly StringKey<Localization> SaveChangesQuery = new StringKey<Localization>(nameof(SaveChangesQuery));
        public static readonly StringKey<Localization> SelectAll = new StringKey<Localization>(nameof(SelectAll));
        public static readonly StringKey<Localization> ShowDefaultSettingsFile = new StringKey<Localization>(nameof(ShowDefaultSettingsFile));
        public static readonly StringKey<Localization> ShowErrorPane = new StringKey<Localization>(nameof(ShowErrorPane));
        public static readonly StringKey<Localization> Tools = new StringKey<Localization>(nameof(Tools));
        public static readonly StringKey<Localization> Undo = new StringKey<Localization>(nameof(Undo));
        public static readonly StringKey<Localization> UnsavedChangesTitle = new StringKey<Localization>(nameof(UnsavedChangesTitle));
        public static readonly StringKey<Localization> Untitled = new StringKey<Localization>(nameof(Untitled));
        public static readonly StringKey<Localization> View = new StringKey<Localization>(nameof(View));
        public static readonly StringKey<Localization> WindowMaximize = new StringKey<Localization>(nameof(WindowMaximize));
        public static readonly StringKey<Localization> WindowMinimize = new StringKey<Localization>(nameof(WindowMinimize));
        public static readonly StringKey<Localization> WindowMove = new StringKey<Localization>(nameof(WindowMove));
        public static readonly StringKey<Localization> WindowRestore = new StringKey<Localization>(nameof(WindowRestore));
        public static readonly StringKey<Localization> WindowSize = new StringKey<Localization>(nameof(WindowSize));
        public static readonly StringKey<Localization> ZoomIn = new StringKey<Localization>(nameof(ZoomIn));
        public static readonly StringKey<Localization> ZoomOut = new StringKey<Localization>(nameof(ZoomOut));

        public static IEnumerable<KeyValuePair<StringKey<Localization>, string>> DefaultEnglishTranslations(string appName)
            => new Dictionary<StringKey<Localization>, string>
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
                { ErrorPaneTitle, "Messages - [{0}]" },
                { Exit, "Exit" },
                { File, "File" },
                { GoToNextError, "Go to next message" },
                { GoToPreviousError, "Go to previous message" },
                { Help, "Help" },
                { JsonFiles, "Json files" },
                { NoErrorsMessage, "(No messages)" },
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
                { WindowMaximize, "Maximize" },
                { WindowMinimize, "Minimize" },
                { WindowMove, "Move" },
                { WindowRestore, "Restore" },
                { WindowSize, "Size" },
                { ZoomIn, "Zoom in" },
                { ZoomOut, "Zoom out" },
            };
    }
}

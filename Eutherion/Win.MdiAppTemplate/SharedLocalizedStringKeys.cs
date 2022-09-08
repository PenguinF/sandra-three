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

using Eutherion.Text;
using System.Collections.Generic;

namespace Eutherion.Win.MdiAppTemplate
{
    public static class SharedLocalizedStringKeys
    {
        public static readonly StringKey<ForFormattedText> About = new StringKey<ForFormattedText>(nameof(About));
        public static readonly StringKey<ForFormattedText> AllFiles = new StringKey<ForFormattedText>(nameof(AllFiles));
        public static readonly StringKey<ForFormattedText> Close = new StringKey<ForFormattedText>(nameof(Close));
        public static readonly StringKey<ForFormattedText> Copy = new StringKey<ForFormattedText>(nameof(Copy));
        public static readonly StringKey<ForFormattedText> Credits = new StringKey<ForFormattedText>(nameof(Credits));
        public static readonly StringKey<ForFormattedText> Cut = new StringKey<ForFormattedText>(nameof(Cut));
        public static readonly StringKey<ForFormattedText> Edit = new StringKey<ForFormattedText>(nameof(Edit));
        public static readonly StringKey<ForFormattedText> EditCurrentLanguage = new StringKey<ForFormattedText>(nameof(EditCurrentLanguage));
        public static readonly StringKey<ForFormattedText> EditPreferencesFile = new StringKey<ForFormattedText>(nameof(EditPreferencesFile));
        public static readonly StringKey<ForFormattedText> ErrorLocation = new StringKey<ForFormattedText>(nameof(ErrorLocation));
        public static readonly StringKey<ForFormattedText> ErrorPaneTitle = new StringKey<ForFormattedText>(nameof(ErrorPaneTitle));
        public static readonly StringKey<ForFormattedText> Exit = new StringKey<ForFormattedText>(nameof(Exit));
        public static readonly StringKey<ForFormattedText> File = new StringKey<ForFormattedText>(nameof(File));
        public static readonly StringKey<ForFormattedText> GoToNextError = new StringKey<ForFormattedText>(nameof(GoToNextError));
        public static readonly StringKey<ForFormattedText> GoToPreviousError = new StringKey<ForFormattedText>(nameof(GoToPreviousError));
        public static readonly StringKey<ForFormattedText> Help = new StringKey<ForFormattedText>(nameof(Help));
        public static readonly StringKey<ForFormattedText> JsonFiles = new StringKey<ForFormattedText>(nameof(JsonFiles));
        public static readonly StringKey<ForFormattedText> NoErrorsMessage = new StringKey<ForFormattedText>(nameof(NoErrorsMessage));
        public static readonly StringKey<ForFormattedText> OpenExecutableFolder = new StringKey<ForFormattedText>(nameof(OpenExecutableFolder));
        public static readonly StringKey<ForFormattedText> OpenLocalAppDataFolder = new StringKey<ForFormattedText>(nameof(OpenLocalAppDataFolder));
        public static readonly StringKey<ForFormattedText> Paste = new StringKey<ForFormattedText>(nameof(Paste));
        public static readonly StringKey<ForFormattedText> Redo = new StringKey<ForFormattedText>(nameof(Redo));
        public static readonly StringKey<ForFormattedText> Save = new StringKey<ForFormattedText>(nameof(Save));
        public static readonly StringKey<ForFormattedText> SaveAs = new StringKey<ForFormattedText>(nameof(SaveAs));
        public static readonly StringKey<ForFormattedText> SaveChangesQuery = new StringKey<ForFormattedText>(nameof(SaveChangesQuery));
        public static readonly StringKey<ForFormattedText> SelectAll = new StringKey<ForFormattedText>(nameof(SelectAll));
        public static readonly StringKey<ForFormattedText> ShowDefaultSettingsFile = new StringKey<ForFormattedText>(nameof(ShowDefaultSettingsFile));
        public static readonly StringKey<ForFormattedText> ShowErrorPane = new StringKey<ForFormattedText>(nameof(ShowErrorPane));
        public static readonly StringKey<ForFormattedText> Tools = new StringKey<ForFormattedText>(nameof(Tools));
        public static readonly StringKey<ForFormattedText> Undo = new StringKey<ForFormattedText>(nameof(Undo));
        public static readonly StringKey<ForFormattedText> UnsavedChangesTitle = new StringKey<ForFormattedText>(nameof(UnsavedChangesTitle));
        public static readonly StringKey<ForFormattedText> Untitled = new StringKey<ForFormattedText>(nameof(Untitled));
        public static readonly StringKey<ForFormattedText> View = new StringKey<ForFormattedText>(nameof(View));
        public static readonly StringKey<ForFormattedText> WindowMaximize = new StringKey<ForFormattedText>(nameof(WindowMaximize));
        public static readonly StringKey<ForFormattedText> WindowMinimize = new StringKey<ForFormattedText>(nameof(WindowMinimize));
        public static readonly StringKey<ForFormattedText> WindowMove = new StringKey<ForFormattedText>(nameof(WindowMove));
        public static readonly StringKey<ForFormattedText> WindowRestore = new StringKey<ForFormattedText>(nameof(WindowRestore));
        public static readonly StringKey<ForFormattedText> WindowSize = new StringKey<ForFormattedText>(nameof(WindowSize));
        public static readonly StringKey<ForFormattedText> ZoomIn = new StringKey<ForFormattedText>(nameof(ZoomIn));
        public static readonly StringKey<ForFormattedText> ZoomOut = new StringKey<ForFormattedText>(nameof(ZoomOut));

        public static IEnumerable<KeyValuePair<StringKey<ForFormattedText>, string>> DefaultEnglishTranslations(string appName)
            => new Dictionary<StringKey<ForFormattedText>, string>
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

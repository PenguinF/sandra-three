#region License
/*********************************************************************************
 * LocalizedStringKeys.cs
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

using Eutherion;
using Eutherion.Text;
using System.Collections.Generic;

namespace Sandra.UI
{
    internal static class LocalizedStringKeys
    {
        internal static readonly StringKey<Localization> CopyDiagramToClipboard = new StringKey<Localization>(nameof(CopyDiagramToClipboard));
        internal static readonly StringKey<Localization> DeleteLine = new StringKey<Localization>(nameof(DeleteLine));
        internal static readonly StringKey<Localization> DemoteLine = new StringKey<Localization>(nameof(DemoteLine));
        internal static readonly StringKey<Localization> FastBackward = new StringKey<Localization>(nameof(FastBackward));
        internal static readonly StringKey<Localization> FastForward = new StringKey<Localization>(nameof(FastForward));
        internal static readonly StringKey<Localization> FirstMove = new StringKey<Localization>(nameof(FirstMove));
        internal static readonly StringKey<Localization> FlipBoard = new StringKey<Localization>(nameof(FlipBoard));
        internal static readonly StringKey<Localization> GoTo = new StringKey<Localization>(nameof(GoTo));
        internal static readonly StringKey<Localization> LastMove = new StringKey<Localization>(nameof(LastMove));
        internal static readonly StringKey<Localization> NewGame = new StringKey<Localization>(nameof(NewGame));
        internal static readonly StringKey<Localization> NewGameFile = new StringKey<Localization>(nameof(NewGameFile));
        internal static readonly StringKey<Localization> NextLine = new StringKey<Localization>(nameof(NextLine));
        internal static readonly StringKey<Localization> NextMove = new StringKey<Localization>(nameof(NextMove));
        internal static readonly StringKey<Localization> OpenGame = new StringKey<Localization>(nameof(OpenGame));
        internal static readonly StringKey<Localization> OpenGameFile = new StringKey<Localization>(nameof(OpenGameFile));
        internal static readonly StringKey<Localization> PgnFiles = new StringKey<Localization>(nameof(PgnFiles));
        internal static readonly StringKey<Localization> PreviousLine = new StringKey<Localization>(nameof(PreviousLine));
        internal static readonly StringKey<Localization> PreviousMove = new StringKey<Localization>(nameof(PreviousMove));
        internal static readonly StringKey<Localization> PromoteLine = new StringKey<Localization>(nameof(PromoteLine));

        internal static IEnumerable<KeyValuePair<StringKey<Localization>, string>> DefaultEnglishTranslations => new Dictionary<StringKey<Localization>, string>
        {
            { CopyDiagramToClipboard, "Copy diagram to clipboard" },
            { DeleteLine, "Delete line" },
            { DemoteLine, "Demote line" },
            { FastBackward, "Fast backward" },
            { FastForward, "Fast forward" },
            { FirstMove, "First move" },
            { FlipBoard, "Flip board" },
            { GoTo, "Go to" },
            { LastMove, "Last move" },
            { NewGame, "New game" },
            { NewGameFile, "New game file" },
            { NextLine, "Next line" },
            { NextMove, "Next move" },
            { OpenGame, "Open game" },
            { OpenGameFile, "Open game file" },
            { PgnFiles, "Portable game notation files" },
            { PreviousLine, "Previous line" },
            { PreviousMove, "Previous move" },
            { PromoteLine, "Promote line" },
        };
    }
}

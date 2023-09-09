#region License
/*********************************************************************************
 * LocalizedStringKeys.cs
 *
 * Copyright (c) 2004-2023 Henk Nicolai
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
        internal static readonly StringKey<ForFormattedText> CopyDiagramToClipboard = new StringKey<ForFormattedText>(nameof(CopyDiagramToClipboard));
        internal static readonly StringKey<ForFormattedText> DeleteLine = new StringKey<ForFormattedText>(nameof(DeleteLine));
        internal static readonly StringKey<ForFormattedText> DemoteLine = new StringKey<ForFormattedText>(nameof(DemoteLine));
        internal static readonly StringKey<ForFormattedText> FastBackward = new StringKey<ForFormattedText>(nameof(FastBackward));
        internal static readonly StringKey<ForFormattedText> FastForward = new StringKey<ForFormattedText>(nameof(FastForward));
        internal static readonly StringKey<ForFormattedText> FirstMove = new StringKey<ForFormattedText>(nameof(FirstMove));
        internal static readonly StringKey<ForFormattedText> FlipBoard = new StringKey<ForFormattedText>(nameof(FlipBoard));
        internal static readonly StringKey<ForFormattedText> GoTo = new StringKey<ForFormattedText>(nameof(GoTo));
        internal static readonly StringKey<ForFormattedText> LastMove = new StringKey<ForFormattedText>(nameof(LastMove));
        internal static readonly StringKey<ForFormattedText> NewGame = new StringKey<ForFormattedText>(nameof(NewGame));
        internal static readonly StringKey<ForFormattedText> NewGameFile = new StringKey<ForFormattedText>(nameof(NewGameFile));
        internal static readonly StringKey<ForFormattedText> NextLine = new StringKey<ForFormattedText>(nameof(NextLine));
        internal static readonly StringKey<ForFormattedText> NextMove = new StringKey<ForFormattedText>(nameof(NextMove));
        internal static readonly StringKey<ForFormattedText> OpenGame = new StringKey<ForFormattedText>(nameof(OpenGame));
        internal static readonly StringKey<ForFormattedText> OpenGameFile = new StringKey<ForFormattedText>(nameof(OpenGameFile));
        internal static readonly StringKey<ForFormattedText> PgnFiles = new StringKey<ForFormattedText>(nameof(PgnFiles));
        internal static readonly StringKey<ForFormattedText> PreviousLine = new StringKey<ForFormattedText>(nameof(PreviousLine));
        internal static readonly StringKey<ForFormattedText> PreviousMove = new StringKey<ForFormattedText>(nameof(PreviousMove));
        internal static readonly StringKey<ForFormattedText> PromoteLine = new StringKey<ForFormattedText>(nameof(PromoteLine));

        internal static IEnumerable<KeyValuePair<StringKey<ForFormattedText>, string>> DefaultEnglishTranslations => new Dictionary<StringKey<ForFormattedText>, string>
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

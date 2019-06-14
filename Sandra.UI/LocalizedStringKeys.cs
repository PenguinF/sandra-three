#region License
/*********************************************************************************
 * LocalizedStringKeys.cs
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

namespace Sandra.UI
{
    internal static class LocalizedStringKeys
    {
        internal static readonly LocalizedStringKey BreakAtCurrentPosition = new LocalizedStringKey(nameof(BreakAtCurrentPosition));
        internal static readonly LocalizedStringKey Chessboard = new LocalizedStringKey(nameof(Chessboard));
        internal static readonly LocalizedStringKey CopyDiagramToClipboard = new LocalizedStringKey(nameof(CopyDiagramToClipboard));
        internal static readonly LocalizedStringKey DeleteLine = new LocalizedStringKey(nameof(DeleteLine));
        internal static readonly LocalizedStringKey DemoteLine = new LocalizedStringKey(nameof(DemoteLine));
        internal static readonly LocalizedStringKey EndOfGame = new LocalizedStringKey(nameof(EndOfGame));
        internal static readonly LocalizedStringKey FastBackward = new LocalizedStringKey(nameof(FastBackward));
        internal static readonly LocalizedStringKey FastForward = new LocalizedStringKey(nameof(FastForward));
        internal static readonly LocalizedStringKey FirstMove = new LocalizedStringKey(nameof(FirstMove));
        internal static readonly LocalizedStringKey FlipBoard = new LocalizedStringKey(nameof(FlipBoard));
        internal static readonly LocalizedStringKey Game = new LocalizedStringKey(nameof(Game));
        internal static readonly LocalizedStringKey GoTo = new LocalizedStringKey(nameof(GoTo));
        internal static readonly LocalizedStringKey LastMove = new LocalizedStringKey(nameof(LastMove));
        internal static readonly LocalizedStringKey Moves = new LocalizedStringKey(nameof(Moves));
        internal static readonly LocalizedStringKey NewGame = new LocalizedStringKey(nameof(NewGame));
        internal static readonly LocalizedStringKey NewGameFile = new LocalizedStringKey(nameof(NewGameFile));
        internal static readonly LocalizedStringKey NextLine = new LocalizedStringKey(nameof(NextLine));
        internal static readonly LocalizedStringKey NextMove = new LocalizedStringKey(nameof(NextMove));
        internal static readonly LocalizedStringKey OpenGameFile = new LocalizedStringKey(nameof(OpenGameFile));
        internal static readonly LocalizedStringKey PGNFiles = new LocalizedStringKey(nameof(PGNFiles));
        internal static readonly LocalizedStringKey PieceSymbols = new LocalizedStringKey(nameof(PieceSymbols));
        internal static readonly LocalizedStringKey PreviousLine = new LocalizedStringKey(nameof(PreviousLine));
        internal static readonly LocalizedStringKey PreviousMove = new LocalizedStringKey(nameof(PreviousMove));
        internal static readonly LocalizedStringKey PromoteLine = new LocalizedStringKey(nameof(PromoteLine));
        internal static readonly LocalizedStringKey StartOfGame = new LocalizedStringKey(nameof(StartOfGame));
        internal static readonly LocalizedStringKey UseLongAlgebraicNotation = new LocalizedStringKey(nameof(UseLongAlgebraicNotation));
        internal static readonly LocalizedStringKey UsePGNPieceSymbols = new LocalizedStringKey(nameof(UsePGNPieceSymbols));

        internal static IEnumerable<KeyValuePair<LocalizedStringKey, string>> DefaultEnglishTranslations => new Dictionary<LocalizedStringKey, string>
        {
            { BreakAtCurrentPosition, "Break at current position" },
            { Chessboard, "Chessboard" },
            { CopyDiagramToClipboard, "Copy diagram to clipboard" },
            { DeleteLine, "Delete line" },
            { DemoteLine, "Demote line" },
            { EndOfGame, "End of game" },
            { FastBackward, "Fast backward" },
            { FastForward, "Fast forward" },
            { FirstMove, "First move" },
            { FlipBoard, "Flip board" },
            { Game, "Game" },
            { GoTo, "Go to" },
            { LastMove, "Last move" },
            { Moves, "Moves" },
            { NewGame, "New game" },
            { NewGameFile, "New game file" },
            { NextLine, "Next line" },
            { NextMove, "Next move" },
            { OpenGameFile, "Open game file" },
            { PGNFiles, "Portable game notation files" },
            { PieceSymbols, "NBRQK" },
            { PreviousLine, "Previous line" },
            { PreviousMove, "Previous move" },
            { PromoteLine, "Promote line" },
            { StartOfGame, "Start of game" },
            { UseLongAlgebraicNotation, "Use long algebraic notation" },
            { UsePGNPieceSymbols, "Use PGN notation" },
        };
    }
}

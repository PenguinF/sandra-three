﻿/*********************************************************************************
 * Localizers.cs
 * 
 * Copyright (c) 2004-2018 Henk Nicolai
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
 *********************************************************************************/
using System.Collections.Generic;

namespace Sandra.UI.WF
{
    internal static class LocalizedStringKeys
    {
        internal static readonly LocalizedStringKey BreakAtCurrentPosition = new LocalizedStringKey(nameof(BreakAtCurrentPosition));
        internal static readonly LocalizedStringKey Chessboard = new LocalizedStringKey(nameof(Chessboard));
        internal static readonly LocalizedStringKey Copy = new LocalizedStringKey(nameof(Copy));
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
        internal static readonly LocalizedStringKey NextLine = new LocalizedStringKey(nameof(NextLine));
        internal static readonly LocalizedStringKey NextMove = new LocalizedStringKey(nameof(NextMove));
        internal static readonly LocalizedStringKey PieceSymbols = new LocalizedStringKey(nameof(PieceSymbols));
        internal static readonly LocalizedStringKey PreviousLine = new LocalizedStringKey(nameof(PreviousLine));
        internal static readonly LocalizedStringKey PreviousMove = new LocalizedStringKey(nameof(PreviousMove));
        internal static readonly LocalizedStringKey PromoteLine = new LocalizedStringKey(nameof(PromoteLine));
        internal static readonly LocalizedStringKey SelectAll = new LocalizedStringKey(nameof(SelectAll));
        internal static readonly LocalizedStringKey StartOfGame = new LocalizedStringKey(nameof(StartOfGame));
        internal static readonly LocalizedStringKey UseLongAlgebraicNotation = new LocalizedStringKey(nameof(UseLongAlgebraicNotation));
        internal static readonly LocalizedStringKey UsePGNPieceSymbols = new LocalizedStringKey(nameof(UsePGNPieceSymbols));
        internal static readonly LocalizedStringKey View = new LocalizedStringKey(nameof(View));
    }

    internal static class Localizers
    {
        public static readonly Localizer English = new EnglishLocalizer();

        public static readonly Localizer Dutch = new DutchLocalizer();
    }

    internal sealed class EnglishLocalizer : Localizer
    {
        private readonly Dictionary<LocalizedStringKey, string> englishDictionary;

        public override string Localize(LocalizedStringKey localizedStringKey)
        {
            string displayText;

            if (englishDictionary.TryGetValue(localizedStringKey, out displayText))
            {
                return displayText;
            }

            return Default.Localize(localizedStringKey);
        }

        public EnglishLocalizer()
        {
            englishDictionary = new Dictionary<LocalizedStringKey, string>
            {
                { LocalizedStringKeys.BreakAtCurrentPosition, "Break at current position" },
                { LocalizedStringKeys.Chessboard, "Chessboard" },
                { LocalizedStringKeys.Copy, "Copy" },
                { LocalizedStringKeys.CopyDiagramToClipboard, "Copy diagram to clipboard" },
                { LocalizedStringKeys.DeleteLine, "Delete line" },
                { LocalizedStringKeys.DemoteLine, "Demote line" },
                { LocalizedStringKeys.EndOfGame, "End of game" },
                { LocalizedStringKeys.FastBackward, "Fast backward" },
                { LocalizedStringKeys.FastForward, "Fast forward" },
                { LocalizedStringKeys.FirstMove, "First move" },
                { LocalizedStringKeys.FlipBoard, "Flip board" },
                { LocalizedStringKeys.Game, "Game" },
                { LocalizedStringKeys.GoTo, "Go to" },
                { LocalizedStringKeys.LastMove, "Last move" },
                { LocalizedStringKeys.Moves, "Moves" },
                { LocalizedStringKeys.NewGame, "New game" },
                { LocalizedStringKeys.NextLine, "Next line" },
                { LocalizedStringKeys.NextMove, "Next move" },
                { LocalizedStringKeys.PieceSymbols, "NBRQK" },
                { LocalizedStringKeys.PreviousLine, "Previous line" },
                { LocalizedStringKeys.PreviousMove, "Previous move" },
                { LocalizedStringKeys.PromoteLine, "Promote line" },
                { LocalizedStringKeys.SelectAll, "Select All" },
                { LocalizedStringKeys.StartOfGame, "Start of game" },
                { LocalizedStringKeys.UseLongAlgebraicNotation, "Use long algebraic notation" },
                { LocalizedStringKeys.UsePGNPieceSymbols, "Use PGN notation" },
                { LocalizedStringKeys.View, "View" },

                { LocalizedConsoleKeys.ConsoleKeyCtrl, "Ctrl" },
                { LocalizedConsoleKeys.ConsoleKeyShift, "Shift" },
                { LocalizedConsoleKeys.ConsoleKeyAlt, "Alt" },

                { LocalizedConsoleKeys.ConsoleKeyLeftArrow, "Left Arrow" },
                { LocalizedConsoleKeys.ConsoleKeyRightArrow, "Right Arrow" },
                { LocalizedConsoleKeys.ConsoleKeyUpArrow, "Up Arrow" },
                { LocalizedConsoleKeys.ConsoleKeyDownArrow, "Down Arrow" },

                { LocalizedConsoleKeys.ConsoleKeyDelete, "Del" },
                { LocalizedConsoleKeys.ConsoleKeyHome, "Home" },
                { LocalizedConsoleKeys.ConsoleKeyEnd, "End" },
                { LocalizedConsoleKeys.ConsoleKeyPageDown, "PageDown" },
                { LocalizedConsoleKeys.ConsoleKeyPageUp, "PageUp" },
            };
        }
    }

    internal sealed class DutchLocalizer : Localizer
    {
        private readonly Dictionary<LocalizedStringKey, string> dutchDictionary;

        public override string Localize(LocalizedStringKey localizedStringKey)
        {
            string displayText;

            if (dutchDictionary.TryGetValue(localizedStringKey, out displayText))
            {
                return displayText;
            }

            return Default.Localize(localizedStringKey);
        }

        public DutchLocalizer()
        {
            dutchDictionary = new Dictionary<LocalizedStringKey, string>
            {
                { LocalizedStringKeys.BreakAtCurrentPosition, "Afbreken in huidige stelling" },
                { LocalizedStringKeys.Chessboard, "Schaakbord" },
                { LocalizedStringKeys.Copy, "Kopiëren" },
                { LocalizedStringKeys.CopyDiagramToClipboard, "Diagram naar klembord kopiëren" },
                { LocalizedStringKeys.DeleteLine, "Variant verwijderen" },
                { LocalizedStringKeys.DemoteLine, "Variant degraderen" },
                { LocalizedStringKeys.EndOfGame, "Naar einde partij" },
                { LocalizedStringKeys.FastBackward, "Snel achterwaarts" },
                { LocalizedStringKeys.FastForward, "Snel voorwaarts" },
                { LocalizedStringKeys.FirstMove, "Eerste zet" },
                { LocalizedStringKeys.FlipBoard, "Bord omkeren" },
                { LocalizedStringKeys.Game, "Partij" },
                { LocalizedStringKeys.GoTo, "Navigeren" },
                { LocalizedStringKeys.LastMove, "Laatste zet" },
                { LocalizedStringKeys.Moves, "Zetten" },
                { LocalizedStringKeys.NewGame, "Nieuwe partij" },
                { LocalizedStringKeys.NextLine, "Volgende variant" },
                { LocalizedStringKeys.NextMove, "Volgende zet" },
                { LocalizedStringKeys.PieceSymbols, "PLTDK" },
                { LocalizedStringKeys.PreviousLine, "Vorige variant" },
                { LocalizedStringKeys.PreviousMove, "Vorige zet" },
                { LocalizedStringKeys.PromoteLine, "Variant promoveren" },
                { LocalizedStringKeys.SelectAll, "Alles selecteren" },
                { LocalizedStringKeys.StartOfGame, "Naar begin partij" },
                { LocalizedStringKeys.UseLongAlgebraicNotation, "Lange notatie gebruiken" },
                { LocalizedStringKeys.UsePGNPieceSymbols, "PGN notatie gebruiken" },
                { LocalizedStringKeys.View, "Weergave" },

                { LocalizedConsoleKeys.ConsoleKeyCtrl, "Ctrl" },
                { LocalizedConsoleKeys.ConsoleKeyShift, "Shift" },
                { LocalizedConsoleKeys.ConsoleKeyAlt, "Alt" },

                { LocalizedConsoleKeys.ConsoleKeyLeftArrow, "Pijl Links" },
                { LocalizedConsoleKeys.ConsoleKeyRightArrow, "Pijl Rechts" },
                { LocalizedConsoleKeys.ConsoleKeyUpArrow, "Pijl Omhoog" },
                { LocalizedConsoleKeys.ConsoleKeyDownArrow, "Pijl Omlaag" },

                { LocalizedConsoleKeys.ConsoleKeyDelete, "Del" },
                { LocalizedConsoleKeys.ConsoleKeyHome, "Home" },
                { LocalizedConsoleKeys.ConsoleKeyEnd, "End" },
                { LocalizedConsoleKeys.ConsoleKeyPageDown, "PageDown" },
                { LocalizedConsoleKeys.ConsoleKeyPageUp, "PageUp" },
            };
        }
    }
}

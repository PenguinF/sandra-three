﻿#region License
/*********************************************************************************
 * PgnEditorExtensions.cs
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

using Eutherion.Win.MdiAppTemplate;
using Sandra.Chess.Pgn;
using System.Linq;

namespace Sandra.UI
{
    using PgnEditor = SyntaxEditor<RootPgnSyntax, IPgnSymbol, PgnErrorInfo>;

    /// <summary>
    /// Contains extension methods for PGN syntax editors.
    /// </summary>
    public static class PgnEditorExtensions
    {
        public static PgnSyntax FirstNonWhitespaceNode(PgnSyntax pgnSyntax)
        {
            foreach (var symbol in pgnSyntax.TerminalSymbolsInRange(0, pgnSyntax.Length))
            {
                if (!(symbol is PgnWhitespaceSyntax)) return symbol.ToSyntax();
            }

            return null;
        }

        private static PgnPlySyntax LastPlyInMainVariation(PgnGameSyntax pgnGameSyntax)
        {
            var plies = pgnGameSyntax.PlyList.Plies;
            if (plies.Count > 0) return plies[plies.Count - 1];
            return null;
        }

        public static (PgnGameSyntax game, PgnPlySyntax deepestPly) GameAtOrBeforePosition(this PgnEditor pgnEditor, int position)
        {
            PgnPlySyntax deepestPlySyntax = null;

            // We're looking for the symbols before and after the position.
            // If the position is right at the edge between two games, return the previous game; any trivia is part of the next game,
            // and so it's more likely we're closer to the previous game.
            // Hence, take the first symbol from the enumeration.
            if (pgnEditor.SyntaxTree != null)
            {
                PgnGameListSyntax gameList = pgnEditor.SyntaxTree.GameListSyntax;

                if (gameList.Games.Count > 0 && gameList.TerminalSymbolsInRange(position - 1, 2).Any(out IPgnSymbol symbolAtCursor))
                {
                    PgnSyntax pgnSyntax = symbolAtCursor.ToSyntax();

                    while (pgnSyntax != null)
                    {
                        if (deepestPlySyntax == null && pgnSyntax is PgnPlySyntax plySyntax)
                        {
                            deepestPlySyntax = plySyntax;
                        }
                        else if (pgnSyntax is PgnGameSyntax pgnGameSyntax)
                        {
                            // If the cursor position is before the start of the first syntax node of the current game,
                            // return the previous game instead if it exists.
                            PgnSyntax nonWhitespaceNode = FirstNonWhitespaceNode(pgnGameSyntax);
                            if (nonWhitespaceNode != null && position < FirstNonWhitespaceNode(pgnGameSyntax).AbsoluteStart)
                            {
                                if (pgnGameSyntax.ParentIndex > 0)
                                {
                                    pgnGameSyntax = gameList.Games[pgnGameSyntax.ParentIndex - 1];
                                    deepestPlySyntax = null;
                                }
                                else
                                {
                                    return (null, null);
                                }
                            }

                            if (deepestPlySyntax == null)
                            {
                                // Return the last ply of the main variation if the cursor is in a higher position.
                                PgnPlySyntax lastPlyInMainVariation = LastPlyInMainVariation(pgnGameSyntax);
                                if (lastPlyInMainVariation != null && position > lastPlyInMainVariation.AbsoluteStart)
                                {
                                    deepestPlySyntax = lastPlyInMainVariation;
                                }
                            }

                            return (pgnGameSyntax, deepestPlySyntax);
                        }
                        else if (pgnSyntax is PgnTriviaSyntax pgnTriviaSyntax)
                        {
                            // Jump to last game if within trailing trivia.
                            if (pgnTriviaSyntax == gameList.TrailingTrivia)
                            {
                                PgnGameSyntax lastGameSyntax = gameList.Games[gameList.Games.Count - 1];
                                return (lastGameSyntax, LastPlyInMainVariation(lastGameSyntax));
                            }
                        }

                        pgnSyntax = pgnSyntax.ParentSyntax;
                    }
                }
            }

            return (null, null);
        }
    }
}

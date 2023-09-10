#region License
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
        public static (PgnGameSyntax game, PgnPlySyntax deepestPly) GameAtOrBeforePosition(this PgnEditor pgnEditor, int position)
        {
            PgnPlySyntax deepestPlySyntax = null;

            // We're looking for the symbols before and after the position.
            // If the position is right at the edge between two games, return the previous game; any trivia is part of the next game,
            // and so it's more likely we're closer to the previous game.
            // Hence, take the first symbol from the enumeration.
            if (pgnEditor.SyntaxTree != null
                && pgnEditor.SyntaxTree.GameListSyntax.TerminalSymbolsInRange(position - 1, 2).Any(out IPgnSymbol symbolAtCursor))
            {
                PgnSyntax pgnSyntax = symbolAtCursor.ToSyntax();
                while (pgnSyntax != null)
                {
                    pgnSyntax = pgnSyntax.ParentSyntax;
                    if (deepestPlySyntax == null && pgnSyntax is PgnPlySyntax plySyntax)
                    {
                        deepestPlySyntax = plySyntax;
                    }
                    else if (pgnSyntax is PgnGameSyntax pgnGameSyntax)
                    {
                        return (pgnGameSyntax, deepestPlySyntax);
                    }
                }
            }

            return (null, null);
        }
    }
}

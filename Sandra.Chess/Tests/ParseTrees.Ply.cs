#region License
/*********************************************************************************
 * ParseTrees.Ply.cs
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

using Sandra.Chess.Pgn;
using System.Collections.Generic;

namespace Sandra.Chess.Tests
{
    public static partial class ParseTrees
    {
        internal static List<(string, ParseTree)> PlyParseTrees() => new List<(string, ParseTree)>
        {
            ("*", Games(Game(ResultNoTrivia))),
            ("1-0", Games(Game(ResultNoTrivia))),
            ("0-1", Games(Game(ResultNoTrivia))),
            ("1/2-1/2", Games(Game(ResultNoTrivia))),
        };

        internal static List<(string, ParseTree, PgnErrorCode[])> PlyParseTreesWithErrors() => new List<(string, ParseTree, PgnErrorCode[])>
        {
            // Single move tree symbols.
            ("0", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats))), new[] { PgnErrorCode.MissingMove }),
            ("1", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats))), new[] { PgnErrorCode.MissingMove }),
            ("2", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats))), new[] { PgnErrorCode.MissingMove }),
            ("3", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats))), new[] { PgnErrorCode.MissingMove }),
            ("8", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats))), new[] { PgnErrorCode.MissingMove }),
            ("10", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats))), new[] { PgnErrorCode.MissingMove }),

            (".", OneGame(EmptyTagSection, Plies(OnePeriod)), new[] { PgnErrorCode.OrphanPeriod }),
            ("a3", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats))), new[] { PgnErrorCode.MissingMoveNumber }),
            ("O", TagSectionOnly(TagPair(TagName)), new[] { PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagValue, PgnErrorCode.MissingTagBracketClose }),
            ("P", TagSectionOnly(TagPair(TagName)), new[] { PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagValue, PgnErrorCode.MissingTagBracketClose }),
            ("N", TagSectionOnly(TagPair(TagName)), new[] { PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagValue, PgnErrorCode.MissingTagBracketClose }),
            ("/", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats))), new[] { PgnErrorCode.UnrecognizedMove, PgnErrorCode.MissingMoveNumber }),
            ("-", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats))), new[] { PgnErrorCode.UnrecognizedMove, PgnErrorCode.MissingMoveNumber }),
            ("=", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats))), new[] { PgnErrorCode.UnrecognizedMove, PgnErrorCode.MissingMoveNumber }),
            ("+", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats))), new[] { PgnErrorCode.UnrecognizedMove, PgnErrorCode.MissingMoveNumber }),
            ("#", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats))), new[] { PgnErrorCode.UnrecognizedMove, PgnErrorCode.MissingMoveNumber }),
            ("!", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats))), new[] { PgnErrorCode.UnrecognizedMove, PgnErrorCode.MissingMoveNumber }),

            ("$", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats))), new[] { PgnErrorCode.EmptyNag, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),
            ("$0", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats))), new[] { PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),
            ("$1", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats))), new[] { PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),
            ("$255", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats))), new[] { PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),
            ("$256", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats))), new[] { PgnErrorCode.OverflowNag, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),
            ("$-1", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats), Ply(MoveNoFloats))),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.UnrecognizedMove, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),

            ("(", OneGame(EmptyTagSection, Plies(Ply(VariationNoFloats(OpenNoTrivia, NoPlies)))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.MissingParenthesisClose, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),
            (")", OneGame(EmptyTagSection, Plies(OneOrphanClose)),
                new[] { PgnErrorCode.OrphanParenthesisClose }),

            // Expect: move number, periods (1 or 3), move, nag, rav*
            // A move by itself is sufficient so shouldn't yield any ply errors, only the missing move number message and missing game result.
            ("Pe4+!", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats))), new[] { PgnErrorCode.MissingMoveNumber }),

            // All combinations of two ply elements.
            ("1 1", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats), Ply(WS_MoveNumberNoFloats))),
                new[] { PgnErrorCode.MissingMove, PgnErrorCode.MissingMove }),
            ("1.", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats), OnePeriod)),
                new[] { PgnErrorCode.MissingMove }),   // This one should NOT report an OrphanPeriod even though it will appear so in the actual AST.
            ("1 a3", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats, WS_MoveNoFloats))),
                new PgnErrorCode[0]),  // Not actually an error right now.
            ("1$", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats, NAGNoFloats))),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.MissingMove }),
            ("1()", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats, VariationOpenClose))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMove }),

            (".1", OneGame(EmptyTagSection, Plies(Ply(WithFloats(OnePeriod, MoveNumberNoTrivia)))),
                new[] { PgnErrorCode.OrphanPeriod, PgnErrorCode.MissingMove }),
            ("..", OneGame(EmptyTagSection, Plies(TwoPeriods)),
                new[] { PgnErrorCode.OrphanPeriod, PgnErrorCode.OrphanPeriod }),
            (".a3", OneGame(EmptyTagSection, Plies(Ply(WithFloats(OnePeriod, MoveNoTrivia)))),
                new[] { PgnErrorCode.OrphanPeriod, PgnErrorCode.MissingMoveNumber }),
            (".$", OneGame(EmptyTagSection, Plies(Ply(WithFloats(OnePeriod, NAGNoTrivia)))),
                new[] { PgnErrorCode.OrphanPeriod, PgnErrorCode.EmptyNag, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),
            (".()", OneGame(EmptyTagSection, Plies(Ply(WithFloats(OnePeriod, Variation(OpenNoTrivia, NoPlies, CloseNoTrivia))))),
                new[] { PgnErrorCode.OrphanPeriod, PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),

            ("a3 1", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats), Ply(WS_MoveNumberNoFloats))),
                new[] { PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),
            ("a3.", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats), OnePeriod)),
                new[] { PgnErrorCode.OrphanPeriod, PgnErrorCode.MissingMoveNumber }),
            ("a3 a3", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats), Ply(WS_MoveNoFloats))),
                new[] { PgnErrorCode.MissingMoveNumber }),
            ("a3$", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats, NAGNoFloats))),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.MissingMoveNumber }),
            ("a3()", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats, VariationOpenClose))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMoveNumber }),

            // '$' and '1' stick together. So distinguish.
            ("$1", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats))),
                new[] { PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),
            ("$ 1", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats), Ply(WS_MoveNumberNoFloats))),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingMove }),
            ("$.", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats), OnePeriod)),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.OrphanPeriod, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),
            ("$a3", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats), Ply(MoveNoFloats))),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),
            ("$$", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats, NAGNoFloats))),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.EmptyNag, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),
            ("$()", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats, VariationOpenClose))),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),

            ("()1", OneGame(EmptyTagSection, Plies(Ply(VariationOpenClose), Ply(MoveNumberNoFloats))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingMove }),
            ("().", OneGame(EmptyTagSection, Plies(Ply(VariationOpenClose), OnePeriod)),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.OrphanPeriod, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),
            ("()a3", OneGame(EmptyTagSection, Plies(Ply(VariationOpenClose), Ply(MoveNoFloats))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),
            ("()$", OneGame(EmptyTagSection, Plies(Ply(VariationOpenClose), Ply(NAGNoFloats))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.EmptyNag, PgnErrorCode.VariationBeforeNAG, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingMove }),
            ("()()", OneGame(EmptyTagSection, Plies(Ply(VariationOpenClose, VariationOpenClose))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),

            // Placement of Numeric Annotation Glyph.
            ("$1.e4()", OneGame(EmptyTagSection, Plies(
                Ply(NAGNoFloats),
                Ply(WithFloats(OnePeriod, MoveNoTrivia), VariationOpenClose))),
                new[] { PgnErrorCode.OrphanPeriod, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.EmptyVariation }),
            ("$ 1.e4()", OneGame(EmptyTagSection, Plies(
                Ply(NAGNoFloats),
                Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, MoveNoTrivia), VariationOpenClose))),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.EmptyVariation }),
            ("1$.e4()", OneGame(EmptyTagSection, Plies(
                Ply(MoveNumberNoFloats, NAGNoFloats),
                Ply(WithFloats(OnePeriod, MoveNoTrivia), VariationOpenClose))),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.MissingMove, PgnErrorCode.EmptyVariation }),
            ("1.$e4()", OneGame(EmptyTagSection, Plies(
                Ply(MoveNumberNoFloats, WithFloats(OnePeriod, NAGNoTrivia)),
                Ply(MoveNoFloats, VariationOpenClose))),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.MissingMove, PgnErrorCode.EmptyVariation }),
            ("1.e4$()", OneGame(EmptyTagSection, Plies(
                Ply(MoveNumberNoFloats, WithFloats(OnePeriod, MoveNoTrivia), new[] { NAGNoFloats }, new[] { VariationOpenClose }))),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.EmptyVariation }),
            ("1.e4()$", OneGame(EmptyTagSection, Plies(
                Ply(MoveNumberNoFloats, WithFloats(OnePeriod, MoveNoTrivia), VariationOpenClose),
                Ply(NAGNoFloats))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.EmptyNag, PgnErrorCode.VariationBeforeNAG, PgnErrorCode.MissingMove }),

            // Placement of period symbol.
            (".1 e4$", OneGame(EmptyTagSection, Plies(Ply(WithFloats(OnePeriod, MoveNumberNoTrivia), WS_MoveNoFloats, NAGNoFloats))),
                new[] { PgnErrorCode.OrphanPeriod, PgnErrorCode.EmptyNag }),
            ("1. e4$", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move), NAGNoFloats))),
                new[] { PgnErrorCode.EmptyNag }),
            ("1 e4. $", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats, WS_MoveNoFloats, WithFloats(OnePeriod, WS_NAG)))),
                new[] { PgnErrorCode.OrphanPeriod, PgnErrorCode.EmptyNag }),
            ("1 e4$.", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats, WS_MoveNoFloats, NAGNoFloats), OnePeriod)),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.OrphanPeriod }),

            // Ply elements followed by a game termination marker.
            ("1*", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats)), ResultNoTrivia),
                new[] { PgnErrorCode.MissingMove }),
            (".*", OneGame(EmptyTagSection, Plies(OnePeriod), ResultNoTrivia),
                new[] { PgnErrorCode.OrphanPeriod }),
            ("a3*", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats)), ResultNoTrivia),
                new[] { PgnErrorCode.MissingMoveNumber }),
            ("$*", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats)), ResultNoTrivia),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),
            ("()*", OneGame(EmptyTagSection, Plies(Ply(VariationOpenClose)), ResultNoTrivia),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),

            // Plies that miss only move number or move.
            ("e4$1()", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats, NAGNoFloats, VariationOpenClose))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMoveNumber }),
            ("1$1()", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats, NAGNoFloats, VariationOpenClose))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMove }),

            // Moves, both recognized, unrecognized, and which could be tag names too: Qef0 should not trigger a new tag section.
            ("1. e5 2", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)), Ply(WS_MoveNumberNoFloats))),
                new[] { PgnErrorCode.MissingMove }),
            ("1. $1 2", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats, WithFloats(OnePeriod, WS_NAG)), Ply(WS_MoveNumberNoFloats))),
                new[] { PgnErrorCode.MissingMove, PgnErrorCode.MissingMove }),
            ("1. e9+ 2", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)), Ply(WS_MoveNumberNoFloats))),
                new[] { PgnErrorCode.UnrecognizedMove, PgnErrorCode.MissingMove }),
            // Below is what you'd expect. But right now Qef0 still triggers a new tag section.
            //("1. Qef0 2", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)), Ply(WS_MoveNumberNoFloats))),
            //    new[] { PgnErrorCode.UnrecognizedMove, PgnErrorCode.MissingMove }),

            // Special case.
            ("½-½", OneGameTrailingTrivia(EmptyTagSection, Plies(Ply(NoFloats(new ParseTree<PgnMoveWithTriviaSyntax> { IllegalCharacterTrivia, Move }))), IllegalCharacterTrivia),
                new[] { PgnErrorCode.IllegalCharacter, PgnErrorCode.IllegalCharacter, PgnErrorCode.UnrecognizedMove, PgnErrorCode.MissingMoveNumber }),
        };
    }
}

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
        internal static List<(string, ParseTree, PgnErrorCode[])> PlyParseTreesWithErrors() => new List<(string, ParseTree, PgnErrorCode[])>
        {
            // Single move tree symbols.
            ("0", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats))), new[] { PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("1", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats))), new[] { PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("2", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats))), new[] { PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("3", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats))), new[] { PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("8", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats))), new[] { PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("10", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats))), new[] { PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),

            (".", OneGame(EmptyTagSection, Plies(OnePeriod)), new[] { PgnErrorCode.OrphanPeriod, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("a3", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats))), new[] { PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("O", TagSectionOnly(TagPair(TagName)), new[] { PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagValue, PgnErrorCode.MissingTagBracketClose, PgnErrorCode.MissingGameTerminationMarker }),
            ("P", TagSectionOnly(TagPair(TagName)), new[] { PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagValue, PgnErrorCode.MissingTagBracketClose, PgnErrorCode.MissingGameTerminationMarker }),
            ("N", TagSectionOnly(TagPair(TagName)), new[] { PgnErrorCode.MissingTagBracketOpen, PgnErrorCode.MissingTagValue, PgnErrorCode.MissingTagBracketClose, PgnErrorCode.MissingGameTerminationMarker }),
            ("/", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats))), new[] { PgnErrorCode.UnrecognizedMove, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("-", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats))), new[] { PgnErrorCode.UnrecognizedMove, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("=", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats))), new[] { PgnErrorCode.UnrecognizedMove, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("+", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats))), new[] { PgnErrorCode.UnrecognizedMove, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("#", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats))), new[] { PgnErrorCode.UnrecognizedMove, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("!", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats))), new[] { PgnErrorCode.UnrecognizedMove, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),

            ("$", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats))), new[] { PgnErrorCode.EmptyNag, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("$0", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats))), new[] { PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("$1", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats))), new[] { PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("$255", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats))), new[] { PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("$256", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats))), new[] { PgnErrorCode.OverflowNag, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("$-1", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats), Ply(MoveNoFloats))),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.UnrecognizedMove, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),

            ("(", OneGame(EmptyTagSection, Plies(Ply(VariationNoFloats(OpenNoTrivia, NoPlies)))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.MissingParenthesisClose, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            (")", OneGame(EmptyTagSection, Plies(OneOrphanClose)),
                new[] { PgnErrorCode.OrphanParenthesisClose, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),

            ("*", Games(Game(ResultNoTrivia)), new[] { PgnErrorCode.MissingTagSection }),
            ("1-0", Games(Game(ResultNoTrivia)), new[] { PgnErrorCode.MissingTagSection }),
            ("0-1", Games(Game(ResultNoTrivia)), new[] { PgnErrorCode.MissingTagSection }),
            ("1/2-1/2", Games(Game(ResultNoTrivia)), new[] { PgnErrorCode.MissingTagSection }),

            // Expect: move number, periods (1 or 3), move, nag, rav*
            // A move by itself is sufficient so shouldn't yield any ply errors, only the missing move number message and missing game result.
            ("Pe4+!", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats))), new[] { PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),

            // All combinations of two ply elements.
            ("1 1", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats), Ply(WS_MoveNumberNoFloats))),
                new[] { PgnErrorCode.MissingMove, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            // This one should NOT report an OrphanPeriod even though it will appear so in the actual AST.
            ("1.", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats), OnePeriod)),
                new[] { PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("1 a3", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats, WS_MoveNoFloats))),
                new[] { PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("1$", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats, NAGNoFloats))),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("1()", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats, VariationOpenClose))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),

            (".1", OneGame(EmptyTagSection, Plies(Ply(WithFloats(OnePeriod, MoveNumberNoTrivia)))),
                new[] { PgnErrorCode.OrphanPeriod, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("..", OneGame(EmptyTagSection, Plies(TwoPeriods)),
                new[] { PgnErrorCode.OrphanPeriod, PgnErrorCode.OrphanPeriod, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            (".a3", OneGame(EmptyTagSection, Plies(Ply(WithFloats(OnePeriod, MoveNoTrivia)))),
                new[] { PgnErrorCode.OrphanPeriod, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            (".$", OneGame(EmptyTagSection, Plies(Ply(WithFloats(OnePeriod, NAGNoTrivia)))),
                new[] { PgnErrorCode.OrphanPeriod, PgnErrorCode.EmptyNag, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            (".()", OneGame(EmptyTagSection, Plies(Ply(WithFloats(OnePeriod, Variation(OpenNoTrivia, NoPlies, CloseNoTrivia))))),
                new[] { PgnErrorCode.OrphanPeriod, PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),

            ("a3 1", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats), Ply(WS_MoveNumberNoFloats))),
                new[] { PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("a3.", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats), OnePeriod)),
                new[] { PgnErrorCode.OrphanPeriod, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("a3 a3", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats), Ply(WS_MoveNoFloats))),
                new[] { PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("a3$", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats, NAGNoFloats))),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("a3()", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats, VariationOpenClose))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),

            // '$' and '1' stick together. So distinguish.
            ("$1", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats))),
                new[] { PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("$ 1", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats), Ply(WS_MoveNumberNoFloats))),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("$.", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats), OnePeriod)),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.OrphanPeriod, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("$a3", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats), Ply(MoveNoFloats))),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("$$", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats, NAGNoFloats))),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.EmptyNag, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("$()", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats, VariationOpenClose))),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),

            ("()1", OneGame(EmptyTagSection, Plies(Ply(VariationOpenClose), Ply(MoveNumberNoFloats))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("().", OneGame(EmptyTagSection, Plies(Ply(VariationOpenClose), OnePeriod)),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.OrphanPeriod, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("()a3", OneGame(EmptyTagSection, Plies(Ply(VariationOpenClose), Ply(MoveNoFloats))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("()$", OneGame(EmptyTagSection, Plies(Ply(VariationOpenClose), Ply(NAGNoFloats))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.EmptyNag, PgnErrorCode.VariationBeforeNAG, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("()()", OneGame(EmptyTagSection, Plies(Ply(VariationOpenClose, VariationOpenClose))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),

            // Placement of Numeric Annotation Glyph.
            ("$1.e4()", OneGame(EmptyTagSection, Plies(
                Ply(NAGNoFloats),
                Ply(WithFloats(OnePeriod, MoveNoTrivia), VariationOpenClose))),
                new[] { PgnErrorCode.OrphanPeriod, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.EmptyVariation, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("$ 1.e4()", OneGame(EmptyTagSection, Plies(
                Ply(NAGNoFloats),
                Ply(WS_MoveNumberNoFloats, WithFloats(OnePeriod, MoveNoTrivia), VariationOpenClose))),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.EmptyVariation, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("1$.e4()", OneGame(EmptyTagSection, Plies(
                Ply(MoveNumberNoFloats, NAGNoFloats),
                Ply(WithFloats(OnePeriod, MoveNoTrivia), VariationOpenClose))),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.MissingMove, PgnErrorCode.EmptyVariation, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("1.$e4()", OneGame(EmptyTagSection, Plies(
                Ply(MoveNumberNoFloats, WithFloats(OnePeriod, NAGNoTrivia)),
                Ply(MoveNoFloats, VariationOpenClose))),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.MissingMove, PgnErrorCode.EmptyVariation, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("1.e4$()", OneGame(EmptyTagSection, Plies(
                Ply(MoveNumberNoFloats, WithFloats(OnePeriod, MoveNoTrivia), new[] { NAGNoFloats }, new[] { VariationOpenClose }))),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.EmptyVariation, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("1.e4()$", OneGame(EmptyTagSection, Plies(
                Ply(MoveNumberNoFloats, WithFloats(OnePeriod, MoveNoTrivia), VariationOpenClose),
                Ply(NAGNoFloats))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.EmptyNag, PgnErrorCode.VariationBeforeNAG, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),

            // Placement of period symbol.
            (".1 e4$", OneGame(EmptyTagSection, Plies(Ply(WithFloats(OnePeriod, MoveNumberNoTrivia), WS_MoveNoFloats, NAGNoFloats))),
                new[] { PgnErrorCode.OrphanPeriod, PgnErrorCode.EmptyNag, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("1. e4$", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move), NAGNoFloats))),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("1 e4. $", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats, WS_MoveNoFloats, WithFloats(OnePeriod, WS_NAG)))),
                new[] { PgnErrorCode.OrphanPeriod, PgnErrorCode.EmptyNag, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("1 e4$.", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats, WS_MoveNoFloats, NAGNoFloats), OnePeriod)),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.OrphanPeriod, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),

            // Ply elements followed by a game termination marker.
            ("1*", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats)), ResultNoTrivia),
                new[] { PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection }),
            (".*", OneGame(EmptyTagSection, Plies(OnePeriod), ResultNoTrivia),
                new[] { PgnErrorCode.OrphanPeriod, PgnErrorCode.MissingTagSection }),
            ("a3*", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats)), ResultNoTrivia),
                new[] { PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingTagSection }),
            ("$*", OneGame(EmptyTagSection, Plies(Ply(NAGNoFloats)), ResultNoTrivia),
                new[] { PgnErrorCode.EmptyNag, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection }),
            ("()*", OneGame(EmptyTagSection, Plies(Ply(VariationOpenClose)), ResultNoTrivia),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection }),

            // Plies that miss only move number or move.
            ("e4$1()", OneGame(EmptyTagSection, Plies(Ply(MoveNoFloats, NAGNoFloats, VariationOpenClose))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("1$1()", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats, NAGNoFloats, VariationOpenClose))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),

            // Moves, both recognized, unrecognized, and which could be tag names too: Qef0 should not trigger a new tag section.
            ("1. e5 2", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)), Ply(WS_MoveNumberNoFloats))),
                new[] { PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("1. $1 2", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats, WithFloats(OnePeriod, WS_NAG)), Ply(WS_MoveNumberNoFloats))),
                new[] { PgnErrorCode.MissingMove, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("1. e9+ 2", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)), Ply(WS_MoveNumberNoFloats))),
                new[] { PgnErrorCode.UnrecognizedMove, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
            ("1. Qef0 2", OneGame(EmptyTagSection, Plies(Ply(MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)), Ply(WS_MoveNumberNoFloats))),
                new[] { PgnErrorCode.UnrecognizedMove, PgnErrorCode.MissingMove, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker }),
        };
    }
}

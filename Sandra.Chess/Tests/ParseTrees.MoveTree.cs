#region License
/*********************************************************************************
 * ParseTrees.MoveTree.cs
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
        internal static List<(string, ParseTree)> MoveTreeParseTrees() => new List<(string, ParseTree)>
        {
        };

        internal static List<(string, ParseTree, PgnErrorCode[])> MoveTreeParseTreesWithErrors() => new List<(string, ParseTree, PgnErrorCode[])>
        {
            // Recursive annotated variations.
            ("()", OneGame(EmptyTagSection, Plies(Ply(VariationOpenClose))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),

            (" ()", OneGame(EmptyTagSection, Plies(Ply(VariationNoFloats(WS_Open, NoPlies, CloseNoTrivia)))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),
            ("( )", OneGame(EmptyTagSection, Plies(Ply(VariationNoFloats(OpenNoTrivia, NoPlies, WS_Close)))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),
            ("() ", OneGameTrailingTrivia(EmptyTagSection, Plies(Ply(VariationOpenClose)), WhitespaceTrivia),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),

            (".()", OneGame(EmptyTagSection, Plies(Ply(WithFloats(OnePeriod, Variation(OpenNoTrivia, NoPlies, CloseNoTrivia))))),
                new[] { PgnErrorCode.OrphanPeriod, PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),
            ("(.)", OneGame(EmptyTagSection, Plies(Ply(VariationNoFloats(OpenNoTrivia, Plies(OnePeriod), CloseNoTrivia)))),
                new[] { PgnErrorCode.OrphanPeriod, PgnErrorCode.EmptyVariation, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),
            ("().", OneGame(EmptyTagSection, Plies(Ply(VariationOpenClose), OnePeriod)),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.OrphanPeriod, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),

            ("[A\"\"]1.e4(", OneGame(SmallestCorrectTagSection, Plies(
                Ply(MoveNumberNoFloats, WithFloats(OnePeriod, MoveNoTrivia), VariationNoFloats(OpenNoTrivia, NoPlies)))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.MissingParenthesisClose }),
            ("[A\"\"]1.e4)", OneGame(SmallestCorrectTagSection, Plies(
                Ply(MoveNumberNoFloats, WithFloats(OnePeriod, MoveNoTrivia)), OneOrphanClose)),
                new[] { PgnErrorCode.OrphanParenthesisClose }),
            ("[A\"\"]1.e4()", OneGame(SmallestCorrectTagSection, Plies(
                Ply(MoveNumberNoFloats, WithFloats(OnePeriod, MoveNoTrivia), VariationOpenClose))),
                new[] { PgnErrorCode.EmptyVariation }),
            ("[A\"\"]1.e4((", OneGame(SmallestCorrectTagSection, Plies(
                Ply(MoveNumberNoFloats, WithFloats(OnePeriod, MoveNoTrivia),
                    VariationNoFloats(OpenNoTrivia, Plies(Ply(VariationNoFloats(OpenNoTrivia, NoPlies))))))),
                new[] { PgnErrorCode.EmptyVariation, PgnErrorCode.MissingParenthesisClose, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),
            ("[A\"\"]1.e4)(", OneGame(SmallestCorrectTagSection, Plies(
                Ply(MoveNumberNoFloats, WithFloats(OnePeriod, MoveNoTrivia), WithFloats(OneOrphanClose, Variation(OpenNoTrivia, NoPlies))))),
                new[] { PgnErrorCode.OrphanParenthesisClose, PgnErrorCode.EmptyVariation, PgnErrorCode.MissingParenthesisClose }),
            ("[A\"\"]1.e4))", OneGame(SmallestCorrectTagSection, Plies(
                Ply(MoveNumberNoFloats, WithFloats(OnePeriod, MoveNoTrivia)), TwoOrphanClose)),
                new[] { PgnErrorCode.OrphanParenthesisClose, PgnErrorCode.OrphanParenthesisClose }),

            // Deeper nesting.
            ("(((", OneGame(EmptyTagSection, Plies(Ply(
                VariationNoFloats(OpenNoTrivia, Plies(Ply(
                    VariationNoFloats(OpenNoTrivia, Plies(Ply(
                        VariationNoFloats(OpenNoTrivia, NoPlies)))))))))),
                new[] {
                    PgnErrorCode.EmptyVariation, PgnErrorCode.MissingParenthesisClose,
                    PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove,
                    PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove,
                    PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),

            ("((((", OneGame(EmptyTagSection, Plies(Ply(
                VariationNoFloats(OpenNoTrivia, Plies(Ply(
                    VariationNoFloats(OpenNoTrivia, Plies(Ply(
                        VariationNoFloats(OpenNoTrivia, Plies(Ply(
                            VariationNoFloats(OpenNoTrivia, NoPlies))))))))))))),
                new[] {
                    PgnErrorCode.EmptyVariation, PgnErrorCode.MissingParenthesisClose,
                    PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove,
                    PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove,
                    PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove,
                    PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),

            ("(1. a3+(1. b3+(1. c3+(1. d3+", OneGame(EmptyTagSection, Plies(Ply(
                VariationNoFloats(OpenNoTrivia, Plies(Ply(
                    MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move),
                    VariationNoFloats(OpenNoTrivia, Plies(Ply(
                        MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move),
                        VariationNoFloats(OpenNoTrivia, Plies(Ply(
                            MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move),
                            VariationNoFloats(OpenNoTrivia, Plies(Ply(
                                MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move)))))))))))))))),
                new[] { PgnErrorCode.MissingParenthesisClose, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),

            ("(1. a3+(1. b3+(1. c3+(1. d3+)", OneGame(EmptyTagSection, Plies(Ply(
                VariationNoFloats(OpenNoTrivia, Plies(Ply(
                    MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move),
                    VariationNoFloats(OpenNoTrivia, Plies(Ply(
                        MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move),
                        VariationNoFloats(OpenNoTrivia, Plies(Ply(
                            MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move),
                            VariationNoFloats(OpenNoTrivia, Plies(Ply(
                                MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move))), CloseNoTrivia))))))))))))),
                new[] { PgnErrorCode.MissingParenthesisClose, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),

            ("(1. a3+(1. b3+(1. c3+)(1. d3+", OneGame(EmptyTagSection, Plies(Ply(
                VariationNoFloats(OpenNoTrivia, Plies(Ply(
                    MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move),
                    VariationNoFloats(OpenNoTrivia, Plies(PlyVariations(
                        MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move),
                        VariationNoFloats(OpenNoTrivia, Plies(Ply(
                            MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move))), CloseNoTrivia),
                        VariationNoFloats(OpenNoTrivia, Plies(Ply(
                            MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move))))))))))))),
                new[] { PgnErrorCode.MissingParenthesisClose, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),

            ("(1. a3+(1. b3+)(1. c3+(1. d3+", OneGame(EmptyTagSection, Plies(Ply(
                VariationNoFloats(OpenNoTrivia, Plies(PlyVariations(
                    MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move),
                    VariationNoFloats(OpenNoTrivia, Plies(Ply(
                        MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move))), CloseNoTrivia),
                    VariationNoFloats(OpenNoTrivia, Plies(Ply(
                        MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move),
                        VariationNoFloats(OpenNoTrivia, Plies(Ply(
                            MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move))))))))))))),
                new[] { PgnErrorCode.MissingParenthesisClose, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),

            ("(1. a3+)(1. b3+(1. c3+(1. d3+", OneGame(EmptyTagSection, Plies(Ply(
                VariationNoFloats(OpenNoTrivia, Plies(Ply(
                    MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move))), CloseNoTrivia),
                VariationNoFloats(OpenNoTrivia, Plies(Ply(
                    MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move),
                    VariationNoFloats(OpenNoTrivia, Plies(Ply(
                        MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move),
                        VariationNoFloats(OpenNoTrivia, Plies(Ply(
                            MoveNumberNoFloats, WithFloats(OnePeriod, WS_Move))))))))))))),
                new[] { PgnErrorCode.MissingParenthesisClose, PgnErrorCode.MissingMoveNumber, PgnErrorCode.MissingMove }),
        };
    }
}

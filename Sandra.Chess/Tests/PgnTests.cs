﻿#region License
/*********************************************************************************
 * PgnTests.cs
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

using Eutherion.Testing;
using Eutherion.Text;
using Sandra.Chess.Pgn;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;

namespace Sandra.Chess.Tests
{
    public class PgnTests
    {
        private sealed class ToGreenSymbolConverter : PgnSymbolVisitor<IGreenPgnSymbol>
        {
            public override IGreenPgnSymbol VisitBracketCloseSyntax(PgnBracketCloseSyntax node) => node.Green;
            public override IGreenPgnSymbol VisitBracketOpenSyntax(PgnBracketOpenSyntax node) => node.Green;
            public override IGreenPgnSymbol VisitCommentSyntax(PgnCommentSyntax node) => node.Green;
            public override IGreenPgnSymbol VisitEscapeSyntax(PgnEscapeSyntax node) => node.Green;
            public override IGreenPgnSymbol VisitGameResultSyntax(PgnGameResultSyntax node) => node.Green;
            public override IGreenPgnSymbol VisitIllegalCharacterSyntax(PgnIllegalCharacterSyntax node) => node.Green;
            public override IGreenPgnSymbol VisitMoveNumberSyntax(PgnMoveNumberSyntax node) => node.Green;
            public override IGreenPgnSymbol VisitMoveSyntax(PgnMoveSyntax node) => node.Green;
            public override IGreenPgnSymbol VisitNagSyntax(PgnNagSyntax node) => node.Green;
            public override IGreenPgnSymbol VisitOrphanParenthesisCloseSyntax(PgnOrphanParenthesisCloseSyntax node) => GreenPgnParenthesisCloseSyntax.Value;
            public override IGreenPgnSymbol VisitParenthesisCloseSyntax(PgnParenthesisCloseSyntax node) => node.Green;
            public override IGreenPgnSymbol VisitParenthesisOpenSyntax(PgnParenthesisOpenSyntax node) => node.Green;
            public override IGreenPgnSymbol VisitPeriodSyntax(PgnPeriodSyntax node) => node.Green;
            public override IGreenPgnSymbol VisitTagElementInMoveTreeSyntax(PgnTagElementInMoveTreeSyntax node) => node.Green.TagElement;
            public override IGreenPgnSymbol VisitTagNameSyntax(PgnTagNameSyntax node) => node.Green;
            public override IGreenPgnSymbol VisitTagValueSyntax(PgnTagValueSyntax node) => node.Green;
            public override IGreenPgnSymbol VisitWhitespaceSyntax(PgnWhitespaceSyntax node) => node.Green;
        }

        /// <summary>
        /// Indicates if two symbols of the same type should combine into one.
        /// </summary>
        private static bool WillCombine(Type tokenType1, Type tokenType2, out Type resultTokenType)
        {
            if (tokenType1 == typeof(GreenPgnWhitespaceSyntax))
            {
                if (tokenType1 == tokenType2)
                {
                    resultTokenType = tokenType1;
                    return true;
                }
            }
            else if (tokenType1 == typeof(GreenPgnTagNameSyntax))
            {
                if (tokenType2 == typeof(GreenPgnTagNameSyntax)
                    || tokenType2 == typeof(GreenPgnMoveNumberSyntax))
                {
                    resultTokenType = tokenType1;
                    return true;
                }
                else if (tokenType2 == typeof(GreenPgnUnrecognizedMoveSyntax))
                {
                    resultTokenType = tokenType2;
                    return true;
                }
            }
            else if (tokenType1 == typeof(GreenPgnUnrecognizedMoveSyntax))
            {
                if (tokenType2 == typeof(GreenPgnUnrecognizedMoveSyntax)
                    || tokenType2 == typeof(GreenPgnTagNameSyntax)
                    || tokenType2 == typeof(GreenPgnMoveNumberSyntax))
                {
                    resultTokenType = tokenType1;
                    return true;
                }
            }
            else if (tokenType1 == typeof(GreenPgnEmptyNagSyntax)
                || tokenType1 == typeof(GreenPgnNagSyntax)
                || tokenType1 == typeof(GreenPgnOverflowNagSyntax))
            {
                if (tokenType2 == typeof(GreenPgnMoveNumberSyntax))
                {
                    resultTokenType = typeof(GreenPgnOverflowNagSyntax);
                    return true;
                }
            }
            else if (tokenType1 == typeof(GreenPgnMoveNumberSyntax))
            {
                if (tokenType2 == typeof(GreenPgnMoveNumberSyntax))
                {
                    resultTokenType = tokenType1;
                    return true;
                }
                else if (tokenType2 == typeof(GreenPgnUnrecognizedMoveSyntax)
                    || tokenType2 == typeof(GreenPgnTagNameSyntax))
                {
                    resultTokenType = typeof(GreenPgnUnrecognizedMoveSyntax);
                    return true;
                }
            }

            resultTokenType = default;
            return false;
        }

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Private backing field of public counterpart")]
        private static IEnumerable<(string pgn, Type expectedType)> _PgnTestSymbols()
        {
            yield return (" ", typeof(GreenPgnWhitespaceSyntax));
            yield return ("\r", typeof(GreenPgnWhitespaceSyntax));
            yield return ("\n", typeof(GreenPgnWhitespaceSyntax));
            yield return ("<", typeof(GreenPgnIllegalCharacterSyntax));
            yield return ("Ā", typeof(GreenPgnIllegalCharacterSyntax));
            yield return ("*", typeof(GreenPgnAsteriskSyntax));
            yield return ("[", typeof(GreenPgnBracketOpenSyntax));
            yield return ("]", typeof(GreenPgnBracketCloseSyntax));
            yield return (")", typeof(GreenPgnParenthesisCloseSyntax));
            yield return ("(", typeof(GreenPgnParenthesisOpenSyntax));
            yield return (".", typeof(GreenPgnPeriodSyntax));
            yield return ("a1=", typeof(GreenPgnUnrecognizedMoveSyntax));
            yield return ("Ø1", typeof(GreenPgnTagNameSyntax));
            yield return ("\"\"", typeof(GreenPgnTagValueSyntax));
            yield return ("\" \"", typeof(GreenPgnTagValueSyntax));
            yield return ("\"a1\"", typeof(GreenPgnTagValueSyntax));
            yield return ("\"\\\"\"", typeof(GreenPgnTagValueSyntax));
            yield return ("\"é\"", typeof(GreenPgnTagValueSyntax));
            yield return ("\"\\n\"", typeof(GreenPgnErrorTagValueSyntax));
            yield return ("\"\n\"", typeof(GreenPgnErrorTagValueSyntax));
            yield return ("{}", typeof(GreenPgnCommentSyntax));
            yield return ("$", typeof(GreenPgnEmptyNagSyntax));
            yield return ("$1", typeof(GreenPgnNagSyntax));
            yield return ("$256", typeof(GreenPgnOverflowNagSyntax));
            yield return ("256", typeof(GreenPgnMoveNumberSyntax));  // pick a big move number to safely predict GreenPgnOverflowNagSyntax.
        }

        private static IEnumerable<(string pgn, Type expectedType)> UnterminatedPgnTestSymbols()
        {
            yield return ("\"", typeof(GreenPgnErrorTagValueSyntax));
            yield return ("\"\\", typeof(GreenPgnErrorTagValueSyntax));
            yield return ("\"\\\"", typeof(GreenPgnErrorTagValueSyntax));
            yield return (";\r", typeof(GreenPgnCommentSyntax));
            yield return ("{", typeof(GreenPgnUnterminatedCommentSyntax));
        }

        private static IEnumerable<IGreenPgnSymbol> TerminalSymbols(string pgn)
        {
            var rootPgnSyntax = PgnParser.Parse(pgn).GameListSyntax;
            var converter = new ToGreenSymbolConverter();

            var totalLength = 0;
            foreach (var symbol in rootPgnSyntax.TerminalSymbolsInRange(0, pgn.Length))
            {
                Assert.Equal(totalLength, symbol.ToSyntax().AbsoluteStart);
                totalLength += symbol.Length;
                yield return converter.Visit(symbol);
            }
        }

        private static void AssertTokens(string pgn, params Action<IGreenPgnSymbol>[] elementInspectors)
            => Assert.Collection(TerminalSymbols(pgn), elementInspectors);

        private static Action<IGreenPgnSymbol> ExpectToken(Type expectedTokenType, int expectedLength)
        {
            return symbol =>
            {
                Assert.NotNull(symbol);

                if (expectedTokenType == typeof(GreenPgnTagNameSyntax))
                {
                    // Exception: symbol could have been converted to an unrecognized move.
                    if (symbol is GreenPgnUnrecognizedMoveSyntax unrecognizedMove)
                    {
                        Assert.True(unrecognizedMove.IsConvertedFromTagName);
                    }
                    else
                    {
                        Assert.IsType(expectedTokenType, symbol);
                    }
                }
                else
                {
                    Assert.IsType(expectedTokenType, symbol);
                }

                Assert.Equal(expectedLength, symbol.Length);
            };
        }

        private static Action<IGreenPgnSymbol> ExpectToken<TExpected>(int expectedLength)
            => ExpectToken(typeof(TExpected), expectedLength);

        [Fact]
        public void ArgumentChecks()
        {
            Assert.Throws<ArgumentNullException>("pgn", () => new RootPgnSyntax(null, new GreenPgnGameListSyntax(ReadOnlySpanList<GreenPgnGameSyntax>.Empty, GreenPgnTriviaSyntax.Empty), ReadOnlyList<PgnErrorInfo>.Empty));
            Assert.Throws<ArgumentNullException>("gameListSyntax", () => new RootPgnSyntax(string.Empty, null, ReadOnlyList<PgnErrorInfo>.Empty));
            Assert.Throws<ArgumentNullException>("errors", () => new RootPgnSyntax(string.Empty, new GreenPgnGameListSyntax(ReadOnlySpanList<GreenPgnGameSyntax>.Empty, GreenPgnTriviaSyntax.Empty), null));

            Assert.Throws<ArgumentNullException>(() => TerminalSymbols(null).Any());

            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnTagNameSyntax(-1, false));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnTagNameSyntax(0, false));

            Assert.Throws<ArgumentNullException>("value", () => new GreenPgnTagValueSyntax(null, 1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnTagValueSyntax(string.Empty, -1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnTagValueSyntax(string.Empty, 0));

            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnErrorTagValueSyntax(-1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnErrorTagValueSyntax(0));

            Assert.Throws<ArgumentOutOfRangeException>("length", () => GreenPgnWhitespaceSyntax.Create(-1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => GreenPgnWhitespaceSyntax.Create(0));

            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnCommentSyntax(-1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnCommentSyntax(0));

            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnUnterminatedCommentSyntax(-1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnUnterminatedCommentSyntax(0));

            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnEscapeSyntax(-1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnEscapeSyntax(0));

            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnNagSyntax(PgnAnnotation.Null, -1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnNagSyntax(PgnAnnotation.Null, 0));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnNagSyntax(PgnAnnotation.Null, 1));

            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnOverflowNagSyntax(-1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnOverflowNagSyntax(2));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnOverflowNagSyntax(3));

            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnMoveNumberSyntax(-1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnMoveNumberSyntax(0));

            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnMoveSyntax(-1, false));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnMoveSyntax(0, false));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnMoveSyntax(1, false));

            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnUnrecognizedMoveSyntax(-1, false));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenPgnUnrecognizedMoveSyntax(0, false));

            Assert.Throws<ArgumentNullException>("backgroundBefore", () => new GreenPgnTriviaElementSyntax(null, new GreenPgnCommentSyntax(1)));
            Assert.Throws<ArgumentNullException>("commentNode", () => new GreenPgnTriviaElementSyntax(ReadOnlySpanList<GreenPgnBackgroundSyntax>.Empty, null));

            Assert.Throws<ArgumentNullException>("commentNodes", () => GreenPgnTriviaSyntax.Create(null, ReadOnlySpanList<GreenPgnBackgroundSyntax>.Empty));
            Assert.Throws<ArgumentNullException>("backgroundAfter", () => GreenPgnTriviaSyntax.Create(ReadOnlySpanList<GreenPgnTriviaElementSyntax>.Empty, null));

            Assert.Throws<ArgumentNullException>("leadingTrivia", () => new GreenWithTriviaSyntax(null, GreenPgnBracketOpenSyntax.Value));
            Assert.Throws<ArgumentNullException>("contentNode", () => new GreenWithTriviaSyntax(GreenPgnTriviaSyntax.Empty, null));

            Assert.Throws<ArgumentNullException>("tagElementNodes", () => new GreenPgnTagPairSyntax(null));
            Assert.Throws<ArgumentException>("tagElementNodes", () => new GreenPgnTagPairSyntax(ReadOnlySpanList<GreenWithTriviaSyntax>.Empty));

            Assert.Throws<ArgumentNullException>("tagPairNodes", () => GreenPgnTagSectionSyntax.Create(null));
            Assert.Same(GreenPgnTagSectionSyntax.Empty, GreenPgnTagSectionSyntax.Create(ReadOnlySpanList<GreenPgnTagPairSyntax>.Empty));

            Assert.Throws<ArgumentNullException>("leadingFloatItems", () => new GreenWithPlyFloatItemsSyntax<GreenWithTriviaSyntax>(null, new GreenWithTriviaSyntax(GreenPgnTriviaSyntax.Empty, GreenPgnNagSyntax.Empty)));
            Assert.Throws<ArgumentNullException>("plyContentNode", () => new GreenWithPlyFloatItemsSyntax<GreenWithTriviaSyntax>(ReadOnlySpanList<GreenWithTriviaSyntax>.Empty, null));

            Assert.Throws<ArgumentNullException>("tagElement", () => new GreenPgnTagElementInMoveTreeSyntax(null));
            Assert.Throws<ArgumentException>("tagElement", () => new GreenPgnTagElementInMoveTreeSyntax(new GreenPgnUnterminatedCommentSyntax(1)));
            Assert.Throws<ArgumentException>("tagElement", () => new GreenPgnTagElementInMoveTreeSyntax(new GreenPgnMoveNumberSyntax(1)));
            Assert.Throws<ArgumentException>("tagElement", () => new GreenPgnTagElementInMoveTreeSyntax(new GreenPgnTagNameSyntax(1, false)));

            Assert.Throws<ArgumentNullException>("nags", () => new GreenPgnPlySyntax(null, null, null, ReadOnlySpanList<GreenWithPlyFloatItemsSyntax<GreenPgnVariationSyntax>>.Empty));
            Assert.Throws<ArgumentNullException>("variations", () => new GreenPgnPlySyntax(null, null, ReadOnlySpanList<GreenWithPlyFloatItemsSyntax<GreenWithTriviaSyntax>>.Empty, null));
            Assert.Throws<ArgumentException>(() => new GreenPgnPlySyntax(null, null, ReadOnlySpanList<GreenWithPlyFloatItemsSyntax<GreenWithTriviaSyntax>>.Empty, ReadOnlySpanList<GreenWithPlyFloatItemsSyntax<GreenPgnVariationSyntax>>.Empty));

            Assert.Throws<ArgumentNullException>("plies", () => GreenPgnPlyListSyntax.Create(null, ReadOnlySpanList<GreenWithTriviaSyntax>.Empty));
            Assert.Throws<ArgumentNullException>("trailingFloatItems", () => GreenPgnPlyListSyntax.Create(ReadOnlySpanList<GreenPgnPlySyntax>.Empty, null));

            Assert.Throws<ArgumentNullException>("parenthesisOpen", () => new GreenPgnVariationSyntax(null, GreenPgnPlyListSyntax.Empty, null));
            Assert.Throws<ArgumentNullException>("pliesWithFloatItems", () => new GreenPgnVariationSyntax(new GreenWithTriviaSyntax(GreenPgnTriviaSyntax.Empty, GreenPgnPeriodSyntax.Value), null, null));

            Assert.Throws<ArgumentNullException>("tagSection", () => new GreenPgnGameSyntax(null, GreenPgnPlyListSyntax.Empty, null));
            Assert.Throws<ArgumentNullException>("plyList", () => new GreenPgnGameSyntax(GreenPgnTagSectionSyntax.Empty, null, null));

            Assert.Throws<ArgumentNullException>("games", () => new GreenPgnGameListSyntax(null, GreenPgnTriviaSyntax.Empty));
            Assert.Throws<ArgumentNullException>("trailingTrivia", () => new GreenPgnGameListSyntax(ReadOnlySpanList<GreenPgnGameSyntax>.Empty, null));
        }

        [Theory]
        [InlineData(-1, 0, "start")]
        [InlineData(-1, -1, "start")]
        [InlineData(0, -1, "length")]
        public void OutOfRangeArgumentsInError(int start, int length, string parameterName)
        {
            Assert.Throws<ArgumentOutOfRangeException>(parameterName, () => new PgnErrorInfo(0, start, length));
        }

        [Fact]
        public void EmptyPgnEmptyTokens()
        {
            Assert.False(TerminalSymbols(string.Empty).Any());
        }

        [Theory]
        [InlineData(PgnErrorCode.IllegalCharacter, 0, 0, null)]
        [InlineData((PgnErrorCode)(-1), 0, 1, new string[0])]
        [InlineData((PgnErrorCode)1, 1, 0, new[] { "\n", "" })]
        [InlineData((PgnErrorCode)999, 0, 2, new[] { "Aa" })]
        public void UnchangedParametersInError(PgnErrorCode errorCode, int start, int length, string[] parameters)
        {
            var errorInfo = new PgnErrorInfo(errorCode, start, length, parameters);
            Assert.Equal(errorCode, errorInfo.ErrorCode);
            Assert.Equal(start, errorInfo.Start);
            Assert.Equal(length, errorInfo.Length);

            // Select Assert.Equal() overload for collections so elements get compared rather than the array by reference.
            Assert.Equal<string>(parameters, errorInfo.Parameters);
        }

        [Theory]
        [InlineData("\\t", 0)]
        [InlineData("\\v", 10)]
        public void IllegalCharacterError(string displayCharValue, int position)
        {
            var error = PgnIllegalCharacterSyntax.CreateError(displayCharValue, position);
            Assert.NotNull(error);
            Assert.Equal(PgnErrorCode.IllegalCharacter, error.ErrorCode);
            Assert.Collection(error.Parameters, x => Assert.Equal(displayCharValue, x));
            Assert.Equal(position, error.Start);
            Assert.Equal(1, error.Length);
        }

        private const int SharedWhitespaceInstanceLengthMinusTwo = GreenPgnWhitespaceSyntax.SharedWhitespaceInstanceLength - 2;
        private const int SharedWhitespaceInstanceLengthMinusOne = GreenPgnWhitespaceSyntax.SharedWhitespaceInstanceLength - 1;
        private const int SharedWhitespaceInstanceLengthPlusOne = GreenPgnWhitespaceSyntax.SharedWhitespaceInstanceLength + 1;
        private const int SharedWhitespaceInstanceLengthPlusTwo = GreenPgnWhitespaceSyntax.SharedWhitespaceInstanceLength + 2;

        [Theory]
        [InlineData(1)]
        [InlineData(SharedWhitespaceInstanceLengthMinusTwo)]
        [InlineData(SharedWhitespaceInstanceLengthMinusOne)]
        [InlineData(GreenPgnWhitespaceSyntax.SharedWhitespaceInstanceLength)]
        [InlineData(SharedWhitespaceInstanceLengthPlusOne)]
        [InlineData(SharedWhitespaceInstanceLengthPlusTwo)]
        public void WhitespaceHasCorrectLength(int length)
        {
            Assert.Equal(length, GreenPgnWhitespaceSyntax.Create(length).Length);
        }

        public static IEnumerable<object[]> TwoPgnTestSymbolCombinations
            => from x1 in _PgnTestSymbols()
               from x2 in _PgnTestSymbols().Concat(UnterminatedPgnTestSymbols())
               select new object[] { x1.pgn, x1.expectedType, x2.pgn, x2.expectedType };

        /// <summary>
        /// Tests all combinations of three tokens.
        /// </summary>
        [Theory]
        [MemberData(nameof(TwoPgnTestSymbolCombinations))]
        public void Transition(string pgn1, Type type1, string pgn2, Type type2)
        {
            // Instead of having a gazillion separate tests over 3 tokens,
            // first test the combination of 2 tokens, and then if that succeeds
            // test every other token that could precede it in a loop.
            {
                if (WillCombine(type1, type2, out Type type12))
                {
                    AssertTokens(
                        pgn1 + pgn2,
                        ExpectToken(type12, pgn1.Length + pgn2.Length));
                }
                else
                {
                    AssertTokens(
                        pgn1 + pgn2,
                        ExpectToken(type1, pgn1.Length),
                        ExpectToken(type2, pgn2.Length));
                }
            }

            // Here Assert.Collection is used so if such a test fails,
            // it gives the index of the third token that was tested.
            Assert.Collection(
                _PgnTestSymbols(),
                Enumerable.Repeat<Action<(string pgn, Type expectedType)>>(x0 =>
                {
                    // Put the third symbol in front, because the last symbol may eat it.
                    string pgn0 = x0.pgn;
                    Type type0 = x0.expectedType;

                    // Exceptions for when 2 or 3 PGN tokens go together and make 1.
                    if (WillCombine(type0, type1, out Type type01))
                    {
                        if (WillCombine(type01, type2, out Type type012))
                        {
                            AssertTokens(
                                pgn0 + pgn1 + pgn2,
                                ExpectToken(type012, pgn0.Length + pgn1.Length + pgn2.Length));
                        }
                        else
                        {
                            AssertTokens(
                                pgn0 + pgn1 + pgn2,
                                ExpectToken(type01, pgn0.Length + pgn1.Length),
                                ExpectToken(type2, pgn2.Length));
                        }
                    }
                    else if (WillCombine(type1, type2, out Type type12))
                    {
                        AssertTokens(
                            pgn0 + pgn1 + pgn2,
                            ExpectToken(type0, pgn0.Length),
                            ExpectToken(type12, pgn1.Length + pgn2.Length));
                    }
                    else
                    {
                        AssertTokens(
                            pgn0 + pgn1 + pgn2,
                            ExpectToken(type0, pgn0.Length),
                            ExpectToken(type1, pgn1.Length),
                            ExpectToken(type2, pgn2.Length));
                    }
                }, _PgnTestSymbols().Count()).ToArray());
        }

        public static IEnumerable<object[]> AllPgnTestSymbols
            => TestUtilities.Wrap(_PgnTestSymbols().Concat(UnterminatedPgnTestSymbols()));

        /// <summary>
        /// Tests all combinations of a single line comment followed by another symbol.
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPgnTestSymbols))]
        public void SingleLineCommentTransitions(string pgn, Type type)
        {
            // Single line comment + symbol.
            // Special treatment if 'pgn' contains a linebreak.
            var commentThenPgn = ";" + pgn;

            int linefeedIndex = pgn.IndexOf('\n');
            if (linefeedIndex >= 0)
            {
                // Special case: the pgn contains a '\n'.
                // Only assert the single line comment, and that the next symbol is whitespace.
                var symbols = TerminalSymbols(commentThenPgn).ToList();
                Assert.True(symbols.Count >= 2);
                ExpectToken<GreenPgnCommentSyntax>(1 + linefeedIndex)(symbols[0]);
                Assert.IsType<GreenPgnWhitespaceSyntax>(symbols[1]);
            }
            else
            {
                // Single line comment should eat everything.
                AssertTokens(
                    commentThenPgn,
                    ExpectToken<GreenPgnCommentSyntax>(1 + pgn.Length));
            }

            // Single line comment + newline + token.
            var commentThenNewLineThenPgn = ";\n" + pgn;

            if (type == typeof(GreenPgnWhitespaceSyntax))
            {
                // Second token should include the '\n'.
                AssertTokens(
                    commentThenNewLineThenPgn,
                    ExpectToken<GreenPgnCommentSyntax>(1),
                    ExpectToken<GreenPgnWhitespaceSyntax>(1 + pgn.Length));
            }
            else
            {
                AssertTokens(
                    commentThenNewLineThenPgn,
                    ExpectToken<GreenPgnCommentSyntax>(1),
                    ExpectToken<GreenPgnWhitespaceSyntax>(1),
                    ExpectToken(type, pgn.Length));
            }
        }

        public static IEnumerable<object[]> AllPgnTestSymbolsWithoutTypes
            => TestUtilities.Wrap(_PgnTestSymbols().Concat(UnterminatedPgnTestSymbols()).Select(x => x.pgn));

        /// <summary>
        /// Tests that all symbols are eaten by a multi-line comment.
        /// </summary>
        [Theory]
        [MemberData(nameof(AllPgnTestSymbolsWithoutTypes))]
        public void MultiLineCommentTransitions(string pgn)
        {
            // Symbol embedded in a multi-line comment.
            // Special treatment if 'pgn' contains a '}'.
            var commentedPgn = "{" + pgn + "}";

            int curlyCloseIndex = pgn.IndexOf('}');
            if (curlyCloseIndex >= 0)
            {
                var symbols = TerminalSymbols(commentedPgn).ToList();
                Assert.True(symbols.Count >= 2);
                ExpectToken<GreenPgnCommentSyntax>(2 + curlyCloseIndex)(symbols[0]);
            }
            else
            {
                AssertTokens(
                    commentedPgn,
                    ExpectToken<GreenPgnCommentSyntax>(2 + pgn.Length));
            }
        }

        [Fact]
        public void EscapeMechanism()
        {
            AssertTokens(
                "% ",
                ExpectToken<GreenPgnEscapeSyntax>(2));

            AssertTokens(
                " %",
                ExpectToken<GreenPgnWhitespaceSyntax>(1),
                ExpectToken<GreenPgnIllegalCharacterSyntax>(1));

            AssertTokens(
                "z%",
                ExpectToken<GreenPgnTagNameSyntax>(1),
                ExpectToken<GreenPgnIllegalCharacterSyntax>(1));

            AssertTokens(
                "\n%\n\n",
                ExpectToken<GreenPgnWhitespaceSyntax>(1),
                ExpectToken<GreenPgnEscapeSyntax>(1),
                ExpectToken<GreenPgnWhitespaceSyntax>(2));

            // Don't trigger escape mechanism inside comments or strings.
            // It would mean that tokens overlap.
            AssertTokens(
                "{\n%}",
                ExpectToken<GreenPgnCommentSyntax>(4));

            AssertTokens(
                ";%",
                ExpectToken<GreenPgnCommentSyntax>(2));

            AssertTokens(
                "\"\n%\"",
                ExpectToken<GreenPgnErrorTagValueSyntax>(4));
        }

        private static (string pgn, Type type) SMTestCase<T>(string pgn)
            => (pgn, typeof(T));

        private static IEnumerable<(string pgn, Type type)> StateMachineSymbols()
        {
            // Special case because if '%' is the first character on a line, it triggers the escape mechanism.
            yield return SMTestCase<GreenPgnEscapeSyntax>("%");

            const string fileLetters = "ah";
            const string nonFileLetters = "NOPXix";
            const string allLetters = nonFileLetters + fileLetters;
            const string nonRankDigits = "09";
            const string rankDigits = "1238";
            const string allDigits = nonRankDigits + rankDigits;

            // Test cases with 1 and 2 characters.
            var uppercaseTagNames = nonFileLetters.SelectMany(x =>
                new SingleElementEnumerable<string>($"{x}")
                .Concat(allLetters.SelectMany(y => new SingleElementEnumerable<string>($"{x}{y}")))
                .Concat(allDigits.SelectMany(y => new SingleElementEnumerable<string>($"{x}{y}"))));

            foreach (var uppercaseTagName in uppercaseTagNames) yield return SMTestCase<GreenPgnTagNameSyntax>(uppercaseTagName);

            var lowercaseTagNames = fileLetters.SelectMany(x =>
                new SingleElementEnumerable<string>($"{x}")
                .Concat(allLetters.SelectMany(y => new SingleElementEnumerable<string>($"{x}{y}")))
                .Concat(nonRankDigits.SelectMany(y => new SingleElementEnumerable<string>($"{x}{y}"))));

            foreach (var lowercaseTagName in lowercaseTagNames) yield return SMTestCase<GreenPgnTagNameSyntax>(lowercaseTagName);

            var moveNumbers = allDigits.SelectMany(x =>
                new SingleElementEnumerable<string>($"{x}")
                .Concat(allDigits.SelectMany(y => new SingleElementEnumerable<string>($"{x}{y}"))));

            foreach (var moveNumber in moveNumbers) yield return SMTestCase<GreenPgnMoveNumberSyntax>(moveNumber);

            // Termination markers.
            yield return SMTestCase<GreenPgnDrawMarkerSyntax>("1/2-1/2");
            yield return SMTestCase<GreenPgnWhiteWinMarkerSyntax>("1-0");
            yield return SMTestCase<GreenPgnBlackWinMarkerSyntax>("0-1");

            // Castling moves.
            yield return SMTestCase<GreenPgnMoveSyntax>("O-O");
            yield return SMTestCase<GreenPgnMoveSyntax>("O-O-O");

            // Pawn moves.
            var fileRanks = fileLetters.SelectMany(x => rankDigits.Select(y => $"{x}{y}"));
            var capturePawnMoves = fileLetters.SelectMany(x => fileRanks.Select(xy => $"{x}x{xy}"));

            var pawnMoves = fileRanks
                .Concat(capturePawnMoves).SelectMany(m =>
                    new SingleElementEnumerable<string>($"{m}")
                    .Concat(new SingleElementEnumerable<string>($"P{m}")));

            foreach (var pawnMove in pawnMoves) yield return SMTestCase<GreenPgnMoveSyntax>(pawnMove);

            // Allow promotion to king, then assume the other piece letters work too.
            var promotionMoves = fileRanks.Concat(capturePawnMoves).Select(m => $"{m}=K");

            foreach (var promotionMove in promotionMoves) yield return SMTestCase<GreenPgnMoveSyntax>(promotionMove);

            // Non-pawn moves.
            var simpleNonPawnMoves = fileRanks.Select(xy => $"Q{xy}");
            var disambiguationMoves1 = fileLetters.SelectMany(x => fileRanks.Select(xy => $"Q{x}{xy}"));
            var disambiguationMoves2 = rankDigits.SelectMany(y => fileRanks.Select(xy => $"Q{y}{xy}"));
            var disambiguationMoves3 = fileRanks.Select(xy => $"Qa1{xy}");

            var nonPawnMoves = simpleNonPawnMoves
                .Concat(disambiguationMoves1)
                .Concat(disambiguationMoves2)
                .Concat(disambiguationMoves3);

            foreach (var nonPawnMove in nonPawnMoves) yield return SMTestCase<GreenPgnMoveSyntax>(nonPawnMove);

            // Incomplete and invalid moves that are valid tag names (3 characters and longer).
            var invalidMoves = new[]
            {
                "Pax", "Bax", "P1x", "B1x",
                "Pah", "Bah", "P1h", "B1h",
                "axh", "Paxh",
                "ax8", "Pax8",
                "ah8", "Pah8",
                "xa1", "Pxa1",
                "P1xa1",
                "a1xa1", "Pa1xa1",
                "Pi1", "Pi8", "Pa0", "Pa9", "P10", "Pa10", "Pa11",
                "Ni1", "Ni8", "Na0", "Na9", "N10", "Na10", "Na11",
                "Pxi1", "Pxi8", "Pxa0", "Pxa9", "Px10", "Pxa10", "Pxa11",
                "Nxi1", "Nxi8", "Nxa0", "Nxa9", "Nx10", "Nxa10", "Nxa11",
                "R0a8", "R9a8", "Ria8", "Ri1a8", "Ri8a8", "Ra0a8", "Ra9a8", "Ra10a8", "Ra11a8",
                "R0xa8", "R9xa8", "Rixa8", "Ri1xa8", "Ri8xa8", "Ra0xa8", "Ra9xa8", "Ra10xa8", "Ra11xa8",
                "Oa1", "Oa1xb2",
            };

            foreach (var invalidMove in invalidMoves) yield return SMTestCase<GreenPgnTagNameSyntax>(invalidMove);

            var movesWithAnnotations = new[]
            {
                "a1!!", "Pa1?!?",
                "axh8+", "axh8#",
                "h8=N+", "h8=N#",
                "axh2+!", "axh2#!",
                "h8=N+?", "h8=N#?",
                "Be4+?", "Red5!!", "O-O-O#", "Q1a8??", "Ke1e2#?!",
            };

            foreach (var movesWithAnnotation in movesWithAnnotations) yield return SMTestCase<GreenPgnMoveSyntax>(movesWithAnnotation);
        }

        public static IEnumerable<object[]> StateMachineValidSymbols => TestUtilities.Wrap(StateMachineSymbols());

        [Theory]
        [MemberData(nameof(StateMachineValidSymbols))]
        public void SymbolStateMachine(string pgn, Type type)
        {
            AssertTokens(pgn, ExpectToken(type, pgn.Length));
        }

        private static readonly string[] InvalidSymbols = new string[]
        {
            "N-", "h-", "0-", "--", "-",
            "N/", "h/", "0/", "-/", "/",
            "N=", "h=", "0=", "-=", "=",
            "N+", "h+", "0+", "-+", "+",
            "N#", "h#", "0#", "-#", "#",
            "N!", "h!", "0!", "-!", "!",
            "N?", "h?", "0?", "-?", "?",

            "0N", "0O", "0P", "0X", "0a", "0h", "0i", "0x",
            "1N", "1O", "1P", "1X", "1a", "1h", "1i", "1x",
            "2N", "2O", "2P", "2X", "2a", "2h", "2i", "2x",
            "3N", "3O", "3P", "3X", "3a", "3h", "3i", "3x",
            "8N", "8O", "8P", "8X", "8a", "8h", "8i", "8x",
            "9N", "9O", "9P", "9X", "9a", "9h", "9i", "9x",

            // Incomplete and invalid game termination markers.
            "1/", "1/2", "1/2-", "1/2-1", "1/2-1/", "1-", "0-",
            "0-0", "1-1", "1/2-0", "1/2-1", "0-1/2", "1-1/2",
            "1/2-1/0", "1/2-1/1", "1/2-1/3", "1/2-1/8", "1/2-1/9",

            // Incomplete and invalid moves.
            "1xa1", "h1-0","h1-O", "h1-O-O", "hO-O", "a1-h8",
            "h8=", "Ph8=", "axh8=",
            "h8=P", "Ph8=P", "axh8=P",
            "h8=+", "Ph8=+", "axh8=+",
            "h8!+", "Ph8!+", "axh8!+",
            "h8=!", "Ph8=!", "axh8=!",
            "h8!=N", "Ph8!=N", "axh8!=N",
            "h8+=N", "Ph8+=N", "axh8+=N",

            "Na1++", "N1a1++", "Naa1++", "Na1a1++", "N1aa1++",
            "Na1=Q", "N1a1=Q", "Naa1=Q", "Na1a1=Q", "N1aa1=Q",
            "Na1!+", "N1a1!+", "Naa1!+", "Na1a1!+", "N1aa1!+",

            // No interference between castling moves and other types of symbols.
            "Oa1xb2+",
            "N-", "N-O-", "N-O-O-", "N-O-O-O", "N-O", "N-O-O",
            "O-", "O-O-", "O-O-O-", "O-O-O-O",
            "P-", "P-O-", "P-O-O-", "P-O-O-O", "P-O", "P-O-O",
            "0-0-0",

            // Check or annotation in the wrong place.
            "O-+", "O-O-+", "Pe+", "Qe+", "Q2+", "Q2e+", "ax+", "axb+", "Qx+", "Qdx+", "Qd2x+", "Qd2xe+",
            "O-!", "O-O-!", "Pe!", "Qe!", "Q2!", "Q2e!", "ax!", "axb!", "Qx!", "Qdx!", "Qd2x!", "Qd2xe!",
        };

        public static IEnumerable<object[]> StateMachineInvalidSymbols => TestUtilities.Wrap(InvalidSymbols);

        [Theory]
        [MemberData(nameof(StateMachineInvalidSymbols))]
        public void SymbolStateMachineInvalidSymbols(string pgn)
        {
            AssertTokens(pgn, ExpectToken<GreenPgnUnrecognizedMoveSyntax>(pgn.Length));
        }

        private static int AssertParseTree(ParseTrees.ParseTree expectedParseTree, PgnSyntax expectedParent, int expectedStart, PgnSyntax actualParseTree)
        {
            Assert.IsType(expectedParseTree.ExpectedType, actualParseTree);
            Assert.Same(expectedParent, actualParseTree.ParentSyntax);
            Assert.Equal(expectedStart, actualParseTree.Start);

            int expectedChildCount = expectedParseTree.ChildNodes.Count;
            Assert.Equal(expectedChildCount, actualParseTree.ChildCount);

            Assert.Throws<ArgumentOutOfRangeException>(() => actualParseTree.GetChild(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => actualParseTree.GetChild(expectedChildCount));
            Assert.Throws<ArgumentOutOfRangeException>(() => actualParseTree.GetChildStartPosition(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => actualParseTree.GetChildStartPosition(expectedChildCount));
            Assert.Throws<ArgumentOutOfRangeException>(() => actualParseTree.GetChildStartOrEndPosition(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => actualParseTree.GetChildStartOrEndPosition(expectedChildCount + 1));

            int length = 0;

            if (expectedChildCount == 0)
            {
                if (actualParseTree.Length > 0)
                {
                    Assert.True(actualParseTree.IsTerminalSymbol(out IPgnSymbol pgnSymbol));
                    length = pgnSymbol.Length;
                }
                else
                {
                    Assert.False(actualParseTree.IsTerminalSymbol(out _));
                }
            }
            else
            {
                Assert.False(actualParseTree.IsTerminalSymbol(out _));

                for (int i = 0; i < expectedChildCount; i++)
                {
                    Assert.Equal(length, actualParseTree.GetChildStartOrEndPosition(i));
                    length += AssertParseTree(expectedParseTree.ChildNodes[i], actualParseTree, length, actualParseTree.GetChild(i));
                }
            }

            Assert.Equal(length, actualParseTree.GetChildStartOrEndPosition(expectedChildCount));

            return length;
        }

        public static IEnumerable<(string pgn, ParseTrees.ParseTree<RootPgnSyntax> expectedParseTree, PgnErrorCode[] expectedErrors)> GetTestParseTrees()
            => ParseTrees.TestParseTrees.Select(x => (x.pgn, new ParseTrees.ParseTree<RootPgnSyntax> { x.expectedParseTree }, Array.Empty<PgnErrorCode>()))
            .Concat(ParseTrees.TestParseTreesWithErrors.Select(x => (x.pgn, new ParseTrees.ParseTree<RootPgnSyntax> { x.expectedParseTree }, x.expectedErrors)));

        public static IEnumerable<object[]> WrappedTestParseTrees() => TestUtilities.Wrap(GetTestParseTrees());

        [Theory]
        [MemberData(nameof(WrappedTestParseTrees))]
        public void ParseTreeTests(string pgn, ParseTrees.ParseTree<RootPgnSyntax> expectedParseTree, PgnErrorCode[] expectedErrors)
        {
            RootPgnSyntax rootSyntax = PgnParser.Parse(pgn);
            AssertParseTree(expectedParseTree, null, 0, rootSyntax);

            // Assert expected errors.
            Assert.Collection(
                rootSyntax.Errors,
                Enumerable.Range(0, expectedErrors.Length)
                          .Select<int, Action<PgnErrorInfo>>(i => errorInfo => Assert.Equal(expectedErrors[i], errorInfo.ErrorCode))
                          .ToArray());
        }
    }
}

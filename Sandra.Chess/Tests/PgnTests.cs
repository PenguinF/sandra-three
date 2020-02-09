#region License
/*********************************************************************************
 * PgnTests.cs
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
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;

namespace Sandra.Chess.Tests
{
    public class PgnTests
    {
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
            else if (tokenType1 == typeof(GreenPgnSymbol))
            {
                if (tokenType1 == tokenType2)
                {
                    resultTokenType = tokenType1;
                    return true;
                }
            }

            resultTokenType = default;
            return false;
        }

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Private backing field of public counterpart")]
        private static IEnumerable<(string, Type)> _PgnTestSymbols
        {
            get
            {
                yield return (" ", typeof(GreenPgnWhitespaceSyntax));
                yield return ("\r", typeof(GreenPgnWhitespaceSyntax));
                yield return ("\n", typeof(GreenPgnWhitespaceSyntax));
                yield return ("é", typeof(GreenPgnIllegalCharacterSyntax));
                yield return ("a1".ToString(), typeof(GreenPgnSymbol));
            }
        }

        private static void AssertTokens(string pgn, params Action<IGreenPgnSymbol>[] elementInspectors)
            => Assert.Collection(PgnParser.TokenizeAll(pgn), elementInspectors);

        private static Action<IGreenPgnSymbol> ExpectToken(Type expectedTokenType, int expectedLength)
        {
            return symbol =>
            {
                Assert.NotNull(symbol);
                Assert.IsType(expectedTokenType, symbol);
                Assert.Equal(expectedLength, symbol.Length);
            };
        }

        [Fact]
        public void ArgumentChecks()
        {
            Assert.Throws<ArgumentNullException>(() => new RootPgnSyntax(null));

            Assert.Throws<ArgumentNullException>(() => PgnParser.TokenizeAll(null).Any());

            Assert.Throws<ArgumentNullException>("displayCharValue", () => new GreenPgnIllegalCharacterSyntax(null));
            Assert.Throws<ArgumentException>("displayCharValue", () => new GreenPgnIllegalCharacterSyntax(string.Empty));

            Assert.Throws<ArgumentOutOfRangeException>("length", () => GreenPgnWhitespaceSyntax.Create(-1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => GreenPgnWhitespaceSyntax.Create(0));
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
            Assert.False(PgnParser.TokenizeAll(string.Empty).Any());
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

        [Fact]
        public void PgnSymbolsWithConstantLength()
        {
            Assert.Equal(1, new GreenPgnIllegalCharacterSyntax("\\0").Length);
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
            => from x1 in _PgnTestSymbols
               from x2 in _PgnTestSymbols
               select new object[] { x1.Item1, x1.Item2, x2.Item1, x2.Item2 };

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
                _PgnTestSymbols,
                Enumerable.Repeat<Action<(string, Type)>>(x0 =>
                {
                    // Put the third symbol in front, because the last symbol may eat it.
                    string pgn0 = x0.Item1;
                    Type type0 = x0.Item2;

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
                }, _PgnTestSymbols.Count()).ToArray());
        }
    }
}

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
using System.Linq;
using Xunit;

namespace Sandra.Chess.Tests
{
    public class PgnTests
    {
        [Theory]
        [InlineData(-1, 0, "start")]
        [InlineData(-1, -1, "start")]
        [InlineData(0, -1, "length")]
        public void OutOfRangeArgumentsInError(int start, int length, string parameterName)
        {
            Assert.Throws<ArgumentOutOfRangeException>(parameterName, () => new PgnErrorInfo(0, start, length));
        }

        [Fact]
        public void CreateRootPgnSyntaxWithNullTerminalsThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new RootPgnSyntax(null));
        }

        [Fact]
        public void NullPgnThrows()
        {
            Assert.Throws<ArgumentNullException>(() => PgnTokenizer.TokenizeAll(null).Any());
        }

        [Fact]
        public void EmptyPgnEmptyTokens()
        {
            Assert.False(PgnTokenizer.TokenizeAll(string.Empty).Any());
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
        public void OutOfRangeArguments()
        {
            Assert.Throws<ArgumentOutOfRangeException>("length", () => GreenPgnWhitespaceSyntax.Create(-1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => GreenPgnWhitespaceSyntax.Create(0));
            Assert.Throws<ArgumentNullException>("displayCharValue", () => new GreenPgnIllegalCharacterSyntax(null));
            Assert.Throws<ArgumentException>("displayCharValue", () => new GreenPgnIllegalCharacterSyntax(string.Empty));
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
    }
}

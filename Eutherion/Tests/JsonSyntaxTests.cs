#region License
/*********************************************************************************
 * JsonSyntaxTests.cs
 *
 * Copyright (c) 2004-2022 Henk Nicolai
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

using Eutherion.Text.Json;
using System;
using Xunit;

namespace Eutherion.Shared.Tests
{
    public class JsonSyntaxTests
    {
        [Fact]
        public void ArgumentChecks()
        {
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenJsonCommentSyntax(-1));

            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenJsonErrorStringSyntax(0));

            Assert.Throws<ArgumentNullException>("value", () => new GreenJsonStringLiteralSyntax(null, 2));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenJsonStringLiteralSyntax(string.Empty, -1));

            Assert.Throws<ArgumentNullException>("displayCharValue", () => new GreenJsonUnknownSymbolSyntax(null));
            Assert.Throws<ArgumentException>("displayCharValue", () => new GreenJsonUnknownSymbolSyntax(string.Empty));

            Assert.Throws<ArgumentNullException>("undefinedValue", () => new GreenJsonUndefinedValueSyntax(null));

            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenJsonUnterminatedMultiLineCommentSyntax(-1));

            Assert.Throws<ArgumentOutOfRangeException>("length", () => GreenJsonWhitespaceSyntax.Create(-1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => GreenJsonWhitespaceSyntax.Create(0));

            Assert.Throws<ArgumentNullException>("value", () => JsonValue.TryCreate(null));
        }

        private const int SharedWhitespaceInstanceLengthMinusTwo = GreenJsonWhitespaceSyntax.SharedInstanceLength - 2;
        private const int SharedWhitespaceInstanceLengthMinusOne = GreenJsonWhitespaceSyntax.SharedInstanceLength - 1;
        private const int SharedWhitespaceInstanceLengthPlusOne = GreenJsonWhitespaceSyntax.SharedInstanceLength + 1;
        private const int SharedWhitespaceInstanceLengthPlusTwo = GreenJsonWhitespaceSyntax.SharedInstanceLength + 2;

        [Theory]
        [InlineData(1)]
        [InlineData(SharedWhitespaceInstanceLengthMinusTwo)]
        [InlineData(SharedWhitespaceInstanceLengthMinusOne)]
        [InlineData(GreenJsonWhitespaceSyntax.SharedInstanceLength)]
        [InlineData(SharedWhitespaceInstanceLengthPlusOne)]
        [InlineData(SharedWhitespaceInstanceLengthPlusTwo)]
        public void WhitespaceHasCorrectLength(int length)
        {
            Assert.Equal(length, GreenJsonWhitespaceSyntax.Create(length).Length);
        }

        [Fact]
        public void JsonSymbolsWithLengthOne()
        {
            // As long as there's code around which depends on these symbols having length 1, this unit test is needed.
            Assert.Equal(1, GreenJsonColonSyntax.Value.Length);
            Assert.Equal(1, GreenJsonCommaSyntax.Value.Length);
            Assert.Equal(1, GreenJsonCurlyCloseSyntax.Value.Length);
            Assert.Equal(1, GreenJsonCurlyOpenSyntax.Value.Length);
            Assert.Equal(1, GreenJsonSquareBracketCloseSyntax.Value.Length);
            Assert.Equal(1, GreenJsonSquareBracketOpenSyntax.Value.Length);
            Assert.Equal(1, new GreenJsonUnknownSymbolSyntax("\\0").Length);
        }

        [Theory]
        [InlineData("\"", 10)]
        [InlineData("{}", 3)]
        // No newline conversions.
        [InlineData("\n", 1)]
        [InlineData("\r\n", 2)]
        public void UnchangedStringLiteralValueParameter(string value, int length)
        {
            // Length includes quotes for json strings.
            var jsonString = new GreenJsonStringLiteralSyntax(value, length + 2);
            Assert.Equal(value, jsonString.Value);
            Assert.Equal(length + 2, jsonString.Length);
        }

        [Theory]
        [InlineData("*")]
        [InlineData("€")]
        public void UnchangedParametersInUnexpectedSymbol(string displayCharValue)
        {
            var symbol = new GreenJsonUnknownSymbolSyntax(displayCharValue);
            Assert.Equal(displayCharValue, symbol.DisplayCharValue);
        }

        [Theory]
        [InlineData("/*  *")]
        public void UnchangedParametersInUnterminatedMultiLineComment(string commentText)
        {
            var symbol = new GreenJsonUnterminatedMultiLineCommentSyntax(commentText.Length);
            Assert.Equal(commentText.Length, symbol.Length);
        }

        [Theory]
        [InlineData("*", 0)]
        [InlineData("€", 0)]
        [InlineData("≥", 0)]

        [InlineData("▓", 200)]
        public void UnexpectedSymbolError(string displayCharValue, int position)
        {
            var error = JsonUnknownSymbolSyntax.CreateError(displayCharValue, position);
            Assert.NotNull(error);
            Assert.Equal(JsonErrorCode.UnexpectedSymbol, error.ErrorCode);
            Assert.Collection(error.Parameters, x => Assert.Equal(displayCharValue, x));
            Assert.Equal(position, error.Start);
            Assert.Equal(1, error.Length);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(2999, 3000)]
        [InlineData(0, int.MaxValue)]
        public void UnterminatedMultiLineCommentError(int start, int length)
        {
            var error = JsonUnterminatedMultiLineCommentSyntax.CreateError(start, length);
            Assert.NotNull(error);
            Assert.Equal(JsonErrorCode.UnterminatedMultiLineComment, error.ErrorCode);
            Assert.Null(error.Parameters);
            Assert.Equal(start, error.Start);
            Assert.Equal(length, error.Length);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(2999, 3000)]
        [InlineData(0, int.MaxValue)]
        public void UnterminatedStringError(int start, int length)
        {
            var error = JsonErrorStringSyntax.Unterminated(start, length);
            Assert.NotNull(error);
            Assert.Equal(JsonErrorCode.UnterminatedString, error.ErrorCode);
            Assert.Null(error.Parameters);
            Assert.Equal(start, error.Start);
            Assert.Equal(length, error.Length);
        }

        [Theory]
        [InlineData("\\u007f", 1)]
        [InlineData("\\n", 70)]
        [InlineData("\\0", 1)]
        public void IllegalControlCharacterInStringError(string displayCharValue, int position)
        {
            var error = JsonErrorStringSyntax.IllegalControlCharacter(displayCharValue, position);
            Assert.Equal(JsonErrorCode.IllegalControlCharacterInString, error.ErrorCode);
            Assert.Collection(error.Parameters, x => Assert.Equal(displayCharValue, x));
            Assert.Equal(position, error.Start);
            Assert.Equal(1, error.Length);
        }

        [Theory]
        [InlineData("\\ ", 2)]
        [InlineData("\\0", 1)]
        public void UnrecognizedEscapeSequenceError(string displayCharValue, int position)
        {
            var error = JsonErrorStringSyntax.UnrecognizedEscapeSequence(displayCharValue, position);
            Assert.Equal(JsonErrorCode.UnrecognizedEscapeSequence, error.ErrorCode);
            Assert.Collection(error.Parameters, x => Assert.Equal(displayCharValue, x));
            Assert.Equal(position, error.Start);
            Assert.Equal(2, error.Length);
        }

        [Theory]
        [InlineData("\\u", 0)]
        [InlineData("\\u00", 1)]
        [InlineData("\\uffff", 1)]
        public void UnrecognizedUnicodeEscapeSequenceError(string displayCharValue, int position)
        {
            var error = JsonErrorStringSyntax.UnrecognizedUnicodeEscapeSequence(displayCharValue, position, displayCharValue.Length);
            Assert.Equal(JsonErrorCode.UnrecognizedEscapeSequence, error.ErrorCode);
            Assert.Collection(error.Parameters, x => Assert.Equal(displayCharValue, x));
            Assert.Equal(position, error.Start);
            Assert.Equal(displayCharValue.Length, error.Length);
        }
    }
}

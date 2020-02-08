#region License
/*********************************************************************************
 * JsonSyntaxTests.cs
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

using Eutherion.Text.Json;
using System;
using System.Collections.Generic;
using Xunit;

namespace Eutherion.Shared.Tests
{
    public class JsonSyntaxTests
    {
        [Fact]
        public void ArgumentChecks()
        {
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenJsonCommentSyntax(-1));

            Assert.Throws<ArgumentNullException>(() => new GreenJsonErrorStringSyntax(2, null));
            Assert.Throws<ArgumentNullException>(() => new GreenJsonErrorStringSyntax(null, 2));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenJsonErrorStringSyntax(Array.Empty<JsonErrorInfo>(), -1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenJsonErrorStringSyntax(-1));

            Assert.Throws<ArgumentNullException>("value", () => new GreenJsonStringLiteralSyntax(null, 2));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenJsonStringLiteralSyntax(string.Empty, -1));

            Assert.Throws<ArgumentNullException>("displayCharValue", () => new GreenJsonUnknownSymbolSyntax(null));
            Assert.Throws<ArgumentException>("displayCharValue", () => new GreenJsonUnknownSymbolSyntax(string.Empty));

            Assert.Throws<ArgumentOutOfRangeException>("length", () => new GreenJsonUnterminatedMultiLineCommentSyntax(-1));

            Assert.Throws<ArgumentOutOfRangeException>("length", () => GreenJsonWhitespaceSyntax.Create(-1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => GreenJsonWhitespaceSyntax.Create(0));

            Assert.Throws<ArgumentNullException>("value", () => JsonValue.Create(null));
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
        public void UnchangedValueParameter(string value, int length)
        {
            // Length includes quotes for json strings.
            var jsonString = new GreenJsonStringLiteralSyntax(value, length + 2);
            Assert.Equal(value, jsonString.Value);
            Assert.Equal(length + 2, jsonString.Length);

            var jsonSymbol = JsonValue.Create(value);
            var jsonValue = Assert.IsType<JsonValue>(jsonSymbol);
            Assert.Equal(value, jsonValue.Value);
            Assert.Equal(value.Length, jsonValue.Length);
        }

        [Theory]
        [InlineData(JsonErrorCode.Custom, "", 0, 2)]
        [InlineData(JsonErrorCode.Unspecified, null, 0, 2)]
        // No newline conversions.
        [InlineData(JsonErrorCode.UnterminatedString, "\n", 1, 2)]
        [InlineData(JsonErrorCode.UnrecognizedEscapeSequence, "\\u00", 0, 3)]
        public void UnchangedParametersInErrorString(JsonErrorCode errorCode, string errorParameter, int start, int length)
        {
            string[] parameters = errorParameter == null ? null : new[] { errorParameter };

            var errorInfo1 = new JsonErrorInfo(errorCode, start, length, parameters);
            var errorInfo2 = new JsonErrorInfo(errorCode, start + 1, length * 2, parameters);
            var errorInfo3 = new JsonErrorInfo(errorCode, start + 2, length * 3, parameters);

            Assert.Collection(
                new GreenJsonErrorStringSyntax(length * 6, errorInfo1, errorInfo2, errorInfo3).Errors,
                error1 => Assert.Same(errorInfo1, error1),
                error2 => Assert.Same(errorInfo2, error2),
                error3 => Assert.Same(errorInfo3, error3));

            // Assert that the elements of the list are copied, i.e. that if this collection is modified
            // after being used to create a JsonErrorInfo, it does not change that JsonErrorInfo.
            var errorList = new List<JsonErrorInfo> { errorInfo1, errorInfo2, errorInfo3 };
            var errorString = new GreenJsonErrorStringSyntax(errorList, 1);
            Assert.NotSame(errorString.Errors, errorList);

            // errorString.Errors should still return the same set of JsonErrorInfos after this statement.
            errorList.Add(errorInfo1);

            Assert.Collection(
                errorString.Errors,
                error1 => Assert.Same(errorInfo1, error1),
                error2 => Assert.Same(errorInfo2, error2),
                error3 => Assert.Same(errorInfo3, error3));
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

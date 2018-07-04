#region License
/*********************************************************************************
 * TextErrorInfoTests.cs
 * 
 * Copyright (c) 2004-2018 Henk Nicolai
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
 *********************************************************************************/
#endregion

using Sandra.UI.WF.Storage;
using System;
using Xunit;

namespace Sandra.UI.WF.Tests
{
    public class TextErrorInfoTests
    {
        [Fact]
        public void NullMessageShouldThrowInError()
        {
            Assert.Throws<ArgumentNullException>(() => new TextErrorInfo(null, 0, 0));
        }

        [Theory]
        [InlineData(-1, 0, "start")]
        [InlineData(-1, -1, "start")]
        [InlineData(0, -1, "length")]
        public void OutOfRangeArgumentsInError(int start, int length, string parameterName)
        {
            Assert.Throws<ArgumentOutOfRangeException>(parameterName, () => new TextErrorInfo(string.Empty, start, length));
        }

        [Theory]
        [InlineData("", 0, 0)]
        [InlineData("Error!", 0, 1)]
        // No newline conversions.
        [InlineData("\n", 1, 0)]
        [InlineData("Error!\r\n", 0, 2)]
        public void UnchangedParametersInError(string message, int start, int length)
        {
            var errorInfo = new TextErrorInfo(message, start, length);
            Assert.Equal(message, errorInfo.Message);
            Assert.Equal(start, errorInfo.Start);
            Assert.Equal(length, errorInfo.Length);
        }

        [Theory]
        [InlineData("*", 0)]
        [InlineData("€", 0)]
        [InlineData("≥", 0)]

        [InlineData("▓", 200)]
        public void UnexpectedSymbolMessage(string displayCharValue, int position)
        {
            var error = TextErrorInfo.UnexpectedSymbol(displayCharValue, position);
            Assert.NotNull(error);
            Assert.Equal($"Unexpected symbol '{displayCharValue}'", error.Message);
            Assert.Equal(position, error.Start);
            Assert.Equal(1, error.Length);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(2999, 3000)]
        [InlineData(0, int.MaxValue)]
        public void UnterminatedMultiLineCommentMessage(int start, int length)
        {
            var error = TextErrorInfo.UnterminatedMultiLineComment(start, length);
            Assert.NotNull(error);
            Assert.Equal("Unterminated multi-line comment", error.Message);
            Assert.Equal(start, error.Start);
            Assert.Equal(length, error.Length);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(2999, 3000)]
        [InlineData(0, int.MaxValue)]
        public void UnterminatedStringMessage(int start, int length)
        {
            var error = TextErrorInfo.UnterminatedString(start, length);
            Assert.NotNull(error);
            Assert.Equal("Unterminated string", error.Message);
            Assert.Equal(start, error.Start);
            Assert.Equal(length, error.Length);
        }

        [Theory]
        [InlineData("\\u007f", 1)]
        [InlineData("\\n", 70)]
        [InlineData("\\0", 1)]
        public void IllegalControlCharacterInStringMessage(string displayCharValue, int position)
        {
            var error = TextErrorInfo.IllegalControlCharacterInString(displayCharValue, position);
            Assert.Equal($"Illegal control character '{displayCharValue}' in string", error.Message);
            Assert.Equal(position, error.Start);
            Assert.Equal(1, error.Length);
        }

        [Theory]
        [InlineData("\\ ", 2)]
        [InlineData("\\0", 1)]
        public void UnrecognizedEscapeSequenceMessage(string displayCharValue, int position)
        {
            var error = TextErrorInfo.UnrecognizedEscapeSequence(displayCharValue, position);
            Assert.Equal($"Unrecognized escape sequence ('{displayCharValue}')", error.Message);
            Assert.Equal(position, error.Start);
            Assert.Equal(2, error.Length);
        }

        [Theory]
        [InlineData("\\u", 0)]
        [InlineData("\\u00", 1)]
        [InlineData("\\uffff", 1)]
        public void UnrecognizedUnicodeEscapeSequenceMessage(string displayCharValue, int position)
        {
            var error = TextErrorInfo.UnrecognizedUnicodeEscapeSequence(displayCharValue, position, displayCharValue.Length);
            Assert.Equal($"Unrecognized escape sequence ('{displayCharValue}')", error.Message);
            Assert.Equal(position, error.Start);
            Assert.Equal(displayCharValue.Length, error.Length);
        }
    }
}

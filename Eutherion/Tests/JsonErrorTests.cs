#region License
/*********************************************************************************
 * JsonErrorTests.cs
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
using System.Linq;
using Xunit;

namespace Eutherion.Shared.Tests
{
    public class JsonErrorTests
    {
        internal static void AssertErrorInfoParameters(JsonErrorInfo actualErrorInfo, params JsonErrorInfoParameter[] expectedParameters)
        {
            if (expectedParameters == null || !expectedParameters.Any())
            {
                Assert.Empty(actualErrorInfo.Parameters);
            }
            else
            {
                Assert.Collection(actualErrorInfo.Parameters, expectedParameters.Select(expected => new Action<JsonErrorInfoParameter>(actual =>
                {
                    Assert.IsType(expected.GetType(), actual);
                    Assert.Equal(expected.UntypedValue, actual.UntypedValue);
                })).ToArrayEx());
            }
        }

        [Theory]
        [InlineData(-1, 0, "start")]
        [InlineData(-1, -1, "start")]
        [InlineData(0, -1, "length")]
        public void OutOfRangeArgumentsInError(int start, int length, string parameterName)
        {
            Assert.Throws<ArgumentOutOfRangeException>(parameterName, () => new JsonErrorInfo(JsonErrorCode.Unspecified, start, length));
        }

        [Theory]
        [InlineData(JsonErrorCode.Unspecified, JsonErrorLevel.Message, 0, 0, null)]
        [InlineData(JsonErrorCode.Custom, JsonErrorLevel.Warning, 0, 1, new string[0])]
        [InlineData(JsonErrorCode.ExpectedEof, JsonErrorLevel.Error, 1, 0, new[] { "\n", "" })]
        [InlineData(JsonErrorCode.Custom + 999, (JsonErrorLevel)(-1), 0, 2, new[] { "Aa" })]
        public void UnchangedParametersInError(JsonErrorCode errorCode, JsonErrorLevel errorLevel, int start, int length, string[] parameters)
        {
            JsonErrorInfoParameter[] errorInfoParameters = parameters?.Select(x => new JsonErrorInfoParameter<string>(x))?.ToArrayEx();

            var errorInfo = new JsonErrorInfo(errorCode, errorLevel, start, length, errorInfoParameters);
            Assert.Equal(errorCode, errorInfo.ErrorCode);
            Assert.Equal(errorLevel, errorInfo.ErrorLevel);
            Assert.Equal(start, errorInfo.Start);
            Assert.Equal(length, errorInfo.Length);

            AssertErrorInfoParameters(errorInfo, errorInfoParameters);
        }

        [Theory]
        [InlineData('*', 0)]
        [InlineData('€', 0)]
        [InlineData('≥', 0)]

        [InlineData('▓', 200)]
        public void UnexpectedSymbolError(char unexpectedCharacter, int position)
        {
            var error = JsonParseErrors.UnexpectedSymbol(unexpectedCharacter, position);
            Assert.NotNull(error);
            Assert.Equal(JsonErrorCode.UnexpectedSymbol, error.ErrorCode);
            AssertErrorInfoParameters(error, new JsonErrorInfoParameter<char>(unexpectedCharacter));
            Assert.Equal(position, error.Start);
            Assert.Equal(1, error.Length);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(2999, 3000)]
        [InlineData(0, int.MaxValue)]
        public void UnterminatedMultiLineCommentError(int start, int length)
        {
            var error = JsonParseErrors.UnterminatedMultiLineComment(start, length);
            Assert.NotNull(error);
            Assert.Equal(JsonErrorCode.UnterminatedMultiLineComment, error.ErrorCode);
            AssertErrorInfoParameters(error);
            Assert.Equal(start, error.Start);
            Assert.Equal(length, error.Length);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(2999, 3000)]
        [InlineData(0, int.MaxValue)]
        public void UnterminatedStringError(int start, int length)
        {
            var error = JsonParseErrors.UnterminatedString(start, length);
            Assert.NotNull(error);
            Assert.Equal(JsonErrorCode.UnterminatedString, error.ErrorCode);
            AssertErrorInfoParameters(error);
            Assert.Equal(start, error.Start);
            Assert.Equal(length, error.Length);
        }

        [Theory]
        [InlineData('\u007f', 1)]
        [InlineData('\n', 70)]
        [InlineData('\0', 1)]
        public void IllegalControlCharacterInStringError(char illegalControlCharacter, int position)
        {
            var error = JsonParseErrors.IllegalControlCharacterInString(illegalControlCharacter, position);
            Assert.Equal(JsonErrorCode.IllegalControlCharacterInString, error.ErrorCode);
            AssertErrorInfoParameters(error, new JsonErrorInfoParameter<char>(illegalControlCharacter));
            Assert.Equal(position, error.Start);
            Assert.Equal(1, error.Length);
        }

        [Theory]
        [InlineData("\\ ", 2)]
        [InlineData("\\0", 1)]
        [InlineData("\\u", 0)]
        [InlineData("\\u00", 1)]
        [InlineData("\\uffff", 1)]
        public void UnrecognizedEscapeSequenceError(string escapeSequence, int position)
        {
            var error = JsonParseErrors.UnrecognizedEscapeSequence(escapeSequence, position, escapeSequence.Length);
            Assert.Equal(JsonErrorCode.UnrecognizedEscapeSequence, error.ErrorCode);
            AssertErrorInfoParameters(error, new JsonErrorInfoParameter<string>(escapeSequence));
            Assert.Equal(position, error.Start);
            Assert.Equal(escapeSequence.Length, error.Length);
        }
    }
}

#region License
/*********************************************************************************
 * JsonErrorInfoTests.cs
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
using System.Linq;
using Xunit;

namespace Eutherion.Shared.Tests
{
    public class JsonErrorInfoTests
    {
        internal static void AssertErrorInfoParameters(JsonErrorInfo actualErrorInfo, params string[] expectedParameters)
        {
            if (expectedParameters == null || !expectedParameters.Any())
            {
                Assert.True(actualErrorInfo.Parameters == null || !actualErrorInfo.Parameters.Any());
            }
            else
            {
                Assert.Collection(actualErrorInfo.Parameters, expectedParameters.Select(expected => new Action<string>(actual =>
                {
                    Assert.Equal(expected, actual);
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
            var errorInfo = new JsonErrorInfo(errorCode, errorLevel, start, length, parameters);
            Assert.Equal(errorCode, errorInfo.ErrorCode);
            Assert.Equal(errorLevel, errorInfo.ErrorLevel);
            Assert.Equal(start, errorInfo.Start);
            Assert.Equal(length, errorInfo.Length);

            // Select Assert.Equal() overload for collections so elements get compared rather than the array by reference.
            Assert.Equal<string>(parameters, errorInfo.Parameters);
        }
    }
}

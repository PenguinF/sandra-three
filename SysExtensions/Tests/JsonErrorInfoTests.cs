#region License
/*********************************************************************************
 * JsonErrorInfoTests.cs
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

using SysExtensions.Text.Json;
using System;
using Xunit;

namespace SysExtensions.Tests
{
    public class JsonErrorInfoTests
    {
        [Theory]
        [InlineData(-1, 0, "start")]
        [InlineData(-1, -1, "start")]
        [InlineData(0, -1, "length")]
        public void OutOfRangeArgumentsInError(int start, int length, string parameterName)
        {
            Assert.Throws<ArgumentOutOfRangeException>(parameterName, () => new JsonErrorInfo(JsonErrorCode.Unspecified, start, length));
        }

        [Theory]
        [InlineData(JsonErrorCode.Unspecified, 0, 0, null)]
        [InlineData(JsonErrorCode.Custom, 0, 1, new string[0])]
        [InlineData(JsonErrorCode.ExpectedEof, 1, 0, new[] { "\n", "" })]
        [InlineData(JsonErrorCode.Custom + 999, 0, 2, new[] { "Aa" })]
        public void UnchangedParametersInError(JsonErrorCode errorCode, int start, int length, string[] parameters)
        {
            var errorInfo = new JsonErrorInfo(errorCode, start, length, parameters);
            Assert.Equal(errorCode, errorInfo.ErrorCode);
            Assert.Equal(start, errorInfo.Start);
            Assert.Equal(length, errorInfo.Length);

            // Select Assert.Equal() overload for collections so elements get compared rather than the array by reference.
            Assert.Equal<string>(parameters, errorInfo.Parameters);
        }
    }
}

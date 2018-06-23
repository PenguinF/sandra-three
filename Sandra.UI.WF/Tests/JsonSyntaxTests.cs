﻿#region License
/*********************************************************************************
 * JsonSyntaxTests.cs
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
    public class JsonSyntaxTests
    {
        [Fact]
        public void NullJsonShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new JsonTerminalSymbol(null, 0, 0));
        }

        [Theory]
        [InlineData("", -1, 0, "start")]
        [InlineData("", -1, -1, "start")]
        [InlineData("", 0, -1, "length")]
        [InlineData("", 1, 0, "start")]
        [InlineData("", 0, 1, "length")]
        [InlineData(" ", 0, 2, "length")]
        [InlineData(" ", 1, 1, "length")]
        [InlineData(" ", 2, 0, "start")]
        public void OutOfRangeArguments(string json, int start, int length, string parameterName)
        {
            Assert.Throws<ArgumentOutOfRangeException>(parameterName, () => new JsonTerminalSymbol(json, start, length));
        }
    }
}

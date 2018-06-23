#region License
/*********************************************************************************
 * JsonTokenizerTests.cs
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
using System.Linq;
using Xunit;

namespace Sandra.UI.WF.Tests
{
    public class JsonTokenizerTests
    {
        [Fact]
        public void NullJsonThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new JsonTokenizer(null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("{}")]
        // No newline conversions.
        [InlineData("\n")]
        [InlineData("\r\n")]
        public void JsonIsUnchanged(string json)
        {
            Assert.True(json == new JsonTokenizer(json).Json);
        }

        [Theory]
        [InlineData("*")]
        [InlineData("\0")]
        [InlineData("€")]
        public void Unknown(string json)
        {
            var tokens = new JsonTokenizer(json).TokenizeAll().ToArray();
            Assert.Collection(tokens, symbol =>
            {
                Assert.NotNull(symbol);
                Assert.IsType<JsonUnknownSymbol>(symbol);
                JsonUnknownSymbol unknownSymbol = (JsonUnknownSymbol)symbol;
                Assert.Equal(json, unknownSymbol.Json);
                Assert.Equal(0, unknownSymbol.Start);
                Assert.Equal(json.Length, unknownSymbol.Length);
            });
        }
    }
}

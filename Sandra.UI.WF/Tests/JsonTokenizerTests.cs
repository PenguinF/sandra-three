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
using System.Collections.Generic;
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
            Assert.Equal(json, new JsonTokenizer(json).Json);
        }

        [Theory]
        [InlineData("//", "//")]
        [InlineData("//\n", "//")]
        [InlineData("//\r\n", "//")]
        [InlineData("//\t\tComment \r\n", "//\t\tComment ")]
        [InlineData("/**/", null)]
        [InlineData("/*  */", null)]
        [InlineData("/*\r\n*/", null)]
        [InlineData("/*/**/", null)]
        [InlineData("/*//\r\n*/\r\n", "/*//\r\n*/")]
        [InlineData("///**/\r\n", "///**/")]
        public void Comment(string json, string alternativeCommentText)
        {
            string expectedCommentText = alternativeCommentText ?? json;
            Assert.Collection(new JsonTokenizer(json).TokenizeAll(), symbol =>
            {
                Assert.NotNull(symbol);
                Assert.IsType<JsonComment>(symbol);
                var commentSymbol = (JsonComment)symbol;
                Assert.Equal(0, commentSymbol.Start);
                Assert.Equal(expectedCommentText.Length, commentSymbol.Length);
            });
        }

        [Theory]
        [InlineData("")]
        [InlineData("\0")]
        [InlineData("                              ")]
        [InlineData("\n")]
        [InlineData("\r")]
        [InlineData("\t\v\u000c\u0085\u00a0")]
        [InlineData("\r\n")]
        [InlineData("\r\n\r\n")]
        [InlineData("\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200a\u2028\u2029\u202f\u205f\u3000")]
        public void WhiteSpace(string ws)
        {
            Assert.False(new JsonTokenizer(ws).TokenizeAll().Any());
        }

        [Theory]
        [InlineData(typeof(JsonCurlyOpen), '{')]
        [InlineData(typeof(JsonCurlyClose), '}')]
        [InlineData(typeof(JsonSquareBracketOpen), '[')]
        [InlineData(typeof(JsonSquareBracketClose), ']')]
        [InlineData(typeof(JsonColon), ':')]
        [InlineData(typeof(JsonComma), ',')]
        [InlineData(typeof(JsonUnknownSymbol), '*')]
        [InlineData(typeof(JsonUnknownSymbol), '€')]
        [InlineData(typeof(JsonUnknownSymbol), '≥')]
        [InlineData(typeof(JsonUnknownSymbol), '¿')]
        [InlineData(typeof(JsonUnknownSymbol), '°')]
        [InlineData(typeof(JsonUnknownSymbol), '╣')]
        [InlineData(typeof(JsonUnknownSymbol), '∙')]

        [InlineData(typeof(JsonUnknownSymbol), '▓')]
        public void SpecialCharacter(Type tokenType, char specialCharacter)
        {
            string json = Convert.ToString(specialCharacter);
            var tokens = new JsonTokenizer(json).TokenizeAll().ToArray();
            Assert.Collection(tokens, symbol =>
            {
                Assert.NotNull(symbol);
                Assert.IsType(tokenType, symbol);
                Assert.Equal(json, symbol.Json);
                Assert.Equal(0, symbol.Start);
                Assert.Equal(json.Length, symbol.Length);
            });
        }

        [Theory]
        [InlineData("true")]
        [InlineData("false")]
        [InlineData("null")]
        [InlineData("0")]
        [InlineData("-1")]
        [InlineData("10.8")]
        [InlineData("-9.00001")]
        [InlineData("+00001")]
        [InlineData("-1e+10")]
        [InlineData("1.9E-5")]
        [InlineData("0b01011001")]
        [InlineData("0xffff")]
        [InlineData("_")]
        [InlineData("x80")]
        [InlineData("189")]
        [InlineData("x²")]
        [InlineData("x₁")]
        [InlineData("Grüßen")]
        // Shamelessly plugged from that online translation tool. These are all 'thing's.
        [InlineData("شيء")]
        [InlineData("вещь")]
        [InlineData("事情")]
        [InlineData("もの")]
        [InlineData("πράγμα")]
        [InlineData("맡은일")]
        [InlineData("चीज़")]
        [InlineData("Điều")]
        [InlineData("דָבָר")]
        [InlineData("สิ่ง")]
        [InlineData("విషయం")]
        [InlineData("விஷயம்")]
        [InlineData("දෙයක්")]
        [InlineData("ڳالھ")]
        public void ValueSymbol(string json)
        {
            var tokens = new JsonTokenizer(json).TokenizeAll().ToArray();
            Assert.Collection(tokens, symbol =>
            {
                Assert.NotNull(symbol);
                Assert.IsType<JsonValue>(symbol);
                Assert.Equal(json, symbol.Json);
                Assert.Equal(0, symbol.Start);
                Assert.Equal(json.Length, symbol.Length);
            });
        }

        [Theory]
        [InlineData("\"\"", "")]
        [InlineData("\" \"", " ")]
        public void StringValue(string json, string expectedValue)
        {
            var tokens = new JsonTokenizer(json).TokenizeAll().ToArray();
            Assert.Collection(tokens, symbol =>
            {
                Assert.NotNull(symbol);
                Assert.IsType<JsonString>(symbol);
                var stringSymbol = (JsonString)symbol;
                Assert.Equal(json, stringSymbol.Json);
                Assert.Equal(0, stringSymbol.Start);
                Assert.Equal(json.Length, stringSymbol.Length);
                Assert.Equal(expectedValue, stringSymbol.Value);
            });
        }

        public static IEnumerable<object[]> TwoSymbolsOfEachType()
        {
            var symbolTypes = new Dictionary<string, Type>
            {
                { "//\n", typeof(JsonComment) },
                { "/**/", typeof(JsonComment) },
                { "{", typeof(JsonCurlyOpen) },
                { "}", typeof(JsonCurlyClose) },
                { "[", typeof(JsonSquareBracketOpen) },
                { "]", typeof(JsonSquareBracketClose) },
                { ":", typeof(JsonColon) },
                { ",", typeof(JsonComma) },
                { "*", typeof(JsonUnknownSymbol) },
                { "_", typeof(JsonValue) },
                { "true", typeof(JsonValue) },
                { "\"\"", typeof(JsonString) },
            };

            var keys = symbolTypes.Keys;
            foreach (var key1 in keys)
            {
                foreach (var key2 in keys)
                {
                    Type type1 = symbolTypes[key1];
                    Type type2 = symbolTypes[key2];
                    yield return new object[] { key1, type1, key2, type2 };
                }
            }
        }

        [Theory]
        [MemberData(nameof(TwoSymbolsOfEachType))]
        public void Transition(string json1, Type type1, string json2, Type type2)
        {
            // Test all eight combinations of whitespace before/in between/after both strings.
            for (int i = 0; i < 8; i++)
            {
                // Two JsonValues are glued together if there's no whitespace in between, so skip those.
                if ((i & 2) == 0 && type1 == typeof(JsonValue) && type2 == typeof(JsonValue))
                {
                    continue;
                }

                string ws1 = (i & 1) != 0 ? " " : "";
                string ws2 = (i & 2) != 0 ? " " : "";
                string ws3 = (i & 4) != 0 ? " " : "";

                int expectedSymbol1Start = (i & 1) != 0 ? 1 : 0;
                int expectedSymbol2Start = expectedSymbol1Start + json1.Length + ((i & 2) != 0 ? 1 : 0);

                // Exception for end-of-line comment "//\n".
                int expectedSymbol1Length = json1.Length;
                if (json1[json1.Length - 1] == '\n') expectedSymbol1Length--;
                int expectedSymbol2Length = json2.Length;
                if (json2[json2.Length - 1] == '\n') expectedSymbol2Length--;

                var json = $"{ws1}{json1}{ws2}{json2}{ws3}";
                var tokens = new JsonTokenizer(json).TokenizeAll().ToArray();

                Assert.Collection(tokens, symbol1 =>
                {
                    Assert.NotNull(symbol1);
                    Assert.IsType(type1, symbol1);
                    Assert.Equal(expectedSymbol1Start, symbol1.Start);
                    Assert.Equal(expectedSymbol1Length, symbol1.Length);
                }, symbol2 =>
                {
                    Assert.NotNull(symbol2);
                    Assert.IsType(type2, symbol2);
                    Assert.Equal(expectedSymbol2Start, symbol2.Start);
                    Assert.Equal(expectedSymbol2Length, symbol2.Length);
                });
            }
        }
    }
}

#region License
/*********************************************************************************
 * JsonTokenizerTests.cs
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
using System.Linq;
using Xunit;

namespace Eutherion.Shared.Tests
{
    public class JsonTokenizerTests
    {
        [Fact]
        public void NullJsonThrows()
        {
            Assert.Throws<ArgumentNullException>(() => JsonTokenizer.TokenizeAll(null).Any());
        }

        [Theory]
        [InlineData("//", null)]
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

            void firstTokenAssert(JsonSymbol symbol)
            {
                Assert.NotNull(symbol);
                Assert.IsType<JsonComment>(symbol);
                Assert.Equal(expectedCommentText.Length, symbol.Length);
                Assert.Equal(expectedCommentText, json.Substring(0, symbol.Length));
            }

            if (alternativeCommentText == null)
            {
                Assert.Collection(
                    JsonTokenizer.TokenizeAll(json),
                    firstTokenAssert);
            }
            else
            {
                // Expect some whitespace at the end.
                Assert.Collection(
                    JsonTokenizer.TokenizeAll(json),
                    firstTokenAssert,
                    symbol => Assert.IsType<JsonWhitespace>(symbol));
            }
        }

        [Fact]
        public void EmptyStringNoTokens()
        {
            Assert.False(JsonTokenizer.TokenizeAll("").Any());
        }

        [Theory]
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
            // Exactly one whitespace token.
            Assert.Collection(JsonTokenizer.TokenizeAll(ws), x => Assert.IsType<JsonWhitespace>(x));
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
            Assert.Collection(JsonTokenizer.TokenizeAll(json), symbol =>
            {
                Assert.NotNull(symbol);
                Assert.IsType(tokenType, symbol);
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
            Assert.Collection(JsonTokenizer.TokenizeAll(json), symbol =>
            {
                Assert.NotNull(symbol);
                var valueSymbol = Assert.IsType<JsonValue>(symbol);
                Assert.Equal(json.Length, symbol.Length);
                Assert.Equal(json, json.Substring(0, symbol.Length));
                Assert.Equal(json, valueSymbol.Value);
            });
        }

        [Theory]
        [InlineData("\"\"", "")]
        [InlineData("\" \"", " ")]
        [InlineData("\"xxx\"", "xxx")]

        // Escape sequences.
        [InlineData("\"\\\"\"", "\"")]
        [InlineData("\"\\/\"", "/")]
        [InlineData("\"\\b\"", "\b")]
        [InlineData("\"\\f\"", "\f")]
        [InlineData("\"\\n\"", "\n")]
        [InlineData("\"\\r\"", "\r")]
        [InlineData("\"\\t\"", "\t")]
        [InlineData("\"\\v\"", "\v")]  // Support \v, contrary to JSON specification.

        // \u escape sequences.
        [InlineData("\"\\u000d\\u000a\"", "\r\n")]
        [InlineData("\"\\u0020\\u004E\"", " N")]
        [InlineData("\"\\u00200\"", " 0")] // last 0 is not part of the \u escape sequence
        public void StringValue(string json, string expectedValue)
        {
            Assert.Collection(JsonTokenizer.TokenizeAll(json), symbol =>
            {
                Assert.NotNull(symbol);
                var stringSymbol = Assert.IsType<JsonString>(symbol);
                Assert.Equal(json.Length, symbol.Length);
                Assert.Equal(expectedValue, stringSymbol.Value);
            });
        }

        public static IEnumerable<object[]> TwoSymbolsOfEachType()
        {
            var symbolTypes = new Dictionary<string, Type>
            {
                { "//\n", typeof(JsonComment) },
                { "/**/", typeof(JsonComment) },
                { "/***/", typeof(JsonComment) },
                { "/*/*/", typeof(JsonComment) },
                { "/*", typeof(JsonUnterminatedMultiLineComment) },
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
                { "\" \"", typeof(JsonString) },  // Have to check if the space isn't interpreted as whitespace.
                { "\"", typeof(JsonErrorString) },
                { "\"\n\\ \n\"", typeof(JsonErrorString) },
                { "\"\\u0\"", typeof(JsonErrorString) },
            };

            var keys = symbolTypes.Keys;
            foreach (var key1 in keys)
            {
                // Unterminated strings/comments mess up the tokenization, skip those if they're the first key.
                if (key1 != "\"" && key1 != "/*")
                {
                    foreach (var key2 in keys)
                    {
                        Type type1 = symbolTypes[key1];
                        Type type2 = symbolTypes[key2];
                        yield return new object[] { key1, type1, key2, type2 };
                    }
                }
            }
        }

        private static int ExpectedSymbolLength(string singleJsonSymbol)
        {
            // Exception for end-of-line comment "//\n".
            int expectedLength = singleJsonSymbol.Length;
            if (singleJsonSymbol[singleJsonSymbol.Length - 1] == '\n') expectedLength--;
            return expectedLength;
        }

        [Theory]
        [MemberData(nameof(TwoSymbolsOfEachType))]
        public void Transition(string json1, Type type1, string json2, Type type2)
        {
            // Test all eight combinations of whitespace before/in between/after both strings.
            for (int i = 0; i < 8; i++)
            {
                string ws1 = (i & 1) != 0 ? " " : "";
                string ws2 = (i & 2) != 0 ? " " : "";
                string ws3 = (i & 4) != 0 ? " " : "";

                int expectedSymbol1Start = (i & 1) != 0 ? 1 : 0;
                int expectedSymbol2Start = expectedSymbol1Start + json1.Length + ((i & 2) != 0 ? 1 : 0);

                int expectedSymbol1Length = ExpectedSymbolLength(json1);
                int expectedSymbol2Length = ExpectedSymbolLength(json2);

                var json = $"{ws1}{json1}{ws2}{json2}{ws3}";

                // Two JsonValues are glued together if there's no whitespace in between,
                // so assert that this is indeed what happens.
                if ((i & 2) == 0 && type1 == typeof(JsonValue) && type2 == typeof(JsonValue))
                {
                    Assert.Collection(JsonTokenizer.TokenizeAll(json).Where(x => !(x is JsonWhitespace)), symbol1 =>
                    {
                        Assert.NotNull(symbol1);
                        Assert.IsType(type1, symbol1);
                        Assert.Equal(expectedSymbol2Start + expectedSymbol2Length - expectedSymbol1Start, symbol1.Length);
                    });
                }
                else
                {
                    if ((i & 4) != 0 && (json2 == "\"" || json2 == "/*"))
                    {
                        // If symbol2 is an unterminated string/comment, its length should include the whitespace after it.
                        expectedSymbol2Length++;
                    }

                    Assert.Collection(JsonTokenizer.TokenizeAll(json).Where(x => !(x is JsonWhitespace)), symbol1 =>
                    {
                        Assert.NotNull(symbol1);
                        Assert.IsType(type1, symbol1);
                        Assert.Equal(expectedSymbol1Length, symbol1.Length);
                    }, symbol2 =>
                    {
                        Assert.NotNull(symbol2);
                        Assert.IsType(type2, symbol2);
                        Assert.Equal(expectedSymbol2Length, symbol2.Length);
                    });
                }
            }
        }

        public static IEnumerable<object[]> GetErrorStrings()
        {
            yield return new object[] { "*", new[] { JsonUnknownSymbol.CreateError("*", 0) } };
            yield return new object[] { " *", new[] { JsonUnknownSymbol.CreateError("*", 1) } };
            yield return new object[] { "  °  ", new[] { JsonUnknownSymbol.CreateError("°", 2) } };

            // Unterminated comments.
            yield return new object[] { "/*", new[] { JsonUnterminatedMultiLineComment.CreateError(0, 2) } };
            yield return new object[] { "/*\n\n", new[] { JsonUnterminatedMultiLineComment.CreateError(0, 4) } };
            yield return new object[] { "  /*\n\n*", new[] { JsonUnterminatedMultiLineComment.CreateError(2, 5) } };
            yield return new object[] { "  /*\n\n* /", new[] { JsonUnterminatedMultiLineComment.CreateError(2, 7) } };

            // Invalid strings.
            yield return new object[] { "\"", new[] { JsonErrorString.Unterminated(0, 1) } };
            yield return new object[] { "\"\\", new[] { JsonErrorString.Unterminated(0, 2) } };

            // Unterminated because the closing " is escaped.
            yield return new object[] { "\"\\\"", new[] { JsonErrorString.Unterminated(0, 3) } };
            yield return new object[] { "\"\\ \"", new[] { JsonErrorString.UnrecognizedEscapeSequence("\\ ", 1) } };
            yield return new object[] { "\"\\e\"", new[] { JsonErrorString.UnrecognizedEscapeSequence("\\e", 1) } };

            // Unicode escape sequences.
            yield return new object[] { "\"\\u\"", new[] { JsonErrorString.UnrecognizedEscapeSequence("\\u", 1) } };
            yield return new object[] { "\"\\ux\"", new[] { JsonErrorString.UnrecognizedEscapeSequence("\\u", 1) } };
            yield return new object[] { "\"\\uxxxx\"", new[] { JsonErrorString.UnrecognizedEscapeSequence("\\u", 1) } };
            yield return new object[] { "\"\\u0\"", new[] { JsonErrorString.UnrecognizedUnicodeEscapeSequence("\\u0", 1, 3) } };
            yield return new object[] { "\"\\u00\"", new[] { JsonErrorString.UnrecognizedUnicodeEscapeSequence("\\u00", 1, 4) } };
            yield return new object[] { "\"\\u000\"", new[] { JsonErrorString.UnrecognizedUnicodeEscapeSequence("\\u000", 1, 5) } };
            yield return new object[] { "\"\\u000g\"", new[] { JsonErrorString.UnrecognizedUnicodeEscapeSequence("\\u000", 1, 5) } };

            // Prevent int.TryParse hacks.
            yield return new object[] { "\"\\u-1000\"", new[] { JsonErrorString.UnrecognizedUnicodeEscapeSequence("\\u", 1, 2) } };

            // Disallow control characters.
            yield return new object[] { "\"\n\"", new[] { JsonErrorString.IllegalControlCharacter("\\n", 1) } };
            yield return new object[] { "\"\t\"", new[] { JsonErrorString.IllegalControlCharacter("\\t", 1) } };
            yield return new object[] { "\"\0\"", new[] { JsonErrorString.IllegalControlCharacter("\\0", 1) } };
            yield return new object[] { "\"\u0001\"", new[] { JsonErrorString.IllegalControlCharacter("\\u0001", 1) } };
            yield return new object[] { "\"\u007f\"", new[] { JsonErrorString.IllegalControlCharacter("\\u007f", 1) } };

            // Multiple errors.
            yield return new object[] { " ∙\"∙\"\"", new JsonErrorInfo[] {
                JsonUnknownSymbol.CreateError("∙", 1),
                JsonErrorString.Unterminated(5, 1) } };
            yield return new object[] { "\"\r\n\"", new[] {
                JsonErrorString.IllegalControlCharacter("\\r", 1),
                JsonErrorString.IllegalControlCharacter("\\n", 2) } };
            yield return new object[] { "\"\\ ", new[] {
                JsonErrorString.UnrecognizedEscapeSequence("\\ ", 1),
                JsonErrorString.Unterminated(0, 3) } };
            yield return new object[] { "\"\r\n∙\"∙", new[] {
                JsonErrorString.IllegalControlCharacter("\\r", 1),
                JsonErrorString.IllegalControlCharacter("\\n", 2),
                JsonUnknownSymbol.CreateError("∙", 5) } };
            yield return new object[] { "\"\t\n", new[] {
                JsonErrorString.IllegalControlCharacter("\\t", 1),
                JsonErrorString.IllegalControlCharacter("\\n", 2),
                JsonErrorString.Unterminated(0, 3) } };
            yield return new object[] { "\" \\ \n\"", new[] {
                JsonErrorString.UnrecognizedEscapeSequence("\\ ", 2),
                JsonErrorString.IllegalControlCharacter("\\n", 4) } };
            yield return new object[] { "\"\n\\ \n\"", new[] {
                JsonErrorString.IllegalControlCharacter("\\n", 1),
                JsonErrorString.UnrecognizedEscapeSequence("\\ ", 2),
                JsonErrorString.IllegalControlCharacter("\\n", 4) } };
            yield return new object[] { "\"\\u", new[] {
                JsonErrorString.UnrecognizedEscapeSequence("\\u", 1),
                JsonErrorString.Unterminated(0, 3) } };
            yield return new object[] { "\"\\uA", new[] {
                JsonErrorString.UnrecognizedUnicodeEscapeSequence("\\uA", 1, 3),
                JsonErrorString.Unterminated(0, 4) } };
            yield return new object[] { "\"\\u\n", new[] {
                JsonErrorString.UnrecognizedEscapeSequence("\\u", 1),
                JsonErrorString.IllegalControlCharacter("\\n", 3),
                JsonErrorString.Unterminated(0, 4) } };
            yield return new object[] { "\"\\ufff\n", new[] {
                JsonErrorString.UnrecognizedUnicodeEscapeSequence("\\ufff", 1, 5),
                JsonErrorString.IllegalControlCharacter("\\n", 6),
                JsonErrorString.Unterminated(0, 7) } };
            yield return new object[] { "\"\n\\ ∙\"∙", new[] {
                JsonErrorString.IllegalControlCharacter("\\n", 1),
                JsonErrorString.UnrecognizedEscapeSequence("\\ ", 2),
                JsonUnknownSymbol.CreateError("∙", 6) } };
            yield return new object[] { "∙\"\n\\ ∙\"", new[] {
                JsonUnknownSymbol.CreateError("∙", 0),
                JsonErrorString.IllegalControlCharacter("\\n", 2),
                JsonErrorString.UnrecognizedEscapeSequence("\\ ", 3) } };

            // Know what's unterminated.
            yield return new object[] { "\"/*", new[] { JsonErrorString.Unterminated(0, 3) } };
            yield return new object[] { "/*\"", new[] { JsonUnterminatedMultiLineComment.CreateError(0, 3) } };
            yield return new object[] { "///*\n\"", new[] { JsonErrorString.Unterminated(5, 1) } };
            yield return new object[] { "///*\"\n/*", new[] { JsonUnterminatedMultiLineComment.CreateError(6, 2) } };
        }

        [Theory]
        [MemberData(nameof(GetErrorStrings))]
        public void Errors(string json, JsonErrorInfo[] expectedErrors)
        {
            var generatedErrors = new List<JsonErrorInfo>();
            int length = 0;

            foreach (var token in JsonTokenizer.TokenizeAll(json))
            {
                if (token.HasErrors) generatedErrors.AddRange(token.GetErrors(length));
                length += token.Length;
            }

            Assert.Collection(generatedErrors, expectedErrors.Select(expectedError => new Action<JsonErrorInfo>(generatedError =>
            {
                Assert.NotNull(generatedError);
                Assert.Equal(expectedError.ErrorCode, generatedError.ErrorCode);
                Assert.Equal(expectedError.Start, generatedError.Start);
                Assert.Equal(expectedError.Length, generatedError.Length);

                // Select Assert.Equal() overload for collections so elements get compared rather than the array by reference.
                Assert.Equal<string>(expectedError.Parameters, generatedError.Parameters);
            })).ToArray());
        }
    }
}

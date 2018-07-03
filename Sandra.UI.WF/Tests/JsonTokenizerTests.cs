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
            Assert.Collection(new JsonTokenizer(json).TokenizeAll(), symbol =>
            {
                Assert.NotNull(symbol);
                var commentSymbol = Assert.IsType<JsonComment>(symbol);
                Assert.Equal(0, commentSymbol.Start);
                Assert.Equal(expectedCommentText.Length, commentSymbol.Length);
                Assert.Equal(expectedCommentText, commentSymbol.Text);
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
            Assert.Collection(new JsonTokenizer(json).TokenizeAll(), symbol =>
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
            Assert.Collection(new JsonTokenizer(json).TokenizeAll(), symbol =>
            {
                Assert.NotNull(symbol);
                var valueSymbol = Assert.IsType<JsonValue>(symbol);
                Assert.Equal(json, symbol.Json);
                Assert.Equal(0, symbol.Start);
                Assert.Equal(json.Length, symbol.Length);
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
            Assert.Collection(new JsonTokenizer(json).TokenizeAll(), symbol =>
            {
                Assert.NotNull(symbol);
                var stringSymbol = Assert.IsType<JsonString>(symbol);
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

                // Exception for end-of-line comment "//\n".
                int expectedSymbol1Length = json1.Length;
                if (json1[json1.Length - 1] == '\n') expectedSymbol1Length--;
                int expectedSymbol2Length = json2.Length;
                if (json2[json2.Length - 1] == '\n') expectedSymbol2Length--;

                var json = $"{ws1}{json1}{ws2}{json2}{ws3}";

                // Two JsonValues are glued together if there's no whitespace in between,
                // so assert that this is indeed what happens.
                if ((i & 2) == 0 && type1 == typeof(JsonValue) && type2 == typeof(JsonValue))
                {
                    Assert.Collection(new JsonTokenizer(json).TokenizeAll(), symbol1 =>
                    {
                        Assert.NotNull(symbol1);
                        Assert.IsType(type1, symbol1);
                        Assert.Equal(expectedSymbol1Start, symbol1.Start);
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

                    Assert.Collection(new JsonTokenizer(json).TokenizeAll(), symbol1 =>
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

        public static IEnumerable<object[]> GetErrorStrings()
        {
            yield return new object[] { "*", new[] { TextErrorInfo.UnexpectedSymbol("*", 0) } };
            yield return new object[] { " *", new[] { TextErrorInfo.UnexpectedSymbol("*", 1) } };
            yield return new object[] { "  °  ", new[] { TextErrorInfo.UnexpectedSymbol("°", 2) } };

            // Unterminated comments.
            yield return new object[] { "/*", new[] { TextErrorInfo.UnterminatedMultiLineComment(2) } };
            yield return new object[] { "/*\n\n", new[] { TextErrorInfo.UnterminatedMultiLineComment(4) } };
            yield return new object[] { "  /*\n\n*", new[] { TextErrorInfo.UnterminatedMultiLineComment(7) } };
            yield return new object[] { "  /*\n\n* /", new[] { TextErrorInfo.UnterminatedMultiLineComment(9) } };

            // Invalid strings.
            yield return new object[] { "\"", new[] { TextErrorInfo.UnterminatedString(1) } };
            yield return new object[] { "\"\\", new[] { TextErrorInfo.UnterminatedString(2) } };

            // Unterminated because the closing " is escaped.
            yield return new object[] { "\"\\\"", new[] { TextErrorInfo.UnterminatedString(3) } };
            yield return new object[] { "\"\\ \"", new[] { TextErrorInfo.UnrecognizedEscapeSequence("\\ ", 1) } };
            yield return new object[] { "\"\\e\"", new[] { TextErrorInfo.UnrecognizedEscapeSequence("\\e", 1) } };

            // Unicode escape sequences.
            yield return new object[] { "\"\\u\"", new[] { TextErrorInfo.UnrecognizedEscapeSequence("\\u", 1) } };
            yield return new object[] { "\"\\ux\"", new[] { TextErrorInfo.UnrecognizedEscapeSequence("\\u", 1) } };
            yield return new object[] { "\"\\uxxxx\"", new[] { TextErrorInfo.UnrecognizedEscapeSequence("\\u", 1) } };
            yield return new object[] { "\"\\u0\"", new[] { TextErrorInfo.UnrecognizedUnicodeEscapeSequence("\\u0", 1, 3) } };
            yield return new object[] { "\"\\u00\"", new[] { TextErrorInfo.UnrecognizedUnicodeEscapeSequence("\\u00", 1, 4) } };
            yield return new object[] { "\"\\u000\"", new[] { TextErrorInfo.UnrecognizedUnicodeEscapeSequence("\\u000", 1, 5) } };
            yield return new object[] { "\"\\u000g\"", new[] { TextErrorInfo.UnrecognizedUnicodeEscapeSequence("\\u000", 1, 5) } };

            // Prevent int.TryParse hacks.
            yield return new object[] { "\"\\u-1000\"", new[] { TextErrorInfo.UnrecognizedUnicodeEscapeSequence("\\u", 1, 2) } };

            // Disallow control characters.
            yield return new object[] { "\"\n\"", new[] { TextErrorInfo.IllegalControlCharacterInString("\\n", 1) } };
            yield return new object[] { "\"\t\"", new[] { TextErrorInfo.IllegalControlCharacterInString("\\t", 1) } };
            yield return new object[] { "\"\0\"", new[] { TextErrorInfo.IllegalControlCharacterInString("\\0", 1) } };
            yield return new object[] { "\"\u0001\"", new[] { TextErrorInfo.IllegalControlCharacterInString("\\u0001", 1) } };
            yield return new object[] { "\"\u007f\"", new[] { TextErrorInfo.IllegalControlCharacterInString("\\u007f", 1) } };

            // Multiple errors.
            yield return new object[] { " ∙\"∙\"\"", new TextErrorInfo[] {
                TextErrorInfo.UnexpectedSymbol("∙", 1),
                TextErrorInfo.UnterminatedString(6) } };
            yield return new object[] { "\"\r\n\"", new[] {
                TextErrorInfo.IllegalControlCharacterInString("\\r", 1),
                TextErrorInfo.IllegalControlCharacterInString("\\n", 2) } };
            yield return new object[] { "\"\\ ", new[] {
                TextErrorInfo.UnrecognizedEscapeSequence("\\ ", 1),
                TextErrorInfo.UnterminatedString(3) } };
            yield return new object[] { "\"\r\n∙\"∙", new[] {
                TextErrorInfo.IllegalControlCharacterInString("\\r", 1),
                TextErrorInfo.IllegalControlCharacterInString("\\n", 2),
                TextErrorInfo.UnexpectedSymbol("∙", 5) } };
            yield return new object[] { "\"\t\n", new[] {
                TextErrorInfo.IllegalControlCharacterInString("\\t", 1),
                TextErrorInfo.IllegalControlCharacterInString("\\n", 2),
                TextErrorInfo.UnterminatedString(3) } };
            yield return new object[] { "\" \\ \n\"", new[] {
                TextErrorInfo.UnrecognizedEscapeSequence("\\ ", 2),
                TextErrorInfo.IllegalControlCharacterInString("\\n", 4) } };
            yield return new object[] { "\"\n\\ \n\"", new[] {
                TextErrorInfo.IllegalControlCharacterInString("\\n", 1),
                TextErrorInfo.UnrecognizedEscapeSequence("\\ ", 2),
                TextErrorInfo.IllegalControlCharacterInString("\\n", 4) } };
            yield return new object[] { "\"\\u", new[] {
                TextErrorInfo.UnrecognizedEscapeSequence("\\u", 1),
                TextErrorInfo.UnterminatedString(3) } };
            yield return new object[] { "\"\\uA", new[] {
                TextErrorInfo.UnrecognizedUnicodeEscapeSequence("\\uA", 1, 3),
                TextErrorInfo.UnterminatedString(4) } };
            yield return new object[] { "\"\\u\n", new[] {
                TextErrorInfo.UnrecognizedEscapeSequence("\\u", 1),
                TextErrorInfo.IllegalControlCharacterInString("\\n", 3),
                TextErrorInfo.UnterminatedString(4) } };
            yield return new object[] { "\"\\ufff\n", new[] {
                TextErrorInfo.UnrecognizedUnicodeEscapeSequence("\\ufff", 1, 5),
                TextErrorInfo.IllegalControlCharacterInString("\\n", 6),
                TextErrorInfo.UnterminatedString(7) } };
            yield return new object[] { "\"\n\\ ∙\"∙", new[] {
                TextErrorInfo.IllegalControlCharacterInString("\\n", 1),
                TextErrorInfo.UnrecognizedEscapeSequence("\\ ", 2),
                TextErrorInfo.UnexpectedSymbol("∙", 6) } };
            yield return new object[] { "∙\"\n\\ ∙\"", new[] {
                TextErrorInfo.UnexpectedSymbol("∙", 0),
                TextErrorInfo.IllegalControlCharacterInString("\\n", 2),
                TextErrorInfo.UnrecognizedEscapeSequence("\\ ", 3) } };

            // Know what's unterminated.
            yield return new object[] { "\"/*", new[] { TextErrorInfo.UnterminatedString(3) } };
            yield return new object[] { "/*\"", new[] { TextErrorInfo.UnterminatedMultiLineComment(3) } };
            yield return new object[] { "///*\n\"", new[] { TextErrorInfo.UnterminatedString(6) } };
            yield return new object[] { "///*\"\n/*", new[] { TextErrorInfo.UnterminatedMultiLineComment(8) } };
        }

        private class ErrorInfoFinder : JsonTerminalSymbolVisitor<IEnumerable<TextErrorInfo>>
        {
            public override IEnumerable<TextErrorInfo> DefaultVisit(JsonTerminalSymbol symbol)
                => Enumerable.Empty<TextErrorInfo>();

            public override IEnumerable<TextErrorInfo> VisitUnknownSymbol(JsonUnknownSymbol symbol)
            {
                yield return symbol.Error;
            }

            public override IEnumerable<TextErrorInfo> VisitUnterminatedMultiLineComment(JsonUnterminatedMultiLineComment symbol)
            {
                yield return symbol.Error;
            }

            public override IEnumerable<TextErrorInfo> VisitErrorString(JsonErrorString symbol)
                => symbol.Errors;
        }

        [Theory]
        [MemberData(nameof(GetErrorStrings))]
        public void Errors(string json, TextErrorInfo[] expectedErrors)
        {
            ErrorInfoFinder errorInfoFinder = new ErrorInfoFinder();

            var generatedErrors = new JsonTokenizer(json).TokenizeAll().SelectMany(errorInfoFinder.Visit);
            Assert.Collection(generatedErrors, expectedErrors.Select(expectedError => new Action<TextErrorInfo>(generatedError =>
            {
                Assert.NotNull(generatedError);
                Assert.Equal(expectedError.Message, generatedError.Message);
                Assert.Equal(expectedError.Start, generatedError.Start);
                Assert.Equal(expectedError.Length, generatedError.Length);
            })).ToArray());
        }
    }
}

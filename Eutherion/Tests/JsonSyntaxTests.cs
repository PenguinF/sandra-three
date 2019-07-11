#region License
/*********************************************************************************
 * JsonSyntaxTests.cs
 *
 * Copyright (c) 2004-2019 Henk Nicolai
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

using Eutherion.Text;
using Eutherion.Text.Json;
using System;
using System.Collections.Generic;
using Xunit;

namespace Eutherion.Shared.Tests
{
    public class JsonSyntaxTests
    {
        /// <summary>
        /// For testing JsonTerminalSymbols in general.
        /// </summary>
        private class JsonTestSymbol : JsonSymbol
        {
            public override int Length { get; }

            public JsonTestSymbol(int length) => Length = length;

            public override void Accept(JsonSymbolVisitor visitor) => visitor.DefaultVisit(this);
            public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.DefaultVisit(this);
            public override TResult Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.DefaultVisit(this, arg);
        }

        [Fact]
        public void NullSymbolShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new TextElement<JsonSymbol>(null, 0, 0));
        }

        [Fact]
        public void OutOfRangeArguments()
        {
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new JsonComment(-1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new JsonErrorString(Array.Empty<JsonErrorInfo>(), -1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new JsonErrorString(-1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new JsonString(string.Empty, -1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => new JsonUnterminatedMultiLineComment(-1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => JsonWhitespace.Create(-1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => JsonWhitespace.Create(0));

            Assert.Throws<ArgumentOutOfRangeException>("start", () => new TextElement<JsonSymbol>(new JsonString(string.Empty, 2), -1, 2));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(0, 2)]
        public void UnchangedParameters(int start, int length)
        {
            var testSymbol = new JsonTestSymbol(length);
            var textElement = new TextElement<JsonSymbol>(testSymbol, start, length);
            Assert.Equal(start, textElement.Start);
            Assert.Equal(length, textElement.Length);
            Assert.Same(testSymbol, textElement.TerminalSymbol);
        }

        [Fact]
        public void JsonSymbolsWithLengthOne()
        {
            // As long as there's code around which depends on these symbols having length 1, this unit test is needed.
            Assert.Equal(1, JsonColon.Value.Length);
            Assert.Equal(1, JsonComma.Value.Length);
            Assert.Equal(1, JsonCurlyClose.Value.Length);
            Assert.Equal(1, JsonCurlyOpen.Value.Length);
            Assert.Equal(1, JsonSquareBracketClose.Value.Length);
            Assert.Equal(1, JsonSquareBracketOpen.Value.Length);
            Assert.Equal(1, new JsonUnknownSymbol("\\0").Length);
        }

        [Fact]
        public void NullValueShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new JsonString(null, 0));
            Assert.Throws<ArgumentNullException>(() => JsonValue.Create(null));
        }

        [Theory]
        [InlineData("", 10)]
        [InlineData("{}", 3)]
        // No newline conversions.
        [InlineData("\n", 1)]
        [InlineData("\r\n", 2)]
        public void UnchangedValueParameter(string value, int length)
        {
            var jsonString = new JsonString(value, length);
            Assert.Equal(value, jsonString.Value);
            Assert.Equal(length, jsonString.Length);

            var jsonValue = JsonValue.Create(value);
            Assert.Equal(value, jsonValue.Value);
            Assert.Equal(value.Length, jsonValue.Length);
        }

        [Fact]
        public void NullErrorsShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new JsonErrorString(0, null));
            Assert.Throws<ArgumentNullException>(() => new JsonErrorString(null, 0));
        }

        [Theory]
        [InlineData(JsonErrorCode.Custom, "", 0, 0)]
        [InlineData(JsonErrorCode.Unspecified, null, 0, 1)]
        // No newline conversions.
        [InlineData(JsonErrorCode.UnterminatedString, "\n", 1, 0)]
        [InlineData(JsonErrorCode.UnrecognizedEscapeSequence, "\\u00", 0, 2)]
        public void UnchangedParametersInErrorString(JsonErrorCode errorCode, string errorParameter, int start, int length)
        {
            string[] parameters = errorParameter == null ? null : new[] { errorParameter };

            var errorInfo1 = new JsonErrorInfo(errorCode, start, length, parameters);
            var errorInfo2 = new JsonErrorInfo(errorCode, start + 1, length * 2, parameters);
            var errorInfo3 = new JsonErrorInfo(errorCode, start + 2, length * 3, parameters);

            Assert.Collection(
                new JsonErrorString(length * 6, errorInfo1, errorInfo2, errorInfo3).Errors,
                error1 => Assert.Same(errorInfo1, error1),
                error2 => Assert.Same(errorInfo2, error2),
                error3 => Assert.Same(errorInfo3, error3));

            // Assert that the elements of the list are copied, i.e. that if this collection is modified
            // after being used to create a JsonErrorInfo, it does not change that JsonErrorInfo.
            var errorList = new List<JsonErrorInfo> { errorInfo1, errorInfo2, errorInfo3 };
            var errorString = new JsonErrorString(errorList, 1);
            Assert.NotSame(errorString.Errors, errorList);

            // errorString.Errors should still return the same set of JsonErrorInfos after this statement.
            errorList.Add(errorInfo1);

            Assert.Collection(
                errorString.Errors,
                error1 => Assert.Same(errorInfo1, error1),
                error2 => Assert.Same(errorInfo2, error2),
                error3 => Assert.Same(errorInfo3, error3));
        }

        [Fact]
        public void UnexpectedSymbolShouldBeNotNull()
        {
            Assert.Throws<ArgumentNullException>(() => new JsonUnknownSymbol(null));
        }

        [Fact]
        public void UnexpectedSymbolShouldBeNonEmpty()
        {
            Assert.Throws<ArgumentException>(() => new JsonUnknownSymbol(string.Empty));
        }

        [Theory]
        [InlineData("*")]
        [InlineData("€")]
        public void UnchangedParametersInUnexpectedSymbol(string displayCharValue)
        {
            var symbol = new JsonUnknownSymbol(displayCharValue);
            Assert.Equal(displayCharValue, symbol.DisplayCharValue);
        }

        [Theory]
        [InlineData("/*  *")]
        public void UnchangedParametersInUnterminatedMultiLineComment(string commentText)
        {
            var symbol = new JsonUnterminatedMultiLineComment(commentText.Length);
            Assert.Equal(commentText.Length, symbol.Length);
        }

        [Theory]
        [InlineData("*", 0)]
        [InlineData("€", 0)]
        [InlineData("≥", 0)]

        [InlineData("▓", 200)]
        public void UnexpectedSymbolError(string displayCharValue, int position)
        {
            var error = JsonUnknownSymbol.CreateError(displayCharValue, position);
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
            var error = JsonUnterminatedMultiLineComment.CreateError(start, length);
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
            var error = JsonErrorString.Unterminated(start, length);
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
            var error = JsonErrorString.IllegalControlCharacter(displayCharValue, position);
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
            var error = JsonErrorString.UnrecognizedEscapeSequence(displayCharValue, position);
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
            var error = JsonErrorString.UnrecognizedUnicodeEscapeSequence(displayCharValue, position, displayCharValue.Length);
            Assert.Equal(JsonErrorCode.UnrecognizedEscapeSequence, error.ErrorCode);
            Assert.Collection(error.Parameters, x => Assert.Equal(displayCharValue, x));
            Assert.Equal(position, error.Start);
            Assert.Equal(displayCharValue.Length, error.Length);
        }

        public static IEnumerable<object[]> TerminalSymbolsOfEachType()
        {
            yield return new object[] { new JsonComment(2), typeof(JsonComment) };
            yield return new object[] { new JsonUnterminatedMultiLineComment(2), typeof(JsonUnterminatedMultiLineComment) };
            yield return new object[] { JsonCurlyOpen.Value, typeof(JsonCurlyOpen) };
            yield return new object[] { JsonCurlyClose.Value, typeof(JsonCurlyClose) };
            yield return new object[] { JsonSquareBracketOpen.Value, typeof(JsonSquareBracketOpen) };
            yield return new object[] { JsonSquareBracketClose.Value, typeof(JsonSquareBracketClose) };
            yield return new object[] { JsonColon.Value, typeof(JsonColon) };
            yield return new object[] { JsonComma.Value, typeof(JsonComma) };
            yield return new object[] { new JsonUnknownSymbol("*"), typeof(JsonUnknownSymbol) };
            yield return new object[] { JsonValue.TrueJsonValue, typeof(JsonValue) };
            yield return new object[] { new JsonString(string.Empty, 0), typeof(JsonString) };
            yield return new object[] { new JsonErrorString(1, JsonErrorString.Unterminated(0, 1)), typeof(JsonErrorString) };
            yield return new object[] { JsonWhitespace.Create(2), typeof(JsonWhitespace) };
        }

        private sealed class TestVisitor1 : JsonSymbolVisitor
        {
            public bool DefaultVisited;

            public override void DefaultVisit(JsonSymbol symbol) => DefaultVisited = true;
        }

        [Theory]
        [MemberData(nameof(TerminalSymbolsOfEachType))]
#pragma warning disable xUnit1026 // otherwise 2 generate methods are necessary.
        public void DefaultVisitVoid(JsonSymbol symbol, Type symbolType)
#pragma warning restore xUnit1026
        {
            var testVisitor = new TestVisitor1();
            testVisitor.Visit(symbol);
            Assert.True(testVisitor.DefaultVisited);
        }

        private sealed class TestVisitor2 : JsonSymbolVisitor<int>
        {
            public const int ReturnValue = 1;

            public override int DefaultVisit(JsonSymbol symbol) => ReturnValue;
        }

        [Theory]
        [MemberData(nameof(TerminalSymbolsOfEachType))]
#pragma warning disable xUnit1026 // otherwise 2 generate methods are necessary.
        public void DefaultVisitInt(JsonSymbol symbol, Type symbolType)
#pragma warning restore xUnit1026
        {
            var testVisitor = new TestVisitor2();
            Assert.Equal(TestVisitor2.ReturnValue, testVisitor.Visit(symbol));
        }

        private sealed class TestVisitor3 : JsonSymbolVisitor
        {
            public Type VisitedType;

            public override void VisitColon(JsonColon symbol) => VisitedType = typeof(JsonColon);
            public override void VisitComma(JsonComma symbol) => VisitedType = typeof(JsonComma);
            public override void VisitComment(JsonComment symbol) => VisitedType = typeof(JsonComment);
            public override void VisitCurlyClose(JsonCurlyClose symbol) => VisitedType = typeof(JsonCurlyClose);
            public override void VisitCurlyOpen(JsonCurlyOpen symbol) => VisitedType = typeof(JsonCurlyOpen);
            public override void VisitErrorString(JsonErrorString symbol) => VisitedType = typeof(JsonErrorString);
            public override void VisitSquareBracketClose(JsonSquareBracketClose symbol) => VisitedType = typeof(JsonSquareBracketClose);
            public override void VisitSquareBracketOpen(JsonSquareBracketOpen symbol) => VisitedType = typeof(JsonSquareBracketOpen);
            public override void VisitString(JsonString symbol) => VisitedType = typeof(JsonString);
            public override void VisitUnknownSymbol(JsonUnknownSymbol symbol) => VisitedType = typeof(JsonUnknownSymbol);
            public override void VisitUnterminatedMultiLineComment(JsonUnterminatedMultiLineComment symbol) => VisitedType = typeof(JsonUnterminatedMultiLineComment);
            public override void VisitValue(JsonValue symbol) => VisitedType = typeof(JsonValue);
            public override void VisitWhitespace(JsonWhitespace symbol) => VisitedType = typeof(JsonWhitespace);

            public override void DefaultVisit(JsonSymbol symbol)
            {
                throw new InvalidOperationException("DefaultVisit should not have been called");
            }
        }

        [Theory]
        [MemberData(nameof(TerminalSymbolsOfEachType))]
        public void NonDefaultVisitVoid(JsonSymbol symbol, Type symbolType)
        {
            var testVisitor = new TestVisitor3();
            testVisitor.Visit(symbol);
            Assert.Equal(symbolType, testVisitor.VisitedType);
        }

        private sealed class TestVisitor4 : JsonSymbolVisitor<Type>
        {
            public override Type VisitColon(JsonColon symbol) => typeof(JsonColon);
            public override Type VisitComma(JsonComma symbol) => typeof(JsonComma);
            public override Type VisitComment(JsonComment symbol) => typeof(JsonComment);
            public override Type VisitCurlyClose(JsonCurlyClose symbol) => typeof(JsonCurlyClose);
            public override Type VisitCurlyOpen(JsonCurlyOpen symbol) => typeof(JsonCurlyOpen);
            public override Type VisitErrorString(JsonErrorString symbol) => typeof(JsonErrorString);
            public override Type VisitSquareBracketClose(JsonSquareBracketClose symbol) => typeof(JsonSquareBracketClose);
            public override Type VisitSquareBracketOpen(JsonSquareBracketOpen symbol) => typeof(JsonSquareBracketOpen);
            public override Type VisitString(JsonString symbol) => typeof(JsonString);
            public override Type VisitUnknownSymbol(JsonUnknownSymbol symbol) => typeof(JsonUnknownSymbol);
            public override Type VisitUnterminatedMultiLineComment(JsonUnterminatedMultiLineComment symbol) => typeof(JsonUnterminatedMultiLineComment);
            public override Type VisitValue(JsonValue symbol) => typeof(JsonValue);
            public override Type VisitWhitespace(JsonWhitespace symbol) => typeof(JsonWhitespace);

            public override Type DefaultVisit(JsonSymbol symbol)
            {
                throw new InvalidOperationException("DefaultVisit should not have been called");
            }
        }

        [Theory]
        [MemberData(nameof(TerminalSymbolsOfEachType))]
        public void NonDefaultVisit(JsonSymbol symbol, Type symbolType)
        {
            var testVisitor = new TestVisitor4();
            Assert.Equal(symbolType, testVisitor.Visit(symbol));
        }

        private sealed class TestVisitor5 : JsonSymbolVisitor<string, string>
        {
            public override string VisitColon(JsonColon symbol, string x) => typeof(JsonColon).Name + x;
            public override string VisitComma(JsonComma symbol, string x) => typeof(JsonComma).Name + x;
            public override string VisitComment(JsonComment symbol, string x) => typeof(JsonComment).Name + x;
            public override string VisitCurlyClose(JsonCurlyClose symbol, string x) => typeof(JsonCurlyClose).Name + x;
            public override string VisitCurlyOpen(JsonCurlyOpen symbol, string x) => typeof(JsonCurlyOpen).Name + x;
            public override string VisitErrorString(JsonErrorString symbol, string x) => typeof(JsonErrorString).Name + x;
            public override string VisitSquareBracketClose(JsonSquareBracketClose symbol, string x) => typeof(JsonSquareBracketClose).Name + x;
            public override string VisitSquareBracketOpen(JsonSquareBracketOpen symbol, string x) => typeof(JsonSquareBracketOpen).Name + x;
            public override string VisitString(JsonString symbol, string x) => typeof(JsonString).Name + x;
            public override string VisitUnknownSymbol(JsonUnknownSymbol symbol, string x) => typeof(JsonUnknownSymbol).Name + x;
            public override string VisitUnterminatedMultiLineComment(JsonUnterminatedMultiLineComment symbol, string x) => typeof(JsonUnterminatedMultiLineComment).Name + x;
            public override string VisitValue(JsonValue symbol, string x) => typeof(JsonValue).Name + x;
            public override string VisitWhitespace(JsonWhitespace symbol, string x) => typeof(JsonWhitespace).Name + x;

            public override string DefaultVisit(JsonSymbol symbol, string x)
            {
                throw new InvalidOperationException("DefaultVisit should not have been called");
            }
        }

        [Theory]
        [MemberData(nameof(TerminalSymbolsOfEachType))]
        public void NonDefaultVisitWithArg(JsonSymbol symbol, Type symbolType)
        {
            var testVisitor = new TestVisitor5();
            var name = symbolType.Name;
            Assert.Equal(name + name, testVisitor.Visit(symbol, name));
        }
    }
}

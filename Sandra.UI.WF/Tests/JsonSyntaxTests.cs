#region License
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
using SysExtensions.Text;
using System;
using System.Collections.Generic;
using Xunit;

namespace Sandra.UI.WF.Tests
{
    public class JsonSyntaxTests
    {
        /// <summary>
        /// For testing JsonTerminalSymbols in general.
        /// </summary>
        private class JsonTestSymbol : JsonSymbol
        {
            public override void Accept(JsonSymbolVisitor visitor) => visitor.DefaultVisit(this);
            public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.DefaultVisit(this);
        }

        [Fact]
        public void NullSymbolShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new TextElement<JsonSymbol>(null, 0, 0));
        }

        [Theory]
        [InlineData(-1, 0, "start")]
        [InlineData(-1, -1, "start")]
        [InlineData(0, -1, "length")]
        public void OutOfRangeArguments(int start, int length, string parameterName)
        {
            Assert.Throws<ArgumentOutOfRangeException>(parameterName, () => new TextElement<JsonSymbol>(new JsonTestSymbol(), start, length));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(0, 2)]
        public void UnchangedParameters(int start, int length)
        {
            var testSymbol = new JsonTestSymbol();
            var textElement = new TextElement<JsonSymbol>(testSymbol, start, length);
            Assert.Equal(start, textElement.Start);
            Assert.Equal(length, textElement.Length);
            Assert.Same(testSymbol, textElement.TerminalSymbol);
        }

        [Fact]
        public void NullValueShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new JsonString(null));
            Assert.Throws<ArgumentNullException>(() => new JsonValue(null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("{}")]
        // No newline conversions.
        [InlineData("\n")]
        [InlineData("\r\n")]
        public void UnchangedValueParameter(string value)
        {
            var jsonString = new JsonString(value);
            Assert.Equal(value, jsonString.Value);

            var jsonValue = new JsonValue(value);
            Assert.Equal(value, jsonString.Value);
        }

        [Fact]
        public void NullErrorsShouldThrow()
        {
            // Explicit casts to ensure the right constructor overload is always called.
            Assert.Throws<ArgumentNullException>(() => new JsonErrorString((TextErrorInfo[])null));
            Assert.Throws<ArgumentNullException>(() => new JsonErrorString((IEnumerable<TextErrorInfo>)null));
        }

        [Theory]
        [InlineData("", 0, 0)]
        [InlineData("Error!", 0, 1)]
        // No newline conversions.
        [InlineData("\n", 1, 0)]
        [InlineData("Error!\r\n", 0, 2)]
        public void UnchangedParametersInErrorString(string message, int start, int length)
        {
            var errorInfo1 = new TextErrorInfo(message, start, length);
            var errorInfo2 = new TextErrorInfo(message + message, start + 1, length * 2);
            var errorInfo3 = new TextErrorInfo(message + message + message, start + 2, length * 3);

            Assert.Collection(
                new JsonErrorString(errorInfo1, errorInfo2, errorInfo3).Errors,
                error1 => Assert.Same(errorInfo1, error1),
                error2 => Assert.Same(errorInfo2, error2),
                error3 => Assert.Same(errorInfo3, error3));

            // Assert that the elements of the list are copied, i.e. that if this collection is modified
            // after being used to create a JsonErrorInfo, it does not change that JsonErrorInfo.
            var errorList = new List<TextErrorInfo> { errorInfo1, errorInfo2, errorInfo3 };
            var errorString = new JsonErrorString(errorList);
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
            Assert.Throws<ArgumentNullException>(() => new JsonUnknownSymbol(null, 0));
        }

        [Fact]
        public void UnexpectedSymbolShouldBeNonEmpty()
        {
            Assert.Throws<ArgumentException>(() => new JsonUnknownSymbol(string.Empty, 0));
        }

        [Theory]
        [InlineData("*")]
        [InlineData("€")]
        public void UnchangedParametersInUnexpectedSymbol(string displayCharValue)
        {
            var error = JsonUnknownSymbol.CreateError(displayCharValue, 0);
            var symbol = new JsonUnknownSymbol(displayCharValue, 0);
            Assert.True(error.EqualTo(symbol.Error));
        }

        [Theory]
        [InlineData("/*  *")]
        public void UnchangedParametersInUnterminatedMultiLineComment(string commentText)
        {
            var error = JsonUnterminatedMultiLineComment.CreateError(0, commentText.Length);
            var symbol = new JsonUnterminatedMultiLineComment(0, commentText.Length);
            Assert.True(error.EqualTo(symbol.Error));
        }

        [Theory]
        [InlineData("*", 0)]
        [InlineData("€", 0)]
        [InlineData("≥", 0)]

        [InlineData("▓", 200)]
        public void UnexpectedSymbolMessage(string displayCharValue, int position)
        {
            var error = JsonUnknownSymbol.CreateError(displayCharValue, position);
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
            var error = JsonUnterminatedMultiLineComment.CreateError(start, length);
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
            var error = JsonErrorString.Unterminated(start, length);
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
            var error = JsonErrorString.IllegalControlCharacter(displayCharValue, position);
            Assert.Equal($"Illegal control character '{displayCharValue}' in string", error.Message);
            Assert.Equal(position, error.Start);
            Assert.Equal(1, error.Length);
        }

        [Theory]
        [InlineData("\\ ", 2)]
        [InlineData("\\0", 1)]
        public void UnrecognizedEscapeSequenceMessage(string displayCharValue, int position)
        {
            var error = JsonErrorString.UnrecognizedEscapeSequence(displayCharValue, position);
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
            var error = JsonErrorString.UnrecognizedUnicodeEscapeSequence(displayCharValue, position, displayCharValue.Length);
            Assert.Equal($"Unrecognized escape sequence ('{displayCharValue}')", error.Message);
            Assert.Equal(position, error.Start);
            Assert.Equal(displayCharValue.Length, error.Length);
        }

        public static IEnumerable<object[]> TerminalSymbolsOfEachType()
        {
            yield return new object[] { JsonComment.Value, typeof(JsonComment) };
            yield return new object[] { new JsonUnterminatedMultiLineComment(0, 2), typeof(JsonUnterminatedMultiLineComment) };
            yield return new object[] { JsonCurlyOpen.Value, typeof(JsonCurlyOpen) };
            yield return new object[] { JsonCurlyClose.Value, typeof(JsonCurlyClose) };
            yield return new object[] { JsonSquareBracketOpen.Value, typeof(JsonSquareBracketOpen) };
            yield return new object[] { JsonSquareBracketClose.Value, typeof(JsonSquareBracketClose) };
            yield return new object[] { JsonColon.Value, typeof(JsonColon) };
            yield return new object[] { JsonComma.Value, typeof(JsonComma) };
            yield return new object[] { new JsonUnknownSymbol("*", 0), typeof(JsonUnknownSymbol) };
            yield return new object[] { new JsonValue("true"), typeof(JsonValue) };
            yield return new object[] { new JsonString(string.Empty), typeof(JsonString) };
            yield return new object[] { new JsonErrorString(new[] { JsonErrorString.Unterminated(0, 1) }), typeof(JsonErrorString) };
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
    }
}

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
        private class JsonTestSymbol : JsonTerminalSymbol
        {
            public JsonTestSymbol(string json, int start, int length) : base(json, start, length) { }

            public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.DefaultVisit(this);
            public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.DefaultVisit(this);
        }

        [Fact]
        public void NullJsonShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new JsonTestSymbol(null, 0, 0));
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
            Assert.Throws<ArgumentOutOfRangeException>(parameterName, () => new JsonTestSymbol(json, start, length));
        }

        [Theory]
        [InlineData("", 0, 0)]
        [InlineData("{}", 0, 1)]
        // No newline conversions.
        [InlineData("\n", 1, 0)]
        [InlineData("\r\n", 0, 2)]
        public void UnchangedParameters(string json, int start, int length)
        {
            var terminalSymbol = new JsonTestSymbol(json, start, length);
            Assert.Equal(json, terminalSymbol.Json);
            Assert.Equal(start, terminalSymbol.Start);
            Assert.Equal(length, terminalSymbol.Length);
        }

        [Fact]
        public void NullErrorsShouldThrow()
        {
            // Explicit casts to ensure the right constructor overload is always called.
            Assert.Throws<ArgumentNullException>(() => new JsonErrorString("", 0, 0, (TextErrorInfo[])null));
            Assert.Throws<ArgumentNullException>(() => new JsonErrorString("", 0, 0, (IEnumerable<TextErrorInfo>)null));
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
                new JsonErrorString("", 0, 0, errorInfo1, errorInfo2, errorInfo3).Errors,
                error1 => Assert.Same(errorInfo1, error1),
                error2 => Assert.Same(errorInfo2, error2),
                error3 => Assert.Same(errorInfo3, error3));

            // Assert that the elements of the list are copied, i.e. that if this collection is modified
            // after being used to create a JsonErrorInfo, it does not change that JsonErrorInfo.
            var errorList = new List<TextErrorInfo> { errorInfo1, errorInfo2, errorInfo3 };
            var errorString = new JsonErrorString("", 0, 0, errorList);
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
        public void NullErrorInUnexpectedSymbolShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new JsonUnknownSymbol("*", 0, null));
        }

        [Theory]
        [InlineData("*")]
        [InlineData("€")]
        public void UnchangedParametersInUnexpectedSymbol(string displayCharValue)
        {
            var error = TextErrorInfo.UnexpectedSymbol(displayCharValue, 0);
            var symbol = new JsonUnknownSymbol(displayCharValue, 0, error);
            Assert.Same(error, symbol.Error);
        }

        [Fact]
        public void NullErrorInUnterminatedMultiLineCommentShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => new JsonUnterminatedMultiLineComment("/*", 0, 2, null));
        }

        [Theory]
        [InlineData("/*  *")]
        public void UnchangedParametersInUnterminatedMultiLineComment(string commentText)
        {
            var error = TextErrorInfo.UnterminatedMultiLineComment(0, commentText.Length);
            var symbol = new JsonUnterminatedMultiLineComment(commentText, 0, commentText.Length, error);
            Assert.Same(error, symbol.Error);
        }

        public static IEnumerable<object[]> TerminalSymbolsOfEachType()
        {
            yield return new object[] { new JsonComment("//", 0, 2), typeof(JsonComment) };
            yield return new object[] { new JsonUnterminatedMultiLineComment("/*", 0, 2, TextErrorInfo.UnterminatedMultiLineComment(0, 2)), typeof(JsonUnterminatedMultiLineComment) };
            yield return new object[] { new JsonCurlyOpen("{", 0), typeof(JsonCurlyOpen) };
            yield return new object[] { new JsonCurlyClose("}", 0), typeof(JsonCurlyClose) };
            yield return new object[] { new JsonSquareBracketOpen("[", 0), typeof(JsonSquareBracketOpen) };
            yield return new object[] { new JsonSquareBracketClose("]", 0), typeof(JsonSquareBracketClose) };
            yield return new object[] { new JsonColon(":", 0), typeof(JsonColon) };
            yield return new object[] { new JsonComma(",", 0), typeof(JsonComma) };
            yield return new object[] { new JsonUnknownSymbol("*", 0, TextErrorInfo.UnexpectedSymbol("*", 0)), typeof(JsonUnknownSymbol) };
            yield return new object[] { new JsonValue("true", 0, 4), typeof(JsonValue) };
            yield return new object[] { new JsonString("\"\"", 0, 2, string.Empty), typeof(JsonString) };
            yield return new object[] { new JsonErrorString("\"", 0, 1, TextErrorInfo.UnterminatedString(0, 1)), typeof(JsonErrorString) };
        }

        private sealed class TestVisitor1 : JsonTerminalSymbolVisitor
        {
            public bool DefaultVisited;

            public override void DefaultVisit(JsonTerminalSymbol symbol) => DefaultVisited = true;
        }

        [Theory]
        [MemberData(nameof(TerminalSymbolsOfEachType))]
#pragma warning disable xUnit1026 // otherwise 2 generate methods are necessary.
        public void DefaultVisitVoid(JsonTerminalSymbol symbol, Type symbolType)
#pragma warning restore xUnit1026
        {
            var testVisitor = new TestVisitor1();
            testVisitor.Visit(symbol);
            Assert.True(testVisitor.DefaultVisited);
        }

        private sealed class TestVisitor2 : JsonTerminalSymbolVisitor<int>
        {
            public const int ReturnValue = 1;

            public override int DefaultVisit(JsonTerminalSymbol symbol) => ReturnValue;
        }

        [Theory]
        [MemberData(nameof(TerminalSymbolsOfEachType))]
#pragma warning disable xUnit1026 // otherwise 2 generate methods are necessary.
        public void DefaultVisitInt(JsonTerminalSymbol symbol, Type symbolType)
#pragma warning restore xUnit1026
        {
            var testVisitor = new TestVisitor2();
            Assert.Equal(TestVisitor2.ReturnValue, testVisitor.Visit(symbol));
        }

        private sealed class TestVisitor3 : JsonTerminalSymbolVisitor
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

            public override void DefaultVisit(JsonTerminalSymbol symbol)
            {
                throw new InvalidOperationException("DefaultVisit should not have been called");
            }
        }

        [Theory]
        [MemberData(nameof(TerminalSymbolsOfEachType))]
        public void NonDefaultVisitVoid(JsonTerminalSymbol symbol, Type symbolType)
        {
            var testVisitor = new TestVisitor3();
            testVisitor.Visit(symbol);
            Assert.Equal(symbolType, testVisitor.VisitedType);
        }

        private sealed class TestVisitor4 : JsonTerminalSymbolVisitor<Type>
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

            public override Type DefaultVisit(JsonTerminalSymbol symbol)
            {
                throw new InvalidOperationException("DefaultVisit should not have been called");
            }
        }

        [Theory]
        [MemberData(nameof(TerminalSymbolsOfEachType))]
        public void NonDefaultVisit(JsonTerminalSymbol symbol, Type symbolType)
        {
            var testVisitor = new TestVisitor4();
            Assert.Equal(symbolType, testVisitor.Visit(symbol));
        }
    }
}

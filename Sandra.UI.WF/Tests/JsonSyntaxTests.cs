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
            Assert.True(json == terminalSymbol.Json);
            Assert.True(start == terminalSymbol.Start);
            Assert.True(length == terminalSymbol.Length);
        }

        [Fact]
        public void NullMessageShouldThrowInError()
        {
            Assert.Throws<ArgumentNullException>(() => new JsonErrorInfo(null, 0, 0));
        }

        [Theory]
        [InlineData(-1, 0, "start")]
        [InlineData(-1, -1, "start")]
        [InlineData(0, -1, "length")]
        public void OutOfRangeArgumentsInError(int start, int length, string parameterName)
        {
            Assert.Throws<ArgumentOutOfRangeException>(parameterName, () => new JsonErrorInfo(string.Empty, start, length));
        }

        [Theory]
        [InlineData("", 0, 0)]
        [InlineData("Error!", 0, 1)]
        // No newline conversions.
        [InlineData("\n", 1, 0)]
        [InlineData("Error!\r\n", 0, 2)]
        public void UnchangedParametersInError(string message, int start, int length)
        {
            var errorInfo = new JsonErrorInfo(message, start, length);
            Assert.True(message == errorInfo.Message);
            Assert.True(start == errorInfo.Start);
            Assert.True(length == errorInfo.Length);
        }

        public static IEnumerable<object[]> TerminalSymbolsOfEachType()
        {
            yield return new object[] { new JsonCurlyOpen("{", 0), typeof(JsonCurlyOpen) };
            yield return new object[] { new JsonCurlyClose("}", 0), typeof(JsonCurlyClose) };
            yield return new object[] { new JsonSquareBracketOpen("[", 0), typeof(JsonSquareBracketOpen) };
            yield return new object[] { new JsonSquareBracketClose("]", 0), typeof(JsonSquareBracketClose) };
            yield return new object[] { new JsonColon(":", 0), typeof(JsonColon) };
            yield return new object[] { new JsonComma(",", 0), typeof(JsonComma) };
            yield return new object[] { new JsonUnknownSymbol(string.Empty, 0, 0), typeof(JsonUnknownSymbol) };
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
            public override void VisitCurlyClose(JsonCurlyClose symbol) => VisitedType = typeof(JsonCurlyClose);
            public override void VisitCurlyOpen(JsonCurlyOpen symbol) => VisitedType = typeof(JsonCurlyOpen);
            public override void VisitSquareBracketClose(JsonSquareBracketClose symbol) => VisitedType = typeof(JsonSquareBracketClose);
            public override void VisitSquareBracketOpen(JsonSquareBracketOpen symbol) => VisitedType = typeof(JsonSquareBracketOpen);
            public override void VisitUnknownSymbol(JsonUnknownSymbol symbol) => VisitedType = typeof(JsonUnknownSymbol);

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
            public override Type VisitCurlyClose(JsonCurlyClose symbol) => typeof(JsonCurlyClose);
            public override Type VisitCurlyOpen(JsonCurlyOpen symbol) => typeof(JsonCurlyOpen);
            public override Type VisitSquareBracketClose(JsonSquareBracketClose symbol) => typeof(JsonSquareBracketClose);
            public override Type VisitSquareBracketOpen(JsonSquareBracketOpen symbol) => typeof(JsonSquareBracketOpen);
            public override Type VisitUnknownSymbol(JsonUnknownSymbol symbol) => typeof(JsonUnknownSymbol);

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

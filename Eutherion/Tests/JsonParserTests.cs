#region License
/*********************************************************************************
 * JsonParserTests.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Eutherion.Shared.Tests
{
    public class JsonParserTests
    {
        private class UnexpectedJsonSymbolException : Exception
        {
        }

        private sealed class TerminalSymbolTester : JsonSymbolVisitor<IGreenJsonSymbol>
        {
            public static readonly TerminalSymbolTester Instance = new TerminalSymbolTester();

            private TerminalSymbolTester() { }

            public override IGreenJsonSymbol VisitBooleanLiteralSyntax(JsonBooleanLiteralSyntax node) => node.Green;
            public override IGreenJsonSymbol VisitColonSyntax(JsonColonSyntax node) => node.Green;
            public override IGreenJsonSymbol VisitCommaSyntax(JsonCommaSyntax node) => node.Green;
            public override IGreenJsonSymbol VisitCommentSyntax(JsonCommentSyntax node) => node.Green;
            public override IGreenJsonSymbol VisitCurlyCloseSyntax(JsonCurlyCloseSyntax node) => node.Green;
            public override IGreenJsonSymbol VisitCurlyOpenSyntax(JsonCurlyOpenSyntax node) => node.Green;
            public override IGreenJsonSymbol VisitErrorStringSyntax(JsonErrorStringSyntax node) => node.Green;
            public override IGreenJsonSymbol VisitIntegerLiteralSyntax(JsonIntegerLiteralSyntax node) => node.Green;
            public override IGreenJsonSymbol VisitMissingValueSyntax(JsonMissingValueSyntax node) => throw new UnexpectedJsonSymbolException();
            public override IGreenJsonSymbol VisitRootLevelValueDelimiterSyntax(JsonRootLevelValueDelimiterSyntax node) => node.Green.ValueDelimiter;
            public override IGreenJsonSymbol VisitSquareBracketCloseSyntax(JsonSquareBracketCloseSyntax node) => node.Green;
            public override IGreenJsonSymbol VisitSquareBracketOpenSyntax(JsonSquareBracketOpenSyntax node) => node.Green;
            public override IGreenJsonSymbol VisitStringLiteralSyntax(JsonStringLiteralSyntax node) => node.Green;
            public override IGreenJsonSymbol VisitUndefinedValueSyntax(JsonUndefinedValueSyntax node) => node.Green;
            public override IGreenJsonSymbol VisitUnknownSymbolSyntax(JsonUnknownSymbolSyntax node) => node.Green;
            public override IGreenJsonSymbol VisitUnterminatedMultiLineCommentSyntax(JsonUnterminatedMultiLineCommentSyntax node) => node.Green;
            public override IGreenJsonSymbol VisitWhitespaceSyntax(JsonWhitespaceSyntax node) => node.Green;
        }

        public abstract class ParseTree : IEnumerable<ParseTree>
        {
            public abstract Type ExpectedType { get; }
            public readonly List<ParseTree> ChildNodes = new List<ParseTree>();

            // To enable collection initializer syntax:
            public IEnumerator<ParseTree> GetEnumerator() => ChildNodes.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public void Add(ParseTree child) => ChildNodes.Add(child);
        }

        public class ParseTree<T> : ParseTree where T : JsonSyntax
        {
            public override Type ExpectedType => typeof(T);
        }

        /// <summary>
        /// <seealso cref="JsonTokenizerTests.TwoSymbolsOfEachType"/>.
        /// </summary>
        public static IEnumerable<object[]> TwoSymbolsWithoutType()
        {
            var symbolTypes = JsonTokenizerTests.JsonTestSymbols();

            // Unterminated strings/comments mess up the tokenization, skip those if they're the first key.
            foreach (var (key1, _) in symbolTypes)
            {
                foreach (var (key2, _) in symbolTypes.Union(JsonTokenizerTests.UnterminatedJsonTestSymbols()))
                {
                    yield return new object[] { key1, key2 };
                }
            }
        }

        /// <summary>
        /// Tests if terminal symbols returned by a parsed <see cref="JsonSyntax"/> match those returned by the <see cref="JsonTokenizer"/>.
        /// </summary>
        [Theory]
        [MemberData(nameof(TwoSymbolsWithoutType))]
        public void ParseTreeTokensMatch(string json1, string json2)
        {
            // Sane structure as JsonTokenizerTests.Transition: first check two symbols, then all combinations of three.
            {
                string json = json1 + json2;
                var expectedTokens = JsonTokenizer.TokenizeAll(json).Where(x => x.Length > 0);
                Action<IJsonSymbol>[] tokenInspectors = expectedTokens.Select<IGreenJsonSymbol, Action<IJsonSymbol>>((IGreenJsonSymbol expectedGreen) => (IJsonSymbol red) =>
                {
                    IGreenJsonSymbol actualGreen = TerminalSymbolTester.Instance.Visit(red);
                    Assert.IsType(expectedGreen.GetType(), actualGreen);
                    Assert.Equal(expectedGreen.Length, actualGreen.Length);
                    Assert.Equal(expectedGreen.Length, red.Length);
                }).ToArray();

                Assert.Collection(
                    // Skip JsonMissingValueSyntax instances.
                    JsonParser.Parse(json).Syntax.TerminalSymbolsInRange(0, json.Length).Where(x => x.Length > 0),
                    tokenInspectors);
            }

            // Here Assert.Collection is used so if such a test fails,
            // it gives the index of the third token that was tested.
            Assert.Collection(
                JsonTokenizerTests.JsonTestSymbols(),
                Enumerable.Repeat<Action<(string, Type)>>(x0 =>
                {
                    string json = x0.Item1 + json1 + json2;
                    var expectedTokens = JsonTokenizer.TokenizeAll(json).Where(x => x.Length > 0);
                    Action<IJsonSymbol>[] tokenInspectors = expectedTokens.Select<IGreenJsonSymbol, Action<IJsonSymbol>>(expectedGreen => symbol =>
                    {
                        IGreenJsonSymbol actualGreen = TerminalSymbolTester.Instance.Visit(symbol);
                        Assert.IsType(expectedGreen.GetType(), actualGreen);
                        Assert.Equal(expectedGreen.Length, actualGreen.Length);
                        Assert.Equal(expectedGreen.Length, symbol.Length);
                    }).ToArray();

                    Assert.Collection(
                        // Skip JsonMissingValueSyntax instances.
                        JsonParser.Parse(json).Syntax.TerminalSymbolsInRange(0, json.Length).Where(x => x.Length > 0),
                        tokenInspectors);

                }, JsonTokenizerTests.JsonTestSymbols().Count()).ToArray());
        }

        private static readonly List<(string, ParseTree)> TestParseTrees = new List<(string, ParseTree)>
        {
            ("", new ParseTree<JsonMultiValueSyntax>
            {
                new ParseTree<JsonValueWithBackgroundSyntax>
                {
                    new ParseTree<JsonBackgroundListSyntax>(),
                    new ParseTree<JsonMissingValueSyntax>()
                },
                new ParseTree<JsonBackgroundListSyntax>()
            }),
        };

        private static readonly List<(string, ParseTree, JsonErrorCode[])> TestParseTreesWithErrors = new List<(string, ParseTree, JsonErrorCode[])>
        {
            ("/*", new ParseTree<JsonMultiValueSyntax>
            {
                new ParseTree<JsonValueWithBackgroundSyntax>
                {
                    new ParseTree<JsonBackgroundListSyntax> { new ParseTree<JsonUnterminatedMultiLineCommentSyntax>() },
                    new ParseTree<JsonMissingValueSyntax>()
                },
                new ParseTree<JsonBackgroundListSyntax>()
            },
            new[] { JsonErrorCode.UnterminatedMultiLineComment } ),
        };

        private static void AssertParseTree(ParseTree expectedParseTree, JsonSyntax actualParseTree)
        {
            Assert.IsType(expectedParseTree.ExpectedType, actualParseTree);

            int expectedChildCount = expectedParseTree.ChildNodes.Count;
            Assert.Equal(expectedChildCount, actualParseTree.ChildCount);

            for (int i = 0; i < expectedChildCount; i++)
            {
                AssertParseTree(expectedParseTree.ChildNodes[i], actualParseTree.GetChild(i));
            }
        }

        public static IEnumerable<object[]> GetTestParseTrees()
            => TestParseTrees.Select(x => new object[] { x.Item1, x.Item2, Array.Empty<JsonErrorCode>() })
            .Union(TestParseTreesWithErrors.Select(x => new object[] { x.Item1, x.Item2, x.Item3 }));

        [Theory]
        [MemberData(nameof(GetTestParseTrees))]
        public void ParseTrees(string json, ParseTree parseTree, JsonErrorCode[] expectedErrors)
        {
            RootJsonSyntax rootSyntax = JsonParser.Parse(json);
            AssertParseTree(parseTree, rootSyntax.Syntax);

            // Assert expected errors.
            Assert.Collection(
                rootSyntax.Errors,
                Enumerable.Range(0, expectedErrors.Length)
                          .Select<int, Action<JsonErrorInfo>>(i => errorInfo => Assert.Equal(expectedErrors[i], errorInfo.ErrorCode))
                          .ToArray());
        }
    }
}

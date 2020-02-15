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
                var expectedTokens = JsonTokenizer.TokenizeAll(json);
                Action<IJsonSymbol>[] tokenInspectors = expectedTokens.Select<IGreenJsonSymbol, Action<IJsonSymbol>>((IGreenJsonSymbol expectedGreen) => (IJsonSymbol red) =>
                {
                    IGreenJsonSymbol actualGreen = TerminalSymbolTester.Instance.Visit(red);
                    Assert.IsType(expectedGreen.GetType(), actualGreen);
                    Assert.Equal(expectedGreen.Length, actualGreen.Length);
                    Assert.Equal(expectedGreen.Length, red.Length);
                }).ToArray();

                Assert.Collection(
                    JsonParser.Parse(json).Syntax.TerminalSymbolsInRange(0, json.Length),
                    tokenInspectors);
            }

            // Here Assert.Collection is used so if such a test fails,
            // it gives the index of the third token that was tested.
            Assert.Collection(
                JsonTokenizerTests.JsonTestSymbols(),
                Enumerable.Repeat<Action<(string, Type)>>(x0 =>
                {
                    string json = x0.Item1 + json1 + json2;
                    var expectedTokens = JsonTokenizer.TokenizeAll(json);
                    Action<IJsonSymbol>[] tokenInspectors = expectedTokens.Select<IGreenJsonSymbol, Action<IJsonSymbol>>(expectedGreen => symbol =>
                    {
                        IGreenJsonSymbol actualGreen = TerminalSymbolTester.Instance.Visit(symbol);
                        Assert.IsType(expectedGreen.GetType(), actualGreen);
                        Assert.Equal(expectedGreen.Length, actualGreen.Length);
                        Assert.Equal(expectedGreen.Length, symbol.Length);
                    }).ToArray();

                    Assert.Collection(
                        JsonParser.Parse(json).Syntax.TerminalSymbolsInRange(0, json.Length),
                        tokenInspectors);

                }, JsonTokenizerTests.JsonTestSymbols().Count()).ToArray());
        }

        private static readonly ParseTree Whitespace = new ParseTree<JsonWhitespaceSyntax>();
        private static readonly ParseTree Comment = new ParseTree<JsonCommentSyntax>();
        private static readonly ParseTree RootLevelValueDelimiter = new ParseTree<JsonRootLevelValueDelimiterSyntax>();

        private static readonly ParseTree NoBackground = new ParseTree<JsonBackgroundListSyntax>();
        private static readonly ParseTree WhitespaceBackground = new ParseTree<JsonBackgroundListSyntax> { Whitespace };

        private static readonly ParseTree NoValue = new ParseTree<JsonMissingValueSyntax>();

        private static readonly ParseTree NoValueOrBackground = new ParseTree<JsonValueWithBackgroundSyntax>
        {
            NoBackground,
            NoValue
        };

        private static readonly ParseTree NoValuesOrBackground = new ParseTree<JsonMultiValueSyntax>
        {
            NoValueOrBackground,
            NoBackground
        };

        private static readonly ParseTree CurlyOpen = new ParseTree<JsonCurlyOpenSyntax>();
        private static readonly ParseTree SquareBracketOpen = new ParseTree<JsonSquareBracketOpenSyntax>();

        private static readonly List<(string, ParseTree)> TestParseTrees = new List<(string, ParseTree)>
        {
            ("", new ParseTree<JsonMultiValueSyntax>
            {
                NoValueOrBackground,
                NoBackground
            }),

            (" ", new ParseTree<JsonMultiValueSyntax>
            {
                new ParseTree<JsonValueWithBackgroundSyntax>
                {
                    WhitespaceBackground,
                    NoValue
                },
                NoBackground
            }),

            ("//", new ParseTree<JsonMultiValueSyntax>
            {
                new ParseTree<JsonValueWithBackgroundSyntax>
                {
                    new ParseTree<JsonBackgroundListSyntax> { Comment },
                    NoValue
                },
                NoBackground
            }),

            ("true", new ParseTree<JsonMultiValueSyntax>
            {
                new ParseTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ParseTree<JsonBooleanLiteralSyntax.True>()
                },
                NoBackground
            }),

            ("0", new ParseTree<JsonMultiValueSyntax>
            {
                new ParseTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ParseTree<JsonIntegerLiteralSyntax>()
                },
                NoBackground
            }),

            ("\"\"", new ParseTree<JsonMultiValueSyntax>
            {
                new ParseTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ParseTree<JsonStringLiteralSyntax>()
                },
                NoBackground
            }),
        };

        private static readonly List<(string, ParseTree, JsonErrorCode[])> TestParseTreesWithErrors = new List<(string, ParseTree, JsonErrorCode[])>
        {
            ("/*", new ParseTree<JsonMultiValueSyntax>
            {
                new ParseTree<JsonValueWithBackgroundSyntax>
                {
                    new ParseTree<JsonBackgroundListSyntax> { new ParseTree<JsonUnterminatedMultiLineCommentSyntax>() },
                    NoValue
                },
                NoBackground
            },
            new[] { JsonErrorCode.UnterminatedMultiLineComment } ),

            ("\"", new ParseTree<JsonMultiValueSyntax>
            {
                new ParseTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ParseTree<JsonErrorStringSyntax>()
                },
                NoBackground
            },
            new[] { JsonErrorCode.UnterminatedString } ),

            ("_", new ParseTree<JsonMultiValueSyntax>
            {
                new ParseTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ParseTree<JsonUndefinedValueSyntax>()
                },
                NoBackground
            },
            new[] { JsonErrorCode.UnrecognizedValue } ),

            ("*", new ParseTree<JsonMultiValueSyntax>
            {
                new ParseTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ParseTree<JsonUnknownSymbolSyntax>()
                },
                NoBackground
            },
            new[] { JsonErrorCode.UnexpectedSymbol } ),

            (",", new ParseTree<JsonMultiValueSyntax>
            {
                new ParseTree<JsonValueWithBackgroundSyntax>
                {
                    new ParseTree<JsonBackgroundListSyntax> { RootLevelValueDelimiter },
                    NoValue
                },
                NoBackground
            },
            new[] { JsonErrorCode.ExpectedEof } ),

            (":", new ParseTree<JsonMultiValueSyntax>
            {
                new ParseTree<JsonValueWithBackgroundSyntax>
                {
                    new ParseTree<JsonBackgroundListSyntax> { RootLevelValueDelimiter },
                    NoValue
                },
                NoBackground
            },
            new[] { JsonErrorCode.ExpectedEof } ),

            ("[", new ParseTree<JsonMultiValueSyntax>
            {
                new ParseTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ParseTree<JsonListSyntax>
                    {
                        SquareBracketOpen,
                        NoValuesOrBackground
                    }
                },
                NoBackground
            },
            new[] { JsonErrorCode.UnexpectedEofInArray } ),

            ("]", new ParseTree<JsonMultiValueSyntax>
            {
                new ParseTree<JsonValueWithBackgroundSyntax>
                {
                    new ParseTree<JsonBackgroundListSyntax> { RootLevelValueDelimiter },
                    NoValue
                },
                NoBackground
            },
            new[] { JsonErrorCode.ExpectedEof } ),

            ("{", new ParseTree<JsonMultiValueSyntax>
            {
                new ParseTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ParseTree<JsonMapSyntax>
                    {
                        CurlyOpen,
                        new ParseTree<JsonKeyValueSyntax> { NoValuesOrBackground },
                    }
                },
                NoBackground
            },
            new[] { JsonErrorCode.UnexpectedEofInObject } ),

            ("}", new ParseTree<JsonMultiValueSyntax>
            {
                new ParseTree<JsonValueWithBackgroundSyntax>
                {
                    new ParseTree<JsonBackgroundListSyntax> { RootLevelValueDelimiter },
                    NoValue
                },
                NoBackground
            },
            new[] { JsonErrorCode.ExpectedEof } ),
        };

        private static int AssertParseTree(ParseTree expectedParseTree, JsonSyntax expectedParent, int expectedStart, JsonSyntax actualParseTree)
        {
            Assert.IsType(expectedParseTree.ExpectedType, actualParseTree);
            Assert.Same(expectedParent, actualParseTree.ParentSyntax);
            Assert.Equal(expectedStart, actualParseTree.Start);

            int expectedChildCount = expectedParseTree.ChildNodes.Count;
            Assert.Equal(expectedChildCount, actualParseTree.ChildCount);

            Assert.Throws<IndexOutOfRangeException>(() => actualParseTree.GetChild(-1));
            Assert.Throws<IndexOutOfRangeException>(() => actualParseTree.GetChild(expectedChildCount));
            Assert.Throws<IndexOutOfRangeException>(() => actualParseTree.GetChildStartPosition(-1));
            Assert.Throws<IndexOutOfRangeException>(() => actualParseTree.GetChildStartPosition(expectedChildCount));
            Assert.Throws<IndexOutOfRangeException>(() => actualParseTree.GetChildStartOrEndPosition(-1));
            Assert.Throws<IndexOutOfRangeException>(() => actualParseTree.GetChildStartOrEndPosition(expectedChildCount + 1));

            int length = 0;

            if (expectedChildCount == 0)
            {
                if (actualParseTree.Length > 0)
                {
                    Assert.True(actualParseTree.IsTerminalSymbol(out IJsonSymbol jsonSymbol));
                    length = jsonSymbol.Length;
                }
                else
                {
                    Assert.False(actualParseTree.IsTerminalSymbol(out _));
                }
            }
            else
            {
                Assert.False(actualParseTree.IsTerminalSymbol(out _));

                for (int i = 0; i < expectedChildCount; i++)
                {
                    Assert.Equal(length, actualParseTree.GetChildStartOrEndPosition(i));
                    length += AssertParseTree(expectedParseTree.ChildNodes[i], actualParseTree, length, actualParseTree.GetChild(i));
                }
            }

            Assert.Equal(length, actualParseTree.GetChildStartOrEndPosition(expectedChildCount));

            return length;
        }

        public static IEnumerable<object[]> GetTestParseTrees()
            => TestParseTrees.Select(x => new object[] { x.Item1, x.Item2, Array.Empty<JsonErrorCode>() })
            .Union(TestParseTreesWithErrors.Select(x => new object[] { x.Item1, x.Item2, x.Item3 }));

        [Theory]
        [MemberData(nameof(GetTestParseTrees))]
        public void ParseTrees(string json, ParseTree parseTree, JsonErrorCode[] expectedErrors)
        {
            RootJsonSyntax rootSyntax = JsonParser.Parse(json);
            AssertParseTree(parseTree, null, 0, rootSyntax.Syntax);

            // Assert expected errors.
            Assert.Collection(
                rootSyntax.Errors,
                Enumerable.Range(0, expectedErrors.Length)
                          .Select<int, Action<JsonErrorInfo>>(i => errorInfo => Assert.Equal(expectedErrors[i], errorInfo.ErrorCode))
                          .ToArray());
        }
    }
}

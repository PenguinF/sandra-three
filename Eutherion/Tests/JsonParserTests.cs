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

            public override IGreenJsonSymbol VisitBooleanLiteralSyntax(JsonBooleanLiteralSyntax node, _void arg) => node.Green;
            public override IGreenJsonSymbol VisitColonSyntax(JsonColonSyntax node, _void arg) => node.Green;
            public override IGreenJsonSymbol VisitCommaSyntax(JsonCommaSyntax node, _void arg) => node.Green;
            public override IGreenJsonSymbol VisitCommentSyntax(JsonCommentSyntax node, _void arg) => node.Green;
            public override IGreenJsonSymbol VisitCurlyCloseSyntax(JsonCurlyCloseSyntax node, _void arg) => node.Green;
            public override IGreenJsonSymbol VisitCurlyOpenSyntax(JsonCurlyOpenSyntax node, _void arg) => node.Green;
            public override IGreenJsonSymbol VisitErrorStringSyntax(JsonErrorStringSyntax node, _void arg) => node.Green;
            public override IGreenJsonSymbol VisitIntegerLiteralSyntax(JsonIntegerLiteralSyntax node, _void arg) => node.Green;
            public override IGreenJsonSymbol VisitRootLevelValueDelimiterSyntax(JsonRootLevelValueDelimiterSyntax node, _void arg) => node.Green.ValueDelimiter;
            public override IGreenJsonSymbol VisitSquareBracketCloseSyntax(JsonSquareBracketCloseSyntax node, _void arg) => node.Green;
            public override IGreenJsonSymbol VisitSquareBracketOpenSyntax(JsonSquareBracketOpenSyntax node, _void arg) => node.Green;
            public override IGreenJsonSymbol VisitStringLiteralSyntax(JsonStringLiteralSyntax node, _void arg) => node.Green;
            public override IGreenJsonSymbol VisitUndefinedValueSyntax(JsonUndefinedValueSyntax node, _void arg) => node.Green;
            public override IGreenJsonSymbol VisitUnknownSymbolSyntax(JsonUnknownSymbolSyntax node, _void arg) => node.Green;
            public override IGreenJsonSymbol VisitUnterminatedMultiLineCommentSyntax(JsonUnterminatedMultiLineCommentSyntax node, _void arg) => node.Green;
            public override IGreenJsonSymbol VisitWhitespaceSyntax(JsonWhitespaceSyntax node, _void arg) => node.Green;
        }

        public abstract class ExpectedJsonTree : IEnumerable<ExpectedJsonTree>
        {
            public abstract Type ExpectedType { get; }
            public readonly List<ExpectedJsonTree> ChildNodes = new List<ExpectedJsonTree>();

            // To enable collection initializer syntax:
            public IEnumerator<ExpectedJsonTree> GetEnumerator() => ChildNodes.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public void Add(ExpectedJsonTree child) => ChildNodes.Add(child);
        }

        public class ExpectedJsonTree<T> : ExpectedJsonTree where T : JsonSyntax
        {
            public override Type ExpectedType => typeof(T);
        }

        private enum JsonSymbolTypeClass { Background, ValueStarter, ValueDelimiter }

        private static void AssertJsonSymbolType(JsonSymbolType jsonSymbolType, JsonSymbolTypeClass expectedJsonSymbolTypeClass)
        {
            switch (expectedJsonSymbolTypeClass)
            {
                case JsonSymbolTypeClass.Background:
                    Assert.True(jsonSymbolType < JsonParser.ForegroundThreshold);
                    Assert.True(jsonSymbolType < JsonParser.ValueDelimiterThreshold);
                    break;
                case JsonSymbolTypeClass.ValueStarter:
                    Assert.False(jsonSymbolType < JsonParser.ForegroundThreshold);
                    Assert.True(jsonSymbolType < JsonParser.ValueDelimiterThreshold);
                    break;
                case JsonSymbolTypeClass.ValueDelimiter:
                default:
                    Assert.False(jsonSymbolType < JsonParser.ForegroundThreshold);
                    Assert.False(jsonSymbolType < JsonParser.ValueDelimiterThreshold);
                    break;
            }
        }

        /// <summary>
        /// Checks if assumptions by <see cref="JsonParser"/> still apply.
        /// </summary>
        [Fact]
        public void JsonSymbolTypeAssumptions()
        {
            new[]
            {
                JsonSymbolType.Whitespace,
                JsonSymbolType.Comment,
                JsonSymbolType.UnterminatedMultiLineComment,
            }
            .ForEach(x => AssertJsonSymbolType(x, JsonSymbolTypeClass.Background));

            new[]
            {
                JsonSymbolType.BooleanLiteral,
                JsonSymbolType.IntegerLiteral,
                JsonSymbolType.StringLiteral,
                JsonSymbolType.ErrorString,
                JsonSymbolType.UndefinedValue,
                JsonSymbolType.UnknownSymbol,
                JsonSymbolType.CurlyOpen,
                JsonSymbolType.BracketOpen,
            }
            .ForEach(x => AssertJsonSymbolType(x, JsonSymbolTypeClass.ValueStarter));

            new[]
            {
                JsonSymbolType.Colon,
                JsonSymbolType.Comma,
                JsonSymbolType.CurlyClose,
                JsonSymbolType.BracketClose,
                JsonSymbolType.Eof,
            }
            .ForEach(x => AssertJsonSymbolType(x, JsonSymbolTypeClass.ValueDelimiter));
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
                var expectedTokens = JsonParser.TokenizeAll(json).Item1;
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
                    var expectedTokens = JsonParser.TokenizeAll(json).Item1;
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

        private static readonly ExpectedJsonTree Whitespace = new ExpectedJsonTree<JsonWhitespaceSyntax>();
        private static readonly ExpectedJsonTree Comment = new ExpectedJsonTree<JsonCommentSyntax>();
        private static readonly ExpectedJsonTree RootLevelValueDelimiter = new ExpectedJsonTree<JsonRootLevelValueDelimiterSyntax>();

        private static readonly ExpectedJsonTree NoBackground = new ExpectedJsonTree<JsonBackgroundListSyntax>();
        private static readonly ExpectedJsonTree WhitespaceBackground = new ExpectedJsonTree<JsonBackgroundListSyntax> { Whitespace };

        private static readonly ExpectedJsonTree NoValue = new ExpectedJsonTree<JsonMissingValueSyntax>();

        private static readonly ExpectedJsonTree NoValueOrBackground = new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
        {
            NoBackground,
            NoValue
        };

        private static readonly ExpectedJsonTree NoValuesOrBackground = new ExpectedJsonTree<JsonMultiValueSyntax>
        {
            NoValueOrBackground,
            NoBackground
        };

        private static readonly ExpectedJsonTree Colon = new ExpectedJsonTree<JsonColonSyntax>();
        private static readonly ExpectedJsonTree Comma = new ExpectedJsonTree<JsonCommaSyntax>();
        private static readonly ExpectedJsonTree CurlyClose = new ExpectedJsonTree<JsonCurlyCloseSyntax>();
        private static readonly ExpectedJsonTree CurlyOpen = new ExpectedJsonTree<JsonCurlyOpenSyntax>();
        private static readonly ExpectedJsonTree SquareBracketClose = new ExpectedJsonTree<JsonSquareBracketCloseSyntax>();
        private static readonly ExpectedJsonTree SquareBracketOpen = new ExpectedJsonTree<JsonSquareBracketOpenSyntax>();

        private static readonly ExpectedJsonTree IntegerValue = new ExpectedJsonTree<JsonIntegerLiteralSyntax>();
        private static readonly ExpectedJsonTree StringValue = new ExpectedJsonTree<JsonStringLiteralSyntax>();

        private static readonly ExpectedJsonTree IntegerValueWithoutBackground = new ExpectedJsonTree<JsonMultiValueSyntax>
        {
            new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
            {
                NoBackground,
                IntegerValue
            },
            NoBackground
        };

        private static readonly ExpectedJsonTree StringValueWithoutBackground = new ExpectedJsonTree<JsonMultiValueSyntax>
        {
            new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
            {
                NoBackground,
                StringValue
            },
            NoBackground
        };

        private static readonly ExpectedJsonTree ErrorStringWithoutBackground = new ExpectedJsonTree<JsonMultiValueSyntax>
        {
            new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
            {
                NoBackground,
                new ExpectedJsonTree<JsonErrorStringSyntax>()
            },
            NoBackground
        };

        /// <summary>
        /// Expects an unterminated <see cref="JsonListSyntax"/>.
        /// </summary>
        private static readonly ExpectedJsonTree SquareBracketOpenWithoutBackground = new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
        {
            NoBackground,
            new ExpectedJsonTree<JsonListSyntax>
            {
                SquareBracketOpen,
                NoValuesOrBackground
            }
        };

        private static readonly ExpectedJsonTree EmptyListWithoutBackground = new ExpectedJsonTree<JsonMultiValueSyntax>
        {
            new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
            {
                NoBackground,
                new ExpectedJsonTree<JsonListSyntax>
                {
                    SquareBracketOpen,
                    NoValuesOrBackground,
                    SquareBracketClose
                }
            },
            NoBackground
        };

        private static readonly List<(string, ExpectedJsonTree)> TestParseTrees = new List<(string, ExpectedJsonTree)>
        {
            ("", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                NoValueOrBackground,
                NoBackground
            }),

            (" ", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    WhitespaceBackground,
                    NoValue
                },
                NoBackground
            }),

            ("//", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    new ExpectedJsonTree<JsonBackgroundListSyntax> { Comment },
                    NoValue
                },
                NoBackground
            }),

            ("true", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonBooleanLiteralSyntax.True>()
                },
                NoBackground
            }),

            ("0", IntegerValueWithoutBackground),

            ("\"\"", StringValueWithoutBackground),

            ("[]", EmptyListWithoutBackground),

            ("[0]", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonListSyntax>
                    {
                        SquareBracketOpen,
                        IntegerValueWithoutBackground,
                        SquareBracketClose
                    }
                },
                NoBackground
            }),

            ("[0,]", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonListSyntax>
                    {
                        SquareBracketOpen,
                        IntegerValueWithoutBackground,
                        Comma,
                        NoValuesOrBackground,
                        SquareBracketClose
                    }
                },
                NoBackground
            }),

            ("[0,1]", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonListSyntax>
                    {
                        SquareBracketOpen,
                        IntegerValueWithoutBackground,
                        Comma,
                        IntegerValueWithoutBackground,
                        SquareBracketClose
                    }
                },
                NoBackground
            }),

            ("{}", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonMapSyntax>
                    {
                        CurlyOpen,
                        new ExpectedJsonTree<JsonKeyValueSyntax> { NoValuesOrBackground },
                        CurlyClose
                    }
                },
                NoBackground
            }),

            ("{\"\":0}", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonMapSyntax>
                    {
                        CurlyOpen,
                        new ExpectedJsonTree<JsonKeyValueSyntax>
                        {
                            StringValueWithoutBackground,
                            Colon,
                            IntegerValueWithoutBackground
                        },
                        CurlyClose
                    }
                },
                NoBackground
            }),

            ("{\"\":0,}", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonMapSyntax>
                    {
                        CurlyOpen,
                        new ExpectedJsonTree<JsonKeyValueSyntax>
                        {
                            StringValueWithoutBackground,
                            Colon,
                            IntegerValueWithoutBackground
                        },
                        Comma,
                        new ExpectedJsonTree<JsonKeyValueSyntax> { NoValuesOrBackground },
                        CurlyClose
                    }
                },
                NoBackground
            }),

            ("{ \"a\" :0,\"b\":[]}", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonMapSyntax>
                    {
                        CurlyOpen,
                        new ExpectedJsonTree<JsonKeyValueSyntax>
                        {
                            new ExpectedJsonTree<JsonMultiValueSyntax>
                            {
                                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                                {
                                    WhitespaceBackground,
                                    StringValue
                                },
                                WhitespaceBackground
                            },
                            Colon,
                            IntegerValueWithoutBackground
                        },
                        Comma,
                        new ExpectedJsonTree<JsonKeyValueSyntax>
                        {
                            StringValueWithoutBackground,
                            Colon,
                            EmptyListWithoutBackground
                        },
                        CurlyClose
                    }
                },
                NoBackground
            }),
        };

        private static readonly List<(string, ExpectedJsonTree, JsonErrorCode[])> TestParseTreesWithErrors = new List<(string, ExpectedJsonTree, JsonErrorCode[])>
        {
            ("/*", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    new ExpectedJsonTree<JsonBackgroundListSyntax> { new ExpectedJsonTree<JsonUnterminatedMultiLineCommentSyntax>() },
                    NoValue
                },
                NoBackground
            },
            new[] { JsonErrorCode.UnterminatedMultiLineComment } ),

            ("\"", ErrorStringWithoutBackground, new[] { JsonErrorCode.UnterminatedString } ),

            ("_", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonUndefinedValueSyntax>()
                },
                NoBackground
            },
            new[] { JsonErrorCode.UnrecognizedValue } ),

            ("*", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonUnknownSymbolSyntax>()
                },
                NoBackground
            },
            new[] { JsonErrorCode.UnexpectedSymbol } ),

            (",", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    new ExpectedJsonTree<JsonBackgroundListSyntax> { RootLevelValueDelimiter },
                    NoValue
                },
                NoBackground
            },
            new[] { JsonErrorCode.ExpectedEof } ),

            (":", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    new ExpectedJsonTree<JsonBackgroundListSyntax> { RootLevelValueDelimiter },
                    NoValue
                },
                NoBackground
            },
            new[] { JsonErrorCode.ExpectedEof } ),

            ("[", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                SquareBracketOpenWithoutBackground,
                NoBackground
            },
            new[] { JsonErrorCode.UnexpectedEofInArray } ),

            ("]", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    new ExpectedJsonTree<JsonBackgroundListSyntax> { RootLevelValueDelimiter },
                    NoValue
                },
                NoBackground
            },
            new[] { JsonErrorCode.ExpectedEof } ),

            ("{", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonMapSyntax>
                    {
                        CurlyOpen,
                        new ExpectedJsonTree<JsonKeyValueSyntax> { NoValuesOrBackground },
                    }
                },
                NoBackground
            },
            new[] { JsonErrorCode.UnexpectedEofInObject } ),

            ("}", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    new ExpectedJsonTree<JsonBackgroundListSyntax> { RootLevelValueDelimiter },
                    NoValue
                },
                NoBackground
            },
            new[] { JsonErrorCode.ExpectedEof } ),

            ("//\n]", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    new ExpectedJsonTree<JsonBackgroundListSyntax> { Comment, Whitespace, RootLevelValueDelimiter },
                    NoValue
                },
                NoBackground
            },
            new[] { JsonErrorCode.ExpectedEof } ),

            (" -1 //\nfalse ", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    WhitespaceBackground,
                    IntegerValue
                },
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    new ExpectedJsonTree<JsonBackgroundListSyntax>
                    {
                        Whitespace,
                        Comment,
                        Whitespace
                    },
                    new ExpectedJsonTree<JsonBooleanLiteralSyntax.False>()
                },
                WhitespaceBackground
            },
            new[] { JsonErrorCode.ExpectedEof } ),

            (",,", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    new ExpectedJsonTree<JsonBackgroundListSyntax> { RootLevelValueDelimiter, RootLevelValueDelimiter },
                    NoValue
                },
                NoBackground
            },
            new[] { JsonErrorCode.ExpectedEof, JsonErrorCode.ExpectedEof } ),

            ("0,,", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    IntegerValue
                },
                new ExpectedJsonTree<JsonBackgroundListSyntax> { RootLevelValueDelimiter, RootLevelValueDelimiter }
            },
            new[] { JsonErrorCode.ExpectedEof, JsonErrorCode.ExpectedEof } ),

            (",0,", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    new ExpectedJsonTree<JsonBackgroundListSyntax> { RootLevelValueDelimiter },
                    IntegerValue
                },
                new ExpectedJsonTree<JsonBackgroundListSyntax> { RootLevelValueDelimiter }
            },
            new[] { JsonErrorCode.ExpectedEof, JsonErrorCode.ExpectedEof } ),

            (",,0", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    new ExpectedJsonTree<JsonBackgroundListSyntax> { RootLevelValueDelimiter, RootLevelValueDelimiter },
                    IntegerValue
                },
                NoBackground
            },
            new[] { JsonErrorCode.ExpectedEof, JsonErrorCode.ExpectedEof } ),

            ("[,]", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonListSyntax>
                    {
                        SquareBracketOpen,
                        NoValuesOrBackground,
                        Comma,
                        NoValuesOrBackground,
                        SquareBracketClose
                    }
                },
                NoBackground
            },
            new[] { JsonErrorCode.MissingValue } ),

            ("[,0]", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonListSyntax>
                    {
                        SquareBracketOpen,
                        NoValuesOrBackground,
                        Comma,
                        IntegerValueWithoutBackground,
                        SquareBracketClose
                    }
                },
                NoBackground
            },
            new[] { JsonErrorCode.MissingValue } ),

            ("[0 ", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonListSyntax>
                    {
                        SquareBracketOpen,
                        new ExpectedJsonTree<JsonMultiValueSyntax>
                        {
                            new ExpectedJsonTree<JsonValueWithBackgroundSyntax> { NoBackground, IntegerValue },
                            WhitespaceBackground
                        }
                    }
                },
                NoBackground
            },
            new[] { JsonErrorCode.UnexpectedEofInArray } ),

            (" [ 0  0 ] ", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    WhitespaceBackground,
                    new ExpectedJsonTree<JsonListSyntax>
                    {
                        SquareBracketOpen,
                        new ExpectedJsonTree<JsonMultiValueSyntax>
                        {
                            new ExpectedJsonTree<JsonValueWithBackgroundSyntax> { WhitespaceBackground, IntegerValue },
                            new ExpectedJsonTree<JsonValueWithBackgroundSyntax> { WhitespaceBackground, IntegerValue },
                            WhitespaceBackground
                        },
                        SquareBracketClose
                    }
                },
                WhitespaceBackground
            },
            new[] { JsonErrorCode.MultipleValues } ),

            ("[[]", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonListSyntax>
                    {
                        SquareBracketOpen,
                        new ExpectedJsonTree<JsonMultiValueSyntax>
                        {
                            new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                            {
                                NoBackground,
                                new ExpectedJsonTree<JsonListSyntax>
                                {
                                    SquareBracketOpen,
                                    NoValuesOrBackground,
                                    SquareBracketClose
                                }
                            },
                            NoBackground
                        }
                    }
                },
                NoBackground
            },
            new[] { JsonErrorCode.UnexpectedEofInArray } ),

            ("[]]", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonListSyntax>
                    {
                        SquareBracketOpen,
                        NoValuesOrBackground,
                        SquareBracketClose
                    }
                },
                new ExpectedJsonTree<JsonBackgroundListSyntax> { RootLevelValueDelimiter }
            },
            new[] { JsonErrorCode.ExpectedEof } ),

            ("[:]", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                SquareBracketOpenWithoutBackground,
                new ExpectedJsonTree<JsonBackgroundListSyntax> { RootLevelValueDelimiter, RootLevelValueDelimiter }
            },
            new[] { JsonErrorCode.ControlSymbolInArray, JsonErrorCode.ExpectedEof, JsonErrorCode.ExpectedEof } ),

            ("[{]", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonListSyntax>
                    {
                        SquareBracketOpen,
                        new ExpectedJsonTree<JsonMultiValueSyntax>
                        {
                            new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                            {
                                NoBackground,
                                new ExpectedJsonTree<JsonMapSyntax>
                                {
                                    CurlyOpen,
                                    new ExpectedJsonTree<JsonKeyValueSyntax> { NoValuesOrBackground }
                                }
                            },
                            NoBackground
                        },
                        SquareBracketClose
                    }
                },
                NoBackground
            },
            new[] { JsonErrorCode.ControlSymbolInObject } ),

            ("[}]", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                SquareBracketOpenWithoutBackground,
                new ExpectedJsonTree<JsonBackgroundListSyntax> { RootLevelValueDelimiter, RootLevelValueDelimiter }
            },
            new[] { JsonErrorCode.ControlSymbolInArray, JsonErrorCode.ExpectedEof, JsonErrorCode.ExpectedEof } ),

            ("[{]}", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonListSyntax>
                    {
                        SquareBracketOpen,
                        new ExpectedJsonTree<JsonMultiValueSyntax>
                        {
                            new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                            {
                                NoBackground,
                                new ExpectedJsonTree<JsonMapSyntax>
                                {
                                    CurlyOpen,
                                    new ExpectedJsonTree<JsonKeyValueSyntax> { NoValuesOrBackground }
                                }
                            },
                            NoBackground
                        },
                        SquareBracketClose
                    }
                },
                new ExpectedJsonTree<JsonBackgroundListSyntax> { RootLevelValueDelimiter }
            },
            new[] { JsonErrorCode.ControlSymbolInObject, JsonErrorCode.ExpectedEof } ),

            ("{0 ", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonMapSyntax>
                    {
                        CurlyOpen,
                        new ExpectedJsonTree<JsonKeyValueSyntax>
                        {
                            new ExpectedJsonTree<JsonMultiValueSyntax>
                            {
                                new ExpectedJsonTree<JsonValueWithBackgroundSyntax> { NoBackground, IntegerValue },
                                WhitespaceBackground
                            }
                        }
                    }
                },
                NoBackground
            },
            new[] { JsonErrorCode.InvalidPropertyKey, JsonErrorCode.UnexpectedEofInObject } ),

            ("{0}", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonMapSyntax>
                    {
                        CurlyOpen,
                        new ExpectedJsonTree<JsonKeyValueSyntax> { IntegerValueWithoutBackground },
                        CurlyClose
                    }
                },
                NoBackground
            },
            new[] { JsonErrorCode.InvalidPropertyKey, JsonErrorCode.MissingValue } ),

            ("{\"\" 0}", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonMapSyntax>
                    {
                        CurlyOpen,
                        new ExpectedJsonTree<JsonKeyValueSyntax>
                        {
                            new ExpectedJsonTree<JsonMultiValueSyntax>
                            {
                                new ExpectedJsonTree<JsonValueWithBackgroundSyntax> { NoBackground, StringValue },
                                new ExpectedJsonTree<JsonValueWithBackgroundSyntax> { WhitespaceBackground, IntegerValue },
                                NoBackground
                            }
                        },
                        CurlyClose
                    }
                },
                NoBackground
            },
            new[] { JsonErrorCode.MultiplePropertyKeys, JsonErrorCode.MissingValue } ),

            ("{0 \"\"}", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonMapSyntax>
                    {
                        CurlyOpen,
                        new ExpectedJsonTree<JsonKeyValueSyntax>
                        {
                            new ExpectedJsonTree<JsonMultiValueSyntax>
                            {
                                new ExpectedJsonTree<JsonValueWithBackgroundSyntax> { NoBackground, IntegerValue },
                                new ExpectedJsonTree<JsonValueWithBackgroundSyntax> { WhitespaceBackground, StringValue },
                                NoBackground
                            }
                        },
                        CurlyClose
                    }
                },
                NoBackground
            },
            new[] { JsonErrorCode.MultiplePropertyKeys, JsonErrorCode.InvalidPropertyKey, JsonErrorCode.MissingValue } ),

            ("{:}", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonMapSyntax>
                    {
                        CurlyOpen,
                        new ExpectedJsonTree<JsonKeyValueSyntax>
                        {
                            NoValuesOrBackground,
                            Colon,
                            NoValuesOrBackground
                        },
                        CurlyClose
                    }
                },
                NoBackground
            },
            new[] { JsonErrorCode.MissingPropertyKey, JsonErrorCode.MissingValue } ),

            ("{::}", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonMapSyntax>
                    {
                        CurlyOpen,
                        new ExpectedJsonTree<JsonKeyValueSyntax>
                        {
                            NoValuesOrBackground,
                            Colon,
                            NoValuesOrBackground,
                            Colon,
                            NoValuesOrBackground
                        },
                        CurlyClose
                    }
                },
                NoBackground
            },
            new[] { JsonErrorCode.MultiplePropertyKeySections, JsonErrorCode.MissingPropertyKey, JsonErrorCode.MissingValue } ),

            ("{[:[}", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonMapSyntax>
                    {
                        CurlyOpen,
                        new ExpectedJsonTree<JsonKeyValueSyntax>
                        {
                            new ExpectedJsonTree<JsonMultiValueSyntax> { SquareBracketOpenWithoutBackground, NoBackground },
                            Colon,
                            new ExpectedJsonTree<JsonMultiValueSyntax> { SquareBracketOpenWithoutBackground, NoBackground }
                        },
                        CurlyClose
                    }
                },
                NoBackground
            },
            new[] { JsonErrorCode.ControlSymbolInArray, JsonErrorCode.InvalidPropertyKey, JsonErrorCode.ControlSymbolInArray } ),

            ("{,}", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonMapSyntax>
                    {
                        CurlyOpen,
                        new ExpectedJsonTree<JsonKeyValueSyntax> { NoValuesOrBackground },
                        Comma,
                        new ExpectedJsonTree<JsonKeyValueSyntax> { NoValuesOrBackground },
                        CurlyClose
                    }
                },
                NoBackground
            },
            new[] { JsonErrorCode.MissingPropertyKey, JsonErrorCode.MissingValue } ),

            ("{[,[}", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonMapSyntax>
                    {
                        CurlyOpen,
                        new ExpectedJsonTree<JsonKeyValueSyntax>
                        {
                            new ExpectedJsonTree<JsonMultiValueSyntax>
                            {
                                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                                {
                                    NoBackground,
                                    new ExpectedJsonTree<JsonListSyntax>
                                    {
                                        SquareBracketOpen,
                                        NoValuesOrBackground,
                                        Comma,
                                        new ExpectedJsonTree<JsonMultiValueSyntax> { SquareBracketOpenWithoutBackground, NoBackground }
                                    }
                                },
                                NoBackground
                            }
                        },
                        CurlyClose
                    }
                },
                NoBackground
            },
            new[]
            {
                JsonErrorCode.MissingValue,          // From the missing value before the ','.
                JsonErrorCode.ControlSymbolInArray,  // From seeing the '}' in the inner array.
                JsonErrorCode.ControlSymbolInArray,  // From seeing the '}' in the outer array.
                JsonErrorCode.InvalidPropertyKey,    // An array cannot be a property key.
                JsonErrorCode.MissingValue           // Missing value for the intended property key.
            }),

            ("{\"\":,\"", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonMapSyntax>
                    {
                        CurlyOpen,
                        new ExpectedJsonTree<JsonKeyValueSyntax>
                        {
                            StringValueWithoutBackground,
                            Colon,
                            NoValuesOrBackground
                        },
                        Comma,
                        new ExpectedJsonTree<JsonKeyValueSyntax> { ErrorStringWithoutBackground }
                    }
                },
                NoBackground
            },
            new[] { JsonErrorCode.MissingValue, JsonErrorCode.UnterminatedString, JsonErrorCode.InvalidPropertyKey, JsonErrorCode.UnexpectedEofInObject } ),

            ("{\"\":,\"\"", new ExpectedJsonTree<JsonMultiValueSyntax>
            {
                new ExpectedJsonTree<JsonValueWithBackgroundSyntax>
                {
                    NoBackground,
                    new ExpectedJsonTree<JsonMapSyntax>
                    {
                        CurlyOpen,
                        new ExpectedJsonTree<JsonKeyValueSyntax>
                        {
                            StringValueWithoutBackground,
                            Colon,
                            NoValuesOrBackground
                        },
                        Comma,
                        new ExpectedJsonTree<JsonKeyValueSyntax> { StringValueWithoutBackground }
                    }
                },
                NoBackground
            },
            new[] { JsonErrorCode.MissingValue, JsonErrorCode.PropertyKeyAlreadyExists, JsonErrorCode.UnexpectedEofInObject } ),
        };

        private static int AssertParseTree(ExpectedJsonTree expectedParseTree, JsonSyntax expectedParent, int expectedStart, JsonSyntax actualParseTree)
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
        public void ParseTrees(string json, ExpectedJsonTree parseTree, JsonErrorCode[] expectedErrors)
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

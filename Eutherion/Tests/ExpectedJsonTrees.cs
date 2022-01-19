#region License
/*********************************************************************************
 * ExpectedJsonTrees.cs
 *
 * Copyright (c) 2004-2022 Henk Nicolai
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
using System.Collections.Generic;

namespace Eutherion.Shared.Tests
{
    /// <summary>
    /// Contains definitions for expected parse trees.
    /// </summary>
    public static class ExpectedJsonTrees
    {
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

        public static readonly List<(string, ExpectedJsonTree)> TestParseTrees = new List<(string, ExpectedJsonTree)>
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

        public static readonly List<(string, ExpectedJsonTree, JsonErrorCode[])> TestParseTreesWithErrors = new List<(string, ExpectedJsonTree, JsonErrorCode[])>
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
    }
}

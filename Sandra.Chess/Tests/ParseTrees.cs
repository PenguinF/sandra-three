#region License
/*********************************************************************************
 * ParseTrees.cs
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

using Sandra.Chess.Pgn;
using Sandra.Chess.Pgn.Temp;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sandra.Chess.Tests
{
    public static class ParseTrees
    {
        public abstract class ParseTree : IEnumerable<ParseTree>
        {
            public abstract Type ExpectedType { get; }
            public readonly List<ParseTree> ChildNodes = new List<ParseTree>();

            // To enable collection initializer syntax:
            public IEnumerator<ParseTree> GetEnumerator() => ChildNodes.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public void Add(ParseTree child) => ChildNodes.Add(child);
        }

        public class ParseTree<T> : ParseTree where T : PgnSyntax
        {
            public override Type ExpectedType => typeof(T);
        }

        private static readonly ParseTree WhitespaceElement = new ParseTree<PgnWhitespaceSyntax>();
        private static readonly ParseTree IllegalCharacter = new ParseTree<PgnIllegalCharacterSyntax>();
        private static readonly ParseTree EscapedLine = new ParseTree<PgnEscapeSyntax>();

        private static readonly ParseTree EmptyBackground = new ParseTree<PgnBackgroundListSyntax>();
        private static readonly ParseTree Whitespace = new ParseTree<PgnBackgroundListSyntax> { WhitespaceElement };

        private static readonly ParseTree Symbol = new ParseTree<PgnSymbol>();

        internal static readonly List<(string, ParseTree)> TestParseTrees = new List<(string, ParseTree)>
        {
            ("", new ParseTree<PgnSyntaxNodes> { EmptyBackground }),
            (" ", new ParseTree<PgnSyntaxNodes> { Whitespace }),
            ("%", new ParseTree<PgnSyntaxNodes> { new ParseTree<PgnBackgroundListSyntax> { EscapedLine } }),
            ("% ", new ParseTree<PgnSyntaxNodes> { new ParseTree<PgnBackgroundListSyntax> { EscapedLine } }),
            ("% \n", new ParseTree<PgnSyntaxNodes> { new ParseTree<PgnBackgroundListSyntax> { EscapedLine, WhitespaceElement } }),
            ("\n%", new ParseTree<PgnSyntaxNodes> { new ParseTree<PgnBackgroundListSyntax> { WhitespaceElement, EscapedLine } }),
        };

        internal static readonly List<(string, ParseTree, PgnErrorCode[])> TestParseTreesWithErrors = new List<(string, ParseTree, PgnErrorCode[])>
        {
            (" %", new ParseTree<PgnSyntaxNodes> { new ParseTree<PgnBackgroundListSyntax> { WhitespaceElement, IllegalCharacter } },
                new[] { PgnErrorCode.IllegalCharacter }),
            (" % ", new ParseTree<PgnSyntaxNodes> { new ParseTree<PgnBackgroundListSyntax> { WhitespaceElement, IllegalCharacter, WhitespaceElement } },
                new[] { PgnErrorCode.IllegalCharacter }),

            ("\"", new ParseTree<PgnSyntaxNodes> { EmptyBackground, Symbol, EmptyBackground },
                new[] { PgnErrorCode.UnterminatedTagValue }),
            ("\"\n", new ParseTree<PgnSyntaxNodes> { EmptyBackground, Symbol, EmptyBackground },
                new[] { PgnErrorCode.IllegalControlCharacterInTagValue, PgnErrorCode.UnterminatedTagValue }),

            ("{", new ParseTree<PgnSyntaxNodes> { EmptyBackground, Symbol, EmptyBackground }, new[] { PgnErrorCode.UnterminatedMultiLineComment }),
            (" {", new ParseTree<PgnSyntaxNodes> { Whitespace, Symbol, EmptyBackground }, new[] { PgnErrorCode.UnterminatedMultiLineComment }),
            ("{ ", new ParseTree<PgnSyntaxNodes> { EmptyBackground, Symbol, EmptyBackground }, new[] { PgnErrorCode.UnterminatedMultiLineComment }),
            (" { ", new ParseTree<PgnSyntaxNodes> { Whitespace, Symbol, EmptyBackground }, new[] { PgnErrorCode.UnterminatedMultiLineComment }),
        };
    }
}

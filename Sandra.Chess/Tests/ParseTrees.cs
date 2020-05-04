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

        private static readonly ParseTree<PgnWhitespaceSyntax> WhitespaceElement = new ParseTree<PgnWhitespaceSyntax>();
        private static readonly ParseTree<PgnIllegalCharacterSyntax> IllegalCharacter = new ParseTree<PgnIllegalCharacterSyntax>();
        private static readonly ParseTree<PgnEscapeSyntax> EscapedLine = new ParseTree<PgnEscapeSyntax>();

        private static readonly ParseTree<PgnBackgroundListSyntax> EmptyBackground = new ParseTree<PgnBackgroundListSyntax>();
        private static readonly ParseTree<PgnTriviaSyntax> EmptyTrivia = new ParseTree<PgnTriviaSyntax> { EmptyBackground };
        private static readonly ParseTree<PgnBackgroundListSyntax> Whitespace = new ParseTree<PgnBackgroundListSyntax> { WhitespaceElement };
        private static readonly ParseTree<PgnTriviaSyntax> WhitespaceTrivia = new ParseTree<PgnTriviaSyntax> { Whitespace };
        private static readonly ParseTree<PgnBackgroundListSyntax> TwoIllegalCharacters = new ParseTree<PgnBackgroundListSyntax> { IllegalCharacter, IllegalCharacter };

        private static readonly ParseTree<PgnCommentSyntax> Comment = new ParseTree<PgnCommentSyntax>();
        private static readonly ParseTree<PgnSymbol> Symbol = new ParseTree<PgnSymbol>();

        private static ParseTree<PgnSymbolWithTrivia> PgnSymbolWithLeadingTrivia(ParseTree<PgnTriviaSyntax> leadingTrivia)
            => new ParseTree<PgnSymbolWithTrivia> { leadingTrivia, Symbol };

        private static ParseTree<PgnSymbolWithTrivia> PgnSymbolWithLeadingTrivia(
            ParseTree<PgnTriviaElementSyntax> element1,
            ParseTree<PgnBackgroundListSyntax> backgroundAfter)
            => PgnSymbolWithLeadingTrivia(new ParseTree<PgnTriviaSyntax> { element1, backgroundAfter });

        private static ParseTree<PgnSymbolWithTrivia> PgnSymbolWithLeadingTrivia(
            ParseTree<PgnTriviaElementSyntax> element1,
            ParseTree<PgnTriviaElementSyntax> element2,
            ParseTree<PgnBackgroundListSyntax> backgroundAfter)
            => PgnSymbolWithLeadingTrivia(new ParseTree<PgnTriviaSyntax> { element1, element2, backgroundAfter });

        private static readonly ParseTree<PgnTriviaElementSyntax> CommentNoBackground = new ParseTree<PgnTriviaElementSyntax> { EmptyBackground, Comment };
        private static readonly ParseTree<PgnSymbolWithTrivia> SymbolNoTrivia = PgnSymbolWithLeadingTrivia(EmptyTrivia);
        private static readonly ParseTree<PgnTriviaElementSyntax> WhitespaceThenComment = new ParseTree<PgnTriviaElementSyntax> { Whitespace, Comment };
        private static readonly ParseTree<PgnSymbolWithTrivia> WhitespaceTriviaThenSymbol = PgnSymbolWithLeadingTrivia(WhitespaceTrivia);

        private static readonly ParseTree<PgnTriviaSyntax> CommentTrivia = new ParseTree<PgnTriviaSyntax> { CommentNoBackground, EmptyBackground };
        private static readonly ParseTree<PgnTriviaSyntax> WhitespaceThenCommentTrivia = new ParseTree<PgnTriviaSyntax> { WhitespaceThenComment, EmptyBackground };
        private static readonly ParseTree<PgnTriviaSyntax> WhitespaceCommentWhitespace = new ParseTree<PgnTriviaSyntax> { WhitespaceThenComment, Whitespace };

        internal static readonly List<(string, ParseTree)> TestParseTrees = new List<(string, ParseTree)>
        {
            #region Background

            ("", new ParseTree<PgnSyntaxNodes> { EmptyTrivia }),
            (" ", new ParseTree<PgnSyntaxNodes> { WhitespaceTrivia }),
            ("%", new ParseTree<PgnSyntaxNodes> { new ParseTree<PgnTriviaSyntax> { new ParseTree<PgnBackgroundListSyntax> { EscapedLine } } }),
            ("% ", new ParseTree<PgnSyntaxNodes> { new ParseTree<PgnTriviaSyntax> { new ParseTree<PgnBackgroundListSyntax> { EscapedLine } } }),
            ("% \n", new ParseTree<PgnSyntaxNodes> { new ParseTree<PgnTriviaSyntax> { new ParseTree<PgnBackgroundListSyntax> { EscapedLine, WhitespaceElement } } }),
            ("\n%", new ParseTree<PgnSyntaxNodes> { new ParseTree<PgnTriviaSyntax> { new ParseTree<PgnBackgroundListSyntax> { WhitespaceElement, EscapedLine } } }),

            #endregion Background

            #region Combinations of trivia and symbols

            ("0 0", new ParseTree<PgnSyntaxNodes> { SymbolNoTrivia, WhitespaceTriviaThenSymbol, EmptyTrivia }),
            (" 0  00   000    0 ", new ParseTree<PgnSyntaxNodes> { WhitespaceTriviaThenSymbol, WhitespaceTriviaThenSymbol, WhitespaceTriviaThenSymbol, WhitespaceTriviaThenSymbol, WhitespaceTrivia }),

            ("{}", new ParseTree<PgnSyntaxNodes> { CommentTrivia }),
            ("  {}   * ", new ParseTree<PgnSyntaxNodes> { PgnSymbolWithLeadingTrivia(WhitespaceThenComment, Whitespace), WhitespaceTrivia }),
            (" *   {}  ", new ParseTree<PgnSyntaxNodes> { WhitespaceTriviaThenSymbol, WhitespaceCommentWhitespace }),
            (" {} {} ", new ParseTree<PgnSyntaxNodes> { new ParseTree<PgnTriviaSyntax> { WhitespaceThenComment, WhitespaceThenComment, Whitespace } }),

            (" * {} {} ", new ParseTree<PgnSyntaxNodes> { WhitespaceTriviaThenSymbol, new ParseTree<PgnTriviaSyntax> { WhitespaceThenComment, WhitespaceThenComment, Whitespace } }),
            (" {} * {} ", new ParseTree<PgnSyntaxNodes> { PgnSymbolWithLeadingTrivia(WhitespaceCommentWhitespace), WhitespaceCommentWhitespace }),
            (" {} {} * ", new ParseTree<PgnSyntaxNodes> { PgnSymbolWithLeadingTrivia(WhitespaceThenComment, WhitespaceThenComment, Whitespace), WhitespaceTrivia }),

            (" {} * * ", new ParseTree<PgnSyntaxNodes> { PgnSymbolWithLeadingTrivia(WhitespaceThenComment, Whitespace), WhitespaceTriviaThenSymbol, WhitespaceTrivia }),
            (" * {} * ", new ParseTree<PgnSyntaxNodes> { WhitespaceTriviaThenSymbol, PgnSymbolWithLeadingTrivia(WhitespaceThenComment, Whitespace), WhitespaceTrivia }),
            (" * * {} ", new ParseTree<PgnSyntaxNodes> { WhitespaceTriviaThenSymbol, WhitespaceTriviaThenSymbol, WhitespaceCommentWhitespace }),

            ("{}{}*{}", new ParseTree<PgnSyntaxNodes> { PgnSymbolWithLeadingTrivia(CommentNoBackground, CommentNoBackground, EmptyBackground), CommentTrivia }),
            ("{}{}**", new ParseTree<PgnSyntaxNodes> { PgnSymbolWithLeadingTrivia(CommentNoBackground, CommentNoBackground, EmptyBackground), SymbolNoTrivia, EmptyTrivia }),
            ("{}*{}{}", new ParseTree<PgnSyntaxNodes> { PgnSymbolWithLeadingTrivia(CommentNoBackground, EmptyBackground), new ParseTree<PgnTriviaSyntax> { CommentNoBackground, CommentNoBackground, EmptyBackground } }),
            ("{}*{}*", new ParseTree<PgnSyntaxNodes> { PgnSymbolWithLeadingTrivia(CommentNoBackground, EmptyBackground), PgnSymbolWithLeadingTrivia(CommentNoBackground, EmptyBackground), EmptyTrivia }),
            ("{}**{}", new ParseTree<PgnSyntaxNodes> { PgnSymbolWithLeadingTrivia(CommentNoBackground, EmptyBackground), SymbolNoTrivia, CommentTrivia }),
            ("*{}{}*", new ParseTree<PgnSyntaxNodes> { SymbolNoTrivia, PgnSymbolWithLeadingTrivia(CommentNoBackground, CommentNoBackground, EmptyBackground), EmptyTrivia }),
            ("*{}*{}", new ParseTree<PgnSyntaxNodes> { SymbolNoTrivia, PgnSymbolWithLeadingTrivia(CommentNoBackground, EmptyBackground), CommentTrivia }),
            ("*{}**", new ParseTree<PgnSyntaxNodes> { SymbolNoTrivia, PgnSymbolWithLeadingTrivia(CommentNoBackground, EmptyBackground), SymbolNoTrivia, EmptyTrivia }),
            ("**{}{}", new ParseTree<PgnSyntaxNodes> { SymbolNoTrivia, SymbolNoTrivia, new ParseTree<PgnTriviaSyntax> { CommentNoBackground, CommentNoBackground, EmptyBackground } }),
            ("**{}*", new ParseTree<PgnSyntaxNodes> { SymbolNoTrivia, SymbolNoTrivia, PgnSymbolWithLeadingTrivia(CommentNoBackground, EmptyBackground), EmptyTrivia }),

            #endregion Combinations of trivia and symbols
        };

        internal static readonly List<(string, ParseTree, PgnErrorCode[])> TestParseTreesWithErrors = new List<(string, ParseTree, PgnErrorCode[])>
        {
            #region Background and trivia errors

            (" %", new ParseTree<PgnSyntaxNodes> { new ParseTree<PgnTriviaSyntax> { new ParseTree<PgnBackgroundListSyntax> { WhitespaceElement, IllegalCharacter } } },
                new[] { PgnErrorCode.IllegalCharacter }),
            (" % ", new ParseTree<PgnSyntaxNodes> { new ParseTree<PgnTriviaSyntax> { new ParseTree<PgnBackgroundListSyntax> { WhitespaceElement, IllegalCharacter, WhitespaceElement } } },
                new[] { PgnErrorCode.IllegalCharacter }),

            ("\"", new ParseTree<PgnSyntaxNodes> { SymbolNoTrivia, EmptyTrivia },
                new[] { PgnErrorCode.UnterminatedTagValue }),
            ("\"\n", new ParseTree<PgnSyntaxNodes> { SymbolNoTrivia, EmptyTrivia },
                new[] { PgnErrorCode.IllegalControlCharacterInTagValue, PgnErrorCode.UnterminatedTagValue }),

            ("{", new ParseTree<PgnSyntaxNodes> { CommentTrivia }, new[] { PgnErrorCode.UnterminatedMultiLineComment }),
            (" {", new ParseTree<PgnSyntaxNodes> { WhitespaceThenCommentTrivia }, new[] { PgnErrorCode.UnterminatedMultiLineComment }),
            ("{ ", new ParseTree<PgnSyntaxNodes> { CommentTrivia }, new[] { PgnErrorCode.UnterminatedMultiLineComment }),
            (" { ", new ParseTree<PgnSyntaxNodes> { WhitespaceThenCommentTrivia }, new[] { PgnErrorCode.UnterminatedMultiLineComment }),

            ("0%%0", new ParseTree<PgnSyntaxNodes> { SymbolNoTrivia, PgnSymbolWithLeadingTrivia(new ParseTree<PgnTriviaSyntax> { TwoIllegalCharacters }), EmptyTrivia },
                new[] { PgnErrorCode.IllegalCharacter, PgnErrorCode.IllegalCharacter }),

            #endregion Background and trivia errors
        };
    }
}

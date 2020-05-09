#region License
/*********************************************************************************
 * ParseTrees.Trivia.cs
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
using System.Collections.Generic;

namespace Sandra.Chess.Tests
{
    public static partial class ParseTrees
    {
        internal static List<(string, ParseTree)> TriviaParseTrees() => new List<(string, ParseTree)>
        {
            ("", new ParseTree<PgnSyntaxNodes> { EmptyTrivia }),
            (" ", new ParseTree<PgnSyntaxNodes> { WhitespaceTrivia }),
            ("%", new ParseTree<PgnSyntaxNodes> { new ParseTree<PgnTriviaSyntax> { new ParseTree<PgnBackgroundListSyntax> { EscapedLine } } }),
            ("% ", new ParseTree<PgnSyntaxNodes> { new ParseTree<PgnTriviaSyntax> { new ParseTree<PgnBackgroundListSyntax> { EscapedLine } } }),
            ("% \n", new ParseTree<PgnSyntaxNodes> { new ParseTree<PgnTriviaSyntax> { new ParseTree<PgnBackgroundListSyntax> { EscapedLine, WhitespaceElement } } }),
            ("\n%", new ParseTree<PgnSyntaxNodes> { new ParseTree<PgnTriviaSyntax> { new ParseTree<PgnBackgroundListSyntax> { WhitespaceElement, EscapedLine } } }),

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
        };

        internal static List<(string, ParseTree, PgnErrorCode[])> TriviaParseTreesWithErrors() => new List<(string, ParseTree, PgnErrorCode[])>
        {
            (" %", new ParseTree<PgnSyntaxNodes> { new ParseTree<PgnTriviaSyntax> { new ParseTree<PgnBackgroundListSyntax> { WhitespaceElement, IllegalCharacter } } },
                new[] { PgnErrorCode.IllegalCharacter }),
            (" % ", new ParseTree<PgnSyntaxNodes> { new ParseTree<PgnTriviaSyntax> { new ParseTree<PgnBackgroundListSyntax> { WhitespaceElement, IllegalCharacter, WhitespaceElement } } },
                new[] { PgnErrorCode.IllegalCharacter }),

            ("{", new ParseTree<PgnSyntaxNodes> { CommentTrivia }, new[] { PgnErrorCode.UnterminatedMultiLineComment }),
            (" {", new ParseTree<PgnSyntaxNodes> { WhitespaceThenCommentTrivia }, new[] { PgnErrorCode.UnterminatedMultiLineComment }),
            ("{ ", new ParseTree<PgnSyntaxNodes> { CommentTrivia }, new[] { PgnErrorCode.UnterminatedMultiLineComment }),
            (" { ", new ParseTree<PgnSyntaxNodes> { WhitespaceThenCommentTrivia }, new[] { PgnErrorCode.UnterminatedMultiLineComment }),

            ("0%%0", new ParseTree<PgnSyntaxNodes> { SymbolNoTrivia, PgnSymbolWithLeadingTrivia(new ParseTree<PgnTriviaSyntax> { TwoIllegalCharacters }), EmptyTrivia },
                new[] { PgnErrorCode.IllegalCharacter, PgnErrorCode.IllegalCharacter }),
        };
    }
}

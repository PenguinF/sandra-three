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
using System.Collections.Generic;

namespace Sandra.Chess.Tests
{
    public static partial class ParseTrees
    {
        private static ParseTree<PgnGameListSyntax> NoGame(ParseTree<PgnTriviaSyntax> trailingTrivia)
            => new ParseTree<PgnGameListSyntax> { trailingTrivia };

        internal static List<(string, ParseTree)> TriviaParseTrees() => new List<(string, ParseTree)>
        {
            ("", NoGame(EmptyTrivia)),
            (" ", NoGame(WhitespaceTrivia)),
            ("%", NoGame(new ParseTree<PgnTriviaSyntax> { new ParseTree<PgnBackgroundListSyntax> { EscapedLine } })),
            ("% ", NoGame(new ParseTree<PgnTriviaSyntax> { new ParseTree<PgnBackgroundListSyntax> { EscapedLine } })),
            ("% \n", NoGame(new ParseTree<PgnTriviaSyntax> { new ParseTree<PgnBackgroundListSyntax> { EscapedLine, WhitespaceElement } })),
            ("\n%", NoGame(new ParseTree<PgnTriviaSyntax> { new ParseTree<PgnBackgroundListSyntax> { WhitespaceElement, EscapedLine } })),

            ("{}", NoGame(CommentTrivia)),
            (" {} {} ", NoGame(new ParseTree<PgnTriviaSyntax> { WhitespaceThenComment, WhitespaceThenComment, Whitespace })),
        };

        internal static List<(string, ParseTree, PgnErrorCode[])> TriviaParseTreesWithErrors() => new List<(string, ParseTree, PgnErrorCode[])>
        {
            ("  {}   * ", Games(Game(ResultWithTrivia(WhitespaceThenComment, Whitespace)), WhitespaceTrivia),
                new[] { PgnErrorCode.MissingTagSection }),
            (" *   {}  ", Games(Game(WS_Result), WhitespaceCommentWhitespace),
                new[] { PgnErrorCode.MissingTagSection }),

            (" * {} {} ", Games(Game(WS_Result), new ParseTree<PgnTriviaSyntax> { WhitespaceThenComment, WhitespaceThenComment, Whitespace }),
                new[] { PgnErrorCode.MissingTagSection }),
            (" {} * {} ", Games(Game(ResultWithTrivia(WhitespaceCommentWhitespace)), WhitespaceCommentWhitespace),
                new[] { PgnErrorCode.MissingTagSection }),
            (" {} {} * ", Games(Game(ResultWithTrivia(WhitespaceThenComment, WhitespaceThenComment, Whitespace)), WhitespaceTrivia),
                new[] { PgnErrorCode.MissingTagSection }),

            (" {} * * ", Games(Game(ResultWithTrivia(WhitespaceThenComment, Whitespace)), Game(WS_Result), WhitespaceTrivia),
                new[] { PgnErrorCode.MissingTagSection, PgnErrorCode.MissingTagSection }),
            (" * {} * ", Games(Game(WS_Result), Game(ResultWithTrivia(WhitespaceThenComment, Whitespace)), WhitespaceTrivia),
                new[] { PgnErrorCode.MissingTagSection, PgnErrorCode.MissingTagSection }),
            (" * * {} ", Games(Game(WS_Result), Game(WS_Result), WhitespaceCommentWhitespace),
                new[] { PgnErrorCode.MissingTagSection, PgnErrorCode.MissingTagSection }),

            ("{}{}*{}", Games(Game(ResultWithTrivia(CommentNoBackground, CommentNoBackground, EmptyBackground)), CommentTrivia),
                new[] { PgnErrorCode.MissingTagSection }),
            ("{}{}**", Games(Game(ResultWithTrivia(CommentNoBackground, CommentNoBackground, EmptyBackground)), Game(ResultNoTrivia)),
                new[] { PgnErrorCode.MissingTagSection, PgnErrorCode.MissingTagSection }),
            ("{}*{}{}", Games(Game(ResultWithTrivia(CommentNoBackground, EmptyBackground)), new ParseTree<PgnTriviaSyntax> { CommentNoBackground, CommentNoBackground, EmptyBackground } ),
                new[] { PgnErrorCode.MissingTagSection }),
            ("{}*{}*", Games(Game(ResultWithTrivia(CommentNoBackground, EmptyBackground)), Game(ResultWithTrivia(CommentNoBackground, EmptyBackground))),
                new[] { PgnErrorCode.MissingTagSection, PgnErrorCode.MissingTagSection }),
            ("{}**{}", Games(Game(ResultWithTrivia(CommentNoBackground, EmptyBackground)), Game(ResultNoTrivia), CommentTrivia),
                new[] { PgnErrorCode.MissingTagSection, PgnErrorCode.MissingTagSection }),
            ("*{}{}*", Games(Game(ResultNoTrivia), Game(ResultWithTrivia(CommentNoBackground, CommentNoBackground, EmptyBackground))),
                new[] { PgnErrorCode.MissingTagSection, PgnErrorCode.MissingTagSection }),
            ("*{}*{}", Games(Game(ResultNoTrivia), Game(ResultWithTrivia(CommentNoBackground, EmptyBackground)), CommentTrivia),
                new[] { PgnErrorCode.MissingTagSection, PgnErrorCode.MissingTagSection }),
            ("*{}**", Games(Game(ResultNoTrivia), Game(ResultWithTrivia(CommentNoBackground, EmptyBackground)), Game(ResultNoTrivia)),
                new[] { PgnErrorCode.MissingTagSection, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingTagSection }),
            ("**{}{}", Games(Game(ResultNoTrivia), Game(ResultNoTrivia), new ParseTree<PgnTriviaSyntax> { CommentNoBackground, CommentNoBackground, EmptyBackground } ),
                new[] { PgnErrorCode.MissingTagSection, PgnErrorCode.MissingTagSection }),
            ("**{}*", Games(Game(ResultNoTrivia), Game(ResultNoTrivia), Game(ResultWithTrivia(CommentNoBackground, EmptyBackground))),
                new[] { PgnErrorCode.MissingTagSection, PgnErrorCode.MissingTagSection, PgnErrorCode.MissingTagSection }),

            (" %", NoGame(new ParseTree<PgnTriviaSyntax> { new ParseTree<PgnBackgroundListSyntax> { WhitespaceElement, IllegalCharacter } }),
                new[] { PgnErrorCode.IllegalCharacter }),
            (" % ", NoGame(new ParseTree<PgnTriviaSyntax> { new ParseTree<PgnBackgroundListSyntax> { WhitespaceElement, IllegalCharacter, WhitespaceElement } }),
                new[] { PgnErrorCode.IllegalCharacter }),

            ("{", NoGame(CommentTrivia), new[] { PgnErrorCode.UnterminatedMultiLineComment }),
            (" {", NoGame(WhitespaceThenCommentTrivia), new[] { PgnErrorCode.UnterminatedMultiLineComment }),
            ("{ ", NoGame(CommentTrivia), new[] { PgnErrorCode.UnterminatedMultiLineComment }),
            (" { ", NoGame(WhitespaceThenCommentTrivia), new[] { PgnErrorCode.UnterminatedMultiLineComment }),

            ("0 0", OneGame(EmptyTagSection, Plies(Ply(NoFloats(MoveNumberNoTrivia)), Ply(NoFloats(WS_MoveNumber)))),
                new[]
                {
                    PgnErrorCode.MissingMove, PgnErrorCode.MissingMove,
                    PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker
                }),
            (" 0  00   000    0 ", OneGameTrailingTrivia(EmptyTagSection, Plies(Ply(NoFloats(WS_MoveNumber)), Ply(NoFloats(WS_MoveNumber)), Ply(NoFloats(WS_MoveNumber)), Ply(NoFloats(WS_MoveNumber))), WhitespaceTrivia),
                new[]
                {
                    PgnErrorCode.MissingMove, PgnErrorCode.MissingMove, PgnErrorCode.MissingMove, PgnErrorCode.MissingMove,
                    PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker
                }),

            ("0%%0", OneGame(EmptyTagSection, Plies(Ply(NoFloats(MoveNumberNoTrivia)), Ply(NoFloats(new ParseTree<PgnMoveNumberWithTriviaSyntax> { TwoIllegalCharactersTrivia, MoveNumber })))),
                new[]
                {
                    PgnErrorCode.IllegalCharacter, PgnErrorCode.IllegalCharacter, PgnErrorCode.MissingMove, PgnErrorCode.MissingMove,
                    PgnErrorCode.MissingTagSection, PgnErrorCode.MissingGameTerminationMarker
                }),
        };
    }
}

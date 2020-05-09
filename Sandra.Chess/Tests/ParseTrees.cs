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
using System.Linq;

namespace Sandra.Chess.Tests
{
    public static partial class ParseTrees
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

        private static ParseTree<PgnTagElementWithTriviaSyntax> NoTrivia<TTagElement>()
            where TTagElement : PgnTagElementSyntax
            => new ParseTree<PgnTagElementWithTriviaSyntax> { EmptyTrivia, new ParseTree<TTagElement>() };

        private static ParseTree<PgnTagElementWithTriviaSyntax> LeadingWhitespace<TTagElement>()
            where TTagElement : PgnTagElementSyntax
            => new ParseTree<PgnTagElementWithTriviaSyntax> { WhitespaceTrivia, new ParseTree<TTagElement>() };

        private static readonly ParseTree<PgnTagElementWithTriviaSyntax> BracketClose = NoTrivia<PgnBracketCloseSyntax>();
        private static readonly ParseTree<PgnTagElementWithTriviaSyntax> BracketOpen = NoTrivia<PgnBracketOpenSyntax>();
        private static readonly ParseTree<PgnTagElementWithTriviaSyntax> TagName = NoTrivia<PgnTagNameSyntax>();
        private static readonly ParseTree<PgnTagElementWithTriviaSyntax> TagValue = NoTrivia<PgnTagValueSyntax>();

        private static readonly ParseTree<PgnTagElementWithTriviaSyntax> WS_BracketClose = LeadingWhitespace<PgnBracketCloseSyntax>();
        private static readonly ParseTree<PgnTagElementWithTriviaSyntax> WS_BracketOpen = LeadingWhitespace<PgnBracketOpenSyntax>();
        private static readonly ParseTree<PgnTagElementWithTriviaSyntax> WS_TagName = LeadingWhitespace<PgnTagNameSyntax>();
        private static readonly ParseTree<PgnTagElementWithTriviaSyntax> WS_TagValue = LeadingWhitespace<PgnTagValueSyntax>();

        private static ParseTree<PgnTagPairSyntax> TagPair(ParseTree<PgnTagElementWithTriviaSyntax> firstSymbol, params ParseTree<PgnTagElementWithTriviaSyntax>[] otherSymbols)
        {
            var tagPairSyntax = new ParseTree<PgnTagPairSyntax> { firstSymbol };
            otherSymbols.ForEach(tagPairSyntax.Add);
            return tagPairSyntax;
        }

        private static ParseTree<PgnSyntaxNodes> TagSectionTrailingTrivia(
            ParseTree<PgnTriviaSyntax> trailingTrivia,
            ParseTree<PgnTagPairSyntax> firstTagPair,
            params ParseTree<PgnTagPairSyntax>[] otherTagPairs)
        {
            var tagSectionSyntax = new ParseTree<PgnTagSectionSyntax> { firstTagPair };
            otherTagPairs.ForEach(tagSectionSyntax.Add);
            return new ParseTree<PgnSyntaxNodes> { tagSectionSyntax, trailingTrivia };
        }

        internal static readonly List<(string, ParseTree)> TestParseTrees
            = TriviaParseTrees()
            .Union(TagSectionParseTrees())
            .ToList();

        internal static readonly List<(string, ParseTree, PgnErrorCode[])> TestParseTreesWithErrors
            = TriviaParseTreesWithErrors()
            .Union(TagSectionParseTreesWithErrors())
            .ToList();
    }
}

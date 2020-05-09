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

        private class TagSectionAndMoveTreeAndResult
        {
            public ParseTree<PgnTagSectionSyntax> TagSection;
            public ParseTree<PgnSymbolWithTrivia>[] MoveSection;
            public ParseTree<PgnSymbolWithTrivia> Result;

            public TagSectionAndMoveTreeAndResult(ParseTree<PgnSymbolWithTrivia> result)
            {
                Result = result;
            }

            public TagSectionAndMoveTreeAndResult(ParseTree<PgnTagSectionSyntax> tagSection, ParseTree<PgnSymbolWithTrivia>[] moveSection)
            {
                TagSection = tagSection;
                MoveSection = moveSection;
            }

            public TagSectionAndMoveTreeAndResult(ParseTree<PgnTagSectionSyntax> tagSection, ParseTree<PgnSymbolWithTrivia>[] moveSection, ParseTree<PgnSymbolWithTrivia> result)
            {
                TagSection = tagSection;
                MoveSection = moveSection;
                Result = result;
            }

            public void AddTo(ParseTree<PgnSyntaxNodes> gamesSyntax)
            {
                if (TagSection != null && TagSection.Any()) gamesSyntax.Add(TagSection);
                if (MoveSection != null) MoveSection.ForEach(gamesSyntax.Add);
                if (Result != null) gamesSyntax.Add(Result);
            }
        }

        private static readonly ParseTree<PgnWhitespaceSyntax> WhitespaceElement = new ParseTree<PgnWhitespaceSyntax>();
        private static readonly ParseTree<PgnIllegalCharacterSyntax> IllegalCharacter = new ParseTree<PgnIllegalCharacterSyntax>();
        private static readonly ParseTree<PgnEscapeSyntax> EscapedLine = new ParseTree<PgnEscapeSyntax>();

        private static readonly ParseTree<PgnBackgroundListSyntax> EmptyBackground = new ParseTree<PgnBackgroundListSyntax>();
        private static readonly ParseTree<PgnTriviaSyntax> EmptyTrivia = new ParseTree<PgnTriviaSyntax> { EmptyBackground };
        private static readonly ParseTree<PgnBackgroundListSyntax> Whitespace = new ParseTree<PgnBackgroundListSyntax> { WhitespaceElement };
        private static readonly ParseTree<PgnTriviaSyntax> WhitespaceTrivia = new ParseTree<PgnTriviaSyntax> { Whitespace };
        private static readonly ParseTree<PgnBackgroundListSyntax> TwoIllegalCharacters = new ParseTree<PgnBackgroundListSyntax> { IllegalCharacter, IllegalCharacter };
        private static readonly ParseTree<PgnTriviaSyntax> TwoIllegalCharactersTrivia = new ParseTree<PgnTriviaSyntax> { TwoIllegalCharacters };

        private static readonly ParseTree<PgnCommentSyntax> Comment = new ParseTree<PgnCommentSyntax>();

        private static readonly ParseTree<PgnTriviaElementSyntax> CommentNoBackground = new ParseTree<PgnTriviaElementSyntax> { EmptyBackground, Comment };
        private static readonly ParseTree<PgnTriviaElementSyntax> WhitespaceThenComment = new ParseTree<PgnTriviaElementSyntax> { Whitespace, Comment };

        private static readonly ParseTree<PgnTriviaSyntax> CommentTrivia = new ParseTree<PgnTriviaSyntax> { CommentNoBackground, EmptyBackground };
        private static readonly ParseTree<PgnTriviaSyntax> WhitespaceThenCommentTrivia = new ParseTree<PgnTriviaSyntax> { WhitespaceThenComment, EmptyBackground };
        private static readonly ParseTree<PgnTriviaSyntax> WhitespaceCommentWhitespace = new ParseTree<PgnTriviaSyntax> { WhitespaceThenComment, Whitespace };

        private static readonly ParseTree<PgnSymbol> MoveNumber = new ParseTree<PgnSymbol>();
        private static readonly ParseTree<PgnSymbolWithTrivia> MoveNumberNoTrivia = new ParseTree<PgnSymbolWithTrivia> { EmptyTrivia, MoveNumber };
        private static readonly ParseTree<PgnSymbolWithTrivia> WS_MoveNumber = new ParseTree<PgnSymbolWithTrivia> { WhitespaceTrivia, MoveNumber };

        private static readonly ParseTree<PgnSymbol> GameResult = new ParseTree<PgnSymbol>();

        private static readonly ParseTree<PgnSymbolWithTrivia> ResultNoTrivia = new ParseTree<PgnSymbolWithTrivia> { EmptyTrivia, GameResult };

        private static ParseTree<PgnSymbolWithTrivia> ResultWithTrivia(
            ParseTree<PgnTriviaSyntax> leadingTrivia)
            => new ParseTree<PgnSymbolWithTrivia> { leadingTrivia, GameResult };

        private static ParseTree<PgnSymbolWithTrivia> ResultWithTrivia(
            ParseTree<PgnTriviaElementSyntax> element1,
            ParseTree<PgnBackgroundListSyntax> backgroundAfter)
            => ResultWithTrivia(new ParseTree<PgnTriviaSyntax> { element1, backgroundAfter });

        private static ParseTree<PgnSymbolWithTrivia> ResultWithTrivia(
            ParseTree<PgnTriviaElementSyntax> element1,
            ParseTree<PgnTriviaElementSyntax> element2,
            ParseTree<PgnBackgroundListSyntax> backgroundAfter)
            => ResultWithTrivia(new ParseTree<PgnTriviaSyntax> { element1, element2, backgroundAfter });

        private static readonly ParseTree<PgnSymbolWithTrivia> WS_Result = ResultWithTrivia(WhitespaceTrivia);

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

        private static readonly ParseTree<PgnTagSectionSyntax> EmptyTagSection = new ParseTree<PgnTagSectionSyntax> { };

        private static ParseTree<PgnTagSectionSyntax> TagSection(params ParseTree<PgnTagPairSyntax>[] tagPairs)
        {
            var tagSectionSyntax = new ParseTree<PgnTagSectionSyntax>();
            tagPairs.ForEach(tagSectionSyntax.Add);
            return tagSectionSyntax;
        }

        private static ParseTree<PgnSymbolWithTrivia>[] Plies(params ParseTree<PgnSymbolWithTrivia>[] symbols)
            => symbols;

        private static readonly ParseTree<PgnSymbolWithTrivia>[] NoPlies = Plies();

        private static TagSectionAndMoveTreeAndResult Game(
            ParseTree<PgnSymbolWithTrivia> result)
            => new TagSectionAndMoveTreeAndResult(result);

        private static TagSectionAndMoveTreeAndResult Game(
            ParseTree<PgnTagSectionSyntax> tagSection,
            ParseTree<PgnSymbolWithTrivia>[] moveSection)
            => new TagSectionAndMoveTreeAndResult(tagSection, moveSection);

        private static ParseTree<PgnSyntaxNodes> Games(TagSectionAndMoveTreeAndResult game1, ParseTree<PgnTriviaSyntax> trailingTrivia)
        {
            var gamesSyntax = new ParseTree<PgnSyntaxNodes>();
            game1.AddTo(gamesSyntax);
            gamesSyntax.Add(trailingTrivia);
            return gamesSyntax;
        }

        private static ParseTree<PgnSyntaxNodes> Games(TagSectionAndMoveTreeAndResult game1, TagSectionAndMoveTreeAndResult game2, ParseTree<PgnTriviaSyntax> trailingTrivia)
        {
            var gamesSyntax = new ParseTree<PgnSyntaxNodes>();
            game1.AddTo(gamesSyntax);
            game2.AddTo(gamesSyntax);
            gamesSyntax.Add(trailingTrivia);
            return gamesSyntax;
        }

        private static ParseTree<PgnSyntaxNodes> Games(params TagSectionAndMoveTreeAndResult[] games)
        {
            var gamesSyntax = new ParseTree<PgnSyntaxNodes>();
            games.ForEach(x => x.AddTo(gamesSyntax));
            gamesSyntax.Add(EmptyTrivia);
            return gamesSyntax;
        }

        private static ParseTree<PgnSyntaxNodes> OneGameTrailingTrivia(
            ParseTree<PgnTagSectionSyntax> tagSection,
            ParseTree<PgnSymbolWithTrivia>[] moveSection,
            ParseTree<PgnTriviaSyntax> trailingTrivia)
            => Games(Game(tagSection, moveSection), trailingTrivia);

        private static ParseTree<PgnSyntaxNodes> OneGame(ParseTree<PgnTagSectionSyntax> tagSection, ParseTree<PgnSymbolWithTrivia>[] moveSection)
            => Games(Game(tagSection, moveSection));

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

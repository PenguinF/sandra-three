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
            public ParseTree<PgnPlyListSyntax> MoveSection;
            public ParseTree<PgnGameResultWithTriviaSyntax> Result;

            public TagSectionAndMoveTreeAndResult(ParseTree<PgnTagSectionSyntax> tagSection, ParseTree<PgnPlyListSyntax> moveSection)
            {
                TagSection = tagSection;
                MoveSection = moveSection;
            }

            public TagSectionAndMoveTreeAndResult(ParseTree<PgnTagSectionSyntax> tagSection, ParseTree<PgnPlyListSyntax> moveSection, ParseTree<PgnGameResultWithTriviaSyntax> result)
            {
                TagSection = tagSection;
                MoveSection = moveSection;
                Result = result;
            }

            public void AddTo(ParseTree<PgnSyntaxNodes> gamesSyntax)
            {
                if (TagSection.Any()) gamesSyntax.Add(TagSection);
                if (MoveSection.Any()) gamesSyntax.Add(MoveSection);
                if (Result != null) gamesSyntax.Add(Result);
            }
        }

        private static readonly ParseTree<PgnEmptySyntax> Missing = new ParseTree<PgnEmptySyntax>();

        private static readonly ParseTree<PgnWhitespaceSyntax> WhitespaceElement = new ParseTree<PgnWhitespaceSyntax>();
        private static readonly ParseTree<PgnIllegalCharacterSyntax> IllegalCharacter = new ParseTree<PgnIllegalCharacterSyntax>();
        private static readonly ParseTree<PgnEscapeSyntax> EscapedLine = new ParseTree<PgnEscapeSyntax>();

        private static readonly ParseTree<PgnBackgroundListSyntax> EmptyBackground = new ParseTree<PgnBackgroundListSyntax>();
        private static readonly ParseTree<PgnTriviaSyntax> EmptyTrivia = new ParseTree<PgnTriviaSyntax> { EmptyBackground };
        private static readonly ParseTree<PgnBackgroundListSyntax> Whitespace = new ParseTree<PgnBackgroundListSyntax> { WhitespaceElement };
        private static readonly ParseTree<PgnTriviaSyntax> WhitespaceTrivia = new ParseTree<PgnTriviaSyntax> { Whitespace };
        private static readonly ParseTree<PgnTriviaSyntax> IllegalCharacterTrivia = new ParseTree<PgnTriviaSyntax> { new ParseTree<PgnBackgroundListSyntax> { IllegalCharacter } };
        private static readonly ParseTree<PgnTriviaSyntax> TwoIllegalCharactersTrivia = new ParseTree<PgnTriviaSyntax> { new ParseTree<PgnBackgroundListSyntax> { IllegalCharacter, IllegalCharacter } };

        private static readonly ParseTree<PgnCommentSyntax> Comment = new ParseTree<PgnCommentSyntax>();

        private static readonly ParseTree<PgnTriviaElementSyntax> CommentNoBackground = new ParseTree<PgnTriviaElementSyntax> { EmptyBackground, Comment };
        private static readonly ParseTree<PgnTriviaElementSyntax> WhitespaceThenComment = new ParseTree<PgnTriviaElementSyntax> { Whitespace, Comment };

        private static readonly ParseTree<PgnTriviaSyntax> CommentTrivia = new ParseTree<PgnTriviaSyntax> { CommentNoBackground, EmptyBackground };
        private static readonly ParseTree<PgnTriviaSyntax> WhitespaceThenCommentTrivia = new ParseTree<PgnTriviaSyntax> { WhitespaceThenComment, EmptyBackground };
        private static readonly ParseTree<PgnTriviaSyntax> WhitespaceCommentWhitespace = new ParseTree<PgnTriviaSyntax> { WhitespaceThenComment, Whitespace };

        private static readonly ParseTree<PgnMoveNumberSyntax> MoveNumber = new ParseTree<PgnMoveNumberSyntax>();
        private static readonly ParseTree<PgnMoveNumberWithTriviaSyntax> MoveNumberNoTrivia = new ParseTree<PgnMoveNumberWithTriviaSyntax> { EmptyTrivia, MoveNumber };
        private static readonly ParseTree<PgnMoveNumberWithTriviaSyntax> WS_MoveNumber = new ParseTree<PgnMoveNumberWithTriviaSyntax> { WhitespaceTrivia, MoveNumber };

        private static readonly ParseTree<PgnMoveSyntax> Move = new ParseTree<PgnMoveSyntax>();
        private static readonly ParseTree<PgnMoveWithTriviaSyntax> MoveNoTrivia = new ParseTree<PgnMoveWithTriviaSyntax> { EmptyTrivia, Move };
        private static readonly ParseTree<PgnMoveWithTriviaSyntax> WS_Move = new ParseTree<PgnMoveWithTriviaSyntax> { WhitespaceTrivia, Move };

        private static readonly ParseTree<PgnPeriodSyntax> Period = new ParseTree<PgnPeriodSyntax>();
        private static readonly ParseTree<PgnPeriodWithTriviaSyntax> PeriodNoTrivia = new ParseTree<PgnPeriodWithTriviaSyntax> { EmptyTrivia, Period };
        private static readonly ParseTree<PgnPeriodWithTriviaSyntax> WS_Period = new ParseTree<PgnPeriodWithTriviaSyntax> { WhitespaceTrivia, Period };

        private static readonly ParseTree<PgnNagSyntax> NAG = new ParseTree<PgnNagSyntax>();
        private static readonly ParseTree<PgnNagWithTriviaSyntax> NAGNoTrivia = new ParseTree<PgnNagWithTriviaSyntax> { EmptyTrivia, NAG };
        private static readonly ParseTree<PgnNagWithTriviaSyntax> WS_NAG = new ParseTree<PgnNagWithTriviaSyntax> { WhitespaceTrivia, NAG };

        private static readonly ParseTree<PgnGameResultSyntax> GameResult = new ParseTree<PgnGameResultSyntax>();

        private static readonly ParseTree<PgnGameResultWithTriviaSyntax> ResultNoTrivia = new ParseTree<PgnGameResultWithTriviaSyntax> { EmptyTrivia, GameResult };

        private static ParseTree<PgnGameResultWithTriviaSyntax> ResultWithTrivia(
            ParseTree<PgnTriviaSyntax> leadingTrivia)
            => new ParseTree<PgnGameResultWithTriviaSyntax> { leadingTrivia, GameResult };

        private static ParseTree<PgnGameResultWithTriviaSyntax> ResultWithTrivia(
            ParseTree<PgnTriviaElementSyntax> element1,
            ParseTree<PgnBackgroundListSyntax> backgroundAfter)
            => ResultWithTrivia(new ParseTree<PgnTriviaSyntax> { element1, backgroundAfter });

        private static ParseTree<PgnGameResultWithTriviaSyntax> ResultWithTrivia(
            ParseTree<PgnTriviaElementSyntax> element1,
            ParseTree<PgnTriviaElementSyntax> element2,
            ParseTree<PgnBackgroundListSyntax> backgroundAfter)
            => ResultWithTrivia(new ParseTree<PgnTriviaSyntax> { element1, element2, backgroundAfter });

        private static readonly ParseTree<PgnGameResultWithTriviaSyntax> WS_Result = ResultWithTrivia(WhitespaceTrivia);

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

        private static readonly ParseTree<PgnPlyFloatItemListSyntax> EmptyFloatItems
            = new ParseTree<PgnPlyFloatItemListSyntax>();

        private static readonly ParseTree<PgnPlyFloatItemListSyntax> OnePeriod
            = new ParseTree<PgnPlyFloatItemListSyntax> { PeriodNoTrivia };

        private static readonly ParseTree<PgnPlyFloatItemListSyntax> TwoPeriods
            = new ParseTree<PgnPlyFloatItemListSyntax> { PeriodNoTrivia, PeriodNoTrivia };

        private static ParseTree<PgnMoveNumberWithFloatItemsSyntax> WithFloats(
            ParseTree<PgnPlyFloatItemListSyntax> leadingFloatItems,
            ParseTree<PgnMoveNumberWithTriviaSyntax> moveNumberWithTrivia)
            => new ParseTree<PgnMoveNumberWithFloatItemsSyntax> { leadingFloatItems, moveNumberWithTrivia };

        private static ParseTree<PgnMoveWithFloatItemsSyntax> WithFloats(
            ParseTree<PgnPlyFloatItemListSyntax> leadingFloatItems,
            ParseTree<PgnMoveWithTriviaSyntax> moveWithTrivia)
            => new ParseTree<PgnMoveWithFloatItemsSyntax> { leadingFloatItems, moveWithTrivia };

        private static ParseTree<PgnNagWithFloatItemsSyntax> WithFloats(
            ParseTree<PgnPlyFloatItemListSyntax> leadingFloatItems,
            ParseTree<PgnNagWithTriviaSyntax> nagWithTrivia)
            => new ParseTree<PgnNagWithFloatItemsSyntax> { leadingFloatItems, nagWithTrivia };

        private static ParseTree<PgnMoveNumberWithFloatItemsSyntax> NoFloats(ParseTree<PgnMoveNumberWithTriviaSyntax> moveNumberWithTrivia)
            => WithFloats(EmptyFloatItems, moveNumberWithTrivia);

        private static ParseTree<PgnMoveWithFloatItemsSyntax> NoFloats(ParseTree<PgnMoveWithTriviaSyntax> moveWithTrivia)
            => WithFloats(EmptyFloatItems, moveWithTrivia);

        private static ParseTree<PgnNagWithFloatItemsSyntax> NoFloats(ParseTree<PgnNagWithTriviaSyntax> nagWithTrivia)
            => WithFloats(EmptyFloatItems, nagWithTrivia);

        private static readonly ParseTree<PgnMoveNumberWithFloatItemsSyntax> MoveNumberNoFloats = NoFloats(MoveNumberNoTrivia);
        private static readonly ParseTree<PgnMoveWithFloatItemsSyntax> MoveNoFloats = NoFloats(MoveNoTrivia);
        private static readonly ParseTree<PgnNagWithFloatItemsSyntax> NAGNoFloats = NoFloats(NAGNoTrivia);

        private static readonly ParseTree<PgnMoveNumberWithFloatItemsSyntax> WS_MoveNumberNoFloats = NoFloats(WS_MoveNumber);
        private static readonly ParseTree<PgnMoveWithFloatItemsSyntax> WS_MoveNoFloats = NoFloats(WS_Move);
        private static readonly ParseTree<PgnNagWithFloatItemsSyntax> WS_NAGNoFloats = NoFloats(WS_NAG);

        private static ParseTree<PgnPlySyntax> Ply(
            ParseTree<PgnMoveNumberWithFloatItemsSyntax> moveNumber,
            ParseTree<PgnMoveWithFloatItemsSyntax> move,
            params ParseTree<PgnNagWithFloatItemsSyntax>[] nags)
        {
            var plySyntax = new ParseTree<PgnPlySyntax>();
            if (moveNumber != null) plySyntax.Add(moveNumber); else plySyntax.Add(Missing);
            if (move != null) plySyntax.Add(move); else plySyntax.Add(Missing);
            nags.ForEach(plySyntax.Add);
            return plySyntax;
        }

        private static ParseTree<PgnPlySyntax> Ply(
            ParseTree<PgnMoveNumberWithFloatItemsSyntax> moveNumber,
            params ParseTree<PgnNagWithFloatItemsSyntax>[] nags)
            => Ply(moveNumber, null, nags);

        private static ParseTree<PgnPlySyntax> Ply(
            ParseTree<PgnMoveWithFloatItemsSyntax> move,
            params ParseTree<PgnNagWithFloatItemsSyntax>[] nags)
            => Ply(null, move, nags);

        private static ParseTree<PgnPlySyntax> Ply(
            params ParseTree<PgnNagWithFloatItemsSyntax>[] nags)
            => Ply(null, null, nags);

        private static ParseTree<PgnPlyListSyntax> PliesTrailingFloatItems(
            ParseTree<PgnPlyFloatItemListSyntax> trailingFloatItems,
            params ParseTree<PgnPlySyntax>[] plies)
        {
            var plyListSyntax = new ParseTree<PgnPlyListSyntax>();
            plies.ForEach(plyListSyntax.Add);
            if (plies.Any() || trailingFloatItems.Any()) plyListSyntax.Add(trailingFloatItems);
            return plyListSyntax;
        }

        private static ParseTree<PgnPlyListSyntax> Plies(
            ParseTree<PgnPlyFloatItemListSyntax> trailingFloatItems)
            => new ParseTree<PgnPlyListSyntax> { trailingFloatItems };

        private static ParseTree<PgnPlyListSyntax> Plies(
            ParseTree<PgnPlySyntax> ply1,
            ParseTree<PgnPlyFloatItemListSyntax> trailingFloatItems)
            => new ParseTree<PgnPlyListSyntax> { ply1, trailingFloatItems };

        private static ParseTree<PgnPlyListSyntax> Plies(params ParseTree<PgnPlySyntax>[] plies)
            => PliesTrailingFloatItems(EmptyFloatItems, plies);

        private static readonly ParseTree<PgnPlyListSyntax> NoPlies = Plies();

        private static TagSectionAndMoveTreeAndResult Game(
            ParseTree<PgnGameResultWithTriviaSyntax> result)
            => new TagSectionAndMoveTreeAndResult(EmptyTagSection, NoPlies, result);

        private static TagSectionAndMoveTreeAndResult Game(
            ParseTree<PgnTagSectionSyntax> tagSection,
            ParseTree<PgnPlyListSyntax> moveSection)
            => new TagSectionAndMoveTreeAndResult(tagSection, moveSection);

        private static TagSectionAndMoveTreeAndResult Game(
            ParseTree<PgnTagSectionSyntax> tagSection,
            ParseTree<PgnPlyListSyntax> moveSection,
            ParseTree<PgnGameResultWithTriviaSyntax> result)
            => new TagSectionAndMoveTreeAndResult(tagSection, moveSection, result);

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
            ParseTree<PgnPlyListSyntax> moveSection,
            ParseTree<PgnTriviaSyntax> trailingTrivia)
            => Games(Game(tagSection, moveSection), trailingTrivia);

        private static ParseTree<PgnSyntaxNodes> OneGame(
            ParseTree<PgnTagSectionSyntax> tagSection,
            ParseTree<PgnPlyListSyntax> moveSection)
            => Games(Game(tagSection, moveSection));

        private static ParseTree<PgnSyntaxNodes> OneGame(
            ParseTree<PgnTagSectionSyntax> tagSection,
            ParseTree<PgnPlyListSyntax> moveSection,
            ParseTree<PgnGameResultWithTriviaSyntax> result)
            => Games(Game(tagSection, moveSection, result));

        private static ParseTree<PgnSyntaxNodes> TagSectionOnly(params ParseTree<PgnTagPairSyntax>[] tagPairs)
            => OneGame(TagSection(tagPairs), NoPlies);

        internal static readonly List<(string, ParseTree)> TestParseTrees
            = TriviaParseTrees()
            .Union(TagSectionParseTrees())
            .Union(PlyParseTrees())
            .ToList();

        internal static readonly List<(string, ParseTree, PgnErrorCode[])> TestParseTreesWithErrors
            = TriviaParseTreesWithErrors()
            .Union(TagSectionParseTreesWithErrors())
            .Union(PlyParseTreesWithErrors())
            .ToList();
    }
}

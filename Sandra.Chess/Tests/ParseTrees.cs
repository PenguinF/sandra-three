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
        private static readonly ParseTree<PgnPlyFloatItemWithTriviaSyntax> PeriodNoTrivia = new ParseTree<PgnPlyFloatItemWithTriviaSyntax> { EmptyTrivia, Period };

        private static readonly ParseTree<PgnNagSyntax> NAG = new ParseTree<PgnNagSyntax>();
        private static readonly ParseTree<PgnNagWithTriviaSyntax> NAGNoTrivia = new ParseTree<PgnNagWithTriviaSyntax> { EmptyTrivia, NAG };
        private static readonly ParseTree<PgnNagWithTriviaSyntax> WS_NAG = new ParseTree<PgnNagWithTriviaSyntax> { WhitespaceTrivia, NAG };

        private static readonly ParseTree<PgnOrphanParenthesisCloseSyntax> OrphanClose = new ParseTree<PgnOrphanParenthesisCloseSyntax>();
        private static readonly ParseTree<PgnPlyFloatItemWithTriviaSyntax> OrphanCloseNoTrivia = new ParseTree<PgnPlyFloatItemWithTriviaSyntax> { EmptyTrivia, OrphanClose };

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

        private static readonly ParseTree<PgnTagSectionSyntax> SmallestCorrectTagSection
            = TagSection(TagPair(BracketOpen, TagName, TagValue, BracketClose));

        private static readonly ParseTree<PgnPlyFloatItemListSyntax> EmptyFloatItems
            = new ParseTree<PgnPlyFloatItemListSyntax>();

        private static readonly ParseTree<PgnPlyFloatItemListSyntax> OnePeriod
            = new ParseTree<PgnPlyFloatItemListSyntax> { PeriodNoTrivia };

        private static readonly ParseTree<PgnPlyFloatItemListSyntax> TwoPeriods
            = new ParseTree<PgnPlyFloatItemListSyntax> { PeriodNoTrivia, PeriodNoTrivia };

        private static readonly ParseTree<PgnPlyFloatItemListSyntax> OneOrphanClose
            = new ParseTree<PgnPlyFloatItemListSyntax> { OrphanCloseNoTrivia };

        private static readonly ParseTree<PgnPlyFloatItemListSyntax> TwoOrphanClose
            = new ParseTree<PgnPlyFloatItemListSyntax> { OrphanCloseNoTrivia, OrphanCloseNoTrivia };

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

        private static ParseTree<PgnVariationWithFloatItemsSyntax> WithFloats(
            ParseTree<PgnPlyFloatItemListSyntax> leadingFloatItems,
            ParseTree<PgnVariationSyntax> variation)
            => new ParseTree<PgnVariationWithFloatItemsSyntax> { leadingFloatItems, variation };

        private static ParseTree<PgnMoveNumberWithFloatItemsSyntax> NoFloats(ParseTree<PgnMoveNumberWithTriviaSyntax> moveNumberWithTrivia)
            => WithFloats(EmptyFloatItems, moveNumberWithTrivia);

        private static ParseTree<PgnMoveWithFloatItemsSyntax> NoFloats(ParseTree<PgnMoveWithTriviaSyntax> moveWithTrivia)
            => WithFloats(EmptyFloatItems, moveWithTrivia);

        private static ParseTree<PgnNagWithFloatItemsSyntax> NoFloats(ParseTree<PgnNagWithTriviaSyntax> nagWithTrivia)
            => WithFloats(EmptyFloatItems, nagWithTrivia);

        private static ParseTree<PgnVariationWithFloatItemsSyntax> NoFloats(ParseTree<PgnVariationSyntax> variation)
            => WithFloats(EmptyFloatItems, variation);

        private static readonly ParseTree<PgnMoveNumberWithFloatItemsSyntax> MoveNumberNoFloats = NoFloats(MoveNumberNoTrivia);
        private static readonly ParseTree<PgnMoveWithFloatItemsSyntax> MoveNoFloats = NoFloats(MoveNoTrivia);
        private static readonly ParseTree<PgnNagWithFloatItemsSyntax> NAGNoFloats = NoFloats(NAGNoTrivia);

        private static readonly ParseTree<PgnMoveNumberWithFloatItemsSyntax> WS_MoveNumberNoFloats = NoFloats(WS_MoveNumber);
        private static readonly ParseTree<PgnMoveWithFloatItemsSyntax> WS_MoveNoFloats = NoFloats(WS_Move);
        private static readonly ParseTree<PgnNagWithFloatItemsSyntax> WS_NAGNoFloats = NoFloats(WS_NAG);

        private static readonly ParseTree<PgnParenthesisOpenSyntax> Open = new ParseTree<PgnParenthesisOpenSyntax>();
        private static readonly ParseTree<PgnParenthesisOpenWithTriviaSyntax> OpenNoTrivia = new ParseTree<PgnParenthesisOpenWithTriviaSyntax> { EmptyTrivia, Open };
        private static readonly ParseTree<PgnParenthesisOpenWithTriviaSyntax> WS_Open = new ParseTree<PgnParenthesisOpenWithTriviaSyntax> { WhitespaceTrivia, Open };

        private static readonly ParseTree<PgnParenthesisCloseSyntax> Close = new ParseTree<PgnParenthesisCloseSyntax>();
        private static readonly ParseTree<PgnParenthesisCloseWithTriviaSyntax> CloseNoTrivia = new ParseTree<PgnParenthesisCloseWithTriviaSyntax> { EmptyTrivia, Close };
        private static readonly ParseTree<PgnParenthesisCloseWithTriviaSyntax> WS_Close = new ParseTree<PgnParenthesisCloseWithTriviaSyntax> { WhitespaceTrivia, Close };

        private static ParseTree<PgnPlySyntax> Ply(
            ParseTree<PgnMoveNumberWithFloatItemsSyntax> moveNumber,
            ParseTree<PgnMoveWithFloatItemsSyntax> move,
            ParseTree<PgnNagWithFloatItemsSyntax>[] nags,
            ParseTree<PgnVariationWithFloatItemsSyntax>[] variations)
        {
            var plySyntax = new ParseTree<PgnPlySyntax>();
            if (moveNumber != null) plySyntax.Add(moveNumber); else plySyntax.Add(Missing);
            if (move != null) plySyntax.Add(move); else plySyntax.Add(Missing);
            if (nags != null) nags.ForEach(plySyntax.Add);
            if (variations != null) variations.ForEach(plySyntax.Add);
            return plySyntax;
        }

        private static ParseTree<PgnPlySyntax> Ply(
            ParseTree<PgnMoveNumberWithFloatItemsSyntax> moveNumber)
            => Ply(moveNumber, null, null, null);

        private static ParseTree<PgnPlySyntax> Ply(
            ParseTree<PgnMoveWithFloatItemsSyntax> move)
            => Ply(null, move, null, null);

        private static ParseTree<PgnPlySyntax> Ply(
            params ParseTree<PgnNagWithFloatItemsSyntax>[] nags)
            => Ply(null, null, nags, null);

        private static ParseTree<PgnPlySyntax> Ply(
            params ParseTree<PgnVariationWithFloatItemsSyntax>[] variations)
            => Ply(null, null, null, variations);

        private static ParseTree<PgnPlySyntax> Ply(
            ParseTree<PgnMoveNumberWithFloatItemsSyntax> moveNumber,
            ParseTree<PgnMoveWithFloatItemsSyntax> move)
            => Ply(moveNumber, move, null, null);

        private static ParseTree<PgnPlySyntax> Ply(
            ParseTree<PgnMoveNumberWithFloatItemsSyntax> moveNumber,
            ParseTree<PgnNagWithFloatItemsSyntax> nag)
            => Ply(moveNumber, null, new[] { nag }, null);

        private static ParseTree<PgnPlySyntax> Ply(
            ParseTree<PgnMoveNumberWithFloatItemsSyntax> moveNumber,
            ParseTree<PgnVariationWithFloatItemsSyntax> variation)
            => Ply(moveNumber, null, null, new[] { variation });

        private static ParseTree<PgnPlySyntax> Ply(
            ParseTree<PgnMoveWithFloatItemsSyntax> move,
            ParseTree<PgnNagWithFloatItemsSyntax> nag)
            => Ply(null, move, new[] { nag }, null);

        private static ParseTree<PgnPlySyntax> Ply(
            ParseTree<PgnMoveWithFloatItemsSyntax> move,
            ParseTree<PgnVariationWithFloatItemsSyntax> variation)
            => Ply(null, move, null, new[] { variation });

        private static ParseTree<PgnPlySyntax> Ply(
            ParseTree<PgnNagWithFloatItemsSyntax> nag,
            ParseTree<PgnVariationWithFloatItemsSyntax> variation)
            => Ply(null, null, new[] { nag }, new[] { variation });

        private static ParseTree<PgnPlySyntax> Ply(
            ParseTree<PgnMoveNumberWithFloatItemsSyntax> moveNumber,
            ParseTree<PgnMoveWithFloatItemsSyntax> move,
            ParseTree<PgnNagWithFloatItemsSyntax> nag)
            => Ply(moveNumber, move, new[] { nag }, null);

        private static ParseTree<PgnPlySyntax> Ply(
            ParseTree<PgnMoveNumberWithFloatItemsSyntax> moveNumber,
            ParseTree<PgnMoveWithFloatItemsSyntax> move,
            ParseTree<PgnVariationWithFloatItemsSyntax> variation1)
            => Ply(moveNumber, move, null, new[] { variation1 });

        private static ParseTree<PgnPlySyntax> PlyVariations(
            ParseTree<PgnMoveNumberWithFloatItemsSyntax> moveNumber,
            ParseTree<PgnMoveWithFloatItemsSyntax> move,
            ParseTree<PgnVariationWithFloatItemsSyntax> variation1,
            ParseTree<PgnVariationWithFloatItemsSyntax> variation2)
            => Ply(moveNumber, move, null, new[] { variation1, variation2 });

        private static ParseTree<PgnPlySyntax> Ply(
            ParseTree<PgnMoveNumberWithFloatItemsSyntax> moveNumber,
            ParseTree<PgnNagWithFloatItemsSyntax> nag,
            ParseTree<PgnVariationWithFloatItemsSyntax> variation)
            => Ply(moveNumber, null, new[] { nag }, new[] { variation });

        private static ParseTree<PgnPlySyntax> Ply(
            ParseTree<PgnMoveWithFloatItemsSyntax> move,
            ParseTree<PgnNagWithFloatItemsSyntax> nag,
            ParseTree<PgnVariationWithFloatItemsSyntax> variation)
            => Ply(null, move, new[] { nag }, new[] { variation });

        private static ParseTree<PgnPlyListSyntax> PliesTrailingFloatItems(
            ParseTree<PgnPlyFloatItemListSyntax> trailingFloatItems,
            params ParseTree<PgnPlySyntax>[] plies)
        {
            var plyListSyntax = new ParseTree<PgnPlyListSyntax>();
            plies.ForEach(plyListSyntax.Add);
            plyListSyntax.Add(trailingFloatItems);
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

        private static readonly ParseTree<PgnPlyListSyntax> NoPlies = new ParseTree<PgnPlyListSyntax> { EmptyFloatItems };

        private static ParseTree<PgnVariationSyntax> Variation(
            ParseTree<PgnParenthesisOpenWithTriviaSyntax> open,
            ParseTree<PgnPlyListSyntax> plies)
            => new ParseTree<PgnVariationSyntax> { open, plies, Missing };

        private static ParseTree<PgnVariationSyntax> Variation(
            ParseTree<PgnParenthesisOpenWithTriviaSyntax> open,
            ParseTree<PgnPlyListSyntax> plies,
            ParseTree<PgnParenthesisCloseWithTriviaSyntax> close)
            => new ParseTree<PgnVariationSyntax> { open, plies, close };

        private static ParseTree<PgnVariationWithFloatItemsSyntax> VariationNoFloats(
            ParseTree<PgnParenthesisOpenWithTriviaSyntax> open,
            ParseTree<PgnPlyListSyntax> plies)
            => NoFloats(Variation(open, plies));

        private static ParseTree<PgnVariationWithFloatItemsSyntax> VariationNoFloats(
            ParseTree<PgnParenthesisOpenWithTriviaSyntax> open,
            ParseTree<PgnPlyListSyntax> plies,
            ParseTree<PgnParenthesisCloseWithTriviaSyntax> close)
            => NoFloats(Variation(open, plies, close));

        private static readonly ParseTree<PgnVariationWithFloatItemsSyntax> VariationOpenClose
            = VariationNoFloats(OpenNoTrivia, NoPlies, CloseNoTrivia);

        private static ParseTree<PgnGameSyntax> Game(
            ParseTree<PgnGameResultWithTriviaSyntax> result)
            => new ParseTree<PgnGameSyntax> { EmptyTagSection, NoPlies, result };

        private static ParseTree<PgnGameSyntax> Game(
            ParseTree<PgnTagSectionSyntax> tagSection,
            ParseTree<PgnPlyListSyntax> moveSection)
            => new ParseTree<PgnGameSyntax> { tagSection, moveSection, Missing };

        private static ParseTree<PgnGameSyntax> Game(
            ParseTree<PgnTagSectionSyntax> tagSection,
            ParseTree<PgnPlyListSyntax> moveSection,
            ParseTree<PgnGameResultWithTriviaSyntax> result)
            => new ParseTree<PgnGameSyntax> { tagSection, moveSection, result };

        private static ParseTree<PgnGameListSyntax> Games(ParseTree<PgnGameSyntax> game1, ParseTree<PgnTriviaSyntax> trailingTrivia)
            => new ParseTree<PgnGameListSyntax> { game1, trailingTrivia };

        private static ParseTree<PgnGameListSyntax> Games(ParseTree<PgnGameSyntax> game1, ParseTree<PgnGameSyntax> game2, ParseTree<PgnTriviaSyntax> trailingTrivia)
            => new ParseTree<PgnGameListSyntax> { game1, game2, trailingTrivia };

        private static ParseTree<PgnGameListSyntax> Games(params ParseTree<PgnGameSyntax>[] games)
        {
            var gamesSyntax = new ParseTree<PgnGameListSyntax>();
            games.ForEach(gamesSyntax.Add);
            gamesSyntax.Add(EmptyTrivia);
            return gamesSyntax;
        }

        private static ParseTree<PgnGameListSyntax> OneGameTrailingTrivia(
            ParseTree<PgnTagSectionSyntax> tagSection,
            ParseTree<PgnPlyListSyntax> moveSection,
            ParseTree<PgnTriviaSyntax> trailingTrivia)
            => Games(Game(tagSection, moveSection), trailingTrivia);

        private static ParseTree<PgnGameListSyntax> OneGame(
            ParseTree<PgnTagSectionSyntax> tagSection,
            ParseTree<PgnPlyListSyntax> moveSection)
            => Games(Game(tagSection, moveSection));

        private static ParseTree<PgnGameListSyntax> OneGame(
            ParseTree<PgnTagSectionSyntax> tagSection,
            ParseTree<PgnPlyListSyntax> moveSection,
            ParseTree<PgnGameResultWithTriviaSyntax> result)
            => Games(Game(tagSection, moveSection, result));

        private static ParseTree<PgnGameListSyntax> TagSectionOnly(params ParseTree<PgnTagPairSyntax>[] tagPairs)
            => OneGame(TagSection(tagPairs), NoPlies);

        internal static readonly List<(string, ParseTree)> TestParseTrees
            = TriviaParseTrees()
            .ToList();

        internal static readonly List<(string, ParseTree, PgnErrorCode[])> TestParseTreesWithErrors
            = TriviaParseTreesWithErrors()
            .Union(TagSectionParseTreesWithErrors())
            .Union(PlyParseTreesWithErrors())
            .Union(MoveTreeParseTreesWithErrors())
            .Union(MiscParseTrees())
            .ToList();
    }
}

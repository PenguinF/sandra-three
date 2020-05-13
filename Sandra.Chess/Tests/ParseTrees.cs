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

        private class LeadingFloatItemsAndMoveNumber
        {
            public readonly List<ParseTree<PgnPeriodWithTriviaSyntax>> LeadingFloatItems;
            public readonly ParseTree<PgnMoveNumberWithTriviaSyntax> MoveNumberWithTrivia;

            public LeadingFloatItemsAndMoveNumber(List<ParseTree<PgnPeriodWithTriviaSyntax>> leadingFloatItems, ParseTree<PgnMoveNumberWithTriviaSyntax> moveNumberWithTrivia)
            {
                LeadingFloatItems = leadingFloatItems;
                MoveNumberWithTrivia = moveNumberWithTrivia;
            }
        }

        private class LeadingFloatItemsAndMove
        {
            public readonly List<ParseTree<PgnPeriodWithTriviaSyntax>> LeadingFloatItems;
            public readonly ParseTree<PgnMoveWithTriviaSyntax> MoveWithTrivia;

            public LeadingFloatItemsAndMove(List<ParseTree<PgnPeriodWithTriviaSyntax>> leadingFloatItems, ParseTree<PgnMoveWithTriviaSyntax> moveWithTrivia)
            {
                LeadingFloatItems = leadingFloatItems;
                MoveWithTrivia = moveWithTrivia;
            }
        }

        private class LeadingFloatItemsAndNAG
        {
            public readonly List<ParseTree<PgnPeriodWithTriviaSyntax>> LeadingFloatItems;
            public readonly ParseTree<PgnNagWithTriviaSyntax> NagWithTrivia;

            public LeadingFloatItemsAndNAG(List<ParseTree<PgnPeriodWithTriviaSyntax>> leadingFloatItems, ParseTree<PgnNagWithTriviaSyntax> nagWithTrivia)
            {
                LeadingFloatItems = leadingFloatItems;
                NagWithTrivia = nagWithTrivia;
            }
        }

        private class SinglePly
        {
            public readonly LeadingFloatItemsAndMoveNumber MoveNumber;
            public readonly LeadingFloatItemsAndMove Move;
            public readonly LeadingFloatItemsAndNAG[] Nags;

            public SinglePly(LeadingFloatItemsAndMoveNumber moveNumber, LeadingFloatItemsAndMove move, LeadingFloatItemsAndNAG[] nags)
            {
                MoveNumber = moveNumber;
                Move = move;
                Nags = nags;
            }
        }

        private class MainLineAndTrailingFloats
        {
            public SinglePly[] MainLine;
            public List<ParseTree<PgnPeriodWithTriviaSyntax>> TrailingFloatItems;

            public MainLineAndTrailingFloats(SinglePly[] mainLine, List<ParseTree<PgnPeriodWithTriviaSyntax>> trailingFloatItems)
            {
                MainLine = mainLine;
                TrailingFloatItems = trailingFloatItems;
            }

            public MainLineAndTrailingFloats(SinglePly[] mainLine)
                : this(mainLine, EmptyFloatItems)
            {
            }
        }

        private class TagSectionAndMoveTreeAndResult
        {
            public ParseTree<PgnTagSectionSyntax> TagSection;
            public MainLineAndTrailingFloats MoveSection;
            public ParseTree<PgnGameResultWithTriviaSyntax> Result;

            public TagSectionAndMoveTreeAndResult(ParseTree<PgnTagSectionSyntax> tagSection, MainLineAndTrailingFloats moveSection)
            {
                TagSection = tagSection;
                MoveSection = moveSection;
            }

            public TagSectionAndMoveTreeAndResult(ParseTree<PgnTagSectionSyntax> tagSection, MainLineAndTrailingFloats moveSection, ParseTree<PgnGameResultWithTriviaSyntax> result)
            {
                TagSection = tagSection;
                MoveSection = moveSection;
                Result = result;
            }

            public void AddTo(ParseTree<PgnSyntaxNodes> gamesSyntax)
            {
                if (TagSection.Any()) gamesSyntax.Add(TagSection);

                ParseTree<PgnPlyListSyntax> plyListSyntax = new ParseTree<PgnPlyListSyntax>();
                foreach (SinglePly singlePly in MoveSection.MainLine)
                {
                    ParseTree<PgnPlySyntax> plySyntax = new ParseTree<PgnPlySyntax>();
                    if (singlePly.MoveNumber != null)
                    {
                        ParseTree<PgnMoveNumberWithFloatItemsSyntax> moveNumberSyntax = new ParseTree<PgnMoveNumberWithFloatItemsSyntax>();
                        ParseTree<PgnPlyFloatItemListSyntax> floatItemsSyntax = new ParseTree<PgnPlyFloatItemListSyntax>();
                        singlePly.MoveNumber.LeadingFloatItems.ForEach(floatItemsSyntax.Add);
                        moveNumberSyntax.Add(floatItemsSyntax);
                        moveNumberSyntax.Add(singlePly.MoveNumber.MoveNumberWithTrivia);
                        plySyntax.Add(moveNumberSyntax);
                    }
                    else
                    {
                        plySyntax.Add(Missing);
                    }
                    if (singlePly.Move != null)
                    {
                        ParseTree<PgnMoveWithFloatItemsSyntax> moveSyntax = new ParseTree<PgnMoveWithFloatItemsSyntax>();
                        ParseTree<PgnPlyFloatItemListSyntax> floatItemsSyntax = new ParseTree<PgnPlyFloatItemListSyntax>();
                        singlePly.Move.LeadingFloatItems.ForEach(floatItemsSyntax.Add);
                        moveSyntax.Add(floatItemsSyntax);
                        moveSyntax.Add(singlePly.Move.MoveWithTrivia);
                        plySyntax.Add(moveSyntax);
                    }
                    else
                    {
                        plySyntax.Add(Missing);
                    }
                    foreach (LeadingFloatItemsAndNAG nag in singlePly.Nags)
                    {
                        ParseTree<PgnNagWithFloatItemsSyntax> nagSyntax = new ParseTree<PgnNagWithFloatItemsSyntax>();
                        ParseTree<PgnPlyFloatItemListSyntax> floatItemsSyntax = new ParseTree<PgnPlyFloatItemListSyntax>();
                        nag.LeadingFloatItems.ForEach(floatItemsSyntax.Add);
                        nagSyntax.Add(floatItemsSyntax);
                        nagSyntax.Add(nag.NagWithTrivia);
                        plySyntax.Add(nagSyntax);
                    }
                    plyListSyntax.Add(plySyntax);
                }

                if (plyListSyntax.Any() || MoveSection.TrailingFloatItems.Any())
                {
                    ParseTree<PgnPlyFloatItemListSyntax> trailingFloatItemsSyntax = new ParseTree<PgnPlyFloatItemListSyntax>();
                    MoveSection.TrailingFloatItems.ForEach(trailingFloatItemsSyntax.Add);
                    plyListSyntax.Add(trailingFloatItemsSyntax);
                }

                if (plyListSyntax.Any()) gamesSyntax.Add(plyListSyntax);

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

        private static readonly List<ParseTree<PgnPeriodWithTriviaSyntax>> EmptyFloatItems
            = new List<ParseTree<PgnPeriodWithTriviaSyntax>>();

        private static readonly List<ParseTree<PgnPeriodWithTriviaSyntax>> OnePeriod
            = new List<ParseTree<PgnPeriodWithTriviaSyntax>> { PeriodNoTrivia };

        private static readonly List<ParseTree<PgnPeriodWithTriviaSyntax>> TwoPeriods
            = new List<ParseTree<PgnPeriodWithTriviaSyntax>> { PeriodNoTrivia, PeriodNoTrivia };

        private static LeadingFloatItemsAndMoveNumber WithFloats(
            List<ParseTree<PgnPeriodWithTriviaSyntax>> leadingFloatItems,
            ParseTree<PgnMoveNumberWithTriviaSyntax> moveNumberWithTrivia)
            => new LeadingFloatItemsAndMoveNumber(leadingFloatItems, moveNumberWithTrivia);

        private static LeadingFloatItemsAndMove WithFloats(
            List<ParseTree<PgnPeriodWithTriviaSyntax>> leadingFloatItems,
            ParseTree<PgnMoveWithTriviaSyntax> moveWithTrivia)
            => new LeadingFloatItemsAndMove(leadingFloatItems, moveWithTrivia);

        private static LeadingFloatItemsAndNAG WithFloats(
            List<ParseTree<PgnPeriodWithTriviaSyntax>> leadingFloatItems,
            ParseTree<PgnNagWithTriviaSyntax> nagWithTrivia)
            => new LeadingFloatItemsAndNAG(leadingFloatItems, nagWithTrivia);

        private static LeadingFloatItemsAndMoveNumber NoFloats(ParseTree<PgnMoveNumberWithTriviaSyntax> moveNumberWithTrivia)
            => WithFloats(EmptyFloatItems, moveNumberWithTrivia);

        private static LeadingFloatItemsAndMove NoFloats(ParseTree<PgnMoveWithTriviaSyntax> moveWithTrivia)
            => WithFloats(EmptyFloatItems, moveWithTrivia);

        private static LeadingFloatItemsAndNAG NoFloats(ParseTree<PgnNagWithTriviaSyntax> nagWithTrivia)
            => WithFloats(EmptyFloatItems, nagWithTrivia);

        private static readonly LeadingFloatItemsAndMoveNumber MoveNumberNoFloats = NoFloats(MoveNumberNoTrivia);
        private static readonly LeadingFloatItemsAndMove MoveNoFloats = NoFloats(MoveNoTrivia);
        private static readonly LeadingFloatItemsAndNAG NAGNoFloats = NoFloats(NAGNoTrivia);

        private static readonly LeadingFloatItemsAndMoveNumber WS_MoveNumberNoFloats = NoFloats(WS_MoveNumber);
        private static readonly LeadingFloatItemsAndMove WS_MoveNoFloats = NoFloats(WS_Move);
        private static readonly LeadingFloatItemsAndNAG WS_NAGNoFloats = NoFloats(WS_NAG);

        private static SinglePly Ply(
            LeadingFloatItemsAndMoveNumber moveNumber,
            LeadingFloatItemsAndMove move,
            params LeadingFloatItemsAndNAG[] nags)
            => new SinglePly(moveNumber, move, nags);

        private static SinglePly Ply(
            LeadingFloatItemsAndMoveNumber moveNumber,
            params LeadingFloatItemsAndNAG[] nags)
            => Ply(moveNumber, null, nags);

        private static SinglePly Ply(
            LeadingFloatItemsAndMove move,
            params LeadingFloatItemsAndNAG[] nags)
            => Ply(null, move, nags);

        private static SinglePly Ply(
            params LeadingFloatItemsAndNAG[] nags)
            => Ply(null, null, nags);

        private static MainLineAndTrailingFloats PliesTrailingFloatItems(
            List<ParseTree<PgnPeriodWithTriviaSyntax>> trailingFloatItems,
            params SinglePly[] plies)
            => new MainLineAndTrailingFloats(plies, trailingFloatItems);

        private static MainLineAndTrailingFloats Plies(
            List<ParseTree<PgnPeriodWithTriviaSyntax>> trailingFloatItems)
            => new MainLineAndTrailingFloats(Array.Empty<SinglePly>(), trailingFloatItems);

        private static MainLineAndTrailingFloats Plies(
            SinglePly ply1,
            List<ParseTree<PgnPeriodWithTriviaSyntax>> trailingFloatItems)
            => new MainLineAndTrailingFloats(new SinglePly[] { ply1 }, trailingFloatItems);

        private static MainLineAndTrailingFloats Plies(params SinglePly[] plies)
            => PliesTrailingFloatItems(EmptyFloatItems, plies);

        private static readonly MainLineAndTrailingFloats NoPlies = Plies();

        private static TagSectionAndMoveTreeAndResult Game(
            ParseTree<PgnGameResultWithTriviaSyntax> result)
            => new TagSectionAndMoveTreeAndResult(EmptyTagSection, NoPlies, result);

        private static TagSectionAndMoveTreeAndResult Game(
            ParseTree<PgnTagSectionSyntax> tagSection,
            MainLineAndTrailingFloats moveSection)
            => new TagSectionAndMoveTreeAndResult(tagSection, moveSection);

        private static TagSectionAndMoveTreeAndResult Game(
            ParseTree<PgnTagSectionSyntax> tagSection,
            MainLineAndTrailingFloats moveSection,
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
            MainLineAndTrailingFloats moveSection,
            ParseTree<PgnTriviaSyntax> trailingTrivia)
            => Games(Game(tagSection, moveSection), trailingTrivia);

        private static ParseTree<PgnSyntaxNodes> OneGame(
            ParseTree<PgnTagSectionSyntax> tagSection,
            MainLineAndTrailingFloats moveSection)
            => Games(Game(tagSection, moveSection));

        private static ParseTree<PgnSyntaxNodes> OneGame(
            ParseTree<PgnTagSectionSyntax> tagSection,
            MainLineAndTrailingFloats moveSection,
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

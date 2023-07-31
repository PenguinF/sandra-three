#region License
/*********************************************************************************
 * PgnParser.cs
 *
 * Copyright (c) 2004-2023 Henk Nicolai
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

using Eutherion.Text;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Partitions a source PGN file into separate tokens, then generates an abstract syntax tree from it.
    /// </summary>
    public sealed class PgnParser
    {
        private class VariationStackFrame
        {
            // Saved parenthesis open of the current recursive variation, including leading trivia.
            public GreenWithTriviaSyntax SavedParenthesisOpenWithTrivia;

            // Whether to report a missing move number.
            public bool HasPly;

            // Current ply being built.
            public GreenWithPlyFloatItemsSyntax<GreenWithTriviaSyntax> MoveNumber;
            public GreenWithPlyFloatItemsSyntax<GreenWithTriviaSyntax> Move;

            // Builds list of floating items within the current ply.
            public List<GreenWithTriviaSyntax> FloatItemListBuilder;

            // Builds list of NAGs of the current ply.
            public List<GreenWithPlyFloatItemsSyntax<GreenWithTriviaSyntax>> NagListBuilder;

            // Builds list of variations after the current ply.
            public List<GreenWithPlyFloatItemsSyntax<GreenPgnVariationSyntax>> VariationListBuilder;

            // List of already built plies in this variation.
            public List<GreenPgnPlySyntax> PlyListBuilder;

            public VariationStackFrame()
            {
                FloatItemListBuilder = new List<GreenWithTriviaSyntax>();
                NagListBuilder = new List<GreenWithPlyFloatItemsSyntax<GreenWithTriviaSyntax>>();
                VariationListBuilder = new List<GreenWithPlyFloatItemsSyntax<GreenPgnVariationSyntax>>();
                PlyListBuilder = new List<GreenPgnPlySyntax>();
            }
        }

        /// <summary>
        /// See also <see cref="CStyleStringLiteral.EscapeCharacter"/>.
        /// </summary>
        private static readonly string EscapeCharacterString = "\\";

        /// <summary>
        /// Parses source text in the PGN format.
        /// </summary>
        /// <param name="pgn">
        /// The source text to parse.
        /// </param>
        /// <returns>
        /// A <see cref="RootPgnSyntax"/> containing the parse syntax tree and parse errors.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="pgn"/> is null.
        /// </exception>
        public static RootPgnSyntax Parse(string pgn)
        {
            if (pgn == null) throw new ArgumentNullException(nameof(pgn));

            var parser = new PgnParser(pgn);
            parser.ParsePgnText();
            return new RootPgnSyntax(parser.YieldEof(), ReadOnlyList<PgnErrorInfo>.FromBuilder(parser.Errors));
        }

        private readonly ArrayBuilder<PgnErrorInfo> Errors;
        private readonly ArrayBuilder<GreenPgnBackgroundSyntax> BackgroundBuilder;
        private readonly ArrayBuilder<GreenPgnTriviaElementSyntax> TriviaBuilder;
        private readonly ArrayBuilder<GreenWithTriviaSyntax> TagPairBuilder;
        private readonly ArrayBuilder<GreenPgnTagPairSyntax> TagSectionBuilder;
        private readonly Stack<VariationStackFrame> VariationBuilderStack;
        private readonly ArrayBuilder<GreenPgnGameSyntax> GameListBuilder;

        private readonly string pgnText;

        // Invariant is that this index is always at the start of the yielded symbol.
        private int symbolStartIndex;

        // Save as fields, which is useful for error reporting.
        private GreenWithTriviaSyntax symbolBeingYielded;
        private GreenPgnTriviaSyntax trailingTrivia;

        private bool InTagPair;
        private bool HasTagPairBracketOpen;
        private bool HasTagPairTagName;
        private bool HasTagPairTagValue;

        // Saved in case tag elements are found in a move tree and it's yet undecided whether or not to switch to a new tag section.
        // Start positions must be saved if errors on the saved symbols must still be reported.
        private int savedBracketOpenStartPosition;
        private GreenWithTriviaSyntax savedBracketOpen;
        private int savedTagNameStartPosition;
        private GreenWithTriviaSyntax savedTagName;

        // Saved until an entire game is captured.
        private GreenPgnTagSectionSyntax LatestTagSection;

        // Contains stack frame for the current variation (main or side line) being built.
        private VariationStackFrame CurrentFrame;

        // All content node yielders. They depend on the position in the parse tree, i.e. the current parser state.
        private readonly Action YieldInTagSectionAction;
        private readonly Action YieldInMoveTreeSectionAction;
        private readonly Action YieldAfterBracketOpenInMoveTreeSectionAction;
        private readonly Action YieldAfterTagNameInMoveTreeSectionAction;

        // This is either YieldInTagSectionAction or YieldInMoveTreeSectionAction.
        // It is important that this action is an instance method, since invocation of such delegates is the fastest.
        private Action YieldContentNode;

        private PgnParser(string pgnText)
        {
            this.pgnText = pgnText;

            Errors = new ArrayBuilder<PgnErrorInfo>();
            BackgroundBuilder = new ArrayBuilder<GreenPgnBackgroundSyntax>();
            TriviaBuilder = new ArrayBuilder<GreenPgnTriviaElementSyntax>();
            TagPairBuilder = new ArrayBuilder<GreenWithTriviaSyntax>();
            TagSectionBuilder = new ArrayBuilder<GreenPgnTagPairSyntax>();
            VariationBuilderStack = new Stack<VariationStackFrame>();
            GameListBuilder = new ArrayBuilder<GreenPgnGameSyntax>();

            LatestTagSection = GreenPgnTagSectionSyntax.Empty;

            CurrentFrame = new VariationStackFrame();

            YieldInTagSectionAction = YieldInTagSection;
            YieldInMoveTreeSectionAction = YieldInMoveTreeSection;
            YieldAfterBracketOpenInMoveTreeSectionAction = YieldAfterBracketOpenInMoveTreeSection;
            YieldAfterTagNameInMoveTreeSectionAction = YieldAfterTagNameInMoveTreeSection;

            YieldContentNode = YieldInTagSectionAction;
        }

        #region Conversions from one type of symbol to another

        private GreenWithTriviaSyntax ConvertToOrphanParenthesisClose(GreenWithTriviaSyntax parenthesisClose)
            => new GreenWithTriviaSyntax(
                parenthesisClose.LeadingTrivia,
                GreenPgnOrphanParenthesisCloseSyntax.Value);

        private GreenWithTriviaSyntax ConvertToTagName(GreenWithTriviaSyntax moveSyntax)
            => new GreenWithTriviaSyntax(
                moveSyntax.LeadingTrivia,
                new GreenPgnTagNameSyntax(moveSyntax.ContentNode.Length, isConvertedFromMove: true));

        private GreenWithTriviaSyntax ConvertToTagElementInMoveTree(GreenWithTriviaSyntax tagElementSyntax)
            => new GreenWithTriviaSyntax(
                tagElementSyntax.LeadingTrivia,
                new GreenPgnTagElementInMoveTreeSyntax(tagElementSyntax.ContentNode));

        private GreenWithTriviaSyntax ConvertToUnrecognizedMove(int startPosition, GreenWithTriviaSyntax tagNameSyntax)
        {
            // Report the error here.
            Errors.Add(PgnMoveSyntax.CreateUnrecognizedMoveError(
                pgnText.Substring(startPosition, tagNameSyntax.ContentNode.Length),
                startPosition));

            return new GreenWithTriviaSyntax(
                tagNameSyntax.LeadingTrivia,
                new GreenPgnUnrecognizedMoveSyntax(tagNameSyntax.ContentNode.Length, isConvertedFromTagName: true));
        }

        #endregion Conversions from one type of symbol to another

        #region Error reporting helpers

        // At the end of the file, symbolBeingYielded is null; return the start position of the trailing trivia.
        // Otherwise, symbolStartIndex is at the end of the leading trivia of symbolBeingYielded.
        private int GetCurrentTriviaStartPosition()
            => symbolBeingYielded != null
            ? symbolStartIndex - symbolBeingYielded.LeadingTrivia.Length
            : pgnText.Length - trailingTrivia.Length;

        private GreenWithTriviaSyntax HeadNode(GreenPgnGameSyntax gameSyntax)
        {
            if (gameSyntax.TagSection.Length > 0) return HeadNode(gameSyntax.TagSection.TagPairNodes[0]);
            else if (gameSyntax.PlyList.Length > 0) return HeadNode(gameSyntax.PlyList);
            else return gameSyntax.GameResult;
        }

        private GreenWithTriviaSyntax HeadNode(GreenPgnVariationSyntax variationSyntax)
            => variationSyntax.ParenthesisOpen;

        private GreenWithTriviaSyntax HeadNode(GreenPgnPlyListSyntax plyListSyntax)
        {
            if (plyListSyntax.Plies.Count > 0) return HeadNode(plyListSyntax.Plies[0]);
            else return plyListSyntax.TrailingFloatItems[0];
        }

        private GreenWithPlyFloatItemsSyntax HeadFloatItemAnchorNode(GreenPgnPlySyntax plySyntax)
        {
            if (plySyntax.MoveNumber != null) return plySyntax.MoveNumber;
            else if (plySyntax.Move != null) return plySyntax.Move;
            else if (plySyntax.Nags.Count > 0) return plySyntax.Nags[0];
            else return plySyntax.Variations[0];
        }

        private GreenWithTriviaSyntax HeadNode(GreenWithPlyFloatItemsSyntax plyAnchorNode)
            => plyAnchorNode.PlyContentNode.FirstWithTriviaNode;

        private GreenWithTriviaSyntax HeadNode(GreenPgnPlySyntax plySyntax)
            => HeadNode(HeadFloatItemAnchorNode(plySyntax));

        private GreenWithTriviaSyntax HeadNode(GreenPgnTagPairSyntax tagPairSyntax)
            => tagPairSyntax.TagElementNodes[0];

        private int GetLengthWithoutLeadingTrivia(GreenPgnGameSyntax gameSyntax)
            => gameSyntax.Length - HeadNode(gameSyntax).LeadingTrivia.Length;

        private int GetLengthWithoutLeadingTrivia(GreenPgnVariationSyntax variationSyntax)
            => variationSyntax.Length - HeadNode(variationSyntax).LeadingTrivia.Length;

        private int GetLengthWithoutLeadingFloatsAndTrivia(GreenPgnPlySyntax plySyntax)
        {
            // Subtract both the leading float items length plus leading trivia length.
            GreenWithPlyFloatItemsSyntax headAnchorNode = HeadFloatItemAnchorNode(plySyntax);
            return plySyntax.Length - headAnchorNode.LeadingFloatItems.Length - HeadNode(headAnchorNode).LeadingTrivia.Length;
        }

        private int GetLengthWithoutLeadingTrivia(GreenPgnTagPairSyntax tagPairSyntax)
            => tagPairSyntax.Length - HeadNode(tagPairSyntax).LeadingTrivia.Length;

        #endregion Error reporting helpers

        #region Game parsing

        private void CaptureGame(GreenPgnPlyListSyntax plyListSyntax, GreenWithTriviaSyntax maybeGameResult)
        {
            // Reset HasPly for the next game.
            CurrentFrame.HasPly = false;

            var gameSyntax = new GreenPgnGameSyntax(LatestTagSection, plyListSyntax, maybeGameResult);
            LatestTagSection = GreenPgnTagSectionSyntax.Empty;
            GameListBuilder.Add(gameSyntax);

            if (gameSyntax.TagSection.Length == 0 || maybeGameResult == null)
            {
                // The end position of the game is before the latest trivia.
                int gameEndPosition
                    = maybeGameResult != null ? symbolStartIndex + maybeGameResult.ContentNode.Length
                    : GetCurrentTriviaStartPosition();

                // Subtract the length of the trivia before the game.
                int gameLength = GetLengthWithoutLeadingTrivia(gameSyntax);

                if (gameSyntax.TagSection.Length == 0)
                {
                    Errors.Add(new PgnErrorInfo(
                        PgnErrorCode.MissingTagSection,
                        gameEndPosition - gameLength,
                        gameLength));
                }

                if (maybeGameResult == null)
                {
                    Errors.Add(new PgnErrorInfo(
                        PgnErrorCode.MissingGameTerminationMarker,
                        gameEndPosition - gameLength,
                        gameLength));
                }
            }
        }

        private void CaptureMainLine(GreenWithTriviaSyntax maybeGameResult)
        {
            bool innermostVariation = true;

            while (VariationBuilderStack.Count > 0)
            {
                var variationSyntax = CaptureVariation(null);

                if (innermostVariation)
                {
                    // Report MissingParenthesisClose only once, for the innermost variation.
                    int variationEndPosition = GetCurrentTriviaStartPosition();
                    int variationLength = GetLengthWithoutLeadingTrivia(variationSyntax);

                    Errors.Add(new PgnErrorInfo(
                        PgnErrorCode.MissingParenthesisClose,
                        variationEndPosition - variationLength,
                        variationLength));

                    innermostVariation = false;
                }

                // Capture floating items from before the variation, then add to VariationListBuilder.
                CurrentFrame.VariationListBuilder.Add(new GreenWithPlyFloatItemsSyntax<GreenPgnVariationSyntax>(CaptureFloatItems(), variationSyntax));
            }

            CaptureGame(CapturePlyList(CapturePly()), maybeGameResult);
        }

        #endregion Game parsing

        #region Variation parsing

        private GreenPgnVariationSyntax CaptureVariation(GreenWithTriviaSyntax maybeParenthesisClose)
        {
            var trailingFloatItems = CapturePly();
            bool reportEmptyVariationMessage = !CurrentFrame.HasPly;
            var plyListSyntax = CapturePlyList(trailingFloatItems);

            // Parent stack frame contains the saved parenthesis open.
            CurrentFrame = VariationBuilderStack.Pop();

            var variationSyntax = new GreenPgnVariationSyntax(
                CurrentFrame.SavedParenthesisOpenWithTrivia,
                plyListSyntax,
                maybeParenthesisClose);

            CurrentFrame.SavedParenthesisOpenWithTrivia = null;

            if (reportEmptyVariationMessage)
            {
                // The end position of the variation is at the end of the closing parenthesis,
                // or at the start of the current trivia.
                int variationEndPosition
                    = maybeParenthesisClose != null ? symbolStartIndex + PgnParenthesisCloseSyntax.ParenthesisCloseLength
                    : GetCurrentTriviaStartPosition();

                int variationLength = GetLengthWithoutLeadingTrivia(variationSyntax);

                Errors.Add(new PgnErrorInfo(
                    PgnErrorCode.EmptyVariation,
                    variationEndPosition - variationLength,
                    variationLength));
            }

            return variationSyntax;
        }

        private GreenPgnPlyListSyntax CapturePlyList(ReadOnlySpanList<GreenWithTriviaSyntax> trailingFloatItems)
        {
            var plyListSyntax = GreenPgnPlyListSyntax.Create(CurrentFrame.PlyListBuilder, trailingFloatItems);
            CurrentFrame.PlyListBuilder.Clear();
            return plyListSyntax;
        }

        private void YieldParenthesisOpen()
        {
            CurrentFrame.SavedParenthesisOpenWithTrivia = symbolBeingYielded;
            VariationBuilderStack.Push(CurrentFrame);

            // Initialize new frame.
            CurrentFrame = new VariationStackFrame();
        }

        private void YieldParenthesisClose()
        {
            if (VariationBuilderStack.Count > 0)
            {
                // Must call CaptureVariation before calling CaptureFloatItems.
                var variationSyntax = CaptureVariation(symbolBeingYielded);

                // Capture floating items from before the variation, then add to VariationListBuilder.
                CurrentFrame.VariationListBuilder.Add(new GreenWithPlyFloatItemsSyntax<GreenPgnVariationSyntax>(CaptureFloatItems(), variationSyntax));
            }
            else
            {
                YieldOrphanParenthesisClose();
            }
        }

        #endregion Variation parsing

        #region Ply parsing

        private void CapturePlyUnchecked(int trailingFloatItemsLength)
        {
            var plySyntax = new GreenPgnPlySyntax(CurrentFrame.MoveNumber, CurrentFrame.Move, CurrentFrame.NagListBuilder, CurrentFrame.VariationListBuilder);

            if (!CurrentFrame.HasPly && CurrentFrame.MoveNumber == null || CurrentFrame.Move == null)
            {
                // The captured ply including its trailing floating items ends at the start of the current trivia.
                // So we need to subtract the length of the floating items that trail the captured ply.
                int plyEndPosition = GetCurrentTriviaStartPosition() - trailingFloatItemsLength;
                int plyLength = GetLengthWithoutLeadingFloatsAndTrivia(plySyntax);
                int plyStartPosition = plyEndPosition - plyLength;

                if (!CurrentFrame.HasPly && CurrentFrame.MoveNumber == null)
                {
                    // Only report missing move number for the first ply.
                    Errors.Add(new PgnErrorInfo(
                        PgnErrorCode.MissingMoveNumber,
                        plyStartPosition,
                        plyLength));
                }

                if (CurrentFrame.Move == null)
                {
                    Errors.Add(new PgnErrorInfo(
                        PgnErrorCode.MissingMove,
                        plyStartPosition,
                        plyLength));
                }
            }

            CurrentFrame.MoveNumber = null;
            CurrentFrame.Move = null;
            CurrentFrame.NagListBuilder.Clear();
            CurrentFrame.VariationListBuilder.Clear();

            CurrentFrame.HasPly = true;
            CurrentFrame.PlyListBuilder.Add(plySyntax);
        }

        private ReadOnlySpanList<GreenWithTriviaSyntax> CapturePly()
        {
            ReadOnlySpanList<GreenWithTriviaSyntax> trailingFloatItems = CaptureFloatItems();

            if (CurrentFrame.MoveNumber != null
                || CurrentFrame.Move != null
                || CurrentFrame.NagListBuilder.Count > 0
                || CurrentFrame.VariationListBuilder.Count > 0)
            {
                CapturePlyUnchecked(trailingFloatItems.Length);
            }

            return trailingFloatItems;
        }

        private ReadOnlySpanList<GreenWithTriviaSyntax> CaptureFloatItems()
        {
            var floatItems = ReadOnlySpanList<GreenWithTriviaSyntax>.Create(CurrentFrame.FloatItemListBuilder);
            CurrentFrame.FloatItemListBuilder.Clear();
            return floatItems;
        }

        private void YieldMoveNumber(ReadOnlySpanList<GreenWithTriviaSyntax> leadingFloatItems)
        {
            CurrentFrame.MoveNumber = new GreenWithPlyFloatItemsSyntax<GreenWithTriviaSyntax>(leadingFloatItems, symbolBeingYielded);
        }

        private void YieldPeriod()
        {
            // Report orphan period if not in between move number and move.
            if (CurrentFrame.MoveNumber == null || CurrentFrame.Move != null)
            {
                Errors.Add(new PgnErrorInfo(
                    PgnErrorCode.OrphanPeriod,
                    symbolStartIndex,
                    PgnPeriodSyntax.PeriodLength));
            }

            CurrentFrame.FloatItemListBuilder.Add(symbolBeingYielded);
        }

        private void YieldMove(ReadOnlySpanList<GreenWithTriviaSyntax> leadingFloatItems)
        {
            CurrentFrame.Move = new GreenWithPlyFloatItemsSyntax<GreenWithTriviaSyntax>(leadingFloatItems, symbolBeingYielded);
        }

        private void YieldNag(ReadOnlySpanList<GreenWithTriviaSyntax> leadingFloatItems)
        {
            CurrentFrame.NagListBuilder.Add(new GreenWithPlyFloatItemsSyntax<GreenWithTriviaSyntax>(leadingFloatItems, symbolBeingYielded));
        }

        private void YieldOrphanParenthesisClose()
        {
            Errors.Add(new PgnErrorInfo(
                PgnErrorCode.OrphanParenthesisClose,
                symbolStartIndex,
                PgnParenthesisCloseSyntax.ParenthesisCloseLength));

            CurrentFrame.FloatItemListBuilder.Add(ConvertToOrphanParenthesisClose(symbolBeingYielded));
        }

        private void YieldTagElementInMoveTree(int tagElementStartPosition, GreenWithTriviaSyntax tagElementInMoveTree)
        {
            PgnErrorCode errorCode;

            switch (tagElementInMoveTree.ContentNode.SymbolType)
            {
                case PgnSymbolType.BracketOpen:
                    errorCode = PgnErrorCode.OrphanBracketOpen;
                    break;
                case PgnSymbolType.BracketClose:
                    errorCode = PgnErrorCode.OrphanBracketClose;
                    break;
                case PgnSymbolType.TagValue:
                case PgnSymbolType.ErrorTagValue:
                    errorCode = PgnErrorCode.OrphanTagValue;
                    break;
                default:
                    throw new UnreachableException();
            }

            Errors.Add(new PgnErrorInfo(
                errorCode,
                tagElementStartPosition,
                tagElementInMoveTree.ContentNode.Length));

            CurrentFrame.FloatItemListBuilder.Add(ConvertToTagElementInMoveTree(tagElementInMoveTree));
        }

        #endregion Ply parsing

        #region Tag section parsing

        private void CaptureTagPair(bool hasTagPairBracketClose)
        {
            var tagPairSyntax = new GreenPgnTagPairSyntax(ReadOnlySpanList<GreenWithTriviaSyntax>.FromBuilder(TagPairBuilder));

            // Analyze for errors.
            // Expect '[', tag name. tag value, ']'.
            if (!HasTagPairBracketOpen || !HasTagPairTagName || !HasTagPairTagValue || !hasTagPairBracketClose)
            {
                // Calculate the end position of the tag pair syntax.
                int tagPairEndPosition
                    = hasTagPairBracketClose ? symbolStartIndex + PgnBracketCloseSyntax.BracketCloseLength
                    : GetCurrentTriviaStartPosition();

                // To report tag pair errors, start at the '[', not where its leading trivia starts.
                int tagPairLength = GetLengthWithoutLeadingTrivia(tagPairSyntax);
                int tagPairStartPosition = tagPairEndPosition - tagPairLength;

                if (!HasTagPairBracketOpen)
                {
                    Errors.Add(new PgnErrorInfo(
                        PgnErrorCode.MissingTagBracketOpen,
                        tagPairStartPosition,
                        tagPairLength));
                }

                if (!HasTagPairTagName)
                {
                    if (!HasTagPairTagValue)
                    {
                        Errors.Add(new PgnErrorInfo(
                            PgnErrorCode.EmptyTag,
                            tagPairStartPosition,
                            tagPairLength));
                    }
                    else
                    {
                        Errors.Add(new PgnErrorInfo(
                            PgnErrorCode.MissingTagName,
                            tagPairStartPosition,
                            tagPairLength));
                    }
                }
                else if (!HasTagPairTagValue)
                {
                    Errors.Add(new PgnErrorInfo(
                        PgnErrorCode.MissingTagValue,
                        tagPairStartPosition,
                        tagPairLength));
                }

                if (!hasTagPairBracketClose)
                {
                    Errors.Add(new PgnErrorInfo(
                        PgnErrorCode.MissingTagBracketClose,
                        tagPairStartPosition,
                        tagPairLength));
                }
            }

            TagSectionBuilder.Add(tagPairSyntax);

            InTagPair = false;
            HasTagPairBracketOpen = false;
            HasTagPairTagName = false;
            HasTagPairTagValue = false;
        }

        private void CaptureTagPairIfNecessary()
        {
            if (InTagPair) CaptureTagPair(hasTagPairBracketClose: false);
        }

        private void CaptureTagSection()
        {
            if (TagSectionBuilder.Count > 0)
            {
                LatestTagSection = GreenPgnTagSectionSyntax.Create(ReadOnlySpanList<GreenPgnTagPairSyntax>.FromBuilder(TagSectionBuilder));
            }
        }

        private void AddTagElementToBuilder()
        {
            InTagPair = true;
            TagPairBuilder.Add(symbolBeingYielded);
        }

        #endregion Tag section parsing

        #region Yield content nodes

        private void YieldInTagSection()
        {
            switch (symbolBeingYielded.ContentNode.SymbolType)
            {
                case PgnSymbolType.BracketOpen:
                    // When encountering a new '[', open a new tag pair.
                    CaptureTagPairIfNecessary();
                    HasTagPairBracketOpen = true;
                    AddTagElementToBuilder();
                    break;
                case PgnSymbolType.BracketClose:
                    // When encountering a ']', always immediately close this tag pair.
                    AddTagElementToBuilder();
                    CaptureTagPair(hasTagPairBracketClose: true);
                    break;
                case PgnSymbolType.TagName:
                    // Open a new tag pair if a tag name or value was seen earlier in the same tag pair.
                    if (HasTagPairTagName || HasTagPairTagValue) CaptureTagPair(hasTagPairBracketClose: false);
                    HasTagPairTagName = true;
                    AddTagElementToBuilder();
                    break;
                case PgnSymbolType.TagValue:
                case PgnSymbolType.ErrorTagValue:
                    // Only accept the first tag value.
                    if (!HasTagPairTagValue)
                    {
                        HasTagPairTagValue = true;
                    }
                    else
                    {
                        Errors.Add(new PgnErrorInfo(
                            PgnErrorCode.MultipleTagValues,
                            symbolStartIndex,
                            symbolBeingYielded.ContentNode.Length));
                    }
                    AddTagElementToBuilder();
                    break;
                case PgnSymbolType.MoveNumber:
                    // Switch to move tree section.
                    CaptureTagPairIfNecessary();
                    CaptureTagSection();
                    YieldMoveNumber(ReadOnlySpanList<GreenWithTriviaSyntax>.Empty);
                    YieldContentNode = YieldInMoveTreeSectionAction;
                    break;
                case PgnSymbolType.Period:
                    // Switch to move tree section.
                    CaptureTagPairIfNecessary();
                    CaptureTagSection();
                    YieldPeriod();
                    YieldContentNode = YieldInMoveTreeSectionAction;
                    break;
                case PgnSymbolType.Move:
                    if (HasTagPairBracketOpen)
                    {
                        // Reinterpret as a tag name?
                        GreenPgnMoveSyntax moveSyntax = (GreenPgnMoveSyntax)symbolBeingYielded.ContentNode;
                        if (moveSyntax.IsValidTagName)
                        {
                            // Replace symbolBeingYielded, then go to the tag name case.
                            symbolBeingYielded = ConvertToTagName(symbolBeingYielded);
                            goto case PgnSymbolType.TagName;
                        }
                    }
                    // Switch to move tree section.
                    CaptureTagPairIfNecessary();
                    CaptureTagSection();
                    YieldMove(ReadOnlySpanList<GreenWithTriviaSyntax>.Empty);
                    YieldContentNode = YieldInMoveTreeSectionAction;
                    break;
                case PgnSymbolType.UnrecognizedMove:
                    // Switch to move tree section.
                    CaptureTagPairIfNecessary();
                    CaptureTagSection();
                    YieldMove(ReadOnlySpanList<GreenWithTriviaSyntax>.Empty);
                    YieldContentNode = YieldInMoveTreeSectionAction;
                    break;
                case PgnSymbolType.Nag:
                case PgnSymbolType.EmptyNag:
                case PgnSymbolType.OverflowNag:
                    // Switch to move tree section.
                    CaptureTagPairIfNecessary();
                    CaptureTagSection();
                    YieldNag(ReadOnlySpanList<GreenWithTriviaSyntax>.Empty);
                    YieldContentNode = YieldInMoveTreeSectionAction;
                    break;
                case PgnSymbolType.ParenthesisOpen:
                    // Switch to move tree section.
                    CaptureTagPairIfNecessary();
                    CaptureTagSection();
                    YieldParenthesisOpen();
                    YieldContentNode = YieldInMoveTreeSectionAction;
                    break;
                case PgnSymbolType.ParenthesisClose:
                    // Switch to move tree section.
                    CaptureTagPairIfNecessary();
                    CaptureTagSection();
                    YieldOrphanParenthesisClose();
                    YieldContentNode = YieldInMoveTreeSectionAction;
                    break;
                case PgnSymbolType.Asterisk:
                case PgnSymbolType.DrawMarker:
                case PgnSymbolType.WhiteWinMarker:
                case PgnSymbolType.BlackWinMarker:
                    CaptureTagPairIfNecessary();
                    CaptureTagSection();
                    CaptureGame(GreenPgnPlyListSyntax.Empty, symbolBeingYielded);
                    break;
                default:
                    throw new UnreachableException();
            }
        }

        private void YieldMoveNumberInMoveTreeSection()
        {
            // Move number always starts a new ply, so capture any unfinished ply.
            YieldMoveNumber(CapturePly());
        }

        private void YieldMoveInMoveTreeSection()
        {
            // Only allow a preceding move number in the same ply.
            var floatItems = CaptureFloatItems();
            if (CurrentFrame.Move != null
                || CurrentFrame.NagListBuilder.Count > 0
                || CurrentFrame.VariationListBuilder.Count > 0)
            {
                CapturePlyUnchecked(floatItems.Length);
            }
            YieldMove(floatItems);
        }

        private void YieldNagInMoveTreeSection()
        {
            var floatItems = CaptureFloatItems();
            if (CurrentFrame.VariationListBuilder.Count > 0)
            {
                // Report variation before NAG message.
                Errors.Add(new PgnErrorInfo(
                    PgnErrorCode.VariationBeforeNAG,
                    symbolStartIndex,
                    symbolBeingYielded.ContentNode.Length));

                CapturePlyUnchecked(floatItems.Length);
            }
            YieldNag(floatItems);
        }

        private void YieldGameResultInMoveTreeSection()
        {
            CaptureMainLine(symbolBeingYielded);
        }

        private void CaptureSavedBracketOpen()
        {
            YieldTagElementInMoveTree(savedBracketOpenStartPosition, savedBracketOpen);
            savedBracketOpen = null;
        }

        private void CaptureSavedTagName()
        {
            CaptureSavedBracketOpen();

            // Because YieldMoveInMoveTreeSection does all kinds of error reporting,
            // which takes dependencies on symbolStartIndex and symbolBeingYielded, temporarily overwrite
            // those fields. It's ugly and brittle but anything more parametrized involves a PF hit
            // on files with only few errors.
            // The CurrentFrame is not affected, it only has an extra float item, being the captured '['.
            int savedSymbolStartIndex = symbolStartIndex;
            GreenWithTriviaSyntax savedSymbolBeingYielded = symbolBeingYielded;

            symbolStartIndex = savedTagNameStartPosition;
            symbolBeingYielded = ConvertToUnrecognizedMove(savedTagNameStartPosition, savedTagName);
            savedTagName = null;

            YieldMoveInMoveTreeSection();

            symbolStartIndex = savedSymbolStartIndex;
            symbolBeingYielded = savedSymbolBeingYielded;
        }

        private void YieldInMoveTreeSection()
        {
            switch (symbolBeingYielded.ContentNode.SymbolType)
            {
                case PgnSymbolType.BracketOpen:
                    // Only switch to a new tag section if encountering a '[', followed by a tag name, then a tag value.
                    //
                    // Rationale:
                    //
                    // a) There's overlap between tag names and move texts, especially if a move contains a typo and yields a valid tag name. ("Bf9")
                    // b) '{' or '}' are easily typoed if one forgets pressing SHIFT, yielding '[' and ']'.
                    //
                    // If most of the PGN text is valid, a tag section has two or more tag pairs, -and- new tag sections are also triggered
                    // by a game termination marker. If the '[' was meant to start a new tag pair after all, we were likely already in
                    // a tag section (first game, or preceding termination marker), -or- another valid tag pair will follow in which case
                    // that one will trigger the tag section switch (and this '[' character wouldn't yield a valid tag pair anyway), -or-
                    // user genuinely forgets the game termination marker, and proceeds to starts a new game, in which case the tag section
                    // switch is triggered once valid tag name and values are entered.
                    savedBracketOpenStartPosition = symbolStartIndex;
                    savedBracketOpen = symbolBeingYielded;
                    YieldContentNode = YieldAfterBracketOpenInMoveTreeSectionAction;
                    break;
                case PgnSymbolType.TagName:
                    // Reinterpret as an unrecognized move.
                    symbolBeingYielded = ConvertToUnrecognizedMove(symbolStartIndex, symbolBeingYielded);
                    goto case PgnSymbolType.UnrecognizedMove;
                case PgnSymbolType.BracketClose:
                case PgnSymbolType.TagValue:
                case PgnSymbolType.ErrorTagValue:
                    YieldTagElementInMoveTree(symbolStartIndex, symbolBeingYielded);
                    break;
                case PgnSymbolType.MoveNumber:
                    YieldMoveNumberInMoveTreeSection();
                    break;
                case PgnSymbolType.Period:
                    YieldPeriod();
                    break;
                case PgnSymbolType.Move:
                case PgnSymbolType.UnrecognizedMove:
                    YieldMoveInMoveTreeSection();
                    break;
                case PgnSymbolType.Nag:
                case PgnSymbolType.EmptyNag:
                case PgnSymbolType.OverflowNag:
                    YieldNagInMoveTreeSection();
                    break;
                case PgnSymbolType.ParenthesisOpen:
                    YieldParenthesisOpen();
                    break;
                case PgnSymbolType.ParenthesisClose:
                    YieldParenthesisClose();
                    break;
                case PgnSymbolType.Asterisk:
                case PgnSymbolType.DrawMarker:
                case PgnSymbolType.WhiteWinMarker:
                case PgnSymbolType.BlackWinMarker:
                    YieldGameResultInMoveTreeSection();
                    YieldContentNode = YieldInTagSectionAction;
                    break;
                default:
                    throw new UnreachableException();
            }
        }

        private void YieldAfterBracketOpenInMoveTreeSection()
        {
            switch (symbolBeingYielded.ContentNode.SymbolType)
            {
                case PgnSymbolType.BracketOpen:
                    CaptureSavedBracketOpen();
                    savedBracketOpenStartPosition = symbolStartIndex;
                    savedBracketOpen = symbolBeingYielded;
                    break;
                case PgnSymbolType.TagName:
                    savedTagNameStartPosition = symbolStartIndex;
                    savedTagName = symbolBeingYielded;
                    YieldContentNode = YieldAfterTagNameInMoveTreeSectionAction;
                    break;
                case PgnSymbolType.BracketClose:
                case PgnSymbolType.TagValue:
                case PgnSymbolType.ErrorTagValue:
                    CaptureSavedBracketOpen();
                    YieldTagElementInMoveTree(symbolStartIndex, symbolBeingYielded);
                    YieldContentNode = YieldInMoveTreeSectionAction;
                    break;
                case PgnSymbolType.MoveNumber:
                    CaptureSavedBracketOpen();
                    YieldMoveNumberInMoveTreeSection();
                    YieldContentNode = YieldInMoveTreeSectionAction;
                    break;
                case PgnSymbolType.Period:
                    CaptureSavedBracketOpen();
                    YieldPeriod();
                    YieldContentNode = YieldInMoveTreeSectionAction;
                    break;
                case PgnSymbolType.Move:
                case PgnSymbolType.UnrecognizedMove:
                    CaptureSavedBracketOpen();
                    YieldMoveInMoveTreeSection();
                    YieldContentNode = YieldInMoveTreeSectionAction;
                    break;
                case PgnSymbolType.Nag:
                case PgnSymbolType.EmptyNag:
                case PgnSymbolType.OverflowNag:
                    CaptureSavedBracketOpen();
                    YieldNagInMoveTreeSection();
                    YieldContentNode = YieldInMoveTreeSectionAction;
                    break;
                case PgnSymbolType.ParenthesisOpen:
                    CaptureSavedBracketOpen();
                    YieldParenthesisOpen();
                    YieldContentNode = YieldInMoveTreeSectionAction;
                    break;
                case PgnSymbolType.ParenthesisClose:
                    CaptureSavedBracketOpen();
                    YieldParenthesisClose();
                    YieldContentNode = YieldInMoveTreeSectionAction;
                    break;
                case PgnSymbolType.Asterisk:
                case PgnSymbolType.DrawMarker:
                case PgnSymbolType.WhiteWinMarker:
                case PgnSymbolType.BlackWinMarker:
                    CaptureSavedBracketOpen();
                    YieldGameResultInMoveTreeSection();
                    YieldContentNode = YieldInTagSectionAction;
                    break;
                default:
                    throw new UnreachableException();
            }
        }

        private void YieldAfterTagNameInMoveTreeSection()
        {
            switch (symbolBeingYielded.ContentNode.SymbolType)
            {
                case PgnSymbolType.BracketOpen:
                    CaptureSavedTagName();
                    savedBracketOpenStartPosition = symbolStartIndex;
                    savedBracketOpen = symbolBeingYielded;
                    YieldContentNode = YieldAfterBracketOpenInMoveTreeSectionAction;
                    break;
                case PgnSymbolType.BracketClose:
                    CaptureSavedTagName();
                    YieldTagElementInMoveTree(symbolStartIndex, symbolBeingYielded);
                    YieldContentNode = YieldInMoveTreeSectionAction;
                    break;
                case PgnSymbolType.TagName:
                    CaptureSavedTagName();
                    // Reinterpret as an unrecognized move.
                    symbolBeingYielded = ConvertToUnrecognizedMove(symbolStartIndex, symbolBeingYielded);
                    goto unrecognizedMove;
                case PgnSymbolType.TagValue:
                case PgnSymbolType.ErrorTagValue:
                    // '[' + tag name + tag value triggers end of the game and a new tag section.
                    // Like in CaptureSavedTagName, for error reporting we need to pretend we're
                    // still at the saved symbols and not already 2 symbols ahead.
                    int savedSymbolStartIndex = symbolStartIndex;
                    GreenWithTriviaSyntax savedSymbolBeingYielded = symbolBeingYielded;

                    // Now replay what would have happened had we switched to a tag section directly at the '[' character.
                    symbolStartIndex = savedBracketOpenStartPosition;
                    symbolBeingYielded = savedBracketOpen;
                    CaptureMainLine(null);
                    HasTagPairBracketOpen = true;
                    AddTagElementToBuilder();
                    savedBracketOpen = null;

                    symbolStartIndex = savedTagNameStartPosition;
                    symbolBeingYielded = savedTagName;
                    HasTagPairTagName = true;
                    AddTagElementToBuilder();
                    savedTagName = null;

                    // Restore now we're at the current symbol.
                    symbolStartIndex = savedSymbolStartIndex;
                    symbolBeingYielded = savedSymbolBeingYielded;
                    HasTagPairTagValue = true;
                    AddTagElementToBuilder();
                    YieldContentNode = YieldInTagSectionAction;
                    break;
                case PgnSymbolType.MoveNumber:
                    CaptureSavedTagName();
                    YieldMoveNumberInMoveTreeSection();
                    YieldContentNode = YieldInMoveTreeSectionAction;
                    break;
                case PgnSymbolType.Period:
                    CaptureSavedTagName();
                    YieldPeriod();
                    YieldContentNode = YieldInMoveTreeSectionAction;
                    break;
                case PgnSymbolType.Move:
                case PgnSymbolType.UnrecognizedMove:
                    CaptureSavedTagName();
                unrecognizedMove:
                    YieldMoveInMoveTreeSection();
                    YieldContentNode = YieldInMoveTreeSectionAction;
                    break;
                case PgnSymbolType.Nag:
                case PgnSymbolType.EmptyNag:
                case PgnSymbolType.OverflowNag:
                    CaptureSavedTagName();
                    YieldNagInMoveTreeSection();
                    YieldContentNode = YieldInMoveTreeSectionAction;
                    break;
                case PgnSymbolType.ParenthesisOpen:
                    CaptureSavedTagName();
                    YieldParenthesisOpen();
                    YieldContentNode = YieldInMoveTreeSectionAction;
                    break;
                case PgnSymbolType.ParenthesisClose:
                    CaptureSavedTagName();
                    YieldParenthesisClose();
                    YieldContentNode = YieldInMoveTreeSectionAction;
                    break;
                case PgnSymbolType.Asterisk:
                case PgnSymbolType.DrawMarker:
                case PgnSymbolType.WhiteWinMarker:
                case PgnSymbolType.BlackWinMarker:
                    CaptureSavedTagName();
                    YieldGameResultInMoveTreeSection();
                    YieldContentNode = YieldInTagSectionAction;
                    break;
                default:
                    throw new UnreachableException();
            }
        }

        #endregion Yield content nodes

        #region Yield tokens and EOF

        private ReadOnlySpanList<GreenPgnTriviaElementSyntax> CaptureTrivia()
        {
            return ReadOnlySpanList<GreenPgnTriviaElementSyntax>.FromBuilder(TriviaBuilder);
        }

        private ReadOnlySpanList<GreenPgnBackgroundSyntax> CaptureBackground()
        {
            return ReadOnlySpanList<GreenPgnBackgroundSyntax>.FromBuilder(BackgroundBuilder);
        }

        private void Yield(IGreenPgnSymbol symbol)
        {
            symbolBeingYielded = new GreenWithTriviaSyntax(GreenPgnTriviaSyntax.Create(CaptureTrivia(), CaptureBackground()), symbol);
            YieldContentNode();
        }

        private void YieldTrivia(GreenPgnCommentSyntax commentSyntax)
        {
            TriviaBuilder.Add(new GreenPgnTriviaElementSyntax(CaptureBackground(), commentSyntax));
        }

        private void YieldBackground(GreenPgnBackgroundSyntax backgroundSyntax)
        {
            BackgroundBuilder.Add(backgroundSyntax);
        }

        private GreenPgnGameListSyntax YieldEof()
        {
            trailingTrivia = GreenPgnTriviaSyntax.Create(CaptureTrivia(), CaptureBackground());
            symbolBeingYielded = null;

            if (YieldContentNode == YieldInTagSectionAction)
            {
                CaptureTagPairIfNecessary();
                CaptureTagSection();

                // If the last tag section was non-empty, also capture it as a new empty game.
                if (LatestTagSection.TagPairNodes.Count > 0)
                {
                    CaptureGame(GreenPgnPlyListSyntax.Empty, null);
                }
            }
            else
            {
                if (YieldContentNode == YieldAfterBracketOpenInMoveTreeSectionAction)
                {
                    CaptureSavedBracketOpen();
                }
                else if (YieldContentNode == YieldAfterTagNameInMoveTreeSectionAction)
                {
                    CaptureSavedTagName();
                }

                CaptureMainLine(null);
            }

            return new GreenPgnGameListSyntax(ReadOnlySpanList<GreenPgnGameSyntax>.FromBuilder(GameListBuilder), trailingTrivia);
        }

        #endregion Yield tokens and EOF

        #region Lexing

        private void ReportIllegalCharacterSyntaxError(char c, int position)
        {
            Errors.Add(PgnIllegalCharacterSyntax.CreateError(
                CStyleStringLiteral.CharacterMustBeEscaped(c)
                ? CStyleStringLiteral.EscapedCharacterString(c)
                : Convert.ToString(c), position));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void YieldPgnSymbol(ref PgnSymbolStateMachine symbolBuilder, string pgnText, int symbolStartIndex, int length)
        {
            IGreenPgnSymbol symbol = symbolBuilder.Yield(length);

            if (symbol == null)
            {
                Errors.Add(PgnMoveSyntax.CreateUnrecognizedMoveError(pgnText.Substring(symbolStartIndex, length), symbolStartIndex));
                Yield(new GreenPgnUnrecognizedMoveSyntax(length, isConvertedFromTagName: false));
            }
            else
            {
                Yield(symbol);
            }
        }

        private void ParsePgnText()
        {
            // This tokenizer uses labels with goto to switch between modes of tokenization.

            int length = pgnText.Length;

            int currentIndex = symbolStartIndex;
            StringBuilder valueBuilder = new StringBuilder();
            bool hasStringErrors;

            // Reusable structure to build green PGN symbols.
            PgnSymbolStateMachine symbolBuilder = default;

        inWhitespace:

            while (currentIndex < length)
            {
                char c = pgnText[currentIndex];
                int characterClass = PgnParserCharacterClass.GetCharacterClass(c);

                if (characterClass != PgnParserCharacterClass.WhitespaceCharacter)
                {
                    if (symbolStartIndex < currentIndex)
                    {
                        YieldBackground(GreenPgnWhitespaceSyntax.Create(currentIndex - symbolStartIndex));
                        symbolStartIndex = currentIndex;
                    }

                    if (characterClass != PgnParserCharacterClass.IllegalCharacter)
                    {
                        int symbolCharacterClass = characterClass & PgnParserCharacterClass.SymbolCharacterMask;
                        if (symbolCharacterClass != 0)
                        {
                            symbolBuilder.Start(symbolCharacterClass);
                            goto inSymbol;
                        }

                        switch (c)
                        {
                            case PgnGameResultSyntax.AsteriskCharacter:
                                Yield(GreenPgnAsteriskSyntax.Value);
                                symbolStartIndex++;
                                break;
                            case PgnBracketOpenSyntax.BracketOpenCharacter:
                                Yield(GreenPgnBracketOpenSyntax.Value);
                                symbolStartIndex++;
                                break;
                            case PgnBracketCloseSyntax.BracketCloseCharacter:
                                Yield(GreenPgnBracketCloseSyntax.Value);
                                symbolStartIndex++;
                                break;
                            case PgnParenthesisCloseSyntax.ParenthesisCloseCharacter:
                                Yield(GreenPgnParenthesisCloseSyntax.Value);
                                symbolStartIndex++;
                                break;
                            case PgnParenthesisOpenSyntax.ParenthesisOpenCharacter:
                                Yield(GreenPgnParenthesisOpenSyntax.Value);
                                symbolStartIndex++;
                                break;
                            case PgnPeriodSyntax.PeriodCharacter:
                                Yield(GreenPgnPeriodSyntax.Value);
                                symbolStartIndex++;
                                break;
                            case CStyleStringLiteral.QuoteCharacter:
                                goto inString;
                            case PgnCommentSyntax.EndOfLineCommentStartCharacter:
                                goto inEndOfLineComment;
                            case PgnCommentSyntax.MultiLineCommentStartCharacter:
                                goto inMultiLineComment;
                            case PgnNagSyntax.NagCharacter:
                                goto inNumericAnnotationGlyph;
                            case PgnEscapeSyntax.EscapeCharacter:
                                // Escape mechanism only triggered directly after a newline.
                                if (currentIndex == 0 || pgnText[currentIndex - 1] == '\n') goto inEscapeSequence;
                                ReportIllegalCharacterSyntaxError(c, symbolStartIndex);
                                YieldBackground(GreenPgnIllegalCharacterSyntax.Value);
                                symbolStartIndex++;
                                break;
                            default:
                                throw new InvalidOperationException("Case statement on special characters is not exhaustive.");
                        }
                    }
                    else
                    {
                        ReportIllegalCharacterSyntaxError(c, symbolStartIndex);
                        YieldBackground(GreenPgnIllegalCharacterSyntax.Value);
                        symbolStartIndex++;
                    }
                }

                currentIndex++;
            }

            if (symbolStartIndex < currentIndex)
            {
                YieldBackground(GreenPgnWhitespaceSyntax.Create(currentIndex - symbolStartIndex));
            }

            return;

        inSymbol:

            // Eat the first symbol character, but leave symbolStartIndex unchanged.
            currentIndex++;

            GreenPgnBackgroundSyntax backgroundToYield;
            IGreenPgnSymbol characterToYield;

            while (currentIndex < length)
            {
                char c = pgnText[currentIndex];
                int characterClass = PgnParserCharacterClass.GetCharacterClass(c);

                if (characterClass == PgnParserCharacterClass.WhitespaceCharacter)
                {
                    if (symbolStartIndex < currentIndex)
                    {
                        YieldPgnSymbol(ref symbolBuilder, pgnText, symbolStartIndex, currentIndex - symbolStartIndex);
                        symbolStartIndex = currentIndex;
                    }

                    currentIndex++;
                    goto inWhitespace;
                }

                if (characterClass != PgnParserCharacterClass.IllegalCharacter)
                {
                    int symbolCharacterClass = characterClass & PgnParserCharacterClass.SymbolCharacterMask;
                    if (symbolCharacterClass != 0)
                    {
                        symbolBuilder.Transition(symbolCharacterClass);
                    }
                    else
                    {
                        switch (c)
                        {
                            case PgnGameResultSyntax.AsteriskCharacter:
                                characterToYield = GreenPgnAsteriskSyntax.Value;
                                goto yieldSymbolThenCharacter;
                            case PgnBracketOpenSyntax.BracketOpenCharacter:
                                characterToYield = GreenPgnBracketOpenSyntax.Value;
                                goto yieldSymbolThenCharacter;
                            case PgnBracketCloseSyntax.BracketCloseCharacter:
                                characterToYield = GreenPgnBracketCloseSyntax.Value;
                                goto yieldSymbolThenCharacter;
                            case PgnParenthesisCloseSyntax.ParenthesisCloseCharacter:
                                characterToYield = GreenPgnParenthesisCloseSyntax.Value;
                                goto yieldSymbolThenCharacter;
                            case PgnParenthesisOpenSyntax.ParenthesisOpenCharacter:
                                characterToYield = GreenPgnParenthesisOpenSyntax.Value;
                                goto yieldSymbolThenCharacter;
                            case PgnPeriodSyntax.PeriodCharacter:
                                characterToYield = GreenPgnPeriodSyntax.Value;
                                goto yieldSymbolThenCharacter;
                            case CStyleStringLiteral.QuoteCharacter:
                                if (symbolStartIndex < currentIndex) YieldPgnSymbol(ref symbolBuilder, pgnText, symbolStartIndex, currentIndex - symbolStartIndex);
                                symbolStartIndex = currentIndex;
                                goto inString;
                            case PgnCommentSyntax.EndOfLineCommentStartCharacter:
                                if (symbolStartIndex < currentIndex) YieldPgnSymbol(ref symbolBuilder, pgnText, symbolStartIndex, currentIndex - symbolStartIndex);
                                symbolStartIndex = currentIndex;
                                goto inEndOfLineComment;
                            case PgnCommentSyntax.MultiLineCommentStartCharacter:
                                if (symbolStartIndex < currentIndex) YieldPgnSymbol(ref symbolBuilder, pgnText, symbolStartIndex, currentIndex - symbolStartIndex);
                                symbolStartIndex = currentIndex;
                                goto inMultiLineComment;
                            case PgnNagSyntax.NagCharacter:
                                if (symbolStartIndex < currentIndex) YieldPgnSymbol(ref symbolBuilder, pgnText, symbolStartIndex, currentIndex - symbolStartIndex);
                                symbolStartIndex = currentIndex;
                                goto inNumericAnnotationGlyph;
                            case PgnEscapeSyntax.EscapeCharacter:
                                ReportIllegalCharacterSyntaxError(c, currentIndex);
                                backgroundToYield = GreenPgnIllegalCharacterSyntax.Value;
                                goto yieldSymbolThenBackground;
                            default:
                                throw new InvalidOperationException("Case statement on special characters is not exhaustive.");
                        }
                    }
                }
                else
                {
                    ReportIllegalCharacterSyntaxError(c, currentIndex);
                    backgroundToYield = GreenPgnIllegalCharacterSyntax.Value;
                    goto yieldSymbolThenBackground;
                }

                currentIndex++;
            }

            if (symbolStartIndex < currentIndex)
            {
                YieldPgnSymbol(ref symbolBuilder, pgnText, symbolStartIndex, currentIndex - symbolStartIndex);
            }

            return;

        yieldSymbolThenCharacter:

            // Yield a GreenPgnSymbol, then symbolToYield, then go to whitespace.
            if (symbolStartIndex < currentIndex) YieldPgnSymbol(ref symbolBuilder, pgnText, symbolStartIndex, currentIndex - symbolStartIndex);
            symbolStartIndex = currentIndex;
            currentIndex++;
            Yield(characterToYield);
            symbolStartIndex = currentIndex;
            goto inWhitespace;

        yieldSymbolThenBackground:

            // Yield a GreenPgnSymbol, then symbolToYield, then go to whitespace.
            if (symbolStartIndex < currentIndex) YieldPgnSymbol(ref symbolBuilder, pgnText, symbolStartIndex, currentIndex - symbolStartIndex);
            symbolStartIndex = currentIndex;
            currentIndex++;
            YieldBackground(backgroundToYield);
            symbolStartIndex = currentIndex;
            goto inWhitespace;

        inString:

            // Detect errors.
            hasStringErrors = false;

            // Eat " character, but leave symbolStartIndex unchanged.
            currentIndex++;

            while (currentIndex < length)
            {
                char c = pgnText[currentIndex];

                // Closing quote character?
                if (c == CStyleStringLiteral.QuoteCharacter)
                {
                    // Include last character in the syntax node.
                    currentIndex++;

                    if (hasStringErrors)
                    {
                        Yield(new GreenPgnErrorTagValueSyntax(currentIndex - symbolStartIndex));
                    }
                    else
                    {
                        Yield(new GreenPgnTagValueSyntax(valueBuilder.ToString(), currentIndex - symbolStartIndex));
                    }

                    valueBuilder.Clear();
                    symbolStartIndex = currentIndex;
                    goto inWhitespace;
                }
                else if (c == CStyleStringLiteral.EscapeCharacter)
                {
                    // Look ahead one character.
                    int escapeSequenceStart = currentIndex;
                    currentIndex++;

                    if (currentIndex < length)
                    {
                        char escapedChar = pgnText[currentIndex];

                        // Only two escape sequences are supported in the PGN standard: \" and \\
                        if (escapedChar == CStyleStringLiteral.QuoteCharacter || escapedChar == CStyleStringLiteral.EscapeCharacter)
                        {
                            valueBuilder.Append(escapedChar);
                        }
                        else
                        {
                            hasStringErrors = true;

                            if (char.IsControl(escapedChar))
                            {
                                Errors.Add(PgnTagValueSyntax.IllegalControlCharacterError(escapedChar, currentIndex));
                            }

                            if (CStyleStringLiteral.CharacterMustBeEscaped(escapedChar))
                            {
                                // Just don't show the control character.
                                Errors.Add(PgnTagValueSyntax.UnrecognizedEscapeSequenceError(
                                    EscapeCharacterString,
                                    escapeSequenceStart,
                                    2));
                            }
                            else
                            {
                                Errors.Add(PgnTagValueSyntax.UnrecognizedEscapeSequenceError(
                                    new string(new[] { CStyleStringLiteral.EscapeCharacter, escapedChar }),
                                    escapeSequenceStart,
                                    2));
                            }
                        }
                    }
                    else
                    {
                        // In addition to this, break out of the loop because this is now also an unterminated string.
                        Errors.Add(PgnTagValueSyntax.UnrecognizedEscapeSequenceError(EscapeCharacterString, escapeSequenceStart, 1));
                        break;
                    }
                }
                else if (char.IsControl(c))
                {
                    hasStringErrors = true;
                    Errors.Add(PgnTagValueSyntax.IllegalControlCharacterError(c, currentIndex));
                }
                else
                {
                    valueBuilder.Append(c);
                }

                currentIndex++;
            }

            int unterminatedStringLength = length - symbolStartIndex;
            Errors.Add(PgnTagValueSyntax.UnterminatedError(symbolStartIndex, unterminatedStringLength));
            Yield(new GreenPgnErrorTagValueSyntax(unterminatedStringLength));
            return;

        inEndOfLineComment:

            // Eat the ';' character, but leave symbolStartIndex unchanged.
            currentIndex++;

            while (currentIndex < length)
            {
                char c = pgnText[currentIndex];

                if (c == '\r')
                {
                    // Can already eat this whitespace character.
                    currentIndex++;

                    // Look ahead to see if the next character is a linefeed.
                    // Otherwise, the '\r' just becomes part of the comment.
                    if (currentIndex < length)
                    {
                        char secondChar = pgnText[currentIndex];
                        if (secondChar == '\n')
                        {
                            YieldTrivia(new GreenPgnCommentSyntax(currentIndex - 1 - symbolStartIndex));

                            // Eat the '\n'.
                            symbolStartIndex = currentIndex - 1;
                            currentIndex++;
                            goto inWhitespace;
                        }
                    }
                }
                else if (c == '\n')
                {
                    YieldTrivia(new GreenPgnCommentSyntax(currentIndex - symbolStartIndex));

                    // Eat the '\n'.
                    symbolStartIndex = currentIndex;
                    currentIndex++;
                    goto inWhitespace;
                }

                currentIndex++;
            }

            YieldTrivia(new GreenPgnCommentSyntax(length - symbolStartIndex));
            return;

        inEscapeSequence:

            // Copy of inEndOfLineComment above, except with a different trigger and result.
            currentIndex++;
            while (currentIndex < length)
            {
                char c = pgnText[currentIndex];
                if (c == '\r')
                {
                    currentIndex++;
                    if (currentIndex < length)
                    {
                        char secondChar = pgnText[currentIndex];
                        if (secondChar == '\n')
                        {
                            YieldBackground(new GreenPgnEscapeSyntax(currentIndex - 1 - symbolStartIndex));
                            symbolStartIndex = currentIndex - 1;
                            currentIndex++;
                            goto inWhitespace;
                        }
                    }
                }
                else if (c == '\n')
                {
                    YieldBackground(new GreenPgnEscapeSyntax(currentIndex - symbolStartIndex));
                    symbolStartIndex = currentIndex;
                    currentIndex++;
                    goto inWhitespace;
                }
                currentIndex++;
            }

            YieldBackground(new GreenPgnEscapeSyntax(length - symbolStartIndex));
            return;

        inMultiLineComment:

            // Eat the '{' character, but leave symbolStartIndex unchanged.
            currentIndex++;

            while (currentIndex < length)
            {
                char c = pgnText[currentIndex];

                // Any character here, including the delimiter '}', is part of the comment.
                currentIndex++;

                if (c == PgnCommentSyntax.MultiLineCommentEndCharacter)
                {
                    YieldTrivia(new GreenPgnCommentSyntax(currentIndex - symbolStartIndex));
                    symbolStartIndex = currentIndex;
                    goto inWhitespace;
                }
            }

            int unterminatedCommentLength = length - symbolStartIndex;
            Errors.Add(PgnCommentSyntax.CreateUnterminatedCommentMessage(symbolStartIndex, unterminatedCommentLength));
            YieldTrivia(new GreenPgnUnterminatedCommentSyntax(unterminatedCommentLength));
            return;

        inNumericAnnotationGlyph:

            // Eat the '$' character, but leave symbolStartIndex unchanged.
            currentIndex++;

            int annotationValue = 0;
            bool emptyNag = true;
            bool overflowNag = false;

            while (currentIndex < length)
            {
                int digit = pgnText[currentIndex] - '0';

                if (digit < 0 || digit > 9)
                {
                    if (emptyNag)
                    {
                        Errors.Add(PgnNagSyntax.CreateEmptyNagMessage(symbolStartIndex));
                        Yield(GreenPgnEmptyNagSyntax.Value);
                    }
                    else if (!overflowNag)
                    {
                        Yield(new GreenPgnNagSyntax((PgnAnnotation)annotationValue, currentIndex - symbolStartIndex));
                    }
                    else
                    {
                        int overflowNagLength = currentIndex - symbolStartIndex;
                        Errors.Add(PgnNagSyntax.CreateOverflowNagMessage(pgnText.Substring(symbolStartIndex, overflowNagLength), symbolStartIndex));
                        Yield(new GreenPgnOverflowNagSyntax(overflowNagLength));
                    }

                    symbolStartIndex = currentIndex;
                    goto inWhitespace;
                }

                emptyNag = false;

                if (!overflowNag)
                {
                    annotationValue = annotationValue * 10 + digit;
                    if (annotationValue >= 0x100) overflowNag = true;
                }

                currentIndex++;
            }

            if (emptyNag)
            {
                Errors.Add(PgnNagSyntax.CreateEmptyNagMessage(symbolStartIndex));
                Yield(GreenPgnEmptyNagSyntax.Value);
            }
            else if (!overflowNag)
            {
                Yield(new GreenPgnNagSyntax((PgnAnnotation)annotationValue, length - symbolStartIndex));
            }
            else
            {
                int overflowNagLength = currentIndex - symbolStartIndex;
                Errors.Add(PgnNagSyntax.CreateOverflowNagMessage(pgnText.Substring(symbolStartIndex, overflowNagLength), symbolStartIndex));
                Yield(new GreenPgnOverflowNagSyntax(overflowNagLength));
            }
        }

        #endregion Lexing
    }
}

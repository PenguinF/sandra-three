#region License
/*********************************************************************************
 * PgnParser.cs
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

using Eutherion.Text;
using Eutherion.Utils;
using Sandra.Chess.Pgn.Temp;
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
        private struct VariationStackFrame
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

            public void Reset()
            {
                SavedParenthesisOpenWithTrivia = null;
                HasPly = false;
                MoveNumber = null;
                Move = null;
                FloatItemListBuilder = new List<GreenWithTriviaSyntax>();
                NagListBuilder = new List<GreenWithPlyFloatItemsSyntax<GreenWithTriviaSyntax>>();
                VariationListBuilder = new List<GreenWithPlyFloatItemsSyntax<GreenPgnVariationSyntax>>();
                PlyListBuilder = new List<GreenPgnPlySyntax>();
            }
        }

        /// <summary>
        /// See also <see cref="StringLiteral.EscapeCharacter"/>.
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
            GreenPgnTriviaSyntax trailingTrivia = parser.YieldEof();

            return new RootPgnSyntax(
                parser.SymbolBuilder,
                trailingTrivia,
                parser.Errors);
        }

        private readonly List<PgnErrorInfo> Errors;
        private readonly List<GreenPgnBackgroundSyntax> BackgroundBuilder;
        private readonly List<GreenPgnTriviaElementSyntax> TriviaBuilder;
        private readonly List<GreenWithTriviaSyntax> TagPairBuilder;
        private readonly List<GreenPgnTagPairSyntax> TagSectionBuilder;
        private readonly Stack<VariationStackFrame> VariationBuilderStack;
        private readonly List<IGreenPgnTopLevelSyntax> SymbolBuilder;

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

        // Contains stack frame for the current variation (main or side line) being built.
        private VariationStackFrame CurrentFrame;

        // All content node yielders. They depend on the position in the parse tree, i.e. the current parser state.
        private readonly Action YieldInTagSectionAction;
        private readonly Action YieldInMoveTreeSectionAction;

        // This is either YieldInTagSectionAction or YieldInMoveTreeSectionAction.
        // It is important that this action is an instance method, since invocation of such delegates is the fastest.
        private Action YieldContentNode;

        private PgnParser(string pgnText)
        {
            this.pgnText = pgnText;

            Errors = new List<PgnErrorInfo>();
            BackgroundBuilder = new List<GreenPgnBackgroundSyntax>();
            TriviaBuilder = new List<GreenPgnTriviaElementSyntax>();
            TagPairBuilder = new List<GreenWithTriviaSyntax>();
            TagSectionBuilder = new List<GreenPgnTagPairSyntax>();
            VariationBuilderStack = new Stack<VariationStackFrame>();
            SymbolBuilder = new List<IGreenPgnTopLevelSyntax>();

            CurrentFrame.Reset();

            YieldInTagSectionAction = YieldInTagSection;
            YieldInMoveTreeSectionAction = YieldInMoveTreeSection;

            YieldContentNode = YieldInTagSectionAction;
        }

        #region Conversions from one type of symbol to another

        private GreenWithTriviaSyntax ConvertToOrphanParenthesisClose(GreenWithTriviaSyntax parenthesisClose)
            => new GreenWithTriviaSyntax(
                parenthesisClose.LeadingTrivia,
                GreenPgnOrphanParenthesisCloseSyntax.Value);

        #endregion Conversions from one type of symbol to another

        #region Variation parsing

        private void CaptureMainLine()
        {
            CaptureNestedUnfinishedVariations();
            var floatItems = CapturePly();
            SymbolBuilder.Add(CapturePlyList(floatItems));
        }

        private void CaptureNestedUnfinishedVariations()
        {
            bool innermostVariation = true;

            while (VariationBuilderStack.Count > 0)
            {
                var variationSyntax = CaptureVariation(null);

                if (innermostVariation)
                {
                    // Report MissingParenthesisClose only once, for the innermost variation.
                    // The end position of the variation is where we are now.
                    int variationEndPosition
                        = symbolBeingYielded != null ? symbolStartIndex - symbolBeingYielded.LeadingTrivia.Length
                        : pgnText.Length - trailingTrivia.Length;

                    // Subtract the leading trivia length.
                    int variationLength = variationSyntax.Length - variationSyntax.ParenthesisOpen.LeadingTrivia.Length;

                    Errors.Add(new PgnErrorInfo(
                        PgnErrorCode.MissingParenthesisClose,
                        variationEndPosition - variationLength,
                        variationLength));

                    innermostVariation = false;
                }

                SymbolBuilder.Add(new GreenPgnTopLevelSymbolSyntax(variationSyntax.ParenthesisOpen, (parent, index, green) => new PgnParenthesisOpenWithTriviaSyntax(parent, index, green)));
                SymbolBuilder.Add(variationSyntax.PliesWithFloatItems);
                if (variationSyntax.ParenthesisClose != null)
                {
                    SymbolBuilder.Add(new GreenPgnTopLevelSymbolSyntax(variationSyntax.ParenthesisClose, (parent, index, green) => new PgnParenthesisCloseWithTriviaSyntax(parent, index, green)));
                }
            }
        }

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
                // or at the start of the current non-closing parenthesis being yielded,
                // or at the end of the PGN.
                int variationEndPosition
                    = maybeParenthesisClose != null ? symbolStartIndex + PgnParenthesisCloseSyntax.ParenthesisCloseLength
                    : symbolBeingYielded != null ? symbolStartIndex - symbolBeingYielded.LeadingTrivia.Length
                    : pgnText.Length - trailingTrivia.Length;

                // Subtract the leading trivia length.
                int variationLength = variationSyntax.Length - variationSyntax.ParenthesisOpen.LeadingTrivia.Length;

                Errors.Add(new PgnErrorInfo(
                    PgnErrorCode.EmptyVariation,
                    variationEndPosition - variationLength,
                    variationLength));
            }

            return variationSyntax;
        }

        private GreenPgnPlyListSyntax CapturePlyList(ReadOnlySpanList<GreenWithTriviaSyntax> trailingFloatItems)
        {
            var plyListSyntax = new GreenPgnPlyListSyntax(CurrentFrame.PlyListBuilder, trailingFloatItems);
            CurrentFrame.PlyListBuilder.Clear();
            return plyListSyntax;
        }

        private void YieldParenthesisOpen()
        {
            CurrentFrame.SavedParenthesisOpenWithTrivia = symbolBeingYielded;
            VariationBuilderStack.Push(CurrentFrame);

            // Initialize new frame.
            CurrentFrame.Reset();
        }

        private void YieldParenthesisClose()
        {
            if (VariationBuilderStack.Count > 0)
            {
                var variationSyntax = CaptureVariation(symbolBeingYielded);
                SymbolBuilder.Add(new GreenPgnTopLevelSymbolSyntax(variationSyntax.ParenthesisOpen, (parent, index, green) => new PgnParenthesisOpenWithTriviaSyntax(parent, index, green)));
                SymbolBuilder.Add(variationSyntax.PliesWithFloatItems);
                if (variationSyntax.ParenthesisClose != null)
                {
                    SymbolBuilder.Add(new GreenPgnTopLevelSymbolSyntax(variationSyntax.ParenthesisClose, (parent, index, green) => new PgnParenthesisCloseWithTriviaSyntax(parent, index, green)));
                }
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
                // See CaptureTagPair on how to calculate the error position and length.
                int plyEndPosition
                    = symbolBeingYielded != null ? symbolStartIndex - symbolBeingYielded.LeadingTrivia.Length
                    : pgnText.Length - trailingTrivia.Length;

                // For a ply though we need to subtract the length of the floating items that trail the captured ply.
                plyEndPosition -= trailingFloatItemsLength;

                // For plies, start at the first content node of the first ply content node.
                // So subtract both the leading float items length plus leading trivia length.
                GreenWithPlyFloatItemsSyntax firstNode;

                if (CurrentFrame.MoveNumber != null) firstNode = CurrentFrame.MoveNumber;
                else if (CurrentFrame.Move != null) firstNode = CurrentFrame.Move;
                else if (CurrentFrame.NagListBuilder.Count > 0) firstNode = CurrentFrame.NagListBuilder[0];
                else firstNode = CurrentFrame.VariationListBuilder[0];

                int plyLength = plySyntax.Length - firstNode.LeadingFloatItems.Length - firstNode.PlyContentNode.FirstWithTriviaNode.LeadingTrivia.Length;
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

        #endregion Ply parsing

        #region Tag section parsing

        private void CaptureTagPair(bool hasTagPairBracketClose)
        {
            var tagPairSyntax = new GreenPgnTagPairSyntax(TagPairBuilder);

            // Analyze for errors.
            // Expect '[', tag name. tag value, ']'.
            if (!HasTagPairBracketOpen || !HasTagPairTagName || !HasTagPairTagValue || !hasTagPairBracketClose)
            {
                // Calculate the end position of the tag pair syntax.
                // - At the end of the file, contentNodeBeingYielded is null; the end position is the length of the pgn minus its trailing trivia.
                // - If hasTagPairBracketClose is true, symbolStartIndex is at the start of the closing bracket.
                // - If hasTagPairBracketClose is false, symbolStartIndex is at the start of the first symbol not in the tag pair.
                int tagPairEndPosition
                    = hasTagPairBracketClose ? symbolStartIndex + 1
                    : symbolBeingYielded != null ? symbolStartIndex - symbolBeingYielded.LeadingTrivia.Length
                    : pgnText.Length - trailingTrivia.Length;

                // To report tag pair errors, start at the '[', not where its leading trivia starts.
                int tagPairLength = tagPairSyntax.Length - tagPairSyntax.TagElementNodes[0].LeadingTrivia.Length;
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
            TagPairBuilder.Clear();
        }

        private void CaptureTagPairIfNecessary()
        {
            if (InTagPair) CaptureTagPair(hasTagPairBracketClose: false);
        }

        private void CaptureTagSection()
        {
            if (TagSectionBuilder.Count > 0)
            {
                SymbolBuilder.Add(GreenPgnTagSectionSyntax.Create(TagSectionBuilder));
                TagSectionBuilder.Clear();
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
                    SymbolBuilder.Add(new GreenPgnTopLevelSymbolSyntax(symbolBeingYielded, (parent, index, green) => new PgnGameResultWithTriviaSyntax(parent, index, green)));
                    break;
                default:
                    throw new UnreachableException();
            }
        }

        private void YieldInMoveTreeSection()
        {
            ReadOnlySpanList<GreenWithTriviaSyntax> floatItems;

            switch (symbolBeingYielded.ContentNode.SymbolType)
            {
                case PgnSymbolType.BracketOpen:
                    CaptureMainLine();
                    HasTagPairBracketOpen = true;
                    AddTagElementToBuilder();
                    YieldContentNode = YieldInTagSectionAction;
                    break;
                case PgnSymbolType.BracketClose:
                    // When encountering a ']', switch to tag section and immediately open and close a tag pair.
                    CaptureMainLine();
                    AddTagElementToBuilder();
                    CaptureTagPair(hasTagPairBracketClose: true);
                    YieldContentNode = YieldInTagSectionAction;
                    break;
                case PgnSymbolType.TagName:
                    CaptureMainLine();
                    HasTagPairTagName = true;
                    AddTagElementToBuilder();
                    YieldContentNode = YieldInTagSectionAction;
                    break;
                case PgnSymbolType.TagValue:
                case PgnSymbolType.ErrorTagValue:
                    CaptureMainLine();
                    HasTagPairTagValue = true;
                    AddTagElementToBuilder();
                    YieldContentNode = YieldInTagSectionAction;
                    break;
                case PgnSymbolType.MoveNumber:
                    // Move number always starts a new ply, so capture any unfinished ply.
                    floatItems = CapturePly();
                    YieldMoveNumber(floatItems);
                    break;
                case PgnSymbolType.Period:
                    YieldPeriod();
                    break;
                case PgnSymbolType.Move:
                case PgnSymbolType.UnrecognizedMove:
                    // Only allow a preceding move number in the same ply.
                    floatItems = CaptureFloatItems();
                    if (CurrentFrame.Move != null
                        || CurrentFrame.NagListBuilder.Count > 0
                        || CurrentFrame.VariationListBuilder.Count > 0)
                    {
                        CapturePlyUnchecked(floatItems.Length);
                    }
                    YieldMove(floatItems);
                    break;
                case PgnSymbolType.Nag:
                case PgnSymbolType.EmptyNag:
                case PgnSymbolType.OverflowNag:
                    floatItems = CaptureFloatItems();
                    if (CurrentFrame.VariationListBuilder.Count > 0)
                    {
                        CapturePlyUnchecked(floatItems.Length);
                    }
                    YieldNag(floatItems);
                    break;
                case PgnSymbolType.ParenthesisOpen:
                    CaptureNestedUnfinishedVariations();
                    floatItems = CapturePly();
                    SymbolBuilder.Add(CapturePlyList(floatItems));
                    YieldParenthesisOpen();
                    break;
                case PgnSymbolType.ParenthesisClose:
                    YieldParenthesisClose();
                    break;
                case PgnSymbolType.Asterisk:
                case PgnSymbolType.DrawMarker:
                case PgnSymbolType.WhiteWinMarker:
                case PgnSymbolType.BlackWinMarker:
                    CaptureMainLine();
                    SymbolBuilder.Add(new GreenPgnTopLevelSymbolSyntax(symbolBeingYielded, (parent, index, green) => new PgnGameResultWithTriviaSyntax(parent, index, green)));
                    YieldContentNode = YieldInTagSectionAction;
                    break;
                default:
                    throw new UnreachableException();
            }
        }

        #endregion Yield content nodes

        #region Yield tokens and EOF

        private void Yield(IGreenPgnSymbol symbol)
        {
            symbolBeingYielded = new GreenWithTriviaSyntax(GreenPgnTriviaSyntax.Create(TriviaBuilder, BackgroundBuilder), symbol);
            YieldContentNode();
            BackgroundBuilder.Clear();
            TriviaBuilder.Clear();
        }

        private void YieldTrivia(GreenPgnCommentSyntax commentSyntax)
        {
            TriviaBuilder.Add(new GreenPgnTriviaElementSyntax(BackgroundBuilder, commentSyntax));
            BackgroundBuilder.Clear();
        }

        private void YieldBackground(GreenPgnBackgroundSyntax backgroundSyntax)
        {
            BackgroundBuilder.Add(backgroundSyntax);
        }

        private GreenPgnTriviaSyntax YieldEof()
        {
            trailingTrivia = GreenPgnTriviaSyntax.Create(TriviaBuilder, BackgroundBuilder);
            symbolBeingYielded = null;

            if (YieldContentNode == YieldInTagSectionAction)
            {
                CaptureTagPairIfNecessary();
                CaptureTagSection();
            }
            else
            {
                CaptureMainLine();
            }

            return trailingTrivia;
        }

        #endregion Yield tokens and EOF

        #region Lexing

        private void ReportIllegalCharacterSyntaxError(char c, int position)
        {
            Errors.Add(PgnIllegalCharacterSyntax.CreateError(
                StringLiteral.CharacterMustBeEscaped(c)
                ? StringLiteral.EscapedCharacterString(c)
                : Convert.ToString(c), position));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void YieldPgnSymbol(ref PgnSymbolStateMachine symbolBuilder, string pgnText, int symbolStartIndex, int length)
        {
            IGreenPgnSymbol symbol = symbolBuilder.Yield(length);

            if (symbol == null)
            {
                Errors.Add(PgnMoveSyntax.CreateUnrecognizedMoveError(pgnText.Substring(symbolStartIndex, length), symbolStartIndex));
                Yield(new GreenPgnUnrecognizedMoveSyntax(length));
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
                            case StringLiteral.QuoteCharacter:
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
                            case StringLiteral.QuoteCharacter:
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
                if (c == StringLiteral.QuoteCharacter)
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
                else if (c == StringLiteral.EscapeCharacter)
                {
                    // Look ahead one character.
                    int escapeSequenceStart = currentIndex;
                    currentIndex++;

                    if (currentIndex < length)
                    {
                        char escapedChar = pgnText[currentIndex];

                        // Only two escape sequences are supported in the PGN standard: \" and \\
                        if (escapedChar == StringLiteral.QuoteCharacter || escapedChar == StringLiteral.EscapeCharacter)
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

                            if (StringLiteral.CharacterMustBeEscaped(escapedChar))
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
                                    new string(new[] { StringLiteral.EscapeCharacter, escapedChar }),
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

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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Partitions a source PGN file into separate tokens, then generates an abstract syntax tree from it.
    /// </summary>
    public sealed class PgnParser
    {
        #region PGN character classes

        private const int IllegalCharacter = 0;

        // Symbol characters in discrete partitions of character sets.
        private const int SymbolCharacterMask = 0x3f;

        private const int SpecialCharacter = 1 << 6;
        private const int WhitespaceCharacter = 1 << 7;

        /// <summary>
        /// Contains a bitfield of character classes relevant for PGN, for each 8-bit character.
        /// A value of 0 means the character is not allowed.
        /// </summary>
        private static readonly int[] PgnCharacterClassTable = new int[0x100];

        static PgnParser()
        {
            // 0x00..0x20: treat 4 control characters and ' ' as whitespace.
            PgnCharacterClassTable['\t'] = WhitespaceCharacter;
            PgnCharacterClassTable['\n'] = WhitespaceCharacter;
            PgnCharacterClassTable['\v'] = WhitespaceCharacter;
            PgnCharacterClassTable['\r'] = WhitespaceCharacter;
            PgnCharacterClassTable[' '] = WhitespaceCharacter;

            // Treat 0xa0 as a space separator too.
            PgnCharacterClassTable[0xa0] = WhitespaceCharacter;

            new[]
            {
                PgnGameResultSyntax.AsteriskCharacter,
                PgnBracketOpenSyntax.BracketOpenCharacter,
                PgnBracketCloseSyntax.BracketCloseCharacter,
                PgnParenthesisCloseSyntax.ParenthesisCloseCharacter,
                PgnParenthesisOpenSyntax.ParenthesisOpenCharacter,
                PgnPeriodSyntax.PeriodCharacter,
                StringLiteral.QuoteCharacter,
                PgnCommentSyntax.EndOfLineCommentStartCharacter,
                PgnCommentSyntax.MultiLineCommentStartCharacter,
                PgnNagSyntax.NagCharacter,
                PgnEscapeSyntax.EscapeCharacter,
            }.ForEach(c => PgnCharacterClassTable[c] = SpecialCharacter);

            // Digits.
            PgnCharacterClassTable['0'] = PgnSymbolStateMachine.Digit0;
            PgnCharacterClassTable['1'] = PgnSymbolStateMachine.Digit1;
            PgnCharacterClassTable['2'] = PgnSymbolStateMachine.Digit2;
            for (char c = '3'; c <= '8'; c++) PgnCharacterClassTable[c] = PgnSymbolStateMachine.Digit3_8;
            PgnCharacterClassTable['9'] = PgnSymbolStateMachine.Digit9;

            // Letters.
            for (char c = 'A'; c <= 'Z'; c++) PgnCharacterClassTable[c] = PgnSymbolStateMachine.OtherUpperCaseLetter;
            for (char c = 'À'; c <= 'Ö'; c++) PgnCharacterClassTable[c] = PgnSymbolStateMachine.OtherUpperCaseLetter;  //0xc0-0xd6
            for (char c = 'Ø'; c <= 'Þ'; c++) PgnCharacterClassTable[c] = PgnSymbolStateMachine.OtherUpperCaseLetter;  //0xd8-0xde
            for (char c = 'a'; c <= 'h'; c++) PgnCharacterClassTable[c] = PgnSymbolStateMachine.LowercaseAtoH;
            for (char c = 'i'; c <= 'z'; c++) PgnCharacterClassTable[c] = PgnSymbolStateMachine.OtherLowercaseLetter;
            for (char c = 'ß'; c <= 'ö'; c++) PgnCharacterClassTable[c] = PgnSymbolStateMachine.OtherLowercaseLetter;  //0xdf-0xf6
            for (char c = 'ø'; c <= 'ÿ'; c++) PgnCharacterClassTable[c] = PgnSymbolStateMachine.OtherLowercaseLetter;  //0xf8-0xff

            // Treat the underscore as a lower case character.
            PgnCharacterClassTable['_'] = PgnSymbolStateMachine.OtherLowercaseLetter;

            // Special cases.
            PgnCharacterClassTable['O'] = PgnSymbolStateMachine.LetterO;
            PgnCharacterClassTable['P'] = PgnSymbolStateMachine.LetterP;
            PgnMoveFormatter.PieceSymbols.ForEach(c => PgnCharacterClassTable[c] = PgnSymbolStateMachine.OtherPieceLetter);
            PgnCharacterClassTable['x'] = PgnSymbolStateMachine.LowercaseX;
            PgnCharacterClassTable['-'] = PgnSymbolStateMachine.Dash;
            PgnCharacterClassTable['/'] = PgnSymbolStateMachine.Slash;
            PgnCharacterClassTable['='] = PgnSymbolStateMachine.EqualitySign;
            PgnCharacterClassTable['+'] = PgnSymbolStateMachine.PlusOrOctothorpe;
            PgnCharacterClassTable['#'] = PgnSymbolStateMachine.PlusOrOctothorpe;
            PgnCharacterClassTable['!'] = PgnSymbolStateMachine.ExclamationOrQuestionMark;
            PgnCharacterClassTable['?'] = PgnSymbolStateMachine.ExclamationOrQuestionMark;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetCharacterClass(char pgnCharacter)
            => pgnCharacter <= 0xff ? PgnCharacterClassTable[pgnCharacter] : IllegalCharacter;

        #endregion PGN character classes

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

            var parser = new PgnParser();
            parser.ParsePgnText(pgn);
            parser.YieldEof();

            return new RootPgnSyntax(
                parser.SymbolBuilder,
                GreenPgnTriviaSyntax.Create(parser.TriviaBuilder, parser.BackgroundBuilder),
                parser.Errors);
        }

        private readonly List<PgnErrorInfo> Errors;
        private readonly List<IGreenPgnSymbol> BackgroundBuilder;
        private readonly List<GreenPgnTriviaElementSyntax> TriviaBuilder;
        private readonly List<WithTrivia> TagPairBuilder;
        private readonly List<GreenPgnTagPairSyntax> TagSectionBuilder;
        private readonly List<IGreenPgnTopLevelSyntax> SymbolBuilder;

        private int symbolStartIndex;

        private bool InTagPair;
        private int TagPairLeadingTriviaLength;
        private int TagPairStartIndex;
        private bool HasTagPairBracketOpen;
        private bool HasTagPairTagName;
        private bool HasTagPairTagValue;

        private PgnParser()
        {
            Errors = new List<PgnErrorInfo>();
            BackgroundBuilder = new List<IGreenPgnSymbol>();
            TriviaBuilder = new List<GreenPgnTriviaElementSyntax>();
            TagPairBuilder = new List<WithTrivia>();
            TagSectionBuilder = new List<GreenPgnTagPairSyntax>();
            SymbolBuilder = new List<IGreenPgnTopLevelSyntax>();
        }

        #region Tag section parsing

        private void CaptureTagPair(bool hasTagPairBracketClose)
        {
            var tagPairSyntax = new GreenPgnTagPairSyntax(TagPairBuilder);

            // To report tag pair errors, start at the '[', not where its leading trivia starts.
            int tagPairErrorLength = tagPairSyntax.Length - TagPairLeadingTriviaLength;

            // Analyze for errors.
            // Expect '[', tag name. tag value, ']'.
            if (!HasTagPairBracketOpen)
            {
                Errors.Add(new PgnErrorInfo(
                    PgnErrorCode.MissingTagBracketOpen,
                    TagPairStartIndex,
                    tagPairErrorLength));
            }

            if (!HasTagPairTagName)
            {
                if (!HasTagPairTagValue)
                {
                    Errors.Add(new PgnErrorInfo(
                        PgnErrorCode.EmptyTag,
                        TagPairStartIndex,
                        tagPairErrorLength));
                }
                else
                {
                    Errors.Add(new PgnErrorInfo(
                        PgnErrorCode.MissingTagName,
                        TagPairStartIndex,
                        tagPairErrorLength));
                }
            }
            else if (!HasTagPairTagValue)
            {
                Errors.Add(new PgnErrorInfo(
                    PgnErrorCode.MissingTagValue,
                    TagPairStartIndex,
                    tagPairErrorLength));
            }

            if (!hasTagPairBracketClose)
            {
                Errors.Add(new PgnErrorInfo(
                    PgnErrorCode.MissingTagBracketClose,
                    TagPairStartIndex,
                    tagPairErrorLength));
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

        private void AddTagElementToBuilder(GreenPgnTriviaSyntax leadingTrivia, IGreenPgnSymbol node)
        {
            if (!InTagPair)
            {
                InTagPair = true;
                TagPairLeadingTriviaLength = leadingTrivia.Length;
                TagPairStartIndex = symbolStartIndex;
            }

            TagPairBuilder.Add(new WithTrivia(leadingTrivia, node));
        }

        #endregion Tag section parsing

        #region Yield tokens and EOF

        private void Yield(IGreenPgnSymbol symbol)
        {
            Errors.AddRange(symbol.GetErrors(symbolStartIndex));

            var symbolType = symbol.SymbolType;

            if (symbolType.IsBackground())
            {
                BackgroundBuilder.Add(symbol);
            }
            else if (symbolType.IsTrivia())
            {
                TriviaBuilder.Add(new GreenPgnTriviaElementSyntax(BackgroundBuilder, symbol));
                BackgroundBuilder.Clear();
            }
            else
            {
                GreenPgnTriviaSyntax leadingTrivia = GreenPgnTriviaSyntax.Create(TriviaBuilder, BackgroundBuilder);

                switch (symbolType)
                {
                    case PgnSymbolType.BracketOpen:
                        // When encountering a new '[', open a new tag pair.
                        CaptureTagPairIfNecessary();
                        HasTagPairBracketOpen = true;
                        AddTagElementToBuilder(leadingTrivia, symbol);
                        break;
                    case PgnSymbolType.BracketClose:
                        // When encountering a ']', always immediately close this tag pair.
                        AddTagElementToBuilder(leadingTrivia, symbol);
                        CaptureTagPair(hasTagPairBracketClose: true);
                        break;
                    case PgnSymbolType.TagName:
                        // Open a new tag pair if a tag name or value was seen earlier in the same tag pair.
                        if (HasTagPairTagName || HasTagPairTagValue) CaptureTagPair(hasTagPairBracketClose: false);
                        HasTagPairTagName = true;
                        AddTagElementToBuilder(leadingTrivia, symbol);
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
                                symbol.Length));
                        }
                        AddTagElementToBuilder(leadingTrivia, symbol);
                        break;
                    case PgnSymbolType.MoveNumber:
                        // Switch to move tree section.
                        CaptureTagPairIfNecessary();
                        CaptureTagSection();
                        WithTrivia moveNumber = new WithTrivia(leadingTrivia, symbol);
                        SymbolBuilder.Add(new GreenPgnTopLevelSymbolSyntax(moveNumber, (parent, index, green) => new PgnMoveNumberWithTriviaSyntax(parent, index, green)));
                        break;
                    case PgnSymbolType.Period:
                        // Switch to move tree section.
                        CaptureTagPairIfNecessary();
                        CaptureTagSection();
                        WithTrivia period = new WithTrivia(leadingTrivia, symbol);
                        SymbolBuilder.Add(new GreenPgnTopLevelSymbolSyntax(period, (parent, index, green) => new PgnPeriodWithTriviaSyntax(parent, index, green)));
                        break;
                    case PgnSymbolType.Move:
                    case PgnSymbolType.UnrecognizedMove:
                        // Switch to move tree section.
                        CaptureTagPairIfNecessary();
                        CaptureTagSection();
                        WithTrivia move = new WithTrivia(leadingTrivia, symbol);
                        SymbolBuilder.Add(new GreenPgnTopLevelSymbolSyntax(move, (parent, index, green) => new PgnMoveWithTriviaSyntax(parent, index, green)));
                        break;
                    case PgnSymbolType.Nag:
                    case PgnSymbolType.EmptyNag:
                    case PgnSymbolType.OverflowNag:
                        // Switch to move tree section.
                        CaptureTagPairIfNecessary();
                        CaptureTagSection();
                        WithTrivia nag = new WithTrivia(leadingTrivia, symbol);
                        SymbolBuilder.Add(new GreenPgnTopLevelSymbolSyntax(nag, (parent, index, green) => new PgnNagWithTriviaSyntax(parent, index, green)));
                        break;
                    case PgnSymbolType.ParenthesisOpen:
                        // Switch to move tree section.
                        CaptureTagPairIfNecessary();
                        CaptureTagSection();
                        WithTrivia parenthesisOpen = new WithTrivia(leadingTrivia, symbol);
                        SymbolBuilder.Add(new GreenPgnTopLevelSymbolSyntax(parenthesisOpen, (parent, index, green) => new PgnParenthesisOpenWithTriviaSyntax(parent, index, green)));
                        break;
                    case PgnSymbolType.ParenthesisClose:
                        // Switch to move tree section.
                        CaptureTagPairIfNecessary();
                        CaptureTagSection();
                        WithTrivia parenthesisClose = new WithTrivia(leadingTrivia, symbol);
                        SymbolBuilder.Add(new GreenPgnTopLevelSymbolSyntax(parenthesisClose, (parent, index, green) => new PgnParenthesisCloseWithTriviaSyntax(parent, index, green)));
                        break;
                    case PgnSymbolType.Asterisk:
                    case PgnSymbolType.DrawMarker:
                    case PgnSymbolType.WhiteWinMarker:
                    case PgnSymbolType.BlackWinMarker:
                        CaptureTagPairIfNecessary();
                        CaptureTagSection();
                        WithTrivia gameResult = new WithTrivia(leadingTrivia, symbol);
                        SymbolBuilder.Add(new GreenPgnTopLevelSymbolSyntax(gameResult, (parent, index, green) => new PgnGameResultWithTriviaSyntax(parent, index, green)));
                        break;
                    default:
                        throw new UnreachableException();
                }

                BackgroundBuilder.Clear();
                TriviaBuilder.Clear();
            }
        }

        private void YieldEof()
        {
            CaptureTagPairIfNecessary();
            CaptureTagSection();
        }

        #endregion Yield tokens and EOF

        #region Lexing

        private GreenPgnIllegalCharacterSyntax CreateIllegalCharacterSyntax(char c, int position)
        {
            Errors.Add(PgnIllegalCharacterSyntax.CreateError(
                StringLiteral.CharacterMustBeEscaped(c)
                ? StringLiteral.EscapedCharacterString(c)
                : Convert.ToString(c), position));

            return GreenPgnIllegalCharacterSyntax.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void YieldPgnSymbol(ref PgnSymbolStateMachine symbolBuilder, string pgnText, int symbolStartIndex, int length)
        {
            IGreenPgnSymbol symbol = symbolBuilder.Yield(length);

            if (symbol == null)
            {
                Yield(new GreenPgnUnrecognizedMoveSyntax(pgnText.Substring(symbolStartIndex, length)));
            }
            else
            {
                Yield(symbol);
            }
        }

        private void ParsePgnText(string pgnText)
        {
            // This tokenizer uses labels with goto to switch between modes of tokenization.

            int length = symbolStartIndex + pgnText.Length;

            int currentIndex = symbolStartIndex;
            StringBuilder valueBuilder = new StringBuilder();
            List<PgnErrorInfo> errors = new List<PgnErrorInfo>();
            bool hasStringErrors;

            // Reusable structure to build green PGN symbols.
            PgnSymbolStateMachine symbolBuilder = default;

        inWhitespace:

            while (currentIndex < length)
            {
                char c = pgnText[currentIndex];
                int characterClass = GetCharacterClass(c);

                if (characterClass != WhitespaceCharacter)
                {
                    if (symbolStartIndex < currentIndex)
                    {
                        Yield(GreenPgnWhitespaceSyntax.Create(currentIndex - symbolStartIndex));
                        symbolStartIndex = currentIndex;
                    }

                    if (characterClass != IllegalCharacter)
                    {
                        int symbolCharacterClass = characterClass & SymbolCharacterMask;
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
                                Yield(CreateIllegalCharacterSyntax(c, symbolStartIndex));
                                symbolStartIndex++;
                                break;
                            default:
                                throw new InvalidOperationException("Case statement on special characters is not exhaustive.");
                        }
                    }
                    else
                    {
                        Yield(CreateIllegalCharacterSyntax(c, symbolStartIndex));
                        symbolStartIndex++;
                    }
                }

                currentIndex++;
            }

            if (symbolStartIndex < currentIndex)
            {
                Yield(GreenPgnWhitespaceSyntax.Create(currentIndex - symbolStartIndex));
            }

            return;

        inSymbol:

            // Eat the first symbol character, but leave symbolStartIndex unchanged.
            currentIndex++;

            IGreenPgnSymbol symbolToYield;

            while (currentIndex < length)
            {
                char c = pgnText[currentIndex];
                int characterClass = GetCharacterClass(c);

                if (characterClass == WhitespaceCharacter)
                {
                    if (symbolStartIndex < currentIndex)
                    {
                        YieldPgnSymbol(ref symbolBuilder, pgnText, symbolStartIndex, currentIndex - symbolStartIndex);
                        symbolStartIndex = currentIndex;
                    }

                    currentIndex++;
                    goto inWhitespace;
                }

                if (characterClass != IllegalCharacter)
                {
                    int symbolCharacterClass = characterClass & SymbolCharacterMask;
                    if (symbolCharacterClass != 0)
                    {
                        symbolBuilder.Transition(symbolCharacterClass);
                    }
                    else
                    {
                        switch (c)
                        {
                            case PgnGameResultSyntax.AsteriskCharacter:
                                symbolToYield = GreenPgnAsteriskSyntax.Value;
                                goto yieldSymbolThenCharacter;
                            case PgnBracketOpenSyntax.BracketOpenCharacter:
                                symbolToYield = GreenPgnBracketOpenSyntax.Value;
                                goto yieldSymbolThenCharacter;
                            case PgnBracketCloseSyntax.BracketCloseCharacter:
                                symbolToYield = GreenPgnBracketCloseSyntax.Value;
                                goto yieldSymbolThenCharacter;
                            case PgnParenthesisCloseSyntax.ParenthesisCloseCharacter:
                                symbolToYield = GreenPgnParenthesisCloseSyntax.Value;
                                goto yieldSymbolThenCharacter;
                            case PgnParenthesisOpenSyntax.ParenthesisOpenCharacter:
                                symbolToYield = GreenPgnParenthesisOpenSyntax.Value;
                                goto yieldSymbolThenCharacter;
                            case PgnPeriodSyntax.PeriodCharacter:
                                symbolToYield = GreenPgnPeriodSyntax.Value;
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
                                symbolToYield = CreateIllegalCharacterSyntax(c, currentIndex);
                                goto yieldSymbolThenCharacter;
                            default:
                                throw new InvalidOperationException("Case statement on special characters is not exhaustive.");
                        }
                    }
                }
                else
                {
                    symbolToYield = CreateIllegalCharacterSyntax(c, currentIndex);
                    goto yieldSymbolThenCharacter;
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
            currentIndex++;
            Yield(symbolToYield);
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
                        Yield(new GreenPgnErrorTagValueSyntax(currentIndex - symbolStartIndex, errors));
                        errors.Clear();
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
                                errors.Add(PgnTagValueSyntax.IllegalControlCharacterError(escapedChar, currentIndex - symbolStartIndex));
                            }

                            if (StringLiteral.CharacterMustBeEscaped(escapedChar))
                            {
                                // Just don't show the control character.
                                errors.Add(PgnTagValueSyntax.UnrecognizedEscapeSequenceError(
                                    EscapeCharacterString,
                                    escapeSequenceStart - symbolStartIndex,
                                    2));
                            }
                            else
                            {
                                errors.Add(PgnTagValueSyntax.UnrecognizedEscapeSequenceError(
                                    new string(new[] { StringLiteral.EscapeCharacter, escapedChar }),
                                    escapeSequenceStart - symbolStartIndex,
                                    2));
                            }
                        }
                    }
                    else
                    {
                        // In addition to this, break out of the loop because this is now also an unterminated string.
                        errors.Add(PgnTagValueSyntax.UnrecognizedEscapeSequenceError(
                            EscapeCharacterString,
                            escapeSequenceStart - symbolStartIndex,
                            1));

                        break;
                    }
                }
                else if (char.IsControl(c))
                {
                    hasStringErrors = true;
                    errors.Add(PgnTagValueSyntax.IllegalControlCharacterError(c, currentIndex - symbolStartIndex));
                }
                else
                {
                    valueBuilder.Append(c);
                }

                currentIndex++;
            }

            errors.Add(PgnTagValueSyntax.UnterminatedError(length - symbolStartIndex));

            Yield(new GreenPgnErrorTagValueSyntax(length - symbolStartIndex, errors));
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
                            Yield(new GreenPgnCommentSyntax(currentIndex - 1 - symbolStartIndex));

                            // Eat the '\n'.
                            symbolStartIndex = currentIndex - 1;
                            currentIndex++;
                            goto inWhitespace;
                        }
                    }
                }
                else if (c == '\n')
                {
                    Yield(new GreenPgnCommentSyntax(currentIndex - symbolStartIndex));

                    // Eat the '\n'.
                    symbolStartIndex = currentIndex;
                    currentIndex++;
                    goto inWhitespace;
                }

                currentIndex++;
            }

            Yield(new GreenPgnCommentSyntax(length - symbolStartIndex));
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
                            Yield(new GreenPgnEscapeSyntax(currentIndex - 1 - symbolStartIndex));
                            symbolStartIndex = currentIndex - 1;
                            currentIndex++;
                            goto inWhitespace;
                        }
                    }
                }
                else if (c == '\n')
                {
                    Yield(new GreenPgnEscapeSyntax(currentIndex - symbolStartIndex));
                    symbolStartIndex = currentIndex;
                    currentIndex++;
                    goto inWhitespace;
                }
                currentIndex++;
            }

            Yield(new GreenPgnEscapeSyntax(length - symbolStartIndex));
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
                    Yield(new GreenPgnCommentSyntax(currentIndex - symbolStartIndex));
                    symbolStartIndex = currentIndex;
                    goto inWhitespace;
                }
            }

            Yield(new GreenPgnUnterminatedCommentSyntax(length - symbolStartIndex));
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
                    if (emptyNag) Yield(GreenPgnEmptyNagSyntax.Value);
                    else if (!overflowNag) Yield(new GreenPgnNagSyntax((PgnAnnotation)annotationValue, currentIndex - symbolStartIndex));
                    else Yield(new GreenPgnOverflowNagSyntax(pgnText.Substring(symbolStartIndex, currentIndex - symbolStartIndex)));

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

            if (emptyNag) Yield(GreenPgnEmptyNagSyntax.Value);
            else if (!overflowNag) Yield(new GreenPgnNagSyntax((PgnAnnotation)annotationValue, length - symbolStartIndex));
            else Yield(new GreenPgnOverflowNagSyntax(pgnText.Substring(symbolStartIndex, currentIndex - symbolStartIndex)));
        }

        #endregion Lexing
    }
}

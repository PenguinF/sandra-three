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
        internal const PgnSymbolType ForegroundThreshold = PgnSymbolType.Asterisk;

        #region PGN character classes

        private const ulong IllegalCharacter = 0;
        private const ulong WhitespaceCharacter = 0x1;
        private const ulong SymbolCharacter = 0x1 << 1;
        private const ulong UppercaseLetterCharacter = 0x1 << 2;
        private const ulong LowercaseLetterCharacter = 0x1 << 3;
        private const ulong DigitCharacter = 0x1 << 4;

        /// <summary>
        /// Contains a bitfield of character classes relevant for PGN, for each 8-bit character.
        /// A value of 0 means the character is not allowed.
        /// </summary>
        private static readonly ulong[] PgnCharacterClassTable = new ulong[0x100];

        static PgnParser()
        {
            // 0x00..0x20: treat 4 control characters and ' ' as whitespace.
            PgnCharacterClassTable['\t'] |= WhitespaceCharacter;
            PgnCharacterClassTable['\n'] |= WhitespaceCharacter;
            PgnCharacterClassTable['\v'] |= WhitespaceCharacter;
            PgnCharacterClassTable['\r'] |= WhitespaceCharacter;
            PgnCharacterClassTable[' '] |= WhitespaceCharacter;

            // 0x21..0x7e
            for (char c = '!'; c <= '~'; c++) PgnCharacterClassTable[c] |= SymbolCharacter;

            // 0xa0..0xbf: discouraged but allowed.
            // Treat 0xa0 as a space separator.
            PgnCharacterClassTable[0xa0] |= WhitespaceCharacter;
            for (char c = '¡'; c <= '¿'; c++) PgnCharacterClassTable[c] |= SymbolCharacter;

            // 0xc0..0xff: allowed and encouraged.
            for (char c = 'À'; c <= 'ÿ'; c++) PgnCharacterClassTable[c] |= SymbolCharacter;

            // Letters, digits.
            for (char c = '0'; c <= '9'; c++) PgnCharacterClassTable[c] |= DigitCharacter;
            for (char c = 'A'; c <= 'Z'; c++) PgnCharacterClassTable[c] |= UppercaseLetterCharacter;
            for (char c = 'À'; c <= 'Ö'; c++) PgnCharacterClassTable[c] |= UppercaseLetterCharacter;  //0xc0-0xd6
            for (char c = 'Ø'; c <= 'Þ'; c++) PgnCharacterClassTable[c] |= UppercaseLetterCharacter;  //0xd8-0xde
            for (char c = 'a'; c <= 'z'; c++) PgnCharacterClassTable[c] |= LowercaseLetterCharacter;
            for (char c = 'ß'; c <= 'ö'; c++) PgnCharacterClassTable[c] |= LowercaseLetterCharacter;  //0xdf-0xf6
            for (char c = 'ø'; c <= 'ÿ'; c++) PgnCharacterClassTable[c] |= LowercaseLetterCharacter;  //0xf8-0xff

            // Treat the underscore as a lower case character.
            PgnCharacterClassTable['_'] |= LowercaseLetterCharacter;

            // < and > are reserved for future expansion according to the PGN spec. Therefore treat as illegal.
            PgnCharacterClassTable['<'] = 0;
            PgnCharacterClassTable['>'] = 0;

            // Special case: this character delimits multi-line comments, and is otherwise only valid inside strings.
            PgnCharacterClassTable['}'] = 0;
        }

        #endregion PGN character classes

        /// <summary>
        /// See also <see cref="StringLiteral.EscapeCharacter"/>.
        /// </summary>
        private static readonly string EscapeCharacterString = "\\";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static GreenPgnIllegalCharacterSyntax CreateIllegalCharacterSyntax(char c)
            => new GreenPgnIllegalCharacterSyntax(
                StringLiteral.CharacterMustBeEscaped(c)
                ? StringLiteral.EscapedCharacterString(c)
                : Convert.ToString(c));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IGreenPgnSymbol CreatePgnSymbol(bool allLegalTagNameCharacters, int length)
        {
            if (allLegalTagNameCharacters) return new GreenPgnTagNameSyntax(length);
            return new GreenPgnUnknownSymbolSyntax(length);
        }

        /// <summary>
        /// Tokenizes source text in the PGN format.
        /// </summary>
        /// <param name="pgnText">
        /// The PGN to tokenize.
        /// </param>
        /// <returns>
        /// An enumeration of <see cref="IGreenPgnSymbol"/> instances.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="pgnText"/> is null/
        /// </exception>
        public static IEnumerable<IGreenPgnSymbol> TokenizeAll(string pgnText)
        {
            // This tokenizer uses labels with goto to switch between modes of tokenization.

            if (pgnText == null) throw new ArgumentNullException(nameof(pgnText));
            int length = pgnText.Length;

            int currentIndex = 0;
            int symbolStartIndex = 0;
            StringBuilder valueBuilder = new StringBuilder();
            List<PgnErrorInfo> errors = new List<PgnErrorInfo>();

            // Keep track of whether characters were found that cannot be in tag names.
            bool allLegalTagNameCharacters;

        inWhitespace:

            while (currentIndex < length)
            {
                char c = pgnText[currentIndex];
                ulong characterClass = c <= 0xff ? PgnCharacterClassTable[c] : IllegalCharacter;

                if (characterClass != WhitespaceCharacter)
                {
                    if (symbolStartIndex < currentIndex)
                    {
                        yield return GreenPgnWhitespaceSyntax.Create(currentIndex - symbolStartIndex);
                        symbolStartIndex = currentIndex;
                    }

                    if (characterClass != IllegalCharacter)
                    {
                        switch (c)
                        {
                            case PgnAsteriskSyntax.AsteriskCharacter:
                                yield return GreenPgnAsteriskSyntax.Value;
                                symbolStartIndex++;
                                break;
                            case PgnBracketOpenSyntax.BracketOpenCharacter:
                                yield return GreenPgnBracketOpenSyntax.Value;
                                symbolStartIndex++;
                                break;
                            case PgnBracketCloseSyntax.BracketCloseCharacter:
                                yield return GreenPgnBracketCloseSyntax.Value;
                                symbolStartIndex++;
                                break;
                            case PgnParenthesisCloseSyntax.ParenthesisCloseCharacter:
                                yield return GreenPgnParenthesisCloseSyntax.Value;
                                symbolStartIndex++;
                                break;
                            case PgnParenthesisOpenSyntax.ParenthesisOpenCharacter:
                                yield return GreenPgnParenthesisOpenSyntax.Value;
                                symbolStartIndex++;
                                break;
                            case PgnPeriodSyntax.PeriodCharacter:
                                yield return GreenPgnPeriodSyntax.Value;
                                symbolStartIndex++;
                                break;
                            case StringLiteral.QuoteCharacter:
                                goto inString;
                            case PgnCommentSyntax.EndOfLineCommentStartCharacter:
                                goto inEndOfLineComment;
                            case PgnCommentSyntax.MultiLineCommentStartCharacter:
                                goto inMultiLineComment;
                            case PgnEscapeSyntax.EscapeCharacter:
                                // Escape mechanism only triggered directly after a newline.
                                if (currentIndex == 0 || pgnText[currentIndex - 1] == '\n') goto inEscapeSequence;
                                goto default;
                            default:
                                // Tag names must start with an uppercase letter.
                                allLegalTagNameCharacters = characterClass.Test(UppercaseLetterCharacter | LowercaseLetterCharacter);
                                goto inSymbol;
                        }
                    }
                    else
                    {
                        yield return CreateIllegalCharacterSyntax(c);
                        symbolStartIndex++;
                    }
                }

                currentIndex++;
            }

            if (symbolStartIndex < currentIndex)
            {
                yield return GreenPgnWhitespaceSyntax.Create(currentIndex - symbolStartIndex);
            }

            yield break;

        inSymbol:

            // Eat the first symbol character, but leave symbolStartIndex unchanged.
            currentIndex++;

            IGreenPgnSymbol symbolToYield;

            while (currentIndex < length)
            {
                char c = pgnText[currentIndex];
                ulong characterClass = c <= 0xff ? PgnCharacterClassTable[c] : IllegalCharacter;

                if (characterClass == WhitespaceCharacter)
                {
                    if (symbolStartIndex < currentIndex)
                    {
                        yield return CreatePgnSymbol(allLegalTagNameCharacters, currentIndex - symbolStartIndex);
                        symbolStartIndex = currentIndex;
                    }

                    currentIndex++;
                    goto inWhitespace;
                }

                if (characterClass != IllegalCharacter)
                {
                    switch (c)
                    {
                        case PgnAsteriskSyntax.AsteriskCharacter:
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
                            if (symbolStartIndex < currentIndex) yield return CreatePgnSymbol(allLegalTagNameCharacters, currentIndex - symbolStartIndex);
                            symbolStartIndex = currentIndex;
                            goto inString;
                        case PgnCommentSyntax.EndOfLineCommentStartCharacter:
                            if (symbolStartIndex < currentIndex) yield return CreatePgnSymbol(allLegalTagNameCharacters, currentIndex - symbolStartIndex);
                            symbolStartIndex = currentIndex;
                            goto inEndOfLineComment;
                        case PgnCommentSyntax.MultiLineCommentStartCharacter:
                            if (symbolStartIndex < currentIndex) yield return CreatePgnSymbol(allLegalTagNameCharacters, currentIndex - symbolStartIndex);
                            symbolStartIndex = currentIndex;
                            goto inMultiLineComment;
                    }

                    // Allow only digits, letters or the underscore character in tag names.
                    const ulong letterOrDigit = UppercaseLetterCharacter | LowercaseLetterCharacter | DigitCharacter;
                    allLegalTagNameCharacters = allLegalTagNameCharacters && characterClass.Test(letterOrDigit);
                }
                else
                {
                    symbolToYield = CreateIllegalCharacterSyntax(c);
                    goto yieldSymbolThenCharacter;
                }

                currentIndex++;
            }

            if (symbolStartIndex < currentIndex)
            {
                yield return CreatePgnSymbol(allLegalTagNameCharacters, currentIndex - symbolStartIndex);
            }

            yield break;

        yieldSymbolThenCharacter:

            // Yield a GreenPgnSymbol, then symbolToYield, then go to whitespace.
            if (symbolStartIndex < currentIndex) yield return CreatePgnSymbol(allLegalTagNameCharacters, currentIndex - symbolStartIndex);
            yield return symbolToYield;
            currentIndex++;
            symbolStartIndex = currentIndex;
            goto inWhitespace;

        inString:

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

                    if (errors.Count > 0)
                    {
                        yield return new GreenPgnErrorTagValueSyntax(currentIndex - symbolStartIndex, errors);
                        errors.Clear();
                    }
                    else
                    {
                        yield return new GreenPgnTagValueSyntax(valueBuilder.ToString(), currentIndex - symbolStartIndex);
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
                            if (char.IsControl(escapedChar))
                            {
                                errors.Add(PgnErrorTagValueSyntax.IllegalControlCharacter(escapedChar, currentIndex - symbolStartIndex));
                            }

                            if (StringLiteral.CharacterMustBeEscaped(escapedChar))
                            {
                                // Just don't show the control character.
                                errors.Add(PgnErrorTagValueSyntax.UnrecognizedEscapeSequence(
                                    EscapeCharacterString,
                                    escapeSequenceStart - symbolStartIndex,
                                    2));
                            }
                            else
                            {
                                errors.Add(PgnErrorTagValueSyntax.UnrecognizedEscapeSequence(
                                    new string(new[] { StringLiteral.EscapeCharacter, escapedChar }),
                                    escapeSequenceStart - symbolStartIndex,
                                    2));
                            }
                        }
                    }
                    else
                    {
                        // In addition to this, break out of the loop because this is now also an unterminated string.
                        errors.Add(PgnErrorTagValueSyntax.UnrecognizedEscapeSequence(
                            EscapeCharacterString,
                            escapeSequenceStart - symbolStartIndex,
                            1));

                        break;
                    }
                }
                else if (char.IsControl(c))
                {
                    errors.Add(PgnErrorTagValueSyntax.IllegalControlCharacter(c, currentIndex - symbolStartIndex));
                }
                else
                {
                    valueBuilder.Append(c);
                }

                currentIndex++;
            }

            errors.Add(PgnErrorTagValueSyntax.Unterminated(symbolStartIndex, length - symbolStartIndex));

            yield return new GreenPgnErrorTagValueSyntax(length - symbolStartIndex, errors);
            yield break;

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
                            yield return new GreenPgnCommentSyntax(currentIndex - 1 - symbolStartIndex);

                            // Eat the '\n'.
                            symbolStartIndex = currentIndex - 1;
                            currentIndex++;
                            goto inWhitespace;
                        }
                    }
                }
                else if (c == '\n')
                {
                    yield return new GreenPgnCommentSyntax(currentIndex - symbolStartIndex);

                    // Eat the '\n'.
                    symbolStartIndex = currentIndex;
                    currentIndex++;
                    goto inWhitespace;
                }

                currentIndex++;
            }

            yield return new GreenPgnCommentSyntax(length - symbolStartIndex);
            yield break;

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
                            yield return new GreenPgnEscapeSyntax(currentIndex - 1 - symbolStartIndex);
                            symbolStartIndex = currentIndex - 1;
                            currentIndex++;
                            goto inWhitespace;
                        }
                    }
                }
                else if (c == '\n')
                {
                    yield return new GreenPgnEscapeSyntax(currentIndex - symbolStartIndex);
                    symbolStartIndex = currentIndex;
                    currentIndex++;
                    goto inWhitespace;
                }
                currentIndex++;
            }

            yield return new GreenPgnEscapeSyntax(length - symbolStartIndex);
            yield break;

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
                    yield return new GreenPgnCommentSyntax(currentIndex - symbolStartIndex);
                    symbolStartIndex = currentIndex;
                    goto inWhitespace;
                }
            }

            yield return new GreenPgnUnterminatedCommentSyntax(length - symbolStartIndex);
        }

        /// <summary>
        /// Parses source text in the PGN format.
        /// </summary>
        /// <param name="pgn">
        /// The source text to parse.
        /// </param>
        /// <returns>
        /// A <see cref="RootPgnSyntax"/> containing the parse syntax tree and parse errors.
        /// </returns>
        public static RootPgnSyntax Parse(string pgn)
        {
            var terminalList = new List<IGreenPgnSymbol>(TokenizeAll(pgn));

            int startPosition = 0;
            var errors = new List<PgnErrorInfo>();
            foreach (var terminal in terminalList)
            {
                errors.AddRange(terminal.GetErrors(startPosition));
                startPosition += terminal.Length;
            }

            return new RootPgnSyntax(new GreenPgnSyntaxNodes(terminalList), errors);
        }

        private PgnParser() { }
    }
}

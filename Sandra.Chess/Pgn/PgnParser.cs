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
using System.Text;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Partitions a source PGN file into separate tokens, then generates an abstract syntax tree from it.
    /// </summary>
    public sealed class PgnParser
    {
        /// <summary>
        /// See also <see cref="StringLiteral.EscapeCharacter"/>.
        /// </summary>
        private static readonly string EscapeCharacterString = "\\";

        private static GreenPgnIllegalCharacterSyntax CreateIllegalCharacterSyntax(char c)
            => new GreenPgnIllegalCharacterSyntax(
                StringLiteral.CharacterMustBeEscaped(c)
                ? StringLiteral.EscapedCharacterString(c)
                : Convert.ToString(c));

        private static IPgnForegroundSymbol CreatePgnSymbol(bool allLegalTagNameCharacters, int length)
        {
            if (allLegalTagNameCharacters) return new GreenPgnTagNameSyntax(length);
            return new GreenPgnSymbol(length);
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
            int firstUnusedIndex = 0;
            StringBuilder valueBuilder = new StringBuilder();
            List<PgnErrorInfo> errors = new List<PgnErrorInfo>();

            // Keep track of whether characters were found that cannot be in tag names.
            bool allLegalTagNameCharacters;

        inWhitespace:

            while (currentIndex < length)
            {
                char c = pgnText[currentIndex];

                // Treat all control characters as whitespace.
                if (c > ' ')
                {
                    if (firstUnusedIndex < currentIndex)
                    {
                        yield return GreenPgnWhitespaceSyntax.Create(currentIndex - firstUnusedIndex);
                        firstUnusedIndex = currentIndex;
                    }

                    // All legal PGN characters have a value below 0x7F.
                    if (c <= 0x7e)
                    {
                        switch (c)
                        {
                            case PgnAsteriskSyntax.AsteriskCharacter:
                                yield return GreenPgnAsteriskSyntax.Value;
                                firstUnusedIndex++;
                                break;
                            case PgnBracketOpenSyntax.BracketOpenCharacter:
                                yield return GreenPgnBracketOpenSyntax.Value;
                                firstUnusedIndex++;
                                break;
                            case PgnBracketCloseSyntax.BracketCloseCharacter:
                                yield return GreenPgnBracketCloseSyntax.Value;
                                firstUnusedIndex++;
                                break;
                            case PgnParenthesisCloseSyntax.ParenthesisCloseCharacter:
                                yield return GreenPgnParenthesisCloseSyntax.Value;
                                firstUnusedIndex++;
                                break;
                            case PgnParenthesisOpenSyntax.ParenthesisOpenCharacter:
                                yield return GreenPgnParenthesisOpenSyntax.Value;
                                firstUnusedIndex++;
                                break;
                            case PgnPeriodSyntax.PeriodCharacter:
                                yield return GreenPgnPeriodSyntax.Value;
                                firstUnusedIndex++;
                                break;
                            case StringLiteral.QuoteCharacter:
                                goto inString;
                            default:
                                // Tag names must start with an uppercase letter.
                                allLegalTagNameCharacters = c >= 'A' && c <= 'Z';
                                goto inSymbol;
                        }
                    }
                    else
                    {
                        yield return CreateIllegalCharacterSyntax(c);
                        firstUnusedIndex++;
                    }
                }

                currentIndex++;
            }

            if (firstUnusedIndex < currentIndex)
            {
                yield return GreenPgnWhitespaceSyntax.Create(currentIndex - firstUnusedIndex);
            }

            yield break;

        inSymbol:

            // Eat the first symbol character, but leave firstUnusedIndex unchanged.
            currentIndex++;

            IGreenPgnSymbol symbolToYield;

            while (currentIndex < length)
            {
                char c = pgnText[currentIndex];

                // Treat all control characters as whitespace.
                if (c <= ' ')
                {
                    if (firstUnusedIndex < currentIndex)
                    {
                        yield return CreatePgnSymbol(allLegalTagNameCharacters, currentIndex - firstUnusedIndex);
                        firstUnusedIndex = currentIndex;
                    }

                    currentIndex++;
                    goto inWhitespace;
                }

                // All legal PGN characters have a value below 0x7F.
                if (c <= 0x7e)
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
                            if (firstUnusedIndex < currentIndex) yield return CreatePgnSymbol(allLegalTagNameCharacters, currentIndex - firstUnusedIndex);
                            firstUnusedIndex = currentIndex;
                            goto inString;
                        default:
                            // Allow only digits, letters or the underscore character in tag names.
                            if (allLegalTagNameCharacters
                                && (c < '0' || c > '9' && c < 'A' || c > 'Z' && c != '_' && c < 'a' || c > 'z'))
                            {
                                allLegalTagNameCharacters = false;
                            }
                            break;
                    }
                }
                else
                {
                    symbolToYield = CreateIllegalCharacterSyntax(c);
                    goto yieldSymbolThenCharacter;
                }

                currentIndex++;
            }

            if (firstUnusedIndex < currentIndex)
            {
                yield return CreatePgnSymbol(allLegalTagNameCharacters, currentIndex - firstUnusedIndex);
            }

            yield break;

        yieldSymbolThenCharacter:

            // Yield a GreenPgnSymbol, then symbolToYield, then go to whitespace.
            if (firstUnusedIndex < currentIndex) yield return CreatePgnSymbol(allLegalTagNameCharacters, currentIndex - firstUnusedIndex);
            yield return symbolToYield;
            currentIndex++;
            firstUnusedIndex = currentIndex;
            goto inWhitespace;

        inString:

            // Eat " character, but leave firstUnusedIndex unchanged.
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
                        yield return new GreenPgnErrorTagValueSyntax(currentIndex - firstUnusedIndex, errors);
                        errors.Clear();
                    }
                    else
                    {
                        yield return new GreenPgnTagValueSyntax(valueBuilder.ToString(), currentIndex - firstUnusedIndex);
                    }

                    valueBuilder.Clear();
                    firstUnusedIndex = currentIndex;
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
                                errors.Add(PgnErrorTagValueSyntax.IllegalControlCharacter(escapedChar, currentIndex));
                            }

                            if (StringLiteral.CharacterMustBeEscaped(escapedChar))
                            {
                                // Just don't show the control character.
                                errors.Add(PgnErrorTagValueSyntax.UnrecognizedEscapeSequence(
                                    EscapeCharacterString,
                                    escapeSequenceStart - firstUnusedIndex,
                                    2));
                            }
                            else
                            {
                                errors.Add(PgnErrorTagValueSyntax.UnrecognizedEscapeSequence(
                                    new string(new[] { StringLiteral.EscapeCharacter, escapedChar }),
                                    escapeSequenceStart - firstUnusedIndex,
                                    2));
                            }
                        }
                    }
                    else
                    {
                        // In addition to this, break out of the loop because this is now also an unterminated string.
                        errors.Add(PgnErrorTagValueSyntax.UnrecognizedEscapeSequence(
                            EscapeCharacterString,
                            escapeSequenceStart - firstUnusedIndex,
                            1));

                        break;
                    }
                }
                else if (char.IsControl(c))
                {
                    errors.Add(PgnErrorTagValueSyntax.IllegalControlCharacter(c, currentIndex));
                }
                else
                {
                    valueBuilder.Append(c);
                }

                currentIndex++;
            }

            errors.Add(PgnErrorTagValueSyntax.Unterminated(firstUnusedIndex, length - firstUnusedIndex));

            yield return new GreenPgnErrorTagValueSyntax(length - firstUnusedIndex, errors);
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

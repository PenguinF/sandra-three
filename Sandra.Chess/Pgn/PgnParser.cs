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

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Partitions a source PGN file into separate tokens, then generates an abstract syntax tree from it.
    /// </summary>
    public sealed class PgnParser
    {
        private static GreenPgnIllegalCharacterSyntax CreateIllegalCharacterSyntax(char c)
        {
            var category = char.GetUnicodeCategory(c);

            string displayCharValue = category == UnicodeCategory.OtherNotAssigned
                                   || category == UnicodeCategory.Control
                ? $"\\u{((int)c).ToString("x4")}"
                : Convert.ToString(c);

            return new GreenPgnIllegalCharacterSyntax(displayCharValue);
        }

        private static IPgnForegroundSymbol CreatePgnSymbol(int length)
        {
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
                            case PgnBracketStartSyntax.BracketStartCharacter:
                                yield return GreenPgnBracketStartSyntax.Value;
                                firstUnusedIndex++;
                                break;
                            case PgnBracketEndSyntax.BracketEndCharacter:
                                yield return GreenPgnBracketEndSyntax.Value;
                                firstUnusedIndex++;
                                break;
                            default:
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
                        yield return CreatePgnSymbol(currentIndex - firstUnusedIndex);
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
                        case PgnBracketStartSyntax.BracketStartCharacter:
                            symbolToYield = GreenPgnBracketStartSyntax.Value;
                            goto yieldSymbolThenCharacter;
                        case PgnBracketEndSyntax.BracketEndCharacter:
                            symbolToYield = GreenPgnBracketEndSyntax.Value;
                            goto yieldSymbolThenCharacter;
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
                yield return CreatePgnSymbol(currentIndex - firstUnusedIndex);
            }

            yield break;

        yieldSymbolThenCharacter:

            // Yield a GreenPgnSymbol, then symbolToYield, then go to whitespace.
            if (firstUnusedIndex < currentIndex) yield return CreatePgnSymbol(currentIndex - firstUnusedIndex);
            yield return symbolToYield;
            currentIndex++;
            firstUnusedIndex = currentIndex;
            goto inWhitespace;
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

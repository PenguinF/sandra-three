#region License
/*********************************************************************************
 * JsonTokenizer.cs
 * 
 * Copyright (c) 2004-2018 Henk Nicolai
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
 *********************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Sandra.UI.WF.Storage
{
    /// <summary>
    /// Based on https://www.json.org/.
    /// </summary>
    public sealed class JsonTokenizer
    {
        private readonly string json;
        private readonly int length;

        // Current state.
        private int currentIndex;
        private int firstUnusedIndex;
        private Func<IEnumerable<JsonTerminalSymbol>> currentTokenizer;

        /// <summary>
        /// Gets the JSON which is tokenized.
        /// </summary>
        public string Json => json;

        /// <summary>
        /// Initializes a new instance of <see cref="JsonTokenizer"/>.
        /// </summary>
        /// <param name="json">
        /// The JSON to tokenize.
        /// </param>
        public JsonTokenizer(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            this.json = json;
            length = json.Length;
            currentIndex = 0;
            firstUnusedIndex = 0;
            currentTokenizer = Default;
        }

        private IEnumerable<JsonTerminalSymbol> Default()
        {
            while (currentIndex < length)
            {
                char c = json[currentIndex];

                bool isSeparator = false;
                bool isSymbol = false;

                var category = char.GetUnicodeCategory(c);
                switch (category)
                {
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.TitlecaseLetter:
                    case UnicodeCategory.ModifierLetter:
                    case UnicodeCategory.OtherLetter:
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.SpacingCombiningMark:
                    case UnicodeCategory.EnclosingMark:
                    case UnicodeCategory.DecimalDigitNumber:
                    case UnicodeCategory.LetterNumber:
                    case UnicodeCategory.OtherNumber:
                    case UnicodeCategory.Surrogate:
                    case UnicodeCategory.ConnectorPunctuation:  // underscore-like characters
                    case UnicodeCategory.DashPunctuation:
                        // Treat as part of a value.
                        break;
                    case UnicodeCategory.OpenPunctuation:
                    case UnicodeCategory.ClosePunctuation:
                    case UnicodeCategory.InitialQuotePunctuation:
                    case UnicodeCategory.FinalQuotePunctuation:
                    case UnicodeCategory.CurrencySymbol:
                    case UnicodeCategory.ModifierSymbol:
                    case UnicodeCategory.OtherSymbol:
                    case UnicodeCategory.OtherNotAssigned:
                        isSymbol = true;
                        isSeparator = true;
                        break;
                    case UnicodeCategory.OtherPunctuation:
                        if (c != '.')
                        {
                            isSymbol = true;
                            isSeparator = true;
                        }
                        break;
                    case UnicodeCategory.MathSymbol:
                        if (c != '+')
                        {
                            isSymbol = true;
                            isSeparator = true;
                        }
                        break;
                    case UnicodeCategory.SpaceSeparator:
                    case UnicodeCategory.LineSeparator:
                    case UnicodeCategory.ParagraphSeparator:
                    case UnicodeCategory.Control:
                    case UnicodeCategory.Format:
                    case UnicodeCategory.PrivateUse:
                    default:
                        // Whitespace is a separator.
                        isSeparator = true;
                        break;
                }

                if (isSeparator)
                {
                    if (firstUnusedIndex < currentIndex)
                    {
                        yield return new JsonValue(
                            json,
                            firstUnusedIndex,
                            currentIndex - firstUnusedIndex);

                        // Important not to increment firstUnusedIndex here already, in case of a '"'.
                        firstUnusedIndex = currentIndex;
                    }

                    if (isSymbol)
                    {
                        switch (c)
                        {
                            case '{':
                                yield return new JsonCurlyOpen(json, currentIndex);
                                break;
                            case '}':
                                yield return new JsonCurlyClose(json, currentIndex);
                                break;
                            case '[':
                                yield return new JsonSquareBracketOpen(json, currentIndex);
                                break;
                            case ']':
                                yield return new JsonSquareBracketClose(json, currentIndex);
                                break;
                            case ':':
                                yield return new JsonColon(json, currentIndex);
                                break;
                            case ',':
                                yield return new JsonComma(json, currentIndex);
                                break;
                            case '"':
                                currentTokenizer = InString;
                                yield break;
                            case '/':
                                // Look ahead 1 character to see if this is the start of a comment.
                                // In all other cases, treat as an unexpected symbol.
                                if (currentIndex + 1 < length)
                                {
                                    char secondChar = json[currentIndex + 1];
                                    if (secondChar == '/')
                                    {
                                        currentTokenizer = InSingleLineComment;
                                        yield break;
                                    }
                                    else if (secondChar == '*')
                                    {
                                        currentTokenizer = InMultiLineComment;
                                        yield break;
                                    }
                                }
                                goto default;
                            default:
                                string displayCharValue = category == UnicodeCategory.OtherNotAssigned
                                    ? $"\\u{((int)c).ToString("x4")}"
                                    : Convert.ToString(c);
                                yield return new JsonUnknownSymbol(
                                    json,
                                    currentIndex,
                                    JsonErrorInfo.UnexpectedSymbol(displayCharValue, currentIndex));
                                break;
                        }
                    }

                    firstUnusedIndex++;
                }

                currentIndex++;
            }

            if (firstUnusedIndex < currentIndex)
            {
                yield return new JsonValue(
                    json,
                    firstUnusedIndex,
                    currentIndex - firstUnusedIndex);
            }

            currentTokenizer = null;
        }

        private IEnumerable<JsonTerminalSymbol> InString()
        {
            // Eat " character, but leave firstUnusedIndex unchanged.
            currentIndex++;

            List<JsonErrorInfo> errors = new List<JsonErrorInfo>();
            StringBuilder valueBuilder = new StringBuilder();

            while (currentIndex < length)
            {
                char c = json[currentIndex];
                switch (c)
                {
                    case '"':
                        currentIndex++;
                        if (errors.Count > 0)
                        {
                            yield return new JsonErrorString(
                                json,
                                firstUnusedIndex,
                                currentIndex - firstUnusedIndex,
                                errors);
                        }
                        else
                        {
                            yield return new JsonString(
                                json,
                                firstUnusedIndex,
                                currentIndex - firstUnusedIndex,
                                valueBuilder.ToString());
                        }
                        firstUnusedIndex = currentIndex;
                        currentTokenizer = Default;
                        yield break;
                    case '\\':
                        // Escape sequence.
                        int escapeSequenceStart = currentIndex;
                        currentIndex++;
                        if (currentIndex < length)
                        {
                            char escapedChar = json[currentIndex];
                            switch (escapedChar)
                            {
                                case '"':
                                case '\\':
                                case '/':  // Weird one, but it's in the specification.
                                    valueBuilder.Append(escapedChar);
                                    break;
                                case 'b':
                                    valueBuilder.Append('\b');
                                    break;
                                case 'f':
                                    valueBuilder.Append('\f');
                                    break;
                                case 'n':
                                    valueBuilder.Append('\n');
                                    break;
                                case 'r':
                                    valueBuilder.Append('\r');
                                    break;
                                case 't':
                                    valueBuilder.Append('\t');
                                    break;
                                case 'v':
                                    valueBuilder.Append('\v');
                                    break;
                                default:
                                    errors.Add(JsonErrorInfo.UnrecognizedEscapeSequence(
                                        json.Substring(escapeSequenceStart, 2),
                                        escapeSequenceStart));
                                    break;
                            }
                        }
                        break;
                    default:
                        if (char.IsControl(c))
                        {
                            // Generate user friendly representation of the illegal character in error message.
                            string displayCharValue;
                            switch (c)
                            {
                                case '\0': displayCharValue = "\\0"; break;
                                case '\b': displayCharValue = "\\b"; break;
                                case '\f': displayCharValue = "\\f"; break;
                                case '\n': displayCharValue = "\\n"; break;
                                case '\r': displayCharValue = "\\r"; break;
                                case '\t': displayCharValue = "\\t"; break;
                                case '\v': displayCharValue = "\\v"; break;
                                default: displayCharValue = $"\\u{((int)c).ToString("x4")}"; break;
                            }

                            errors.Add(JsonErrorInfo.IllegalControlCharacterInString(displayCharValue, currentIndex));
                        }
                        else
                        {
                            valueBuilder.Append(c);
                        }
                        break;
                }

                currentIndex++;
            }

            // Use length rather than currentIndex; currentIndex is bigger after a '\'.
            errors.Add(JsonErrorInfo.UnterminatedString(length));
            yield return new JsonErrorString(
                json,
                firstUnusedIndex,
                length - firstUnusedIndex,
                errors);

            currentTokenizer = null;
        }

        private IEnumerable<JsonTerminalSymbol> InSingleLineComment()
        {
            // Eat both / characters, but leave firstUnusedIndex unchanged.
            currentIndex += 2;

            while (currentIndex < length)
            {
                char c = json[currentIndex];

                switch (c)
                {
                    case '\r':
                        // Can already eat this whitespace character.
                        currentIndex++;

                        // Look ahead to see if the next character is a linefeed.
                        if (currentIndex < length)
                        {
                            char secondChar = json[currentIndex];
                            if (secondChar == '\n')
                            {
                                yield return new JsonComment(
                                    json,
                                    firstUnusedIndex,
                                    currentIndex - firstUnusedIndex - 1);

                                // Eat the second whitespace character.
                                currentIndex++;
                                firstUnusedIndex = currentIndex;
                                currentTokenizer = Default;
                                yield break;
                            }
                        }
                        break;
                    case '\n':
                        yield return new JsonComment(
                            json,
                            firstUnusedIndex,
                            currentIndex - firstUnusedIndex);

                        // Eat the '\n'.
                        currentIndex++;
                        firstUnusedIndex = currentIndex;
                        currentTokenizer = Default;
                        yield break;
                }

                currentIndex++;
            }

            yield return new JsonComment(
                json,
                firstUnusedIndex,
                currentIndex - firstUnusedIndex);

            currentTokenizer = null;
        }

        private IEnumerable<JsonTerminalSymbol> InMultiLineComment()
        {
            // Eat /* characters, but leave firstUnusedIndex unchanged.
            currentIndex += 2;

            while (currentIndex < length)
            {
                if (json[currentIndex] == '*')
                {
                    currentIndex++;

                    // Look ahead to see if the next character is a slash.
                    if (currentIndex < length)
                    {
                        char secondChar = json[currentIndex];
                        if (secondChar == '/')
                        {
                            // Increment so the closing '*/' is regarded as part of the comment.
                            currentIndex++;

                            yield return new JsonComment(
                                json,
                                firstUnusedIndex,
                                currentIndex - firstUnusedIndex);

                            firstUnusedIndex = currentIndex;
                            currentTokenizer = Default;
                            yield break;
                        }
                    }
                }

                currentIndex++;
            }

            yield return new JsonComment(
                json,
                firstUnusedIndex,
                currentIndex - firstUnusedIndex);

            currentTokenizer = null;
        }

        /// <summary>
        /// Tokenizes the source <see cref="Json"/> from start to end.
        /// </summary>
        /// <returns>
        /// An enumeration of <see cref="JsonTerminalSymbol"/> instances.
        /// </returns>
        public IEnumerable<JsonTerminalSymbol> TokenizeAll()
        {
            // currentTokenizer represents the state the tokenizer is in,
            // e.g. whitespace, in a string, or whatnot.
            while (currentTokenizer != null)
            {
                foreach (var symbol in currentTokenizer())
                {
                    yield return symbol;
                }
            }
        }
    }
}

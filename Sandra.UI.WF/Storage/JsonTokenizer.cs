﻿#region License
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

namespace Sandra.UI.WF.Storage
{
    /// <summary>
    /// Based on https://www.json.org/.
    /// </summary>
    public sealed class JsonTokenizer
    {
        private readonly int length;
        private readonly string json;

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
                if (!char.IsWhiteSpace(c))
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
                        case '/':
                            // Look ahead 1 character to see if this is the start of a comment.
                            // In all other cases, treat as an unexpected symbol.
                            if (currentIndex + 1 < length)
                            {
                                char secondChar = json[currentIndex + 1];
                                if (secondChar == '*')
                                {
                                    currentTokenizer = InMultiLineComment;
                                    yield break;
                                }
                            }
                            goto default;
                        default:
                            yield return new JsonUnknownSymbol(json, currentIndex);
                            break;
                    }
                }

                firstUnusedIndex++;
                currentIndex++;
            }

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

            currentTokenizer = null;
            yield return new JsonComment(
                json,
                firstUnusedIndex,
                currentIndex - firstUnusedIndex);
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

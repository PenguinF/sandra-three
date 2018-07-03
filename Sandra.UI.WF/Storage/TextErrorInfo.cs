#region License
/*********************************************************************************
 * TextErrorInfo.cs
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

namespace Sandra.UI.WF.Storage
{
    public class TextErrorInfo
    {
        public string Message { get; }
        public int Start { get; }
        public int Length { get; }

        public TextErrorInfo(string message, int start, int length)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));

            Message = message;
            Start = start;
            Length = length;
        }

        /// <summary>
        /// Creates a <see cref="TextErrorInfo"/> for unexpected symbol characters.
        /// </summary>
        public static TextErrorInfo UnexpectedSymbol(string displayCharValue, int position)
            => new TextErrorInfo($"Unexpected symbol '{displayCharValue}'", position, 1);

        /// <summary>
        /// Creates a <see cref="TextErrorInfo"/> for unterminated multiline comments.
        /// </summary>
        /// <param name="start">
        /// The length of the source json, or the position of the unexpected EOF.
        /// </param>
        public static TextErrorInfo UnterminatedMultiLineComment(int start)
            => new TextErrorInfo("Unterminated multi-line comment", start, 0);

        /// <summary>
        /// Creates a <see cref="TextErrorInfo"/> for unterminated strings.
        /// </summary>
        /// <param name="start">
        /// The length of the source json, or the position of the unexpected EOF.
        /// </param>
        public static TextErrorInfo UnterminatedString(int start)
            => new TextErrorInfo("Unterminated string", start, 0);

        /// <summary>
        /// Creates a <see cref="TextErrorInfo"/> for unrecognized escape sequences.
        /// </summary>
        public static TextErrorInfo UnrecognizedEscapeSequence(string displayCharValue, int start)
            => new TextErrorInfo($"Unrecognized escape sequence ('{displayCharValue}')", start, 2);

        /// <summary>
        /// Creates a <see cref="TextErrorInfo"/> for unrecognized Unicode escape sequences.
        /// </summary>
        public static TextErrorInfo UnrecognizedUnicodeEscapeSequence(string displayCharValue, int start, int length)
            => new TextErrorInfo($"Unrecognized escape sequence ('{displayCharValue}')", start, length);

        /// <summary>
        /// Creates a <see cref="TextErrorInfo"/> for illegal control characters inside string literals.
        /// </summary>
        public static TextErrorInfo IllegalControlCharacterInString(string displayCharValue, int start)
            => new TextErrorInfo($"Illegal control character '{displayCharValue}' in string", start, 1);
    }
}

#region License
/*********************************************************************************
 * JsonErrorString.cs
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
using System.Linq;

namespace SysExtensions.Text.Json
{
    public class JsonErrorString : JsonSymbol
    {
        /// <summary>
        /// Creates a <see cref="TextErrorInfo"/> for unterminated strings.
        /// </summary>
        /// <param name="start">
        /// The start position of the unterminated string.
        /// </param>
        /// <param name="length">
        /// The length of the unterminated string.
        /// </param>
        public static JsonErrorInfo Unterminated(int start, int length)
            => new JsonErrorInfo(JsonErrorCode.UnterminatedString, "Unterminated string", start, length);

        /// <summary>
        /// Creates a <see cref="TextErrorInfo"/> for unrecognized escape sequences.
        /// </summary>
        public static JsonErrorInfo UnrecognizedEscapeSequence(string displayCharValue, int start)
            => new JsonErrorInfo(JsonErrorCode.UnrecognizedEscapeSequence, $"Unrecognized escape sequence ('{displayCharValue}')", start, 2);

        /// <summary>
        /// Creates a <see cref="TextErrorInfo"/> for unrecognized Unicode escape sequences.
        /// </summary>
        public static JsonErrorInfo UnrecognizedUnicodeEscapeSequence(string displayCharValue, int start, int length)
            => new JsonErrorInfo(JsonErrorCode.UnrecognizedEscapeSequence, $"Unrecognized escape sequence ('{displayCharValue}')", start, length);

        /// <summary>
        /// Creates a <see cref="TextErrorInfo"/> for illegal control characters inside string literals.
        /// </summary>
        public static JsonErrorInfo IllegalControlCharacter(string displayCharValue, int start)
            => new JsonErrorInfo(JsonErrorCode.IllegalControlCharacterInString, $"Illegal control character '{displayCharValue}' in string", start, 1);

        public override IEnumerable<TextErrorInfo> Errors { get; }
        public override bool IsValueStartSymbol => true;

        public JsonErrorString(params TextErrorInfo[] errors)
        {
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }

        public JsonErrorString(IEnumerable<TextErrorInfo> errors)
        {
            if (errors == null) throw new ArgumentNullException(nameof(errors));
            Errors = errors.ToArray();
        }

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitErrorString(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitErrorString(this);
    }
}

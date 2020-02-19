#region License
/*********************************************************************************
 * JsonErrorStringSyntax.cs
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

using Eutherion.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a string literal value syntax node which contains errors.
    /// </summary>
    public sealed class GreenJsonErrorStringSyntax : GreenJsonValueSyntax, IGreenJsonSymbol
    {
        internal ReadOnlyList<JsonErrorInfo> Errors { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length { get; }

        /// <summary>
        /// Generates a sequence of errors associated with this symbol at a given start position.
        /// </summary>
        /// <param name="startPosition">
        /// The start position for which to generate the errors.
        /// </param>
        /// <returns>
        /// A sequence of errors associated with this symbol.
        /// </returns>
        public IEnumerable<JsonErrorInfo> GetErrors(int startPosition)
            => Errors.Select(error => new JsonErrorInfo(
                error.ErrorCode,
                startPosition + error.Start,
                error.Length,
                error.Parameters));

        public JsonSymbolType SymbolType => JsonSymbolType.ErrorString;

        public GreenJsonErrorStringSyntax(int length, params JsonErrorInfo[] errors)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            Length = length;
            Errors = ReadOnlyList<JsonErrorInfo>.Create(errors);
        }

        public GreenJsonErrorStringSyntax(IEnumerable<JsonErrorInfo> errors, int length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            Length = length;
            Errors = ReadOnlyList<JsonErrorInfo>.Create(errors);
        }

        public override void Accept(GreenJsonValueSyntaxVisitor visitor) => visitor.VisitErrorStringSyntax(this);
        public override TResult Accept<TResult>(GreenJsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitErrorStringSyntax(this);
        public override TResult Accept<T, TResult>(GreenJsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitErrorStringSyntax(this, arg);
    }

    /// <summary>
    /// Represents a string literal value syntax node which contains errors.
    /// </summary>
    public sealed class JsonErrorStringSyntax : JsonValueSyntax, IJsonSymbol
    {
        /// <summary>
        /// Creates a <see cref="JsonErrorInfo"/> for unterminated strings.
        /// </summary>
        /// <param name="start">
        /// The start position of the unterminated string.
        /// </param>
        /// <param name="length">
        /// The length of the unterminated string.
        /// </param>
        public static JsonErrorInfo Unterminated(int start, int length)
            => new JsonErrorInfo(JsonErrorCode.UnterminatedString, start, length);

        /// <summary>
        /// Creates a <see cref="JsonErrorInfo"/> for unrecognized escape sequences.
        /// </summary>
        public static JsonErrorInfo UnrecognizedEscapeSequence(string displayCharValue, int start)
            => new JsonErrorInfo(JsonErrorCode.UnrecognizedEscapeSequence, start, 2, new[] { displayCharValue });

        /// <summary>
        /// Creates a <see cref="JsonErrorInfo"/> for unrecognized Unicode escape sequences.
        /// </summary>
        public static JsonErrorInfo UnrecognizedUnicodeEscapeSequence(string displayCharValue, int start, int length)
            => new JsonErrorInfo(JsonErrorCode.UnrecognizedEscapeSequence, start, length, new[] { displayCharValue });

        /// <summary>
        /// Creates a <see cref="JsonErrorInfo"/> for illegal control characters inside string literals.
        /// </summary>
        public static JsonErrorInfo IllegalControlCharacter(string displayCharValue, int start)
            => new JsonErrorInfo(JsonErrorCode.IllegalControlCharacterInString, start, 1, new[] { displayCharValue });

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenJsonErrorStringSyntax Green { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        internal JsonErrorStringSyntax(JsonValueWithBackgroundSyntax parent, GreenJsonErrorStringSyntax green) : base(parent) => Green = green;

        public override void Accept(JsonValueSyntaxVisitor visitor) => visitor.VisitErrorStringSyntax(this);
        public override TResult Accept<TResult>(JsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitErrorStringSyntax(this);
        public override TResult Accept<T, TResult>(JsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitErrorStringSyntax(this, arg);

        void IJsonSymbol.Accept(JsonSymbolVisitor visitor) => visitor.VisitErrorStringSyntax(this);
        TResult IJsonSymbol.Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitErrorStringSyntax(this);
        TResult IJsonSymbol.Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitErrorStringSyntax(this, arg);
    }
}

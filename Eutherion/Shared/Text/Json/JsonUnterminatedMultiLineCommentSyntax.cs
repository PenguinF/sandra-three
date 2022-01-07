﻿#region License
/*********************************************************************************
 * JsonUnterminatedMultiLineCommentSyntax.cs
 *
 * Copyright (c) 2004-2022 Henk Nicolai
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

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a syntax node which contains an unterminated multi-line comment.
    /// </summary>
    public sealed class GreenJsonUnterminatedMultiLineCommentSyntax : GreenJsonBackgroundSyntax, IGreenJsonSymbol
    {
        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length { get; }

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public JsonSymbolType SymbolType => JsonSymbolType.UnterminatedMultiLineComment;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenJsonUnterminatedMultiLineCommentSyntax"/> with a specified length.
        /// </summary>
        /// <param name="length">
        /// The length of the text span corresponding with the node to create.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length"/> is 0 or lower.
        /// </exception>
        public GreenJsonUnterminatedMultiLineCommentSyntax(int length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            Length = length;
        }

        internal override TResult Accept<T, TResult>(GreenJsonBackgroundSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitUnterminatedMultiLineCommentSyntax(this, arg);
    }

    /// <summary>
    /// Represents a syntax node which contains an unterminated multi-line comment.
    /// </summary>
    public sealed class JsonUnterminatedMultiLineCommentSyntax : JsonBackgroundSyntax, IJsonSymbol
    {
        /// <summary>
        /// Creates a <see cref="JsonErrorInfo"/> for unterminated multiline comments.
        /// </summary>
        /// <param name="start">
        /// The start position of the unterminated comment.
        /// </param>
        /// <param name="length">
        /// The length of the unterminated comment.
        /// </param>
        public static JsonErrorInfo CreateError(int start, int length)
            => new JsonErrorInfo(JsonErrorCode.UnterminatedMultiLineComment, JsonErrorLevel.Warning, start, length);

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenJsonUnterminatedMultiLineCommentSyntax Green { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        internal JsonUnterminatedMultiLineCommentSyntax(JsonBackgroundListSyntax parent, int backgroundNodeIndex, GreenJsonUnterminatedMultiLineCommentSyntax green)
            : base(parent, backgroundNodeIndex)
            => Green = green;

        TResult IJsonSymbol.Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitUnterminatedMultiLineCommentSyntax(this, arg);
    }
}

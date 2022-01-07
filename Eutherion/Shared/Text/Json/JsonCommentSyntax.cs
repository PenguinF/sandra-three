#region License
/*********************************************************************************
 * JsonCommentSyntax.cs
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
    /// Represents a syntax node which contains a comment.
    /// </summary>
    public sealed class GreenJsonCommentSyntax : GreenJsonBackgroundSyntax, IGreenJsonSymbol
    {
        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length { get; }

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public JsonSymbolType SymbolType => JsonSymbolType.Comment;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenJsonCommentSyntax"/> with a specified length.
        /// </summary>
        /// <param name="length">
        /// The length of the text span corresponding with the node to create.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length"/> is 0 or lower.
        /// </exception>
        public GreenJsonCommentSyntax(int length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            Length = length;
        }

        public override void Accept(GreenJsonBackgroundSyntaxVisitor visitor) => visitor.VisitCommentSyntax(this);
        public override TResult Accept<TResult>(GreenJsonBackgroundSyntaxVisitor<TResult> visitor) => visitor.VisitCommentSyntax(this);
        public override TResult Accept<T, TResult>(GreenJsonBackgroundSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitCommentSyntax(this, arg);
    }

    /// <summary>
    /// Represents a syntax node which contains a comment.
    /// </summary>
    public sealed class JsonCommentSyntax : JsonBackgroundSyntax, IJsonSymbol
    {
        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenJsonCommentSyntax Green { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        internal JsonCommentSyntax(JsonBackgroundListSyntax parent, int backgroundNodeIndex, GreenJsonCommentSyntax green)
            : base(parent, backgroundNodeIndex)
            => Green = green;

        void IJsonSymbol.Accept(JsonSymbolVisitor visitor) => visitor.VisitCommentSyntax(this);
        TResult IJsonSymbol.Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitCommentSyntax(this);
        TResult IJsonSymbol.Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitCommentSyntax(this, arg);
    }
}

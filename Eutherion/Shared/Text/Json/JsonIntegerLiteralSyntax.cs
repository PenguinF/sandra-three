#region License
/*********************************************************************************
 * JsonIntegerLiteralSyntax.cs
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
using System.Numerics;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents an integer literal value syntax node.
    /// </summary>
    public sealed class GreenJsonIntegerLiteralSyntax : GreenJsonValueSyntax, IGreenJsonSymbol
    {
        /// <summary>
        /// Gets the integer value represented by this literal syntax.
        /// </summary>
        public BigInteger Value { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length { get; }

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public JsonSymbolType SymbolType => JsonSymbolType.IntegerLiteral;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenJsonIntegerLiteralSyntax"/>.
        /// </summary>
        /// <param name="value">
        /// The integer value represented by this literal syntax.
        /// </param>
        /// <param name="length">
        /// The length of the text span corresponding with this syntax node.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length"/> is 0 or lower.
        /// </exception>
        public GreenJsonIntegerLiteralSyntax(BigInteger value, int length)
        {
            Value = value;
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            Length = length;
        }

        public override void Accept(GreenJsonValueSyntaxVisitor visitor) => visitor.VisitIntegerLiteralSyntax(this);
        public override TResult Accept<TResult>(GreenJsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitIntegerLiteralSyntax(this);
        public override TResult Accept<T, TResult>(GreenJsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitIntegerLiteralSyntax(this, arg);
    }

    /// <summary>
    /// Represents an integer literal value syntax node.
    /// </summary>
    public sealed class JsonIntegerLiteralSyntax : JsonValueSyntax, IJsonSymbol
    {
        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenJsonIntegerLiteralSyntax Green { get; }

        /// <summary>
        /// Gets the integer value represented by this literal syntax.
        /// </summary>
        public BigInteger Value => Green.Value;

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        internal JsonIntegerLiteralSyntax(JsonValueWithBackgroundSyntax parent, GreenJsonIntegerLiteralSyntax green) : base(parent) => Green = green;

        public override void Accept(JsonValueSyntaxVisitor visitor) => visitor.VisitIntegerLiteralSyntax(this);
        public override TResult Accept<TResult>(JsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitIntegerLiteralSyntax(this);
        public override TResult Accept<T, TResult>(JsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitIntegerLiteralSyntax(this, arg);

        void IJsonSymbol.Accept(JsonSymbolVisitor visitor) => visitor.VisitIntegerLiteralSyntax(this);
        TResult IJsonSymbol.Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitIntegerLiteralSyntax(this);
        TResult IJsonSymbol.Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitIntegerLiteralSyntax(this, arg);
    }
}

﻿#region License
/*********************************************************************************
 * JsonStringLiteralSyntax.cs
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
    /// Represents a string literal value syntax node.
    /// </summary>
    public sealed class GreenJsonStringLiteralSyntax : GreenJsonValueSyntax, IGreenJsonSymbol
    {
        /// <summary>
        /// Gets the string value represented by this literal syntax.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length { get; }

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public JsonSymbolType SymbolType => JsonSymbolType.StringLiteral;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenJsonStringLiteralSyntax"/>.
        /// </summary>
        /// <param name="value">
        /// The string value to be represented by this literal syntax.
        /// </param>
        /// <param name="length">
        /// The length of the text span corresponding with this syntax node.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length"/> is 0 or lower.
        /// </exception>
        public GreenJsonStringLiteralSyntax(string value, int length)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            Length = length;
        }

        public override void Accept(GreenJsonValueSyntaxVisitor visitor) => visitor.VisitStringLiteralSyntax(this);
        public override TResult Accept<TResult>(GreenJsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitStringLiteralSyntax(this);
        public override TResult Accept<T, TResult>(GreenJsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitStringLiteralSyntax(this, arg);
    }

    /// <summary>
    /// Represents a string literal value syntax node.
    /// </summary>
    public sealed class JsonStringLiteralSyntax : JsonValueSyntax, IJsonSymbol
    {
        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenJsonStringLiteralSyntax Green { get; }

        /// <summary>
        /// Gets the string value represented by this literal syntax.
        /// </summary>
        public string Value => Green.Value;

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        internal JsonStringLiteralSyntax(JsonValueWithBackgroundSyntax parent, GreenJsonStringLiteralSyntax green) : base(parent) => Green = green;

        public override void Accept(JsonValueSyntaxVisitor visitor) => visitor.VisitStringLiteralSyntax(this);
        public override TResult Accept<TResult>(JsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitStringLiteralSyntax(this);
        public override TResult Accept<T, TResult>(JsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitStringLiteralSyntax(this, arg);

        void IJsonSymbol.Accept(JsonSymbolVisitor visitor) => visitor.VisitStringLiteralSyntax(this);
        TResult IJsonSymbol.Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitStringLiteralSyntax(this);
        TResult IJsonSymbol.Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitStringLiteralSyntax(this, arg);
    }
}

#region License
/*********************************************************************************
 * PgnTagValueSyntax.cs
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

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a tag value syntax node.
    /// </summary>
    public sealed class GreenPgnTagValueSyntax : GreenPgnTagElementSyntax, IGreenPgnSymbol
    {
        /// <summary>
        /// Gets the value of this syntax node.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public override int Length { get; }

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public PgnSymbolType SymbolType => PgnSymbolType.TagValue;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnTagValueSyntax"/>.
        /// </summary>
        /// <param name="value">
        /// The value of the tag.
        /// </param>
        /// <param name="length">
        /// The length of the text span corresponding with the node to create.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length"/> is 0 or lower.
        /// </exception>
        public GreenPgnTagValueSyntax(string value, int length)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            Length = length;
        }

        IEnumerable<PgnErrorInfo> IGreenPgnSymbol.GetErrors(int startPosition) => EmptyEnumerable<PgnErrorInfo>.Instance;

        public override void Accept(GreenPgnTagElementSyntaxVisitor visitor) => visitor.VisitTagValueSyntax(this);
        public override TResult Accept<TResult>(GreenPgnTagElementSyntaxVisitor<TResult> visitor) => visitor.VisitTagValueSyntax(this);
        public override TResult Accept<T, TResult>(GreenPgnTagElementSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitTagValueSyntax(this, arg);
    }
}

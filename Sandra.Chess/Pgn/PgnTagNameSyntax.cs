#region License
/*********************************************************************************
 * PgnTagNameSyntax.cs
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

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a tag name syntax node.
    /// </summary>
    public sealed class GreenPgnTagNameSyntax : GreenPgnTagElementSyntax
    {
        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public override int Length { get; }

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public override PgnSymbolType SymbolType => PgnSymbolType.TagName;

        /// <summary>
        /// Gets if this symbol was originally parsed as a move but reinterpreted as a tag name.
        /// </summary>
        public bool IsConvertedFromMove { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnTagNameSyntax"/>.
        /// </summary>
        /// <param name="length">
        /// The length of the text span corresponding with the node to create.
        /// </param>
        /// <param name="isConvertedFromMove">
        /// If this symbol was originally parsed as a move but reinterpreted as a tag name.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length"/> is 0 or lower.
        /// </exception>
        public GreenPgnTagNameSyntax(int length, bool isConvertedFromMove)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            Length = length;
            IsConvertedFromMove = isConvertedFromMove;
        }

        internal override PgnTagElementSyntax CreateRedNode(PgnTagElementWithTriviaSyntax parent) => new PgnTagNameSyntax(parent, this);
    }

    /// <summary>
    /// Represents a tag name syntax node.
    /// </summary>
    public sealed class PgnTagNameSyntax : PgnTagElementSyntax, IPgnSymbol
    {
        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnTagNameSyntax Green { get; }

        /// <summary>
        /// Gets if this tag name syntax is a valid (<see cref="PgnMoveSyntax"/>) as well, but reinterpreted as a tag name.
        /// </summary>
        public bool IsConvertedFromMove => Green.IsConvertedFromMove;

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        internal PgnTagNameSyntax(PgnTagElementWithTriviaSyntax parent, GreenPgnTagNameSyntax green) : base(parent) => Green = green;

        public override void Accept(PgnTagElementSyntaxVisitor visitor) => visitor.VisitTagNameSyntax(this);
        public override TResult Accept<TResult>(PgnTagElementSyntaxVisitor<TResult> visitor) => visitor.VisitTagNameSyntax(this);
        public override TResult Accept<T, TResult>(PgnTagElementSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitTagNameSyntax(this, arg);

        void IPgnSymbol.Accept(PgnSymbolVisitor visitor) => visitor.VisitTagNameSyntax(this);
        TResult IPgnSymbol.Accept<TResult>(PgnSymbolVisitor<TResult> visitor) => visitor.VisitTagNameSyntax(this);
        TResult IPgnSymbol.Accept<T, TResult>(PgnSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitTagNameSyntax(this, arg);
    }
}

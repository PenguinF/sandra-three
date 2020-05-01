#region License
/*********************************************************************************
 * PgnEscapeSyntax.cs
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
    /// Represents a syntax node which contains a PGN escape sqquence.
    /// </summary>
    public sealed class GreenPgnEscapeSyntax : GreenPgnBackgroundSyntax, IGreenPgnSymbol
    {
        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public override int Length { get; }

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public PgnSymbolType SymbolType => PgnSymbolType.Escape;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnEscapeSyntax"/>.
        /// </summary>
        /// <param name="length">
        /// The length of the syntax node, including its delimiter character '%',
        /// and excluding the '\n' and preceding '\r' that terminates it.
        /// </param>
        public GreenPgnEscapeSyntax(int length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            Length = length;
        }

        IEnumerable<PgnErrorInfo> IGreenPgnSymbol.GetErrors(int startPosition) => EmptyEnumerable<PgnErrorInfo>.Instance;

        public override void Accept(GreenPgnBackgroundSyntaxVisitor visitor) => visitor.VisitEscapeSyntax(this);
        public override TResult Accept<TResult>(GreenPgnBackgroundSyntaxVisitor<TResult> visitor) => visitor.VisitEscapeSyntax(this);
        public override TResult Accept<T, TResult>(GreenPgnBackgroundSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitEscapeSyntax(this, arg);
    }

    /// <summary>
    /// Represents a syntax node which contains a PGN escape sqquence.
    /// </summary>
    public sealed class PgnEscapeSyntax : PgnBackgroundSyntax, IPgnSymbol
    {
        /// <summary>
        /// The character which triggers the PGN escape mechanism.
        /// </summary>
        public const char EscapeCharacter = '%';

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnEscapeSyntax Green { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        internal PgnEscapeSyntax(PgnBackgroundListSyntax parent, int parentIndex, GreenPgnEscapeSyntax green)
            : base(parent, parentIndex)
            => Green = green;

        void IPgnSymbol.Accept(PgnSymbolVisitor visitor) => visitor.VisitEscapeSyntax(this);
        TResult IPgnSymbol.Accept<TResult>(PgnSymbolVisitor<TResult> visitor) => visitor.VisitEscapeSyntax(this);
        TResult IPgnSymbol.Accept<T, TResult>(PgnSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitEscapeSyntax(this, arg);
    }
}

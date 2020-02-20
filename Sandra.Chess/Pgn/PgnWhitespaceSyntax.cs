#region License
/*********************************************************************************
 * PgnWhitespaceSyntax.cs
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

using Eutherion;
using System;
using System.Collections.Generic;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a PGN syntax node which contains whitespace.
    /// </summary>
    public sealed class GreenPgnWhitespaceSyntax : GreenPgnBackgroundSyntax, IGreenPgnSymbol
    {
        /// <summary>
        /// Maximum length before new <see cref="GreenPgnWhitespaceSyntax"/> instances are always newly allocated.
        /// </summary>
        public const int SharedWhitespaceInstanceLength = 255;

        private static readonly GreenPgnWhitespaceSyntax[] SharedInstances;

        static GreenPgnWhitespaceSyntax()
        {
            // Do not allocate a zero length whitespace.
            SharedInstances = new GreenPgnWhitespaceSyntax[SharedWhitespaceInstanceLength - 1];
            SharedInstances.Fill(i => new GreenPgnWhitespaceSyntax(i + 1));
        }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public override int Length { get; }

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public PgnSymbolType SymbolType => PgnSymbolType.Whitespace;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnWhitespaceSyntax"/> with a specified length.
        /// </summary>
        /// <param name="length">
        /// The length of the text span corresponding with the node to create.
        /// </param>
        /// <returns>
        /// The new <see cref="GreenPgnWhitespaceSyntax"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length"/> is 0 or lower.
        /// </exception>
        public static GreenPgnWhitespaceSyntax Create(int length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (length < SharedWhitespaceInstanceLength) return SharedInstances[length - 1];
            return new GreenPgnWhitespaceSyntax(length);
        }

        private GreenPgnWhitespaceSyntax(int length) => Length = length;

        IEnumerable<PgnErrorInfo> IGreenPgnSymbol.GetErrors(int startPosition) => EmptyEnumerable<PgnErrorInfo>.Instance;

        public override void Accept(GreenPgnBackgroundSyntaxVisitor visitor) => visitor.VisitWhitespaceSyntax(this);
        public override TResult Accept<TResult>(GreenPgnBackgroundSyntaxVisitor<TResult> visitor) => visitor.VisitWhitespaceSyntax(this);
        public override TResult Accept<T, TResult>(GreenPgnBackgroundSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitWhitespaceSyntax(this, arg);
    }

    /// <summary>
    /// Represents a PGN syntax node which contains whitespace.
    /// </summary>
    public sealed class PgnWhitespaceSyntax : PgnBackgroundSyntax, IPgnSymbol
    {
        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnWhitespaceSyntax Green { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        internal PgnWhitespaceSyntax(PgnSyntaxNodes parent, int parentIndex, GreenPgnWhitespaceSyntax green)
            : base(parent, parentIndex)
            => Green = green;

        void IPgnSymbol.Accept(PgnSymbolVisitor visitor) => visitor.VisitWhitespaceSyntax(this);
        TResult IPgnSymbol.Accept<TResult>(PgnSymbolVisitor<TResult> visitor) => visitor.VisitWhitespaceSyntax(this);
        TResult IPgnSymbol.Accept<T, TResult>(PgnSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitWhitespaceSyntax(this, arg);
    }
}

#region License
/*********************************************************************************
 * PgnMoveNumberSyntax.cs
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

using Sandra.Chess.Pgn.Temp;
using System;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a syntax node which contains an integer move number.
    /// </summary>
    public sealed class GreenPgnMoveNumberSyntax : IGreenPgnSymbol
    {
        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public PgnSymbolType SymbolType => PgnSymbolType.MoveNumber;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnMoveNumberSyntax"/>.
        /// </summary>
        /// <param name="length">
        /// The length of the text span corresponding with the node to create.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length"/> is 0 or lower.
        /// </exception>
        public GreenPgnMoveNumberSyntax(int length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            Length = length;
        }
    }

    /// <summary>
    /// Represents a syntax node which contains an integer move number.
    /// </summary>
    public sealed class PgnMoveNumberSyntax : PgnSyntax, IPgnSymbol
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public PgnMoveNumberWithTriviaSyntax Parent { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnMoveNumberSyntax Green { get; }

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Green.LeadingTrivia.Length;

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override PgnSyntax ParentSyntax => Parent;

        internal PgnMoveNumberSyntax(PgnMoveNumberWithTriviaSyntax parent, GreenPgnMoveNumberSyntax green)
        {
            Parent = parent;
            Green = green;
        }

        void IPgnSymbol.Accept(PgnSymbolVisitor visitor) => visitor.VisitMoveNumberSyntax(this);
        TResult IPgnSymbol.Accept<TResult>(PgnSymbolVisitor<TResult> visitor) => visitor.VisitMoveNumberSyntax(this);
        TResult IPgnSymbol.Accept<T, TResult>(PgnSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitMoveNumberSyntax(this, arg);
    }

    /// <summary>
    /// Represents a syntax node which contains a move number, together with its leading trivia.
    /// </summary>
    public sealed class PgnMoveNumberWithTriviaSyntax : WithTriviaSyntax<PgnMoveNumberSyntax>, IPgnTopLevelSyntax
    {
        PgnSyntax IPgnTopLevelSyntax.ToPgnSyntax() => this;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public WithPlyFloatItemsSyntax Parent { get; }

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Green.LeadingFloatItems.Length;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override PgnSyntax ParentSyntax => Parent;

        internal override PgnMoveNumberSyntax CreateContentNode() => new PgnMoveNumberSyntax(this, (GreenPgnMoveNumberSyntax)Green.ContentNode);

        internal PgnMoveNumberWithTriviaSyntax(WithPlyFloatItemsSyntax parent, GreenWithTriviaSyntax green)
            : base(green)
        {
            Parent = parent;
        }
    }
}

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
using System.Collections.Generic;

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

        IEnumerable<PgnErrorInfo> IGreenPgnSymbol.GetErrors(int startPosition) => EmptyEnumerable<PgnErrorInfo>.Instance;
    }

    public sealed class PgnMoveNumberSyntax : PgnSyntax, IPgnSymbol
    {
        public PgnMoveNumberWithTriviaSyntax Parent { get; }
        public GreenPgnMoveNumberSyntax Green { get; }
        public override int Start => Parent.Green.LeadingTrivia.Length;
        public override int Length => Green.Length;
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

    public sealed class PgnMoveNumberWithTriviaSyntax : WithTriviaSyntax<GreenPgnMoveNumberSyntax, PgnMoveNumberSyntax>, IPgnTopLevelSyntax
    {
        PgnSyntax IPgnTopLevelSyntax.ToPgnSyntax() => this;

        public PgnSyntaxNodes Parent { get; }
        public int ParentIndex { get; }

        public override int Start => Parent.GreenTopLevelNodes.GetElementOffset(ParentIndex);
        public override PgnSyntax ParentSyntax => Parent;

        internal override PgnMoveNumberSyntax CreateContentNode() => new PgnMoveNumberSyntax(this, Green.ContentNode);

        internal PgnMoveNumberWithTriviaSyntax(PgnSyntaxNodes parent, int parentIndex, WithTrivia<GreenPgnMoveNumberSyntax> green)
            : base(green)
        {
            Parent = parent;
            ParentIndex = parentIndex;
        }
    }
}

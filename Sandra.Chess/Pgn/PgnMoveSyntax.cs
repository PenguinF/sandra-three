#region License
/*********************************************************************************
 * PgnMoveSyntax.cs
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
    /// Represents a syntax node which contains a move text.
    /// </summary>
    /// <remarks>
    /// When encountered in a tag pair, this node may be reinterpreted as a <see cref="GreenPgnTagNameSyntax"/>.
    /// </remarks>
    public class GreenPgnMoveSyntax : IGreenPgnSymbol
    {
        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public virtual PgnSymbolType SymbolType => PgnSymbolType.Move;

        /// <summary>
        /// Gets if the move syntax is a valid tag name (<see cref="GreenPgnTagNameSyntax"/>) as well.
        /// </summary>
        public bool IsValidTagName { get; }

        /// <summary>
        /// Gets if this is an unrecognized move.
        /// </summary>
        public virtual bool IsUnrecognizedMove => false;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnMoveSyntax"/>.
        /// </summary>
        /// <param name="length">
        /// The length of the text span corresponding with the node to create.
        /// </param>
        /// <param name="isValidTagName">
        /// If the move syntax is a valid tag name (<see cref="GreenPgnTagNameSyntax"/>) as well.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length"/> is 1 or lower.
        /// </exception>
        public GreenPgnMoveSyntax(int length, bool isValidTagName) : this(length)
        {
            if (length <= 1) throw new ArgumentOutOfRangeException(nameof(length));
            IsValidTagName = isValidTagName;
        }

        internal GreenPgnMoveSyntax(int length) => Length = length;
    }

    /// <summary>
    /// Represents a syntax node which contains a move text.
    /// </summary>
    public sealed class PgnMoveSyntax : PgnSyntax, IPgnSymbol
    {
        /// <summary>
        /// Creates a <see cref="PgnErrorInfo"/> for a PGN syntax node with an unknown symbol.
        /// </summary>
        /// <param name="symbolText">
        /// The text containing the unknown symbol.
        /// </param>
        /// <param name="start">
        /// The start position of the unknown symbol.
        /// </param>
        public static PgnErrorInfo CreateUnrecognizedMoveError(string symbolText, int start)
            => new PgnErrorInfo(
                PgnErrorCode.UnrecognizedMove,
                start,
                symbolText.Length,
                new[] { symbolText });

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public PgnMoveWithTriviaSyntax Parent { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnMoveSyntax Green { get; }

        /// <summary>
        /// Gets if the move syntax is a valid tag name (<see cref="PgnTagNameSyntax"/>) as well.
        /// </summary>
        public bool IsValidTagName => Green.IsValidTagName;

        /// <summary>
        /// Gets if this is an unrecognized move.
        /// </summary>
        public bool IsUnrecognizedMove => Green.IsUnrecognizedMove;

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

        internal PgnMoveSyntax(PgnMoveWithTriviaSyntax parent, GreenPgnMoveSyntax green)
        {
            Parent = parent;
            Green = green;
        }

        void IPgnSymbol.Accept(PgnSymbolVisitor visitor) => visitor.VisitMoveSyntax(this);
        TResult IPgnSymbol.Accept<TResult>(PgnSymbolVisitor<TResult> visitor) => visitor.VisitMoveSyntax(this);
        TResult IPgnSymbol.Accept<T, TResult>(PgnSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitMoveSyntax(this, arg);
    }

    /// <summary>
    /// Represents a syntax node which contains a move text, together with its leading trivia.
    /// </summary>
    public sealed class PgnMoveWithTriviaSyntax : WithTriviaSyntax<PgnMoveSyntax>, IPgnTopLevelSyntax
    {
        PgnSyntax IPgnTopLevelSyntax.ToPgnSyntax() => this;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public WithPlyFloatItemsSyntax Parent { get; }
        public int ParentIndex { get; }

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.GreenTopLevelNodes.GetElementOffset(ParentIndex);

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override PgnSyntax ParentSyntax => Parent;

        internal override PgnMoveSyntax CreateContentNode() => new PgnMoveSyntax(this, (GreenPgnMoveSyntax)Green.ContentNode);

        internal PgnMoveWithTriviaSyntax(WithPlyFloatItemsSyntax parent, int parentIndex, GreenWithTriviaSyntax green)
            : base(green)
        {
            Parent = parent;
            ParentIndex = parentIndex;
        }
    }
}

#region License
/*********************************************************************************
 * PgnIllegalCharactersSyntax.cs
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

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a character which is illegal in the PGN standard.
    /// </summary>
    public sealed class GreenPgnIllegalCharacterSyntax : GreenPgnBackgroundSyntax
    {
        /// <summary>
        /// Gets the single <see cref="GreenPgnIllegalCharacterSyntax"/> value.
        /// </summary>
        public static GreenPgnIllegalCharacterSyntax Value { get; } = new GreenPgnIllegalCharacterSyntax();

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public override int Length => PgnIllegalCharacterSyntax.IllegalCharacterLength;

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public override PgnSymbolType SymbolType => PgnSymbolType.IllegalCharacter;

        private GreenPgnIllegalCharacterSyntax() { }

        internal override PgnBackgroundSyntax CreateRedNode(PgnBackgroundListSyntax parent, int parentIndex) => new PgnIllegalCharacterSyntax(parent, parentIndex, this);
    }

    /// <summary>
    /// Represents a character which is illegal in the PGN standard.
    /// </summary>
    public sealed class PgnIllegalCharacterSyntax : PgnBackgroundSyntax, IPgnSymbol
    {
        public const int IllegalCharacterLength = 1;

        /// <summary>
        /// Creates a <see cref="PgnErrorInfo"/> for illegal PGN characters.
        /// </summary>
        /// <param name="displayCharValue">
        /// A friendly representation of the illegal character.
        /// </param>
        /// <param name="startPosition">
        /// The start position for which to create the error.
        /// </param>
        /// <returns>
        /// The new <see cref="PgnErrorInfo"/>.
        /// </returns>
        public static PgnErrorInfo CreateError(string displayCharValue, int startPosition)
            => new PgnErrorInfo(PgnErrorCode.IllegalCharacter, startPosition, IllegalCharacterLength, new[] { displayCharValue });

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnIllegalCharacterSyntax Green { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => IllegalCharacterLength;

        internal PgnIllegalCharacterSyntax(PgnBackgroundListSyntax parent, int parentIndex, GreenPgnIllegalCharacterSyntax green)
            : base(parent, parentIndex)
            => Green = green;

        void IPgnSymbol.Accept(PgnSymbolVisitor visitor) => visitor.VisitIllegalCharacterSyntax(this);
        TResult IPgnSymbol.Accept<TResult>(PgnSymbolVisitor<TResult> visitor) => visitor.VisitIllegalCharacterSyntax(this);
        TResult IPgnSymbol.Accept<T, TResult>(PgnSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitIllegalCharacterSyntax(this, arg);

        public override void Accept(PgnBackgroundSyntaxVisitor visitor) => visitor.VisitIllegalCharacterSyntax(this);
        public override TResult Accept<TResult>(PgnBackgroundSyntaxVisitor<TResult> visitor) => visitor.VisitIllegalCharacterSyntax(this);
        public override TResult Accept<T, TResult>(PgnBackgroundSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitIllegalCharacterSyntax(this, arg);
    }
}

#region License
/*********************************************************************************
 * PgnParenthesisOpenSyntax.cs
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
    /// Represents the parenthesis open character '(' in PGN text.
    /// </summary>
    public sealed class GreenPgnParenthesisOpenSyntax : IGreenPgnSymbol
    {
        /// <summary>
        /// Gets the single <see cref="GreenPgnParenthesisOpenSyntax"/> value.
        /// </summary>
        public static GreenPgnParenthesisOpenSyntax Value { get; } = new GreenPgnParenthesisOpenSyntax();

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length => PgnParenthesisOpenSyntax.ParenthesisOpenLength;

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public PgnSymbolType SymbolType => PgnSymbolType.ParenthesisOpen;

        private GreenPgnParenthesisOpenSyntax() { }
    }

    /// <summary>
    /// Represents the parenthesis open character '(' in PGN text.
    /// </summary>
    public sealed class PgnParenthesisOpenSyntax : PgnSyntax, IPgnSymbol
    {
        public const char ParenthesisOpenCharacter = '(';
        public const int ParenthesisOpenLength = 1;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public PgnParenthesisOpenWithTriviaSyntax Parent { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnParenthesisOpenSyntax Green => GreenPgnParenthesisOpenSyntax.Value;

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Green.LeadingTrivia.Length;

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => ParenthesisOpenLength;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override PgnSyntax ParentSyntax => Parent;

        internal PgnParenthesisOpenSyntax(PgnParenthesisOpenWithTriviaSyntax parent) => Parent = parent;

        void IPgnSymbol.Accept(PgnSymbolVisitor visitor) => visitor.VisitParenthesisOpenSyntax(this);
        TResult IPgnSymbol.Accept<TResult>(PgnSymbolVisitor<TResult> visitor) => visitor.VisitParenthesisOpenSyntax(this);
        TResult IPgnSymbol.Accept<T, TResult>(PgnSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitParenthesisOpenSyntax(this, arg);
    }

    /// <summary>
    /// Represents the parenthesis open character '(', together with its leading trivia.
    /// </summary>
    public sealed class PgnParenthesisOpenWithTriviaSyntax : WithTriviaSyntax<PgnParenthesisOpenSyntax>
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public PgnVariationSyntax Parent { get; }

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => 0;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override PgnSyntax ParentSyntax => Parent;

        internal override PgnParenthesisOpenSyntax CreateContentNode() => new PgnParenthesisOpenSyntax(this);

        internal PgnParenthesisOpenWithTriviaSyntax(PgnVariationSyntax parent, GreenWithTriviaSyntax green)
            : base(green)
        {
            Parent = parent;
        }
    }
}

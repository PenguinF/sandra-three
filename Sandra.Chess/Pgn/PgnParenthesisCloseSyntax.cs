#region License
/*********************************************************************************
 * PgnParenthesisCloseSyntax.cs
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
    /// Represents the parenthesis close character ')' in PGN text.
    /// </summary>
    public sealed class GreenPgnParenthesisCloseSyntax : IGreenPgnSymbol
    {
        /// <summary>
        /// Gets the single <see cref="GreenPgnParenthesisCloseSyntax"/> value.
        /// </summary>
        public static GreenPgnParenthesisCloseSyntax Value { get; } = new GreenPgnParenthesisCloseSyntax();

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length => PgnParenthesisCloseSyntax.ParenthesisCloseLength;

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public PgnSymbolType SymbolType => PgnSymbolType.ParenthesisClose;

        private GreenPgnParenthesisCloseSyntax() { }
    }

    /// <summary>
    /// Represents the parenthesis close character ')' in PGN text.
    /// </summary>
    public sealed class PgnParenthesisCloseSyntax : PgnSyntax, IPgnSymbol
    {
        public const char ParenthesisCloseCharacter = ')';
        public const int ParenthesisCloseLength = 1;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public PgnParenthesisCloseWithTriviaSyntax Parent { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnParenthesisCloseSyntax Green => GreenPgnParenthesisCloseSyntax.Value;

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Green.LeadingTrivia.Length;

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => ParenthesisCloseLength;

        /// <summary>
        /// Gets the parent syntax node of this instance. Returns null for the root node.
        /// </summary>
        public override PgnSyntax ParentSyntax => Parent;

        internal PgnParenthesisCloseSyntax(PgnParenthesisCloseWithTriviaSyntax parent) => Parent = parent;

        void IPgnSymbol.Accept(PgnSymbolVisitor visitor) => visitor.VisitParenthesisCloseSyntax(this);
        TResult IPgnSymbol.Accept<TResult>(PgnSymbolVisitor<TResult> visitor) => visitor.VisitParenthesisCloseSyntax(this);
        TResult IPgnSymbol.Accept<T, TResult>(PgnSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitParenthesisCloseSyntax(this, arg);
    }

    /// <summary>
    /// Represents the parenthesis close character ')', together with its leading trivia.
    /// </summary>
    public sealed class PgnParenthesisCloseWithTriviaSyntax : WithTriviaSyntax<PgnParenthesisCloseSyntax>
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public PgnVariationSyntax Parent { get; }

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Green.Length - Green.Length;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override PgnSyntax ParentSyntax => Parent;

        internal override PgnParenthesisCloseSyntax CreateContentNode() => new PgnParenthesisCloseSyntax(this);

        internal PgnParenthesisCloseWithTriviaSyntax(PgnVariationSyntax parent, GreenWithTriviaSyntax green)
            : base(green)
        {
            Parent = parent;
        }
    }
}

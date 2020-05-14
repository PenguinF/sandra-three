#region License
/*********************************************************************************
 * PgnOrphanParenthesisCloseSyntax.cs
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
    /// Represents a parenthesis close character ')' without a matching '(' character.
    /// </summary>
    public sealed class GreenPgnOrphanParenthesisCloseSyntax : GreenPgnPlyFloatItemSyntax
    {
        /// <summary>
        /// Gets the single <see cref="GreenPgnOrphanParenthesisCloseSyntax"/> value.
        /// </summary>
        public static GreenPgnOrphanParenthesisCloseSyntax Value { get; } = new GreenPgnOrphanParenthesisCloseSyntax();

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public override int Length => PgnParenthesisCloseSyntax.ParenthesisCloseLength;

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public override PgnSymbolType SymbolType => PgnSymbolType.ParenthesisClose;

        private GreenPgnOrphanParenthesisCloseSyntax() { }

        internal override PgnPlyFloatItemSyntax CreateRedNode(PgnPlyFloatItemWithTriviaSyntax parent) => new PgnOrphanParenthesisCloseSyntax(parent);
    }

    /// <summary>
    /// Represents a parenthesis close character ')' without a matching '(' character.
    /// </summary>
    public sealed class PgnOrphanParenthesisCloseSyntax : PgnPlyFloatItemSyntax
    {
        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnOrphanParenthesisCloseSyntax Green => GreenPgnOrphanParenthesisCloseSyntax.Value;

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => PgnParenthesisCloseSyntax.ParenthesisCloseLength;

        internal PgnOrphanParenthesisCloseSyntax(PgnPlyFloatItemWithTriviaSyntax parent) : base(parent) { }

        public override void Accept(PgnSymbolVisitor visitor) => visitor.VisitOrphanParenthesisCloseSyntax(this);
        public override TResult Accept<TResult>(PgnSymbolVisitor<TResult> visitor) => visitor.VisitOrphanParenthesisCloseSyntax(this);
        public override TResult Accept<T, TResult>(PgnSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitOrphanParenthesisCloseSyntax(this, arg);
    }
}

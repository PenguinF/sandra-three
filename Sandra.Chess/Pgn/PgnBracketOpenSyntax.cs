#region License
/*********************************************************************************
 * PgnBracketOpenSyntax.cs
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

using System.Collections.Generic;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents the bracket open character '[' in PGN text.
    /// </summary>
    public sealed class GreenPgnBracketOpenSyntax : GreenPgnTagElementSyntax, IGreenPgnSymbol
    {
        /// <summary>
        /// Gets the single <see cref="GreenPgnBracketOpenSyntax"/> value.
        /// </summary>
        public static GreenPgnBracketOpenSyntax Value { get; } = new GreenPgnBracketOpenSyntax();

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public override int Length => PgnBracketOpenSyntax.BracketOpenLength;

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public PgnSymbolType SymbolType => PgnSymbolType.BracketOpen;

        private GreenPgnBracketOpenSyntax() { }

        IEnumerable<PgnErrorInfo> IGreenPgnSymbol.GetErrors(int startPosition) => EmptyEnumerable<PgnErrorInfo>.Instance;

        public override void Accept(GreenPgnTagElementSyntaxVisitor visitor) => visitor.VisitBracketOpenSyntax(this);
        public override TResult Accept<TResult>(GreenPgnTagElementSyntaxVisitor<TResult> visitor) => visitor.VisitBracketOpenSyntax(this);
        public override TResult Accept<T, TResult>(GreenPgnTagElementSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitBracketOpenSyntax(this, arg);
    }

    /// <summary>
    /// Represents the bracket open character '[' in PGN text.
    /// </summary>
    public sealed class PgnBracketOpenSyntax : PgnTagElementSyntax
    {
        public const char BracketOpenCharacter = '[';
        public const int BracketOpenLength = 1;

        public override void Accept(PgnTagElementSyntaxVisitor visitor) => visitor.VisitBracketOpenSyntax(this);
        public override TResult Accept<TResult>(PgnTagElementSyntaxVisitor<TResult> visitor) => visitor.VisitBracketOpenSyntax(this);
        public override TResult Accept<T, TResult>(PgnTagElementSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitBracketOpenSyntax(this, arg);
    }
}

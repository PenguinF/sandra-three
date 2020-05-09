﻿#region License
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

using Sandra.Chess.Pgn.Temp;
using System.Collections.Generic;

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

        IEnumerable<PgnErrorInfo> IGreenPgnSymbol.GetErrors(int startPosition) => EmptyEnumerable<PgnErrorInfo>.Instance;
    }

    public sealed class PgnParenthesisCloseSyntax : PgnSyntax, IPgnSymbol
    {
        public const char ParenthesisCloseCharacter = ')';
        public const int ParenthesisCloseLength = 1;

        public PgnParenthesisCloseWithTriviaSyntax Parent { get; }
        public GreenPgnParenthesisCloseSyntax Green { get; }
        public override int Start => Parent.Green.LeadingTrivia.Length;
        public override int Length => Green.Length;
        public override PgnSyntax ParentSyntax => Parent;

        internal PgnParenthesisCloseSyntax(PgnParenthesisCloseWithTriviaSyntax parent, GreenPgnParenthesisCloseSyntax green)
        {
            Parent = parent;
            Green = green;
        }

        void IPgnSymbol.Accept(PgnSymbolVisitor visitor) => visitor.VisitParenthesisCloseSyntax(this);
        TResult IPgnSymbol.Accept<TResult>(PgnSymbolVisitor<TResult> visitor) => visitor.VisitParenthesisCloseSyntax(this);
        TResult IPgnSymbol.Accept<T, TResult>(PgnSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitParenthesisCloseSyntax(this, arg);
    }

    public sealed class PgnParenthesisCloseWithTriviaSyntax : WithTriviaSyntax<GreenPgnParenthesisCloseSyntax, PgnParenthesisCloseSyntax>, IPgnTopLevelSyntax
    {
        PgnSyntax IPgnTopLevelSyntax.ToPgnSyntax() => this;

        public PgnSyntaxNodes Parent { get; }
        public int ParentIndex { get; }

        public override int Start => Parent.GreenTopLevelNodes.GetElementOffset(ParentIndex);
        public override PgnSyntax ParentSyntax => Parent;

        internal override PgnParenthesisCloseSyntax CreateContentNode() => new PgnParenthesisCloseSyntax(this, Green.ContentNode);

        internal PgnParenthesisCloseWithTriviaSyntax(PgnSyntaxNodes parent, int parentIndex, WithTrivia<GreenPgnParenthesisCloseSyntax> green)
            : base(green)
        {
            Parent = parent;
            ParentIndex = parentIndex;
        }
    }
}

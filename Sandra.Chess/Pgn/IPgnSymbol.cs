﻿#region License
/*********************************************************************************
 * IPgnSymbol.cs
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

using Eutherion.Text;
using Sandra.Chess.Pgn.Temp;
using System.Collections.Generic;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a terminal PGN symbol.
    /// Instances of this type are returned by <see cref="PgnParser"/>.
    /// </summary>
    public interface IGreenPgnSymbol : ISpan
    {
        /// <summary>
        /// Generates a sequence of errors associated with this symbol at a given start position.
        /// </summary>
        /// <param name="startPosition">
        /// The start position for which to generate the errors.
        /// </param>
        /// <returns>
        /// A sequence of errors associated with this symbol.
        /// </returns>
        IEnumerable<PgnErrorInfo> GetErrors(int startPosition);

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        PgnSymbolType SymbolType { get; }
    }

    /// <summary>
    /// Represents a terminal PGN symbol.
    /// These are all <see cref="PgnSyntax"/> nodes which have no child <see cref="PgnSyntax"/> nodes.
    /// Use <see cref="PgnSymbolVisitor"/> overrides to distinguish between implementations of this type.
    /// </summary>
    public interface IPgnSymbol : ISpan
    {
        void Accept(PgnSymbolVisitor visitor);
        TResult Accept<TResult>(PgnSymbolVisitor<TResult> visitor);
        TResult Accept<T, TResult>(PgnSymbolVisitor<T, TResult> visitor, T arg);
    }

    /// <summary>
    /// Contains extension methods for the <see cref="IPgnSymbol"/> interface.
    /// </summary>
    public static class PgnSymbolExtensions
    {
        private sealed class ToPgnSyntaxConverter : PgnSymbolVisitor<PgnSyntax>
        {
            public static readonly ToPgnSyntaxConverter Instance = new ToPgnSyntaxConverter();

            private ToPgnSyntaxConverter() { }

            public override PgnSyntax VisitBracketCloseSyntax(PgnBracketCloseSyntax node) => node;
            public override PgnSyntax VisitBracketOpenSyntax(PgnBracketOpenSyntax node) => node;
            public override PgnSyntax VisitCommentSyntax(PgnCommentSyntax node) => node;
            public override PgnSyntax VisitErrorTagValueSyntax(PgnErrorTagValueSyntax node) => node;
            public override PgnSyntax VisitEscapeSyntax(PgnEscapeSyntax node) => node;
            public override PgnSyntax VisitIllegalCharacterSyntax(PgnIllegalCharacterSyntax node) => node;
            public override PgnSyntax VisitTagNameSyntax(PgnTagNameSyntax node) => node;
            public override PgnSyntax VisitTagValueSyntax(PgnTagValueSyntax node) => node;
            public override PgnSyntax VisitWhitespaceSyntax(PgnWhitespaceSyntax node) => node;

            public override PgnSyntax VisitPgnSymbol(PgnSymbol node) => node;
        }

        /// <summary>
        /// Converts this <see cref="IPgnSymbol"/> to a <see cref="PgnSyntax"/> node.
        /// </summary>
        /// <param name="pgnSymbol">
        /// The <see cref="IPgnSymbol"/> to convert.
        /// </param>
        /// <returns>
        /// The converted <see cref="PgnSyntax"/> node.
        /// </returns>
        public static PgnSyntax ToSyntax(this IPgnSymbol pgnSymbol) => pgnSymbol.Accept(ToPgnSyntaxConverter.Instance);
    }
}

namespace Sandra.Chess.Pgn.Temp
{
    public class PgnSymbolWithTrivia : WithTriviaSyntax<IGreenPgnSymbol, PgnSymbol>, IPgnTopLevelSyntax
    {
        PgnSyntax IPgnTopLevelSyntax.ToPgnSyntax() => this;

        public PgnSyntaxNodes Parent { get; }
        public int ParentIndex { get; }

        public override int Start => Parent.GreenTopLevelNodes.GetElementOffset(ParentIndex);
        public override PgnSyntax ParentSyntax => Parent;

        internal override PgnSymbol CreateChildNode() => new PgnSymbol(this, Green.SyntaxNode);

        internal PgnSymbolWithTrivia(PgnSyntaxNodes parent, int parentIndex, GreenPgnTopLevelSymbolSyntax green)
            : base(green)
        {
            Parent = parent;
            ParentIndex = parentIndex;
        }
    }

    public class PgnSymbol : PgnSyntax, IPgnSymbol
    {
        public PgnSymbolWithTrivia Parent { get; }
        public IGreenPgnSymbol Green { get; }
        public override int Start => Parent.Green.LeadingTrivia.Length;
        public override int Length => Green.Length;
        public override PgnSyntax ParentSyntax => Parent;

        internal PgnSymbol(PgnSymbolWithTrivia parent, IGreenPgnSymbol green)
        {
            Parent = parent;
            Green = green;
        }

        void IPgnSymbol.Accept(PgnSymbolVisitor visitor) => visitor.VisitPgnSymbol(this);
        TResult IPgnSymbol.Accept<TResult>(PgnSymbolVisitor<TResult> visitor) => visitor.VisitPgnSymbol(this);
        TResult IPgnSymbol.Accept<T, TResult>(PgnSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitPgnSymbol(this, arg);
    }
}

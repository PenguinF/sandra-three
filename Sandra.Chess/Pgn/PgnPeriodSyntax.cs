#region License
/*********************************************************************************
 * PgnPeriodSyntax.cs
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

using Eutherion;
using Sandra.Chess.Pgn.Temp;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents the period character '.' in PGN text.
    /// </summary>
    public sealed class GreenPgnPeriodSyntax : IGreenPgnSymbol
    {
        /// <summary>
        /// Gets the single <see cref="GreenPgnPeriodSyntax"/> value.
        /// </summary>
        public static GreenPgnPeriodSyntax Value { get; } = new GreenPgnPeriodSyntax();

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length => PgnPeriodSyntax.PeriodLength;

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public PgnSymbolType SymbolType => PgnSymbolType.Period;

        private GreenPgnPeriodSyntax() { }
    }

    /// <summary>
    /// Represents the period character '.' in PGN text.
    /// </summary>
    public sealed class PgnPeriodSyntax : PgnSyntax, IPgnSymbol
    {
        public const char PeriodCharacter = '.';
        public const int PeriodLength = 1;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public PgnPeriodWithTriviaSyntax Parent { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnPeriodSyntax Green => GreenPgnPeriodSyntax.Value;

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Green.LeadingTrivia.Length;

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => PeriodLength;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override PgnSyntax ParentSyntax => Parent;

        internal PgnPeriodSyntax(PgnPeriodWithTriviaSyntax parent) => Parent = parent;

        void IPgnSymbol.Accept(PgnSymbolVisitor visitor) => visitor.VisitPeriodSyntax(this);
        TResult IPgnSymbol.Accept<TResult>(PgnSymbolVisitor<TResult> visitor) => visitor.VisitPeriodSyntax(this);
        TResult IPgnSymbol.Accept<T, TResult>(PgnSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitPeriodSyntax(this, arg);
    }

    /// <summary>
    /// Represents the period character '.' in PGN text, together with its leading trivia.
    /// </summary>
    public sealed class PgnPeriodWithTriviaSyntax : WithTriviaSyntax<PgnPeriodSyntax>, IPgnTopLevelSyntax
    {
        PgnSyntax IPgnTopLevelSyntax.ToPgnSyntax() => this;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public Union<PgnPlyFloatItemListSyntax, PgnSyntaxNodes> Parent { get; }

        /// <summary>
        /// Gets the index of this syntax node in its parent.
        /// </summary>
        public int ParentIndex { get; }

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Match(whenOption1: x => x.Green.GetElementOffset(ParentIndex), whenOption2: x => x.GreenTopLevelNodes.GetElementOffset(ParentIndex));

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override PgnSyntax ParentSyntax => Parent.Match<PgnSyntax>(whenOption1: x => x, whenOption2: x => x);

        internal override PgnPeriodSyntax CreateContentNode() => new PgnPeriodSyntax(this);

        internal PgnPeriodWithTriviaSyntax(Union<PgnPlyFloatItemListSyntax, PgnSyntaxNodes> parent, int parentIndex, GreenWithTriviaSyntax green)
            : base(green)
        {
            Parent = parent;
            ParentIndex = parentIndex;
        }
    }
}

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

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents the period character '.' in PGN text.
    /// </summary>
    public sealed class GreenPgnPeriodSyntax : GreenPgnPlyFloatItemSyntax
    {
        /// <summary>
        /// Gets the single <see cref="GreenPgnPeriodSyntax"/> value.
        /// </summary>
        public static GreenPgnPeriodSyntax Value { get; } = new GreenPgnPeriodSyntax();

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public override int Length => PgnPeriodSyntax.PeriodLength;

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public override PgnSymbolType SymbolType => PgnSymbolType.Period;

        private GreenPgnPeriodSyntax() { }

        internal override PgnPlyFloatItemSyntax CreateRedNode(PgnPlyFloatItemWithTriviaSyntax parent) => new PgnPeriodSyntax(parent);
    }

    /// <summary>
    /// Represents the period character '.' in PGN text.
    /// </summary>
    public sealed class PgnPeriodSyntax : PgnPlyFloatItemSyntax
    {
        public const char PeriodCharacter = '.';
        public const int PeriodLength = 1;

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnPeriodSyntax Green => GreenPgnPeriodSyntax.Value;

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => PeriodLength;

        internal PgnPeriodSyntax(PgnPlyFloatItemWithTriviaSyntax parent) : base(parent) { }

        public override void Accept(PgnSymbolVisitor visitor) => visitor.VisitPeriodSyntax(this);
        public override TResult Accept<TResult>(PgnSymbolVisitor<TResult> visitor) => visitor.VisitPeriodSyntax(this);
        public override TResult Accept<T, TResult>(PgnSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitPeriodSyntax(this, arg);
    }
}

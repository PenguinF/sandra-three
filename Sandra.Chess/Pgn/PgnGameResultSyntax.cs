﻿#region License
/*********************************************************************************
 * PgnGameResultSyntax.cs
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
    /// Represents any of four types of game termination markers in PGN.
    /// These are <see cref="GreenPgnAsteriskSyntax"/>, <see cref="GreenPgnBlackWinMarkerSyntax"/>,
    /// <see cref="GreenPgnDrawMarkerSyntax"/> and <see cref="GreenPgnWhiteWinMarkerSyntax"/>.
    /// </summary>
    public abstract class GreenPgnGameResultSyntax : IGreenPgnSymbol
    {
        /// <summary>
        /// Gets the <see cref="GreenPgnGameResultSyntax"/> which represents the undetermined result.
        /// </summary>
        public static GreenPgnGameResultSyntax UndeterminedResultSyntax => GreenPgnAsteriskSyntax.Value;

        /// <summary>
        /// Gets the <see cref="GreenPgnGameResultSyntax"/> which represents a black win.
        /// </summary>
        public static GreenPgnGameResultSyntax BlackWinsResultSyntax => GreenPgnBlackWinMarkerSyntax.Value;

        /// <summary>
        /// Gets the <see cref="GreenPgnGameResultSyntax"/> which represents a draw.
        /// </summary>
        public static GreenPgnGameResultSyntax DrawResultSyntax => GreenPgnDrawMarkerSyntax.Value;

        /// <summary>
        /// Gets the <see cref="GreenPgnGameResultSyntax"/> which represents a white win.
        /// </summary>
        public static GreenPgnGameResultSyntax WhiteWinsResultSyntax => GreenPgnWhiteWinMarkerSyntax.Value;

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public abstract int Length { get; }

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public abstract PgnSymbolType SymbolType { get; }

        /// <summary>
        /// Gets the type of game termination marker.
        /// </summary>
        public abstract PgnGameResult GameResult { get; }

        internal GreenPgnGameResultSyntax() { }

        IEnumerable<PgnErrorInfo> IGreenPgnSymbol.GetErrors(int startPosition) => EmptyEnumerable<PgnErrorInfo>.Instance;
    }

    /// <summary>
    /// Represents any of four types of game termination markers in PGN.
    /// </summary>
    public sealed class PgnGameResultSyntax : PgnSyntax, IPgnSymbol
    {
        public const char AsteriskCharacter = '*';
        public const int AsteriskLength = 1;

        public static readonly string BlackWinMarkerText = "0-1";
        public const int BlackWinMarkerLength = 3;

        public static readonly string DrawMarkerText = "1/2-1/2";
        public const int DrawMarkerLength = 7;

        public static readonly string WhiteWinMarkerText = "1-0";
        public const int WhiteWinMarkerLength = 3;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public PgnGameResultWithTriviaSyntax Parent { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnGameResultSyntax Green { get; }

        /// <summary>
        /// Gets the type of game termination marker.
        /// </summary>
        public PgnGameResult GameResult => Green.GameResult;

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Green.LeadingTrivia.Length;

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public override int Length => Green.Length;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override PgnSyntax ParentSyntax => Parent;

        internal PgnGameResultSyntax(PgnGameResultWithTriviaSyntax parent, GreenPgnGameResultSyntax green)
        {
            Parent = parent;
            Green = green;
        }

        void IPgnSymbol.Accept(PgnSymbolVisitor visitor) => visitor.VisitGameResultSyntax(this);
        TResult IPgnSymbol.Accept<TResult>(PgnSymbolVisitor<TResult> visitor) => visitor.VisitGameResultSyntax(this);
        TResult IPgnSymbol.Accept<T, TResult>(PgnSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitGameResultSyntax(this, arg);
    }

    public sealed class PgnGameResultWithTriviaSyntax : WithTriviaSyntax<GreenPgnGameResultSyntax, PgnGameResultSyntax>, IPgnTopLevelSyntax
    {
        PgnSyntax IPgnTopLevelSyntax.ToPgnSyntax() => this;

        public PgnSyntaxNodes Parent { get; }
        public int ParentIndex { get; }

        public override int Start => Parent.GreenTopLevelNodes.GetElementOffset(ParentIndex);
        public override PgnSyntax ParentSyntax => Parent;

        internal override PgnGameResultSyntax CreateContentNode() => new PgnGameResultSyntax(this, Green.ContentNode);

        internal PgnGameResultWithTriviaSyntax(PgnSyntaxNodes parent, int parentIndex, WithTrivia<GreenPgnGameResultSyntax> green)
            : base(green)
        {
            Parent = parent;
            ParentIndex = parentIndex;
        }
    }
}

/*********************************************************************************
 * PGNSyntax.cs
 * 
 * Copyright (c) 2004-2018 Henk Nicolai
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
 *********************************************************************************/
using System;

namespace Sandra.PGN
{
    public interface PGNTerminalSymbol : IEquatable<PGNTerminalSymbol>
    {
        void Accept(PGNTerminalSymbolVisitor visitor);
        TResult Accept<TResult>(PGNTerminalSymbolVisitor<TResult> visitor);
    }

    public abstract class PGNTerminalSymbolVisitor
    {
        public virtual void DefaultVisit(PGNTerminalSymbol symbol) { }
        public virtual void Visit(PGNTerminalSymbol symbol) { if (symbol != null) symbol.Accept(this); }
        public virtual void VisitBlackToMoveEllipsisSymbol(BlackToMoveEllipsisSymbol symbol) => DefaultVisit(symbol);
        public virtual void VisitFormattedMoveSymbol(FormattedMoveSymbol symbol) => DefaultVisit(symbol);
        public virtual void VisitMoveCounterSymbol(MoveCounterSymbol symbol) => DefaultVisit(symbol);
        public virtual void VisitSideLineEndSymbol(SideLineEndSymbol symbol) => DefaultVisit(symbol);
        public virtual void VisitSideLineStartSymbol(SideLineStartSymbol symbol) => DefaultVisit(symbol);
        public virtual void VisitSpaceSymbol(SpaceSymbol symbol) => DefaultVisit(symbol);
    }

    public abstract class PGNTerminalSymbolVisitor<TResult>
    {
        public virtual TResult DefaultVisit(PGNTerminalSymbol symbol) => default(TResult);
        public virtual TResult Visit(PGNTerminalSymbol symbol) => symbol == null ? default(TResult) : symbol.Accept(this);
        public virtual TResult VisitBlackToMoveEllipsisSymbol(BlackToMoveEllipsisSymbol symbol) => DefaultVisit(symbol);
        public virtual TResult VisitFormattedMoveSymbol(FormattedMoveSymbol symbol) => DefaultVisit(symbol);
        public virtual TResult VisitMoveCounterSymbol(MoveCounterSymbol symbol) => DefaultVisit(symbol);
        public virtual TResult VisitSideLineEndSymbol(SideLineEndSymbol symbol) => DefaultVisit(symbol);
        public virtual TResult VisitSideLineStartSymbol(SideLineStartSymbol symbol) => DefaultVisit(symbol);
        public virtual TResult VisitSpaceSymbol(SpaceSymbol symbol) => DefaultVisit(symbol);
    }

    public sealed class SpaceSymbol : PGNTerminalSymbol
    {
        public const string SpaceText = " ";

        public bool Equals(PGNTerminalSymbol other) => other is SpaceSymbol;
        public void Accept(PGNTerminalSymbolVisitor visitor) => visitor.VisitSpaceSymbol(this);
        public TResult Accept<TResult>(PGNTerminalSymbolVisitor<TResult> visitor) => visitor.VisitSpaceSymbol(this);
    }

    public sealed class SideLineStartSymbol : PGNTerminalSymbol
    {
        public const string SideLineStartText = "(";

        public bool Equals(PGNTerminalSymbol other) => other is SideLineStartSymbol;
        public void Accept(PGNTerminalSymbolVisitor visitor) => visitor.VisitSideLineStartSymbol(this);
        public TResult Accept<TResult>(PGNTerminalSymbolVisitor<TResult> visitor) => visitor.VisitSideLineStartSymbol(this);
    }

    public sealed class SideLineEndSymbol : PGNTerminalSymbol
    {
        public const string SideLineEndText = ")";

        public bool Equals(PGNTerminalSymbol other) => other is SideLineEndSymbol;
        public void Accept(PGNTerminalSymbolVisitor visitor) => visitor.VisitSideLineEndSymbol(this);
        public TResult Accept<TResult>(PGNTerminalSymbolVisitor<TResult> visitor) => visitor.VisitSideLineEndSymbol(this);
    }

    public sealed class BlackToMoveEllipsisSymbol : PGNTerminalSymbol
    {
        public const string EllipsisText = "..";

        public bool Equals(PGNTerminalSymbol other) => other is BlackToMoveEllipsisSymbol;
        public void Accept(PGNTerminalSymbolVisitor visitor) => visitor.VisitBlackToMoveEllipsisSymbol(this);
        public TResult Accept<TResult>(PGNTerminalSymbolVisitor<TResult> visitor) => visitor.VisitBlackToMoveEllipsisSymbol(this);
    }

    public sealed class MoveCounterSymbol : PGNTerminalSymbol
    {
        public readonly int MoveCounter;

        public MoveCounterSymbol(int moveCounter)
        {
            MoveCounter = moveCounter;
        }

        public bool Equals(PGNTerminalSymbol other) => other is MoveCounterSymbol && MoveCounter == ((MoveCounterSymbol)other).MoveCounter;
        public void Accept(PGNTerminalSymbolVisitor visitor) => visitor.VisitMoveCounterSymbol(this);
        public TResult Accept<TResult>(PGNTerminalSymbolVisitor<TResult> visitor) => visitor.VisitMoveCounterSymbol(this);
    }

    public sealed class FormattedMoveSymbol : PGNTerminalSymbol
    {
        public readonly string Notation;
        public readonly Chess.Variation Variation;

        public FormattedMoveSymbol(string notation, Chess.Variation variation)
        {
            Notation = notation;
            Variation = variation;
        }

        public bool Equals(PGNTerminalSymbol other) => other is FormattedMoveSymbol && Variation == ((FormattedMoveSymbol)other).Variation;
        public void Accept(PGNTerminalSymbolVisitor visitor) => visitor.VisitFormattedMoveSymbol(this);
        public TResult Accept<TResult>(PGNTerminalSymbolVisitor<TResult> visitor) => visitor.VisitFormattedMoveSymbol(this);
    }

    public class PGNTerminalSymbolTextGenerator : PGNTerminalSymbolVisitor<string>
    {
        public override string VisitBlackToMoveEllipsisSymbol(BlackToMoveEllipsisSymbol symbol) => BlackToMoveEllipsisSymbol.EllipsisText;
        public override string VisitFormattedMoveSymbol(FormattedMoveSymbol symbol) => symbol.Notation;
        public override string VisitMoveCounterSymbol(MoveCounterSymbol symbol) => $"{symbol.MoveCounter}.";
        public override string VisitSideLineEndSymbol(SideLineEndSymbol symbol) => SideLineEndSymbol.SideLineEndText;
        public override string VisitSideLineStartSymbol(SideLineStartSymbol symbol) => SideLineStartSymbol.SideLineStartText;
        public override string VisitSpaceSymbol(SpaceSymbol symbol) => SpaceSymbol.SpaceText;
    }

    public class PGNMoveSearcher : PGNTerminalSymbolVisitor<bool>
    {
        private readonly Chess.MoveTree needle;
        public PGNMoveSearcher(Chess.MoveTree needle) { this.needle = needle; }
        public override bool VisitFormattedMoveSymbol(FormattedMoveSymbol symbol) => symbol.Variation.MoveTree == needle;
    }
}

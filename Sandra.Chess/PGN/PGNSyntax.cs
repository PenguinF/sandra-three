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
        string GetText();
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
        public string GetText() => SpaceText;
    }

    public sealed class SideLineStartSymbol : PGNTerminalSymbol
    {
        public const string SideLineStartText = "(";

        public bool Equals(PGNTerminalSymbol other) => other is SideLineStartSymbol;
        public void Accept(PGNTerminalSymbolVisitor visitor) => visitor.VisitSideLineStartSymbol(this);
        public TResult Accept<TResult>(PGNTerminalSymbolVisitor<TResult> visitor) => visitor.VisitSideLineStartSymbol(this);
        public string GetText() => SideLineStartText;
    }

    public sealed class SideLineEndSymbol : PGNTerminalSymbol
    {
        public const string SideLineEndText = ")";

        public bool Equals(PGNTerminalSymbol other) => other is SideLineEndSymbol;
        public void Accept(PGNTerminalSymbolVisitor visitor) => visitor.VisitSideLineEndSymbol(this);
        public TResult Accept<TResult>(PGNTerminalSymbolVisitor<TResult> visitor) => visitor.VisitSideLineEndSymbol(this);
        public string GetText() => SideLineEndText;
    }

    public sealed class BlackToMoveEllipsisSymbol : PGNTerminalSymbol
    {
        public const string EllipsisText = "..";

        public bool Equals(PGNTerminalSymbol other) => other is BlackToMoveEllipsisSymbol;
        public void Accept(PGNTerminalSymbolVisitor visitor) => visitor.VisitBlackToMoveEllipsisSymbol(this);
        public TResult Accept<TResult>(PGNTerminalSymbolVisitor<TResult> visitor) => visitor.VisitBlackToMoveEllipsisSymbol(this);
        public string GetText() => EllipsisText;
    }

    public sealed class MoveCounterSymbol : PGNTerminalSymbol
    {
        readonly int value;
        public MoveCounterSymbol(int value) { this.value = value; }

        public bool Equals(PGNTerminalSymbol other) => other is MoveCounterSymbol && value == ((MoveCounterSymbol)other).value;
        public void Accept(PGNTerminalSymbolVisitor visitor) => visitor.VisitMoveCounterSymbol(this);
        public TResult Accept<TResult>(PGNTerminalSymbolVisitor<TResult> visitor) => visitor.VisitMoveCounterSymbol(this);
        public string GetText() => value + ".";
    }

    public sealed class FormattedMoveSymbol : PGNTerminalSymbol
    {
        readonly string value;
        public readonly Chess.Variation Variation;
        public FormattedMoveSymbol(string value, Chess.Variation variation)
        {
            this.value = value;
            Variation = variation;
        }

        public bool Equals(PGNTerminalSymbol other) => other is FormattedMoveSymbol && value == ((FormattedMoveSymbol)other).value;
        public void Accept(PGNTerminalSymbolVisitor visitor) => visitor.VisitFormattedMoveSymbol(this);
        public TResult Accept<TResult>(PGNTerminalSymbolVisitor<TResult> visitor) => visitor.VisitFormattedMoveSymbol(this);
        public string GetText() => value;
    }
}

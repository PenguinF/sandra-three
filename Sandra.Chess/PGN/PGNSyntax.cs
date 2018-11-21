#region License
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
#endregion

using System;
using System.Collections.Generic;

namespace Sandra.PGN
{
    public sealed class PGNLine
    {
        // Null for the root move list.
        public PGNPlyWithSidelines Parent { get; internal set; }
        public int ParentIndex { get; internal set; }

        // TODO: use System.Collections.Immutable.
        public readonly IReadOnlyList<PGNPlyWithSidelines> Plies;

        public PGNLine(IEnumerable<PGNPlyWithSidelines> plies)
        {
            if (plies == null) throw new ArgumentNullException(nameof(plies));
            var plyList = new List<PGNPlyWithSidelines>(plies);
            for (int i = 0; i < plyList.Count; ++i)
            {
                if (plyList[i].Parent != null) throw new ArgumentException($"{nameof(plyList)}[{i}] already has a parent {nameof(PGNLine)}.");
                plyList[i].Parent = this;
                plyList[i].ParentIndex = i;
            }
            Plies = plyList;
        }

        public IEnumerable<PGNTerminalSymbol> GenerateTerminalSymbols()
        {
            bool precededByFormattedMoveSymbol = false;
            bool emitSpace = false;

            foreach (var plyWithSidelines in Plies)
            {
                // Emit the main move before side lines which start at the same plyCount.
                if (plyWithSidelines.Ply != null)
                {
                    if (emitSpace) yield return SpaceSymbol.Value; else emitSpace = true;
                    foreach (var symbol in plyWithSidelines.Ply.GenerateTerminalSymbols(precededByFormattedMoveSymbol)) yield return symbol;
                    precededByFormattedMoveSymbol = true;
                }

                if (plyWithSidelines.SideLines != null)
                {
                    foreach (var sideLine in plyWithSidelines.SideLines)
                    {
                        if (emitSpace) yield return SpaceSymbol.Value; else emitSpace = true;
                        yield return new SideLineStartSymbol(sideLine);
                        foreach (var element in sideLine.GenerateTerminalSymbols()) yield return element;
                        yield return new SideLineEndSymbol(sideLine);
                        precededByFormattedMoveSymbol = false;
                    }
                }
            }
        }
    }

    public sealed class PGNPlyWithSidelines
    {
        public PGNLine Parent { get; internal set; }
        public int ParentIndex { get; internal set; }

        public readonly PGNPly Ply;
        // TODO: use System.Collections.Immutable.
        public readonly IReadOnlyList<PGNLine> SideLines;

        public PGNPlyWithSidelines(PGNPly ply, IEnumerable<PGNLine> sideLines)
        {
            if (ply != null)
            {
                if (ply.Parent != null) throw new ArgumentException($"{nameof(ply)} already has a parent {nameof(PGNPlyWithSidelines)}.");
                ply.Parent = this;
            }
            Ply = ply;
            if (sideLines != null)
            {
                var sideLineList = new List<PGNLine>(sideLines);
                for (int i = 0; i < sideLineList.Count; ++i)
                {
                    if (sideLineList[i].Parent != null) throw new ArgumentException($"{nameof(sideLines)}[{i}] already has a parent {nameof(PGNPlyWithSidelines)}.");
                    sideLineList[i].Parent = this;
                    sideLineList[i].ParentIndex = i;
                }
                SideLines = sideLineList;
            }
        }
    }

    public sealed class PGNPly
    {
        public PGNPlyWithSidelines Parent { get; internal set; }

        public readonly int PlyCount;
        public readonly string Notation;
        public readonly Chess.Variation Variation;

        public PGNPly(int plyCount, string notation, Chess.Variation variation)
        {
            PlyCount = plyCount;
            Notation = notation ?? throw new ArgumentNullException(nameof(notation));
            Variation = variation ?? throw new ArgumentNullException(nameof(variation));
        }

        public IEnumerable<PGNTerminalSymbol> GenerateTerminalSymbols(bool precededByFormattedMoveSymbol)
        {
            if (PlyCount % 2 == 0)
            {
                yield return new MoveCounterSymbol(this);
                yield return SpaceSymbol.Value;
            }
            else if (!precededByFormattedMoveSymbol)
            {
                yield return new MoveCounterSymbol(this);
                yield return new BlackToMoveEllipsisSymbol(this);
                yield return SpaceSymbol.Value;
            }
            yield return new FormattedMoveSymbol(this);
        }
    }

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

        public static readonly SpaceSymbol Value = new SpaceSymbol();

        public bool Equals(PGNTerminalSymbol other) => other is SpaceSymbol;

        public void Accept(PGNTerminalSymbolVisitor visitor) => visitor.VisitSpaceSymbol(this);
        public TResult Accept<TResult>(PGNTerminalSymbolVisitor<TResult> visitor) => visitor.VisitSpaceSymbol(this);
    }

    public sealed class SideLineStartSymbol : PGNTerminalSymbol
    {
        public const string SideLineStartText = "(";

        public readonly PGNLine SideLine;

        public SideLineStartSymbol(PGNLine sideLine)
        {
            if (sideLine == null) throw new ArgumentNullException(nameof(sideLine));
            if (sideLine.Plies.Count == 0) throw new ArgumentException($"{nameof(sideLine)}.{nameof(sideLine.Plies)} must be one or greater.");
            SideLine = sideLine;
        }

        public bool Equals(PGNTerminalSymbol other)
            => other is SideLineStartSymbol
            && SideLine.Plies[0].Ply.Variation == ((SideLineStartSymbol)other).SideLine.Plies[0].Ply.Variation;

        public void Accept(PGNTerminalSymbolVisitor visitor) => visitor.VisitSideLineStartSymbol(this);
        public TResult Accept<TResult>(PGNTerminalSymbolVisitor<TResult> visitor) => visitor.VisitSideLineStartSymbol(this);
    }

    public sealed class SideLineEndSymbol : PGNTerminalSymbol
    {
        public const string SideLineEndText = ")";

        public readonly PGNLine SideLine;

        public SideLineEndSymbol(PGNLine sideLine)
        {
            if (sideLine == null) throw new ArgumentNullException(nameof(sideLine));
            if (sideLine.Plies.Count == 0) throw new ArgumentException($"{nameof(sideLine)}.{nameof(sideLine.Plies)} must be one or greater.");
            SideLine = sideLine;
        }

        public bool Equals(PGNTerminalSymbol other)
            => other is SideLineEndSymbol
            && SideLine.Plies[0].Ply.Variation == ((SideLineEndSymbol)other).SideLine.Plies[0].Ply.Variation;

        public void Accept(PGNTerminalSymbolVisitor visitor) => visitor.VisitSideLineEndSymbol(this);
        public TResult Accept<TResult>(PGNTerminalSymbolVisitor<TResult> visitor) => visitor.VisitSideLineEndSymbol(this);
    }

    public sealed class BlackToMoveEllipsisSymbol : PGNTerminalSymbol
    {
        public const string EllipsisText = "..";

        public readonly PGNPly Ply;

        public BlackToMoveEllipsisSymbol(PGNPly ply)
        {
            Ply = ply ?? throw new ArgumentNullException(nameof(ply));
        }

        public bool Equals(PGNTerminalSymbol other)
            => other is BlackToMoveEllipsisSymbol
            && Ply.Variation == ((BlackToMoveEllipsisSymbol)other).Ply.Variation;

        public void Accept(PGNTerminalSymbolVisitor visitor) => visitor.VisitBlackToMoveEllipsisSymbol(this);
        public TResult Accept<TResult>(PGNTerminalSymbolVisitor<TResult> visitor) => visitor.VisitBlackToMoveEllipsisSymbol(this);
    }

    public sealed class MoveCounterSymbol : PGNTerminalSymbol
    {
        public readonly PGNPly Ply;

        public MoveCounterSymbol(PGNPly ply)
        {
            Ply = ply ?? throw new ArgumentNullException(nameof(ply));
        }

        public bool Equals(PGNTerminalSymbol other)
            => other is MoveCounterSymbol
            && Ply.Variation == ((MoveCounterSymbol)other).Ply.Variation;

        public void Accept(PGNTerminalSymbolVisitor visitor) => visitor.VisitMoveCounterSymbol(this);
        public TResult Accept<TResult>(PGNTerminalSymbolVisitor<TResult> visitor) => visitor.VisitMoveCounterSymbol(this);
    }

    public sealed class FormattedMoveSymbol : PGNTerminalSymbol
    {
        public readonly PGNPly Ply;

        public FormattedMoveSymbol(PGNPly ply)
        {
            Ply = ply ?? throw new ArgumentNullException(nameof(ply));
        }

        public bool Equals(PGNTerminalSymbol other)
            => other is FormattedMoveSymbol
            && Ply.Variation == ((FormattedMoveSymbol)other).Ply.Variation;

        public void Accept(PGNTerminalSymbolVisitor visitor) => visitor.VisitFormattedMoveSymbol(this);
        public TResult Accept<TResult>(PGNTerminalSymbolVisitor<TResult> visitor) => visitor.VisitFormattedMoveSymbol(this);
    }

    public class PGNTerminalSymbolTextGenerator : PGNTerminalSymbolVisitor<string>
    {
        public override string VisitBlackToMoveEllipsisSymbol(BlackToMoveEllipsisSymbol symbol) => BlackToMoveEllipsisSymbol.EllipsisText;
        public override string VisitFormattedMoveSymbol(FormattedMoveSymbol symbol) => symbol.Ply.Notation;
        public override string VisitMoveCounterSymbol(MoveCounterSymbol symbol) => $"{symbol.Ply.PlyCount / 2 + 1}.";
        public override string VisitSideLineEndSymbol(SideLineEndSymbol symbol) => SideLineEndSymbol.SideLineEndText;
        public override string VisitSideLineStartSymbol(SideLineStartSymbol symbol) => SideLineStartSymbol.SideLineStartText;
        public override string VisitSpaceSymbol(SpaceSymbol symbol) => SpaceSymbol.SpaceText;
    }

    public class PGNMoveSearcher : PGNTerminalSymbolVisitor<bool>
    {
        private readonly Chess.MoveTree needle;
        public PGNMoveSearcher(Chess.MoveTree needle) { this.needle = needle; }
        public override bool VisitFormattedMoveSymbol(FormattedMoveSymbol symbol) => symbol.Ply.Variation.MoveTree == needle;
    }

    public class PGNActivePlyDetector : PGNTerminalSymbolVisitor<PGNPly>
    {
        public override PGNPly VisitBlackToMoveEllipsisSymbol(BlackToMoveEllipsisSymbol symbol) => symbol.Ply;
        public override PGNPly VisitFormattedMoveSymbol(FormattedMoveSymbol symbol) => symbol.Ply;
        public override PGNPly VisitMoveCounterSymbol(MoveCounterSymbol symbol) => symbol.Ply;
        public override PGNPly VisitSideLineEndSymbol(SideLineEndSymbol symbol) => symbol.SideLine.Plies[symbol.SideLine.Plies.Count - 1].Ply;
        public override PGNPly VisitSideLineStartSymbol(SideLineStartSymbol symbol) => symbol.SideLine.Plies[0].Ply;
    }
}

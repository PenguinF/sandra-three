#region License
/*********************************************************************************
 * PgnSyntaxDeprecated.cs
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

using Eutherion.Utils;
using System;
using System.Collections.Generic;

namespace Sandra.PgnDeprecated
{
    public sealed class PgnLine
    {
        // Null for the root move list.
        public PgnPlyWithSidelines Parent { get; internal set; }
        public int ParentIndex { get; internal set; }

        public readonly IReadOnlyList<PgnPlyWithSidelines> Plies;

        public PgnLine(IEnumerable<PgnPlyWithSidelines> plies)
        {
            if (plies == null) throw new ArgumentNullException(nameof(plies));
            var plyList = ReadOnlyList<PgnPlyWithSidelines>.Create(plies);
            for (int i = 0; i < plyList.Count; ++i)
            {
                if (plyList[i].Parent != null) throw new ArgumentException($"{nameof(plyList)}[{i}] already has a parent {nameof(PgnLine)}.");
                plyList[i].Parent = this;
                plyList[i].ParentIndex = i;
            }
            Plies = plyList;
        }

        public IEnumerable<IPgnTerminalSymbol> GenerateTerminalSymbols()
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

    public sealed class PgnPlyWithSidelines
    {
        public PgnLine Parent { get; internal set; }
        public int ParentIndex { get; internal set; }

        public readonly PgnPly Ply;
        public readonly IReadOnlyList<PgnLine> SideLines;

        public PgnPlyWithSidelines(PgnPly ply, IEnumerable<PgnLine> sideLines)
        {
            if (ply != null)
            {
                if (ply.Parent != null) throw new ArgumentException($"{nameof(ply)} already has a parent {nameof(PgnPlyWithSidelines)}.");
                ply.Parent = this;
            }
            Ply = ply;
            if (sideLines != null)
            {
                var sideLineList = ReadOnlyList<PgnLine>.Create(sideLines);
                for (int i = 0; i < sideLineList.Count; ++i)
                {
                    if (sideLineList[i].Parent != null) throw new ArgumentException($"{nameof(sideLines)}[{i}] already has a parent {nameof(PgnPlyWithSidelines)}.");
                    sideLineList[i].Parent = this;
                    sideLineList[i].ParentIndex = i;
                }
                SideLines = sideLineList;
            }
        }
    }

    public sealed class PgnPly
    {
        public PgnPlyWithSidelines Parent { get; internal set; }

        public readonly int PlyCount;
        public readonly string Notation;
        public readonly Chess.Variation Variation;

        public PgnPly(int plyCount, string notation, Chess.Variation variation)
        {
            PlyCount = plyCount;
            Notation = notation ?? throw new ArgumentNullException(nameof(notation));
            Variation = variation ?? throw new ArgumentNullException(nameof(variation));
        }

        public IEnumerable<IPgnTerminalSymbol> GenerateTerminalSymbols(bool precededByFormattedMoveSymbol)
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

    public interface IPgnTerminalSymbol : IEquatable<IPgnTerminalSymbol>
    {
        void Accept(PgnTerminalSymbolVisitor visitor);
        TResult Accept<TResult>(PgnTerminalSymbolVisitor<TResult> visitor);
    }

    public abstract class PgnTerminalSymbolVisitor
    {
        public virtual void DefaultVisit(IPgnTerminalSymbol symbol) { }
        public virtual void Visit(IPgnTerminalSymbol symbol) { if (symbol != null) symbol.Accept(this); }
        public virtual void VisitBlackToMoveEllipsisSymbol(BlackToMoveEllipsisSymbol symbol) => DefaultVisit(symbol);
        public virtual void VisitFormattedMoveSymbol(FormattedMoveSymbol symbol) => DefaultVisit(symbol);
        public virtual void VisitMoveCounterSymbol(MoveCounterSymbol symbol) => DefaultVisit(symbol);
        public virtual void VisitSideLineEndSymbol(SideLineEndSymbol symbol) => DefaultVisit(symbol);
        public virtual void VisitSideLineStartSymbol(SideLineStartSymbol symbol) => DefaultVisit(symbol);
        public virtual void VisitSpaceSymbol(SpaceSymbol symbol) => DefaultVisit(symbol);
    }

    public abstract class PgnTerminalSymbolVisitor<TResult>
    {
        public virtual TResult DefaultVisit(IPgnTerminalSymbol symbol) => default;
        public virtual TResult Visit(IPgnTerminalSymbol symbol) => symbol == null ? default : symbol.Accept(this);
        public virtual TResult VisitBlackToMoveEllipsisSymbol(BlackToMoveEllipsisSymbol symbol) => DefaultVisit(symbol);
        public virtual TResult VisitFormattedMoveSymbol(FormattedMoveSymbol symbol) => DefaultVisit(symbol);
        public virtual TResult VisitMoveCounterSymbol(MoveCounterSymbol symbol) => DefaultVisit(symbol);
        public virtual TResult VisitSideLineEndSymbol(SideLineEndSymbol symbol) => DefaultVisit(symbol);
        public virtual TResult VisitSideLineStartSymbol(SideLineStartSymbol symbol) => DefaultVisit(symbol);
        public virtual TResult VisitSpaceSymbol(SpaceSymbol symbol) => DefaultVisit(symbol);
    }

    public sealed class SpaceSymbol : IPgnTerminalSymbol
    {
        public const string SpaceText = " ";

        public static readonly SpaceSymbol Value = new SpaceSymbol();

        public bool Equals(IPgnTerminalSymbol other) => other is SpaceSymbol;

        public void Accept(PgnTerminalSymbolVisitor visitor) => visitor.VisitSpaceSymbol(this);
        public TResult Accept<TResult>(PgnTerminalSymbolVisitor<TResult> visitor) => visitor.VisitSpaceSymbol(this);
    }

    public sealed class SideLineStartSymbol : IPgnTerminalSymbol
    {
        public const string SideLineStartText = "(";

        public readonly PgnLine SideLine;

        public SideLineStartSymbol(PgnLine sideLine)
        {
            if (sideLine == null) throw new ArgumentNullException(nameof(sideLine));
            if (sideLine.Plies.Count == 0) throw new ArgumentException($"{nameof(sideLine)}.{nameof(sideLine.Plies)} must be one or greater.");
            SideLine = sideLine;
        }

        public bool Equals(IPgnTerminalSymbol other)
            => other is SideLineStartSymbol
            && SideLine.Plies[0].Ply.Variation == ((SideLineStartSymbol)other).SideLine.Plies[0].Ply.Variation;

        public void Accept(PgnTerminalSymbolVisitor visitor) => visitor.VisitSideLineStartSymbol(this);
        public TResult Accept<TResult>(PgnTerminalSymbolVisitor<TResult> visitor) => visitor.VisitSideLineStartSymbol(this);
    }

    public sealed class SideLineEndSymbol : IPgnTerminalSymbol
    {
        public const string SideLineEndText = ")";

        public readonly PgnLine SideLine;

        public SideLineEndSymbol(PgnLine sideLine)
        {
            if (sideLine == null) throw new ArgumentNullException(nameof(sideLine));
            if (sideLine.Plies.Count == 0) throw new ArgumentException($"{nameof(sideLine)}.{nameof(sideLine.Plies)} must be one or greater.");
            SideLine = sideLine;
        }

        public bool Equals(IPgnTerminalSymbol other)
            => other is SideLineEndSymbol
            && SideLine.Plies[0].Ply.Variation == ((SideLineEndSymbol)other).SideLine.Plies[0].Ply.Variation;

        public void Accept(PgnTerminalSymbolVisitor visitor) => visitor.VisitSideLineEndSymbol(this);
        public TResult Accept<TResult>(PgnTerminalSymbolVisitor<TResult> visitor) => visitor.VisitSideLineEndSymbol(this);
    }

    public sealed class BlackToMoveEllipsisSymbol : IPgnTerminalSymbol
    {
        public const string EllipsisText = "..";

        public readonly PgnPly Ply;

        public BlackToMoveEllipsisSymbol(PgnPly ply)
        {
            Ply = ply ?? throw new ArgumentNullException(nameof(ply));
        }

        public bool Equals(IPgnTerminalSymbol other)
            => other is BlackToMoveEllipsisSymbol
            && Ply.Variation == ((BlackToMoveEllipsisSymbol)other).Ply.Variation;

        public void Accept(PgnTerminalSymbolVisitor visitor) => visitor.VisitBlackToMoveEllipsisSymbol(this);
        public TResult Accept<TResult>(PgnTerminalSymbolVisitor<TResult> visitor) => visitor.VisitBlackToMoveEllipsisSymbol(this);
    }

    public sealed class MoveCounterSymbol : IPgnTerminalSymbol
    {
        public readonly PgnPly Ply;

        public MoveCounterSymbol(PgnPly ply)
        {
            Ply = ply ?? throw new ArgumentNullException(nameof(ply));
        }

        public bool Equals(IPgnTerminalSymbol other)
            => other is MoveCounterSymbol
            && Ply.Variation == ((MoveCounterSymbol)other).Ply.Variation;

        public void Accept(PgnTerminalSymbolVisitor visitor) => visitor.VisitMoveCounterSymbol(this);
        public TResult Accept<TResult>(PgnTerminalSymbolVisitor<TResult> visitor) => visitor.VisitMoveCounterSymbol(this);
    }

    public sealed class FormattedMoveSymbol : IPgnTerminalSymbol
    {
        public readonly PgnPly Ply;

        public FormattedMoveSymbol(PgnPly ply)
        {
            Ply = ply ?? throw new ArgumentNullException(nameof(ply));
        }

        public bool Equals(IPgnTerminalSymbol other)
            => other is FormattedMoveSymbol
            && Ply.Variation == ((FormattedMoveSymbol)other).Ply.Variation;

        public void Accept(PgnTerminalSymbolVisitor visitor) => visitor.VisitFormattedMoveSymbol(this);
        public TResult Accept<TResult>(PgnTerminalSymbolVisitor<TResult> visitor) => visitor.VisitFormattedMoveSymbol(this);
    }

    public class PgnTerminalSymbolTextGenerator : PgnTerminalSymbolVisitor<string>
    {
        public override string VisitBlackToMoveEllipsisSymbol(BlackToMoveEllipsisSymbol symbol) => BlackToMoveEllipsisSymbol.EllipsisText;
        public override string VisitFormattedMoveSymbol(FormattedMoveSymbol symbol) => symbol.Ply.Notation;
        public override string VisitMoveCounterSymbol(MoveCounterSymbol symbol) => $"{symbol.Ply.PlyCount / 2 + 1}.";
        public override string VisitSideLineEndSymbol(SideLineEndSymbol symbol) => SideLineEndSymbol.SideLineEndText;
        public override string VisitSideLineStartSymbol(SideLineStartSymbol symbol) => SideLineStartSymbol.SideLineStartText;
        public override string VisitSpaceSymbol(SpaceSymbol symbol) => SpaceSymbol.SpaceText;
    }

    public class PgnMoveSearcher : PgnTerminalSymbolVisitor<bool>
    {
        private readonly Chess.MoveTree needle;
        public PgnMoveSearcher(Chess.MoveTree needle) { this.needle = needle; }
        public override bool VisitFormattedMoveSymbol(FormattedMoveSymbol symbol) => symbol.Ply.Variation.MoveTree == needle;
    }

    public class PgnActivePlyDetector : PgnTerminalSymbolVisitor<PgnPly>
    {
        public override PgnPly VisitBlackToMoveEllipsisSymbol(BlackToMoveEllipsisSymbol symbol) => symbol.Ply;
        public override PgnPly VisitFormattedMoveSymbol(FormattedMoveSymbol symbol) => symbol.Ply;
        public override PgnPly VisitMoveCounterSymbol(MoveCounterSymbol symbol) => symbol.Ply;
        public override PgnPly VisitSideLineEndSymbol(SideLineEndSymbol symbol) => symbol.SideLine.Plies[symbol.SideLine.Plies.Count - 1].Ply;
        public override PgnPly VisitSideLineStartSymbol(SideLineStartSymbol symbol) => symbol.SideLine.Plies[0].Ply;
    }
}

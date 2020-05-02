#region License
/*********************************************************************************
 * RootPgnSyntax.cs
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
using Eutherion.Text;
using Eutherion.Utils;
using Sandra.Chess.Pgn.Temp;
using System;
using System.Collections.Generic;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Contains the syntax tree and list of parse errors which are the result of parsing PGN.
    /// </summary>
    public sealed class RootPgnSyntax
    {
        public PgnSyntaxNodes Syntax { get; }
        public List<PgnErrorInfo> Errors { get; }

        public RootPgnSyntax(IEnumerable<GreenPgnForegroundSyntax> syntax, IEnumerable<GreenPgnTriviaElementSyntax> trailingTrivia, IEnumerable<GreenPgnBackgroundSyntax> backgroundAfter, List<PgnErrorInfo> errors)
        {
            if (syntax == null) throw new ArgumentNullException(nameof(syntax));
            if (trailingTrivia == null) throw new ArgumentNullException(nameof(trailingTrivia));
            if (backgroundAfter == null) throw new ArgumentNullException(nameof(backgroundAfter));

            List<TempGreenPgnForegroundSyntax> flattenedSyntax = new List<TempGreenPgnForegroundSyntax>();
            foreach (GreenPgnForegroundSyntax foreground in syntax)
            {
                foreach (GreenPgnTriviaElementSyntax leading in foreground.LeadingTrivia)
                {
                    flattenedSyntax.Add(new TempGreenPgnForegroundSyntax(leading.BackgroundBefore, leading.CommentNode));
                }
                flattenedSyntax.Add(new TempGreenPgnForegroundSyntax(foreground.BackgroundBefore, foreground.ForegroundNode));
            }
            foreach (GreenPgnTriviaElementSyntax trailing in trailingTrivia)
            {
                flattenedSyntax.Add(new TempGreenPgnForegroundSyntax(trailing.BackgroundBefore, trailing.CommentNode));
            }

            Syntax = new PgnSyntaxNodes(ReadOnlySpanList<TempGreenPgnForegroundSyntax>.Create(flattenedSyntax), ReadOnlySpanList<GreenPgnBackgroundSyntax>.Create(backgroundAfter));
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }
    }
}

namespace Sandra.Chess.Pgn.Temp
{
    public class GreenPgnForegroundSyntax : ISpan
    {
        public ReadOnlySpanList<GreenPgnTriviaElementSyntax> LeadingTrivia { get; }
        public ReadOnlySpanList<GreenPgnBackgroundSyntax> BackgroundBefore { get; }
        public IGreenPgnSymbol ForegroundNode { get; }

        public int Length => LeadingTrivia.Length + BackgroundBefore.Length + ForegroundNode.Length;

        public GreenPgnForegroundSyntax(IEnumerable<GreenPgnTriviaElementSyntax> leadingTrivia, IEnumerable<GreenPgnBackgroundSyntax> backgroundBefore, IGreenPgnSymbol foreground)
        {
            LeadingTrivia = ReadOnlySpanList<GreenPgnTriviaElementSyntax>.Create(leadingTrivia);
            BackgroundBefore = ReadOnlySpanList<GreenPgnBackgroundSyntax>.Create(backgroundBefore);
            ForegroundNode = foreground;
        }
    }

    public class TempGreenPgnForegroundSyntax : ISpan
    {
        public ReadOnlySpanList<GreenPgnBackgroundSyntax> BackgroundBefore { get; }
        public IGreenPgnSymbol ForegroundNode { get; }

        public int Length => BackgroundBefore.Length + ForegroundNode.Length;

        public TempGreenPgnForegroundSyntax(IEnumerable<GreenPgnBackgroundSyntax> backgroundBefore, IGreenPgnSymbol foreground)
        {
            BackgroundBefore = ReadOnlySpanList<GreenPgnBackgroundSyntax>.Create(backgroundBefore);
            ForegroundNode = foreground;
        }
    }

    public class PgnSyntaxNodes : PgnSyntax
    {
        public ReadOnlySpanList<TempGreenPgnForegroundSyntax> GreenForegroundNodes { get; }
        public ReadOnlySpanList<GreenPgnBackgroundSyntax> GreenBackgroundAfter { get; }

        public SafeLazyObjectCollection<PgnSymbolWithTrivia> ForegroundNodes { get; }

        private readonly SafeLazyObject<PgnBackgroundListSyntax> backgroundAfter;
        public PgnBackgroundListSyntax BackgroundAfter => backgroundAfter.Object;

        public override int Start => 0;
        public override int Length => GreenForegroundNodes.Length + GreenBackgroundAfter.Length;
        public override PgnSyntax ParentSyntax => null;
        public override int AbsoluteStart => 0;
        public override int ChildCount => ForegroundNodes.Count + 1;

        public override PgnSyntax GetChild(int index)
        {
            if (index < ForegroundNodes.Count) return ForegroundNodes[index];
            if (index == ForegroundNodes.Count) return BackgroundAfter;
            throw new IndexOutOfRangeException();
        }

        public override int GetChildStartPosition(int index)
        {
            if (index < ForegroundNodes.Count) return GreenForegroundNodes.GetElementOffset(index);
            if (index == ForegroundNodes.Count) return GreenForegroundNodes.Length;
            throw new IndexOutOfRangeException();
        }

        internal PgnSyntaxNodes(ReadOnlySpanList<TempGreenPgnForegroundSyntax> greenForegroundNodes,
                                ReadOnlySpanList<GreenPgnBackgroundSyntax> greenBackgroundAfter)
        {
            GreenForegroundNodes = greenForegroundNodes;
            GreenBackgroundAfter = greenBackgroundAfter;

            ForegroundNodes = new SafeLazyObjectCollection<PgnSymbolWithTrivia>(
                greenForegroundNodes.Count,
                index => new PgnSymbolWithTrivia(this, index, GreenForegroundNodes[index]));

            backgroundAfter = new SafeLazyObject<PgnBackgroundListSyntax>(
                () => new PgnBackgroundListSyntax(this, greenBackgroundAfter));
        }
    }
}

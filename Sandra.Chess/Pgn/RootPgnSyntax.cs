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

        public RootPgnSyntax(IEnumerable<GreenPgnForegroundSyntax> syntax, IEnumerable<GreenPgnBackgroundSyntax> backgroundAfter, List<PgnErrorInfo> errors)
        {
            if (syntax == null) throw new ArgumentNullException(nameof(syntax));
            if (backgroundAfter == null) throw new ArgumentNullException(nameof(backgroundAfter));

            Syntax = new PgnSyntaxNodes(ReadOnlySpanList<GreenPgnForegroundSyntax>.Create(syntax), ReadOnlySpanList<GreenPgnBackgroundSyntax>.Create(backgroundAfter));
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }
    }
}

namespace Sandra.Chess.Pgn.Temp
{
    public class GreenPgnForegroundSyntax : ISpan
    {
        public ReadOnlySpanList<GreenPgnBackgroundSyntax> BackgroundBefore { get; }
        public IGreenPgnSymbol ForegroundNode { get; }

        public int Length => BackgroundBefore.Length + ForegroundNode.Length;

        public GreenPgnForegroundSyntax(IEnumerable<GreenPgnBackgroundSyntax> backgroundBefore, IGreenPgnSymbol foreground)
        {
            BackgroundBefore = ReadOnlySpanList<GreenPgnBackgroundSyntax>.Create(backgroundBefore);
            ForegroundNode = foreground;
        }
    }

    public class PgnSyntaxNodes : PgnSyntax
    {
        public ReadOnlySpanList<GreenPgnForegroundSyntax> GreenForegroundNodes { get; }
        public ReadOnlySpanList<GreenPgnBackgroundSyntax> GreenBackgroundAfter { get; }

        public SafeLazyObjectCollection<PgnSyntax> ForegroundNodes { get; }

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
            int greenIndex = index >> 1;

            if (index == ChildCount - 1)
            {
                // Background after.
                return GreenForegroundNodes.Length;
            }

            GreenPgnForegroundSyntax green = GreenForegroundNodes[greenIndex];

            if ((index & 1) == 0)
            {
                // 0, 2, ...
                // Some background before.
                return GreenForegroundNodes.GetElementOffset(greenIndex);
            }

            // 1, 3, ...
            // Foreground node.
            return GreenForegroundNodes.GetElementOffset(greenIndex) + green.BackgroundBefore.Length;
        }

        private PgnSyntax CreateChildNode(int index)
        {
            int greenIndex = index >> 1;

            GreenPgnForegroundSyntax green = GreenForegroundNodes[greenIndex];

            if ((index & 1) == 0)
            {
                // 0, 2, ...
                // Some background before.
                return new PgnBackgroundListSyntax(this, greenIndex, green.BackgroundBefore);
            }

            // 1, 3, ...
            // Foreground node.
            return new PgnSymbol(this, greenIndex, green.ForegroundNode);
        }

        internal PgnSyntaxNodes(ReadOnlySpanList<GreenPgnForegroundSyntax> greenForegroundNodes,
                                ReadOnlySpanList<GreenPgnBackgroundSyntax> greenBackgroundAfter)
        {
            GreenForegroundNodes = greenForegroundNodes;
            GreenBackgroundAfter = greenBackgroundAfter;

            ForegroundNodes = new SafeLazyObjectCollection<PgnSyntax>(
                greenForegroundNodes.Count * 2,
                index => CreateChildNode(index));

            backgroundAfter = new SafeLazyObject<PgnBackgroundListSyntax>(
                () => new PgnBackgroundListSyntax(this, greenForegroundNodes.Count, greenBackgroundAfter));
        }
    }
}

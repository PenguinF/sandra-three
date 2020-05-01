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

        public RootPgnSyntax(IEnumerable<GreenPgnForegroundSyntax> syntax, ReadOnlySpanList<GreenPgnBackgroundSyntax> backgroundAfter, List<PgnErrorInfo> errors)
        {
            if (syntax == null) throw new ArgumentNullException(nameof(syntax));
            if (backgroundAfter == null) throw new ArgumentNullException(nameof(backgroundAfter));

            Syntax = new PgnSyntaxNodes(ReadOnlySpanList<GreenPgnForegroundSyntax>.Create(syntax), backgroundAfter);
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

        public GreenPgnForegroundSyntax(ReadOnlySpanList<GreenPgnBackgroundSyntax> backgroundBefore, IGreenPgnSymbol foreground)
        {
            BackgroundBefore = backgroundBefore;
            ForegroundNode = foreground;
        }
    }

    public class PgnSyntaxNodes : PgnSyntax
    {
        public ReadOnlySpanList<GreenPgnForegroundSyntax> Green { get; }
        public ReadOnlySpanList<GreenPgnBackgroundSyntax> BackgroundAfter { get; }
        public SafeLazyObjectCollection<PgnSyntax> ChildNodes { get; }
        public override int Start => 0;
        public override int Length => Green.Length + BackgroundAfter.Length;
        public override PgnSyntax ParentSyntax => null;
        public override int AbsoluteStart => 0;
        public override int ChildCount => ChildNodes.Count;
        public override PgnSyntax GetChild(int index) => ChildNodes[index];

        public override int GetChildStartPosition(int index)
        {
            int greenIndex = index >> 1;

            if (index == ChildCount - 1)
            {
                // Background after.
                return Green.Length;
            }

            GreenPgnForegroundSyntax green = Green[greenIndex];

            if ((index & 1) == 0)
            {
                // 0, 2, ...
                // Some background before.
                return Green.GetElementOffset(greenIndex);
            }

            // 1, 3, ...
            // Foreground node.
            return Green.GetElementOffset(greenIndex) + green.BackgroundBefore.Length;
        }

        private PgnSyntax CreateChildNode(int index)
        {
            int greenIndex = index >> 1;

            if (index == ChildCount - 1)
            {
                // Background after.
                return new PgnBackgroundListSyntax(this, greenIndex, BackgroundAfter);
            }

            GreenPgnForegroundSyntax green = Green[greenIndex];

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

        internal PgnSyntaxNodes(ReadOnlySpanList<GreenPgnForegroundSyntax> green, ReadOnlySpanList<GreenPgnBackgroundSyntax> backgroundAfter)
        {
            Green = green;
            BackgroundAfter = backgroundAfter;

            ChildNodes = new SafeLazyObjectCollection<PgnSyntax>(
                green.Count * 2 + 1,
                index => CreateChildNode(index));
        }
    }
}

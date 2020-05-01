﻿#region License
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

        public RootPgnSyntax(IEnumerable<IGreenPgnSymbol> syntax, List<PgnErrorInfo> errors)
        {
            if (syntax == null) throw new ArgumentNullException(nameof(syntax));
            Syntax = new PgnSyntaxNodes(ReadOnlySpanList<IGreenPgnSymbol>.Create(syntax));
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }
    }

    // Temporary placeholder
    public class PgnSyntaxNodes : PgnSyntax
    {
        private class PgnBackgroundSyntaxCreator : GreenPgnBackgroundSyntaxVisitor<(PgnSyntaxNodes, int), PgnSyntax>
        {
            public static readonly PgnBackgroundSyntaxCreator Instance = new PgnBackgroundSyntaxCreator();

            private PgnBackgroundSyntaxCreator() { }

            public override PgnSyntax VisitEscapeSyntax(GreenPgnEscapeSyntax green, (PgnSyntaxNodes, int) parent)
                => new PgnEscapeSyntax(parent.Item1, parent.Item2, green);

            public override PgnSyntax VisitIllegalCharacterSyntax(GreenPgnIllegalCharacterSyntax green, (PgnSyntaxNodes, int) parent)
                => new PgnIllegalCharacterSyntax(parent.Item1, parent.Item2, green);

            public override PgnSyntax VisitWhitespaceSyntax(GreenPgnWhitespaceSyntax green, (PgnSyntaxNodes, int) parent)
                => new PgnWhitespaceSyntax(parent.Item1, parent.Item2, green);
        }

        public ReadOnlySpanList<IGreenPgnSymbol> Green { get; }
        public SafeLazyObjectCollection<PgnSyntax> ChildNodes { get; }
        public override int Start => 0;
        public override int Length => Green.Length;
        public override PgnSyntax ParentSyntax => null;
        public override int AbsoluteStart => 0;
        public override int ChildCount => ChildNodes.Count;
        public override PgnSyntax GetChild(int index) => ChildNodes[index];
        public override int GetChildStartPosition(int index) => Green.GetElementOffset(index);

        private PgnSyntax CreateChildNode(IGreenPgnSymbol green, int index)
        {
            if (green.SymbolType.IsBackground())
            {
                return PgnBackgroundSyntaxCreator.Instance.Visit((GreenPgnBackgroundSyntax)green, (this, index));
            }
            else
            {
                return new PgnSymbol(this, index, green);
            }
        }

        internal PgnSyntaxNodes(ReadOnlySpanList<IGreenPgnSymbol> green)
        {
            Green = green;

            ChildNodes = new SafeLazyObjectCollection<PgnSyntax>(
                green.Count,
                index => CreateChildNode(green[index], index));
        }
    }
}

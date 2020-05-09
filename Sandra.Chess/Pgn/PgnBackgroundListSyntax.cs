#region License
/*********************************************************************************
 * PgnBackgroundListSyntax.cs
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

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a node with background symbols in an abstract PGN syntax tree.
    /// </summary>
    public sealed class PgnBackgroundListSyntax : PgnSyntax
    {
        private class PgnBackgroundSyntaxCreator : GreenPgnBackgroundSyntaxVisitor<(PgnBackgroundListSyntax, int), PgnBackgroundSyntax>
        {
            public static readonly PgnBackgroundSyntaxCreator Instance = new PgnBackgroundSyntaxCreator();

            private PgnBackgroundSyntaxCreator() { }

            public override PgnBackgroundSyntax VisitEscapeSyntax(GreenPgnEscapeSyntax green, (PgnBackgroundListSyntax, int) parent)
                => new PgnEscapeSyntax(parent.Item1, parent.Item2, green);

            public override PgnBackgroundSyntax VisitIllegalCharacterSyntax(GreenPgnIllegalCharacterSyntax green, (PgnBackgroundListSyntax, int) parent)
                => new PgnIllegalCharacterSyntax(parent.Item1, parent.Item2, green);

            public override PgnBackgroundSyntax VisitWhitespaceSyntax(GreenPgnWhitespaceSyntax green, (PgnBackgroundListSyntax, int) parent)
                => new PgnWhitespaceSyntax(parent.Item1, parent.Item2, green);
        }

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public Union<PgnTriviaElementSyntax, PgnTriviaSyntax> Parent { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' read-only list with background nodes.
        /// </summary>
        public ReadOnlySpanList<IGreenPgnSymbol> Green { get; }

        /// <summary>
        /// Gets the collection of background nodes.
        /// </summary>
        public SafeLazyObjectCollection<PgnBackgroundSyntax> BackgroundNodes { get; }

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Match(whenOption1: _ => 0, whenOption2: x => x.Length - Green.Length);

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override PgnSyntax ParentSyntax => Parent.Match<PgnSyntax>(whenOption1: x => x, whenOption2: x => x);

        /// <summary>
        /// Gets the number of children of this syntax node.
        /// </summary>
        public override int ChildCount => BackgroundNodes.Count;

        /// <summary>
        /// Initializes the child at the given <paramref name="index"/> and returns it.
        /// </summary>
        public override PgnSyntax GetChild(int index) => BackgroundNodes[index];

        /// <summary>
        /// Gets the start position of the child at the given <paramref name="index"/>, without initializing it.
        /// </summary>
        public override int GetChildStartPosition(int index) => Green.GetElementOffset(index);

        internal PgnBackgroundListSyntax(Union<PgnTriviaElementSyntax, PgnTriviaSyntax> parent, ReadOnlySpanList<IGreenPgnSymbol> green)
        {
            Parent = parent;
            Green = green;

            BackgroundNodes = new SafeLazyObjectCollection<PgnBackgroundSyntax>(
                green.Count,
                index => PgnBackgroundSyntaxCreator.Instance.Visit((GreenPgnBackgroundSyntax)Green[index], (this, index)));
        }
    }
}

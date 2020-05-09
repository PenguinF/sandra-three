#region License
/*********************************************************************************
 * PgnTagSectionSyntax.cs
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
    /// Represents a syntax node which contains a collection of <see cref="GreenPgnTagPairSyntax"/> instances.
    /// </summary>
    public sealed class GreenPgnTagSectionSyntax : IGreenPgnTopLevelSyntax, ISpan
    {
        /// <summary>
        /// Gets the empty <see cref="GreenPgnTagSectionSyntax"/>.
        /// </summary>
        public static readonly GreenPgnTagSectionSyntax Empty = new GreenPgnTagSectionSyntax(ReadOnlySpanList<GreenPgnTagPairSyntax>.Empty);

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnTagSectionSyntax"/>.
        /// </summary>
        /// <param name="tagPairNodes">
        /// The tag pair nodes.
        /// </param>
        /// <returns>
        /// The new <see cref="GreenPgnTagSectionSyntax"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="tagPairNodes"/> is null.
        /// </exception>
        public static GreenPgnTagSectionSyntax Create(IEnumerable<GreenPgnTagPairSyntax> tagPairNodes)
        {
            if (tagPairNodes == null) throw new ArgumentNullException(nameof(tagPairNodes));

            var tagPairNodeSpanList = ReadOnlySpanList<GreenPgnTagPairSyntax>.Create(tagPairNodes);

            return tagPairNodeSpanList.Count == 0
               ? Empty
               : new GreenPgnTagSectionSyntax(tagPairNodeSpanList);
        }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length => TagPairNodes.Length;

        /// <summary>
        /// Gets the tag pair nodes.
        /// </summary>
        public ReadOnlySpanList<GreenPgnTagPairSyntax> TagPairNodes { get; }

        private GreenPgnTagSectionSyntax(ReadOnlySpanList<GreenPgnTagPairSyntax> tagPairNodes) => TagPairNodes = tagPairNodes;
    }

    /// <summary>
    /// Represents a syntax node which contains a collection of <see cref="PgnTagPairSyntax"/> instances.
    /// </summary>
    public sealed class PgnTagSectionSyntax : PgnSyntax, IPgnTopLevelSyntax
    {
        PgnSyntax IPgnTopLevelSyntax.ToPgnSyntax() => this;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public PgnSyntaxNodes Parent { get; }

        /// <summary>
        /// Gets the index of this syntax node in its parent.
        /// </summary>
        public int ParentIndex { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnTagSectionSyntax Green { get; }

        /// <summary>
        /// Gets the collection of tag pair nodes.
        /// </summary>
        public SafeLazyObjectCollection<PgnTagPairSyntax> TagPairNodes { get; }

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.GreenTopLevelNodes.GetElementOffset(ParentIndex);

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public override int Length => Green.Length;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override PgnSyntax ParentSyntax => Parent;

        /// <summary>
        /// Gets the number of children of this syntax node.
        /// </summary>
        public override int ChildCount => TagPairNodes.Count;

        /// <summary>
        /// Initializes the child at the given <paramref name="index"/> and returns it.
        /// </summary>
        public override PgnSyntax GetChild(int index) => TagPairNodes[index];

        /// <summary>
        /// Gets the start position of the child at the given <paramref name="index"/>, without initializing it.
        /// </summary>
        public override int GetChildStartPosition(int index) => Green.TagPairNodes.GetElementOffset(index);

        internal PgnTagSectionSyntax(PgnSyntaxNodes parent, int parentIndex, GreenPgnTagSectionSyntax green)
        {
            Parent = parent;
            ParentIndex = parentIndex;
            Green = green;

            TagPairNodes = new SafeLazyObjectCollection<PgnTagPairSyntax>(
                green.TagPairNodes.Count,
                index => new PgnTagPairSyntax(this, index, Green.TagPairNodes[index]));
        }
    }
}

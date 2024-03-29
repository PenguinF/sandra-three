﻿#region License
/*********************************************************************************
 * PgnTagPairSyntax.cs
 *
 * Copyright (c) 2004-2023 Henk Nicolai
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

using Eutherion.Collections;
using Eutherion.Text;
using System;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a syntax node which contains a tag name and a tag value.
    /// </summary>
    public sealed class GreenPgnTagPairSyntax : ISpan
    {
        /// <summary>
        /// Gets the tag element nodes.
        /// </summary>
        public ReadOnlySpanList<GreenWithTriviaSyntax> TagElementNodes { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length => TagElementNodes.Length;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnTagPairSyntax"/>.
        /// </summary>
        /// <param name="tagElementNodes">
        /// The tag element nodes.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="tagElementNodes"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="tagElementNodes"/> is empty.
        /// </exception>
        public GreenPgnTagPairSyntax(ReadOnlySpanList<GreenWithTriviaSyntax> tagElementNodes)
        {
            TagElementNodes = tagElementNodes ?? throw new ArgumentNullException(nameof(tagElementNodes));
            if (tagElementNodes.Count == 0) throw new ArgumentException($"{nameof(tagElementNodes)} is empty", nameof(tagElementNodes));
        }
    }

    /// <summary>
    /// Represents a syntax node which contains a tag name and a tag value.
    /// </summary>
    public sealed class PgnTagPairSyntax : PgnSyntax
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public PgnTagSectionSyntax Parent { get; }

        /// <summary>
        /// Gets the index of this syntax node in its parent.
        /// </summary>
        public int ParentIndex { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnTagPairSyntax Green { get; }

        /// <summary>
        /// Gets the collection of tag element nodes.
        /// </summary>
        public SafeLazyObjectCollection<PgnTagElementWithTriviaSyntax> TagElementNodes { get; }

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Green.TagPairNodes.GetElementOffset(ParentIndex);

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
        public override int ChildCount => TagElementNodes.Count;

        /// <summary>
        /// Initializes the child at the given <paramref name="index"/> and returns it.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than 0 or greater than or equal to <see cref="ChildCount"/>.
        /// </exception>
        public override PgnSyntax GetChild(int index) => TagElementNodes[index];

        /// <summary>
        /// Gets the start position of the child at the given <paramref name="index"/>, without initializing it.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than 0 or greater than or equal to <see cref="ChildCount"/>.
        /// </exception>
        public override int GetChildStartPosition(int index) => Green.TagElementNodes.GetElementOffset(index);

        internal PgnTagPairSyntax(PgnTagSectionSyntax parent, int parentIndex, GreenPgnTagPairSyntax green)
        {
            Parent = parent;
            ParentIndex = parentIndex;
            Green = green;

            TagElementNodes = new SafeLazyObjectCollection<PgnTagElementWithTriviaSyntax>(
                green.TagElementNodes.Count,
                index => new PgnTagElementWithTriviaSyntax(this, index, Green.TagElementNodes[index]));
        }
    }
}

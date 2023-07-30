#region License
/*********************************************************************************
 * PgnPlySyntax.cs
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
using System.Collections.Generic;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a syntax node which contains a single ply (half-move).
    /// </summary>
    public sealed class GreenPgnPlySyntax : ISpan
    {
        /// <summary>
        /// Gets the move number. The move number can be null.
        /// </summary>
        public GreenWithPlyFloatItemsSyntax<GreenWithTriviaSyntax> MoveNumber { get; }

        /// <summary>
        /// Returns the move number's length or 0 if it does not exist.
        /// </summary>
        public int MoveNumberLength => MoveNumber == null ? 0 : MoveNumber.Length;

        /// <summary>
        /// Gets the move. The move can be null.
        /// </summary>
        public GreenWithPlyFloatItemsSyntax<GreenWithTriviaSyntax> Move { get; }

        /// <summary>
        /// Returns the move's length or 0 if it does not exist.
        /// </summary>
        public int MoveLength => Move == null ? 0 : Move.Length;

        /// <summary>
        /// Gets the NAG (Numeric Annotation Glyph) nodes.
        /// </summary>
        public ReadOnlySpanList<GreenWithPlyFloatItemsSyntax<GreenWithTriviaSyntax>> Nags { get; }

        /// <summary>
        /// Gets the variation nodes.
        /// </summary>
        public ReadOnlySpanList<GreenWithPlyFloatItemsSyntax<GreenPgnVariationSyntax>> Variations { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnPlySyntax"/>.
        /// </summary>
        /// <param name="moveNumber">
        /// The move number. This is an optional parameter.
        /// </param>
        /// <param name="move">
        /// The move. This is an optional parameter.
        /// </param>
        /// <param name="nags">
        /// The NAG (Numeric Annotation Glyph) nodes.
        /// </param>
        /// <param name="variations">
        /// The variation nodes.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="nags"/> and/or <paramref name="variations"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="moveNumber"/> is null, <paramref name="move"/> is null, and both <paramref name="nags"/> and <paramref name="variations"/> are empty.
        /// </exception>
        public GreenPgnPlySyntax(
            GreenWithPlyFloatItemsSyntax<GreenWithTriviaSyntax> moveNumber,
            GreenWithPlyFloatItemsSyntax<GreenWithTriviaSyntax> move,
            IEnumerable<GreenWithPlyFloatItemsSyntax<GreenWithTriviaSyntax>> nags,
            IEnumerable<GreenWithPlyFloatItemsSyntax<GreenPgnVariationSyntax>> variations)
        {
            if (nags == null) throw new ArgumentNullException(nameof(nags));
            if (variations == null) throw new ArgumentNullException(nameof(variations));

            MoveNumber = moveNumber;
            Move = move;
            Nags = ReadOnlySpanList<GreenWithPlyFloatItemsSyntax<GreenWithTriviaSyntax>>.Create(nags);
            Variations = ReadOnlySpanList<GreenWithPlyFloatItemsSyntax<GreenPgnVariationSyntax>>.Create(variations);

            int length = Nags.Length + Variations.Length;
            if (moveNumber != null) length += moveNumber.Length;
            if (move != null) length += move.Length;

            if (length == 0)
            {
                throw new ArgumentException($"{nameof(GreenPgnPlySyntax)} cannot be empty.");
            }

            Length = length;
        }
    }

    /// <summary>
    /// Represents a syntax node which contains a single ply (half-move).
    /// </summary>
    public sealed class PgnPlySyntax : PgnSyntax
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public PgnPlyListSyntax Parent { get; }

        /// <summary>
        /// Gets the index of this syntax node in its parent.
        /// </summary>
        public int ParentIndex { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnPlySyntax Green { get; }

        private readonly SafeLazyChildSyntaxOrEmpty<PgnMoveNumberWithFloatItemsSyntax> lazyMoveNumberOrEmpty;

        /// <summary>
        /// Gets the move number. The move number can be null.
        /// </summary>
        public PgnMoveNumberWithFloatItemsSyntax MoveNumber => lazyMoveNumberOrEmpty.ChildNodeOrNull;

        /// <summary>
        /// Gets the move number. The move number can be <see cref="PgnEmptySyntax"/>.
        /// </summary>
        public PgnSyntax MoveNumberOrEmpty => lazyMoveNumberOrEmpty.ChildNodeOrEmpty;

        private readonly SafeLazyChildSyntaxOrEmpty<PgnMoveWithFloatItemsSyntax> lazyMoveOrEmpty;

        /// <summary>
        /// Gets the move. The move can be null.
        /// </summary>
        public PgnMoveWithFloatItemsSyntax Move => lazyMoveOrEmpty.ChildNodeOrNull;

        /// <summary>
        /// Gets the move number. The move number can be <see cref="PgnEmptySyntax"/>.
        /// </summary>
        public PgnSyntax MoveOrEmpty => lazyMoveOrEmpty.ChildNodeOrEmpty;

        /// <summary>
        /// Gets the collection of NAG (Numeric Annotation Glyph) nodes.
        /// </summary>
        public SafeLazyObjectCollection<PgnNagWithFloatItemsSyntax> Nags { get; }

        /// <summary>
        /// Gets the collection of variation nodes.
        /// </summary>
        public SafeLazyObjectCollection<PgnVariationWithFloatItemsSyntax> Variations { get; }

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Green.Plies.GetElementOffset(ParentIndex);

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override PgnSyntax ParentSyntax => Parent;

        /// <summary>
        /// Gets the number of children of this syntax node.
        /// </summary>
        public override int ChildCount => 2 + Nags.Count + Variations.Count;

        /// <summary>
        /// Initializes the child at the given <paramref name="index"/> and returns it.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than 0 or greater than or equal to <see cref="ChildCount"/>.
        /// </exception>
        public override PgnSyntax GetChild(int index)
        {
            if (index == 0) return MoveNumberOrEmpty;
            index--;
            if (index == 0) return MoveOrEmpty;
            index--;
            int variationIndex = index - Nags.Count;
            if (variationIndex < 0) return Nags[index];
            return Variations[variationIndex];
        }

        /// <summary>
        /// Gets the start position of the child at the given <paramref name="index"/>, without initializing it.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than 0 or greater than or equal to <see cref="ChildCount"/>.
        /// </exception>
        public override int GetChildStartPosition(int index)
        {
            if (index == 0) return 0;
            index--;
            if (index == 0) return Green.MoveNumberLength;
            index--;
            int variationIndex = index - Nags.Count;
            if (variationIndex < 0) return Length - Green.Variations.Length - Green.Nags.Length + Green.Nags.GetElementOffset(index);
            return Length - Green.Variations.Length + Green.Variations.GetElementOffset(variationIndex);
        }

        internal PgnPlySyntax(PgnPlyListSyntax parent, int parentIndex, GreenPgnPlySyntax green)
        {
            Parent = parent;
            ParentIndex = parentIndex;
            Green = green;

            int length = 0;
            if (green.MoveNumber != null)
            {
                lazyMoveNumberOrEmpty = new SafeLazyChildSyntaxOrEmpty<PgnMoveNumberWithFloatItemsSyntax>(() => new PgnMoveNumberWithFloatItemsSyntax(this, Green.MoveNumber));
                length += green.MoveNumber.Length;
            }
            else
            {
                lazyMoveNumberOrEmpty = new SafeLazyChildSyntaxOrEmpty<PgnMoveNumberWithFloatItemsSyntax>(this, length);
            }

            if (green.Move != null)
            {
                lazyMoveOrEmpty = new SafeLazyChildSyntaxOrEmpty<PgnMoveWithFloatItemsSyntax>(() => new PgnMoveWithFloatItemsSyntax(this, Green.Move));
                length += green.Move.Length;
            }
            else
            {
                lazyMoveOrEmpty = new SafeLazyChildSyntaxOrEmpty<PgnMoveWithFloatItemsSyntax>(this, length);
            }

            Nags = new SafeLazyObjectCollection<PgnNagWithFloatItemsSyntax>(
                green.Nags.Count,
                index => new PgnNagWithFloatItemsSyntax(this, index, Green.Nags[index]));

            Variations = new SafeLazyObjectCollection<PgnVariationWithFloatItemsSyntax>(
                green.Variations.Count,
                index => new PgnVariationWithFloatItemsSyntax(this, index, Green.Variations[index]));
        }
    }

    /// <summary>
    /// Represents a syntax node which contains a move number, together with its leading floating items.
    /// </summary>
    public sealed class PgnMoveNumberWithFloatItemsSyntax : WithPlyFloatItemsSyntax<GreenWithTriviaSyntax, PgnMoveNumberWithTriviaSyntax>
    {
        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => 0;

        internal override PgnMoveNumberWithTriviaSyntax CreatePlyContentNode() => new PgnMoveNumberWithTriviaSyntax(this, Green.PlyContentNode);

        internal PgnMoveNumberWithFloatItemsSyntax(PgnPlySyntax parent, GreenWithPlyFloatItemsSyntax<GreenWithTriviaSyntax> green)
            : base(parent, green)
        {
        }
    }

    /// <summary>
    /// Represents a syntax node which contains a move text, together with its leading floating items.
    /// </summary>
    public sealed class PgnMoveWithFloatItemsSyntax : WithPlyFloatItemsSyntax<GreenWithTriviaSyntax, PgnMoveWithTriviaSyntax>
    {
        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Green.MoveNumberLength;

        internal override PgnMoveWithTriviaSyntax CreatePlyContentNode() => new PgnMoveWithTriviaSyntax(this, Green.PlyContentNode);

        internal PgnMoveWithFloatItemsSyntax(PgnPlySyntax parent, GreenWithPlyFloatItemsSyntax<GreenWithTriviaSyntax> green)
            : base(parent, green)
        {
        }
    }

    /// <summary>
    /// Represents a Numeric Annotation Glyph syntax node, together with its leading floating items.
    /// </summary>
    public sealed class PgnNagWithFloatItemsSyntax : WithPlyFloatItemsSyntax<GreenWithTriviaSyntax, PgnNagWithTriviaSyntax>
    {
        /// <summary>
        /// Gets the index of this syntax node in its parent.
        /// </summary>
        public int ParentIndex { get; }

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Length - Parent.Green.Variations.Length - Parent.Green.Nags.Length + Parent.Green.Nags.GetElementOffset(ParentIndex);

        internal override PgnNagWithTriviaSyntax CreatePlyContentNode() => new PgnNagWithTriviaSyntax(this, Green.PlyContentNode);

        internal PgnNagWithFloatItemsSyntax(PgnPlySyntax parent, int parentIndex, GreenWithPlyFloatItemsSyntax<GreenWithTriviaSyntax> green)
            : base(parent, green)
        {
            ParentIndex = parentIndex;
        }
    }

    /// <summary>
    /// Represents a side line syntax node, together with its leading floating items.
    /// </summary>
    public sealed class PgnVariationWithFloatItemsSyntax : WithPlyFloatItemsSyntax<GreenPgnVariationSyntax, PgnVariationSyntax>
    {
        /// <summary>
        /// Gets the index of this syntax node in its parent.
        /// </summary>
        public int ParentIndex { get; }

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Length - Parent.Green.Variations.Length + Parent.Green.Variations.GetElementOffset(ParentIndex);

        internal override PgnVariationSyntax CreatePlyContentNode() => new PgnVariationSyntax(this, Green.PlyContentNode);

        internal PgnVariationWithFloatItemsSyntax(PgnPlySyntax parent, int parentIndex, GreenWithPlyFloatItemsSyntax<GreenPgnVariationSyntax> green)
            : base(parent, green)
        {
            ParentIndex = parentIndex;
        }
    }
}

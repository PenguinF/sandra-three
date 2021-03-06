﻿#region License
/*********************************************************************************
 * PgnPlyListSyntax.cs
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
using System;
using System.Collections.Generic;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a syntax node which contains a list of plies (half-moves)
    /// together with its trailing floating items that are not part of a ply.
    /// </summary>
    public sealed class GreenPgnPlyListSyntax : ISpan
    {
        /// <summary>
        /// Gets the empty <see cref="GreenPgnPlyListSyntax"/>.
        /// </summary>
        public static readonly GreenPgnPlyListSyntax Empty = new GreenPgnPlyListSyntax(
            ReadOnlySpanList<GreenPgnPlySyntax>.Empty,
            ReadOnlySpanList<GreenWithTriviaSyntax>.Empty);

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnPlyListSyntax"/>.
        /// </summary>
        /// <param name="plies">
        /// The ply nodes.
        /// </param>
        /// <param name="trailingFloatItems">
        /// The nodes containing the trailing floating items that are not part of a ply.
        /// </param>
        /// <returns>
        /// The new <see cref="GreenPgnPlyListSyntax"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="plies"/> and/or <paramref name="trailingFloatItems"/> is null.
        /// </exception>
        public static GreenPgnPlyListSyntax Create(IEnumerable<GreenPgnPlySyntax> plies, IEnumerable<GreenWithTriviaSyntax> trailingFloatItems)
        {
            if (plies == null) throw new ArgumentNullException(nameof(plies));
            if (trailingFloatItems == null) throw new ArgumentNullException(nameof(trailingFloatItems));

            var plyList = ReadOnlySpanList<GreenPgnPlySyntax>.Create(plies);
            var trailingFloatItemList = ReadOnlySpanList<GreenWithTriviaSyntax>.Create(trailingFloatItems);

            return plyList.Count == 0 && trailingFloatItemList.Count == 0
                ? Empty
                : new GreenPgnPlyListSyntax(plyList, trailingFloatItemList);
        }

        /// <summary>
        /// Gets the ply nodes.
        /// </summary>
        public ReadOnlySpanList<GreenPgnPlySyntax> Plies { get; }

        /// <summary>
        /// Gets the nodes containing the trailing floating items that are not part of a ply.
        /// </summary>
        public ReadOnlySpanList<GreenWithTriviaSyntax> TrailingFloatItems { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length => Plies.Length + TrailingFloatItems.Length;

        private GreenPgnPlyListSyntax(ReadOnlySpanList<GreenPgnPlySyntax> plies, ReadOnlySpanList<GreenWithTriviaSyntax> trailingFloatItems)
        {
            Plies = plies;
            TrailingFloatItems = trailingFloatItems;
        }
    }

    /// <summary>
    /// Represents a syntax node which contains a list of plies (half-moves)
    /// together with its trailing floating items that are not part of a ply.
    /// </summary>
    public sealed class PgnPlyListSyntax : PgnSyntax
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public Union<PgnGameSyntax, PgnVariationSyntax> Parent { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnPlyListSyntax Green { get; }

        /// <summary>
        /// Gets the collection of ply nodes.
        /// </summary>
        public SafeLazyObjectCollection<PgnPlySyntax> Plies { get; }

        private readonly SafeLazyObject<PgnPlyFloatItemListSyntax> trailingFloatItems;

        /// <summary>
        /// Gets the collection of nodes containing the trailing floating items that are not part of a ply.
        /// </summary>
        public PgnPlyFloatItemListSyntax TrailingFloatItems => trailingFloatItems.Object;

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Match(whenOption1: x => x.Green.TagSection.Length, whenOption2: x => x.Green.ParenthesisOpen.Length);

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
        public override int ChildCount => Plies.Count + 1;

        /// <summary>
        /// Initializes the child at the given <paramref name="index"/> and returns it.
        /// </summary>
        public override PgnSyntax GetChild(int index)
        {
            if (index < Plies.Count) return Plies[index];
            if (index == Plies.Count) return TrailingFloatItems;
            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// Gets the start position of the child at the given <paramref name="index"/>, without initializing it.
        /// </summary>
        public override int GetChildStartPosition(int index)
        {
            if (index < Plies.Count) return Green.Plies.GetElementOffset(index);
            if (index == Plies.Count) return Green.Plies.Length;
            throw new IndexOutOfRangeException();
        }

        internal PgnPlyListSyntax(Union<PgnGameSyntax, PgnVariationSyntax> parent, GreenPgnPlyListSyntax green)
        {
            Parent = parent;
            Green = green;

            Plies = new SafeLazyObjectCollection<PgnPlySyntax>(
                green.Plies.Count,
                index => new PgnPlySyntax(this, index, Green.Plies[index]));

            trailingFloatItems = new SafeLazyObject<PgnPlyFloatItemListSyntax>(() => new PgnPlyFloatItemListSyntax(this, Green.TrailingFloatItems));
        }
    }
}

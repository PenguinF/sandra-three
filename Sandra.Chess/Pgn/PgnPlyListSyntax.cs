#region License
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

using Eutherion.Text;
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

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnPlyListSyntax"/>.
        /// </summary>
        /// <param name="plies">
        /// The ply nodes.
        /// </param>
        /// <param name="trailingFloatItems">
        /// The nodes containing the trailing floating items that are not part of a ply.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="plies"/> and/or <paramref name="trailingFloatItems"/> is null.
        /// </exception>
        public GreenPgnPlyListSyntax(IEnumerable<GreenPgnPlySyntax> plies, IEnumerable<GreenWithTriviaSyntax> trailingFloatItems)
        {
            if (plies == null) throw new ArgumentNullException(nameof(plies));
            if (trailingFloatItems == null) throw new ArgumentNullException(nameof(trailingFloatItems));

            Plies = ReadOnlySpanList<GreenPgnPlySyntax>.Create(plies);
            TrailingFloatItems = ReadOnlySpanList<GreenWithTriviaSyntax>.Create(trailingFloatItems);
        }
    }
}

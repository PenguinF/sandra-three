#region License
/*********************************************************************************
 * PgnPlySyntax.cs
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
    /// Represents a syntax node which contains a single ply (half-move).
    /// </summary>
    public sealed class GreenPgnPlySyntax
    {
        /// <summary>
        /// The move number. The move number can be null.
        /// </summary>
        public GreenWithPlyFloatItemsSyntax MoveNumber { get; }

        /// <summary>
        /// Returns the move number's length or 0 if it does not exist.
        /// </summary>
        public int MoveNumberLength => MoveNumber == null ? 0 : MoveNumber.Length;

        /// <summary>
        /// The move. The move can be null.
        /// </summary>
        public GreenWithPlyFloatItemsSyntax Move { get; }

        /// <summary>
        /// Returns the move's length or 0 if it does not exist.
        /// </summary>
        public int MoveLength => Move == null ? 0 : Move.Length;

        /// <summary>
        /// Gets the NAG (Numeric Annotation Glyph) nodes.
        /// </summary>
        public ReadOnlySpanList<GreenWithPlyFloatItemsSyntax> Nags { get; }

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
        /// <exception cref="ArgumentNullException">
        /// <paramref name="nags"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="moveNumber"/> is null, <paramref name="move"/> is null, and/or <paramref name="nags"/> is empty.
        /// </exception>
        public GreenPgnPlySyntax(GreenWithPlyFloatItemsSyntax moveNumber, GreenWithPlyFloatItemsSyntax move, IEnumerable<GreenWithPlyFloatItemsSyntax> nags)
        {
            if (nags == null) throw new ArgumentNullException(nameof(nags));

            MoveNumber = moveNumber;
            Move = move;

            Nags = ReadOnlySpanList<GreenWithPlyFloatItemsSyntax>.Create(nags);

            int length = Nags.Length;
            if (moveNumber != null) length += moveNumber.Length;
            if (move != null) length += move.Length;

            if (length == 0)
            {
                throw new ArgumentException($"{nameof(GreenPgnPlySyntax)} cannot be empty.");
            }

            Length = length;
        }
    }
}

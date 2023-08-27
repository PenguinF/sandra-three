#region License
/*********************************************************************************
 * PgnGameListSyntax.cs
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

using Eutherion;
using Eutherion.Collections;
using Eutherion.Text;
using Eutherion.Threading;
using System;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents the root of an abstract PGN syntax tree. It contains a list of games together with its trailing trivia.
    /// </summary>
    public sealed class GreenPgnGameListSyntax : ISpan
    {
        /// <summary>
        /// Gets the list of games.
        /// </summary>
        public ReadOnlySpanList<GreenPgnGameSyntax> Games { get; }

        /// <summary>
        /// Gets the trailing trivia of the syntax node.
        /// </summary>
        public GreenPgnTriviaSyntax TrailingTrivia { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length => Games.Length + TrailingTrivia.Length;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnGameListSyntax"/>.
        /// </summary>
        /// <param name="games">
        /// The list of games.
        /// </param>
        /// <param name="trailingTrivia">
        /// The trailing trivia of the syntax node.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="games"/> and/or <paramref name="trailingTrivia"/> is null.
        /// </exception>
        public GreenPgnGameListSyntax(ReadOnlySpanList<GreenPgnGameSyntax> games, GreenPgnTriviaSyntax trailingTrivia)
        {
            Games = games ?? throw new ArgumentNullException(nameof(games));
            TrailingTrivia = trailingTrivia ?? throw new ArgumentNullException(nameof(trailingTrivia));
        }
    }

    /// <summary>
    /// Represents the root of an abstract PGN syntax tree. It contains a list of games together with its trailing trivia.
    /// </summary>
    public sealed class PgnGameListSyntax : PgnSyntax
    {
        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnGameListSyntax Green { get; }

        /// <summary>
        /// Gets the collection of games.
        /// </summary>
        public SafeLazyObjectCollection<PgnGameSyntax> Games { get; }

        private readonly SafeLazyObject<PgnTriviaSyntax> trailingTrivia;

        /// <summary>
        /// Gets the trailing trivia of the syntax node.
        /// </summary>
        public PgnTriviaSyntax TrailingTrivia => trailingTrivia.Object;

        /// <summary>
        /// Returns 0, which is the default start position of the root node.
        /// </summary>
        public override int Start => 0;

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        /// <summary>
        /// Returns null, as this is the root node.
        /// </summary>
        public override PgnSyntax ParentSyntax => null;

        /// <summary>
        /// Returns 0, which is the absolute start position of this syntax node.
        /// </summary>
        public override int AbsoluteStart => 0;

        /// <summary>
        /// Gets the number of children of this syntax node.
        /// </summary>
        public override int ChildCount => Games.Count + 1;

        /// <summary>
        /// Initializes the child at the given <paramref name="index"/> and returns it.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than 0 or greater than or equal to <see cref="ChildCount"/>.
        /// </exception>
        public override PgnSyntax GetChild(int index)
        {
            if (index < Games.Count) return Games[index];
            if (index == Games.Count) return TrailingTrivia;
            throw ExceptionUtility.ThrowListIndexOutOfRangeException();
        }

        /// <summary>
        /// Gets the start position of the child at the given <paramref name="index"/>, without initializing it.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than 0 or greater than or equal to <see cref="ChildCount"/>.
        /// </exception>
        public override int GetChildStartPosition(int index)
        {
            if (index < Games.Count) return Green.Games.GetElementOffset(index);
            if (index == Games.Count) return Green.Games.Length;
            throw ExceptionUtility.ThrowListIndexOutOfRangeException();
        }

        internal PgnGameListSyntax(GreenPgnGameListSyntax green)
        {
            Green = green;

            Games = new SafeLazyObjectCollection<PgnGameSyntax>(
                green.Games.Count,
                index => new PgnGameSyntax(this, index, Green.Games[index]));

            trailingTrivia = new SafeLazyObject<PgnTriviaSyntax>(() => new PgnTriviaSyntax(this, Green.TrailingTrivia));
        }
    }
}

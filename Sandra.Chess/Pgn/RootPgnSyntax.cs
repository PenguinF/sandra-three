#region License
/*********************************************************************************
 * RootPgnSyntax.cs
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
using System;
using System.Collections.Generic;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Contains the syntax tree and list of parse errors which are the result of parsing PGN.
    /// </summary>
    public sealed class RootPgnSyntax : PgnSyntax
    {
        /// <summary>
        /// Gets the syntax tree containing the list of PGN games.
        /// </summary>
        public PgnGameListSyntax GameListSyntax { get; }

        /// <summary>
        /// Gets the collection of parse errors.
        /// </summary>
        public ReadOnlyList<PgnErrorInfo> Errors { get; }

        /// <summary>
        /// Returns 0, which is the default start position of the root node.
        /// </summary>
        public override int Start => 0;

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => GameListSyntax.Length;

        /// <summary>
        /// Returns <see langword="null"/>, as this is the root node.
        /// </summary>
        public override PgnSyntax ParentSyntax => null;

        /// <summary>
        /// Returns 0, which is the absolute start position of this syntax node.
        /// </summary>
        public override int AbsoluteStart => 0;

        /// <summary>
        /// Gets the number of children of this syntax node.
        /// </summary>
        public override int ChildCount => 1;

        /// <summary>
        /// Initializes the child at the given <paramref name="index"/> and returns it.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than 0 or greater than or equal to <see cref="ChildCount"/>.
        /// </exception>
        public override PgnSyntax GetChild(int index)
        {
            if (index == 0) return GameListSyntax;
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
            if (index == 0) return 0;
            throw ExceptionUtility.ThrowListIndexOutOfRangeException();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RootPgnSyntax"/>.
        /// </summary>
        /// <param name="gameListSyntax">
        /// The syntax tree containing a list of PGN games.
        /// </param>
        /// <param name="errors">
        /// The collection of parse errors.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="gameListSyntax"/> and/or <paramref name="errors"/> is null.
        /// </exception>
        public RootPgnSyntax(GreenPgnGameListSyntax gameListSyntax, ReadOnlyList<PgnErrorInfo> errors)
        {
            if (gameListSyntax == null) throw new ArgumentNullException(nameof(gameListSyntax));
            GameListSyntax = new PgnGameListSyntax(this, gameListSyntax);
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }
    }
}

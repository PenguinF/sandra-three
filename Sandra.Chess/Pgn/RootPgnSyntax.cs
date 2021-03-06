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

using System;
using System.Collections.Generic;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Contains the syntax tree and list of parse errors which are the result of parsing PGN.
    /// </summary>
    public sealed class RootPgnSyntax
    {
        /// <summary>
        /// Gets the syntax tree containing the list of PGN games.
        /// </summary>
        public PgnGameListSyntax GameListSyntax { get; }

        /// <summary>
        /// Gets the collection of parse errors.
        /// </summary>
        public List<PgnErrorInfo> Errors { get; }

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
        public RootPgnSyntax(GreenPgnGameListSyntax gameListSyntax, List<PgnErrorInfo> errors)
        {
            if (gameListSyntax == null) throw new ArgumentNullException(nameof(gameListSyntax));
            GameListSyntax = new PgnGameListSyntax(gameListSyntax);
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }
    }
}

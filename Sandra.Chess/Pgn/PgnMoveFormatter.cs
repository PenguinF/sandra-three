﻿#region License
/*********************************************************************************
 * PgnMoveFormatter.cs
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

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Move formatter which generates algebraic notation for PGN text.
    /// </summary>
    public class PgnMoveFormatter : ShortAlgebraicMoveFormatter
    {
        /// <summary>
        /// Standardized PGN notation for pieces.
        /// </summary>
        public static readonly string PieceSymbols = "NBRQK";

        /// <summary>
        /// Returns the single <see cref="PgnMoveFormatter"/> instance.
        /// </summary>
        public static PgnMoveFormatter Instance { get; } = new PgnMoveFormatter();

        private PgnMoveFormatter() : base(PieceSymbols) { }
    }
}

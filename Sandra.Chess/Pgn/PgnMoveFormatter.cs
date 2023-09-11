#region License
/*********************************************************************************
 * PgnMoveFormatter.cs
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

using Eutherion.Threading;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Move formatter which generates algebraic notation for PGN text.
    /// </summary>
    public static class PgnMoveFormatter
    {
        /// <summary>
        /// Standardized PGN notation for pieces.
        /// </summary>
        public static readonly string PieceSymbols = "NBRQK";

        private static readonly SafeLazy<ShortAlgebraicMoveFormatter> pgnMoveFormatter = new SafeLazy<ShortAlgebraicMoveFormatter>(
            () => new ShortAlgebraicMoveFormatter(PieceSymbols));

        /// <summary>
        /// Returns a <see cref="ShortAlgebraicMoveFormatter"/> instance that uses PGN piece symbols to generate moves.
        /// </summary>
        public static ShortAlgebraicMoveFormatter MoveFormatter => pgnMoveFormatter.Value;
    }
}

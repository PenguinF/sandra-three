#region License
/*********************************************************************************
 * PgnGameResult.cs
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
    /// Enumerates four types of game termination markers in PGN.
    /// </summary>
    public enum PgnGameResult
    {
        /// <summary>
        /// The game is in progress, its result is unknown, or the game is abandoned.
        /// </summary>
        Undetermined,

        /// <summary>
        /// Black wins the game.
        /// </summary>
        BlackWins,

        /// <summary>
        /// The game is a draw.
        /// </summary>
        Draw,

        /// <summary>
        /// White wins the game.
        /// </summary>
        WhiteWins,
    }
}

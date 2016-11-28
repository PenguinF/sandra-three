/*********************************************************************************
 * MoveFormatter.cs
 * 
 * Copyright (c) 2004-2016 Henk Nicolai
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
 *********************************************************************************/
namespace Sandra.Chess
{
    /// <summary>
    /// Defines methods for showing moves in text, and parsing moves from textual input.
    /// </summary>
    public abstract class AbstractMoveFormatter
    {
        /// <summary>
        /// Generates a formatted notation for a move in a given position.
        /// As a side effect, the position is updated to the resulting position after the move.
        /// </summary>
        /// <param name="positionBefore">
        /// The position in which the move was made.
        /// </param>
        /// <param name="move">
        /// The move to be formatted.
        /// </param>
        /// <returns>
        /// The formatted notation for the move.
        /// </returns>
        public abstract string FormatMove(Position positionBefore, Move move);
    }
}

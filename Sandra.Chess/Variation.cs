/*********************************************************************************
 * Variation.cs
 * 
 * Copyright (c) 2004-2017 Henk Nicolai
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
using System.Collections.Generic;

namespace Sandra.Chess
{
    /// <summary>
    /// Represents a variation of a chess game.
    /// </summary>
    /// <remarks>
    /// For now implemented as a kind of linked list, but this is not ideal for localized access in memory.
    /// </remarks>
    public class Variation
    {
        /// <summary>
        /// Gets the <see cref="Move"/> which starts this variation.
        /// </summary>
        public readonly Move Move;

        public Variation Parent;
        public Variation Main;

        public Variation(Move move)
        {
            Move = move;
        }
    }

    public class MoveIndex
    {
        public readonly int Value;

        public MoveIndex(int value) { Value = value; }

        public static readonly MoveIndex BeforeFirstMove = new MoveIndex(0);
    }

    public class IndexedMove
    {
        public readonly Move Move;
        public readonly MoveIndex MoveIndex;

        public IndexedMove(Move move, MoveIndex moveIndex)
        {
            Move = move;
            MoveIndex = moveIndex;
        }
    }
}

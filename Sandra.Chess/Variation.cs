﻿/*********************************************************************************
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

        /// <summary>
        /// Gets the index for the <see cref="Move"/> which starts this variation.
        /// </summary>
        public readonly MoveIndex MoveIndex;

        public readonly Variation Parent;
        public Variation Main;

        public Variation(Variation parent, Move move, MoveIndex moveIndex)
        {
            Parent = parent;
            Move = move;
            MoveIndex = moveIndex;
        }
    }

    /// <summary>
    /// Represents a tree of moves. At the top level, this is a choice of variations.
    /// </summary>
    public class MoveTree
    {
        public Variation Main;

        public MoveTree()
        {
        }

        public MoveTree(Variation main)
        {
            Main = main;
        }
    }

    public class MoveIndex
    {
        // To generate unique keys.
        static int keyCounter;

        private readonly int key;

        public MoveIndex() { key = keyCounter++; }

        public bool EqualTo(MoveIndex other) { return key == other.key; }

        public static readonly MoveIndex BeforeFirstMove = new MoveIndex();
    }
}

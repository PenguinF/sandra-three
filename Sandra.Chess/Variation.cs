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

        /// <summary>
        /// Gets the parent <see cref="MoveTree"/> in which this variation is embedded. This field is never equal to null.
        /// </summary>
        public readonly MoveTree ParentTree;

        /// <summary>
        /// Gets the tree of moves after <see cref="Move"/> has been played.
        /// </summary>
        public readonly MoveTree MoveTree;

        internal Variation(MoveTree parentTree, Move move)
        {
            ParentTree = parentTree;
            Move = move;
            MoveTree = new MoveTree(this);
        }
    }

    /// <summary>
    /// Represents a tree of moves. At the top level, this is a choice of variations.
    /// </summary>
    public class MoveTree
    {
        /// <summary>
        /// Gets the number of moves played before this move tree starts.
        /// </summary>
        public readonly int MoveCount;

        /// <summary>
        /// Gets the ply count at which this move tree starts.
        /// </summary>
        /// <remarks>
        /// If in the initial position black is to move, the ply count starts at 1 rather than 0.
        /// </remarks>
        public readonly int PlyCount;

        /// <summary>
        /// Gets the optional parent <see cref="Variation"/> with previous move in which this <see cref="MoveTree"/> is embedded.
        /// </summary>
        public readonly Variation ParentVariation;

        /// <summary>
        /// All variations in this list have a unique move.
        /// The first variation in this list can be null, to allow branches to extend from the end of a main variation.
        /// The other varations in this list however are always not-null.
        /// </summary>
        private readonly List<Variation> branches = new List<Variation>();

        public Variation Main => branches[0];

        public void AddVariation(Move move)
        {
            branches[0] = new Variation(this, move);
        }

        public void RemoveVariation(Move move)
        {
            if (Main != null && Main.Move.CreateMoveInfo().InputEquals(move.CreateMoveInfo()))
            {
                branches[0] = null;
            }
        }

        internal MoveTree(Variation parentVariation)
        {
            branches.Add(null);
            MoveCount = parentVariation.ParentTree.MoveCount + 1;
            PlyCount = parentVariation.ParentTree.PlyCount + 1;
            ParentVariation = parentVariation;
        }

        internal MoveTree(bool blackToMove)
        {
            branches.Add(null);
            MoveCount = 0;
            PlyCount = blackToMove ? 1 : 0;
            ParentVariation = null;
        }
    }
}

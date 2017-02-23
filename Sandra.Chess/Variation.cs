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
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Gets the parent <see cref="Chess.MoveTree"/> in which this variation is embedded. This field is never equal to null.
        /// </summary>
        public readonly MoveTree ParentTree;

        /// <summary>
        /// Gets the index of this variation within the list of branches before this move.
        /// </summary>
        public int VariationIndex { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Move"/> which starts this variation.
        /// </summary>
        public readonly Move Move;

        /// <summary>
        /// Gets the tree of moves after <see cref="Move"/> has been played.
        /// </summary>
        public readonly MoveTree MoveTree;

        internal Variation(MoveTree parentTree, int variationIndex, Move move)
        {
            ParentTree = parentTree;
            VariationIndex = variationIndex;
            Move = move;
            MoveTree = new MoveTree(this);
        }

        private void reposition(int destinationVariationIndex, bool after)
        {
            if (destinationVariationIndex < 0
                || destinationVariationIndex >= ParentTree.Variations.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(destinationVariationIndex));
            }

            // Only after the range check, increase the index.
            if (after) destinationVariationIndex++;

            if (VariationIndex > destinationVariationIndex)
            {
                // Promote this line.
                int oldVariationIndex = VariationIndex;
                ParentTree.Variations.RemoveAt(oldVariationIndex);
                ParentTree.Variations.Insert(destinationVariationIndex, this);
                for (int i = destinationVariationIndex; i <= oldVariationIndex; ++i)
                {
                    ParentTree.Variations[i].VariationIndex = i;
                }
            }
            else if (VariationIndex + 1 < destinationVariationIndex)
            {
                // Demote this line.
                int oldVariationIndex = VariationIndex;
                ParentTree.Variations.RemoveAt(VariationIndex);
                ParentTree.Variations.Insert(destinationVariationIndex - 1, this);
                for (int i = oldVariationIndex; i < destinationVariationIndex; ++i)
                {
                    ParentTree.Variations[i].VariationIndex = i;
                }
            }
        }

        /// <summary>
        /// Attempts to reposition this variation before a destination index in the parent move tree.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="destinationVariationIndex"/> is less than zero
        /// or greater than or equal to the number of variations in the parent move tree.
        /// </exception>
        public void RepositionBefore(int destinationVariationIndex)
        {
            reposition(destinationVariationIndex, false);
        }

        /// <summary>
        /// Attempts to reposition this variation after a destination index in the parent move tree.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="destinationVariationIndex"/> is less than zero
        /// or greater than or equal to the number of variations in the parent move tree.
        /// </exception>
        public void RepositionAfter(int destinationVariationIndex)
        {
            reposition(destinationVariationIndex, true);
        }
    }

    /// <summary>
    /// Represents a tree of moves. At the top level, this is a choice of variations.
    /// </summary>
    public class MoveTree
    {
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
        /// The first variation in this list can be null, to allow side lines to extend from the end of a main line.
        /// The other varations in this list however are always not-null.
        /// </summary>
        internal readonly List<Variation> Variations = new List<Variation>();

        public Variation MainLine => Variations[0];

        public IEnumerable<Variation> SideLines => Variations.Skip(1);

        /// <summary>
        /// Gets the total number of variations branching from this position.
        /// </summary>
        public int VariationCount => MainLine == null ? 0 : Variations.Count;

        public Variation GetOrAddVariation(Move move)
        {
            if (Variations[0] == null)
            {
                // Continue adding to ParentVariation.MoveIndex for the main variation.
                Variations[0] = new Variation(this, 0, move);
                return Variations[0];
            }
            else
            {
                foreach (Variation branch in Variations)
                {
                    if (branch.Move.CreateMoveInfo().InputEquals(move.CreateMoveInfo()))
                    {
                        return branch;
                    }
                }

                // Reset moveIndex at 0.
                Variation newBranch = new Variation(this, Variations.Count, move);
                Variations.Add(newBranch);
                return newBranch;
            }
        }

        internal MoveTree(Variation parentVariation)
        {
            Variations.Add(null);
            PlyCount = parentVariation.ParentTree.PlyCount + 1;
            ParentVariation = parentVariation;
        }

        internal MoveTree(bool blackToMove)
        {
            Variations.Add(null);
            PlyCount = blackToMove ? 1 : 0;
            ParentVariation = null;
        }
    }
}

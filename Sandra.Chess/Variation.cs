#region License
/*********************************************************************************
 * Variation.cs
 *
 * Copyright (c) 2004-2019 Henk Nicolai
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
using System.Diagnostics;
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

                // Exceptional case where the main line is null.
                int reIndexUpperBound;
                if (destinationVariationIndex == 0 && ParentTree.Variations[0] == null)
                {
                    ParentTree.Variations[0] = this;
                    reIndexUpperBound = ParentTree.Variations.Count - 1;
                }
                else
                {
                    ParentTree.Variations.Insert(destinationVariationIndex, this);
                    reIndexUpperBound = oldVariationIndex;
                }

                for (int i = destinationVariationIndex; i <= reIndexUpperBound; ++i)
                {
                    ParentTree.Variations[i].VariationIndex = i;
                }
                Debug.Assert(ParentTree.CheckVariationIndexes());
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
                Debug.Assert(ParentTree.CheckVariationIndexes());
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
        /// Gets a list of variations branching from this position.
        /// Each variation in this list starts with a unique move.
        /// The first variation in this list can be null, to allow side lines to extend from the end of a main line.
        /// The other varations in this list however are always not-null.
        /// </summary>
        public readonly List<Variation> Variations = new List<Variation>();

        /// <summary>
        /// Returns the first line in <see cref="Variations"/>.
        /// </summary>
        public Variation MainLine => Variations[0];

        /// <summary>
        /// Enumerates all side lines in <see cref="Variations"/>.
        /// </summary>
        public IEnumerable<Variation> SideLines => Variations.Skip(1);

        public Variation GetOrAddVariation(Move move)
        {
            foreach (Variation branch in Variations)
            {
                if (branch != null && branch.Move.CreateMoveInfo().InputEquals(move.CreateMoveInfo()))
                {
                    return branch;
                }
            }

            if (Variations[0] == null)
            {
                Variations[0] = new Variation(this, 0, move);
                Debug.Assert(CheckVariationIndexes());
                return Variations[0];
            }
            else
            {
                Variation newBranch = new Variation(this, Variations.Count, move);
                Variations.Add(newBranch);
                Debug.Assert(CheckVariationIndexes());
                return newBranch;
            }
        }

        public void RemoveVariation(Variation variation)
        {
            if (variation == null) throw new ArgumentNullException(nameof(variation));
            if (0 <= variation.VariationIndex
                && variation.VariationIndex < Variations.Count
                && ReferenceEquals(variation, Variations[variation.VariationIndex]))
            {
                // Remove and re-index the variations after.
                Variations.RemoveAt(variation.VariationIndex);
                if (Variations.Count == 0)
                {
                    // Replace by an empty main line.
                    Variations.Add(null);
                }
                else
                {
                    for (int i = variation.VariationIndex; i < Variations.Count; ++i)
                    {
                        Variations[i].VariationIndex = i;
                    }
                }
                Debug.Assert(CheckVariationIndexes());
            }
        }

        /// <summary>
        /// Turns the main line into a side line.
        /// </summary>
        public void Break()
        {
            if (Variations[0] != null)
            {
                Variations.Insert(0, null);
                for (int i = 1; i < Variations.Count; ++i)
                {
                    Variations[i].VariationIndex = i;
                }
            }
            Debug.Assert(CheckVariationIndexes());
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

        internal bool CheckVariationIndexes()
        {
            for (int i = 0; i < Variations.Count; ++i)
            {
                if (Variations[i] != null && Variations[i].VariationIndex != i)
                {
                    return false;
                }
            }
            return true;
        }
    }
}

#region License
/*********************************************************************************
 * Game.cs
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

using System;
using System.Collections.Generic;

namespace Sandra.Chess
{
    /// <summary>
    /// Represents a standard game of chess.
    /// </summary>
    public class Game
    {
        /// <summary>
        /// Gets the initial position of this game.
        /// </summary>
        public ReadOnlyPosition InitialPosition { get; }

        /// <summary>
        /// Gets the current position of this game.
        /// </summary>
        public ReadOnlyPosition CurrentPosition { get; private set; }

        /// <summary>
        /// Gets a reference to the root of the <see cref="Chess.MoveTree"/> of this <see cref="Game"/>.
        /// </summary>
        public MoveTree MoveTree { get; }

        /// <summary>
        /// Creates a new game with the default initial <see cref="Position"/>.
        /// </summary>
        public Game()
        {
            Position initialPosition = Position.GetInitialPosition();
            MoveTree moveTree = new MoveTree(initialPosition.SideToMove == Color.Black);
            InitialPosition = new ReadOnlyPosition(initialPosition);
            CurrentPosition = InitialPosition;
            MoveTree = moveTree;
            ActiveTree = moveTree;
        }

        /// <summary>
        /// Gets the move tree which is currently active.
        /// </summary>
        public MoveTree ActiveTree { get; private set; }

        private void SetActiveTreeInner(MoveTree value)
        {
            // Replay all moves until the new active tree has been reached.
            Stack<Move> previousMoves = new Stack<Move>();
            MoveTree newActiveTree = value;

            MoveTree current;
            for (current = newActiveTree; current.ParentVariation != null; current = current.ParentVariation.ParentTree)
            {
                previousMoves.Push(current.ParentVariation.Move);
            }

            // 'value' should be embedded somewhere inside this.moveTree.
            if (current != MoveTree)
            {
                throw new ArgumentException("value is not embedded in Game.MoveTree.", nameof(value));
            }

            Position newPosition = InitialPosition.Copy();
            foreach (Move move in previousMoves)
            {
                newPosition.FastMakeMove(move);
            }

            CurrentPosition = new ReadOnlyPosition(newPosition);
            ActiveTree = newActiveTree;
        }

        public void SetActiveTree(MoveTree value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (ActiveTree != value)
            {
                SetActiveTreeInner(value);
            }
        }

        public bool IsFirstMove => ActiveTree.ParentVariation == null;
        public bool IsLastMove => ActiveTree.MainLine == null;
        public Move PreviousMove() => ActiveTree.ParentVariation.Move;

        public void Backward()
        {
            // No effect if first move.
            if (IsFirstMove) return;

            // Replay until the previous move.
            SetActiveTreeInner(ActiveTree.ParentVariation.ParentTree);
        }

        public void Forward()
        {
            // No effect if last move.
            if (IsLastMove) return;

            Position currentPosition = CurrentPosition.Copy();
            currentPosition.FastMakeMove(ActiveTree.MainLine.Move);
            CurrentPosition = new ReadOnlyPosition(currentPosition);
            ActiveTree = ActiveTree.MainLine.MoveTree;
        }

        /// <summary>
        /// Validates a move against the current position and optionally performs it.
        /// </summary>
        /// <param name="moveInfo">
        /// The move to validate and optionally perform.
        /// </param>
        /// <param name="make">
        /// True if the move must actually be made, false if only validated.
        /// </param>
        /// <returns>
        /// A valid legal <see cref="Move"/> structure if <see cref="MoveInfo.Result"/> is equal to  
        /// <see cref="MoveCheckResult.OK"/>, or an incomplete <see cref="Move"/> if one of the other <see cref="MoveCheckResult"/> values.
        /// If <paramref name="make"/> is true, the move is only made if <see cref="MoveCheckResult.OK"/> is returned.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when any of the move's members have an enumeration value which is outside of the allowed range.
        /// </exception>
        public Move TryMakeMove(ref MoveInfo moveInfo, bool make)
        {
            if (!make)
            {
                moveInfo.Result = CurrentPosition.TestMove(moveInfo);
                return default;
            }
            else
            {
                // Disable until we can modify PGN using its syntax tree.
                moveInfo.Result = ~MoveCheckResult.OK;
                return default;
            }
        }
    }
}

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
        /// Makes a move in the current position if it is legal.
        /// </summary>
        /// <param name="moveInfo">
        /// The move to make.
        /// </param>
        /// <returns>
        /// A <see cref="MoveCheckResult.OK"/> if the move is made and therefore legal; otherwise a <see cref="MoveCheckResult"/> value
        /// which describes the reason why the move is illegal.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when any of <paramref name="moveInfo"/>'s members have an enumeration value which is outside of the allowed range.
        /// </exception>
        public MoveCheckResult TryMakeMove(MoveInfo moveInfo)
        {
            // Disable until we can modify PGN using its syntax tree.
            moveInfo.Result = ~MoveCheckResult.OK;
            return default;
        }
    }
}

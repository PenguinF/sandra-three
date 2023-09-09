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
        /// Creates a new game with the default initial <see cref="Position"/>.
        /// </summary>
        public Game()
        {
            Position initialPosition = Position.GetInitialPosition();
            InitialPosition = new ReadOnlyPosition(initialPosition);
            CurrentPosition = InitialPosition;
        }

        public bool IsFirstMove => true;
        public bool IsLastMove => true;
        public Move PreviousMove() => default;

        public void Backward()
        {
        }

        public void Forward()
        {
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

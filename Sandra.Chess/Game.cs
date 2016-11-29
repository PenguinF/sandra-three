/*********************************************************************************
 * Game.cs
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandra.Chess
{
    /// <summary>
    /// Represents a standard game of chess.
    /// </summary>
    public class Game
    {
        private readonly Position initialPosition;
        private readonly Position currentPosition;
        private readonly List<Move> moveList = new List<Move>();

        public Game(Position initialPosition)
        {
            this.initialPosition = initialPosition;
            currentPosition = initialPosition.Copy();
        }

        /// <summary>
        /// Gets the initial position of this game.
        /// </summary>
        public Position InitialPosition => initialPosition.Copy();

        /// <summary>
        /// Gets the current position of this game.
        /// </summary>
        public Position CurrentPosition => currentPosition.Copy();

        /// <summary>
        /// Returns the number of moves played after the initial position.
        /// </summary>
        public int MoveCount => moveList.Count;

        /// <summary>
        /// Enumerates all moves that led from the initial to the current position.
        /// </summary>
        public IEnumerable<Move> Moves
        {
            get
            {
                foreach (var move in moveList) yield return move;
            }
        }

        /// <summary>
        /// Gets the <see cref="Move"/> at position <paramref name="moveIndex"/>.
        /// </summary>
        public Move GetMove(int moveIndex) => moveList[moveIndex];

        /// <summary>
        /// Gets the <see cref="Color"/> of the side to move.
        /// </summary>
        public Color SideToMove => currentPosition.SideToMove;

        /// <summary>
        /// Gets the <see cref="ColoredPiece"/> which occupies a square, or null if the square is not occupied.
        /// </summary>
        public ColoredPiece? GetColoredPiece(Square square)
        {
            ulong squareVector = square.ToVector();
            Piece piece;
            if (EnumHelper<Piece>.AllValues.Any(x => currentPosition.GetVector(x).Test(squareVector), out piece))
            {
                if (currentPosition.GetVector(Color.White).Test(squareVector))
                {
                    return piece.Combine(Color.White);
                }
                return piece.Combine(Color.Black);
            }
            return null;
        }

        /// <summary>
        /// If a pawn can be captured en passant in this position, returns the square of that pawn.
        /// Otherwise <see cref="Square.A1"/> is returned. 
        /// </summary>
        public Square EnPassantCaptureSquare => currentPosition.EnPassantCaptureVector.GetSingleSquare();

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
            Move move = currentPosition.TryMakeMove(ref moveInfo, make);
            if (make && moveInfo.Result == MoveCheckResult.OK)
            {
                moveList.Add(move);
                RaiseMoveMade(move);
            }
            return move;
        }

        /// <summary>
        /// Occurs when a move has been made successfully.
        /// </summary>
        public event EventHandler<MoveMadeEventArgs> MoveMade;

        /// <summary>
        /// Raises the <see cref="MoveMade"/> event. 
        /// </summary>
        protected virtual void OnMoveMade(MoveMadeEventArgs e)
        {
            MoveMade?.Invoke(this, e);
        }

        protected void RaiseMoveMade(Move move)
        {
            OnMoveMade(new MoveMadeEventArgs(move));
        }
    }

    public class MoveMadeEventArgs : EventArgs
    {
        public readonly Move Move;

        public MoveMadeEventArgs(Move move)
        {
            Move = move;
        }
    }
}

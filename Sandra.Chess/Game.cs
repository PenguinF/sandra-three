/*********************************************************************************
 * Game.cs
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
    /// Represents a standard game of chess.
    /// </summary>
    public class Game
    {
        private readonly Position initialPosition;

        // null == no moves.
        private Variation mainVariation;

        private Position currentPosition;

        // Points at the variation with the move which was just played in the current position.
        // Is null at the start of the game.
        private Variation activeVariation;

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
        /// Gets the <see cref="Color"/> of the side to move in the initial position.
        /// </summary>
        public Color InitialSideToMove => initialPosition.SideToMove;

        /// <summary>
        /// Gets the current position of this game.
        /// </summary>
        public Position CurrentPosition => currentPosition.Copy();

        /// <summary>
        /// Gets or sets the index of the active move. This is a value between 0 and <see cref="MoveCount"/>.
        /// </summary>
        public MoveIndex ActiveMoveIndex
        {
            get
            {
                return activeVariation != null ? activeVariation.MoveIndex : MoveIndex.BeforeFirstMove;
            }
        }

        public void SetActiveMoveIndex(MoveIndex value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            MoveIndex activeMoveIndex = activeVariation != null ? activeVariation.MoveIndex : MoveIndex.BeforeFirstMove;
            if (!activeMoveIndex.EqualTo(value))
            {
                Position newPosition = initialPosition.Copy();
                Variation newActiveVariation = null;
                if (!value.EqualTo(MoveIndex.BeforeFirstMove))
                {
                    // Linear search for the right move index.
                    Variation current = mainVariation;
                    while (current != null)
                    {
                        newPosition.FastMakeMove(current.Move);
                        if (current.MoveIndex.EqualTo(value))
                        {
                            newActiveVariation = current;
                            break;
                        }
                        current = current.Main;
                    }

                    if (newActiveVariation == null)
                    {
                        throw new ArgumentOutOfRangeException(nameof(value));
                    }
                }

                currentPosition = newPosition;
                activeVariation = newActiveVariation;
                RaiseActiveMoveIndexChanged();
            }
        }

        public bool IsFirstMove => activeVariation == null;
        public bool IsLastMove => activeVariation == null ? mainVariation == null : activeVariation.Main == null;
        public Move PreviousMove() => activeVariation.Move;

        public void Backward()
        {
            if (IsFirstMove)
            {
                throw new InvalidOperationException("Cannot go backward when it's the first move.");
            }

            // Replay until the previous move.
            Position newPosition = initialPosition.Copy();
            Variation current = mainVariation;
            while (current != activeVariation)
            {
                newPosition.FastMakeMove(current.Move);
                current = current.Main;
            }

            currentPosition = newPosition;
            activeVariation = activeVariation.Parent;
            RaiseActiveMoveIndexChanged();
        }

        public void Forward()
        {
            if (IsLastMove)
            {
                throw new InvalidOperationException("Cannot go forward when it's the last move.");
            }

            Variation next = activeVariation != null ? activeVariation.Main : mainVariation;
            currentPosition.FastMakeMove(next.Move);
            activeVariation = next;
            RaiseActiveMoveIndexChanged();
        }

        public Variation MainVariation => mainVariation;

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
        /// Enumerates all squares that are occupied by the given colored piece.
        /// </summary>
        public IEnumerable<Square> AllSquaresOccupiedBy(ColoredPiece coloredPiece) => currentPosition.GetVector(coloredPiece).AllSquares();

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
                bool add = true;
                Variation next = activeVariation != null ? activeVariation.Main : mainVariation;
                if (next != null)
                {
                    if (next.Move.CreateMoveInfo().InputEquals(move.CreateMoveInfo()))
                    {
                        // Moves are the same, only move forward.
                        activeVariation = next;
                        add = false;
                    }
                    else
                    {
                        // Erase the active move and everything after.
                        if (activeVariation == null) mainVariation = null;
                        else activeVariation.Main = null;
                    }
                }
                if (add)
                {
                    if (activeVariation == null)
                    {
                        mainVariation = new Variation(activeVariation, move, new MoveIndex(1));
                        activeVariation = mainVariation;
                    }
                    else
                    {
                        activeVariation.Main = new Variation(activeVariation, move, new MoveIndex(activeVariation.MoveIndex.Value + 1));
                        activeVariation = activeVariation.Main;
                    }
                }
                RaiseActiveMoveIndexChanged();
            }
            return move;
        }

        /// <summary>
        /// Occurs when the active move index of the game was updated.
        /// </summary>
        public event EventHandler ActiveMoveIndexChanged;

        /// <summary>
        /// Raises the <see cref="ActiveMoveIndexChanged"/> event. 
        /// </summary>
        protected virtual void OnActiveMoveIndexChanged(EventArgs e)
        {
            ActiveMoveIndexChanged?.Invoke(this, e);
        }

        protected void RaiseActiveMoveIndexChanged()
        {
            OnActiveMoveIndexChanged(EventArgs.Empty);
        }
    }
}

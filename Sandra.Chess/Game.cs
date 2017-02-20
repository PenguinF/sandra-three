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
        private readonly MoveTree moveTree;

        private Position currentPosition;
        private MoveTree activeTree;

        private Game(Position initialPosition, MoveTree moveTree)
        {
            this.initialPosition = initialPosition;
            currentPosition = initialPosition.Copy();
            this.moveTree = moveTree;
            activeTree = moveTree;
        }

        /// <summary>
        /// Creates a new game with a given initial <see cref="Position"/>.
        /// </summary>
        public Game(Position initialPosition) : this(initialPosition,
                                                     new MoveTree(initialPosition.SideToMove == Color.Black))
        {
        }

        /// <summary>
        /// Returns a copy of this game, with the same initial <see cref="Position"/> and shared <see cref="Chess.MoveTree"/>,
        /// but in which <see cref="ActiveTree"/> can be manipulated independently.
        /// </summary>
        public Game Copy() => new Game(initialPosition, moveTree);

        /// <summary>
        /// Gets the initial position of this game.
        /// </summary>
        public Position InitialPosition => initialPosition.Copy();

        /// <summary>
        /// Gets the <see cref="Color"/> of the side to move in the initial position.
        /// </summary>
        public Color InitialSideToMove => initialPosition.SideToMove;

        /// <summary>
        /// Gets a reference to the root of the <see cref="Chess.MoveTree"/> of this <see cref="Game"/>.
        /// </summary>
        public MoveTree MoveTree => moveTree;

        /// <summary>
        /// Gets the current position of this game.
        /// </summary>
        public Position CurrentPosition => currentPosition.Copy();

        /// <summary>
        /// Gets the move tree which is currently active.
        /// </summary>
        public MoveTree ActiveTree => activeTree;

        private void setActiveTree(MoveTree value)
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
            if (current != moveTree)
            {
                throw new ArgumentException(nameof(value), "value is not embedded in Game.MoveTree.");
            }

            Position newPosition = initialPosition.Copy();
            foreach (Move move in previousMoves)
            {
                newPosition.FastMakeMove(move);
            }

            currentPosition = newPosition;
            activeTree = newActiveTree;
            RaiseActiveMoveIndexChanged();
        }

        public void SetActiveTree(MoveTree value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (activeTree != value)
            {
                setActiveTree(value);
            }
        }

        public bool IsFirstMove => activeTree.ParentVariation == null;
        public bool IsLastMove => activeTree.Main == null;
        public Move PreviousMove() => activeTree.ParentVariation.Move;

        public void Backward()
        {
            if (IsFirstMove)
            {
                throw new InvalidOperationException("Cannot go backward when it's the first move.");
            }

            // Replay until the previous move.
            setActiveTree(activeTree.ParentVariation.ParentTree);
        }

        public void Forward()
        {
            if (IsLastMove)
            {
                throw new InvalidOperationException("Cannot go forward when it's the last move.");
            }

            currentPosition.FastMakeMove(activeTree.Main.Move);
            activeTree = activeTree.Main.MoveTree;
            RaiseActiveMoveIndexChanged();
        }

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
                // Move to an existing variation, or create a new one.
                Variation variation = activeTree.GetOrAddVariation(move);
                activeTree = variation.MoveTree;
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

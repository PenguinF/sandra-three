#region License
/*********************************************************************************
 * Game.cs
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

using SysExtensions;
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
        private Position currentPosition;

        /// <summary>
        /// Gets a reference to the root of the <see cref="Chess.MoveTree"/> of this <see cref="Game"/>.
        /// </summary>
        public MoveTree MoveTree { get; }

        private Game(Position initialPosition, MoveTree moveTree)
        {
            this.initialPosition = initialPosition;
            currentPosition = initialPosition.Copy();
            MoveTree = moveTree;
            ActiveTree = moveTree;
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
        public Game Copy() => new Game(initialPosition, MoveTree);

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
                throw new ArgumentException(nameof(value), "value is not embedded in Game.MoveTree.");
            }

            Position newPosition = initialPosition.Copy();
            foreach (Move move in previousMoves)
            {
                newPosition.FastMakeMove(move);
            }

            currentPosition = newPosition;
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

            currentPosition.FastMakeMove(ActiveTree.MainLine.Move);
            ActiveTree = ActiveTree.MainLine.MoveTree;
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

            if (EnumHelper<Piece>.AllValues.Any(x => currentPosition.GetVector(x).Test(squareVector), out Piece piece))
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
                Variation variation = ActiveTree.GetOrAddVariation(move);
                ActiveTree = variation.MoveTree;
            }
            return move;
        }
    }
}

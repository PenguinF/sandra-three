#region License
/*********************************************************************************
 * ReadOnlyPosition.cs
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
    /// Represents a read-only position in a standard chess game.
    /// </summary>
    public class ReadOnlyPosition
    {
        private readonly Position Position;

        /// <summary>
        /// Takes a copy of an existing position and stores it for read-only access.
        /// </summary>
        /// <param name="position">
        /// The <see cref="Chess.Position"/> to store.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="position"/> is <see langword="null"/>.
        /// </exception>
        public ReadOnlyPosition(Position position)
        {
            if (position == null) throw new ArgumentNullException(nameof(position));
            Position = position.Copy();
        }

        /// <summary>
        /// Gets the <see cref="Color"/> of the side to move.
        /// </summary>
        public Color SideToMove => Position.SideToMove;

        /// <summary>
        /// Gets a vector which is true for all squares that contain the given color.
        /// </summary>
        public ulong GetVector(Color color) => Position.GetVector(color);

        /// <summary>
        /// Gets a vector which is true for all squares that contain the given piece.
        /// </summary>
        public ulong GetVector(Piece piece) => Position.GetVector(piece);

        /// <summary>
        /// Gets a vector which is true for all squares that contain the given colored piece.
        /// </summary>
        public ulong GetVector(ColoredPiece coloredPiece) => Position.GetVector(coloredPiece);

        /// <summary>
        /// Gets a vector which is true for all squares that are empty.
        /// </summary>
        public ulong GetEmptyVector() => Position.GetEmptyVector();

        /// <summary>
        /// If a pawn can be captured en passant in this position, returns the vector which is true for the square of that pawn.
        /// </summary>
        public ulong EnPassantCaptureVector => Position.EnPassantCaptureVector;

        /// <summary>
        /// If a pawn can be captured en passant in this position, returns the square of that pawn.
        /// Otherwise <see cref="Square.A1"/> is returned. 
        /// </summary>
        public Square EnPassantCaptureSquare => Position.EnPassantCaptureSquare;

        /// <summary>
        /// Gets the <see cref="ColoredPiece"/> which occupies a square, or <see langword="null"/> if the square is not occupied.
        /// </summary>
        public ColoredPiece? GetColoredPiece(Square square) => Position.GetColoredPiece(square);

        /// <summary>
        /// Enumerates all squares that are occupied by the given colored piece.
        /// </summary>
        public IEnumerable<Square> AllSquaresOccupiedBy(ColoredPiece coloredPiece) => Position.AllSquaresOccupiedBy(coloredPiece);

        /// <summary>
        /// Returns if the given square is attacked by a piece of the opposite color.
        /// </summary>
        public bool IsSquareUnderAttack(Square square, Color defenderColor) => Position.IsSquareUnderAttack(square, defenderColor);

        /// <summary>
        /// Returns the position of the king of the given color.
        /// </summary>
        public Square FindKing(Color color) => Position.FindKing(color);

        /// <summary>
        /// Generates and enumerates all non-castling moves which are legal in this position.
        /// </summary>
        public IEnumerable<MoveInfo> GenerateLegalMoves() => Position.GenerateLegalMoves();

        /// <summary>
        /// Validates a move against this position.
        /// </summary>
        /// <param name="moveInfo">
        /// The move to test.
        /// </param>
        /// <returns>
        /// A <see cref="MoveCheckResult.OK"/> if the tested move is legal; otherwise a <see cref="MoveCheckResult"/> value
        /// which describes the reason why the move is invalid.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when any of <paramref name="moveInfo"/>'s members have an enumeration value which is outside of the allowed range.
        /// </exception>
        public MoveCheckResult TestMove(MoveInfo moveInfo)
        {
            // Using false will leave the position unmodified.
            Position.TryMakeMove(ref moveInfo, false);
            return moveInfo.Result;
        }

        /// <summary>
        /// Creates a mutable copy of this <see cref="ReadOnlyPosition"/>.
        /// </summary>
        public Position Copy() => Position.Copy();
    }
}

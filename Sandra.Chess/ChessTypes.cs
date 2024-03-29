﻿#region License
/*********************************************************************************
 * ChessTypes.cs
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

using Sandra.Chess;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sandra.Chess
{
    /// <summary>
    /// Specifies one of six chess pieces.
    /// </summary>
    public enum Piece
    {
        Pawn, Knight, Bishop, Rook, Queen, King,
    }

    /// <summary>
    /// Specifies one of two chess colors.
    /// </summary>
    public enum Color
    {
        White, Black,
    }

    /// <summary>
    /// Specifies all twelve distinct types of chess pieces.
    /// <see cref="ColoredPiece"/> is a combination of the <see cref="Piece"/> and <see cref="Color"/> enumerations.
    /// </summary>
    public enum ColoredPiece
    {
        WhitePawn, WhiteKnight, WhiteBishop, WhiteRook, WhiteQueen, WhiteKing,
        BlackPawn, BlackKnight, BlackBishop, BlackRook, BlackQueen, BlackKing,
    }

    /// <summary>
    /// Specifies a rank on a standard 8x8 chess board.
    /// </summary>
    /// <remarks>
    /// Be aware that when casting to integer, the 'value' decreases with one, for example (int)Rank._3 yields 2.
    /// </remarks>
    public enum Rank
    {
        _1, _2, _3, _4, _5, _6, _7, _8,
    }

    /// <summary>
    /// Specifies a file on a standard 8x8 chess board.
    /// </summary>
    public enum File
    {
        A, B, C, D, E, F, G, H,
    }

    /// <summary>
    /// Specifies a square on a standard 8x8 chess board.
    /// </summary>
    public enum Square
    {
        A1, B1, C1, D1, E1, F1, G1, H1,
        A2, B2, C2, D2, E2, F2, G2, H2,
        A3, B3, C3, D3, E3, F3, G3, H3,
        A4, B4, C4, D4, E4, F4, G4, H4,
        A5, B5, C5, D5, E5, F5, G5, H5,
        A6, B6, C6, D6, E6, F6, G6, H6,
        A7, B7, C7, D7, E7, F7, G7, H7,
        A8, B8, C8, D8, E8, F8, G8, H8,
    }
}

namespace Sandra
{
    /// <summary>
    /// Contains extension methods for chess enumeration types and bit vectors (<see cref="ulong"/>).
    /// </summary>
    public static class ChessExtensions
    {
        /// <summary>
        /// Returns the opposite of a given <see cref="Color"/>.
        /// </summary>
        public static Color Opposite(this Color color) => 1 - color;

        /// <summary>
        /// Returns the <see cref="Color"/> of a given piece.
        /// </summary>
        public static Color GetColor(this ColoredPiece coloredPiece)
            => (Color)((int)coloredPiece / Constants.PieceCount);

        /// <summary>
        /// Returns the <see cref="Piece"/> part of a given colored piece.
        /// </summary>
        public static Piece GetPiece(this ColoredPiece coloredPiece)
            => (Piece)((int)coloredPiece % Constants.PieceCount);

        /// <summary>
        /// Returns the colored piece combination of a piece and a color.
        /// </summary>
        public static ColoredPiece Combine(this Piece piece, Color color)
            => (ColoredPiece)((int)color * Constants.PieceCount + piece);

        /// <summary>
        /// Returns the square at the position specified by the file and rank.
        /// </summary>
        public static Square Combine(this File file, Rank rank)
            => (Square)((int)rank * Constants.SquareCount + file);

        /// <summary>
        /// Returns the X-coordinate of a square.
        /// </summary>
        public static int X(this Square square) => (int)square % Constants.SquareCount;

        /// <summary>
        /// Returns the Y-coordinate of a square.
        /// </summary>
        public static int Y(this Square square) => (int)square / Constants.SquareCount;

        /// <summary>
        /// Returns a vector which is true only at the given square.
        /// </summary>
        public static ulong ToVector(this Square square) => 1UL << (int)square;

        /// <summary>
        /// Returns a vector which is true for each square in the same file.
        /// </summary>
        public static ulong ToVector(this File file) => Constants.FileMasksByFile[file];

        /// <summary>
        /// Returns a vector which is true for each square in the same rank.
        /// </summary>
        public static ulong ToVector(this Rank rank) => Constants.RankMasksByRank[rank];

        /// <summary>
        /// Gets the single square for which this vector is set, or an undefined value if the number of squares in the vector is not equal to one.
        /// </summary>
        public static Square GetSingleSquare(this ulong oneBitVector) => (Square)BitOperations.Log2(oneBitVector);

        /// <summary>
        /// Enumerates all squares for which this vector is set.
        /// </summary>
        public static IEnumerable<Square> AllSquares(this ulong vector)
        {
            foreach (var index in vector.SetBits().Select(BitOperations.Log2))
            {
                yield return (Square)index;
            }
        }

        /// <summary>
        /// Returns if the given <see cref="MoveCheckResult"/> represents a legal move, even if that move is incomplete.
        /// </summary>
        public static bool IsLegalMove(this MoveCheckResult moveCheckResult)
            => (moveCheckResult & MoveCheckResult.IllegalMove) == MoveCheckResult.OK;

        public static ulong North(this ulong vector) => (vector & ~Constants.Rank8) << 8;
        public static ulong South(this ulong vector) => (vector & ~Constants.Rank1) >> 8;
        public static ulong East(this ulong vector) => (vector & ~Constants.FileH) << 1;
        public static ulong West(this ulong vector) => (vector & ~Constants.FileA) >> 1;
    }
}

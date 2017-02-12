/*********************************************************************************
 * ChessTypes.cs
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
using Sandra.Chess;
using System.Collections.Generic;

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
        /// Gets the index of the single bit of the vector, or an undefined value if the number of set bits in the vector is not equal to one.
        /// </summary>
        public static int GetSingleBitIndex(this ulong oneBitVector)
        {
            // Constant masks.
            const ulong m1 = 0x5555555555555555;  // 010101010101...
            const ulong m2 = 0x3333333333333333;  // 001100110011...
            const ulong m4 = 0x0f0f0f0f0f0f0f0f;  // 000011110000...
            const ulong m8 = 0x00ff00ff00ff00ff;
            const ulong m16 = 0x0000ffff0000ffff;
            const ulong m32 = 0x00000000ffffffff;

            // Calculate the index of the single set bit by testing it against several predefined constants.
            // This index is built as a binary value.
            int index = ((oneBitVector & m32) == 0 ? 32 : 0) |
                        ((oneBitVector & m16) == 0 ? 16 : 0) |
                        ((oneBitVector & m8) == 0 ? 8 : 0) |
                        ((oneBitVector & m4) == 0 ? 4 : 0) |
                        ((oneBitVector & m2) == 0 ? 2 : 0) |
                        ((oneBitVector & m1) == 0 ? 1 : 0);

            return index;
        }

        /// <summary>
        /// Enumerates all indices for which the corresponding bit in the vector is true.
        /// </summary>
        public static IEnumerable<int> Indices(this ulong vector)
        {
            while (vector != 0)
            {
                // Select the least significant 1-bit using a well known trick.
                ulong oneBit = vector & (0U - vector);
                yield return GetSingleBitIndex(oneBit);

                // Zero the least significant 1-bit so the index of the next 1-bit can be yielded.
                vector ^= oneBit;
            }
        }

        /// <summary>
        /// Gets the single square for which this vector is set, or an undefined value if the number of squares in the vector is not equal to one.
        /// </summary>
        public static Square GetSingleSquare(this ulong oneBitVector) => (Square)GetSingleBitIndex(oneBitVector);

        /// <summary>
        /// Enumerates all squares for which this vector is set.
        /// </summary>
        public static IEnumerable<Square> AllSquares(this ulong vector)
        {
            foreach (var index in vector.Indices())
            {
                yield return ((Square)index);
            }
        }

        /// <summary>
        /// Tests if a vector has any set bits.
        /// </summary>
        public static bool Test(this ulong vector) => vector != 0;

        /// <summary>
        /// Tests if a vector is equal to zero or a power of two, i.e. is true for zero or one bits exactly.
        /// </summary>
        public static bool IsMaxOneBit(this ulong vector) => !vector.Test(vector - 1);

        /// <summary>
        /// Tests if another vector has any bits in common with this one.
        /// </summary>
        public static bool Test(this ulong vector, ulong otherVector) => (vector & otherVector) != 0;

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

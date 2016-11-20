/*********************************************************************************
 * ChessConstants.cs
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
namespace Sandra.Chess
{
    /// <summary>
    /// Declares several pre-defined bitfield chess constants for all 64 squares. 
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Gets the number of different pieces in the chess game.
        /// </summary>
        public const int PieceCount = 6;

        /// <summary>
        /// Gets the number of squares in a file or rank on a standard chessboard.
        /// </summary>
        public const int SquareCount = 8;

        public const ulong A1 = 0x1;
        public const ulong B1 = 0x2;
        public const ulong C1 = 0x4;
        public const ulong D1 = 0x8;
        public const ulong E1 = 0x10;
        public const ulong F1 = 0x20;
        public const ulong G1 = 0x40;
        public const ulong H1 = 0x80;

        public const ulong A2 = 0x100;
        public const ulong B2 = 0x200;
        public const ulong C2 = 0x400;
        public const ulong D2 = 0x800;
        public const ulong E2 = 0x1000;
        public const ulong F2 = 0x2000;
        public const ulong G2 = 0x4000;
        public const ulong H2 = 0x8000;

        public const ulong A3 = 0x10000;
        public const ulong B3 = 0x20000;
        public const ulong C3 = 0x40000;
        public const ulong D3 = 0x80000;
        public const ulong E3 = 0x100000;
        public const ulong F3 = 0x200000;
        public const ulong G3 = 0x400000;
        public const ulong H3 = 0x800000;

        public const ulong A4 = 0x1000000;
        public const ulong B4 = 0x2000000;
        public const ulong C4 = 0x4000000;
        public const ulong D4 = 0x8000000;
        public const ulong E4 = 0x10000000;
        public const ulong F4 = 0x20000000;
        public const ulong G4 = 0x40000000;
        public const ulong H4 = 0x80000000;

        public const ulong A5 = 0x100000000;
        public const ulong B5 = 0x200000000;
        public const ulong C5 = 0x400000000;
        public const ulong D5 = 0x800000000;
        public const ulong E5 = 0x1000000000;
        public const ulong F5 = 0x2000000000;
        public const ulong G5 = 0x4000000000;
        public const ulong H5 = 0x8000000000;

        public const ulong A6 = 0x10000000000;
        public const ulong B6 = 0x20000000000;
        public const ulong C6 = 0x40000000000;
        public const ulong D6 = 0x80000000000;
        public const ulong E6 = 0x100000000000;
        public const ulong F6 = 0x200000000000;
        public const ulong G6 = 0x400000000000;
        public const ulong H6 = 0x800000000000;

        public const ulong A7 = 0x1000000000000;
        public const ulong B7 = 0x2000000000000;
        public const ulong C7 = 0x4000000000000;
        public const ulong D7 = 0x8000000000000;
        public const ulong E7 = 0x10000000000000;
        public const ulong F7 = 0x20000000000000;
        public const ulong G7 = 0x40000000000000;
        public const ulong H7 = 0x80000000000000;

        public const ulong A8 = 0x100000000000000;
        public const ulong B8 = 0x200000000000000;
        public const ulong C8 = 0x400000000000000;
        public const ulong D8 = 0x800000000000000;
        public const ulong E8 = 0x1000000000000000;
        public const ulong F8 = 0x2000000000000000;
        public const ulong G8 = 0x4000000000000000;
        public const ulong H8 = 0x8000000000000000;

        public const ulong Rank1 = A1 | B1 | C1 | D1 | E1 | F1 | G1 | H1;
        public const ulong Rank2 = A2 | B2 | C2 | D2 | E2 | F2 | G2 | H2;
        public const ulong Rank3 = A3 | B3 | C3 | D3 | E3 | F3 | G3 | H3;
        public const ulong Rank4 = A4 | B4 | C4 | D4 | E4 | F4 | G4 | H4;
        public const ulong Rank5 = A5 | B5 | C5 | D5 | E5 | F5 | G5 | H5;
        public const ulong Rank6 = A6 | B6 | C6 | D6 | E6 | F6 | G6 | H6;
        public const ulong Rank7 = A7 | B7 | C7 | D7 | E7 | F7 | G7 | H7;
        public const ulong Rank8 = A8 | B8 | C8 | D8 | E8 | F8 | G8 | H8;

        public const ulong FileA = A1 | A2 | A3 | A4 | A5 | A6 | A7 | A8;
        public const ulong FileB = B1 | B2 | B3 | B4 | B5 | B6 | B7 | B8;
        public const ulong FileC = C1 | C2 | C3 | C4 | C5 | C6 | C7 | C8;
        public const ulong FileD = D1 | D2 | D3 | D4 | D5 | D6 | D7 | D8;
        public const ulong FileE = E1 | E2 | E3 | E4 | E5 | E6 | E7 | E8;
        public const ulong FileF = F1 | F2 | F3 | F4 | F5 | F6 | F7 | F8;
        public const ulong FileG = G1 | G2 | G3 | G4 | G5 | G6 | G7 | G8;
        public const ulong FileH = H1 | H2 | H3 | H4 | H5 | H6 | H7 | H8;

        public const ulong KingsInStartPosition = E1 | E8;
        public const ulong QueensInStartPosition = D1 | D8;
        public const ulong RooksInStartPosition = A1 | A8 | H1 | H8;
        public const ulong BishopsInStartPosition = C1 | F1 | C8 | F8;
        public const ulong KnightsInStartPosition = B1 | G1 | B8 | G8;
        public const ulong PawnsInStartPosition = Rank2 | Rank7;
        public const ulong WhiteInStartPosition = Rank1 | Rank2;
        public const ulong BlackInStartPosition = Rank7 | Rank8;

        public static readonly EnumIndexedArray<Square, ulong> KnightMoves;

        static Constants()
        {
            KnightMoves = EnumIndexedArray<Square, ulong>.New();

            for (Square sq = Square.H8; sq >= Square.A1; --sq)
            {
                ulong sqVector = sq.ToVector();

                KnightMoves[sq]
                    = (sqVector & ~Rank8 & ~Rank7 & ~FileA) << 15  // NNW
                    | (sqVector & ~Rank8 & ~Rank7 & ~FileH) << 17  // NNE
                    | (sqVector & ~Rank8 & ~FileG & ~FileH) << 10  // ENE
                    | (sqVector & ~Rank1 & ~FileG & ~FileH) >> 6   // ESE
                    | (sqVector & ~Rank1 & ~Rank2 & ~FileH) >> 15  // SSE
                    | (sqVector & ~Rank1 & ~Rank2 & ~FileA) >> 17  // SSW
                    | (sqVector & ~Rank1 & ~FileB & ~FileA) >> 10  // WSW
                    | (sqVector & ~Rank8 & ~FileB & ~FileA) << 6;  // WNW
            }
        }
    }
}

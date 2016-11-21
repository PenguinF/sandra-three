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

        /// <summary>
        /// Gets the total number of squares on a standard chessboard.
        /// </summary>
        public const int TotalSquareCount = SquareCount * SquareCount;

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

        public const ulong FileA = A1 | A2 | A3 | A4 | A5 | A6 | A7 | A8;
        public const ulong FileB = B1 | B2 | B3 | B4 | B5 | B6 | B7 | B8;
        public const ulong FileC = C1 | C2 | C3 | C4 | C5 | C6 | C7 | C8;
        public const ulong FileD = D1 | D2 | D3 | D4 | D5 | D6 | D7 | D8;
        public const ulong FileE = E1 | E2 | E3 | E4 | E5 | E6 | E7 | E8;
        public const ulong FileF = F1 | F2 | F3 | F4 | F5 | F6 | F7 | F8;
        public const ulong FileG = G1 | G2 | G3 | G4 | G5 | G6 | G7 | G8;
        public const ulong FileH = H1 | H2 | H3 | H4 | H5 | H6 | H7 | H8;

        public const ulong Rank1 = A1 | B1 | C1 | D1 | E1 | F1 | G1 | H1;
        public const ulong Rank2 = A2 | B2 | C2 | D2 | E2 | F2 | G2 | H2;
        public const ulong Rank3 = A3 | B3 | C3 | D3 | E3 | F3 | G3 | H3;
        public const ulong Rank4 = A4 | B4 | C4 | D4 | E4 | F4 | G4 | H4;
        public const ulong Rank5 = A5 | B5 | C5 | D5 | E5 | F5 | G5 | H5;
        public const ulong Rank6 = A6 | B6 | C6 | D6 | E6 | F6 | G6 | H6;
        public const ulong Rank7 = A7 | B7 | C7 | D7 | E7 | F7 | G7 | H7;
        public const ulong Rank8 = A8 | B8 | C8 | D8 | E8 | F8 | G8 | H8;

        public const ulong DiagA8 = A8;
        public const ulong DiagA7B8 = A7 | B8;
        public const ulong DiagA6C8 = A6 | B7 | C8;
        public const ulong DiagA5D8 = A5 | B6 | C7 | D8;
        public const ulong DiagA4E8 = A4 | B5 | C6 | D7 | E8;
        public const ulong DiagA3F8 = A3 | B4 | C5 | D6 | E7 | F8;
        public const ulong DiagA2G8 = A2 | B3 | C4 | D5 | E6 | F7 | G8;
        public const ulong DiagA1H8 = A1 | B2 | C3 | D4 | E5 | F6 | G7 | H8;
        public const ulong DiagB1H7 = B1 | C2 | D3 | E4 | F5 | G6 | H7;
        public const ulong DiagC1H6 = C1 | D2 | E3 | F4 | G5 | H6;
        public const ulong DiagD1H5 = D1 | E2 | F3 | G4 | H5;
        public const ulong DiagE1H4 = E1 | F2 | G3 | H4;
        public const ulong DiagF1H3 = F1 | G2 | H3;
        public const ulong DiagG1H2 = G1 | H2;
        public const ulong DiagH1 = H1;

        public const ulong DiagA1 = A1;
        public const ulong DiagA2B1 = A2 | B1;
        public const ulong DiagA3C1 = A3 | B2 | C1;
        public const ulong DiagA4D1 = A4 | B3 | C2 | D1;
        public const ulong DiagA5E1 = A5 | B4 | C3 | D2 | E1;
        public const ulong DiagA6F1 = A6 | B5 | C4 | D3 | E2 | F1;
        public const ulong DiagA7G1 = A7 | B6 | C5 | D4 | E3 | F2 | G1;
        public const ulong DiagA8H1 = A8 | B7 | C6 | D5 | E4 | F3 | G2 | H1;
        public const ulong DiagB8H2 = B8 | C7 | D6 | E5 | F4 | G3 | H2;
        public const ulong DiagC8H3 = C8 | D7 | E6 | F5 | G4 | H3;
        public const ulong DiagD8H4 = D8 | E7 | F6 | G5 | H4;
        public const ulong DiagE8H5 = E8 | F7 | G6 | H5;
        public const ulong DiagF8H6 = F8 | G7 | H6;
        public const ulong DiagG8H7 = G8 | H7;
        public const ulong DiagH8 = H8;

        public const ulong KingsInStartPosition = E1 | E8;
        public const ulong QueensInStartPosition = D1 | D8;
        public const ulong RooksInStartPosition = A1 | A8 | H1 | H8;
        public const ulong BishopsInStartPosition = C1 | F1 | C8 | F8;
        public const ulong KnightsInStartPosition = B1 | G1 | B8 | G8;
        public const ulong PawnsInStartPosition = Rank2 | Rank7;
        public const ulong WhiteInStartPosition = Rank1 | Rank2;
        public const ulong BlackInStartPosition = Rank7 | Rank8;

        public const ulong PromotionSquares = Rank1 | Rank8;

        /// <summary>
        /// Contains a bitfield which is true for each square in the same file as a given square.
        /// </summary>
        public static readonly EnumIndexedArray<Square, ulong> FileMasks;

        /// <summary>
        /// Contains a bitfield which is true for each square in the same file as a given square, except the two outermost squares.
        /// </summary>
        public static readonly EnumIndexedArray<Square, ulong> InnerFileMasks;

        /// <summary>
        /// Contains a bitfield which is true for each square in the same rank as a given square.
        /// </summary>
        public static readonly EnumIndexedArray<Square, ulong> RankMasks;

        /// <summary>
        /// Contains a bitfield which is true for each square in the same rank as a given square, except the two outermost squares.
        /// </summary>
        public static readonly EnumIndexedArray<Square, ulong> InnerRankMasks;

        /// <summary>
        /// Contains a bitfield which is true for each square where a pawn of a given color on a given square can move to.
        /// </summary>
        public static readonly ColorSquareIndexedArray<ulong> PawnMoves;

        /// <summary>
        /// Contains a bitfield which is true for each square where a pawn of a given color on a given square can capture a piece of the opposite color.
        /// </summary>
        public static readonly ColorSquareIndexedArray<ulong> PawnCaptures;

        /// <summary>
        /// Contains a bitfield which is true for each target square where a knight can jump to from a given square.
        /// </summary>
        public static readonly EnumIndexedArray<Square, ulong> KnightMoves;

        // Occupancy tables.
        private const int totalOccupancies = 1 << 6;
        private static readonly ulong[,] fileOccupancy;
        private static readonly ulong[,] rankOccupancy;
        private static readonly EnumIndexedArray<Square, ulong> fileMultipliers;
        private static readonly EnumIndexedArray<Square, int> rankShifts;

        static Constants()
        {
            FileMasks = EnumIndexedArray<Square, ulong>.New();
            InnerFileMasks = EnumIndexedArray<Square, ulong>.New();
            RankMasks = EnumIndexedArray<Square, ulong>.New();
            InnerRankMasks = EnumIndexedArray<Square, ulong>.New();

            PawnMoves = ColorSquareIndexedArray<ulong>.New();
            PawnCaptures = ColorSquareIndexedArray<ulong>.New();
            KnightMoves = EnumIndexedArray<Square, ulong>.New();

            fileMultipliers = EnumIndexedArray<Square, ulong>.New();
            rankShifts = EnumIndexedArray<Square, int>.New();

            ulong[] allFiles = new ulong[] { FileA, FileB, FileC, FileD, FileE, FileF, FileG, FileH };
            ulong[] allRanks = new ulong[] { Rank1, Rank2, Rank3, Rank4, Rank5, Rank6, Rank7, Rank8 };

            ulong[] fileMultipliers8 = new ulong[]
            {
                0x8040201008040200, //A
                0x4020100804020100, //B
                0x2010080402010080, //C
                0x1008040201008040, //D
                0x0804020100804020, //E
                0x0402010080402010, //F
                0x0201008040201008, //G
                0x0100804020100804  //H
            };

            ulong[] a1h8Multipliers8 = new ulong[]
            {
                0,                  //A8
                0,                  //A7-B8
                0x0101010101010100, //A6-C8
                0x0101010101010100, //A5-D8
                0x0101010101010100, //A4-E8
                0x0101010101010100, //A3-F8
                0x0101010101010100, //A2-G8
                0x0101010101010100, //A1-H8
                0x8080808080808000, //B1-H7
                0x4040404040400000, //C1-H6
                0x2020202020000000, //D1-H5
                0x1010101000000000, //E1-H4
                0x0808080000000000, //F1-H3
                0,                  //G1-H2
                0                   //H1
            };

            ulong[] a8h1Multipliers8 = new ulong[]
            {
                0,                  //A1
                0,                  //A2-B1
                0x0101010101010100, //A3-C1
                0x0101010101010100, //A4-D1
                0x0101010101010100, //A5-E1
                0x0101010101010100, //A6-F1
                0x0101010101010100, //A7-G1
                0x0101010101010100, //A8-H1
                0x0080808080808080, //B8-H2
                0x0040404040404040, //C8-H3
                0x0020202020202020, //D8-H4
                0x0010101010101010, //E8-H5
                0x0008080808080808, //F8-H6
                0,                  //G8-H7
                0                   //H8
            };

            for (Square sq = Square.H8; sq >= Square.A1; --sq)
            {
                ulong sqVector = sq.ToVector();
                int x = sq.X();
                int y = sq.Y();

                FileMasks[sq] = allFiles[x];
                InnerFileMasks[sq] = FileMasks[sq] & ~Rank1 & ~Rank8;
                RankMasks[sq] = allRanks[y];
                InnerRankMasks[sq] = RankMasks[sq] & ~FileA & ~FileH;
                fileMultipliers[sq] = fileMultipliers8[x];
                rankShifts[sq] = y * 8 + 1;

                PawnMoves[Color.White, sq] = (sqVector & ~Rank8) << 8
                                           | (sqVector & Rank2) << 16;   // 2 squares ahead allowed from the starting square.
                PawnMoves[Color.Black, sq] = (sqVector & ~Rank1) >> 8
                                           | (sqVector & Rank7) >> 16;

                PawnCaptures[Color.White, sq] = (sqVector & ~Rank8 & ~FileA) << 7
                                              | (sqVector & ~Rank8 & ~FileH) << 9;
                PawnCaptures[Color.Black, sq] = (sqVector & ~Rank1 & ~FileA) >> 9
                                              | (sqVector & ~Rank1 & ~FileH) >> 7;

                KnightMoves[sq] = (sqVector & ~Rank8 & ~Rank7 & ~FileA) << 15  // NNW
                                | (sqVector & ~Rank8 & ~Rank7 & ~FileH) << 17  // NNE
                                | (sqVector & ~Rank8 & ~FileG & ~FileH) << 10  // ENE
                                | (sqVector & ~Rank1 & ~FileG & ~FileH) >> 6   // ESE
                                | (sqVector & ~Rank1 & ~Rank2 & ~FileH) >> 15  // SSE
                                | (sqVector & ~Rank1 & ~Rank2 & ~FileA) >> 17  // SSW
                                | (sqVector & ~Rank1 & ~FileB & ~FileA) >> 10  // WSW
                                | (sqVector & ~Rank8 & ~FileB & ~FileA) << 6;  // WNW

            }

            // Occupancy calculation is done in a few stages.
            // Given are a bitfield of occupied squares, a source square for which to calculate sliding moves,
            // and a direction, N-S, W-E, NW-SE or SW-NE.
            // Example: take a bitfield in which a white queen is on E1, a black king on C3, and a white king on G8.
            // We'd like to calculate whether or not the black king is in check, so:
            // source square = E1, direction = NW-SE.
            //
            // A) Zero out everything that's not on the file/rank/diagonal to consider.
            //    In the example, only E1, D2, C3, B4, A5 are relevant, so square G8 is zeroed out.
            // B) Also ignore the two edge squares, because no further squares can be blocked.
            //    In the example, this means that square E1 is zeroed out as well.
            //    Only six bits of information remain.
            // C) Multiply by a 'magic' value to map and rotate the value onto the eighth rank.
            //    This will generate garbage on the other seven ranks.
            // D) Shift right by 57 positions to move the value to the first rank instead. This also removes the garbage.
            //    In the example, the result binary is: 000010
            //    Had B4 been occupied as well, the result would have been: 000110
            //    And had D2 been occupied as well, the result would have been: 000111
            // E) Given the source square and the 6-bit occupancy index, convert into a bitfield of reachable squares.
            //    The end result of the example then is a bitfield in which bits are set for C3 and D2.
            //
            // Note that the color of the piece occupying a square can be safely ignored,
            // because all squares occupied by pieces of the same colour can be zeroed out afterwards.

            fileOccupancy = new ulong[TotalSquareCount, totalOccupancies];
            rankOccupancy = new ulong[TotalSquareCount, totalOccupancies];

            for (Square sq = Square.H8; sq >= Square.A1; --sq)
            {
                int x = sq.X();
                int y = sq.Y();

                for (int o = totalOccupancies - 1; o >= 0; --o)
                {
                    fileOccupancy[(int)sq, o] = allFiles[x];
                    rankOccupancy[(int)sq, o] = allRanks[y];
                }
            }
        }

        private static int occupancyIndex_File(Square square, ulong occupied)
        {
            return (int)(((InnerFileMasks[square] & occupied) * fileMultipliers[square]) >> 57);
        }

        private static int occupancyIndex_Rank(Square square, ulong occupied)
        {
            // rankMultiplier[square], if it existed, would map the rank to the seventh rank by a simple shift left,
            // after which a '>> 57' would be correct here too.
            // Both operations can be combined into a single shift.
            return (int)((InnerRankMasks[square] & occupied) >> rankShifts[square]);
        }

        public static ulong ReachableSquaresStraight(Square square, ulong occupied)
        {
            return fileOccupancy[(int)square, occupancyIndex_File(square, occupied)]
                 | rankOccupancy[(int)square, occupancyIndex_Rank(square, occupied)];
        }

        /// <summary>
        /// Forces runtime precalculation lookup tables.
        /// </summary>
        public static void ForceInitialize()
        {
        }
    }
}

﻿#region License
/*********************************************************************************
 * ChessConstants.cs
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

namespace Sandra.Chess
{
    /// <summary>
    /// Declares several constant or pre-calculated bitfield vectors which contain information
    /// about a single true-false property for all 64 squares. 
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

        public const ulong RooksStartPositionQueenside = A1 | A8;
        public const ulong RooksStartPositionKingside = H1 | H8;

        public const ulong KingsInStartPosition = E1 | E8;
        public const ulong QueensInStartPosition = D1 | D8;
        public const ulong RooksInStartPosition = RooksStartPositionQueenside | RooksStartPositionKingside;
        public const ulong BishopsInStartPosition = C1 | F1 | C8 | F8;
        public const ulong KnightsInStartPosition = B1 | G1 | B8 | G8;
        public const ulong PawnsInStartPosition = Rank2 | Rank7;
        public const ulong WhiteInStartPosition = Rank1 | Rank2;
        public const ulong BlackInStartPosition = Rank7 | Rank8;

        public const ulong PromotionSquares = Rank1 | Rank8;

        public const ulong CastlingTargetSquares = C1 | G1 | C8 | G8;
        public const ulong KingsideCastlingTargetSquares = G1 | G8;

        /// <summary>
        /// Contains a vector which encodes the movement of a rook during a queenside castling move.
        /// </summary>
        public static readonly EnumIndexedArray<Color, ulong> CastleQueensideRookDelta;

        /// <summary>
        /// Contains a vector which encodes the movement of a rook during a kingside castling move.
        /// </summary>
        public static readonly EnumIndexedArray<Color, ulong> CastleKingsideRookDelta;

        /// <summary>
        /// Contains a vector which is true for each square in the same file as a given square.
        /// </summary>
        public static readonly EnumIndexedArray<Square, ulong> FileMasks;

        /// <summary>
        /// Contains a vector which is true for each square in the same file as a given file.
        /// </summary>
        public static readonly EnumIndexedArray<File, ulong> FileMasksByFile;

        /// <summary>
        /// Contains a vector which is true for each square in the same file as a given square, except the two outermost squares.
        /// </summary>
        public static readonly EnumIndexedArray<Square, ulong> InnerFileMasks;

        /// <summary>
        /// Contains a vector which is true for each square in the same rank as a given square.
        /// </summary>
        public static readonly EnumIndexedArray<Square, ulong> RankMasks;

        /// <summary>
        /// Contains a vector which is true for each square in the same rank as a given square.
        /// </summary>
        public static readonly EnumIndexedArray<Rank, ulong> RankMasksByRank;

        /// <summary>
        /// Contains a vector which is true for each square in the same rank as a given square, except the two outermost squares.
        /// </summary>
        public static readonly EnumIndexedArray<Square, ulong> InnerRankMasks;

        /// <summary>
        /// Contains a vector which is true for each square in the same SW-NE diagonal as a given square.
        /// </summary>
        public static readonly EnumIndexedArray<Square, ulong> A1H8Masks;

        /// <summary>
        /// Contains a vector which is true for each square in the same SW-NE diagonal as a given square, except the two outermost squares.
        /// </summary>
        public static readonly EnumIndexedArray<Square, ulong> InnerA1H8Masks;

        /// <summary>
        /// Contains a vector which is true for each square in the same NW-SE diagonal as a given square.
        /// </summary>
        public static readonly EnumIndexedArray<Square, ulong> A8H1Masks;

        /// <summary>
        /// Contains a vector which is true for each square in the same NW-SE diagonal as a given square, except the two outermost squares.
        /// </summary>
        public static readonly EnumIndexedArray<Square, ulong> InnerA8H1Masks;

        /// <summary>
        /// Contains a vector which is true for each square where a pawn of a given color on a given square can move to.
        /// </summary>
        public static readonly ColorSquareIndexedArray<ulong> PawnMoves;

        /// <summary>
        /// Contains a vector which is true for each square where a pawn of a given color on a given square can capture a piece of the opposite color.
        /// </summary>
        public static readonly ColorSquareIndexedArray<ulong> PawnCaptures;

        /// <summary>
        /// Contains a vector which is true for each square in front of a pawn of a given color in the starting position.
        /// </summary>
        public static readonly ColorSquareIndexedArray<ulong> EnPassantSquares;

        /// <summary>
        /// Contains a vector which is true for each square that is two squares ahead of a pawn of a given color in the starting position.
        /// </summary>
        public static readonly ColorSquareIndexedArray<ulong> PawnTwoSquaresAhead;

        /// <summary>
        /// Contains a vector which is true for each target square where a knight can jump to from a given square.
        /// </summary>
        public static readonly EnumIndexedArray<Square, ulong> KnightMoves;

        /// <summary>
        /// Contains a vector which is true for each target square where a king can move to from a given square.
        /// </summary>
        public static readonly EnumIndexedArray<Square, ulong> Neighbours;

        // Total number of indices is 8 - 2 = 6 bits, since the two outermost squares don't matter for reachability of squares.
        // There are then 2^6 = 64 different ways in which pieces can occupy a single file, rank or diagonal.
        private const int totalOccupancies = 1 << 6;

        private static readonly EnumIndexedArray<Square, ulong> fileMultipliers;
        private static readonly EnumIndexedArray<Square, int> rankShifts;
        private static readonly EnumIndexedArray<Square, ulong> a1h8Multipliers;
        private static readonly EnumIndexedArray<Square, ulong> a8h1Multipliers;

        private static readonly ulong[,] fileReachability;
        private static readonly ulong[,] rankReachability;
        private static readonly ulong[,] a1h8Reachability;
        private static readonly ulong[,] a8h1Reachability;

        static Constants()
        {
            CastleQueensideRookDelta = EnumIndexedArray<Color, ulong>.New();
            CastleKingsideRookDelta = EnumIndexedArray<Color, ulong>.New();

            FileMasks = EnumIndexedArray<Square, ulong>.New();
            FileMasksByFile = EnumIndexedArray<File, ulong>.New();
            InnerFileMasks = EnumIndexedArray<Square, ulong>.New();
            RankMasks = EnumIndexedArray<Square, ulong>.New();
            RankMasksByRank = EnumIndexedArray<Rank, ulong>.New();
            InnerRankMasks = EnumIndexedArray<Square, ulong>.New();
            A1H8Masks = EnumIndexedArray<Square, ulong>.New();
            InnerA1H8Masks = EnumIndexedArray<Square, ulong>.New();
            A8H1Masks = EnumIndexedArray<Square, ulong>.New();
            InnerA8H1Masks = EnumIndexedArray<Square, ulong>.New();

            PawnMoves = ColorSquareIndexedArray<ulong>.New();
            PawnCaptures = ColorSquareIndexedArray<ulong>.New();
            EnPassantSquares = ColorSquareIndexedArray<ulong>.New();
            PawnTwoSquaresAhead = ColorSquareIndexedArray<ulong>.New();
            KnightMoves = EnumIndexedArray<Square, ulong>.New();
            Neighbours = EnumIndexedArray<Square, ulong>.New();

            fileMultipliers = EnumIndexedArray<Square, ulong>.New();
            rankShifts = EnumIndexedArray<Square, int>.New();
            a1h8Multipliers = EnumIndexedArray<Square, ulong>.New();
            a8h1Multipliers = EnumIndexedArray<Square, ulong>.New();

            ulong[] allFiles = { FileA, FileB, FileC, FileD, FileE, FileF, FileG, FileH };
            ulong[] allRanks = { Rank1, Rank2, Rank3, Rank4, Rank5, Rank6, Rank7, Rank8 };
            ulong[] allDiagsA1H8 = { DiagA8, DiagA7B8, DiagA6C8, DiagA5D8, DiagA4E8, DiagA3F8, DiagA2G8, DiagA1H8, DiagB1H7, DiagC1H6, DiagD1H5, DiagE1H4, DiagF1H3, DiagG1H2, DiagH1 };
            ulong[] allDiagsA8H1 = { DiagA1, DiagA2B1, DiagA3C1, DiagA4D1, DiagA5E1, DiagA6F1, DiagA7G1, DiagA8H1, DiagB8H2, DiagC8H3, DiagD8H4, DiagE8H5, DiagF8H6, DiagG8H7, DiagH8 };

            ulong[] fileMultipliers8 =
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

            ulong[] a1h8Multipliers8 =
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

            ulong[] a8h1Multipliers8 =
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

            CastleQueensideRookDelta[Color.White] = A1 | D1;
            CastleQueensideRookDelta[Color.Black] = A8 | D8;
            CastleKingsideRookDelta[Color.White] = H1 | F1;
            CastleKingsideRookDelta[Color.Black] = H8 | F8;

            for (File file = File.A; file <= File.H; file++)
            {
                FileMasksByFile[file] = allFiles[(int)file];
            }

            for (Rank rank = Rank._1; rank <= Rank._8; rank++)
            {
                RankMasksByRank[rank] = allRanks[(int)rank];
            }

            for (Square sq = Square.H8; sq >= Square.A1; --sq)
            {
                ulong sqVector = sq.ToVector();
                int x = sq.X();
                int y = sq.Y();
                int a1h8Index = 7 + x - y;
                int a8h1Index = x + y;

                FileMasks[sq] = allFiles[x];
                InnerFileMasks[sq] = FileMasks[sq] & ~Rank1 & ~Rank8;
                RankMasks[sq] = allRanks[y];
                InnerRankMasks[sq] = RankMasks[sq] & ~FileA & ~FileH;
                A1H8Masks[sq] = allDiagsA1H8[a1h8Index];
                InnerA1H8Masks[sq] = A1H8Masks[sq] & ~FileA & ~FileH & ~Rank1 & ~Rank8;
                A8H1Masks[sq] = allDiagsA8H1[a8h1Index];
                InnerA8H1Masks[sq] = A8H1Masks[sq] & ~FileA & ~FileH & ~Rank1 & ~Rank8;

                fileMultipliers[sq] = fileMultipliers8[x];
                rankShifts[sq] = y * 8 + 1;
                a1h8Multipliers[sq] = a1h8Multipliers8[a1h8Index];
                a8h1Multipliers[sq] = a8h1Multipliers8[a8h1Index];

                PawnTwoSquaresAhead[Color.White, sq] = (sqVector & Rank2).North().North();
                PawnTwoSquaresAhead[Color.Black, sq] = (sqVector & Rank7).South().South();

                PawnMoves[Color.White, sq] = sqVector.North()
                                           | PawnTwoSquaresAhead[Color.White, sq];  // 2 squares ahead allowed from the starting square.
                PawnMoves[Color.Black, sq] = sqVector.South()
                                           | PawnTwoSquaresAhead[Color.Black, sq];

                PawnCaptures[Color.White, sq] = sqVector.North().West()
                                              | sqVector.North().East();
                PawnCaptures[Color.Black, sq] = sqVector.South().West()
                                              | sqVector.South().East();

                EnPassantSquares[Color.White, sq] = (sqVector & Rank2).North();
                EnPassantSquares[Color.Black, sq] = (sqVector & Rank7).South();

                KnightMoves[sq] = sqVector.North().North().West()
                                | sqVector.North().North().East()
                                | sqVector.North().West().West()
                                | sqVector.North().East().East()
                                | sqVector.South().West().West()
                                | sqVector.South().East().East()
                                | sqVector.South().South().West()
                                | sqVector.South().South().East();

                Neighbours[sq] = sqVector.North().East()
                               | sqVector.North()
                               | sqVector.North().West()
                               | sqVector.East()
                               | sqVector.West()
                               | sqVector.South().East()
                               | sqVector.South()
                               | sqVector.South().West();
            }

            // Reachability calculation is done in a few stages.
            // Given are a bitfield of occupied squares, a source square for which to calculate sliding moves,
            // and a direction, N-S, W-E, NW-SE or SW-NE.
            // Example: take a position in which a white queen is on C3, a black king on E1, and a white king on G8.
            // We'd like to calculate whether or not the black king is in check, so:
            // source square = E1, direction = NW-SE, occupied square bitfield = 2^4 (E1) + 2^18 (C3) + 2^62 (G8).
            //
            // A) Zero out everything that's not on the file/rank/diagonal to consider.
            //    In the example, only A5, B4, C3, D2, E1 are relevant, so square G8 is zeroed out.
            //    Also ignore the two edge squares, because no further squares can be blocked.
            //    In the example, this means that square E1 is zeroed out as well.
            //    Only a maximum of six bits of information remain, in the example only three (B4, C3, D2).
            // B) Multiply by a 'magic' value to map and rotate the value onto the eighth rank.
            //    This will generate some garbage on the other seven ranks.
            // C) Shift right by 57 positions to move the value to the first rank instead. This also shifts out the garbage.
            //    In the example, the result binary is: 000010
            //    Had D2 been occupied as well, the result would have been: 000110
            //    And had B4 been occupied as well, the result would have been: 000111
            // D) Given the source square and the 6-bit occupancy index, use a lookup table to obtain a bitfield of reachable
            //    squares. The end result of the example then is a bitfield in which bits are set for C3 and D2.
            //    Since there's a queen is on C3, this means that the black king is indeed in check.
            //
            // Note that the color of the piece occupying a square can be safely ignored,
            // because all squares occupied by pieces of the same color can be zeroed out afterwards.
            // The number of necessary operations is always four: zero out, multiply, shift right, lookup in table.

            // The order in a ray is determined by which bit in the occupancy index corresponds to a given blocking square.
            // This in turn is determined indirectly by the multipliers which are used in step B.
            Square[] baseFile = { Square.A8, Square.A7, Square.A6, Square.A5, Square.A4, Square.A3, Square.A2, Square.A1 };
            ulong[,] baseFileReachability = CalculateRayReachabilityTable(baseFile);
            fileReachability = new ulong[TotalSquareCount, totalOccupancies];

            Square[] baseRank = { Square.A1, Square.B1, Square.C1, Square.D1, Square.E1, Square.F1, Square.G1, Square.H1 };
            ulong[,] baseRankReachability = CalculateRayReachabilityTable(baseRank);
            rankReachability = new ulong[TotalSquareCount, totalOccupancies];

            Square[] baseA1H8 = { Square.A1, Square.B2, Square.C3, Square.D4, Square.E5, Square.F6, Square.G7, Square.H8 };
            ulong[,] baseA1H8Reachability = CalculateRayReachabilityTable(baseA1H8);
            a1h8Reachability = new ulong[TotalSquareCount, totalOccupancies];

            Square[] baseA8H1 = { Square.A8, Square.B7, Square.C6, Square.D5, Square.E4, Square.F3, Square.G2, Square.H1 };
            ulong[,] baseA8H1Reachability = CalculateRayReachabilityTable(baseA8H1);
            a8h1Reachability = new ulong[TotalSquareCount, totalOccupancies];

            // To zero out part of a diagonal before shifting it to another shorter diagonal.
            ulong[] partialDiagMasks =
            {
                ulong.MaxValue,
                ulong.MaxValue & ~FileH,
                ulong.MaxValue & ~FileG & ~FileH,
                ulong.MaxValue & ~FileF & ~FileG & ~FileH,
                ulong.MaxValue & ~FileE & ~FileF & ~FileG & ~FileH,
                ulong.MaxValue & ~FileD & ~FileE & ~FileF & ~FileG & ~FileH,
                ulong.MaxValue & ~FileC & ~FileD & ~FileE & ~FileF & ~FileG & ~FileH,
                ulong.MaxValue & ~FileB & ~FileC & ~FileD & ~FileE & ~FileF & ~FileG & ~FileH,
            };

            for (Square sq = Square.H8; sq >= Square.A1; --sq)
            {
                int x = sq.X();
                int y = sq.Y();
                // Diagonal indexes relative to the long diagonals.
                int a1h8Index = x - y;
                int a8h1Index = x + y - 7;

                for (int o = totalOccupancies - 1; o >= 0; --o)
                {
                    // Copy from the base reachability table, and shift the result to the current rank/file/diagonal.
                    fileReachability[(int)sq, o] = baseFileReachability[7 - y, o] << x;
                    rankReachability[(int)sq, o] = baseRankReachability[x, o] << (y * 8);

                    if (a1h8Index == 0)
                    {
                        a1h8Reachability[(int)sq, o] = baseA1H8Reachability[x, o];
                    }
                    else if (a1h8Index < 0)
                    {
                        ulong a1h8Diag = baseA1H8Reachability[x, o] & partialDiagMasks[-a1h8Index];
                        a1h8Reachability[(int)sq, o] = a1h8Diag << (-a1h8Index * 8);
                    }
                    else
                    {
                        ulong a1h8Diag = baseA1H8Reachability[y, o] & partialDiagMasks[a1h8Index];
                        a1h8Reachability[(int)sq, o] = a1h8Diag << a1h8Index;
                    }

                    if (a8h1Index == 0)
                    {
                        a8h1Reachability[(int)sq, o] = baseA8H1Reachability[x, o];
                    }
                    else if (a8h1Index < 0)
                    {
                        ulong a8h1Diag = baseA8H1Reachability[x, o] & partialDiagMasks[-a8h1Index];
                        a8h1Reachability[(int)sq, o] = a8h1Diag >> (-a8h1Index * 8);
                    }
                    else
                    {
                        ulong a8h1Diag = baseA8H1Reachability[7 - y, o] & partialDiagMasks[a8h1Index];
                        a8h1Reachability[(int)sq, o] = a8h1Diag << a8h1Index;
                    }
                }
            }
        }

        private static ulong[,] CalculateRayReachabilityTable(Square[] ray)
        {
            // Construct a bitfield which is true for all squares in the ray.
            ulong rayVector = 0;
            for (int sqIndex = 0; sqIndex < ray.Length; ++sqIndex)
            {
                rayVector |= ray[sqIndex].ToVector();
            }

            // Initialize all entries to rayVector to indicate that all squares in the ray are reachable.
            ulong[,] reachability = new ulong[ray.Length, totalOccupancies];
            for (int sqIndex = 0; sqIndex < ray.Length; ++sqIndex)
            {
                for (int o = totalOccupancies - 1; o >= 0; --o)
                {
                    reachability[sqIndex, o] = rayVector;
                }
            }

            // Bitfield of reachable squares.
            ulong reachableNear = ray[0].ToVector();

            // Create a block mask which is 1 for the corresponding bit in the occupancy index.
            int blockMask = 1;

            // Skip the outer two squares, because a piece there cannot block any further squares.
            for (int blockingSqIndex = 1; blockingSqIndex < ray.Length - 1; ++blockingSqIndex)
            {
                // Update reachable squares from both ends of the ray.
                ulong reachableFar = ~reachableNear & rayVector;

                // Only now update reachableNear, so reachableFar and reachableNear always have
                // an overlapping bit in the middle. Otherwise a piece would block itself from
                // reaching the other squares in the ray.
                reachableNear |= ray[blockingSqIndex].ToVector();

                // Can skip 0 because (o & blockMask) != 0 is always false.
                for (int o = totalOccupancies - 1; o > 0; --o)
                {
                    // Only update occupancy for indexes where the corresponding bit is blocked.
                    if ((o & blockMask) != 0)
                    {
                        for (int sqIndex = 0; sqIndex < ray.Length; ++sqIndex)
                        {
                            // Zero out unreachable squares.
                            if (sqIndex < blockingSqIndex)
                            {
                                reachability[sqIndex, o] &= reachableNear;
                            }
                            else if (sqIndex > blockingSqIndex)
                            {
                                reachability[sqIndex, o] &= reachableFar;
                            }
                        }
                    }
                }

                blockMask <<= 1;
            }

            return reachability;
        }

        private static int OccupancyIndex_File(Square square, ulong occupied)
        {
            return (int)(((InnerFileMasks[square] & occupied) * fileMultipliers[square]) >> 57);
        }

        private static int OccupancyIndex_Rank(Square square, ulong occupied)
        {
            // rankMultiplier[square], if it existed, would map the rank to the seventh rank by a simple shift left,
            // after which a '>> 57' would be correct here too.
            // Both operations can be combined into a single shift.
            return (int)((InnerRankMasks[square] & occupied) >> rankShifts[square]);
        }

        private static int OccupancyIndex_DiagA1H8(Square square, ulong occupied)
        {
            return (int)(((InnerA1H8Masks[square] & occupied) * a1h8Multipliers[square]) >> 57);
        }

        private static int OccupancyIndex_DiagA8H1(Square square, ulong occupied)
        {
            return (int)(((InnerA8H1Masks[square] & occupied) * a8h1Multipliers[square]) >> 57);
        }

        /// <summary>
        /// Given a vector of occupied squares, returns a vector with all squares which are reachable from a source square with a rook.
        /// </summary>
        public static ulong ReachableSquaresStraight(Square sourceSquare, ulong occupied)
        {
            return fileReachability[(int)sourceSquare, OccupancyIndex_File(sourceSquare, occupied)]
                 | rankReachability[(int)sourceSquare, OccupancyIndex_Rank(sourceSquare, occupied)];
        }

        /// <summary>
        /// Given a vector of occupied squares, returns a vector with all squares which are reachable from a source square with a bishop.
        /// </summary>
        public static ulong ReachableSquaresDiagonal(Square sourceSquare, ulong occupied)
        {
            return a1h8Reachability[(int)sourceSquare, OccupancyIndex_DiagA1H8(sourceSquare, occupied)]
                 | a8h1Reachability[(int)sourceSquare, OccupancyIndex_DiagA8H1(sourceSquare, occupied)];
        }

        /// <summary>
        /// Forces runtime precalculation of lookup tables.
        /// </summary>
        public static void ForceInitialize()
        {
        }
    }
}

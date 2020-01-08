#region License
/*********************************************************************************
 * PositionTests.cs
 *
 * Copyright (c) 2004-2020 Henk Nicolai
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

using Eutherion;
using Xunit;

namespace Sandra.Chess.Tests
{
    public class PositionTests
    {
        /// <summary>
        /// Mirrors an actual <see cref="Position"/> to model its expected behavior.
        /// </summary>
        class ShadowPosition
        {
            enum ColoredPieceOrEmpty
            {
                Empty = -1,
                WhitePawn, WhiteKnight, WhiteBishop, WhiteRook, WhiteQueen, WhiteKing,
                BlackPawn, BlackKnight, BlackBishop, BlackRook, BlackQueen, BlackKing,
            }

            Color sideToMove;
            ColoredPieceOrEmpty[] piecePerSquare;

            public Color SideToMove { get { return sideToMove; } }

            public ulong GetVector(Color color)
            {
                ulong vector = 0;
                ulong indexVector = 1;
                for (int sq = 0; sq < Constants.TotalSquareCount; ++sq)
                {
                    if (piecePerSquare[sq] != ColoredPieceOrEmpty.Empty)
                    {
                        var coloredPiece = (ColoredPiece)piecePerSquare[sq];
                        if (coloredPiece.GetColor() == color) vector |= indexVector;
                    }
                    indexVector <<= 1;
                }
                return vector;
            }

            public ulong GetVector(Piece piece)
            {
                ulong vector = 0;
                ulong indexVector = 1;
                for (int sq = 0; sq < Constants.TotalSquareCount; ++sq)
                {
                    if (piecePerSquare[sq] != ColoredPieceOrEmpty.Empty)
                    {
                        var coloredPiece = (ColoredPiece)piecePerSquare[sq];
                        if (coloredPiece.GetPiece() == piece) vector |= indexVector;
                    }
                    indexVector <<= 1;
                }
                return vector;
            }

            private ShadowPosition()
            {
                piecePerSquare = new ColoredPieceOrEmpty[Constants.TotalSquareCount];
                piecePerSquare.Fill(ColoredPieceOrEmpty.Empty);
            }

            /// <summary>
            /// Returns the standard initial position.
            /// </summary>
            public static ShadowPosition GetInitialPosition()
            {
                var initialPosition = new ShadowPosition
                {
                    sideToMove = Color.White
                };

                initialPosition.piecePerSquare[0] = ColoredPieceOrEmpty.WhiteRook;
                initialPosition.piecePerSquare[1] = ColoredPieceOrEmpty.WhiteKnight;
                initialPosition.piecePerSquare[2] = ColoredPieceOrEmpty.WhiteBishop;
                initialPosition.piecePerSquare[3] = ColoredPieceOrEmpty.WhiteQueen;
                initialPosition.piecePerSquare[4] = ColoredPieceOrEmpty.WhiteKing;
                initialPosition.piecePerSquare[5] = ColoredPieceOrEmpty.WhiteBishop;
                initialPosition.piecePerSquare[6] = ColoredPieceOrEmpty.WhiteKnight;
                initialPosition.piecePerSquare[7] = ColoredPieceOrEmpty.WhiteRook;

                for (int sq = 8; sq < 16; ++sq) initialPosition.piecePerSquare[sq] = ColoredPieceOrEmpty.WhitePawn;

                for (int sq = 48; sq < 56; ++sq) initialPosition.piecePerSquare[sq] = ColoredPieceOrEmpty.BlackPawn;

                initialPosition.piecePerSquare[56] = ColoredPieceOrEmpty.BlackRook;
                initialPosition.piecePerSquare[57] = ColoredPieceOrEmpty.BlackKnight;
                initialPosition.piecePerSquare[58] = ColoredPieceOrEmpty.BlackBishop;
                initialPosition.piecePerSquare[59] = ColoredPieceOrEmpty.BlackQueen;
                initialPosition.piecePerSquare[60] = ColoredPieceOrEmpty.BlackKing;
                initialPosition.piecePerSquare[61] = ColoredPieceOrEmpty.BlackBishop;
                initialPosition.piecePerSquare[62] = ColoredPieceOrEmpty.BlackKnight;
                initialPosition.piecePerSquare[63] = ColoredPieceOrEmpty.BlackRook;

                return initialPosition;
            }
        }

        [Fact]
        public void InitialPosition()
        {
            ShadowPosition expectedPosition = ShadowPosition.GetInitialPosition();
            Position position = Position.GetInitialPosition();

            Assert.Equal(expectedPosition.SideToMove, position.SideToMove);

            Assert.Equal(expectedPosition.GetVector(Color.White), position.GetVector(Color.White));
            Assert.Equal(expectedPosition.GetVector(Color.Black), position.GetVector(Color.Black));

            Assert.Equal(expectedPosition.GetVector(Piece.Pawn), position.GetVector(Piece.Pawn));
            Assert.Equal(expectedPosition.GetVector(Piece.Knight), position.GetVector(Piece.Knight));
            Assert.Equal(expectedPosition.GetVector(Piece.Bishop), position.GetVector(Piece.Bishop));
            Assert.Equal(expectedPosition.GetVector(Piece.Rook), position.GetVector(Piece.Rook));
            Assert.Equal(expectedPosition.GetVector(Piece.Queen), position.GetVector(Piece.Queen));
            Assert.Equal(expectedPosition.GetVector(Piece.King), position.GetVector(Piece.King));
        }
    }
}

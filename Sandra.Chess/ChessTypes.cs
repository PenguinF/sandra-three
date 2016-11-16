/*********************************************************************************
 * ChessTypes.cs
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
    /// <see cref="NonEmptyColoredPiece"/> is a combination of the <see cref="Piece"/> and <see cref="Color"/> enumerations.
    /// </summary>
    public enum NonEmptyColoredPiece
    {
        WhitePawn, WhiteKnight, WhiteBishop, WhiteRook, WhiteQueen, WhiteKing,
        BlackPawn, BlackKnight, BlackBishop, BlackRook, BlackQueen, BlackKing,
    }

    /// <summary>
    /// Specifies all thirteen possible states of a square.
    /// <see cref="ColoredPiece"/> is a combination of the <see cref="Piece"/> and <see cref="Color"/> enumerations, with an extra <see cref="Empty"/> value.
    /// </summary>
    public enum ColoredPiece
    {
        Empty = -1,
        WhitePawn, WhiteKnight, WhiteBishop, WhiteRook, WhiteQueen, WhiteKing,
        BlackPawn, BlackKnight, BlackBishop, BlackRook, BlackQueen, BlackKing,
    }
}

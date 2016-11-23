/*********************************************************************************
 * Move.cs
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
using System;

namespace Sandra.Chess
{
    public enum MoveType
    {
        Default,
        Promotion,
        EnPassant,
        CastleQueenside,
        CastleKingside,
    }

    public class Move
    {
        public MoveType MoveType;
        public Square SourceSquare;
        public Square TargetSquare;
        public Piece PromoteTo;

        public void ThrowWhenOutOfRange()
        {
            if (MoveType < 0 || MoveType > MoveType.CastleKingside)
            {
                throw new ArgumentOutOfRangeException(nameof(MoveType));
            }
            if (SourceSquare < 0 || SourceSquare > Square.H8)
            {
                throw new ArgumentOutOfRangeException(nameof(SourceSquare));
            }
            if (TargetSquare < 0 || TargetSquare > Square.H8)
            {
                throw new ArgumentOutOfRangeException(nameof(TargetSquare));
            }
            if (PromoteTo < 0 || PromoteTo > Piece.King)
            {
                throw new ArgumentOutOfRangeException(nameof(PromoteTo));
            }
        }
    }
}

#region License
/*********************************************************************************
 * MoveCheckResult.cs
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

namespace Sandra.Chess
{
    /// <summary>
    /// Enumerates all possible results of <see cref="Position.TryMakeMove(MoveInfo, bool)"/>.
    /// </summary>
    [Flags]
    public enum MoveCheckResult
    {
        /// <summary>
        /// The move is valid in the current position.
        /// </summary>
        OK,
        /// <summary>
        /// The given source and target squares are the same.
        /// </summary>
        SourceSquareIsTargetSquare = 1,
        /// <summary>
        /// There is no piece on the source square.
        /// </summary>
        SourceSquareIsEmpty = 2,
        /// <summary>
        /// The piece on the source square does not belong to the side to move.
        /// </summary>
        NotSideToMove = 4,
        /// <summary>
        /// The move would result in a capture of a piece of the same color as the moving piece.
        /// </summary>
        CannotCaptureOwnPiece = 8,
        /// <summary>
        /// The target square is not a legal destination for the moving piece in the current position.
        /// </summary>
        IllegalTargetSquare = 16,
        /// <summary>
        /// <see cref="MoveType.Promotion"/> was specified for a move which does not promote a pawn.
        /// </summary>
        IllegalMoveTypePromotion = 32,
        /// <summary>
        /// <see cref="MoveType.EnPassant"/> was specified for a move which does not capture a pawn en passant.
        /// </summary>
        IllegalMoveTypeEnPassant = 64,
        /// <summary>
        /// <see cref="MoveType.CastleQueenside"/> was specified for a non-castling move.
        /// </summary>
        IllegalMoveTypeCastleQueenside = 128,
        /// <summary>
        /// <see cref="MoveType.CastleKingside"/> was specified for a non-castling move.
        /// </summary>
        IllegalMoveTypeCastleKingside = 256,
        /// <summary>
        /// Making the move would put the friendly king in check.
        /// </summary>
        FriendlyKingInCheck = 512,

        /// <summary>
        /// Mask which selects all flags which denote an illegal move.
        /// </summary>
        IllegalMove = 1023,

        /// <summary>
        /// A move which promotes a pawn does not specify <see cref="MoveType.Promotion"/>, and/or the promotion piece is a pawn or king.
        /// </summary>
        MissingPromotionInformation = 1024,
        /// <summary>
        /// A move which captures a pawn en passant does not specify <see cref="MoveType.EnPassant"/>.
        /// </summary>
        MissingEnPassant = 2048,
        /// <summary>
        /// A castling move does not specify <see cref="MoveType.CastleQueenside"/>.
        /// </summary>
        MissingCastleQueenside = 4096,
        /// <summary>
        /// A castling move does not specify <see cref="MoveType.CastleKingside"/>.
        /// </summary>
        MissingCastleKingside = 8192,

        /// <summary>
        /// Mask which selects all flags which denote an incomplete move.
        /// </summary>
        IncompleteMove = MissingPromotionInformation | MissingEnPassant | MissingCastleQueenside | MissingCastleKingside,
    }
}

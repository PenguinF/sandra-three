/*********************************************************************************
 * Position.cs
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
using System.Diagnostics;
using System.Linq;

namespace Sandra.Chess
{
    /// <summary>
    /// Represents a position in a standard chess game.
    /// </summary>
    public class Position
    {
        private Color sideToMove;

        private EnumIndexedArray<Color, ulong> colorVectors;
        private EnumIndexedArray<Piece, ulong> pieceVectors;
        private ulong enPassantVector;
        private ulong enPassantCaptureVector;
        private ulong castlingRightsVector;

        private bool checkInvariants()
        {
            // Disjunct colors.
            if (colorVectors[Color.White].Test(colorVectors[Color.Black])) return false;
            ulong occupied = colorVectors[Color.White] | colorVectors[Color.Black];

            // Disjunct pieces.
            foreach (Piece piece1 in EnumHelper<Piece>.AllValues)
            {
                for (Piece piece2 = piece1 + 1; piece2 <= Piece.King; ++piece2)
                {
                    if (pieceVectors[piece1].Test(pieceVectors[piece2])) return false;
                }
            }

            // Colors and pieces must match exactly.
            if (occupied != (
                    pieceVectors[Piece.Pawn]
                  | pieceVectors[Piece.Knight]
                  | pieceVectors[Piece.Bishop]
                  | pieceVectors[Piece.Rook]
                  | pieceVectors[Piece.Queen]
                  | pieceVectors[Piece.King])) return false;

            // There is exactly one king on both sides.
            ulong whiteKing = colorVectors[Color.White] & pieceVectors[Piece.King];
            if (!whiteKing.Test()) return false;
            if (!whiteKing.IsMaxOneBit()) return false;
            ulong blackKing = colorVectors[Color.Black] & pieceVectors[Piece.King];
            if (!blackKing.Test()) return false;
            if (!blackKing.IsMaxOneBit()) return false;

            // The enemy king cannot be in check.
            Square enemyKing = FindKing(sideToMove.Opposite());
            if (IsSquareUnderAttack(enemyKing, sideToMove.Opposite())) return false;

            // Pawns cannot be on the back rank.
            if (pieceVectors[Piece.Pawn].Test(Constants.Rank1 | Constants.Rank8)) return false;

            // Castling rights can only be set if king/rooks are in their starting position.
            if (castlingRightsVector.Test(Constants.C8))
            {
                if (!blackKing.Test(Constants.E8)) return false;
                if (!Constants.A8.Test(colorVectors[Color.Black] & pieceVectors[Piece.Rook])) return false;
            }
            if (castlingRightsVector.Test(Constants.G8))
            {
                if (!blackKing.Test(Constants.E8)) return false;
                if (!Constants.H8.Test(colorVectors[Color.Black] & pieceVectors[Piece.Rook])) return false;
            }
            if (castlingRightsVector.Test(Constants.C1))
            {
                if (!whiteKing.Test(Constants.E1)) return false;
                if (!Constants.A1.Test(colorVectors[Color.White] & pieceVectors[Piece.Rook])) return false;
            }
            if (castlingRightsVector.Test(Constants.G1))
            {
                if (!whiteKing.Test(Constants.E1)) return false;
                if (!Constants.H1.Test(colorVectors[Color.White] & pieceVectors[Piece.Rook])) return false;
            }

            // En passant invariants.
            if (!enPassantVector.Test())
            {
                if (enPassantCaptureVector.Test()) return false;
            }
            else
            {
                // enPassantVector must be empty.
                if (occupied.Test(enPassantVector)) return false;

                if (Constants.Rank4.Test(enPassantCaptureVector))
                {
                    // There must be a white pawn on the en passant capture square.
                    if (!enPassantCaptureVector.Test(colorVectors[Color.White] & pieceVectors[Piece.Pawn])) return false;
                    // enPassantVector must be directly south.
                    if (enPassantCaptureVector.South() != enPassantVector) return false;
                    // Starting square must be empty.
                    if (occupied.Test(enPassantCaptureVector.South().South())) return false;
                }
                else if (Constants.Rank5.Test(enPassantCaptureVector))
                {
                    // There must be a black pawn on the en passant capture square.
                    if (!enPassantCaptureVector.Test(colorVectors[Color.Black] & pieceVectors[Piece.Pawn])) return false;
                    // enPassantVector must be directly north.
                    if (enPassantCaptureVector.North() != enPassantVector) return false;
                    // Starting square must be empty.
                    if (occupied.Test(enPassantCaptureVector.North().North())) return false;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the <see cref="Color"/> of the side to move.
        /// </summary>
        public Color SideToMove { get { return sideToMove; } }

        /// <summary>
        /// Gets a bitfield which is true for all squares that contain the given color.
        /// </summary>
        public ulong GetVector(Color color)
        {
            return colorVectors[color];
        }

        /// <summary>
        /// Gets a bitfield which is true for all squares that contain the given piece.
        /// </summary>
        public ulong GetVector(Piece piece)
        {
            return pieceVectors[piece];
        }

        /// <summary>
        /// Gets a bitfield which is true for all squares that contain the given colored piece.
        /// </summary>
        public ulong GetVector(ColoredPiece coloredPiece)
        {
            var colorVector = GetVector(coloredPiece.GetColor());
            var pieceVector = GetVector(coloredPiece.GetPiece());
            return colorVector & pieceVector;
        }

        /// <summary>
        /// Gets a bitfield which is true for all squares that are empty.
        /// </summary>
        public ulong GetEmptyVector()
        {
            // Take the bitfield with 1 values only, and zero out whatever is white or black.
            return ulong.MaxValue ^ colorVectors[Color.White] ^ colorVectors[Color.Black];
        }

        /// <summary>
        /// If a pawn can be captured en passant in this position, returns the vector which is true for the square of that pawn.
        /// </summary>
        public ulong EnPassantCaptureVector
        {
            get { return enPassantCaptureVector; }
        }


        private Position()
        {
            colorVectors = EnumIndexedArray<Color, ulong>.New();
            pieceVectors = EnumIndexedArray<Piece, ulong>.New();
        }

        /// <summary>
        /// Returns the standard initial position.
        /// </summary>
        public static Position GetInitialPosition()
        {
            var initialPosition = new Position();

            initialPosition.sideToMove = Color.White;

            initialPosition.colorVectors[Color.White] = Constants.WhiteInStartPosition;
            initialPosition.colorVectors[Color.Black] = Constants.BlackInStartPosition;

            initialPosition.pieceVectors[Piece.Pawn] = Constants.PawnsInStartPosition;
            initialPosition.pieceVectors[Piece.Knight] = Constants.KnightsInStartPosition;
            initialPosition.pieceVectors[Piece.Bishop] = Constants.BishopsInStartPosition;
            initialPosition.pieceVectors[Piece.Rook] = Constants.RooksInStartPosition;
            initialPosition.pieceVectors[Piece.Queen] = Constants.QueensInStartPosition;
            initialPosition.pieceVectors[Piece.King] = Constants.KingsInStartPosition;

            initialPosition.castlingRightsVector = Constants.CastlingTargetSquares;

            Debug.Assert(initialPosition.checkInvariants());

            return initialPosition;
        }

        /// <summary>
        /// Creates an exact copy of this position and returns it.
        /// </summary>
        public Position Copy()
        {
            var copiedPosition = new Position();

            copiedPosition.sideToMove = sideToMove;
            copiedPosition.colorVectors = colorVectors.Copy();
            copiedPosition.pieceVectors = pieceVectors.Copy();

            copiedPosition.enPassantCaptureVector = enPassantCaptureVector;
            copiedPosition.enPassantVector = enPassantVector;
            copiedPosition.castlingRightsVector = castlingRightsVector;

            return copiedPosition;
        }


        /// <summary>
        /// Returns if the given square is attacked by a piece of the opposite color.
        /// </summary>
        public bool IsSquareUnderAttack(Square square, Color defenderColor)
        {
            ulong attackers = colorVectors[defenderColor.Opposite()];
            ulong occupied = colorVectors[defenderColor] | attackers;

            if (Constants.PawnCaptures[defenderColor, square]
                            .Test(attackers & pieceVectors[Piece.Pawn])
                || Constants.KnightMoves[square]
                            .Test(attackers & pieceVectors[Piece.Knight])
                || Constants.ReachableSquaresStraight(square, occupied)
                            .Test(attackers & (pieceVectors[Piece.Rook] | pieceVectors[Piece.Queen]))
                || Constants.ReachableSquaresDiagonal(square, occupied)
                            .Test(attackers & (pieceVectors[Piece.Bishop] | pieceVectors[Piece.Queen]))
                || Constants.Neighbours[square]
                            .Test(attackers & pieceVectors[Piece.King]))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the position of the king of the given color.
        /// </summary>
        public Square FindKing(Color color)
        {
            return (colorVectors[color] & pieceVectors[Piece.King]).GetSingleSquare();
        }

        private static ulong revokedCastlingRights(ulong moveDelta)
        {
            ulong affectedKingSquares = moveDelta & Constants.KingsInStartPosition;
            // If king squares are affected, only rook squares on the same side of the board can be affected as well.
            // Also a king move from E1 to E8 is impossible. Therefore, the rooks do not need to be checked anymore after a king square check.
            if (affectedKingSquares != 0)
            {
                return affectedKingSquares.West().West() | affectedKingSquares.East().East();
            }
            return (moveDelta & Constants.RooksStartPositionQueenside).East().East() | (moveDelta & Constants.RooksStartPositionKingside).West();
        }

        private MoveCheckResult getIllegalMoveTypeResult(MoveType moveType)
        {
            switch (moveType)
            {
                case MoveType.Promotion:
                    return MoveCheckResult.IllegalMoveTypePromotion;
                case MoveType.EnPassant:
                    return MoveCheckResult.IllegalMoveTypeEnPassant;
                case MoveType.CastleQueenside:
                    return MoveCheckResult.IllegalMoveTypeCastleQueenside;
                case MoveType.CastleKingside:
                    return MoveCheckResult.IllegalMoveTypeCastleKingside;
            }
            return MoveCheckResult.OK;
        }

        private void mandatoryMoveType(MoveType expectedMoveType, MoveType actualMoveType, ref MoveCheckResult moveCheckResult)
        {
            if (actualMoveType == expectedMoveType)
            {
                // Cancel out illegal move type results again.
                moveCheckResult &= ~getIllegalMoveTypeResult(expectedMoveType);
            }
            else if (actualMoveType == MoveType.Default)
            {
                switch (expectedMoveType)
                {
                    case MoveType.Promotion:
                        moveCheckResult |= MoveCheckResult.MissingPromotionInformation;
                        break;
                    case MoveType.EnPassant:
                        moveCheckResult |= MoveCheckResult.MissingEnPassant;
                        break;
                    case MoveType.CastleQueenside:
                        moveCheckResult |= MoveCheckResult.MissingCastleQueenside;
                        break;
                    case MoveType.CastleKingside:
                        moveCheckResult |= MoveCheckResult.MissingCastleKingside;
                        break;
                }
            }
        }

        /// <summary>
        /// Validates a move against the current position and optionally performs it.
        /// </summary>
        /// <param name="moveInfo">
        /// The move to validate and optionally perform.
        /// </param>
        /// <param name="make">
        /// True if the move must actually be made, false if only validated.
        /// </param>
        /// <returns>
        /// A <see cref="Move"/> structure with <see cref="Move.Result"/> equal to  
        /// <see cref="MoveCheckResult.OK"/> if the move was legal, otherwise one of the other <see cref="MoveCheckResult"/> values.
        /// If <paramref name="make"/> is true, the move is only made if <see cref="MoveCheckResult.OK"/> is returned.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when any of the move's members have an enumeration value which is outside of the allowed range.
        /// </exception>
        public Move TryMakeMove(MoveInfo moveInfo, bool make)
        {
            // Range checks.
            moveInfo.ThrowWhenOutOfRange();

            Debug.Assert(checkInvariants());

            Move move = new Move()
            {
                MoveType = MoveType.Default,
                SourceSquare = moveInfo.SourceSquare,
                TargetSquare = moveInfo.TargetSquare,
                PromoteTo = moveInfo.PromoteTo,
            };

            ulong sourceVector = moveInfo.SourceSquare.ToVector();
            ulong targetVector = moveInfo.TargetSquare.ToVector();

            ulong sideToMoveVector = colorVectors[sideToMove];
            ulong oppositeColorVector = colorVectors[sideToMove.Opposite()];
            ulong occupied = sideToMoveVector | oppositeColorVector;

            move.Result = MoveCheckResult.OK;
            if (sourceVector == targetVector)
            {
                // Can never move to the same square.
                move.Result |= MoveCheckResult.SourceSquareIsTargetSquare;
            }

            // Obtain moving piece.
            if (!EnumHelper<Piece>.AllValues.Any(x => pieceVectors[x].Test(sourceVector), out move.MovingPiece))
            {
                move.Result |= MoveCheckResult.SourceSquareIsEmpty;
            }
            else if (!sideToMoveVector.Test(sourceVector))
            {
                // Allow only SideToMove to make a move.
                move.Result |= MoveCheckResult.NotSideToMove;
            }

            // Can only check the rest if the basics are right.
            if (move.Result != 0)
            {
                return move;
            }

            if (sideToMoveVector.Test(targetVector))
            {
                // Do not allow capture of one's own pieces.
                move.Result |= MoveCheckResult.CannotCaptureOwnPiece;
            }

            // Check for illegal move types.
            move.Result |= getIllegalMoveTypeResult(moveInfo.MoveType);

            // Since en passant doesn't capture a pawn on the target square, separate captureVector from targetVector.
            ulong captureVector = targetVector;

            // Check legal target squares and specific rules depending on the moving piece.
            switch (move.MovingPiece)
            {
                case Piece.Pawn:
                    ulong legalCaptureSquares = (oppositeColorVector | enPassantVector)
                                              & Constants.PawnCaptures[sideToMove, moveInfo.SourceSquare];
                    ulong legalMoveToSquares = ~occupied
                                             & Constants.PawnMoves[sideToMove, moveInfo.SourceSquare]
                                             & Constants.ReachableSquaresStraight(moveInfo.SourceSquare, occupied);

                    if ((legalCaptureSquares | legalMoveToSquares).Test(targetVector))
                    {
                        if (Constants.PromotionSquares.Test(targetVector))
                        {
                            move.MoveType = MoveType.Promotion;
                            if (moveInfo.MoveType == MoveType.Promotion)
                            {
                                if (moveInfo.PromoteTo == Piece.Pawn || moveInfo.PromoteTo == Piece.King)
                                {
                                    // Allow only 4 promote-to pieces.
                                    move.Result |= MoveCheckResult.MissingPromotionInformation;
                                }
                            }
                        }
                        else if (enPassantVector.Test(targetVector))
                        {
                            move.MoveType = MoveType.EnPassant;
                            // Don't capture on the target square, but capture the pawn instead.
                            captureVector = enPassantCaptureVector;
                        }
                    }
                    else
                    {
                        move.Result |= MoveCheckResult.IllegalTargetSquare;
                    }
                    break;
                case Piece.Knight:
                    if (!Constants.KnightMoves[moveInfo.SourceSquare].Test(targetVector))
                    {
                        move.Result |= MoveCheckResult.IllegalTargetSquare;
                    }
                    break;
                case Piece.Bishop:
                    if (!Constants.ReachableSquaresDiagonal(moveInfo.SourceSquare, occupied).Test(targetVector))
                    {
                        move.Result |= MoveCheckResult.IllegalTargetSquare;
                    }
                    break;
                case Piece.Rook:
                    if (!Constants.ReachableSquaresStraight(moveInfo.SourceSquare, occupied).Test(targetVector))
                    {
                        move.Result |= MoveCheckResult.IllegalTargetSquare;
                    }
                    break;
                case Piece.Queen:
                    if (!Constants.ReachableSquaresStraight(moveInfo.SourceSquare, occupied).Test(targetVector)
                        && !Constants.ReachableSquaresDiagonal(moveInfo.SourceSquare, occupied).Test(targetVector))
                    {
                        move.Result |= MoveCheckResult.IllegalTargetSquare;
                    }
                    break;
                case Piece.King:
                    if (!Constants.Neighbours[moveInfo.SourceSquare].Test(targetVector))
                    {
                        // Castling moves. If castlingRightsVectors[sideToMove] is true somewhere, the king must be in its starting position.
                        // This means a simple bitwise AND can be done with the empty squares and destination squares reachable by straight rays.
                        ulong castlingTargets = castlingRightsVector
                                              & (sideToMove == Color.White ? Constants.Rank1 : Constants.Rank8)
                                              & Constants.ReachableSquaresStraight(moveInfo.SourceSquare, occupied)
                                              & ~occupied;  // Necessary because captures are not allowed.

                        if (castlingTargets.Test(targetVector))
                        {
                            if (IsSquareUnderAttack(moveInfo.SourceSquare, sideToMove))
                            {
                                // Not allowed to castle out of a check.
                                move.Result |= MoveCheckResult.FriendlyKingInCheck;
                            }
                            if (Constants.KingsideCastlingTargetSquares.Test(targetVector))
                            {
                                move.MoveType = MoveType.CastleKingside;
                                if (IsSquareUnderAttack(moveInfo.SourceSquare + 1, sideToMove))
                                {
                                    // Not allowed to castle over a check.
                                    move.Result |= MoveCheckResult.FriendlyKingInCheck;
                                }
                            }
                            else
                            {
                                move.MoveType = MoveType.CastleQueenside;
                                if (IsSquareUnderAttack(moveInfo.SourceSquare - 1, sideToMove))
                                {
                                    // Not allowed to castle over a check.
                                    move.Result |= MoveCheckResult.FriendlyKingInCheck;
                                }
                            }
                        }
                        else
                        {
                            move.Result |= MoveCheckResult.IllegalTargetSquare;
                        }
                    }
                    break;
            }

            if (move.MoveType != MoveType.Default)
            {
                mandatoryMoveType(move.MoveType, moveInfo.MoveType, ref move.Result);
            }

            if (move.Result.IsLegalMove())
            {
                // Remove whatever was captured.
                Piece capturedPiece;
                move.IsCapture = EnumHelper<Piece>.AllValues.Any(x => pieceVectors[x].Test(captureVector), out capturedPiece);
                if (move.IsCapture)
                {
                    colorVectors[sideToMove.Opposite()] = colorVectors[sideToMove.Opposite()] ^ captureVector;
                    pieceVectors[capturedPiece] = pieceVectors[capturedPiece] ^ captureVector;
                }

                // Move from source to target.
                ulong moveDelta = sourceVector | targetVector;
                colorVectors[sideToMove] = colorVectors[sideToMove] ^ moveDelta;
                pieceVectors[move.MovingPiece] = pieceVectors[move.MovingPiece] ^ moveDelta;

                // Find the king in the resulting position.
                Square friendlyKing = FindKing(sideToMove);

                // See if the friendly king is now under attack.
                if (IsSquareUnderAttack(friendlyKing, sideToMove))
                {
                    move.Result |= MoveCheckResult.FriendlyKingInCheck;
                }

                if (make && move.Result == MoveCheckResult.OK)
                {
                    // Reset en passant vectors.
                    enPassantVector = 0;
                    enPassantCaptureVector = 0;

                    if (move.MovingPiece == Piece.Pawn)
                    {
                        if (moveInfo.MoveType == MoveType.Promotion)
                        {
                            // Change type of piece.
                            pieceVectors[move.MovingPiece] = pieceVectors[move.MovingPiece] ^ targetVector;
                            pieceVectors[moveInfo.PromoteTo] = pieceVectors[moveInfo.PromoteTo] ^ targetVector;
                        }
                        else if (Constants.PawnTwoSquaresAhead[sideToMove, moveInfo.SourceSquare].Test(targetVector))
                        {
                            // If the moving piece was a pawn on its starting square and moved two steps ahead,
                            // it can be captured en passant on the next move.
                            enPassantVector = Constants.EnPassantSquares[sideToMove, moveInfo.SourceSquare];
                            enPassantCaptureVector = targetVector;
                        }
                    }
                    else if (move.MovingPiece == Piece.King)
                    {
                        // Move the rooks as well when castling.
                        if (moveInfo.MoveType == MoveType.CastleQueenside)
                        {
                            var rookDelta = Constants.CastleQueensideRookDelta[sideToMove];
                            colorVectors[sideToMove] = colorVectors[sideToMove] ^ rookDelta;
                            pieceVectors[Piece.Rook] = pieceVectors[Piece.Rook] ^ rookDelta;
                        }
                        else if (moveInfo.MoveType == MoveType.CastleKingside)
                        {
                            var rookDelta = Constants.CastleKingsideRookDelta[sideToMove];
                            colorVectors[sideToMove] = colorVectors[sideToMove] ^ rookDelta;
                            pieceVectors[Piece.Rook] = pieceVectors[Piece.Rook] ^ rookDelta;
                        }
                    }

                    // Update castling rights. Must be done for all pieces because everything can capture a rook on its starting position.
                    if (castlingRightsVector != 0)
                    {
                        // Revoke castling rights if kings or rooks are gone from their starting position.
                        castlingRightsVector &= ~revokedCastlingRights(moveDelta);
                    }

                    sideToMove = sideToMove.Opposite();
                }
                else
                {
                    // Reverse move.
                    colorVectors[sideToMove] = colorVectors[sideToMove] ^ moveDelta;
                    pieceVectors[move.MovingPiece] = pieceVectors[move.MovingPiece] ^ moveDelta;
                    if (move.IsCapture)
                    {
                        colorVectors[sideToMove.Opposite()] = colorVectors[sideToMove.Opposite()] ^ captureVector;
                        pieceVectors[capturedPiece] = pieceVectors[capturedPiece] ^ captureVector;
                    }
                }
            }

            Debug.Assert(checkInvariants());

            return move;
        }
    }

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
        /// The move would result in the capture of a piece of the same color.
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

#region License
/*********************************************************************************
 * Position.cs
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

using Eutherion.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace Sandra.Chess
{
    /// <summary>
    /// Represents a position in a standard chess game.
    /// </summary>
    public class Position
    {
        private EnumIndexedArray<Color, ulong> colorVectors;
        private EnumIndexedArray<Piece, ulong> pieceVectors;
        private ulong enPassantVector;
        private ulong castlingRightsVector;

        private Position() { }

        private bool CheckInvariants()
        {
            // Disjunct colors.
            if (colorVectors[Color.White].Test(colorVectors[Color.Black])) return false;
            ulong occupied = colorVectors[Color.White] | colorVectors[Color.Black];

            // Disjunct pieces.
            foreach (Piece piece1 in EnumValues<Piece>.List)
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
            Square enemyKing = FindKing(SideToMove.Opposite());
            if (IsSquareUnderAttack(enemyKing, SideToMove.Opposite())) return false;

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
                if (EnPassantCaptureVector.Test()) return false;
            }
            else
            {
                // enPassantVector must be empty.
                if (occupied.Test(enPassantVector)) return false;

                if (Constants.Rank4.Test(EnPassantCaptureVector))
                {
                    // There must be a white pawn on the en passant capture square.
                    if (!EnPassantCaptureVector.Test(colorVectors[Color.White] & pieceVectors[Piece.Pawn])) return false;
                    // enPassantVector must be directly south.
                    if (EnPassantCaptureVector.South() != enPassantVector) return false;
                    // Starting square must be empty.
                    if (occupied.Test(EnPassantCaptureVector.South().South())) return false;
                }
                else if (Constants.Rank5.Test(EnPassantCaptureVector))
                {
                    // There must be a black pawn on the en passant capture square.
                    if (!EnPassantCaptureVector.Test(colorVectors[Color.Black] & pieceVectors[Piece.Pawn])) return false;
                    // enPassantVector must be directly north.
                    if (EnPassantCaptureVector.North() != enPassantVector) return false;
                    // Starting square must be empty.
                    if (occupied.Test(EnPassantCaptureVector.North().North())) return false;
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
        public Color SideToMove { get; private set; }

        /// <summary>
        /// Gets a vector which is true for all squares that contain the given color.
        /// </summary>
        public ulong GetVector(Color color) => colorVectors[color];

        /// <summary>
        /// Gets a vector which is true for all squares that contain the given piece.
        /// </summary>
        public ulong GetVector(Piece piece) => pieceVectors[piece];

        /// <summary>
        /// Gets a vector which is true for all squares that contain the given colored piece.
        /// </summary>
        public ulong GetVector(ColoredPiece coloredPiece)
            => GetVector(coloredPiece.GetColor()) & GetVector(coloredPiece.GetPiece());

        /// <summary>
        /// Gets a vector which is true for all squares that are empty.
        /// </summary>
        public ulong GetEmptyVector()
            => ulong.MaxValue ^ colorVectors[Color.White] ^ colorVectors[Color.Black];

        /// <summary>
        /// If a pawn can be captured en passant in this position, returns the vector which is true for the square of that pawn.
        /// </summary>
        public ulong EnPassantCaptureVector { get; private set; }

        /// <summary>
        /// Returns the standard initial position.
        /// </summary>
        public static Position GetInitialPosition()
        {
            var initialPosition = new Position
            {
                SideToMove = Color.White,
                colorVectors = EnumIndexedArray<Color, ulong>.New(),
                pieceVectors = EnumIndexedArray<Piece, ulong>.New(),
                castlingRightsVector = Constants.CastlingTargetSquares,
            };

            initialPosition.colorVectors[Color.White] = Constants.WhiteInStartPosition;
            initialPosition.colorVectors[Color.Black] = Constants.BlackInStartPosition;

            initialPosition.pieceVectors[Piece.Pawn] = Constants.PawnsInStartPosition;
            initialPosition.pieceVectors[Piece.Knight] = Constants.KnightsInStartPosition;
            initialPosition.pieceVectors[Piece.Bishop] = Constants.BishopsInStartPosition;
            initialPosition.pieceVectors[Piece.Rook] = Constants.RooksInStartPosition;
            initialPosition.pieceVectors[Piece.Queen] = Constants.QueensInStartPosition;
            initialPosition.pieceVectors[Piece.King] = Constants.KingsInStartPosition;

            Debug.Assert(initialPosition.CheckInvariants());

            return initialPosition;
        }

        /// <summary>
        /// Creates an exact copy of this position and returns it.
        /// </summary>
        public Position Copy() => new Position
        {
            SideToMove = SideToMove,
            colorVectors = colorVectors.Copy(),
            pieceVectors = pieceVectors.Copy(),

            EnPassantCaptureVector = EnPassantCaptureVector,
            enPassantVector = enPassantVector,
            castlingRightsVector = castlingRightsVector
        };

        /// <summary>
        /// Returns if the given square is attacked by a piece of the opposite color.
        /// </summary>
        public bool IsSquareUnderAttack(Square square, Color defenderColor)
        {
            ulong attackers = colorVectors[defenderColor.Opposite()];
            ulong occupied = colorVectors[defenderColor] | attackers;

            return Constants.PawnCaptures[defenderColor, square]
                            .Test(attackers & pieceVectors[Piece.Pawn])
                || Constants.KnightMoves[square]
                            .Test(attackers & pieceVectors[Piece.Knight])
                || Constants.ReachableSquaresStraight(square, occupied)
                            .Test(attackers & (pieceVectors[Piece.Rook] | pieceVectors[Piece.Queen]))
                || Constants.ReachableSquaresDiagonal(square, occupied)
                            .Test(attackers & (pieceVectors[Piece.Bishop] | pieceVectors[Piece.Queen]))
                || Constants.Neighbours[square]
                            .Test(attackers & pieceVectors[Piece.King]);
        }

        /// <summary>
        /// Returns the position of the king of the given color.
        /// </summary>
        public Square FindKing(Color color)
            => (colorVectors[color] & pieceVectors[Piece.King]).GetSingleSquare();

        private static ulong RevokedCastlingRights(ulong moveDelta)
        {
            ulong affectedKingSquares = moveDelta & Constants.KingsInStartPosition;
            // If king squares are affected, only rook squares on the same side of the board can be affected as well.
            // Also a king move from E1 to E8 is impossible. Therefore, the rooks do not need to be checked anymore after a king square check.
            if (affectedKingSquares.Test())
            {
                return affectedKingSquares.West().West() | affectedKingSquares.East().East();
            }
            return (moveDelta & Constants.RooksStartPositionQueenside).East().East() | (moveDelta & Constants.RooksStartPositionKingside).West();
        }

        private void CompareMoveTypes(MoveType expectedMoveType, MoveType actualMoveType, ref MoveCheckResult moveCheckResult)
        {
            if (actualMoveType != expectedMoveType)
            {
                if (expectedMoveType == MoveType.Default || actualMoveType != MoveType.Default)
                {
                    // Given actualMoveType should not have been specified.
                    switch (actualMoveType)
                    {
                        case MoveType.Promotion:
                            moveCheckResult |= MoveCheckResult.IllegalMoveTypePromotion;
                            break;
                        case MoveType.EnPassant:
                            moveCheckResult |= MoveCheckResult.IllegalMoveTypeEnPassant;
                            break;
                        case MoveType.CastleQueenside:
                            moveCheckResult |= MoveCheckResult.IllegalMoveTypeCastleQueenside;
                            break;
                        case MoveType.CastleKingside:
                            moveCheckResult |= MoveCheckResult.IllegalMoveTypeCastleKingside;
                            break;
                    }
                }
                else
                {
                    // Only warn that a different MoveType was expected. Not an illegal move though.
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
        /// A valid legal <see cref="Move"/> structure if <see cref="MoveInfo.Result"/> is equal to  
        /// <see cref="MoveCheckResult.OK"/>, or an incomplete <see cref="Move"/> if one of the other <see cref="MoveCheckResult"/> values.
        /// If <paramref name="make"/> is true, the move is only made if <see cref="MoveCheckResult.OK"/> is returned.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when any of the move's members have an enumeration value which is outside of the allowed range.
        /// </exception>
        public Move TryMakeMove(ref MoveInfo moveInfo, bool make)
        {
            // Range checks.
            moveInfo.ThrowWhenOutOfRange();

            Debug.Assert(CheckInvariants());

            Move move = new Move
            {
                SourceSquare = moveInfo.SourceSquare,
                TargetSquare = moveInfo.TargetSquare
            };

            ulong sourceVector = move.SourceSquare.ToVector();
            ulong targetVector = move.TargetSquare.ToVector();

            ulong sideToMoveVector = colorVectors[SideToMove];
            ulong oppositeColorVector = colorVectors[SideToMove.Opposite()];
            ulong occupied = sideToMoveVector | oppositeColorVector;

            // Reset result before returning or checking anything.
            moveInfo.Result = MoveCheckResult.OK;

            if (sourceVector == targetVector)
            {
                // Can never move to the same square.
                moveInfo.Result |= MoveCheckResult.SourceSquareIsTargetSquare;
            }

            // Obtain moving piece.
            if (!EnumValues<Piece>.List.Any(x => pieceVectors[x].Test(sourceVector), out move.MovingPiece))
            {
                moveInfo.Result |= MoveCheckResult.SourceSquareIsEmpty;
            }
            else if (!sideToMoveVector.Test(sourceVector))
            {
                // Allow only SideToMove to make a move.
                moveInfo.Result |= MoveCheckResult.NotSideToMove;
            }

            // Can only check the rest if the basics are right.
            if (moveInfo.Result != 0)
            {
                return move;
            }

            if (sideToMoveVector.Test(targetVector))
            {
                // Do not allow capture of one's own pieces.
                moveInfo.Result |= MoveCheckResult.CannotCaptureOwnPiece;
            }

            // Check legal target squares and specific rules depending on the moving piece.
            switch (move.MovingPiece)
            {
                case Piece.Pawn:
                    ulong legalCaptureSquares = (oppositeColorVector | enPassantVector)
                                              & Constants.PawnCaptures[SideToMove, move.SourceSquare];
                    ulong legalMoveToSquares = ~occupied
                                             & Constants.PawnMoves[SideToMove, move.SourceSquare]
                                             & Constants.ReachableSquaresStraight(move.SourceSquare, occupied);

                    if ((legalCaptureSquares | legalMoveToSquares).Test(targetVector))
                    {
                        if (Constants.PromotionSquares.Test(targetVector))
                        {
                            move.MoveType = MoveType.Promotion;
                            if (moveInfo.MoveType == MoveType.Promotion)
                            {
                                // Allow only 4 promote-to pieces.
                                if (moveInfo.PromoteTo == Piece.Pawn || moveInfo.PromoteTo == Piece.King)
                                {
                                    moveInfo.Result |= MoveCheckResult.MissingPromotionInformation;
                                }
                                else
                                {
                                    move.PromoteTo = moveInfo.PromoteTo;
                                }
                            }
                        }
                        else if (enPassantVector.Test(targetVector))
                        {
                            move.MoveType = MoveType.EnPassant;
                        }
                        else if (Constants.PawnTwoSquaresAhead[SideToMove, move.SourceSquare].Test(targetVector))
                        {
                            move.IsPawnTwoSquaresAheadMove = true;
                        }
                    }
                    else
                    {
                        moveInfo.Result |= MoveCheckResult.IllegalTargetSquare;
                    }
                    break;
                case Piece.Knight:
                    if (!Constants.KnightMoves[move.SourceSquare].Test(targetVector))
                    {
                        moveInfo.Result |= MoveCheckResult.IllegalTargetSquare;
                    }
                    break;
                case Piece.Bishop:
                    if (!Constants.ReachableSquaresDiagonal(move.SourceSquare, occupied).Test(targetVector))
                    {
                        moveInfo.Result |= MoveCheckResult.IllegalTargetSquare;
                    }
                    break;
                case Piece.Rook:
                    if (!Constants.ReachableSquaresStraight(move.SourceSquare, occupied).Test(targetVector))
                    {
                        moveInfo.Result |= MoveCheckResult.IllegalTargetSquare;
                    }
                    break;
                case Piece.Queen:
                    if (!Constants.ReachableSquaresStraight(move.SourceSquare, occupied).Test(targetVector)
                        && !Constants.ReachableSquaresDiagonal(move.SourceSquare, occupied).Test(targetVector))
                    {
                        moveInfo.Result |= MoveCheckResult.IllegalTargetSquare;
                    }
                    break;
                case Piece.King:
                    if (!Constants.Neighbours[move.SourceSquare].Test(targetVector))
                    {
                        // Castling moves. If castlingRightsVectors[sideToMove] is true somewhere, the king must be in its starting position.
                        ulong castlingTargets = castlingRightsVector
                                              & (SideToMove == Color.White ? Constants.Rank1 : Constants.Rank8);

                        if (castlingTargets.Test(targetVector))
                        {
                            bool isKingSide = Constants.KingsideCastlingTargetSquares.Test(targetVector);

                            var rookDelta = isKingSide
                                          ? Constants.CastleKingsideRookDelta[SideToMove]
                                          : Constants.CastleQueensideRookDelta[SideToMove];

                            // All squares between the king and rook must be empty.
                            if ((rookDelta & Constants.ReachableSquaresStraight(move.SourceSquare, occupied)) == rookDelta)
                            {
                                if (isKingSide)
                                {
                                    move.MoveType = MoveType.CastleKingside;
                                    if (IsSquareUnderAttack(move.SourceSquare, SideToMove)
                                        || IsSquareUnderAttack(move.SourceSquare + 1, SideToMove))
                                    {
                                        // Not allowed to castle out of or over a check.
                                        moveInfo.Result |= MoveCheckResult.FriendlyKingInCheck;
                                    }
                                }
                                else
                                {
                                    move.MoveType = MoveType.CastleQueenside;
                                    if (IsSquareUnderAttack(move.SourceSquare, SideToMove)
                                        || IsSquareUnderAttack(move.SourceSquare - 1, SideToMove))
                                    {
                                        // Not allowed to castle out of or over a check.
                                        moveInfo.Result |= MoveCheckResult.FriendlyKingInCheck;
                                    }
                                }

                                // Not an illegal target square, so break here already.
                                break;
                            }
                        }

                        moveInfo.Result |= MoveCheckResult.IllegalTargetSquare;
                    }
                    break;
            }

            // Check for illegal move types.
            CompareMoveTypes(move.MoveType, moveInfo.MoveType, ref moveInfo.Result);

            if (moveInfo.Result.IsLegalMove())
            {
                // Since en passant doesn't capture a pawn on the target square, separate captureVector from targetVector.
                ulong captureVector;
                if (move.MoveType == MoveType.EnPassant)
                {
                    // Don't capture on the target square, but capture the pawn instead.
                    captureVector = EnPassantCaptureVector;
                    move.CapturedPiece = Piece.Pawn;
                    move.IsCapture = true;
                }
                else
                {
                    // Find the possible piece on the target square.
                    captureVector = targetVector;
                    move.IsCapture = EnumValues<Piece>.List.Any(x => pieceVectors[x].Test(captureVector), out move.CapturedPiece);
                }

                // Remove whatever was captured.
                if (move.IsCapture)
                {
                    colorVectors[SideToMove.Opposite()] = colorVectors[SideToMove.Opposite()] ^ captureVector;
                    pieceVectors[move.CapturedPiece] = pieceVectors[move.CapturedPiece] ^ captureVector;
                }

                // Move from source to target.
                ulong moveDelta = sourceVector | targetVector;
                colorVectors[SideToMove] = colorVectors[SideToMove] ^ moveDelta;
                pieceVectors[move.MovingPiece] = pieceVectors[move.MovingPiece] ^ moveDelta;

                // Find the king in the resulting position.
                Square friendlyKing = FindKing(SideToMove);

                // See if the friendly king is now under attack.
                if (IsSquareUnderAttack(friendlyKing, SideToMove))
                {
                    moveInfo.Result |= MoveCheckResult.FriendlyKingInCheck;
                }

                if (make && moveInfo.Result == MoveCheckResult.OK)
                {
                    if (move.IsPawnTwoSquaresAheadMove)
                    {
                        // If the moving piece was a pawn on its starting square and moved two steps ahead,
                        // it can be captured en passant on the next move.
                        enPassantVector = Constants.EnPassantSquares[SideToMove, move.SourceSquare];
                        EnPassantCaptureVector = targetVector;
                    }
                    else
                    {
                        // Reset en passant vectors.
                        enPassantVector = 0;
                        EnPassantCaptureVector = 0;
                    }

                    if (move.MoveType == MoveType.Promotion)
                    {
                        // Change type of piece.
                        pieceVectors[move.MovingPiece] = pieceVectors[move.MovingPiece] ^ targetVector;
                        pieceVectors[move.PromoteTo] = pieceVectors[move.PromoteTo] ^ targetVector;
                    }
                    else if (move.MoveType == MoveType.CastleQueenside)
                    {
                        // Move the rooks as well when castling.
                        var rookDelta = Constants.CastleQueensideRookDelta[SideToMove];
                        colorVectors[SideToMove] = colorVectors[SideToMove] ^ rookDelta;
                        pieceVectors[Piece.Rook] = pieceVectors[Piece.Rook] ^ rookDelta;
                    }
                    else if (move.MoveType == MoveType.CastleKingside)
                    {
                        // Move the rooks as well when castling.
                        var rookDelta = Constants.CastleKingsideRookDelta[SideToMove];
                        colorVectors[SideToMove] = colorVectors[SideToMove] ^ rookDelta;
                        pieceVectors[Piece.Rook] = pieceVectors[Piece.Rook] ^ rookDelta;
                    }

                    // Update castling rights. Must be done for all pieces because everything can capture a rook on its starting position.
                    if (castlingRightsVector.Test())
                    {
                        // Revoke castling rights if kings or rooks are gone from their starting position.
                        castlingRightsVector &= ~RevokedCastlingRights(moveDelta);
                    }

                    SideToMove = SideToMove.Opposite();
                }
                else
                {
                    // Reverse move.
                    colorVectors[SideToMove] = colorVectors[SideToMove] ^ moveDelta;
                    pieceVectors[move.MovingPiece] = pieceVectors[move.MovingPiece] ^ moveDelta;
                    if (move.IsCapture)
                    {
                        colorVectors[SideToMove.Opposite()] = colorVectors[SideToMove.Opposite()] ^ captureVector;
                        pieceVectors[move.CapturedPiece] = pieceVectors[move.CapturedPiece] ^ captureVector;
                    }
                }
            }

            Debug.Assert(CheckInvariants());

            return move;
        }

        /// <summary>
        /// This method is for internal use only.
        /// </summary>
        /// <remarks>
        /// This is a copy of TryMakeMove() but without the checks and with make = true.
        /// Only use if absolutely sure that the given Move is correct, or it will leave the position in a corrupted state.
        /// </remarks>
        internal void FastMakeMove(Move move)
        {
            Debug.Assert(CheckInvariants());

            ulong targetVector = move.TargetSquare.ToVector();

            // Remove whatever was captured.
            if (move.IsCapture)
            {
                if (move.MoveType != MoveType.EnPassant)
                {
                    colorVectors[SideToMove.Opposite()] = colorVectors[SideToMove.Opposite()] ^ targetVector;
                    pieceVectors[move.CapturedPiece] = pieceVectors[move.CapturedPiece] ^ targetVector;
                }
                else
                {
                    // Don't capture on the target square, but capture the pawn instead.
                    colorVectors[SideToMove.Opposite()] = colorVectors[SideToMove.Opposite()] ^ EnPassantCaptureVector;
                    pieceVectors[move.CapturedPiece] = pieceVectors[move.CapturedPiece] ^ EnPassantCaptureVector;
                }
            }

            // Move from source to target.
            ulong moveDelta = move.SourceSquare.ToVector() | targetVector;
            colorVectors[SideToMove] = colorVectors[SideToMove] ^ moveDelta;
            pieceVectors[move.MovingPiece] = pieceVectors[move.MovingPiece] ^ moveDelta;

            // Update en passant vectors.
            if (move.IsPawnTwoSquaresAheadMove)
            {
                // If the moving piece was a pawn on its starting square and moved two steps ahead,
                // it can be captured en passant on the next move.
                enPassantVector = Constants.EnPassantSquares[SideToMove, move.SourceSquare];
                EnPassantCaptureVector = targetVector;
            }
            else
            {
                // Reset en passant vectors.
                enPassantVector = 0;
                EnPassantCaptureVector = 0;
            }

            if (move.MoveType == MoveType.Promotion)
            {
                // Change type of piece.
                pieceVectors[move.MovingPiece] = pieceVectors[move.MovingPiece] ^ targetVector;
                pieceVectors[move.PromoteTo] = pieceVectors[move.PromoteTo] ^ targetVector;
            }
            else if (move.MoveType == MoveType.CastleQueenside)
            {
                // Move the rooks as well when castling.
                var rookDelta = Constants.CastleQueensideRookDelta[SideToMove];
                colorVectors[SideToMove] = colorVectors[SideToMove] ^ rookDelta;
                pieceVectors[Piece.Rook] = pieceVectors[Piece.Rook] ^ rookDelta;
            }
            else if (move.MoveType == MoveType.CastleKingside)
            {
                // Move the rooks as well when castling.
                var rookDelta = Constants.CastleKingsideRookDelta[SideToMove];
                colorVectors[SideToMove] = colorVectors[SideToMove] ^ rookDelta;
                pieceVectors[Piece.Rook] = pieceVectors[Piece.Rook] ^ rookDelta;
            }

            // Update castling rights. Must be done for all pieces because everything can capture a rook on its starting position.
            if (castlingRightsVector.Test())
            {
                // Revoke castling rights if kings or rooks are gone from their starting position.
                castlingRightsVector &= ~RevokedCastlingRights(moveDelta);
            }

            SideToMove = SideToMove.Opposite();

            Debug.Assert(CheckInvariants());
        }

        /// <summary>
        /// Generates and enumerates all non-castling moves which are legal in this position.
        /// </summary>
        public IEnumerable<MoveInfo> GenerateLegalMoves()
        {
            Debug.Assert(CheckInvariants());

            ulong sideToMoveVector = colorVectors[SideToMove];
            ulong oppositeColorVector = colorVectors[SideToMove.Opposite()];
            ulong occupied = sideToMoveVector | oppositeColorVector;

            foreach (var movingPiece in EnumValues<Piece>.List)
            {
                // Enumerate over all squares occupied by this piece.
                ulong coloredPieceVector = pieceVectors[movingPiece] & colorVectors[SideToMove];
                foreach (var sourceSquare in coloredPieceVector.AllSquares())
                {
                    // Initialize possible target squares of the moving piece.
                    ulong targetSquares = 0;
                    switch (movingPiece)
                    {
                        case Piece.Pawn:
                            ulong legalCaptureSquares = (oppositeColorVector | enPassantVector)
                                                      & Constants.PawnCaptures[SideToMove, sourceSquare];
                            ulong legalMoveToSquares = ~occupied
                                                     & Constants.PawnMoves[SideToMove, sourceSquare]
                                                     & Constants.ReachableSquaresStraight(sourceSquare, occupied);

                            targetSquares = legalCaptureSquares | legalMoveToSquares;
                            break;
                        case Piece.Knight:
                            targetSquares = Constants.KnightMoves[sourceSquare];
                            break;
                        case Piece.Bishop:
                            targetSquares = Constants.ReachableSquaresDiagonal(sourceSquare, occupied);
                            break;
                        case Piece.Rook:
                            targetSquares = Constants.ReachableSquaresStraight(sourceSquare, occupied);
                            break;
                        case Piece.Queen:
                            targetSquares = Constants.ReachableSquaresStraight(sourceSquare, occupied)
                                          | Constants.ReachableSquaresDiagonal(sourceSquare, occupied);
                            break;
                        case Piece.King:
                            targetSquares = Constants.Neighbours[sourceSquare];
                            break;
                    }

                    // Can never capture one's own pieces. This also filters out targetSquare == sourceSquare.
                    targetSquares &= ~sideToMoveVector;

                    foreach (var targetSquare in targetSquares.AllSquares())
                    {
                        // Reset/initialize move.
                        MoveInfo move = new MoveInfo();

                        ulong targetVector = targetSquare.ToVector();

                        // Since en passant doesn't capture a pawn on the target square, separate captureVector from targetVector.
                        bool isCapture = false;
                        Piece capturedPiece = default;
                        ulong captureVector;
                        if (movingPiece == Piece.Pawn && enPassantVector.Test(targetVector))
                        {
                            // Don't capture on the target square, but capture the pawn instead.
                            move.MoveType = MoveType.EnPassant;
                            captureVector = EnPassantCaptureVector;
                            capturedPiece = Piece.Pawn;
                            isCapture = true;
                        }
                        else
                        {
                            // Find the possible piece on the target square.
                            captureVector = targetVector;
                            isCapture = EnumValues<Piece>.List.Any(x => pieceVectors[x].Test(captureVector), out capturedPiece);
                        }

                        // Remove whatever was captured.
                        if (isCapture)
                        {
                            colorVectors[SideToMove.Opposite()] = colorVectors[SideToMove.Opposite()] ^ captureVector;
                            pieceVectors[capturedPiece] = pieceVectors[capturedPiece] ^ captureVector;
                        }

                        // Move from source to target.
                        ulong moveDelta = sourceSquare.ToVector() | targetVector;
                        colorVectors[SideToMove] = colorVectors[SideToMove] ^ moveDelta;
                        pieceVectors[movingPiece] = pieceVectors[movingPiece] ^ moveDelta;

                        // See if the friendly king is now under attack.
                        bool kingInCheck = IsSquareUnderAttack(FindKing(SideToMove), SideToMove);

                        // Move must be reversed before continuing.
                        colorVectors[SideToMove] = colorVectors[SideToMove] ^ moveDelta;
                        pieceVectors[movingPiece] = pieceVectors[movingPiece] ^ moveDelta;
                        if (isCapture)
                        {
                            colorVectors[SideToMove.Opposite()] = colorVectors[SideToMove.Opposite()] ^ captureVector;
                            pieceVectors[capturedPiece] = pieceVectors[capturedPiece] ^ captureVector;
                        }

                        if (!kingInCheck)
                        {
                            move.SourceSquare = sourceSquare;
                            move.TargetSquare = targetSquare;
                            if (movingPiece == Piece.Pawn && Constants.PromotionSquares.Test(targetVector))
                            {
                                move.MoveType = MoveType.Promotion;
                                move.PromoteTo = Piece.Knight; yield return move;
                                move.PromoteTo = Piece.Bishop; yield return move;
                                move.PromoteTo = Piece.Rook; yield return move;
                                move.PromoteTo = Piece.Queen; yield return move;
                            }
                            else
                            {
                                yield return move;
                            }
                        }
                    }
                }
            }

            Debug.Assert(CheckInvariants());
        }
    }
}

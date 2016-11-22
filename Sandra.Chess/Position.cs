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
        public ulong GetVector(NonEmptyColoredPiece coloredPiece)
        {
            var colorVector = GetVector(coloredPiece.GetColor());
            var pieceVector = GetVector(coloredPiece.GetPiece());
            return colorVector & pieceVector;
        }

        /// <summary>
        /// Gets a bitfield which is true for all squares that contain the given colored piece.
        /// </summary>
        public ulong GetVector(ColoredPiece coloredPiece)
        {
            if (coloredPiece == ColoredPiece.Empty)
            {
                // Take the bitfield with 1 values only, and zero out whatever is white or black.
                return ulong.MaxValue ^ colorVectors[Color.White] ^ colorVectors[Color.Black];
            }
            return GetVector((NonEmptyColoredPiece)coloredPiece);
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

            return initialPosition;
        }

        /// <summary>
        /// Validates a move against the current position and optionally performs it.
        /// </summary>
        /// <param name="move">
        /// The move to validate and optionally perform.
        /// </param>
        /// <param name="make">
        /// True if the move must actually be made, false if only validated.
        /// </param>
        /// <returns>
        /// <see cref="MoveCheckResult.OK"/> if the move was legal, otherwise one of the other <see cref="MoveCheckResult"/> values.
        /// If <paramref name="make"/> is true, the move is only made if <see cref="MoveCheckResult.OK"/> is returned.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="move"/> is null (Nothing in Visual Basic).
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when any of the move's members have an enumeration value which is outside of the allowed range.
        /// </exception>
        public MoveCheckResult TryMakeMove(Move move, bool make)
        {
            // Null and range checks.
            if (move == null) throw new ArgumentNullException(nameof(move));
            move.ThrowWhenOutOfRange();

            ulong sourceVector = move.SourceSquare.ToVector();
            ulong targetVector = move.TargetSquare.ToVector();

            ulong sideToMoveVector = colorVectors[sideToMove];
            ulong oppositeColorVector = colorVectors[sideToMove.Opposite()];
            ulong occupied = sideToMoveVector | oppositeColorVector;

            MoveCheckResult moveCheckResult = MoveCheckResult.OK;
            if (sourceVector == targetVector)
            {
                // Can never move to the same square.
                moveCheckResult |= MoveCheckResult.SourceSquareIsTargetSquare;
            }

            // Obtain moving piece.
            Piece movingPiece;
            if (!EnumHelper<Piece>.AllValues.Any(x => pieceVectors[x].Test(sourceVector), out movingPiece))
            {
                moveCheckResult |= MoveCheckResult.SourceSquareIsEmpty;
            }
            else if (!sideToMoveVector.Test(sourceVector))
            {
                // Allow only SideToMove to make a move.
                moveCheckResult |= MoveCheckResult.NotSideToMove;
            }

            // Can only check the rest if the basics are right.
            if (moveCheckResult != 0)
            {
                return moveCheckResult;
            }

            if (sideToMoveVector.Test(targetVector))
            {
                // Do not allow capture of one's own pieces.
                moveCheckResult |= MoveCheckResult.CannotCaptureOwnPiece;
            }

            if (move.MoveType == MoveType.Promotion && movingPiece != Piece.Pawn)
            {
                // Cannot promote a non-pawn.
                moveCheckResult |= MoveCheckResult.NotPromotion;
            }

            // Check legal target squares and specific rules depending on the moving piece.
            switch (movingPiece)
            {
                case Piece.Pawn:
                    ulong legalCaptureSquares = oppositeColorVector
                                              & Constants.PawnCaptures[sideToMove, move.SourceSquare];
                    ulong legalMoveToSquares = ~occupied
                                             & Constants.PawnMoves[sideToMove, move.SourceSquare]
                                             & Constants.ReachableSquaresStraight(move.SourceSquare, occupied);

                    if ((legalCaptureSquares | legalMoveToSquares).Test(targetVector))
                    {
                        if (Constants.PromotionSquares.Test(targetVector))
                        {
                            if (move.MoveType != MoveType.Promotion
                                || move.PromoteTo == Piece.Pawn
                                || move.PromoteTo == Piece.King)
                            {
                                // Must specify the correct MoveType, and allow only 4 promote-to pieces.
                                moveCheckResult |= MoveCheckResult.IllegalPromotion;
                            }
                        }
                        else if (move.MoveType == MoveType.Promotion)
                        {
                            // Cannot promote to a non-promotion square.
                            moveCheckResult |= MoveCheckResult.NotPromotion;
                        }
                    }
                    else
                    {
                        moveCheckResult |= MoveCheckResult.IllegalTargetSquare;
                    }
                    break;
                case Piece.Knight:
                    if (!Constants.KnightMoves[move.SourceSquare].Test(targetVector))
                    {
                        moveCheckResult |= MoveCheckResult.IllegalTargetSquare;
                    }
                    break;
                case Piece.Bishop:
                    if (!Constants.ReachableSquaresDiagonal(move.SourceSquare, occupied).Test(targetVector))
                    {
                        moveCheckResult |= MoveCheckResult.IllegalTargetSquare;
                    }
                    break;
                case Piece.Rook:
                    if (!Constants.ReachableSquaresStraight(move.SourceSquare, occupied).Test(targetVector))
                    {
                        moveCheckResult |= MoveCheckResult.IllegalTargetSquare;
                    }
                    break;
                case Piece.Queen:
                    if (!Constants.ReachableSquaresStraight(move.SourceSquare, occupied).Test(targetVector)
                        && !Constants.ReachableSquaresDiagonal(move.SourceSquare, occupied).Test(targetVector))
                    {
                        moveCheckResult |= MoveCheckResult.IllegalTargetSquare;
                    }
                    break;
                case Piece.King:
                    if (!Constants.Neighbours[move.SourceSquare].Test(targetVector))
                    {
                        moveCheckResult |= MoveCheckResult.IllegalTargetSquare;
                    }
                    break;
            }

            if (moveCheckResult == MoveCheckResult.OK || moveCheckResult == MoveCheckResult.IllegalPromotion)
            {
                // Remove whatever was captured.
                Piece capturedPiece;
                bool isCapture = EnumHelper<Piece>.AllValues.Any(x => pieceVectors[x].Test(targetVector), out capturedPiece);
                if (isCapture)
                {
                    colorVectors[sideToMove.Opposite()] = colorVectors[sideToMove.Opposite()] ^ targetVector;
                    pieceVectors[capturedPiece] = pieceVectors[capturedPiece] ^ targetVector;
                }

                // Move from source to target.
                ulong moveDelta = sourceVector | targetVector;
                colorVectors[sideToMove] = colorVectors[sideToMove] ^ moveDelta;
                pieceVectors[movingPiece] = pieceVectors[movingPiece] ^ moveDelta;

                // Find the king in the resulting position.
                Square friendlyKing = (Square)(colorVectors[sideToMove] & pieceVectors[Piece.King]).GetSingleBitIndex();
                // Evaluate colorVectors[sideToMove.Opposite()] again because it may have changed because of a capture.
                ulong candidateAttackers = colorVectors[sideToMove.Opposite()];
                ulong resultOccupied = colorVectors[sideToMove] | candidateAttackers;
                // See if the friendly king is now under attack.
                if (Constants.PawnCaptures[sideToMove, friendlyKing].Test(candidateAttackers & pieceVectors[Piece.Pawn])
                    || Constants.KnightMoves[friendlyKing].Test(candidateAttackers & pieceVectors[Piece.Knight])
                    || Constants.ReachableSquaresStraight(friendlyKing, resultOccupied).Test(candidateAttackers & (pieceVectors[Piece.Rook] | pieceVectors[Piece.Queen]))
                    || Constants.ReachableSquaresDiagonal(friendlyKing, resultOccupied).Test(candidateAttackers & (pieceVectors[Piece.Bishop] | pieceVectors[Piece.Queen]))
                    || Constants.Neighbours[friendlyKing].Test(candidateAttackers & pieceVectors[Piece.King]))
                {
                    moveCheckResult |= MoveCheckResult.FriendlyKingInCheck;
                }

                if (make && moveCheckResult == MoveCheckResult.OK)
                {
                    if (move.MoveType == MoveType.Promotion)
                    {
                        // Change type of piece.
                        pieceVectors[movingPiece] = pieceVectors[movingPiece] ^ targetVector;
                        pieceVectors[move.PromoteTo] = pieceVectors[move.PromoteTo] ^ targetVector;
                    }
                    sideToMove = sideToMove.Opposite();
                }
                else
                {
                    // Reverse move.
                    colorVectors[sideToMove] = colorVectors[sideToMove] ^ moveDelta;
                    pieceVectors[movingPiece] = pieceVectors[movingPiece] ^ moveDelta;
                    if (isCapture)
                    {
                        colorVectors[sideToMove.Opposite()] = colorVectors[sideToMove.Opposite()] ^ targetVector;
                        pieceVectors[capturedPiece] = pieceVectors[capturedPiece] ^ targetVector;
                    }
                }
            }

            return moveCheckResult;
        }
    }

    /// <summary>
    /// Enumerates all possible results of <see cref="Position.TryMakeMove(Move, bool)"/>.
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
        /// A move which promotes a pawn does not specify <see cref="MoveType.Promotion"/>, and/or the promotion piece is a pawn or king.
        /// </summary>
        IllegalPromotion = 32,
        /// <summary>
        /// <see cref="MoveType.Promotion"/> was specified for a move which does not promote a pawn.
        /// </summary>
        NotPromotion = 64,
        /// <summary>
        /// Making the move would put the friendly king in check.
        /// </summary>
        FriendlyKingInCheck = 128,
    }
}

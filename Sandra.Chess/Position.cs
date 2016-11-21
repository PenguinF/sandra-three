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

            ulong sourceDelta = move.SourceSquare.ToVector();
            ulong targetDelta = move.TargetSquare.ToVector();

            ulong sideToMoveVector = colorVectors[sideToMove];
            ulong oppositeColorVector = colorVectors[sideToMove.Opposite()];
            ulong occupied = sideToMoveVector | oppositeColorVector;

            MoveCheckResult moveCheckResult = MoveCheckResult.OK;
            if (sourceDelta == targetDelta)
            {
                // Can never move to the same square.
                moveCheckResult |= MoveCheckResult.SourceSquareIsTargetSquare;
            }

            // Obtain moving piece.
            Piece movingPiece;
            if (!EnumHelper<Piece>.AllValues.Any(x => pieceVectors[x].Test(sourceDelta), out movingPiece))
            {
                moveCheckResult |= MoveCheckResult.SourceSquareIsEmpty;
            }
            else if (!sideToMoveVector.Test(sourceDelta))
            {
                // Allow only SideToMove to make a move.
                moveCheckResult |= MoveCheckResult.NotSideToMove;
            }

            // Can only check the rest if the basics are right.
            if (moveCheckResult != 0)
            {
                return moveCheckResult;
            }

            if (sideToMoveVector.Test(targetDelta))
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

                    if ((legalCaptureSquares | legalMoveToSquares).Test(targetDelta))
                    {
                        if (Constants.PromotionSquares.Test(targetDelta))
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
                    if (!Constants.KnightMoves[move.SourceSquare].Test(targetDelta))
                    {
                        moveCheckResult |= MoveCheckResult.IllegalTargetSquare;
                    }
                    break;
                case Piece.Bishop:
                    if (!Constants.ReachableSquaresDiagonal(move.SourceSquare, occupied).Test(targetDelta))
                    {
                        moveCheckResult |= MoveCheckResult.IllegalTargetSquare;
                    }
                    break;
                case Piece.Rook:
                    if (!Constants.ReachableSquaresStraight(move.SourceSquare, occupied).Test(targetDelta))
                    {
                        moveCheckResult |= MoveCheckResult.IllegalTargetSquare;
                    }
                    break;
                case Piece.Queen:
                    if (!Constants.ReachableSquaresStraight(move.SourceSquare, occupied).Test(targetDelta)
                        && !Constants.ReachableSquaresDiagonal(move.SourceSquare, occupied).Test(targetDelta))
                    {
                        moveCheckResult |= MoveCheckResult.IllegalTargetSquare;
                    }
                    break;
                case Piece.King:
                    if (!Constants.Neighbours[move.SourceSquare].Test(targetDelta))
                    {
                        moveCheckResult |= MoveCheckResult.IllegalTargetSquare;
                    }
                    break;
            }

            if (moveCheckResult == MoveCheckResult.OK && make)
            {
                // Remove whatever was captured.
                Piece capturedPiece;
                if (EnumHelper<Piece>.AllValues.Any(x => pieceVectors[x].Test(targetDelta), out capturedPiece))
                {
                    colorVectors[sideToMove.Opposite()] = colorVectors[sideToMove.Opposite()] ^ targetDelta;
                    pieceVectors[capturedPiece] = pieceVectors[capturedPiece] ^ targetDelta;
                }

                // Move from source to target.
                colorVectors[sideToMove] = colorVectors[sideToMove] ^ targetDelta;
                colorVectors[sideToMove] = colorVectors[sideToMove] ^ sourceDelta;
                pieceVectors[movingPiece] = pieceVectors[movingPiece] ^ targetDelta;
                pieceVectors[movingPiece] = pieceVectors[movingPiece] ^ sourceDelta;

                if (move.MoveType == MoveType.Promotion)
                {
                    // Change type of piece.
                    pieceVectors[movingPiece] = pieceVectors[movingPiece] ^ targetDelta;
                    pieceVectors[move.PromoteTo] = pieceVectors[move.PromoteTo] ^ targetDelta;
                }

                sideToMove = sideToMove.Opposite();
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
        /// The move is a valid move in the given position.
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
        /// The piece on the source square does not belong to the color which turn it is.
        /// </summary>
        NotSideToMove = 4,
        /// <summary>
        /// The move would result in capturing a piece of the same color.
        /// </summary>
        CannotCaptureOwnPiece = 8,
        /// <summary>
        /// The target square is not a legal destination for the moving piece in the current position.
        /// </summary>
        IllegalTargetSquare = 16,
        /// <summary>
        /// A move which promotes a pawn does not specify <see cref="MoveType.Promotion"/>, and/or the promotion piece is a pawn or knight.
        /// </summary>
        IllegalPromotion = 32,
        /// <summary>
        /// <see cref="MoveType.Promotion"/> was specified for a move which does not promote a pawn.
        /// </summary>
        NotPromotion = 64,
    }
}

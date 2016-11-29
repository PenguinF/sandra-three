/*********************************************************************************
 * MoveFormatter.cs
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
using System.Collections.Generic;
using System.Text;

namespace Sandra.Chess
{
    /// <summary>
    /// Defines methods for showing moves in text, and parsing moves from textual input.
    /// </summary>
    public abstract class AbstractMoveFormatter
    {
        /// <summary>
        /// Generates a formatted notation for a move in a given position.
        /// As a side effect, the position is updated to the resulting position after the move.
        /// </summary>
        /// <param name="game">
        /// The game in which the move was made.
        /// </param>
        /// <param name="move">
        /// The move to be formatted.
        /// </param>
        /// <returns>
        /// The formatted notation for the move.
        /// </returns>
        public abstract string FormatMove(Game game, Move move);
    }

    /// <summary>
    /// Move formatter which generates short algebraic notation.
    /// </summary>
    public sealed class ShortAlgebraicMoveFormatter : AbstractMoveFormatter
    {
        /// <summary>
        /// Gets the constant string which is generated for moves which are illegal in the position in which they are performed.
        /// </summary>
        public const string IllegalMove = "???";

        private static readonly EnumIndexedArray<Piece, string> pieceSymbols = EnumIndexedArray<Piece, string>.New();

        static ShortAlgebraicMoveFormatter()
        {
            pieceSymbols[Piece.Pawn] = string.Empty;
            pieceSymbols[Piece.Knight] = "N";
            pieceSymbols[Piece.Bishop] = "B";
            pieceSymbols[Piece.Rook] = "R";
            pieceSymbols[Piece.Queen] = "Q";
            pieceSymbols[Piece.King] = "K";
        }

        public override string FormatMove(Game game, Move move)
        {
            MoveInfo moveInfo = game.TryMakeMove(move, false);
            if (moveInfo.Result == MoveCheckResult.OK)
            {
                if (move.MoveType == MoveType.CastleQueenside)
                {
                    return "O-O-O";
                }
                else if (move.MoveType == MoveType.CastleKingside)
                {
                    return "O-O";
                }

                StringBuilder builder = new StringBuilder();

                // Start with the moving piece.
                builder.Append(pieceSymbols[moveInfo.MovingPiece]);

                // When a pawn captures, append the file of the source square of the pawn.
                if (moveInfo.MovingPiece == Piece.Pawn && moveInfo.IsCapture)
                {
                    builder.Append((char)('a' + move.SourceSquare.X()));
                }

                // Disambiguate source square, not needed for pawns or kings.
                if (moveInfo.MovingPiece != Piece.Pawn && moveInfo.MovingPiece != Piece.King)
                {
                    Move testMove = new Move()
                    {
                        TargetSquare = move.TargetSquare,
                    };

                    bool ambiguous = false, fileAmbiguous = false, rankAmbiguous = false;
                    foreach (var square in EnumHelper<Square>.AllValues)
                    {
                        if (square != move.SourceSquare)
                        {
                            testMove.SourceSquare = square;
                            MoveInfo testInfo = game.TryMakeMove(testMove, false);
                            if (testInfo.Result.IsLegalMove() && testInfo.MovingPiece == moveInfo.MovingPiece)
                            {
                                ambiguous = true;
                                fileAmbiguous |= move.SourceSquare.X() == square.X();
                                rankAmbiguous |= move.SourceSquare.Y() == square.Y();
                            }
                        }
                    }

                    if (ambiguous)
                    {
                        // Disambiguation necessary.
                        if (fileAmbiguous)
                        {
                            if (rankAmbiguous)
                            {
                                builder.Append((char)('a' + move.SourceSquare.X()));
                            }
                            builder.Append(move.SourceSquare.Y() + 1);
                        }
                        else
                        {
                            builder.Append((char)('a' + move.SourceSquare.X()));
                        }
                    }
                }

                // Append a 'x' for capturing moves.
                if (moveInfo.IsCapture)
                {
                    builder.Append("x");
                }

                // Append the target square.
                builder.Append((char)('a' + move.TargetSquare.X()));
                builder.Append(move.TargetSquare.Y() + 1);

                // For promotion moves, append the symbol of the promotion piece.
                if (move.MoveType == MoveType.Promotion)
                {
                    builder.Append("=");
                    builder.Append(pieceSymbols[move.PromoteTo]);
                }

                game.TryMakeMove(move, true);

                Position current = game.CurrentPosition;
                Square friendlyKing = (current.GetVector(current.SideToMove) & current.GetVector(Piece.King)).GetSingleSquare();
                if (current.IsSquareUnderAttack(friendlyKing, current.SideToMove))
                {
                    builder.Append("+");
                }

                return builder.ToString();
            }

            return IllegalMove;
        }
    }
}

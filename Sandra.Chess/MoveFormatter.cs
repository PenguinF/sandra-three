﻿/*********************************************************************************
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
using System.Linq;
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
        /// As a side effect, the game is updated to the resulting position after the move.
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
    /// Common base class for <see cref="ShortAlgebraicMoveFormatter"/> and <see cref="LongAlgebraicMoveFormatter"/>. 
    /// </summary>
    public abstract class AlgebraicMoveFormatter : AbstractMoveFormatter
    {
        /// <summary>
        /// Gets the constant string which is generated for moves which are illegal in the position in which they are performed.
        /// </summary>
        public const string IllegalMove = "???";

        protected readonly EnumIndexedArray<Piece, string> pieceSymbols;

        public AlgebraicMoveFormatter(EnumIndexedArray<Piece, string> pieceSymbols)
        {
            this.pieceSymbols = pieceSymbols;
        }

        protected void AppendFile(StringBuilder builder, Square square)
        {
            builder.Append((char)('a' + square.X()));
        }

        protected void AppendRank(StringBuilder builder, Square square)
        {
            builder.Append(square.Y() + 1);
        }

        protected abstract void AppendDisambiguatingMoveSource(StringBuilder builder, Game game, Move move);

        public override string FormatMove(Game game, Move move)
        {
            StringBuilder builder = new StringBuilder();

            if (move.MoveType == MoveType.CastleQueenside)
            {
                builder.Append("O-O-O");
            }
            else if (move.MoveType == MoveType.CastleKingside)
            {
                builder.Append("O-O");
            }
            else
            {
                if (move.MovingPiece != Piece.Pawn)
                {
                    // Start with the moving piece.
                    builder.Append(pieceSymbols[move.MovingPiece]);
                }

                AppendDisambiguatingMoveSource(builder, game, move);

                // Append a 'x' for capturing moves.
                if (move.IsCapture)
                {
                    builder.Append("x");
                }

                // Append the target square.
                AppendFile(builder, move.TargetSquare);
                AppendRank(builder, move.TargetSquare);

                // For promotion moves, append the symbol of the promotion piece.
                if (move.MoveType == MoveType.Promotion)
                {
                    builder.Append("=");
                    builder.Append(pieceSymbols[move.PromoteTo]);
                }
            }

            MoveInfo moveInfo = move.CreateMoveInfo();

            game.TryMakeMove(ref moveInfo, true);

            if (moveInfo.Result == MoveCheckResult.OK)
            {
                Position current = game.CurrentPosition;
                Square friendlyKing = current.FindKing(current.SideToMove);
                if (current.IsSquareUnderAttack(friendlyKing, current.SideToMove))
                {
                    // No need to generate castling moves since castling out of a check is illegal.
                    if (current.GenerateLegalMoves().Any())
                    {
                        builder.Append("+");
                    }
                    else
                    {
                        builder.Append("#");
                    }
                }

                return builder.ToString();
            }

            return IllegalMove;
        }
    }

    /// <summary>
    /// Move formatter which generates long algebraic notation.
    /// </summary>
    public sealed class LongAlgebraicMoveFormatter : AlgebraicMoveFormatter
    {
        public LongAlgebraicMoveFormatter(EnumIndexedArray<Piece, string> pieceSymbols)
            : base(pieceSymbols) { }

        protected override void AppendDisambiguatingMoveSource(StringBuilder builder, Game game, Move move)
        {
            AppendFile(builder, move.SourceSquare);
            AppendRank(builder, move.SourceSquare);

            // Append a '-' for non-capturing moves.
            if (!move.IsCapture)
            {
                builder.Append("-");
            }
        }
    }

    /// <summary>
    /// Move formatter which generates short algebraic notation.
    /// </summary>
    public sealed class ShortAlgebraicMoveFormatter : AlgebraicMoveFormatter
    {
        public ShortAlgebraicMoveFormatter(EnumIndexedArray<Piece, string> pieceSymbols)
            : base(pieceSymbols) { }

        protected override void AppendDisambiguatingMoveSource(StringBuilder builder, Game game, Move move)
        {
            if (move.MovingPiece == Piece.Pawn)
            {
                if (move.IsCapture)
                {
                    // When a pawn captures, append the file of the source square of the pawn.
                    AppendFile(builder, move.SourceSquare);
                }
            }
            else if (move.MovingPiece != Piece.King)
            {
                // Disambiguate source square, not needed for pawns or kings.
                MoveInfo testMoveInfo = new MoveInfo()
                {
                    TargetSquare = move.TargetSquare,
                };

                bool ambiguous = false, fileAmbiguous = false, rankAmbiguous = false;
                foreach (var square in game.AllSquaresOccupiedBy(move.MovingPiece.Combine(game.SideToMove)))
                {
                    if (square != move.SourceSquare)
                    {
                        testMoveInfo.SourceSquare = square;
                        game.TryMakeMove(ref testMoveInfo, false);
                        if (testMoveInfo.Result.IsLegalMove())
                        {
                            // ambiguous can be true while both fileAmbiguous and rankAmbiguous are false.
                            // For example: Nb1-d2 or Nf3-d2.
                            ambiguous = true;
                            fileAmbiguous |= move.SourceSquare.X() == square.X();
                            rankAmbiguous |= move.SourceSquare.Y() == square.Y();
                        }
                    }
                }

                if (ambiguous)
                {
                    // Disambiguation necessary.
                    if (!fileAmbiguous || rankAmbiguous)
                    {
                        AppendFile(builder, move.SourceSquare);
                    }
                    if (fileAmbiguous)
                    {
                        AppendRank(builder, move.SourceSquare);
                    }
                }
            }
        }
    }
}

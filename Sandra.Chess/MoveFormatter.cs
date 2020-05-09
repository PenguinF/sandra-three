#region License
/*********************************************************************************
 * MoveFormatter.cs
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

using Eutherion.Utils;
using Sandra.Chess.Pgn;
using System.Linq;
using System.Text;

namespace Sandra.Chess
{
    /// <summary>
    /// Defines methods for showing moves in text, and parsing moves from textual input.
    /// </summary>
    public interface IMoveFormatter
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
        string FormatMove(Game game, Move move);
    }

    /// <summary>
    /// Common base class for <see cref="ShortAlgebraicMoveFormatter"/> and <see cref="LongAlgebraicMoveFormatter"/>. 
    /// </summary>
    public abstract class AlgebraicMoveFormatter : IMoveFormatter
    {
        /// <summary>
        /// Gets the notation which is generated for moves which are illegal in the position in which they are performed.
        /// </summary>
        public static readonly string IllegalMove = "???";

        /// <summary>
        /// Gets the notation which is generated for castling queenside moves. (&quot;O-O-O&quot;)
        /// </summary>
        public static readonly string CastleQueenSideMove = "O-O-O";

        /// <summary>
        /// Gets the notation which is generated for castling kingside moves. (&quot;O-O&quot;)
        /// </summary>
        public static readonly string CastleKingSideMove = "O-O";

        /// <summary>
        /// Gets the symbol which is generated for moves without captures in long algebraic notation. ('-')
        /// </summary>
        public static readonly char MoveNonCaptureSymbol = '-';

        /// <summary>
        /// Gets the symbol which is generated for captures. ('x')
        /// </summary>
        public static readonly char CaptureSymbol = 'x';

        /// <summary>
        /// Gets the symbol which is generated before a piece a pawn promotes to. ('=')
        /// </summary>
        public static readonly char PromoteToPieceSymbol = '=';

        /// <summary>
        /// Gets the symbol which is generated for moves that put a king in check. ('+')
        /// </summary>
        public static readonly char CheckSymbol = '+';

        /// <summary>
        /// Gets the symbol which is generated for checkmating moves. ('#')
        /// </summary>
        public static readonly char CheckmateSymbol = '#';

        protected readonly EnumIndexedArray<Piece, string> pieceSymbols;

        // Allocate only one StringBuilder for formatting moves.
        protected readonly StringBuilder moveBuilder = new StringBuilder();

        public AlgebraicMoveFormatter(string pieceSymbols)
        {
            if (pieceSymbols == null || pieceSymbols.Length < 5 || pieceSymbols.Length > 6)
            {
                // Revert back to PGN.
                pieceSymbols = PgnMoveFormatter.PieceSymbols;
            }

            EnumIndexedArray<Piece, string> pieceSymbolArray = EnumIndexedArray<Piece, string>.New();

            int pieceIndex = 0;
            if (pieceSymbols.Length == 6)
            {
                // Support for an optional pawn piece symbol.
                pieceSymbolArray[Piece.Pawn] = pieceSymbols[pieceIndex++].ToString();
            }

            pieceSymbolArray[Piece.Knight] = pieceSymbols[pieceIndex++].ToString();
            pieceSymbolArray[Piece.Bishop] = pieceSymbols[pieceIndex++].ToString();
            pieceSymbolArray[Piece.Rook] = pieceSymbols[pieceIndex++].ToString();
            pieceSymbolArray[Piece.Queen] = pieceSymbols[pieceIndex++].ToString();
            pieceSymbolArray[Piece.King] = pieceSymbols[pieceIndex++].ToString();

            this.pieceSymbols = pieceSymbolArray;
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

        public string FormatMove(Game game, Move move)
        {
            try
            {
                if (move.MoveType == MoveType.CastleQueenside)
                {
                    moveBuilder.Append(CastleQueenSideMove);
                }
                else if (move.MoveType == MoveType.CastleKingside)
                {
                    moveBuilder.Append(CastleKingSideMove);
                }
                else
                {
                    if (move.MovingPiece != Piece.Pawn)
                    {
                        // Start with the moving piece.
                        moveBuilder.Append(pieceSymbols[move.MovingPiece]);
                    }

                    AppendDisambiguatingMoveSource(moveBuilder, game, move);

                    // Append a 'x' for capturing moves.
                    if (move.IsCapture)
                    {
                        moveBuilder.Append(CaptureSymbol);
                    }

                    // Append the target square.
                    AppendFile(moveBuilder, move.TargetSquare);
                    AppendRank(moveBuilder, move.TargetSquare);

                    // For promotion moves, append the symbol of the promotion piece.
                    if (move.MoveType == MoveType.Promotion)
                    {
                        moveBuilder.Append(PromoteToPieceSymbol);
                        moveBuilder.Append(pieceSymbols[move.PromoteTo]);
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
                            moveBuilder.Append(CheckSymbol);
                        }
                        else
                        {
                            moveBuilder.Append(CheckmateSymbol);
                        }
                    }

                    return moveBuilder.ToString();
                }

                return IllegalMove;
            }
            finally
            {
                // Clear the builder so it can be used again.
                moveBuilder.Clear();
            }
        }
    }

    /// <summary>
    /// Move formatter which generates long algebraic notation.
    /// </summary>
    public sealed class LongAlgebraicMoveFormatter : AlgebraicMoveFormatter
    {
        public LongAlgebraicMoveFormatter(string pieceSymbols)
            : base(pieceSymbols) { }

        protected override void AppendDisambiguatingMoveSource(StringBuilder builder, Game game, Move move)
        {
            AppendFile(builder, move.SourceSquare);
            AppendRank(builder, move.SourceSquare);

            // Append a '-' for non-capturing moves.
            if (!move.IsCapture)
            {
                builder.Append(MoveNonCaptureSymbol);
            }
        }
    }

    /// <summary>
    /// Move formatter which generates short algebraic notation.
    /// </summary>
    public class ShortAlgebraicMoveFormatter : AlgebraicMoveFormatter
    {
        public ShortAlgebraicMoveFormatter(string pieceSymbols)
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
            else
            {
                // Disambiguate source square.
                MoveInfo testMoveInfo = new MoveInfo
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

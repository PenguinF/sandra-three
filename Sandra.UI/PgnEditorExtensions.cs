#region License
/*********************************************************************************
 * PgnEditorExtensions.cs
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

using Eutherion.Win.MdiAppTemplate;
using Sandra.Chess;
using Sandra.Chess.Pgn;
using System;
using System.Linq;
using System.Numerics;

namespace Sandra.UI
{
    using PgnEditor = SyntaxEditor<RootPgnSyntax, IPgnSymbol, PgnErrorInfo>;

    /// <summary>
    /// Contains extension methods for PGN syntax editors.
    /// </summary>
    public static class PgnEditorExtensions
    {
        public static PgnGameSyntax GameAtOrBeforePosition(this PgnEditor pgnEditor, int position)
        {
            // We're looking for the symbols before and after the position.
            // If the position is right at the edge between two games, return the previous game; any trivia is part of the next game,
            // and so it's more likely we're closer to the previous game.
            // Hence, take the first symbol from the enumeration.
            if (pgnEditor.SyntaxTree != null
                && pgnEditor.SyntaxTree.GameListSyntax.TerminalSymbolsInRange(position - 1, 2).Any(out IPgnSymbol symbolAtCursor))
            {
                PgnSyntax pgnSyntax = symbolAtCursor.ToSyntax();
                while (pgnSyntax != null)
                {
                    pgnSyntax = pgnSyntax.ParentSyntax;
                    if (pgnSyntax is PgnGameSyntax pgnGameSyntax) return pgnGameSyntax;
                }
            }

            return null;
        }

        private static Piece GetPiece(char c)
        {
            switch (c)
            {
                case 'N': return Piece.Knight;
                case 'B': return Piece.Bishop;
                case 'R': return Piece.Rook;
                case 'Q': return Piece.Queen;
                case 'K': return Piece.King;
                default: return Piece.Pawn;
            }
        }

        private static MoveInfo GetMoveInfo(Game game, string moveText, Color sideToMove)
        {
            MoveInfo moveInfo = new MoveInfo();

            // Very free-style parsing, based on the assumption that this is a recognized move.
            if (moveText == "O-O")
            {
                moveInfo.MoveType = MoveType.CastleKingside;
                if (sideToMove == Color.White)
                {
                    moveInfo.SourceSquare = Square.E1;
                    moveInfo.TargetSquare = Square.G1;
                }
                else
                {
                    moveInfo.SourceSquare = Square.E8;
                    moveInfo.TargetSquare = Square.G8;
                }
            }
            else if (moveText == "O-O-O")
            {
                moveInfo.MoveType = MoveType.CastleQueenside;
                if (sideToMove == Color.White)
                {
                    moveInfo.SourceSquare = Square.E1;
                    moveInfo.TargetSquare = Square.C1;
                }
                else
                {
                    moveInfo.SourceSquare = Square.E8;
                    moveInfo.TargetSquare = Square.C8;
                }
            }
            else
            {
                // Piece, disambiguation, capturing 'x', target square, promotion, check/mate/nag.
                Piece movingPiece = Piece.Pawn;
                int index = 0;
                if (moveText[index] >= 'A' && moveText[index] <= 'Z')
                {
                    movingPiece = GetPiece(moveText[index]);
                    index++;
                }

                File? disambiguatingSourceFile = null;
                Rank? disambiguatingSourceRank = null;
                File? targetFile = null;
                Rank? targetRank = null;
                Piece? promoteTo = null;

                while (index < moveText.Length)
                {
                    char currentChar = moveText[index];
                    if (currentChar == '=')
                    {
                        index++;
                        promoteTo = GetPiece(moveText[index]);
                        break;
                    }
                    else if (currentChar >= 'a' && currentChar <= 'h')
                    {
                        if (targetFile != null) disambiguatingSourceFile = targetFile;
                        targetFile = (File)(currentChar - 'a');
                    }
                    else if (currentChar >= '1' && currentChar <= '8')
                    {
                        if (targetRank != null) disambiguatingSourceRank = targetRank;
                        targetRank = (Rank)(currentChar - '1');
                    }

                    // Ignore 'x', '+', '#', '!', '?', increase index.
                    index++;
                }

                moveInfo.TargetSquare = ((File)targetFile).Combine((Rank)targetRank);

                // Get vector of pieces of the correct color that can move to the target square.
                ulong occupied = ~game.CurrentPosition.GetEmptyVector();
                ulong sourceSquareCandidates = game.CurrentPosition.GetVector(sideToMove) & game.CurrentPosition.GetVector(movingPiece);

                if (movingPiece == Piece.Pawn)
                {
                    // Capture or normal move?
                    if (disambiguatingSourceFile != null)
                    {
                        // Capture, go backwards by using the opposite side to move.
                        sourceSquareCandidates &= Constants.PawnCaptures[sideToMove.Opposite(), moveInfo.TargetSquare];

                        foreach (Square sourceSquareCandidate in sourceSquareCandidates.AllSquares())
                        {
                            if (disambiguatingSourceFile == (File)sourceSquareCandidate.X())
                            {
                                moveInfo.SourceSquare = sourceSquareCandidate;
                                break;
                            }
                        }

                        // En passant special move type, if the target capture square is empty.
                        if (!moveInfo.TargetSquare.ToVector().Test(occupied))
                        {
                            moveInfo.MoveType = MoveType.EnPassant;
                        }
                    }
                    else
                    {
                        // One or two squares backwards.
                        Func<ulong, ulong> direction;
                        if (sideToMove == Color.White) direction = ChessExtensions.South;
                        else direction = ChessExtensions.North;
                        ulong straightMoves = direction(moveInfo.TargetSquare.ToVector());
                        if (!straightMoves.Test(occupied)) straightMoves |= direction(straightMoves);
                        sourceSquareCandidates &= straightMoves;

                        foreach (Square sourceSquareCandidate in sourceSquareCandidates.AllSquares())
                        {
                            moveInfo.SourceSquare = sourceSquareCandidate;
                            break;
                        }
                    }

                    if (promoteTo != null)
                    {
                        moveInfo.MoveType = MoveType.Promotion;
                        moveInfo.PromoteTo = (Piece)promoteTo;
                    }
                }
                else
                {
                    switch (movingPiece)
                    {
                        case Piece.Knight:
                            sourceSquareCandidates &= Constants.KnightMoves[moveInfo.TargetSquare];
                            break;
                        case Piece.Bishop:
                            sourceSquareCandidates &= Constants.ReachableSquaresDiagonal(moveInfo.TargetSquare, occupied);
                            break;
                        case Piece.Rook:
                            sourceSquareCandidates &= Constants.ReachableSquaresStraight(moveInfo.TargetSquare, occupied);
                            break;
                        case Piece.Queen:
                            sourceSquareCandidates &= Constants.ReachableSquaresDiagonal(moveInfo.TargetSquare, occupied)
                                                    | Constants.ReachableSquaresStraight(moveInfo.TargetSquare, occupied);
                            break;
                        case Piece.King:
                            sourceSquareCandidates &= Constants.Neighbours[moveInfo.TargetSquare];
                            break;
                        default:
                            sourceSquareCandidates = 0;
                            break;
                    }

                    foreach (Square sourceSquareCandidate in sourceSquareCandidates.AllSquares())
                    {
                        if (disambiguatingSourceFile != null)
                        {
                            if (disambiguatingSourceFile == (File)sourceSquareCandidate.X())
                            {
                                if (disambiguatingSourceRank != null)
                                {
                                    if (disambiguatingSourceRank == (Rank)sourceSquareCandidate.Y())
                                    {
                                        moveInfo.SourceSquare = sourceSquareCandidate;
                                        break;
                                    }
                                }
                                else
                                {
                                    moveInfo.SourceSquare = sourceSquareCandidate;
                                    break;
                                }
                            }
                        }
                        else if (disambiguatingSourceRank != null)
                        {
                            if (disambiguatingSourceRank == (Rank)sourceSquareCandidate.Y())
                            {
                                moveInfo.SourceSquare = sourceSquareCandidate;
                                break;
                            }
                        }
                        else
                        {
                            moveInfo.SourceSquare = sourceSquareCandidate;
                            break;
                        }
                    }
                }
            }

            return moveInfo;
        }

        public static Game CreateGame(this PgnEditor pgnEditor, PgnGameSyntax gameSyntax)
        {
            var game = new Game();

            foreach (PgnPlySyntax ply in gameSyntax.PlyList.Plies)
            {
                // For now, invalidate the remainder of the game if seeing a null or unrecognized move.
                if (ply.Move == null) break;
                PgnMoveSyntax moveSyntax = ply.Move.PlyContentNode.ContentNode;
                if (moveSyntax.IsUnrecognizedMove) break;

                var sideToMove = game.CurrentPosition.SideToMove;
                MoveInfo moveInfo = GetMoveInfo(game, pgnEditor.GetTextRange(moveSyntax.AbsoluteStart, moveSyntax.Length), sideToMove);
                game.TryMakeMove(moveInfo);

                // Also invalidate on illegal move.
                if (sideToMove == game.CurrentPosition.SideToMove) break;
            }

            return game;
        }
    }
}

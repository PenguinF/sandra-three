#region License
/*********************************************************************************
 * Game.cs
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

using Sandra.Chess.Pgn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sandra.Chess
{
    /// <summary>
    /// Represents a standard game of chess.
    /// </summary>
    public class Game
    {
        private class PlyInfo
        {
            public PgnPlySyntax Ply;
            public PlyInfo Previous; // Points to the previous move. Is null for the first move.
            public bool IsLegalMove;
            public Move Move;
            public readonly List<PlyInfo> NextPlies = new List<PlyInfo>();
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

        private static MoveInfo GetMoveInfo(Position position, ReadOnlySpan<char> moveText, Color sideToMove)
        {
            MoveInfo moveInfo = new MoveInfo();

            // Very free-style parsing, based on the assumption that this is a recognized move.
            if (moveText.Equals("O-O".AsSpan(), StringComparison.Ordinal))
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
            else if (moveText.Equals("O-O-O".AsSpan(), StringComparison.Ordinal))
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
                ulong occupied = ~position.GetEmptyVector();
                ulong sourceSquareCandidates = position.GetVector(sideToMove) & position.GetVector(movingPiece);

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

        public static readonly string WhiteTagName = "White";
        public static readonly string BlackTagName = "Black";
        public static readonly string WhiteEloTagName = "WhiteElo";
        public static readonly string BlackEloTagName = "BlackElo";

        public PgnGameSyntax PgnGame { get; }

        public PgnTagValueSyntax White { get; }
        public PgnTagValueSyntax Black { get; }
        public PgnTagValueSyntax WhiteElo { get; }
        public PgnTagValueSyntax BlackElo { get; }

        /// <summary>
        /// Gets the initial position of this game.
        /// </summary>
        public ReadOnlyPosition InitialPosition { get; }

        private readonly Dictionary<PgnPlySyntax, PlyInfo> AllPlies = new Dictionary<PgnPlySyntax, PlyInfo>();
        private readonly List<PlyInfo> FirstPlies = new List<PlyInfo>();

        /// <summary>
        /// Gets the current position of this game.
        /// </summary>
        public ReadOnlyPosition CurrentPosition { get; private set; }

        /// <summary>
        /// Creates a new game from a given <see cref="PgnGameSyntax"/>.
        /// </summary>
        /// <param name="pgnGame">
        /// The syntax node which contains the game to create.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="pgnGame"/> is <see langword="null"/>.
        /// </exception>
        public Game(PgnGameSyntax pgnGame)
        {
            PgnGame = pgnGame ?? throw new ArgumentNullException(nameof(pgnGame));

            // Look in the game's tags for 4 known tag names.
            foreach (PgnTagPairSyntax tagPairSyntax in pgnGame.TagSection.TagPairNodes)
            {
                ReadOnlySpan<char> tagName = default;
                PgnTagValueSyntax tagValue = null;

                foreach (PgnTagElementSyntax tagElementSyntax in tagPairSyntax.TagElementNodes.Select(x => x.ContentNode))
                {
                    if (tagElementSyntax is PgnTagNameSyntax tagNameSyntax)
                    {
                        tagName = tagNameSyntax.SourcePgnAsSpan;
                    }
                    else if (tagElementSyntax is PgnTagValueSyntax tagValueSyntax)
                    {
                        tagValue = tagValueSyntax;
                    }
                }

                if (tagName.Length > 0 && tagValue != null)
                {
                    if (tagName.Equals(WhiteTagName.AsSpan(), StringComparison.OrdinalIgnoreCase)) White = tagValue;
                    else if (tagName.Equals(BlackTagName.AsSpan(), StringComparison.OrdinalIgnoreCase)) Black = tagValue;
                    else if (tagName.Equals(WhiteEloTagName.AsSpan(), StringComparison.OrdinalIgnoreCase)) WhiteElo = tagValue;
                    else if (tagName.Equals(BlackEloTagName.AsSpan(), StringComparison.OrdinalIgnoreCase)) BlackElo = tagValue;
                }
            }

            // Working position used for making moves.
            Position position = Position.GetInitialPosition();

            // Create a copy for reference.
            InitialPosition = new ReadOnlyPosition(position);

            // Initialize CurrentPosition. Copy-by-reference is ok since this is immutable.
            CurrentPosition = InitialPosition;

            AddPlyList(position, null, pgnGame.PlyList);
        }

        private void AddPlyList(Position position, PlyInfo previous, PgnPlyListSyntax plyList)
        {
            foreach (PgnPlySyntax ply in plyList.Plies)
            {
                // For now, invalidate the remainder of the game if seeing a null or unrecognized move.
                if (ply.Move == null) break;
                PgnMoveSyntax moveSyntax = ply.Move.PlyContentNode.ContentNode;
                if (moveSyntax.IsUnrecognizedMove) break;

                var sideToMove = CurrentPosition.SideToMove;
                MoveInfo moveInfo = GetMoveInfo(position, moveSyntax.SourcePgnAsSpan, sideToMove);
                TryMakeMove(moveInfo);

                // Also invalidate on illegal move.
                if (sideToMove == CurrentPosition.SideToMove) break;
            }
        }

        public bool IsFirstMove => true;
        public bool IsLastMove => true;
        public Move PreviousMove => default;

        public void Backward()
        {
        }

        public void Forward()
        {
        }

        /// <summary>
        /// Makes a move in the current position if it is legal.
        /// </summary>
        /// <param name="moveInfo">
        /// The move to make.
        /// </param>
        /// <returns>
        /// A <see cref="MoveCheckResult.OK"/> if the move is made and therefore legal; otherwise a <see cref="MoveCheckResult"/> value
        /// which describes the reason why the move is illegal.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when any of <paramref name="moveInfo"/>'s members have an enumeration value which is outside of the allowed range.
        /// </exception>
        public MoveCheckResult TryMakeMove(MoveInfo moveInfo)
        {
            // Disable until we can modify PGN using its syntax tree.
            moveInfo.Result = ~MoveCheckResult.OK;
            return default;
        }
    }
}

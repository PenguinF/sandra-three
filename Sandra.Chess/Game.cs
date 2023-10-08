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
    /// Represents a standard game of chess, with a pointer to an active ply.
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

        private static bool GetMoveInfo(Position position, ReadOnlySpan<char> moveText, out MoveInfo moveInfo)
        {
            moveInfo = new MoveInfo();

            // Very free-style parsing, based on the knowledge that this is a recognized move.
            if (moveText.Equals("O-O".AsSpan(), StringComparison.Ordinal))
            {
                moveInfo.MoveType = MoveType.CastleKingside;

                if (position.SideToMove == Color.White)
                {
                    moveInfo.SourceSquare = Square.E1;
                    moveInfo.TargetSquare = Square.G1;
                }
                else
                {
                    moveInfo.SourceSquare = Square.E8;
                    moveInfo.TargetSquare = Square.G8;
                }

                return true;
            }
            else if (moveText.Equals("O-O-O".AsSpan(), StringComparison.Ordinal))
            {
                moveInfo.MoveType = MoveType.CastleQueenside;

                if (position.SideToMove == Color.White)
                {
                    moveInfo.SourceSquare = Square.E1;
                    moveInfo.TargetSquare = Square.C1;
                }
                else
                {
                    moveInfo.SourceSquare = Square.E8;
                    moveInfo.TargetSquare = Square.C8;
                }

                return true;
            }

            // Piece, disambiguation, capturing 'x', target square, promotion, check/mate/nag.
            Piece movingPiece = Piece.Pawn;
            int index = 0;
            if (moveText[index] >= 'A' && moveText[index] <= 'Z')
            {
                movingPiece = GetPiece(moveText[index]);
                index++;
            }

            Maybe<File> disambiguatingSourceFile = Maybe<File>.Nothing;
            Maybe<Rank> disambiguatingSourceRank = Maybe<Rank>.Nothing;
            File? targetFile = null;
            Rank? targetRank = null;

            while (index < moveText.Length)
            {
                char currentChar = moveText[index];
                if (currentChar == '=')
                {
                    index++;
                    moveInfo.MoveType = MoveType.Promotion;
                    moveInfo.PromoteTo = GetPiece(moveText[index]);
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

            ulong sourceSquareCandidates = position.LegalSourceSquares(movingPiece, moveInfo.TargetSquare);

            if (movingPiece == Piece.Pawn)
            {
                // Special pawn handling: no disambiguating source file means a non-capture.
                if (!disambiguatingSourceFile.IsNothing)
                {
                    // En passant special move type, if the target capture square is empty.
                    if (moveInfo.TargetSquare.ToVector().Test(position.GetEmptyVector()))
                    {
                        moveInfo.MoveType = MoveType.EnPassant;
                    }
                }
                else
                {
                    // Zero out all source squares not in the same file as the target square.
                    sourceSquareCandidates &= Constants.FileMasks[moveInfo.TargetSquare];
                }
            }

            if (disambiguatingSourceFile.IsJust(out File sourceFile))
            {
                // Only allow candidates in the right file.
                sourceSquareCandidates &= sourceFile.ToVector();
            }

            if (disambiguatingSourceRank.IsJust(out Rank sourceRank))
            {
                // Only allow candidates in the right rank.
                sourceSquareCandidates &= sourceRank.ToVector();
            }

            // Allow only umambiguous moves.
            if (sourceSquareCandidates.IsMaxOneBit())
            {
                moveInfo.SourceSquare = sourceSquareCandidates.GetSingleSquare();
                return true;
            }

            return false;
        }

        public static readonly string WhiteTagName = "White";
        public static readonly string BlackTagName = "Black";
        public static readonly string WhiteEloTagName = "WhiteElo";
        public static readonly string BlackEloTagName = "BlackElo";

        public static IEnumerable<string> WellKnownTagNames
        {
            get
            {
                // TODO standard order seems to be:
                // Event, Site, Date, Round, White, Black, Result, WhiteElo, BlackElo, ECO, PlyCount
                // Not sure where the FEN of a custom start position comes in.
                yield return WhiteTagName;
                yield return BlackTagName;
                yield return WhiteEloTagName;
                yield return BlackEloTagName;
            }
        }

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
        /// Gets the current position of this game, which depends on the active ply.
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
                Position savedCopy = null;
                if (ply.Variations.Count > 0) savedCopy = position.Copy();

                // Add this ply before the variations, so it becomes the main line.
                PlyInfo plyInfo = AddPly(position, previous, ply);

                foreach (var variation in ply.Variations)
                {
                    // Variations must use the same 'previous' ply info as the actual move.
                    AddPlyList(savedCopy.Copy(), previous, variation.PlyContentNode.PliesWithFloatItems);
                }

                previous = plyInfo;
            }
        }

        private PlyInfo AddPly(Position position, PlyInfo previous, PgnPlySyntax ply)
        {
            PlyInfo current = new PlyInfo
            {
                Ply = ply,
                Previous = previous,
                IsLegalMove = false,
            };

            if (previous == null) FirstPlies.Add(current);
            else previous.NextPlies.Add(current);

            // For now, invalidate the remainder of the game if seeing a null or unrecognized move.
            if ((previous == null || previous.IsLegalMove) && ply.Move != null)
            {
                PgnMoveSyntax moveSyntax = ply.Move.PlyContentNode.ContentNode;

                if (!moveSyntax.IsUnrecognizedMove
                    && GetMoveInfo(position, moveSyntax.SourcePgnAsSpan, out MoveInfo moveInfo)
                    // This condition detects missing information such as promotion piece.
                    && position.TryMakeMove(moveInfo, true, out Move move) == MoveCheckResult.OK)
                {
                    current.IsLegalMove = true;
                    current.Move = move;
                }
            }

            AllPlies.Add(ply, current);

            return current;
        }

        // Null before the first move.
        private PlyInfo activePly;

        /// <summary>
        /// Gets or sets the ply syntax node which is currently active.
        /// If a ply contains an illegal move, the active ply is set to the last valid ply.
        /// </summary>
        public PgnPlySyntax ActivePly { get => activePly?.Ply; set => SetActivePly(value); }

        private void SetActivePly(PgnPlySyntax newActivePly)
        {
            // Quick exit?
            if (newActivePly == ActivePly) return;

            if (newActivePly == null || !AllPlies.TryGetValue(newActivePly, out PlyInfo plyInfo))
            {
                // If null or unknown, just go to the initial position.
                activePly = null;
                CurrentPosition = InitialPosition;
                return;
            }

            Stack<Move> moves = new Stack<Move>();
            for (PlyInfo p = plyInfo; p != null; p = p.Previous)
            {
                if (p.IsLegalMove) moves.Push(p.Move);
                else plyInfo = p.Previous;
            }

            Position position = InitialPosition.Copy();

            while (moves.Count > 0)
            {
                Move move = moves.Pop();
                position.FastMakeMove(move);
            }

            activePly = plyInfo;
            CurrentPosition = new ReadOnlyPosition(position);
        }

        /// <summary>
        /// Returns the last played move, or a default value if <see cref="IsFirstMove"/> is <see langword="true"/>.
        /// </summary>
        public Move PreviousMove => activePly == null ? default : activePly.Move;

        private List<PlyInfo> ActiveNextPlies => activePly == null ? FirstPlies : activePly.NextPlies;

        private bool LegalNextPly(out PlyInfo legalNextPlay) => ActiveNextPlies.Any(x => x.IsLegalMove, out legalNextPlay);

        /// <summary>
        /// Returns if <see cref="ActivePly"/> is null, i.e. at the initial position.
        /// </summary>
        public bool IsFirstMove => activePly == null;

        /// <summary>
        /// Returns if the active ply is followed by another in a main or side line.
        /// </summary>
        public bool IsLastMove => !LegalNextPly(out _);

        /// <summary>
        /// Moves <see cref="ActivePly"/> one ply backwards, towards the first move.
        /// If <see cref="IsFirstMove"/> is <see langword="true"/>, calling this method has no effect.
        /// </summary>
        public void Backward() => ActivePly = activePly?.Previous?.Ply;

        /// <summary>
        /// Moves <see cref="ActivePly"/> one ply forwards, towards the last move.
        /// If <see cref="IsLastMove"/> is <see langword="true"/>, calling this method has no effect.
        /// </summary>
        public void Forward()
        {
            if (LegalNextPly(out PlyInfo next)) ActivePly = next.Ply;
        }

        public bool TryGetPreviousSibling(PgnPlySyntax ply, out PgnPlySyntax previousSibling)
        {
            // Choose simple implementation to only select from direct and legal siblings.
            if (ply != null && AllPlies.TryGetValue(ply, out PlyInfo plyInfo))
            {
                List<PlyInfo> siblings = plyInfo.Previous == null ? FirstPlies : plyInfo.Previous.NextPlies;
                int siblingIndex = siblings.IndexOf(plyInfo);
                while (siblingIndex > 0)
                {
                    siblingIndex--;
                    PlyInfo candidate = siblings[siblingIndex];
                    if (candidate.IsLegalMove)
                    {
                        previousSibling = candidate.Ply;
                        return true;
                    }
                }
            }

            previousSibling = null;
            return false;
        }

        public bool TryGetNextSibling(PgnPlySyntax ply, out PgnPlySyntax nextSibling)
        {
            // Choose simple implementation to only select from direct and legal siblings.
            if (ply != null && AllPlies.TryGetValue(ply, out PlyInfo plyInfo))
            {
                List<PlyInfo> siblings = plyInfo.Previous == null ? FirstPlies : plyInfo.Previous.NextPlies;
                int siblingIndex = siblings.IndexOf(plyInfo);
                while (siblingIndex < siblings.Count - 1)
                {
                    siblingIndex++;
                    PlyInfo candidate = siblings[siblingIndex];
                    if (candidate.IsLegalMove)
                    {
                        nextSibling = candidate.Ply;
                        return true;
                    }
                }
            }

            nextSibling = null;
            return false;
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
            return ~MoveCheckResult.OK;
        }
    }
}

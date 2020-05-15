#region License
/*********************************************************************************
 * PgnGameSyntax.cs
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

using Eutherion;
using Eutherion.Text;
using System;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a single chess game in PGN. It contains a tag section, a move tree section,
    /// and an optional game termination marker.
    /// </summary>
    public sealed class GreenPgnGameSyntax : ISpan
    {
        /// <summary>
        /// Gets the tag section of the game.
        /// </summary>
        public GreenPgnTagSectionSyntax TagSection { get; }

        /// <summary>
        /// Gets the ply list of the game.
        /// </summary>
        public GreenPgnPlyListSyntax PlyList { get; }

        /// <summary>
        /// Gets the result of the game. The game result can be null.
        /// </summary>
        public GreenWithTriviaSyntax GameResult { get; }

        /// <summary>
        /// Returns the game result's length or 0 if it does not exist.
        /// </summary>
        public int GameResultLength => GameResult == null ? 0 : GameResult.Length;

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnGameSyntax"/>.
        /// </summary>
        /// <param name="tagSection">
        /// The tag section of the game.
        /// </param>
        /// <param name="plyList">
        /// The ply list of the game.
        /// </param>
        /// <param name="gameResult">
        /// The result of the game. This is an optional parameter.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="tagSection"/> and/or <paramref name="plyList"/> is null.
        /// </exception>
        public GreenPgnGameSyntax(
            GreenPgnTagSectionSyntax tagSection,
            GreenPgnPlyListSyntax plyList,
            GreenWithTriviaSyntax gameResult)
        {
            TagSection = tagSection ?? throw new ArgumentNullException(nameof(tagSection));
            PlyList = plyList ?? throw new ArgumentNullException(nameof(plyList));
            GameResult = gameResult;

            Length = tagSection.Length + plyList.Length + GameResultLength;
        }
    }

    /// <summary>
    /// Represents a single chess game in PGN. It contains a tag section, a move tree section,
    /// and an optional game termination marker.
    /// </summary>
    public sealed class PgnGameSyntax : PgnSyntax
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public PgnGameListSyntax Parent { get; }

        /// <summary>
        /// Gets the index of this syntax node in its parent.
        /// </summary>
        public int ParentIndex { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnGameSyntax Green { get; }

        private readonly SafeLazyObject<PgnTagSectionSyntax> tagSection;

        /// <summary>
        /// Gets the tag section of the game.
        /// </summary>
        public PgnTagSectionSyntax TagSection => tagSection.Object;

        private readonly SafeLazyObject<PgnPlyListSyntax> plyList;

        /// <summary>
        /// Gets the ply list of the game.
        /// </summary>
        public PgnPlyListSyntax PlyList => plyList.Object;

        private readonly SafeLazyChildSyntaxOrEmpty<PgnGameResultWithTriviaSyntax> lazyGameResultOrEmpty;

        /// <summary>
        /// Gets the result of the game. The game result can be null.
        /// </summary>
        public PgnGameResultWithTriviaSyntax GameResult => lazyGameResultOrEmpty.ChildNodeOrNull;

        /// <summary>
        /// Gets the result of the game. The game result can be <see cref="PgnEmptySyntax"/>.
        /// </summary>
        public PgnSyntax GameResultOrEmpty => lazyGameResultOrEmpty.ChildNodeOrEmpty;

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Green.Games.GetElementOffset(ParentIndex);

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public override int Length => Green.Length;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override PgnSyntax ParentSyntax => Parent;

        /// <summary>
        /// Gets the number of children of this syntax node.
        /// </summary>
        public override int ChildCount => 3;

        /// <summary>
        /// Initializes the child at the given <paramref name="index"/> and returns it.
        /// </summary>
        public override PgnSyntax GetChild(int index)
        {
            if (index == 0) return TagSection;
            if (index == 1) return PlyList;
            if (index == 2) return GameResultOrEmpty;
            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// Gets the start position of the child at the given <paramref name="index"/>, without initializing it.
        /// </summary>
        public override int GetChildStartPosition(int index)
        {
            if (index == 0) return 0;
            if (index == 1) return Green.TagSection.Length;
            if (index == 2) return Green.TagSection.Length + Green.PlyList.Length;
            throw new IndexOutOfRangeException();
        }

        internal PgnGameSyntax(PgnGameListSyntax parent, int parentIndex, GreenPgnGameSyntax green)
        {
            Parent = parent;
            ParentIndex = parentIndex;
            Green = green;

            tagSection = new SafeLazyObject<PgnTagSectionSyntax>(() => new PgnTagSectionSyntax(this, green.TagSection));
            plyList = new SafeLazyObject<PgnPlyListSyntax>(() => new PgnPlyListSyntax(this, green.PlyList));

            if (green.GameResult != null)
            {
                lazyGameResultOrEmpty = new SafeLazyChildSyntaxOrEmpty<PgnGameResultWithTriviaSyntax>(
                    () => new PgnGameResultWithTriviaSyntax(this, Green.GameResult));
            }
            else
            {
                lazyGameResultOrEmpty = new SafeLazyChildSyntaxOrEmpty<PgnGameResultWithTriviaSyntax>(
                    this, green.TagSection.Length + green.PlyList.Length);
            }
        }
    }
}

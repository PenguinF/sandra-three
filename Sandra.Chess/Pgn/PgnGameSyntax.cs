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
}

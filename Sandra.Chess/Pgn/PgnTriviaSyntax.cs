#region License
/*********************************************************************************
 * PgnTriviaSyntax.cs
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
using System.Collections.Generic;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a node with comments and background in an abstract PGN syntax tree.
    /// </summary>
    public sealed class GreenPgnTriviaSyntax : ISpan
    {
        /// <summary>
        /// Gets the comment nodes.
        /// </summary>
        public ReadOnlySpanList<GreenPgnTriviaElementSyntax> CommentNodes { get; }

        /// <summary>
        /// Gets the background after the comment nodes.
        /// </summary>
        public ReadOnlySpanList<GreenPgnBackgroundSyntax> BackgroundAfter { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length => CommentNodes.Length + BackgroundAfter.Length;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnTriviaSyntax"/>.
        /// </summary>
        /// <param name="commentNodes">
        /// The comment nodes.
        /// </param>
        /// <param name="backgroundAfter">
        /// The background after the comment nodes.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="commentNodes"/> and/or <paramref name="backgroundAfter"/> are null.
        /// </exception>
        public GreenPgnTriviaSyntax(IEnumerable<GreenPgnTriviaElementSyntax> commentNodes, IEnumerable<GreenPgnBackgroundSyntax> backgroundAfter)
        {
            if (commentNodes == null) throw new ArgumentNullException(nameof(commentNodes));
            if (backgroundAfter == null) throw new ArgumentNullException(nameof(backgroundAfter));

            CommentNodes = ReadOnlySpanList<GreenPgnTriviaElementSyntax>.Create(commentNodes);
            BackgroundAfter = ReadOnlySpanList<GreenPgnBackgroundSyntax>.Create(backgroundAfter);
        }
    }
}

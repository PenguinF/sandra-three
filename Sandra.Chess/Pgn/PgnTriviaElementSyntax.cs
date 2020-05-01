#region License
/*********************************************************************************
 * PgnTriviaElementSyntax.cs
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
    /// Represents a PGN syntax node which contains a comment and its preceding background.
    /// </summary>
    public sealed class GreenPgnTriviaElementSyntax : ISpan
    {
        /// <summary>
        /// Gets the background symbols which directly precede the comment foreground node.
        /// </summary>
        public ReadOnlySpanList<GreenPgnBackgroundSyntax> BackgroundBefore { get; }

        /// <summary>
        /// Gets the foreground node which contains the comment.
        /// </summary>
        public GreenPgnCommentSyntax CommentNode { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnTriviaElementSyntax"/>.
        /// </summary>
        /// <param name="backgroundBefore">
        /// The background symbols which directly precede the comment foreground node.
        /// </param>
        /// <param name="contentNode">
        /// The foreground node which contains the comment.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="backgroundBefore"/> and/or <paramref name="commentNode"/> are null.
        /// </exception>
        public GreenPgnTriviaElementSyntax(IEnumerable<GreenPgnBackgroundSyntax> backgroundBefore, GreenPgnCommentSyntax commentNode)
        {
            if (backgroundBefore == null) throw new ArgumentNullException(nameof(backgroundBefore));

            BackgroundBefore = ReadOnlySpanList<GreenPgnBackgroundSyntax>.Create(backgroundBefore);
            CommentNode = commentNode ?? throw new ArgumentNullException(nameof(commentNode));
            Length = BackgroundBefore.Length + CommentNode.Length;
        }
    }
}

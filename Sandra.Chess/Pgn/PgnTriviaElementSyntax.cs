#region License
/*********************************************************************************
 * PgnTriviaElementSyntax.cs
 *
 * Copyright (c) 2004-2021 Henk Nicolai
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
using Eutherion.Threading;
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
        /// <param name="commentNode">
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

    /// <summary>
    /// Represents a PGN syntax node which contains a comment and its preceding background.
    /// </summary>
    public sealed class PgnTriviaElementSyntax : PgnSyntax
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public PgnTriviaSyntax Parent { get; }

        /// <summary>
        /// Gets the index of this syntax node in the comment nodes collection of its parent.
        /// </summary>
        public int CommentNodeIndex { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnTriviaElementSyntax Green { get; }

        private readonly SafeLazyObject<PgnBackgroundListSyntax> backgroundBefore;

        /// <summary>
        /// Gets the background before the comment node.
        /// </summary>
        public PgnBackgroundListSyntax BackgroundBefore => backgroundBefore.Object;

        private readonly SafeLazyObject<PgnCommentSyntax> commentNode;

        /// <summary>
        /// Gets the foreground node which contains the comment.
        /// </summary>
        public PgnCommentSyntax CommentNode => commentNode.Object;

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Green.CommentNodes.GetElementOffset(CommentNodeIndex);

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override PgnSyntax ParentSyntax => Parent;

        /// <summary>
        /// Gets the number of children of this syntax node.
        /// </summary>
        public override int ChildCount => 2;

        /// <summary>
        /// Initializes the child at the given <paramref name="index"/> and returns it.
        /// </summary>
        public override PgnSyntax GetChild(int index)
        {
            if (index == 0) return BackgroundBefore;
            if (index == 1) return CommentNode;
            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// Gets the start position of the child at the given <paramref name="index"/>, without initializing it.
        /// </summary>
        public override int GetChildStartPosition(int index)
        {
            if (index == 0) return 0;
            if (index == 1) return Green.BackgroundBefore.Length;
            throw new IndexOutOfRangeException();
        }

        internal PgnTriviaElementSyntax(PgnTriviaSyntax parent, int commentNodeIndex, GreenPgnTriviaElementSyntax green)
        {
            Parent = parent;
            CommentNodeIndex = commentNodeIndex;
            Green = green;

            backgroundBefore = new SafeLazyObject<PgnBackgroundListSyntax>(() => new PgnBackgroundListSyntax(this, Green.BackgroundBefore));

            commentNode = new SafeLazyObject<PgnCommentSyntax>(() => new PgnCommentSyntax(this, Green.CommentNode));
        }
    }
}

#region License
/*********************************************************************************
 * PgnTriviaSyntax.cs
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

using Eutherion;
using Eutherion.Collections;
using Eutherion.Text;
using Eutherion.Threading;
using System;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a node with comments and background in an abstract PGN syntax tree.
    /// </summary>
    public sealed class GreenPgnTriviaSyntax : ISpan
    {
        /// <summary>
        /// Gets the empty <see cref="GreenPgnTriviaSyntax"/>.
        /// </summary>
        public static readonly GreenPgnTriviaSyntax Empty = new GreenPgnTriviaSyntax(
            ReadOnlySpanList<GreenPgnTriviaElementSyntax>.Empty,
            ReadOnlySpanList<GreenPgnBackgroundSyntax>.Empty);

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnTriviaSyntax"/>.
        /// </summary>
        /// <param name="commentNodes">
        /// The comment nodes.
        /// </param>
        /// <param name="backgroundAfter">
        /// The background after the comment nodes.
        /// </param>
        /// <returns>
        /// The new <see cref="GreenPgnTriviaSyntax"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="commentNodes"/> and/or <paramref name="backgroundAfter"/> are null.
        /// </exception>
        public static GreenPgnTriviaSyntax Create(ReadOnlySpanList<GreenPgnTriviaElementSyntax> commentNodes, ReadOnlySpanList<GreenPgnBackgroundSyntax> backgroundAfter)
        {
            if (commentNodes == null) throw new ArgumentNullException(nameof(commentNodes));
            if (backgroundAfter == null) throw new ArgumentNullException(nameof(backgroundAfter));

            return commentNodes.Count == 0 && backgroundAfter.Count == 0
                ? Empty
                : new GreenPgnTriviaSyntax(commentNodes, backgroundAfter);
        }

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

        private GreenPgnTriviaSyntax(ReadOnlySpanList<GreenPgnTriviaElementSyntax> commentNodes, ReadOnlySpanList<GreenPgnBackgroundSyntax> backgroundAfter)
        {
            CommentNodes = commentNodes;
            BackgroundAfter = backgroundAfter;
        }
    }

    /// <summary>
    /// Represents a node with comments and background in an abstract PGN syntax tree.
    /// </summary>
    public sealed class PgnTriviaSyntax : PgnSyntax
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public Union<WithTriviaSyntax, PgnGameListSyntax> Parent { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnTriviaSyntax Green { get; }

        /// <summary>
        /// Gets the collection of comment nodes.
        /// </summary>
        public SafeLazyObjectCollection<PgnTriviaElementSyntax> CommentNodes { get; }

        private readonly SafeLazyObject<PgnBackgroundListSyntax> backgroundAfter;

        /// <summary>
        /// Gets the background after the comment nodes.
        /// </summary>
        public PgnBackgroundListSyntax BackgroundAfter => backgroundAfter.Object;

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Match(whenOption1: _ => 0, whenOption2: x => x.Green.Games.Length);

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override PgnSyntax ParentSyntax => Parent.Match<PgnSyntax>(whenOption1: x => x, whenOption2: x => x);

        /// <summary>
        /// Gets the number of children of this syntax node.
        /// </summary>
        public override int ChildCount => CommentNodes.Count + 1;

        /// <summary>
        /// Initializes the child at the given <paramref name="index"/> and returns it.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than 0 or greater than or equal to <see cref="ChildCount"/>.
        /// </exception>
        public override PgnSyntax GetChild(int index)
        {
            if (index < CommentNodes.Count) return CommentNodes[index];
            if (index == CommentNodes.Count) return BackgroundAfter;
            throw ExceptionUtil.ThrowListIndexOutOfRangeException();
        }

        /// <summary>
        /// Gets the start position of the child at the given <paramref name="index"/>, without initializing it.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than 0 or greater than or equal to <see cref="ChildCount"/>.
        /// </exception>
        public override int GetChildStartPosition(int index)
        {
            if (index < CommentNodes.Count) return Green.CommentNodes.GetElementOffset(index);
            if (index == CommentNodes.Count) return Green.CommentNodes.Length;
            throw ExceptionUtil.ThrowListIndexOutOfRangeException();
        }

        internal PgnTriviaSyntax(Union<WithTriviaSyntax, PgnGameListSyntax> parent, GreenPgnTriviaSyntax green)
        {
            Parent = parent;
            Green = green;

            CommentNodes = new SafeLazyObjectCollection<PgnTriviaElementSyntax>(
                green.CommentNodes.Count,
                index => new PgnTriviaElementSyntax(this, index, Green.CommentNodes[index]));

            backgroundAfter = new SafeLazyObject<PgnBackgroundListSyntax>(() => new PgnBackgroundListSyntax(this, Green.BackgroundAfter));
        }
    }
}

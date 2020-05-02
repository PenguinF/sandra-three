﻿#region License
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

using Eutherion;
using Eutherion.Text;
using Eutherion.Utils;
using Sandra.Chess.Pgn.Temp;
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

    /// <summary>
    /// Represents a node with comments and background in an abstract PGN syntax tree.
    /// </summary>
    public sealed class PgnTriviaSyntax : PgnSyntax
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public PgnSyntaxNodes Parent { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnTriviaSyntax Green { get; }

        /// <summary>
        /// Gets the collection of comment nodes.
        /// </summary>
        public SafeLazyObjectCollection<TempPgnSymbolWithTrivia> CommentNodes { get; }

        private readonly SafeLazyObject<PgnBackgroundListSyntax> backgroundAfter;

        /// <summary>
        /// Gets the background after the comment nodes.
        /// </summary>
        public PgnBackgroundListSyntax BackgroundAfter => backgroundAfter.Object;

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.GreenForegroundNodes.Length;

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
        public override int ChildCount => CommentNodes.Count + 1;

        /// <summary>
        /// Initializes the child at the given <paramref name="index"/> and returns it.
        /// </summary>
        public override PgnSyntax GetChild(int index)
        {
            if (index < CommentNodes.Count) return CommentNodes[index];
            if (index == CommentNodes.Count) return BackgroundAfter;
            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// Gets the start position of the child at the given <paramref name="index"/>, without initializing it.
        /// </summary>
        public override int GetChildStartPosition(int index)
        {
            if (index < CommentNodes.Count) return Green.CommentNodes.GetElementOffset(index);
            if (index == CommentNodes.Count) return Green.CommentNodes.Length;
            throw new IndexOutOfRangeException();
        }

        internal PgnTriviaSyntax(PgnSyntaxNodes parent, GreenPgnTriviaSyntax green)
        {
            Parent = parent;
            Green = green;

            List<TempGreenPgnForegroundSyntax> flattenedSyntax = new List<TempGreenPgnForegroundSyntax>();
            foreach (GreenPgnTriviaElementSyntax trailing in green.CommentNodes)
            {
                flattenedSyntax.Add(new TempGreenPgnForegroundSyntax(trailing.BackgroundBefore, trailing.CommentNode));
            }

            CommentNodes = new SafeLazyObjectCollection<TempPgnSymbolWithTrivia>(
                flattenedSyntax.Count,
                index => new TempPgnSymbolWithTrivia(this, index, flattenedSyntax[index]));

            backgroundAfter = new SafeLazyObject<PgnBackgroundListSyntax>(() => new PgnBackgroundListSyntax(this, Green.BackgroundAfter));
        }
    }
}
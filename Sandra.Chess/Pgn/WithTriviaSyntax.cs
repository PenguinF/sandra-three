#region License
/*********************************************************************************
 * WithTriviaSyntax.cs
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
using Eutherion.Threading;
using System;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a syntax node together with its leading trivia.
    /// </summary>
    public sealed class GreenWithTriviaSyntax : IPlyFloatItemAnchor
    {
        /// <summary>
        /// Gets the leading trivia of the syntax node.
        /// </summary>
        public GreenPgnTriviaSyntax LeadingTrivia { get; }

        /// <summary>
        /// Gets the content syntax node which anchors the trivia.
        /// </summary>
        public IGreenPgnSymbol ContentNode { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length => LeadingTrivia.Length + ContentNode.Length;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenWithTriviaSyntax"/>.
        /// </summary>
        /// <param name="leadingTrivia">
        /// The leading trivia of the syntax node.
        /// </param>
        /// <param name="contentNode">
        /// The content syntax node which anchors the trivia.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="leadingTrivia"/> and/or <paramref name="contentNode"/> are null.
        /// </exception>
        public GreenWithTriviaSyntax(GreenPgnTriviaSyntax leadingTrivia, IGreenPgnSymbol contentNode)
        {
            LeadingTrivia = leadingTrivia ?? throw new ArgumentNullException(nameof(leadingTrivia));
            ContentNode = contentNode ?? throw new ArgumentNullException(nameof(contentNode));
        }

        GreenWithTriviaSyntax IPlyFloatItemAnchor.FirstWithTriviaNode => this;
    }

    /// <summary>
    /// Represents a syntax node together with its leading trivia.
    /// </summary>
    public abstract class WithTriviaSyntax : PgnSyntax
    {
        private readonly SafeLazyObject<PgnTriviaSyntax> leadingTrivia;

        /// <summary>
        /// Gets the leading trivia of the syntax node.
        /// </summary>
        public PgnTriviaSyntax LeadingTrivia => leadingTrivia.Object;

        /// <summary>
        /// Gets the content syntax node which anchors the trivia.
        /// </summary>
        public PgnSyntax ContentNode => ContentNodeUntyped;

        internal abstract PgnSyntax ContentNodeUntyped { get; }

        internal WithTriviaSyntax(GreenPgnTriviaSyntax greenLeadingTrivia)
        {
            leadingTrivia = new SafeLazyObject<PgnTriviaSyntax>(() => new PgnTriviaSyntax(this, greenLeadingTrivia));
        }
    }

    /// <summary>
    /// Represents a syntax node together with its leading trivia.
    /// </summary>
    /// <typeparam name="TSyntaxNode">
    /// The type of <see cref="PgnSyntax"/> syntax node.
    /// </typeparam>
    public abstract class WithTriviaSyntax<TSyntaxNode> : WithTriviaSyntax
        where TSyntaxNode : PgnSyntax
    {
        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenWithTriviaSyntax Green { get; }

        private readonly SafeLazyObject<TSyntaxNode> contentNode;

        /// <summary>
        /// Gets the content syntax node which anchors the trivia.
        /// </summary>
        /// <remarks>
        /// Intentionally hides the base <see cref="ContentNode"/> property.
        /// </remarks>
        public new TSyntaxNode ContentNode => contentNode.Object;

        internal sealed override PgnSyntax ContentNodeUntyped => ContentNode;

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public sealed override int Length => Green.Length;

        /// <summary>
        /// Gets the number of children of this syntax node.
        /// </summary>
        public sealed override int ChildCount => 2;

        /// <summary>
        /// Initializes the child at the given <paramref name="index"/> and returns it.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than 0 or greater than or equal to <see cref="ChildCount"/>.
        /// </exception>
        public sealed override PgnSyntax GetChild(int index)
        {
            if (index == 0) return LeadingTrivia;
            if (index == 1) return ContentNode;
            throw ExceptionUtil.ThrowListIndexOutOfRangeException();
        }

        /// <summary>
        /// Gets the start position of the child at the given <paramref name="index"/>, without initializing it.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than 0 or greater than or equal to <see cref="ChildCount"/>.
        /// </exception>
        public sealed override int GetChildStartPosition(int index)
        {
            if (index == 0) return 0;
            if (index == 1) return Green.LeadingTrivia.Length;
            throw ExceptionUtil.ThrowListIndexOutOfRangeException();
        }

        internal abstract TSyntaxNode CreateContentNode();

        internal WithTriviaSyntax(GreenWithTriviaSyntax green)
            : base(green.LeadingTrivia)
        {
            Green = green;

            contentNode = new SafeLazyObject<TSyntaxNode>(CreateContentNode);
        }
    }
}

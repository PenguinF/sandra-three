#region License
/*********************************************************************************
 * PgnSyntaxWithLeadingTrivia.cs
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
    /// Represents a syntax node together with its leading trivia.
    /// </summary>
    public abstract class GreenPgnSyntaxWithLeadingTrivia : ISpan
    {
        /// <summary>
        /// Gets the leading trivia of the syntax node.
        /// </summary>
        public GreenPgnTriviaSyntax LeadingTrivia { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public abstract int Length { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnSyntaxWithLeadingTrivia"/>.
        /// </summary>
        /// <param name="leadingTrivia">
        /// The leading trivia of the syntax node.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="leadingTrivia"/> is null.
        /// </exception>
        public GreenPgnSyntaxWithLeadingTrivia(GreenPgnTriviaSyntax leadingTrivia)
            => LeadingTrivia = leadingTrivia ?? throw new ArgumentNullException(nameof(leadingTrivia));
    }

    /// <summary>
    /// Represents a syntax node together with its leading trivia.
    /// </summary>
    /// <typeparam name="TSyntaxNode">
    /// The type of <see cref="IGreenPgnSymbol"/> syntax node.
    /// </typeparam>
    public class GreenPgnSyntaxWithLeadingTrivia<TSyntaxNode> : GreenPgnSyntaxWithLeadingTrivia where TSyntaxNode : IGreenPgnSymbol
    {
        /// <summary>
        /// Gets the inner syntax node.
        /// </summary>
        public TSyntaxNode SyntaxNode { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public sealed override int Length => LeadingTrivia.Length + SyntaxNode.Length;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnSyntaxWithLeadingTrivia"/>.
        /// </summary>
        /// <param name="leadingTrivia">
        /// The leading trivia of the syntax node.
        /// </param>
        /// <param name="syntaxNode">
        /// The inner syntax node.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="leadingTrivia"/> and/or <paramref name="syntaxNode"/> are null.
        /// </exception>
        public GreenPgnSyntaxWithLeadingTrivia(GreenPgnTriviaSyntax leadingTrivia, TSyntaxNode syntaxNode)
            : base(leadingTrivia)
        {
            if (syntaxNode == null) throw new ArgumentNullException(nameof(syntaxNode));
            SyntaxNode = syntaxNode;
        }
    }

    /// <summary>
    /// Represents a syntax node together with its leading trivia.
    /// </summary>
    public abstract class PgnSyntaxWithLeadingTrivia : PgnSyntax
    {
        private readonly SafeLazyObject<PgnTriviaSyntax> leadingTrivia;

        /// <summary>
        /// Gets the leading trivia of the syntax node.
        /// </summary>
        public PgnTriviaSyntax LeadingTrivia => leadingTrivia.Object;

        internal PgnSyntaxWithLeadingTrivia(GreenPgnSyntaxWithLeadingTrivia green)
        {
            leadingTrivia = new SafeLazyObject<PgnTriviaSyntax>(() => new PgnTriviaSyntax(this, green.LeadingTrivia));
        }
    }

    /// <summary>
    /// Represents a syntax node together with its leading trivia.
    /// </summary>
    /// <typeparam name="TGreenSyntaxNode">
    /// The type of <see cref="IGreenPgnSymbol"/> syntax node.
    /// </typeparam>
    /// <typeparam name="TSyntaxNode">
    /// The type of <see cref="PgnSyntax"/> syntax node.
    /// </typeparam>
    public abstract class PgnSyntaxWithLeadingTrivia<TGreenSyntaxNode, TSyntaxNode> : PgnSyntaxWithLeadingTrivia
        where TGreenSyntaxNode : IGreenPgnSymbol
        where TSyntaxNode : PgnSyntax
    {
        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnSyntaxWithLeadingTrivia<TGreenSyntaxNode> Green { get; }

        private readonly SafeLazyObject<TSyntaxNode> syntaxNode;

        /// <summary>
        /// Gets the inner syntax node.
        /// </summary>
        public TSyntaxNode SyntaxNode => syntaxNode.Object;

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public sealed override int Length => Green.Length;

        /// <summary>
        /// Gets the number of children of this syntax node.
        /// </summary>
        public sealed override int ChildCount => 2;

        /// <summary>
        /// Initializes the child at the given <paramref name="index"/> and returns it.
        /// </summary>
        public sealed override PgnSyntax GetChild(int index)
        {
            if (index == 0) return LeadingTrivia;
            if (index == 1) return SyntaxNode;
            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// Gets the start position of the child at the given <paramref name="index"/>, without initializing it.
        /// </summary>
        public sealed override int GetChildStartPosition(int index)
        {
            if (index == 0) return 0;
            if (index == 1) return Green.LeadingTrivia.Length;
            throw new IndexOutOfRangeException();
        }

        internal abstract TSyntaxNode CreateChildNode();

        internal PgnSyntaxWithLeadingTrivia(GreenPgnSyntaxWithLeadingTrivia<TGreenSyntaxNode> green)
            : base(green)
        {
            Green = green;

            syntaxNode = new SafeLazyObject<TSyntaxNode>(CreateChildNode);
        }
    }
}

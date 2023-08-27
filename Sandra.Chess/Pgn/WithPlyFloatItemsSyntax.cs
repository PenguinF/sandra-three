#region License
/*********************************************************************************
 * WithPlyFloatItemsSyntax.cs
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
using Eutherion.Text;
using Eutherion.Threading;
using System;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a syntax node which is an element of a ply, together with its leading floating items.
    /// </summary>
    public abstract class GreenWithPlyFloatItemsSyntax : ISpan
    {
        /// <summary>
        /// Gets the leading floating items of the syntax node.
        /// </summary>
        public ReadOnlySpanList<GreenWithTriviaSyntax> LeadingFloatItems { get; }

        /// <summary>
        /// Gets the ply content syntax node which anchors the floating items.
        /// </summary>
        public IPlyFloatItemAnchor PlyContentNode => PlyContentNodeUntyped;

        internal abstract IPlyFloatItemAnchor PlyContentNodeUntyped { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length => LeadingFloatItems.Length + PlyContentNode.Length;

        internal GreenWithPlyFloatItemsSyntax(ReadOnlySpanList<GreenWithTriviaSyntax> leadingFloatItems)
        {
            LeadingFloatItems = leadingFloatItems ?? throw new ArgumentNullException(nameof(leadingFloatItems));
        }
    }

    /// <summary>
    /// Represents a syntax node which is an element of a ply, together with its leading floating items.
    /// </summary>
    /// <typeparam name="TGreenSyntaxNode">
    /// The type of <see cref="PlyContentNode"/>. It must implement <see cref="IPlyFloatItemAnchor"/>
    /// </typeparam>
    public sealed class GreenWithPlyFloatItemsSyntax<TGreenSyntaxNode> : GreenWithPlyFloatItemsSyntax
        where TGreenSyntaxNode : IPlyFloatItemAnchor
    {
        /// <summary>
        /// Gets the ply content syntax node which anchors the floating items.
        /// </summary>
        /// <remarks>
        /// Intentionally hides the base <see cref="PlyContentNode"/> property.
        /// </remarks>
        public new TGreenSyntaxNode PlyContentNode { get; }

        internal sealed override IPlyFloatItemAnchor PlyContentNodeUntyped => PlyContentNode;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenWithPlyFloatItemsSyntax"/>.
        /// </summary>
        /// <param name="leadingFloatItems">
        /// The leading floating items of the syntax node.
        /// </param>
        /// <param name="plyContentNode">
        /// The ply content syntax node which anchors the float items.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="leadingFloatItems"/> and/or <paramref name="plyContentNode"/> are null.
        /// </exception>
        public GreenWithPlyFloatItemsSyntax(ReadOnlySpanList<GreenWithTriviaSyntax> leadingFloatItems, TGreenSyntaxNode plyContentNode)
            : base(leadingFloatItems)
        {
            if (plyContentNode == null) throw new ArgumentNullException(nameof(plyContentNode));
            PlyContentNode = plyContentNode;
        }
    }

    /// <summary>
    /// Represents a syntax node which is an element of a ply, together with its leading floating items.
    /// </summary>
    public abstract class WithPlyFloatItemsSyntax : PgnSyntax
    {
        private readonly SafeLazyObject<PgnPlyFloatItemListSyntax> leadingFloatItems;

        /// <summary>
        /// Gets the leading floating items of the syntax node.
        /// </summary>
        public PgnPlyFloatItemListSyntax LeadingFloatItems => leadingFloatItems.Object;

        /// <summary>
        /// Gets the content syntax node which anchors the floating items.
        /// </summary>
        public PgnSyntax PlyContentNode => PlyContentNodeUntyped;

        internal abstract PgnSyntax PlyContentNodeUntyped { get; }

        internal WithPlyFloatItemsSyntax(ReadOnlySpanList<GreenWithTriviaSyntax> greenLeadingFloatItems)
        {
            leadingFloatItems = new SafeLazyObject<PgnPlyFloatItemListSyntax>(() => new PgnPlyFloatItemListSyntax(this, greenLeadingFloatItems));
        }
    }

    /// <summary>
    /// Represents a syntax node which is an element of a ply, together with its leading floating items.
    /// </summary>
    /// <typeparam name="TSyntaxNode">
    /// The type of <see cref="PgnSyntax"/> syntax node.
    /// </typeparam>
    public abstract class WithPlyFloatItemsSyntax<TGreenSyntaxNode, TSyntaxNode> : WithPlyFloatItemsSyntax
        where TGreenSyntaxNode : IPlyFloatItemAnchor
        where TSyntaxNode : PgnSyntax
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public PgnPlySyntax Parent { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenWithPlyFloatItemsSyntax<TGreenSyntaxNode> Green { get; }

        private readonly SafeLazyObject<TSyntaxNode> plyContentNode;

        /// <summary>
        /// Gets the content syntax node which anchors the floating items.
        /// </summary>
        /// <remarks>
        /// Intentionally hides the base <see cref="PlyContentNode"/> property.
        /// </remarks>
        public new TSyntaxNode PlyContentNode => plyContentNode.Object;

        internal sealed override PgnSyntax PlyContentNodeUntyped => PlyContentNode;

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public sealed override int Length => Green.Length;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public sealed override PgnSyntax ParentSyntax => Parent;

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
            if (index == 0) return LeadingFloatItems;
            if (index == 1) return PlyContentNode;
            throw ExceptionUtility.ThrowListIndexOutOfRangeException();
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
            if (index == 1) return Green.LeadingFloatItems.Length;
            throw ExceptionUtility.ThrowListIndexOutOfRangeException();
        }

        internal abstract TSyntaxNode CreatePlyContentNode();

        internal WithPlyFloatItemsSyntax(PgnPlySyntax parent, GreenWithPlyFloatItemsSyntax<TGreenSyntaxNode> green)
            : base(green.LeadingFloatItems)
        {
            Parent = parent;
            Green = green;

            plyContentNode = new SafeLazyObject<TSyntaxNode>(CreatePlyContentNode);
        }
    }

    /// <summary>
    /// Marks a green syntax node as a potential anchor for floating items within a ply.
    /// It is implemented by both <see cref="GreenWithTriviaSyntax"/> and <see cref="GreenPgnVariationSyntax"/>.
    /// It exposes a method to get the first <see cref="IGreenPgnSymbol"/> node with its leading trivia,
    /// which is used for error reporting.
    /// </summary>
    public interface IPlyFloatItemAnchor : ISpan
    {
        /// <summary>
        /// Gets the first <see cref="GreenWithTriviaSyntax"/> which contains a <see cref="IGreenPgnSymbol"/>
        /// together with its leading trivia.
        /// </summary>
        GreenWithTriviaSyntax FirstWithTriviaNode { get; }
    }
}

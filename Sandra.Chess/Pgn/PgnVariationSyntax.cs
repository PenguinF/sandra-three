#region License
/*********************************************************************************
 * PgnVariationSyntax.cs
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

using System;
using System.Threading;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a syntax node which contains a side line, i.e. a list of plies and their surrounding parentheses.
    /// </summary>
    public sealed class GreenPgnVariationSyntax : IPlyFloatItemAnchor
    {
        /// <summary>
        /// Gets the opening parenthesis.
        /// </summary>
        public GreenWithTriviaSyntax ParenthesisOpen { get; }

        /// <summary>
        /// Gets the list of plies and trailing floating items that are not captured by a ply.
        /// </summary>
        public GreenPgnPlyListSyntax PliesWithFloatItems { get; }

        /// <summary>
        /// Gets the closing parenthesis. The closing parenthesis can be null.
        /// </summary>
        public GreenWithTriviaSyntax ParenthesisClose { get; }

        /// <summary>
        /// Returns the closing parenthesis's length or 0 if it is missing.
        /// </summary>
        public int ParenthesisCloseLength => ParenthesisClose == null ? 0 : ParenthesisClose.Length;

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnVariationSyntax"/>.
        /// </summary>
        /// <param name="parenthesisOpen">
        /// The opening parenthesis.
        /// </param>
        /// <param name="pliesWithFloatItems">
        /// The list of plies and trailing floating items that are not captured by a ply.
        /// </param>
        /// <param name="parenthesisClose">
        /// The closing parenthesis. This is an optional parameter.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="parenthesisOpen"/> and/or <paramref name="pliesWithFloatItems"/> is null.
        /// </exception>
        public GreenPgnVariationSyntax(
            GreenWithTriviaSyntax parenthesisOpen,
            GreenPgnPlyListSyntax pliesWithFloatItems,
            GreenWithTriviaSyntax parenthesisClose)
        {
            ParenthesisOpen = parenthesisOpen ?? throw new ArgumentNullException(nameof(parenthesisOpen));
            PliesWithFloatItems = pliesWithFloatItems ?? throw new ArgumentNullException(nameof(pliesWithFloatItems));
            ParenthesisClose = parenthesisClose;

            Length = parenthesisOpen.Length + pliesWithFloatItems.Length + ParenthesisCloseLength;
        }

        GreenWithTriviaSyntax IPlyFloatItemAnchor.FirstWithTriviaNode => ParenthesisOpen;
    }

    /// <summary>
    /// Represents a syntax node which contains a side line, i.e. a list of plies and their surrounding parentheses.
    /// </summary>
    public sealed class PgnVariationSyntax : PgnSyntax
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public PgnVariationWithFloatItemsSyntax Parent { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnVariationSyntax Green { get; }

        private readonly SafeLazyObject<PgnParenthesisOpenWithTriviaSyntax> parenthesisOpen;

        /// <summary>
        /// Gets the opening parenthesis.
        /// </summary>
        public PgnParenthesisOpenWithTriviaSyntax ParenthesisOpen => parenthesisOpen.Object;

        private readonly SafeLazyObject<PgnPlyListSyntax> pliesWithFloatItems;

        /// <summary>
        /// Gets the list of plies and trailing floating items that are not captured by a ply.
        /// </summary>
        public PgnPlyListSyntax PliesWithFloatItems => pliesWithFloatItems.Object;

        private readonly SafeLazyChildSyntaxOrEmpty<PgnParenthesisCloseWithTriviaSyntax> lazyParenthesisCloseOrEmpty;

        /// <summary>
        /// Gets the closing parenthesis. The closing parenthesis can be null.
        /// </summary>
        public PgnParenthesisCloseWithTriviaSyntax ParenthesisClose => lazyParenthesisCloseOrEmpty.ChildNodeOrNull;

        /// <summary>
        /// Gets the closing parenthesis. The closing parenthesis can be <see cref="PgnEmptySyntax"/>.
        /// </summary>
        public PgnSyntax ParenthesisCloseOrEmpty => lazyParenthesisCloseOrEmpty.ChildNodeOrEmpty;

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Green.LeadingFloatItems.Length;

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
        public override int ChildCount => 3;

        /// <summary>
        /// Initializes the child at the given <paramref name="index"/> and returns it.
        /// </summary>
        public override PgnSyntax GetChild(int index)
        {
            if (index == 0) return ParenthesisOpen;
            if (index == 1) return PliesWithFloatItems;
            if (index == 2) return ParenthesisCloseOrEmpty;
            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// Gets the start position of the child at the given <paramref name="index"/>, without initializing it.
        /// </summary>
        public sealed override int GetChildStartPosition(int index)
        {
            if (index == 0) return 0;
            if (index == 1) return Green.ParenthesisOpen.Length;
            if (index == 2) return Green.ParenthesisOpen.Length + Green.PliesWithFloatItems.Length;
            throw new IndexOutOfRangeException();
        }

        internal PgnVariationSyntax(PgnVariationWithFloatItemsSyntax parent, GreenPgnVariationSyntax green)
        {
            Parent = parent;
            Green = green;

            parenthesisOpen = new SafeLazyObject<PgnParenthesisOpenWithTriviaSyntax>(
                () => new PgnParenthesisOpenWithTriviaSyntax(this, Green.ParenthesisOpen));

            pliesWithFloatItems = new SafeLazyObject<PgnPlyListSyntax>(
                () => new PgnPlyListSyntax(this, Green.PliesWithFloatItems));

            if (green.ParenthesisClose != null)
            {
                lazyParenthesisCloseOrEmpty = new SafeLazyChildSyntaxOrEmpty<PgnParenthesisCloseWithTriviaSyntax>(
                    () => new PgnParenthesisCloseWithTriviaSyntax(this, Green.ParenthesisClose));
            }
            else
            {
                lazyParenthesisCloseOrEmpty = new SafeLazyChildSyntaxOrEmpty<PgnParenthesisCloseWithTriviaSyntax>(
                    this, green.ParenthesisOpen.Length + green.PliesWithFloatItems.Length);
            }
        }
    }
}

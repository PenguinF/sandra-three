#region License
/*********************************************************************************
 * PgnEmptySyntax.cs
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
using System;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents an empty placeholder node.
    /// </summary>
    // This exists primarily to simplify length and count offset calculation code.
    public sealed class PgnEmptySyntax : PgnSyntax
    {
        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => 0;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override PgnSyntax ParentSyntax { get; }

        internal PgnEmptySyntax(PgnSyntax parentSyntax, int start)
        {
            ParentSyntax = parentSyntax;
            Start = start;
        }
    }

    /// <summary>
    /// Wraps either an empty syntax or a lazy child node of the specified type.
    /// </summary>
    internal struct SafeLazyChildSyntaxOrEmpty<TChildSyntax> where TChildSyntax : PgnSyntax
    {
        private readonly SafeLazyObject<TChildSyntax> lazyNodeIfNonEmpty;
        private readonly PgnEmptySyntax nodeIfEmpty;

        public TChildSyntax ChildNodeOrNull => nodeIfEmpty == null ? lazyNodeIfNonEmpty.Object : null;
        public PgnSyntax ChildNodeOrEmpty => nodeIfEmpty == null ? lazyNodeIfNonEmpty.Object : (PgnSyntax)nodeIfEmpty;

        /// <summary>
        /// Initializes as lazy child node.
        /// </summary>
        public SafeLazyChildSyntaxOrEmpty(Func<TChildSyntax> childConstructor)
        {
            lazyNodeIfNonEmpty = new SafeLazyObject<TChildSyntax>(childConstructor);
            nodeIfEmpty = null;
        }

        /// <summary>
        /// Initializes as empty.
        /// </summary>
        public SafeLazyChildSyntaxOrEmpty(PgnSyntax parent, int start)
        {
            lazyNodeIfNonEmpty = default;
            nodeIfEmpty = new PgnEmptySyntax(parent, start);
        }
    }
}

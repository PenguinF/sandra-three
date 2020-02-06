#region License
/*********************************************************************************
 * PgnBackgroundSyntax.cs
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

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a single background node in an abstract PGN syntax tree.
    /// Use <see cref="GreenPgnBackgroundSyntaxVisitor"/> overrides to distinguish between implementations of this type.
    /// </summary>
    public abstract class GreenPgnBackgroundSyntax : ISpan
    {
        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public abstract int Length { get; }

        public abstract void Accept(GreenPgnBackgroundSyntaxVisitor visitor);
        public abstract TResult Accept<TResult>(GreenPgnBackgroundSyntaxVisitor<TResult> visitor);
        public abstract TResult Accept<T, TResult>(GreenPgnBackgroundSyntaxVisitor<T, TResult> visitor, T arg);
    }

    /// <summary>
    /// Represents a single background node in an abstract PGN syntax tree.
    /// </summary>
    public abstract class PgnBackgroundSyntax : PgnSyntax
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public PgnSyntaxNodes Parent { get; }

        /// <summary>
        /// Gets the index of this syntax node in its parent.
        /// </summary>
        public int ParentIndex { get; }

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Green.ChildNodes.GetElementOffset(ParentIndex);

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override PgnSyntax ParentSyntax => Parent;

        internal PgnBackgroundSyntax(PgnSyntaxNodes parent, int parentIndex)
        {
            Parent = parent;
            ParentIndex = parentIndex;
        }
    }
}

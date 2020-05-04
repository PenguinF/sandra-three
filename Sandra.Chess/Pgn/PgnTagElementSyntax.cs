#region License
/*********************************************************************************
 * PgnTagElementSyntax.cs
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
    /// Represents a node containing a single tag section element in an abstract PGN syntax tree.
    /// Use <see cref="GreenPgnTagElementSyntaxVisitor"/> overrides to distinguish between implementations of this type.
    /// </summary>
    public abstract class GreenPgnTagElementSyntax : ISpan
    {
        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public abstract int Length { get; }

        public abstract void Accept(GreenPgnTagElementSyntaxVisitor visitor);
        public abstract TResult Accept<TResult>(GreenPgnTagElementSyntaxVisitor<TResult> visitor);
        public abstract TResult Accept<T, TResult>(GreenPgnTagElementSyntaxVisitor<T, TResult> visitor, T arg);
    }

    /// <summary>
    /// Represents a node containing a single tag section element in an abstract PGN syntax tree.
    /// Use <see cref="PgnTagElementSyntaxVisitor"/> overrides to distinguish between implementations of this type.
    /// </summary>
    public abstract class PgnTagElementSyntax : PgnSyntax
    {
        /// <summary>
        /// Gets the parent syntax node of this instance. Returns null for the root node.
        /// </summary>
        public PgnTagElementWithTriviaSyntax Parent { get; }

        public IGreenPgnSymbol Green { get; }

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Green.LeadingTrivia.Length;

        public override int Length => Green.Length;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override PgnSyntax ParentSyntax => Parent;

        internal PgnTagElementSyntax(PgnTagElementWithTriviaSyntax parent, IGreenPgnSymbol green)
        {
            Parent = parent;
            Green = green;
        }

        public abstract void Accept(PgnTagElementSyntaxVisitor visitor);
        public abstract TResult Accept<TResult>(PgnTagElementSyntaxVisitor<TResult> visitor);
        public abstract TResult Accept<T, TResult>(PgnTagElementSyntaxVisitor<T, TResult> visitor, T arg);
    }
}

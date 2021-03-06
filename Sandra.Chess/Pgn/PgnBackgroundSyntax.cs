﻿#region License
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

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a single background node in an abstract PGN syntax tree.
    /// </summary>
    public abstract class GreenPgnBackgroundSyntax : IGreenPgnSymbol
    {
        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public abstract int Length { get; }

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public abstract PgnSymbolType SymbolType { get; }

        internal abstract PgnBackgroundSyntax CreateRedNode(PgnBackgroundListSyntax parent, int parentIndex);
    }

    /// <summary>
    /// Represents a single background node in an abstract PGN syntax tree.
    /// Use <see cref="PgnBackgroundSyntaxVisitor"/> overrides to distinguish between implementations of this type.
    /// </summary>
    public abstract class PgnBackgroundSyntax : PgnSyntax
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public PgnBackgroundListSyntax Parent { get; }

        /// <summary>
        /// Gets the index of this syntax node in its parent.
        /// </summary>
        public int ParentIndex { get; }

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Green.GetElementOffset(ParentIndex);

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override PgnSyntax ParentSyntax => Parent;

        internal PgnBackgroundSyntax(PgnBackgroundListSyntax parent, int parentIndex)
        {
            Parent = parent;
            ParentIndex = parentIndex;
        }

        public abstract void Accept(PgnBackgroundSyntaxVisitor visitor);
        public abstract TResult Accept<TResult>(PgnBackgroundSyntaxVisitor<TResult> visitor);
        public abstract TResult Accept<T, TResult>(PgnBackgroundSyntaxVisitor<T, TResult> visitor, T arg);
    }
}

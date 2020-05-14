#region License
/*********************************************************************************
 * PgnPlyFloatItemSyntax.cs
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
    /// Represents any grammatically significant node that is allowed to freely float around within a ply.
    /// Other than a period character between a move number and move, this generally corresponds to some kind of error.
    /// </summary>
    public abstract class GreenPgnPlyFloatItemSyntax : IGreenPgnSymbol
    {
        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public abstract int Length { get; }

        /// <summary>
        /// Gets the type of the underlying symbol.
        /// </summary>
        public abstract PgnSymbolType SymbolType { get; }

        internal abstract PgnPlyFloatItemSyntax CreateRedNode(PgnPlyFloatItemWithTriviaSyntax parent);
    }

    /// <summary>
    /// Represents any grammatically significant node that is allowed to freely float around within a ply.
    /// Other than a period character between a move number and move, this generally represents some kind of error state.
    /// </summary>
    public abstract class PgnPlyFloatItemSyntax : PgnSyntax, IPgnSymbol
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public PgnPlyFloatItemWithTriviaSyntax Parent { get; }

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public sealed override int Start => Parent.Green.LeadingTrivia.Length;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public sealed override PgnSyntax ParentSyntax => Parent;

        internal PgnPlyFloatItemSyntax(PgnPlyFloatItemWithTriviaSyntax parent) => Parent = parent;

        public abstract void Accept(PgnSymbolVisitor visitor);
        public abstract TResult Accept<TResult>(PgnSymbolVisitor<TResult> visitor);
        public abstract TResult Accept<T, TResult>(PgnSymbolVisitor<T, TResult> visitor, T arg);
    }

    /// <summary>
    /// Represents a floating item node within a ply, together with its leading trivia.
    /// </summary>
    public sealed class PgnPlyFloatItemWithTriviaSyntax : WithTriviaSyntax<PgnPlyFloatItemSyntax>
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public PgnPlyFloatItemListSyntax Parent { get; }

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

        internal override PgnPlyFloatItemSyntax CreateContentNode() => ((GreenPgnPlyFloatItemSyntax)Green.ContentNode).CreateRedNode(this);

        internal PgnPlyFloatItemWithTriviaSyntax(PgnPlyFloatItemListSyntax parent, int parentIndex, GreenWithTriviaSyntax green)
            : base(green)
        {
            Parent = parent;
            ParentIndex = parentIndex;
        }
    }
}

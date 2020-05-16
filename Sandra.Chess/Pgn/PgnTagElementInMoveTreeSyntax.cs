#region License
/*********************************************************************************
 * PgnTagElementInMoveTreeSyntax.cs
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

using System;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a <see cref="GreenPgnTagElementSyntax"/> which was found in the middle of a move tree section, but did not trigger a new tag section.
    /// </summary>
    public sealed class GreenPgnTagElementInMoveTreeSyntax : GreenPgnPlyFloatItemSyntax
    {
        /// <summary>
        /// Gets the tag element which was ignored in the move tree section.
        /// </summary>
        public IGreenPgnSymbol TagElement { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public override int Length => TagElement.Length;

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public override PgnSymbolType SymbolType => TagElement.SymbolType;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnTagElementInMoveTreeSyntax"/>.
        /// </summary>
        /// <param name="tagElement">
        /// The tag element which was ignored in the move tree section.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="tagElement"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="tagElement"/> is not a tag element, -or- it is a tag name which should have been converted to an unrecognized move instead.
        /// </exception>
        public GreenPgnTagElementInMoveTreeSyntax(IGreenPgnSymbol tagElement)
        {
            TagElement = tagElement ?? throw new ArgumentNullException(nameof(tagElement));

            if (tagElement.SymbolType < PgnSymbolType.BracketOpen
                || tagElement.SymbolType > PgnSymbolType.ErrorTagValue
                || tagElement.SymbolType == PgnSymbolType.TagName)
            {
                throw new ArgumentException($"{nameof(tagElement)} has illegal symbol type {tagElement.SymbolType}", nameof(tagElement));
            }
        }

        internal override PgnPlyFloatItemSyntax CreateRedNode(PgnPlyFloatItemWithTriviaSyntax parent) => new PgnTagElementInMoveTreeSyntax(parent, this);
    }

    /// <summary>
    /// Represents a <see cref="PgnTagElementSyntax"/> which was found in the middle of a move tree section, but did not trigger a new tag section.
    /// </summary>
    public sealed class PgnTagElementInMoveTreeSyntax : PgnPlyFloatItemSyntax
    {
        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnTagElementInMoveTreeSyntax Green { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        internal PgnTagElementInMoveTreeSyntax(PgnPlyFloatItemWithTriviaSyntax parent, GreenPgnTagElementInMoveTreeSyntax green)
            : base(parent)
        {
            Green = green;
        }

        public override void Accept(PgnSymbolVisitor visitor) => visitor.VisitTagElementInMoveTreeSyntax(this);
        public override TResult Accept<TResult>(PgnSymbolVisitor<TResult> visitor) => visitor.VisitTagElementInMoveTreeSyntax(this);
        public override TResult Accept<T, TResult>(PgnSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitTagElementInMoveTreeSyntax(this, arg);
    }
}

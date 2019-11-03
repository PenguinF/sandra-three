#region License
/*********************************************************************************
 * JsonBackgroundSyntax.cs
 *
 * Copyright (c) 2004-2019 Henk Nicolai
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

using Eutherion.Utils;
using System;
using System.Collections.Generic;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a node with background symbols in an abstract json syntax tree.
    /// </summary>
    public sealed class GreenJsonBackgroundSyntax : ISpan
    {
        /// <summary>
        /// Gets the empty <see cref="GreenJsonBackgroundSyntax"/>.
        /// </summary>
        public static readonly GreenJsonBackgroundSyntax Empty = new GreenJsonBackgroundSyntax(ReadOnlySpanList<JsonSymbol>.Empty);

        /// <summary>
        /// Initializes a new instance of <see cref="GreenJsonBackgroundSyntax"/>.
        /// </summary>
        /// <param name="source">
        /// The source enumeration of <see cref="JsonSymbol"/>.
        /// </param>
        /// <returns>
        /// The new <see cref="GreenJsonBackgroundSyntax"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> is null.
        /// </exception>
        public static GreenJsonBackgroundSyntax Create(IEnumerable<JsonSymbol> source)
        {
            var readOnlyBackground = ReadOnlySpanList<JsonSymbol>.Create(source);
            if (readOnlyBackground.Count == 0) return Empty;
            return new GreenJsonBackgroundSyntax(readOnlyBackground);
        }

        /// <summary>
        /// Gets the read-only list with background symbols.
        /// </summary>
        public ReadOnlySpanList<JsonSymbol> BackgroundSymbols { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax.
        /// </summary>
        public int Length => BackgroundSymbols.Length;

        private GreenJsonBackgroundSyntax(ReadOnlySpanList<JsonSymbol> backgroundSymbols) => BackgroundSymbols = backgroundSymbols;
    }

    /// <summary>
    /// Represents a node with background symbols in an abstract json syntax tree.
    /// </summary>
    public sealed class JsonBackgroundSyntax : JsonSyntax
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public Union<JsonValueWithBackgroundSyntax, JsonMultiValueSyntax> Parent { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenJsonBackgroundSyntax Green { get; }

        /// <summary>
        /// Gets the read-only list with background symbols.
        /// </summary>
        public ReadOnlySpanList<JsonSymbol> BackgroundSymbols => Green.BackgroundSymbols;

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Match(
            whenOption1: valueWithBackgroundSyntax => 0,
            whenOption2: multiValueSyntax => multiValueSyntax.Length - Length);

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override JsonSyntax ParentSyntax => Parent.Match<JsonSyntax>(
            whenOption1: x => x,
            whenOption2: x => x);

        internal JsonBackgroundSyntax(JsonValueWithBackgroundSyntax backgroundBeforeParent)
        {
            Parent = backgroundBeforeParent;
            Green = backgroundBeforeParent.Green.BackgroundBefore;
        }

        internal JsonBackgroundSyntax(JsonMultiValueSyntax backgroundAfterParent)
        {
            Parent = backgroundAfterParent;
            Green = backgroundAfterParent.Green.BackgroundAfter;
        }

        // Treat JsonBackgroundSyntax as a terminal symbol.
        // Can always specify further for each individual background JsonSymbol if the need arises.
        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitBackgroundSyntax(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitBackgroundSyntax(this);
        public override TResult Accept<T, TResult>(JsonTerminalSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitBackgroundSyntax(this, arg);
    }
}

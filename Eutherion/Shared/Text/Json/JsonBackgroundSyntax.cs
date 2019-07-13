﻿#region License
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

using System;
using System.Collections.Generic;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a node with background symbols in an abstract json syntax tree.
    /// </summary>
    public sealed class JsonBackgroundSyntax : ISpan
    {
        /// <summary>
        /// Gets the empty <see cref="JsonBackgroundSyntax"/>.
        /// </summary>
        public static readonly JsonBackgroundSyntax Empty = new JsonBackgroundSyntax(ReadOnlySpanList<JsonSymbol>.Empty);

        /// <summary>
        /// Initializes a new instance of <see cref="JsonBackgroundSyntax"/>.
        /// </summary>
        /// <param name="source">
        /// The source enumeration of <see cref="JsonSymbol"/>.
        /// </param>
        /// <returns>
        /// The new <see cref="JsonBackgroundSyntax"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> is null.
        /// </exception>
        public static JsonBackgroundSyntax Create(IEnumerable<JsonSymbol> source)
        {
            var readOnlyBackground = ReadOnlySpanList<JsonSymbol>.Create(source);
            if (readOnlyBackground.Count == 0) return Empty;
            return new JsonBackgroundSyntax(readOnlyBackground);
        }

        /// <summary>
        /// Gets the read-only list with background symbols.
        /// </summary>
        public ReadOnlySpanList<JsonSymbol> BackgroundSymbols { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax.
        /// </summary>
        public int Length => BackgroundSymbols.Length;

        private JsonBackgroundSyntax(ReadOnlySpanList<JsonSymbol> backgroundSymbols) => BackgroundSymbols = backgroundSymbols;
    }
}

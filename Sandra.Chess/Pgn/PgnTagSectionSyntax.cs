#region License
/*********************************************************************************
 * PgnTagSectionSyntax.cs
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
using Sandra.Chess.Pgn.Temp;
using System;
using System.Collections.Generic;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a syntax node which contains a collection of <see cref="GreenPgnTagPairSyntax"/> instances.
    /// </summary>
    public sealed class GreenPgnTagSectionSyntax : IGreenPgnTopLevelSyntax, ISpan
    {
        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length => TagPairNodes.Length;

        /// <summary>
        /// Gets the tag pair nodes.
        /// </summary>
        public ReadOnlySpanList<GreenPgnTagPairSyntax> TagPairNodes { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnTagSectionSyntax"/>.
        /// </summary>
        /// <param name="tagPairNodes">
        /// The tag pair nodes.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="tagPairNodes"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="tagPairNodes"/> is empty.
        /// </exception>
        public GreenPgnTagSectionSyntax(IEnumerable<GreenPgnTagPairSyntax> tagPairNodes)
        {
            if (tagPairNodes == null) throw new ArgumentNullException(nameof(tagPairNodes));
            TagPairNodes = ReadOnlySpanList<GreenPgnTagPairSyntax>.Create(tagPairNodes);
            if (TagPairNodes.Count == 0) throw new ArgumentException($"{nameof(tagPairNodes)} is empty", nameof(tagPairNodes));
        }
    }
}

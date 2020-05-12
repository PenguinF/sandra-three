#region License
/*********************************************************************************
 * WithPlyFloatItemsSyntax.cs
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
using System;
using System.Collections.Generic;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a syntax node which is an element of a ply, together with its leading floating items.
    /// </summary>
    public sealed class GreenWithPlyFloatItemsSyntax : ISpan
    {
        /// <summary>
        /// Gets the leading floating items of the syntax node.
        /// </summary>
        public ReadOnlySpanList<GreenWithTriviaSyntax> LeadingFloatItems { get; }

        /// <summary>
        /// Gets the ply content syntax node which anchors the floating items.
        /// </summary>
        public GreenWithTriviaSyntax PlyContentNode { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length => LeadingFloatItems.Length + PlyContentNode.Length;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenWithPlyFloatItemsSyntax"/>.
        /// </summary>
        /// <param name="leadingFloatItems">
        /// The leading floating items of the syntax node.
        /// </param>
        /// <param name="plyContentNode">
        /// The ply content syntax node which anchors the float items.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="leadingFloatItems"/> and/or <paramref name="plyContentNode"/> are null.
        /// </exception>
        public GreenWithPlyFloatItemsSyntax(IEnumerable<GreenWithTriviaSyntax> leadingFloatItems, GreenWithTriviaSyntax plyContentNode)
        {
            if (leadingFloatItems == null) throw new ArgumentNullException(nameof(leadingFloatItems));
            LeadingFloatItems = ReadOnlySpanList<GreenWithTriviaSyntax>.Create(leadingFloatItems);
            PlyContentNode = plyContentNode ?? throw new ArgumentNullException(nameof(plyContentNode));
        }
    }
}

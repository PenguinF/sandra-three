#region License
/*********************************************************************************
 * JsonListSyntax.cs
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
    /// Represents a list syntax node.
    /// </summary>
    public sealed class JsonListSyntax : JsonValueSyntax
    {
        public ReadOnlyList<JsonMultiValueSyntax> ListItemNodes { get; }

        private readonly int[] ListItemNodePositions;

        public bool MissingSquareBracketClose { get; }

        /// <summary>
        /// Returns ListItemNodes.Count, or one less if the last element is a JsonMissingValueSyntax.
        /// </summary>
        public int FilteredListItemNodeCount
        {
            get
            {
                int count = ListItemNodes.Count;

                // Discard last item if it's a missing value, so that a trailing comma is ignored.
                if (ListItemNodes[count - 1].ValueNode.ContentNode is JsonMissingValueSyntax)
                {
                    return count - 1;
                }

                return count;
            }
        }

        public override int Length { get; }

        public JsonListSyntax(IEnumerable<JsonMultiValueSyntax> listItemNodes, bool missingSquareBracketClose)
        {
            ListItemNodes = ReadOnlyList<JsonMultiValueSyntax>.Create(listItemNodes);

            if (ListItemNodes.Count == 0)
            {
                throw new ArgumentException($"{nameof(listItemNodes)} cannot be empty", nameof(listItemNodes));
            }

            MissingSquareBracketClose = missingSquareBracketClose;

            ListItemNodePositions = new int[ListItemNodes.Count - 1];
            int cumulativeLength = ListItemNodes[0].Length;

            for (int i = 1; i < ListItemNodes.Count; i++)
            {
                cumulativeLength += JsonComma.CommaLength;
                ListItemNodePositions[i - 1] = cumulativeLength;
                cumulativeLength += ListItemNodes[i].Length;
            }

            if (!missingSquareBracketClose)
            {
                cumulativeLength += JsonSquareBracketClose.SquareBracketCloseLength;
            }

            Length = JsonSquareBracketOpen.SquareBracketOpenLength + cumulativeLength;
        }

        /// <summary>
        /// Gets the start position of an element node relative to the start position of this <see cref="JsonListSyntax"/>.
        /// </summary>
        public int GetElementNodeStart(int index) => JsonSquareBracketOpen.SquareBracketOpenLength + (index == 0 ? 0 : ListItemNodePositions[index - 1]);

        public override void Accept(JsonValueSyntaxVisitor visitor) => visitor.VisitListSyntax(this);
        public override TResult Accept<TResult>(JsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitListSyntax(this);
        public override TResult Accept<T, TResult>(JsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitListSyntax(this, arg);
    }
}

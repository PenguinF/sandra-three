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
        public ReadOnlyList<JsonMultiValueSyntax> ElementNodes { get; }

        /// <summary>
        /// Returns ElementNodes.Count, or one less if the last element is a JsonMissingValueSyntax.
        /// </summary>
        public int FilteredElementNodeCount
        {
            get
            {
                int count = ElementNodes.Count;

                // Discard last item if it's a missing value, so that a trailing comma is ignored.
                if (ElementNodes[count - 1].ValueNode.ContentNode is JsonMissingValueSyntax)
                {
                    return count - 1;
                }

                return count;
            }
        }

        public override int Length { get; }

        public JsonListSyntax(IEnumerable<JsonMultiValueSyntax> elementNodes, bool missingSquareBracketClose)
        {
            ElementNodes = ReadOnlyList<JsonMultiValueSyntax>.Create(elementNodes);

            if (ElementNodes.Count == 0)
            {
                throw new ArgumentException($"{nameof(elementNodes)} cannot be empty", nameof(elementNodes));
            }

            // This code assumes that JsonSquareBracketOpen.SquareBracketOpenLength == JsonComma.CommaLength.
            // The first iteration should formally be SquareBracketOpenLength rather than CommaLength.
            int cumulativeLength = 0;

            for (int i = 0; i < ElementNodes.Count; i++)
            {
                cumulativeLength += JsonComma.CommaLength;
                cumulativeLength += ElementNodes[i].Length;
            }

            if (!missingSquareBracketClose)
            {
                cumulativeLength += JsonSquareBracketClose.SquareBracketCloseLength;
            }

            Length = cumulativeLength;
        }

        public override void Accept(JsonValueSyntaxVisitor visitor) => visitor.VisitListSyntax(this);
        public override TResult Accept<TResult>(JsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitListSyntax(this);
        public override TResult Accept<T, TResult>(JsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitListSyntax(this, arg);
    }
}

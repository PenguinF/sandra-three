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
        public ReadOnlySeparatedSpanList<JsonMultiValueSyntax, JsonComma> ListItemNodes { get; }

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
            ListItemNodes = ReadOnlySeparatedSpanList<JsonMultiValueSyntax, JsonComma>.Create(listItemNodes, JsonComma.Value);

            if (ListItemNodes.Count == 0)
            {
                throw new ArgumentException($"{nameof(listItemNodes)} cannot be empty", nameof(listItemNodes));
            }

            MissingSquareBracketClose = missingSquareBracketClose;

            Length = JsonSquareBracketOpen.SquareBracketOpenLength
                   + ListItemNodes.Length
                   + (missingSquareBracketClose ? 0 : JsonSquareBracketClose.SquareBracketCloseLength);
        }

        /// <summary>
        /// Gets the start position of an element node relative to the start position of this <see cref="JsonListSyntax"/>.
        /// </summary>
        public int GetElementNodeStart(int index) => JsonSquareBracketOpen.SquareBracketOpenLength + ListItemNodes.GetElementOffset(index);

        public override void Accept(JsonValueSyntaxVisitor visitor) => visitor.VisitListSyntax(this);
        public override TResult Accept<TResult>(JsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitListSyntax(this);
        public override TResult Accept<T, TResult>(JsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitListSyntax(this, arg);
    }

    public sealed class RedJsonListSyntax : RedJsonValueSyntax
    {
        public JsonListSyntax Green { get; }

        // Always create the [ and ], avoid overhead of SafeLazyObject.
        public RedJsonSquareBracketOpen SquareBracketOpen { get; }

        // Always create the [ and ], avoid overhead of SafeLazyObject.
        public Maybe<RedJsonSquareBracketClose> SquareBracketClose { get; }

        public override int Length => Green.Length;

        internal RedJsonListSyntax(RedJsonValueWithBackgroundSyntax parent, JsonListSyntax green) : base(parent)
        {
            Green = green;

            SquareBracketOpen = new RedJsonSquareBracketOpen(this);

            SquareBracketClose = green.MissingSquareBracketClose
                               ? Maybe<RedJsonSquareBracketClose>.Nothing
                               : new RedJsonSquareBracketClose(this);
        }

        public override void Accept(RedJsonValueSyntaxVisitor visitor) => visitor.VisitListSyntax(this);
        public override TResult Accept<TResult>(RedJsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitListSyntax(this);
        public override TResult Accept<T, TResult>(RedJsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitListSyntax(this, arg);
    }
}

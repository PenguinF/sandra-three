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
    public sealed class GreenJsonListSyntax : GreenJsonValueSyntax
    {
        public ReadOnlySeparatedSpanList<GreenJsonMultiValueSyntax, JsonComma> ListItemNodes { get; }

        public bool MissingSquareBracketClose { get; }

        /// <summary>
        /// Returns ListItemNodes.Count, or one less if the last element is a GreenJsonMissingValueSyntax.
        /// </summary>
        public int FilteredListItemNodeCount
        {
            get
            {
                int count = ListItemNodes.Count;

                // Discard last item if it's a missing value, so that a trailing comma is ignored.
                if (ListItemNodes[count - 1].ValueNode.ContentNode is GreenJsonMissingValueSyntax)
                {
                    return count - 1;
                }

                return count;
            }
        }

        public override int Length { get; }

        public GreenJsonListSyntax(IEnumerable<GreenJsonMultiValueSyntax> listItemNodes, bool missingSquareBracketClose)
        {
            ListItemNodes = ReadOnlySeparatedSpanList<GreenJsonMultiValueSyntax, JsonComma>.Create(listItemNodes, JsonComma.Value);

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
        /// Gets the start position of an element node relative to the start position of this <see cref="GreenJsonListSyntax"/>.
        /// </summary>
        public int GetElementNodeStart(int index) => JsonSquareBracketOpen.SquareBracketOpenLength + ListItemNodes.GetElementOffset(index);

        public override void Accept(GreenJsonValueSyntaxVisitor visitor) => visitor.VisitListSyntax(this);
        public override TResult Accept<TResult>(GreenJsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitListSyntax(this);
        public override TResult Accept<T, TResult>(GreenJsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitListSyntax(this, arg);
    }

    public sealed class JsonListSyntax : JsonValueSyntax
    {
        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenJsonListSyntax Green { get; }

        // Always create the [ and ], avoid overhead of SafeLazyObject.
        public JsonSquareBracketOpenSyntax SquareBracketOpen { get; }

        private readonly SafeLazyObjectCollection<JsonMultiValueSyntax> listItemNodes;
        private readonly SafeLazyObjectCollection<JsonCommaSyntax> commas;

        // Always create the [ and ], avoid overhead of SafeLazyObject.
        public Maybe<JsonSquareBracketCloseSyntax> SquareBracketClose { get; }

        public int ListItemNodeCount => listItemNodes.Count;

        public JsonMultiValueSyntax GetListItemNode(int index)
        {
            return listItemNodes.Get(index, i => new JsonMultiValueSyntax(this, i, Green.ListItemNodes[i]));
        }

        public int CommaCount => commas.Count;

        public JsonCommaSyntax GetComma(int index)
        {
            return commas.Get(index, i => new JsonCommaSyntax(this, i));
        }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        /// <summary>
        /// Gets the number of children of this syntax node.
        /// </summary>
        public override int ChildCount => ListItemNodeCount + CommaCount + (Green.MissingSquareBracketClose ? 1 : 2);

        /// <summary>
        /// Initializes the child at the given <paramref name="index"/> and returns it.
        /// </summary>
        public override JsonSyntax GetChild(int index)
        {
            if (index == 0) return SquareBracketOpen;

            index--;
            int itemAndCommaCount = ListItemNodeCount + CommaCount;

            if (index < itemAndCommaCount)
            {
                if ((index & 1) == 0) return GetListItemNode(index >> 1);
                return GetComma(index >> 1);
            }

            if (index == itemAndCommaCount && SquareBracketClose.IsJust(out JsonSquareBracketCloseSyntax jsonSquareBracketClose))
            {
                return jsonSquareBracketClose;
            }

            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// Gets the start position of the child at the given <paramref name="index"/>, without initializing it.
        /// </summary>
        public override int GetChildStartPosition(int index)
        {
            if (index == 0) return 0;

            index--;
            int itemAndCommaCount = ListItemNodeCount + CommaCount;

            if (index < itemAndCommaCount)
            {
                return Green.ListItemNodes.GetElementOrSeparatorOffset(index);
            }

            if (index == itemAndCommaCount && !Green.MissingSquareBracketClose)
            {
                return Length - JsonSquareBracketClose.SquareBracketCloseLength;
            }

            throw new IndexOutOfRangeException();
        }

        internal JsonListSyntax(JsonValueWithBackgroundSyntax parent, GreenJsonListSyntax green) : base(parent)
        {
            Green = green;

            SquareBracketOpen = new JsonSquareBracketOpenSyntax(this);

            int listItemNodeCount = green.ListItemNodes.Count;
            listItemNodes = listItemNodes = new SafeLazyObjectCollection<JsonMultiValueSyntax>(listItemNodeCount);
            commas = new SafeLazyObjectCollection<JsonCommaSyntax>(listItemNodeCount - 1);

            SquareBracketClose = green.MissingSquareBracketClose
                               ? Maybe<JsonSquareBracketCloseSyntax>.Nothing
                               : new JsonSquareBracketCloseSyntax(this);
        }

        public override void Accept(JsonValueSyntaxVisitor visitor) => visitor.VisitListSyntax(this);
        public override TResult Accept<TResult>(JsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitListSyntax(this);
        public override TResult Accept<T, TResult>(JsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitListSyntax(this, arg);
    }
}

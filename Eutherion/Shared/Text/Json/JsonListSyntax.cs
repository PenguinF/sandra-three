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
using System.Threading;

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
        public GreenJsonListSyntax Green { get; }

        // Always create the [ and ], avoid overhead of SafeLazyObject.
        public JsonSquareBracketOpenSyntax SquareBracketOpen { get; }

        private readonly JsonMultiValueSyntax[] listItemNodes;
        private readonly JsonCommaSyntax[] commas;

        // Always create the [ and ], avoid overhead of SafeLazyObject.
        public Maybe<JsonSquareBracketCloseSyntax> SquareBracketClose { get; }

        public int ListItemNodeCount => listItemNodes.Length;

        public JsonMultiValueSyntax GetListItemNode(int index)
        {
            if (listItemNodes[index] == null)
            {
                // Replace with an initialized value as an atomic operation.
                // Note that if multiple threads race to this statement, they'll all construct a new syntax,
                // but then only one of these syntaxes will 'win' and be returned.
                Interlocked.CompareExchange(ref listItemNodes[index], new JsonMultiValueSyntax(this, index, Green.ListItemNodes[index]), null);
            }

            return listItemNodes[index];
        }

        public int CommaCount => commas.Length;

        public JsonCommaSyntax GetComma(int index)
        {
            if (commas[index] == null)
            {
                // Replace with an initialized value as an atomic operation.
                // Note that if multiple threads race to this statement, they'll all construct a new syntax,
                // but then only one of these syntaxes will 'win' and be returned.
                Interlocked.CompareExchange(ref commas[index], new JsonCommaSyntax(this, index), null);
            }

            return commas[index];
        }

        public override int Length => Green.Length;

        public override int ChildCount => ListItemNodeCount + CommaCount + (Green.MissingSquareBracketClose ? 1 : 2);

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
            listItemNodes = listItemNodeCount > 0 ? new JsonMultiValueSyntax[listItemNodeCount] : Array.Empty<JsonMultiValueSyntax>();
            commas = listItemNodeCount > 1 ? new JsonCommaSyntax[listItemNodeCount - 1] : Array.Empty<JsonCommaSyntax>();

            SquareBracketClose = green.MissingSquareBracketClose
                               ? Maybe<JsonSquareBracketCloseSyntax>.Nothing
                               : new JsonSquareBracketCloseSyntax(this);
        }

        public override void Accept(JsonValueSyntaxVisitor visitor) => visitor.VisitListSyntax(this);
        public override TResult Accept<TResult>(JsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitListSyntax(this);
        public override TResult Accept<T, TResult>(JsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitListSyntax(this, arg);
    }
}

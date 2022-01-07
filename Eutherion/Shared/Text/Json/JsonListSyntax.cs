#region License
/*********************************************************************************
 * JsonListSyntax.cs
 *
 * Copyright (c) 2004-2022 Henk Nicolai
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
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a syntax node which contains a list.
    /// </summary>
    public sealed class GreenJsonListSyntax : GreenJsonValueSyntax
    {
        /// <summary>
        /// Gets the non-empty list of list item nodes.
        /// </summary>
        public ReadOnlySeparatedSpanList<GreenJsonMultiValueSyntax, GreenJsonCommaSyntax> ListItemNodes { get; }

        /// <summary>
        /// Gets if the list is not terminated by a closing square bracket.
        /// </summary>
        public bool MissingSquareBracketClose { get; }

        /// <summary>
        /// Returns ListItemNodes.Count, or one less if the last element is a <see cref="GreenJsonMissingValueSyntax"/>.
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

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="GreenJsonListSyntax"/>.
        /// </summary>
        /// <param name="listItemNodes">
        /// The non-empty enumeration of list item nodes.
        /// </param>
        /// <param name="missingSquareBracketClose">
        /// False if the list is terminated by a closing square bracket, otherwise true.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="listItemNodes"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="listItemNodes"/> is an empty enumeration.
        /// </exception>
        public GreenJsonListSyntax(IEnumerable<GreenJsonMultiValueSyntax> listItemNodes, bool missingSquareBracketClose)
        {
            ListItemNodes = ReadOnlySeparatedSpanList<GreenJsonMultiValueSyntax, GreenJsonCommaSyntax>.Create(listItemNodes, GreenJsonCommaSyntax.Value);

            if (ListItemNodes.Count == 0)
            {
                throw new ArgumentException($"{nameof(listItemNodes)} cannot be empty", nameof(listItemNodes));
            }

            MissingSquareBracketClose = missingSquareBracketClose;

            Length = JsonSpecialCharacter.SpecialCharacterLength
                   + ListItemNodes.Length
                   + (missingSquareBracketClose ? 0 : JsonSpecialCharacter.SpecialCharacterLength);
        }

        /// <summary>
        /// Gets the start position of an element node relative to the start position of this <see cref="GreenJsonListSyntax"/>.
        /// </summary>
        public int GetElementNodeStart(int index) => JsonSpecialCharacter.SpecialCharacterLength + ListItemNodes.GetElementOffset(index);

        internal override TResult Accept<T, TResult>(GreenJsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitListSyntax(this, arg);
    }

    /// <summary>
    /// Represents a syntax node which contains a list.
    /// </summary>
    public sealed class JsonListSyntax : JsonValueSyntax
    {
        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenJsonListSyntax Green { get; }

        /// <summary>
        /// Gets the <see cref="JsonSquareBracketOpenSyntax"/> node at the start of this list syntax node.
        /// </summary>
        // Always create the [ and ], avoid overhead of SafeLazyObject.
        public JsonSquareBracketOpenSyntax SquareBracketOpen { get; }

        /// <summary>
        /// Gets the non-empty collection of list item nodes separated by comma characters.
        /// </summary>
        public SafeLazyObjectCollection<JsonMultiValueSyntax> ListItemNodes { get; }

        /// <summary>
        /// Gets the child comma syntax node collection.
        /// </summary>
        public SafeLazyObjectCollection<JsonCommaSyntax> Commas { get; }

        /// <summary>
        /// Gets the <see cref="JsonSquareBracketCloseSyntax"/> node at the end of this list syntax node, if it exists.
        /// </summary>
        // Always create the [ and ], avoid overhead of SafeLazyObject.
        public Maybe<JsonSquareBracketCloseSyntax> SquareBracketClose { get; }

        /// <summary>
        /// Returns ListItemNodes.Count, or one less if the last element is a <see cref="JsonMissingValueSyntax"/>.
        /// </summary>
        public int FilteredListItemNodeCount { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        /// <summary>
        /// Gets the number of children of this syntax node.
        /// </summary>
        public override int ChildCount => ListItemNodes.Count + Commas.Count + (Green.MissingSquareBracketClose ? 1 : 2);

        /// <summary>
        /// Initializes the child at the given <paramref name="index"/> and returns it.
        /// </summary>
        public override JsonSyntax GetChild(int index)
        {
            if (index == 0) return SquareBracketOpen;

            index--;
            int itemAndCommaCount = ListItemNodes.Count + Commas.Count;

            if (index < itemAndCommaCount)
            {
                if ((index & 1) == 0) return ListItemNodes[index >> 1];
                return Commas[index >> 1];
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
            int itemAndCommaCount = ListItemNodes.Count + Commas.Count;

            if (index < itemAndCommaCount)
            {
                return Green.ListItemNodes.GetElementOrSeparatorOffset(index) + JsonSpecialCharacter.SpecialCharacterLength;
            }

            if (index == itemAndCommaCount && !Green.MissingSquareBracketClose)
            {
                return Length - JsonSpecialCharacter.SpecialCharacterLength;
            }

            throw new IndexOutOfRangeException();
        }

        internal JsonListSyntax(JsonValueWithBackgroundSyntax parent, GreenJsonListSyntax green) : base(parent)
        {
            Green = green;

            SquareBracketOpen = new JsonSquareBracketOpenSyntax(this);

            int listItemNodeCount = green.ListItemNodes.Count;
            ListItemNodes = new SafeLazyObjectCollection<JsonMultiValueSyntax>(
                listItemNodeCount,
                index => new JsonMultiValueSyntax(this, index));

            Commas = new SafeLazyObjectCollection<JsonCommaSyntax>(
                listItemNodeCount - 1,
                index => new JsonCommaSyntax(this, index));

            SquareBracketClose = green.MissingSquareBracketClose
                               ? Maybe<JsonSquareBracketCloseSyntax>.Nothing
                               : new JsonSquareBracketCloseSyntax(this);

            FilteredListItemNodeCount = Green.FilteredListItemNodeCount;
        }

        internal override TResult Accept<T, TResult>(JsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitListSyntax(this, arg);
    }
}

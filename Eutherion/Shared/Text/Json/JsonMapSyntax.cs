#region License
/*********************************************************************************
 * JsonMapSyntax.cs
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
    /// Represents a syntax node which contains a map.
    /// </summary>
    public sealed class GreenJsonMapSyntax : GreenJsonValueSyntax
    {
        /// <summary>
        /// Gets the non-empty list of key-value nodes.
        /// </summary>
        public ReadOnlySeparatedSpanList<GreenJsonKeyValueSyntax, GreenJsonCommaSyntax> KeyValueNodes { get; }

        /// <summary>
        /// Gets if the map is not terminated by a closing curly brace.
        /// </summary>
        public bool MissingCurlyClose { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="GreenJsonMapSyntax"/>.
        /// </summary>
        /// <param name="keyValueNodes">
        /// The non-empty enumeration of key-value nodes.
        /// </param>
        /// <param name="missingCurlyClose">
        /// False if the list is terminated by a closing curly brace, otherwise true.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="keyValueNodes"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="keyValueNodes"/> is an empty enumeration.
        /// </exception>
        public GreenJsonMapSyntax(IEnumerable<GreenJsonKeyValueSyntax> keyValueNodes, bool missingCurlyClose)
        {
            KeyValueNodes = ReadOnlySeparatedSpanList<GreenJsonKeyValueSyntax, GreenJsonCommaSyntax>.Create(keyValueNodes, GreenJsonCommaSyntax.Value);

            if (KeyValueNodes.Count == 0)
            {
                throw new ArgumentException($"{nameof(keyValueNodes)} cannot be empty", nameof(keyValueNodes));
            }

            MissingCurlyClose = missingCurlyClose;

            Length = JsonSpecialCharacter.SpecialCharacterLength
                   + KeyValueNodes.Length
                   + (missingCurlyClose ? 0 : JsonSpecialCharacter.SpecialCharacterLength);
        }

        /// <summary>
        /// Enumerates all semantically valid key-value pairs together with their starting positions relative to the start position of this <see cref="GreenJsonMapSyntax"/>.
        /// </summary>
        public IEnumerable<(int, GreenJsonStringLiteralSyntax, int, GreenJsonValueSyntax)> ValidKeyValuePairs
        {
            get
            {
                for (int i = 0; i < KeyValueNodes.Count; i++)
                {
                    var keyValueNode = KeyValueNodes[i];

                    if (keyValueNode.ValidKey.IsJust(out GreenJsonStringLiteralSyntax stringLiteral)
                        && keyValueNode.FirstValueNode.IsJust(out GreenJsonMultiValueSyntax multiValueNode)
                        && !(multiValueNode.ValueNode.ContentNode is GreenJsonMissingValueSyntax))
                    {
                        // Only the first value can be valid, even if it's undefined.
                        int keyNodeStart = GetKeyValueNodeStart(i) + keyValueNode.KeyNode.ValueNode.BackgroundBefore.Length;
                        int valueNodeStart = GetKeyValueNodeStart(i) + keyValueNode.GetFirstValueNodeStart() + multiValueNode.ValueNode.BackgroundBefore.Length;

                        yield return (keyNodeStart, stringLiteral, valueNodeStart, multiValueNode.ValueNode.ContentNode);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the start position of a key-value node relative to the start position of this <see cref="GreenJsonMapSyntax"/>.
        /// </summary>
        public int GetKeyValueNodeStart(int index) => JsonSpecialCharacter.SpecialCharacterLength + KeyValueNodes.GetElementOffset(index);

        internal override TResult Accept<T, TResult>(GreenJsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitMapSyntax(this, arg);
    }

    /// <summary>
    /// Represents a syntax node which contains a map.
    /// </summary>
    public sealed class JsonMapSyntax : JsonValueSyntax
    {
        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenJsonMapSyntax Green { get; }

        /// <summary>
        /// Gets the <see cref="JsonCurlyOpenSyntax"/> node at the start of this map syntax node.
        /// </summary>
        // Always create the { and }, avoid overhead of SafeLazyObject.
        public JsonCurlyOpenSyntax CurlyOpen { get; }

        /// <summary>
        /// Gets the non-empty collection of key-value nodes separated by comma characters.
        /// </summary>
        public SafeLazyObjectCollection<JsonKeyValueSyntax> KeyValueNodes { get; }

        /// <summary>
        /// Gets the child comma syntax node collection.
        /// </summary>
        public SafeLazyObjectCollection<JsonCommaSyntax> Commas { get; }

        /// <summary>
        /// Gets the <see cref="JsonCurlyCloseSyntax"/> node at the end of this map syntax node, if it exists.
        /// </summary>
        // Always create the { and }, avoid overhead of SafeLazyObject.
        public Maybe<JsonCurlyCloseSyntax> CurlyClose { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        /// <summary>
        /// Gets the number of children of this syntax node.
        /// </summary>
        public override int ChildCount => KeyValueNodes.Count + Commas.Count + (Green.MissingCurlyClose ? 1 : 2);

        /// <summary>
        /// Initializes the child at the given <paramref name="index"/> and returns it.
        /// </summary>
        public override JsonSyntax GetChild(int index)
        {
            if (index == 0) return CurlyOpen;

            index--;
            int keyValueAndCommaCount = KeyValueNodes.Count + Commas.Count;

            if (index < keyValueAndCommaCount)
            {
                if ((index & 1) == 0) return KeyValueNodes[index >> 1];
                return Commas[index >> 1];
            }

            if (index == keyValueAndCommaCount && CurlyClose.IsJust(out JsonCurlyCloseSyntax jsonCurlyClose))
            {
                return jsonCurlyClose;
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
            int keyValueAndCommaCount = KeyValueNodes.Count + Commas.Count;

            if (index < keyValueAndCommaCount)
            {
                return Green.KeyValueNodes.GetElementOrSeparatorOffset(index) + JsonSpecialCharacter.SpecialCharacterLength;
            }

            if (index == keyValueAndCommaCount && !Green.MissingCurlyClose)
            {
                return Length - JsonSpecialCharacter.SpecialCharacterLength;
            }

            throw new IndexOutOfRangeException();
        }

        internal JsonMapSyntax(JsonValueWithBackgroundSyntax parent, GreenJsonMapSyntax green) : base(parent)
        {
            Green = green;

            CurlyOpen = new JsonCurlyOpenSyntax(this);

            int keyValueNodeCount = green.KeyValueNodes.Count;
            KeyValueNodes = new SafeLazyObjectCollection<JsonKeyValueSyntax>(
                keyValueNodeCount,
                index => new JsonKeyValueSyntax(this, index));

            Commas = new SafeLazyObjectCollection<JsonCommaSyntax>(
                keyValueNodeCount - 1,
                index => new JsonCommaSyntax(this, index));

            CurlyClose = green.MissingCurlyClose
                       ? Maybe<JsonCurlyCloseSyntax>.Nothing
                       : new JsonCurlyCloseSyntax(this);
        }

        internal override TResult Accept<T, TResult>(JsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitMapSyntax(this, arg);
    }
}

#region License
/*********************************************************************************
 * JsonMapSyntax.cs
 *
 * Copyright (c) 2004-2021 Henk Nicolai
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
    /// Represents a map syntax node.
    /// </summary>
    public sealed class GreenJsonMapSyntax : GreenJsonValueSyntax
    {
        public ReadOnlySeparatedSpanList<GreenJsonKeyValueSyntax, GreenJsonCommaSyntax> KeyValueNodes { get; }

        public bool MissingCurlyClose { get; }

        public override int Length { get; }

        public GreenJsonMapSyntax(IEnumerable<GreenJsonKeyValueSyntax> keyValueNodes, bool missingCurlyClose)
        {
            KeyValueNodes = ReadOnlySeparatedSpanList<GreenJsonKeyValueSyntax, GreenJsonCommaSyntax>.Create(keyValueNodes, GreenJsonCommaSyntax.Value);

            if (KeyValueNodes.Count == 0)
            {
                throw new ArgumentException($"{nameof(keyValueNodes)} cannot be empty", nameof(keyValueNodes));
            }

            MissingCurlyClose = missingCurlyClose;

            Length = JsonCurlyOpenSyntax.CurlyOpenLength
                   + KeyValueNodes.Length
                   + (missingCurlyClose ? 0 : JsonCurlyCloseSyntax.CurlyCloseLength);
        }

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
        /// Gets the start position of an key-value node relative to the start position of this <see cref="GreenJsonMapSyntax"/>.
        /// </summary>
        public int GetKeyValueNodeStart(int index) => JsonCurlyOpenSyntax.CurlyOpenLength + KeyValueNodes.GetElementOffset(index);

        public override void Accept(GreenJsonValueSyntaxVisitor visitor) => visitor.VisitMapSyntax(this);
        public override TResult Accept<TResult>(GreenJsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitMapSyntax(this);
        public override TResult Accept<T, TResult>(GreenJsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitMapSyntax(this, arg);
    }

    /// <summary>
    /// Represents a json map value syntax node.
    /// </summary>
    public sealed class JsonMapSyntax : JsonValueSyntax
    {
        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenJsonMapSyntax Green { get; }

        /// <summary>
        /// Gets the <see cref="JsonCurlyOpenSyntax"/> node at the start of this map value syntax node.
        /// </summary>
        // Always create the { and }, avoid overhead of SafeLazyObject.
        public JsonCurlyOpenSyntax CurlyOpen { get; }

        /// <summary>
        /// Gets the collection of key-value syntax nodes separated by comma characters.
        /// </summary>
        public SafeLazyObjectCollection<JsonKeyValueSyntax> KeyValueNodes { get; }

        /// <summary>
        /// Gets the child comma syntax node collection.
        /// </summary>
        public SafeLazyObjectCollection<JsonCommaSyntax> Commas { get; }

        /// <summary>
        /// Gets the <see cref="JsonCurlyCloseSyntax"/> node at the end of this map value syntax node, if it exists.
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
                return Green.KeyValueNodes.GetElementOrSeparatorOffset(index) + JsonCurlyOpenSyntax.CurlyOpenLength;
            }

            if (index == keyValueAndCommaCount && !Green.MissingCurlyClose)
            {
                return Length - JsonCurlyCloseSyntax.CurlyCloseLength;
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

        public override void Accept(JsonValueSyntaxVisitor visitor) => visitor.VisitMapSyntax(this);
        public override TResult Accept<TResult>(JsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitMapSyntax(this);
        public override TResult Accept<T, TResult>(JsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitMapSyntax(this, arg);
    }
}

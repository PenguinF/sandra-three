#region License
/*********************************************************************************
 * JsonMapSyntax.cs
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
    /// Represents a map syntax node.
    /// </summary>
    public sealed class JsonMapSyntax : JsonValueSyntax
    {
        public ReadOnlySeparatedSpanList<JsonKeyValueSyntax, JsonComma> KeyValueNodes { get; }

        public bool MissingCurlyClose { get; }

        public override int Length { get; }

        public JsonMapSyntax(IEnumerable<JsonKeyValueSyntax> keyValueNodes, bool missingCurlyClose)
        {
            KeyValueNodes = ReadOnlySeparatedSpanList<JsonKeyValueSyntax, JsonComma>.Create(keyValueNodes, JsonComma.Value);

            if (KeyValueNodes.Count == 0)
            {
                throw new ArgumentException($"{nameof(keyValueNodes)} cannot be empty", nameof(keyValueNodes));
            }

            MissingCurlyClose = missingCurlyClose;

            Length = JsonCurlyOpen.CurlyOpenLength
                   + KeyValueNodes.Length
                   + (missingCurlyClose ? 0 : JsonCurlyClose.CurlyCloseLength);
        }

        public IEnumerable<(int, JsonStringLiteralSyntax, int, JsonValueSyntax)> ValidKeyValuePairs
        {
            get
            {
                for (int i = 0; i < KeyValueNodes.Count; i++)
                {
                    var keyValueNode = KeyValueNodes[i];

                    if (keyValueNode.ValidKey.IsJust(out JsonStringLiteralSyntax stringLiteral)
                        && keyValueNode.FirstValueNode.IsJust(out JsonMultiValueSyntax multiValueNode)
                        && !(multiValueNode.ValueNode.ContentNode is JsonMissingValueSyntax))
                    {
                        // Only the first value can be valid, even if it's undefined.
                        int keyNodeStart = GetKeyValueNodeStart(i) + keyValueNode.KeyNode.ValueNode.BackgroundBefore.Length;
                        int valueNodeStart = GetKeyValueNodeStart(i) + keyValueNode.GetValueNodeStart(0) + multiValueNode.ValueNode.BackgroundBefore.Length;

                        yield return (keyNodeStart, stringLiteral, valueNodeStart, multiValueNode.ValueNode.ContentNode);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the start position of an key-value node relative to the start position of this <see cref="JsonMapSyntax"/>.
        /// </summary>
        public int GetKeyValueNodeStart(int index) => JsonCurlyOpen.CurlyOpenLength + KeyValueNodes.GetElementOffset(index);

        public override void Accept(JsonValueSyntaxVisitor visitor) => visitor.VisitMapSyntax(this);
        public override TResult Accept<TResult>(JsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitMapSyntax(this);
        public override TResult Accept<T, TResult>(JsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitMapSyntax(this, arg);
    }

    public sealed class RedJsonMapSyntax : RedJsonValueSyntax
    {
        public JsonMapSyntax Green { get; }

        // Always create the { and }, avoid overhead of SafeLazyObject.
        public JsonCurlyOpenSyntax CurlyOpen { get; }

        private readonly RedJsonKeyValueSyntax[] keyValueNodes;
        public int KeyValueNodesCount => keyValueNodes.Length;
        public RedJsonKeyValueSyntax GetKeyValueNode(int index)
        {
            if (keyValueNodes[index] == null)
            {
                // Replace with an initialized value as an atomic operation.
                // Note that if multiple threads race to this statement, they'll all construct a new syntax,
                // but then only one of these syntaxes will 'win' and be returned.
                Interlocked.CompareExchange(ref keyValueNodes[index], new RedJsonKeyValueSyntax(this, index, Green.KeyValueNodes[index]), null);
            }

            return keyValueNodes[index];
        }

        private readonly JsonCommaSyntax[] commas;
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

        // Always create the { and }, avoid overhead of SafeLazyObject.
        public Maybe<JsonCurlyCloseSyntax> CurlyClose { get; }

        public override int Length => Green.Length;

        public override int ChildCount => KeyValueNodesCount + CommaCount + (Green.MissingCurlyClose ? 1 : 2);

        public override JsonSyntax GetChild(int index)
        {
            if (index == 0) return CurlyOpen;

            index--;
            int keyValueAndCommaCount = KeyValueNodesCount + CommaCount;

            if (index < keyValueAndCommaCount)
            {
                if ((index & 1) == 0) return GetKeyValueNode(index >> 1);
                return GetComma(index >> 1);
            }

            if (index == keyValueAndCommaCount && CurlyClose.IsJust(out JsonCurlyCloseSyntax jsonCurlyClose))
            {
                return jsonCurlyClose;
            }

            throw new IndexOutOfRangeException();
        }

        public override int GetChildStartPosition(int index)
        {
            if (index == 0) return 0;

            index--;
            int keyValueAndCommaCount = KeyValueNodesCount + CommaCount;

            if (index < keyValueAndCommaCount)
            {
                return Green.KeyValueNodes.GetElementOrSeparatorOffset(index);
            }

            if (index == keyValueAndCommaCount && !Green.MissingCurlyClose)
            {
                return Length - JsonCurlyClose.CurlyCloseLength;
            }

            throw new IndexOutOfRangeException();
        }

        internal RedJsonMapSyntax(RedJsonValueWithBackgroundSyntax parent, JsonMapSyntax green) : base(parent)
        {
            Green = green;

            CurlyOpen = new JsonCurlyOpenSyntax(this);

            int keyValueNodeCount = green.KeyValueNodes.Count;
            keyValueNodes = keyValueNodeCount > 0 ? new RedJsonKeyValueSyntax[keyValueNodeCount] : Array.Empty<RedJsonKeyValueSyntax>();
            commas = keyValueNodeCount > 1 ? new JsonCommaSyntax[keyValueNodeCount - 1] : Array.Empty<JsonCommaSyntax>();

            CurlyClose = green.MissingCurlyClose
                       ? Maybe<JsonCurlyCloseSyntax>.Nothing
                       : new JsonCurlyCloseSyntax(this);
        }

        public override void Accept(RedJsonValueSyntaxVisitor visitor) => visitor.VisitMapSyntax(this);
        public override TResult Accept<TResult>(RedJsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitMapSyntax(this);
        public override TResult Accept<T, TResult>(RedJsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitMapSyntax(this, arg);
    }
}

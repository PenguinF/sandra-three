﻿#region License
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

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a map syntax node.
    /// </summary>
    public sealed class JsonMapSyntax : JsonValueSyntax
    {
        public ReadOnlyList<JsonKeyValueSyntax> KeyValueNodes { get; }

        private readonly int[] KeyValueNodePositions;

        public bool MissingCurlyClose { get; }

        public override int Length { get; }

        public JsonMapSyntax(IEnumerable<JsonKeyValueSyntax> keyValueNodes, bool missingCurlyClose)
        {
            KeyValueNodes = ReadOnlyList<JsonKeyValueSyntax>.Create(keyValueNodes);

            if (KeyValueNodes.Count == 0)
            {
                throw new ArgumentException($"{nameof(keyValueNodes)} cannot be empty", nameof(keyValueNodes));
            }

            MissingCurlyClose = missingCurlyClose;

            KeyValueNodePositions = new int[KeyValueNodes.Count];
            int cumulativeLength = JsonCurlyOpen.CurlyOpenLength;
            KeyValueNodePositions[0] = cumulativeLength;
            cumulativeLength += KeyValueNodes[0].Length;

            for (int i = 1; i < KeyValueNodes.Count; i++)
            {
                cumulativeLength += JsonComma.CommaLength;
                KeyValueNodePositions[i] = cumulativeLength;
                cumulativeLength += KeyValueNodes[i].Length;
            }

            if (!missingCurlyClose)
            {
                cumulativeLength += JsonCurlyClose.CurlyCloseLength;
            }

            Length = cumulativeLength;
        }

        public IEnumerable<(int, JsonStringLiteralSyntax, int, JsonValueSyntax)> ValidKeyValuePairs
        {
            get
            {
                for (int i = 0; i < KeyValueNodes.Count; i++)
                {
                    var keyValueNode = KeyValueNodes[i];

                    if (keyValueNode.ValidKey.IsJust(out JsonStringLiteralSyntax stringLiteral) && keyValueNode.ValueNodes.Count > 0)
                    {
                        JsonMultiValueSyntax multiValueNode = keyValueNode.ValueNodes[0];

                        // Only the first value can be valid, even if it's undefined.
                        if (!(multiValueNode.ValueNode.ContentNode is JsonMissingValueSyntax))
                        {
                            int keyNodeStart = KeyValueNodePositions[i] + keyValueNode.KeyNode.ValueNode.BackgroundBefore.Length;
                            int valueNodeStart = KeyValueNodePositions[i] + keyValueNode.GetValueNodeStart(0) + multiValueNode.ValueNode.BackgroundBefore.Length;

                            yield return (keyNodeStart, stringLiteral, valueNodeStart, multiValueNode.ValueNode.ContentNode);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the start position of an key-value node relative to the start position of this <see cref="JsonMapSyntax"/>.
        /// </summary>
        public int GetKeyValueNodeStart(int index) => KeyValueNodePositions[index];

        public override void Accept(JsonValueSyntaxVisitor visitor) => visitor.VisitMapSyntax(this);
        public override TResult Accept<TResult>(JsonValueSyntaxVisitor<TResult> visitor) => visitor.VisitMapSyntax(this);
        public override TResult Accept<T, TResult>(JsonValueSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitMapSyntax(this, arg);
    }
}

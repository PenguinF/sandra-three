#region License
/*********************************************************************************
 * JsonKeyValueSyntax.cs
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
using System.Diagnostics;
using System.Threading;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a single key-value pair in a <see cref="GreenJsonMapSyntax"/>.
    /// </summary>
    public sealed class GreenJsonKeyValueSyntax : ISpan
    {
        /// <summary>
        /// Gets the syntax node containing the key of this <see cref="GreenJsonKeyValueSyntax"/>.
        /// </summary>
        public GreenJsonMultiValueSyntax KeyNode => ValueSectionNodes[0];

        /// <summary>
        /// If <see cref="KeyNode"/> contains a valid key, returns it.
        /// </summary>
        public Maybe<GreenJsonStringLiteralSyntax> ValidKey { get; }

        /// <summary>
        /// Returns the first value node containing the value of this <see cref="GreenJsonKeyValueSyntax"/>, if it was provided.
        /// </summary>
        public Maybe<GreenJsonMultiValueSyntax> FirstValueNode => ValueSectionNodes.Count > 1 ? ValueSectionNodes[1] : Maybe<GreenJsonMultiValueSyntax>.Nothing;

        /// <summary>
        /// Gets the list of value section nodes separated by colon characters.
        /// </summary>
        public ReadOnlySeparatedSpanList<GreenJsonMultiValueSyntax, JsonColon> ValueSectionNodes { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax.
        /// </summary>
        public int Length => ValueSectionNodes.Length;

        /// <summary>
        /// Initializes a new instance of a <see cref="GreenJsonKeyValueSyntax"/>.
        /// </summary>
        /// <param name="validKey">
        /// Nothing if no valid key was found, just the valid key otherwise.
        /// </param>
        /// <param name="valueSectionNodes">
        /// The list of syntax nodes containing the key and values.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="validKey"/> and/or <paramref name="valueSectionNodes"/> are null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="validKey"/> is not the expected syntax node -or- <paramref name="valueSectionNodes"/> is an enumeration containing one or less elements.
        /// </exception>
        public GreenJsonKeyValueSyntax(Maybe<GreenJsonStringLiteralSyntax> validKey, IEnumerable<GreenJsonMultiValueSyntax> valueSectionNodes)
        {
            ValidKey = validKey ?? throw new ArgumentNullException(nameof(validKey));
            ValueSectionNodes = ReadOnlySeparatedSpanList<GreenJsonMultiValueSyntax, JsonColon>.Create(valueSectionNodes, JsonColon.Value);

            if (ValueSectionNodes.Count == 0)
            {
                throw new ArgumentException($"{nameof(valueSectionNodes)} cannot be empty", nameof(valueSectionNodes));
            }

            // If a valid key node is given, the node must always be equal to keyNode.ValueNode.Node.
            if (validKey.IsJust(out GreenJsonStringLiteralSyntax validKeyNode)
                && validKeyNode != ValueSectionNodes[0].ValueNode.ContentNode)
            {
                throw new ArgumentException(nameof(validKey));
            }
        }

        /// <summary>
        /// Gets the start position of a value node relative to the start position of this <see cref="GreenJsonKeyValueSyntax"/>.
        /// </summary>
        public int GetValueNodeStart(int index) => ValueSectionNodes.GetElementOffset(index + 1);
    }

    public sealed class JsonKeyValueSyntax : JsonSyntax
    {
        public JsonMapSyntax Parent { get; }
        public int ParentKeyValueNodeIndex { get; }

        public GreenJsonKeyValueSyntax Green { get; }

        private readonly JsonMultiValueSyntax[] valueSectionNodes;
        private readonly JsonColonSyntax[] colons;

        public int ValueSectionNodeCount => valueSectionNodes.Length;

        public JsonMultiValueSyntax GetValueSectionNode(int index)
        {
            if (valueSectionNodes[index] == null)
            {
                // Replace with an initialized value as an atomic operation.
                // Note that if multiple threads race to this statement, they'll all construct a new syntax,
                // but then only one of these syntaxes will 'win' and be returned.
                Interlocked.CompareExchange(ref valueSectionNodes[index], new JsonMultiValueSyntax(this, index, Green.ValueSectionNodes[index]), null);
            }

            return valueSectionNodes[index];
        }

        public int ColonCount => colons.Length;

        public JsonColonSyntax GetColon(int index)
        {
            if (colons[index] == null)
            {
                // Replace with an initialized value as an atomic operation.
                // Note that if multiple threads race to this statement, they'll all construct a new syntax,
                // but then only one of these syntaxes will 'win' and be returned.
                Interlocked.CompareExchange(ref colons[index], new JsonColonSyntax(this, index), null);
            }

            return colons[index];
        }

        public override int Start => JsonCurlyOpen.CurlyOpenLength + Parent.Green.KeyValueNodes.GetElementOffset(ParentKeyValueNodeIndex);
        public override int Length => Green.Length;
        public override JsonSyntax ParentSyntax => Parent;

        public override int ChildCount => ValueSectionNodeCount + ColonCount;

        public override JsonSyntax GetChild(int index)
        {
            // '>>' has the happy property that (-1) >> 1 evaluates to -1, which correctly throws an IndexOutOfRangeException.
            if ((index & 1) == 0) return GetValueSectionNode(index >> 1);
            return GetColon(index >> 1);
        }

        public override int GetChildStartPosition(int index) => Green.ValueSectionNodes.GetElementOrSeparatorOffset(index);

        internal JsonKeyValueSyntax(JsonMapSyntax parent, int parentKeyValueNodeIndex, GreenJsonKeyValueSyntax green)
        {
            Parent = parent;
            ParentKeyValueNodeIndex = parentKeyValueNodeIndex;
            Green = green;

            // Assert that ChildCount will always return 1 or higher.
            int valueSectionNodeCount = green.ValueSectionNodes.Count;
            Debug.Assert(valueSectionNodeCount > 0);
            valueSectionNodes = new JsonMultiValueSyntax[valueSectionNodeCount];
            colons = valueSectionNodeCount > 1 ? new JsonColonSyntax[valueSectionNodeCount - 1] : Array.Empty<JsonColonSyntax>();
        }
    }
}

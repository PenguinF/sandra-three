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
    /// Represents a single key-value pair in a <see cref="JsonMapSyntax"/>.
    /// </summary>
    public sealed class JsonKeyValueSyntax : ISpan
    {
        /// <summary>
        /// Gets the syntax node containing the key of this <see cref="JsonKeyValueSyntax"/>.
        /// </summary>
        public JsonMultiValueSyntax KeyNode => ValueSectionNodes[0];

        /// <summary>
        /// If <see cref="KeyNode"/> contains a valid key, returns it.
        /// </summary>
        public Maybe<JsonStringLiteralSyntax> ValidKey { get; }

        /// <summary>
        /// Returns the first value node containing the value of this <see cref="JsonKeyValueSyntax"/>, if it was provided.
        /// </summary>
        public Maybe<JsonMultiValueSyntax> FirstValueNode => ValueSectionNodes.Count > 1 ? ValueSectionNodes[1] : Maybe<JsonMultiValueSyntax>.Nothing;

        /// <summary>
        /// Gets the list of value section nodes separated by colon characters.
        /// </summary>
        public ReadOnlySeparatedSpanList<JsonMultiValueSyntax, JsonColon> ValueSectionNodes { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax.
        /// </summary>
        public int Length => ValueSectionNodes.Length;

        /// <summary>
        /// Initializes a new instance of a <see cref="JsonKeyValueSyntax"/>.
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
        public JsonKeyValueSyntax(Maybe<JsonStringLiteralSyntax> validKey, IEnumerable<JsonMultiValueSyntax> valueSectionNodes)
        {
            ValidKey = validKey ?? throw new ArgumentNullException(nameof(validKey));
            ValueSectionNodes = ReadOnlySeparatedSpanList<JsonMultiValueSyntax, JsonColon>.Create(valueSectionNodes, JsonColon.Value);

            if (ValueSectionNodes.Count == 0)
            {
                throw new ArgumentException($"{nameof(valueSectionNodes)} cannot be empty", nameof(valueSectionNodes));
            }

            // If a valid key node is given, the node must always be equal to keyNode.ValueNode.Node.
            if (validKey.IsJust(out JsonStringLiteralSyntax validKeyNode)
                && validKeyNode != ValueSectionNodes[0].ValueNode.ContentNode) throw new ArgumentException(nameof(validKey));
        }

        /// <summary>
        /// Gets the start position of a value node relative to the start position of this <see cref="JsonKeyValueSyntax"/>.
        /// </summary>
        public int GetValueNodeStart(int index) => ValueSectionNodes.GetElementOffset(index + 1);
    }

    public sealed class RedJsonKeyValueSyntax : JsonSyntax
    {
        public RedJsonMapSyntax Parent { get; }
        public int ParentKeyValueNodeIndex { get; }

        public JsonKeyValueSyntax Green { get; }

        private readonly RedJsonMultiValueSyntax[] valueSectionNodes;
        public int ValueSectionNodeCount => valueSectionNodes.Length;
        public RedJsonMultiValueSyntax GetValueSectionNode(int index)
        {
            if (valueSectionNodes[index] == null)
            {
                // Replace with an initialized value as an atomic operation.
                // Note that if multiple threads race to this statement, they'll all construct a new syntax,
                // but then only one of these syntaxes will 'win' and be returned.
                Interlocked.CompareExchange(ref valueSectionNodes[index], new RedJsonMultiValueSyntax(this, index, Green.ValueSectionNodes[index]), null);
            }

            return valueSectionNodes[index];
        }

        private readonly RedJsonColon[] colons;
        public int ColonCount => colons.Length;
        public RedJsonColon GetColon(int index)
        {
            if (colons[index] == null)
            {
                // Replace with an initialized value as an atomic operation.
                // Note that if multiple threads race to this statement, they'll all construct a new syntax,
                // but then only one of these syntaxes will 'win' and be returned.
                Interlocked.CompareExchange(ref colons[index], new RedJsonColon(this, index), null);
            }

            return colons[index];
        }

        public override int Length => Green.Length;
        public override JsonSyntax ParentSyntax => Parent;

        public override int ChildCount => ValueSectionNodeCount + ColonCount;

        internal RedJsonKeyValueSyntax(RedJsonMapSyntax parent, int parentKeyValueNodeIndex, JsonKeyValueSyntax green)
        {
            Parent = parent;
            ParentKeyValueNodeIndex = parentKeyValueNodeIndex;
            Green = green;

            // Assert that ChildCount will always return 1 or higher.
            int valueSectionNodeCount = green.ValueSectionNodes.Count;
            Debug.Assert(valueSectionNodeCount > 0);
            valueSectionNodes = new RedJsonMultiValueSyntax[valueSectionNodeCount];
            colons = valueSectionNodeCount > 1 ? new RedJsonColon[valueSectionNodeCount - 1] : Array.Empty<RedJsonColon>();
        }
    }
}

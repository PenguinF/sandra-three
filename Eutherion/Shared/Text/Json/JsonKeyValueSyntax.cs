﻿#region License
/*********************************************************************************
 * JsonKeyValueSyntax.cs
 *
 * Copyright (c) 2004-2020 Henk Nicolai
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
        public ReadOnlySeparatedSpanList<GreenJsonMultiValueSyntax, GreenJsonColonSyntax> ValueSectionNodes { get; }

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
            ValueSectionNodes = ReadOnlySeparatedSpanList<GreenJsonMultiValueSyntax, GreenJsonColonSyntax>.Create(valueSectionNodes, GreenJsonColonSyntax.Value);

            if (ValueSectionNodes.Count == 0)
            {
                throw new ArgumentException($"{nameof(valueSectionNodes)} cannot be empty", nameof(valueSectionNodes));
            }

            // If a valid key node is given, the node must always be equal to keyNode.ValueNode.Node.
            if (validKey.IsJust(out GreenJsonStringLiteralSyntax validKeyNode)
                && validKeyNode != ValueSectionNodes[0].ValueNode.ContentNode)
            {
                throw new ArgumentException("validKey and ValueSectionNodes[0].ValueNode.ContentNode shoukd be the same", nameof(validKey));
            }
        }

        /// <summary>
        /// Gets the start position of <see cref="FirstValueNode"/> relative to the start position of this <see cref="GreenJsonKeyValueSyntax"/>.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// There is no first value node, i.e. this <see cref="GreenJsonKeyValueSyntax"/> has a key only.
        /// </exception>
        public int GetFirstValueNodeStart() => ValueSectionNodes.GetElementOffset(1);
    }

    /// <summary>
    /// Represents a single key-value pair in a <see cref="JsonMapSyntax"/>.
    /// </summary>
    public sealed class JsonKeyValueSyntax : JsonSyntax
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public JsonMapSyntax Parent { get; }

        /// <summary>
        /// Gets the index of this key-value pair in the key-value pair collection of its parent.
        /// </summary>
        public int ParentKeyValueNodeIndex { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenJsonKeyValueSyntax Green { get; }

        /// <summary>
        /// Gets the value section node collection separated by colon characters.
        /// </summary>
        public SafeLazyObjectCollection<JsonMultiValueSyntax> ValueSectionNodes { get; }

        /// <summary>
        /// Gets the child colon syntax node collection.
        /// </summary>
        public SafeLazyObjectCollection<JsonColonSyntax> Colons { get; }

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => JsonCurlyOpenSyntax.CurlyOpenLength + Parent.Green.KeyValueNodes.GetElementOffset(ParentKeyValueNodeIndex);

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override JsonSyntax ParentSyntax => Parent;

        /// <summary>
        /// Gets the number of children of this syntax node.
        /// </summary>
        public override int ChildCount => ValueSectionNodes.Count + Colons.Count;

        /// <summary>
        /// Initializes the child at the given <paramref name="index"/> and returns it.
        /// </summary>
        public override JsonSyntax GetChild(int index)
        {
            // '>>' has the happy property that (-1) >> 1 evaluates to -1, which correctly throws an IndexOutOfRangeException.
            if ((index & 1) == 0) return ValueSectionNodes[index >> 1];
            return Colons[index >> 1];
        }

        /// <summary>
        /// Gets the start position of the child at the given <paramref name="index"/>, without initializing it.
        /// </summary>
        public override int GetChildStartPosition(int index) => Green.ValueSectionNodes.GetElementOrSeparatorOffset(index);

        internal JsonKeyValueSyntax(JsonMapSyntax parent, int parentKeyValueNodeIndex)
        {
            Parent = parent;
            ParentKeyValueNodeIndex = parentKeyValueNodeIndex;
            Green = parent.Green.KeyValueNodes[parentKeyValueNodeIndex];

            // Assert that ChildCount will always return 1 or higher.
            int valueSectionNodeCount = Green.ValueSectionNodes.Count;
            Debug.Assert(valueSectionNodeCount > 0);

            ValueSectionNodes = new SafeLazyObjectCollection<JsonMultiValueSyntax>(
                valueSectionNodeCount,
                index => new JsonMultiValueSyntax(this, index));

            Colons = new SafeLazyObjectCollection<JsonColonSyntax>(
                valueSectionNodeCount - 1,
                index => new JsonColonSyntax(this, index));
        }
    }
}

#region License
/*********************************************************************************
 * JsonMultiValueSyntax.cs
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

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Contains one or more value nodes together with all background syntax that precedes and follows it.
    /// </summary>
    public sealed class GreenJsonMultiValueSyntax : ISpan
    {
        // This syntax is generated everywhere a single value is expected.
        // It is a parse error if zero values, or two or more values are given, the exception being
        // an optional value between a comma following elements of an array/object and its closing
        // ']' or '}' character. (E.g. [0,1,2,] is not an error.)
        //
        // Below are a couple of examples to show in what states a GreenJsonMultiValueSyntax node can be.
        // The given example json represents an array with one enclosing GreenJsonMultiValueSyntax.
        // For clarity, the surrounding brackets are given, though they are not part of this syntax node.
        //
        // []            - ValueNode.BackgroundBefore.BackgroundSymbols.Count == 0
        //                 ValueNode.ContentNode is GreenJsonMissingValueSyntax
        //                 ValueNodes.Count == 1
        //                 BackgroundAfter.BackgroundSymbols.Count == 0
        //
        // [/**/]        - ValueNode.BackgroundBefore.BackgroundSymbols.Count == 1  (one JsonComment)
        //                 ValueNode.ContentNode is GreenJsonMissingValueSyntax
        //                 ValueNodes.Count == 1
        //                 BackgroundAfter.BackgroundSymbols.Count == 0
        //
        // [/**/0]       - ValueNode.BackgroundBefore.BackgroundSymbols.Count == 1
        //                 ValueNode.ContentNode is GreenJsonIntegerLiteralSyntax
        //                 ValueNodes.Count == 1
        //                 BackgroundAfter.BackgroundSymbols.Count == 0
        //
        // [/**/0/**/]   - ValueNode.BackgroundBefore.BackgroundSymbols.Count == 1
        //                 ValueNode.ContentNode is GreenJsonIntegerLiteralSyntax
        //                 ValueNodes.Count == 1
        //                 BackgroundAfter.BackgroundSymbols.Count == 1
        //
        // [0 ]          - ValueNode.BackgroundBefore.BackgroundSymbols.Count == 0
        //                 ValueNode.ContentNode is GreenJsonIntegerLiteralSyntax
        //                 ValueNodes.Count == 1
        //                 BackgroundAfter.BackgroundSymbols.Count == 1             (one JsonWhitespace)
        //
        // [ 0 false ]   - ValueNode.BackgroundBefore.BackgroundSymbols.Count == 1
        //                 ValueNode.ContentNode is GreenJsonIntegerLiteralSyntax
        //                 ValueNodes.Count == 2
        //                 ValueNodes[1].BackgroundSymbols.Count == 1
        //                 ValueNodes[1].ContentNode is GreenJsonBooleanLiteralSyntax
        //                 BackgroundAfter.BackgroundSymbols.Count == 1
        //
        // Only the first ValueNode (ValueNodes[0]) can be GreenJsonMissingValueSyntax, and if it is, ValueNodes.Count is always 1,
        // ValueNode.BackgroundBefore may still be non-empty, and BackgroundAfter is always empty.
        //
        // The main reason this structure exists like this is because there needs to be a place where
        // background symbols between two control symbols can be stored.

        /// <summary>
        /// Gets the syntax node containing the first value.
        /// Is <see cref="GreenJsonMissingValueSyntax"/> if a value was expected but none given. (E.g. in "[0,,2]", middle element.)
        /// </summary>
        public GreenJsonValueWithBackgroundSyntax ValueNode => ValueNodes[0];

        /// <summary>
        /// Gets the non-empty list of value nodes.
        /// </summary>
        public ReadOnlySpanList<GreenJsonValueWithBackgroundSyntax> ValueNodes { get; }

        /// <summary>
        /// Gets the background after the value nodes.
        /// </summary>
        public GreenJsonBackgroundSyntax BackgroundAfter { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length => ValueNodes.Length + BackgroundAfter.Length;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenJsonMultiValueSyntax"/>.
        /// </summary>
        /// <param name="valueNodes">
        /// The non-empty list of value nodes.
        /// </param>
        /// <param name="backgroundAfter">
        /// The background after the value nodes.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="valueNodes"/> and/or <paramref name="backgroundAfter"/> are null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="valueNodes"/> is an empty enumeration.
        /// </exception>
        public GreenJsonMultiValueSyntax(IEnumerable<GreenJsonValueWithBackgroundSyntax> valueNodes, GreenJsonBackgroundSyntax backgroundAfter)
        {
            ValueNodes = ReadOnlySpanList<GreenJsonValueWithBackgroundSyntax>.Create(valueNodes);

            if (ValueNodes.Count == 0)
            {
                throw new ArgumentException($"{nameof(valueNodes)} cannot be empty", nameof(valueNodes));
            }

            BackgroundAfter = backgroundAfter ?? throw new ArgumentNullException(nameof(backgroundAfter));
        }
    }

    /// <summary>
    /// Represents a json syntax node which contains one or more value nodes together with all background syntax that precedes and follows it.
    /// </summary>
    public sealed class JsonMultiValueSyntax : JsonSyntax
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public Union<_void, JsonListSyntax, JsonKeyValueSyntax> Parent { get; }

        /// <summary>
        /// Gets the index of this syntax node in its parent's collection, or 0 if this syntax node is the root node.
        /// </summary>
        public int ParentIndex { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenJsonMultiValueSyntax Green { get; }

        /// <summary>
        /// Gets the collection of value nodes.
        /// </summary>
        public SafeLazyObjectCollection<JsonValueWithBackgroundSyntax> ValueNodes { get; }

        private readonly SafeLazyObject<JsonBackgroundSyntax> backgroundAfter;

        /// <summary>
        /// Gets the background after the value nodes.
        /// </summary>
        public JsonBackgroundSyntax BackgroundAfter => backgroundAfter.Object;

        /// <summary>
        /// Gets the syntax node containing the first value.
        /// Is <see cref="JsonMissingValueSyntax"/> if a value was expected but none given. (E.g. in "[0,,2]", middle element.)
        /// </summary>
        public JsonValueWithBackgroundSyntax ValueNode => ValueNodes[0];

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position, or 0 if this syntax node is the root node.
        /// </summary>
        public override int Start => Parent.Match(
            whenOption1: _ => 0,
            whenOption2: listSyntax => JsonSquareBracketOpen.SquareBracketOpenLength + listSyntax.Green.ListItemNodes.GetElementOffset(ParentIndex),
            whenOption3: keyValueSyntax => keyValueSyntax.Green.ValueSectionNodes.GetElementOffset(ParentIndex));

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        /// <summary>
        /// Gets the parent syntax node of this instance. Returns null for the root node.
        /// </summary>
        public override JsonSyntax ParentSyntax => Parent.Match<JsonSyntax>(
            whenOption1: null,
            whenOption2: x => x,
            whenOption3: x => x);

        /// <summary>
        /// Gets the absolute start position of this syntax node.
        /// </summary>
        public override int AbsoluteStart => Parent.IsOption1() ? 0 : base.AbsoluteStart;

        /// <summary>
        /// Gets the number of children of this syntax node.
        /// </summary>
        public override int ChildCount => ValueNodes.Count + 1;  // Extra 1 for BackgroundAfter.

        /// <summary>
        /// Initializes the child at the given <paramref name="index"/> and returns it.
        /// </summary>
        public override JsonSyntax GetChild(int index)
        {
            if (index < ValueNodes.Count) return ValueNodes[index];
            if (index == ValueNodes.Count) return BackgroundAfter;
            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// Gets the start position of the child at the given <paramref name="index"/>, without initializing it.
        /// </summary>
        public override int GetChildStartPosition(int index)
        {
            if (index < ValueNodes.Count) return Green.ValueNodes.GetElementOffset(index);
            if (index == ValueNodes.Count) return Length - Green.BackgroundAfter.Length;
            throw new IndexOutOfRangeException();
        }

        private JsonMultiValueSyntax(Union<_void, JsonListSyntax, JsonKeyValueSyntax> parent, GreenJsonMultiValueSyntax green)
        {
            Parent = parent;
            Green = green;

            ValueNodes = new SafeLazyObjectCollection<JsonValueWithBackgroundSyntax>(
                green.ValueNodes.Count,
                index => new JsonValueWithBackgroundSyntax(this, index));

            backgroundAfter = new SafeLazyObject<JsonBackgroundSyntax>(() => new JsonBackgroundSyntax(this));
        }

        // For root nodes.
        internal JsonMultiValueSyntax(GreenJsonMultiValueSyntax green)
            : this(_void._, green)
        {
            // Do not assign ParentIndex, its value is meaningless in this case.
        }

        internal JsonMultiValueSyntax(JsonListSyntax parent, int parentIndex)
            : this(parent, parent.Green.ListItemNodes[parentIndex])
        {
            ParentIndex = parentIndex;
        }

        internal JsonMultiValueSyntax(JsonKeyValueSyntax parent, int parentIndex)
            : this(parent, parent.Green.ValueSectionNodes[parentIndex])
        {
            ParentIndex = parentIndex;
        }
    }
}

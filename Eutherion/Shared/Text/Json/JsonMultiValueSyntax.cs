#region License
/*********************************************************************************
 * JsonMultiValueSyntax.cs
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
    /// Contains one or more value nodes together with all background syntax that precedes and follows it.
    /// </summary>
    public sealed class JsonMultiValueSyntax : ISpan
    {
        // This syntax is generated everywhere a single value is expected.
        // It is a parse error if zero values, or two or more values are given, the exception being
        // an optional value between a comma following elements of an array/object and its closing
        // ']' or '}' character. (E.g. [0,1,2,] is not an error.)
        //
        // Below are a couple of examples to show in what states a JsonMultiValueSyntax node can be.
        // The given example json represents an array with one enclosing JsonMultiValueSyntax.
        // For clarity, the surrounding brackets are given, though they are not part of this syntax node.
        //
        // []            - ValueNode.BackgroundBefore.BackgroundSymbols.Count == 0
        //                 ValueNode.ContentNode is JsonMissingValueSyntax
        //                 ValueNodes.Count == 1
        //                 BackgroundAfter.BackgroundSymbols.Count == 0
        //
        // [/**/]        - ValueNode.BackgroundBefore.BackgroundSymbols.Count == 1  (one JsonComment)
        //                 ValueNode.ContentNode is JsonMissingValueSyntax
        //                 ValueNodes.Count == 1
        //                 BackgroundAfter.BackgroundSymbols.Count == 0
        //
        // [/**/0]       - ValueNode.BackgroundBefore.BackgroundSymbols.Count == 1
        //                 ValueNode.ContentNode is JsonIntegerLiteralSyntax
        //                 ValueNodes.Count == 1
        //                 BackgroundAfter.BackgroundSymbols.Count == 0
        //
        // [/**/0/**/]   - ValueNode.BackgroundBefore.BackgroundSymbols.Count == 1
        //                 ValueNode.ContentNode is JsonIntegerLiteralSyntax
        //                 ValueNodes.Count == 1
        //                 BackgroundAfter.BackgroundSymbols.Count == 1
        //
        // [0 ]          - ValueNode.BackgroundBefore.BackgroundSymbols.Count == 0
        //                 ValueNode.ContentNode is JsonIntegerLiteralSyntax
        //                 ValueNodes.Count == 1
        //                 BackgroundAfter.BackgroundSymbols.Count == 1             (one JsonWhitespace)
        //
        // [ 0 false ]   - ValueNode.BackgroundBefore.BackgroundSymbols.Count == 1
        //                 ValueNode.ContentNode is JsonIntegerLiteralSyntax
        //                 ValueNodes.Count == 2
        //                 ValueNodes[1].BackgroundSymbols.Count == 1
        //                 ValueNodes[1].ContentNode is JsonBooleanLiteralSyntax
        //                 BackgroundAfter.BackgroundSymbols.Count == 1
        //
        // Only the first ValueNode (ValueNodes[0]) can be JsonMissingValueSyntax, and if it is, ValueNodes.Count is always 1,
        // ValueNode.BackgroundBefore may still be non-empty, and BackgroundAfter is always empty.
        //
        // The main reason this structure exists like this is because there needs to be a place where
        // background symbols between two control symbols can be stored.

        /// <summary>
        /// Gets the syntax node containing the first value.
        /// Is JsonMissingValueSyntax if a value was expected but none given. (E.g. in "[0,,2]", middle element.)
        /// </summary>
        public JsonValueWithBackgroundSyntax ValueNode => ValueNodes[0];

        /// <summary>
        /// Gets the non-empty list of value nodes.
        /// </summary>
        public ReadOnlySpanList<JsonValueWithBackgroundSyntax> ValueNodes { get; }

        /// <summary>
        /// Gets the background after the value nodes.
        /// </summary>
        public JsonBackgroundSyntax BackgroundAfter { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length => ValueNodes.Length + BackgroundAfter.Length;

        /// <summary>
        /// Initializes a new instance of <see cref="JsonMultiValueSyntax"/>.
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
        public JsonMultiValueSyntax(IEnumerable<JsonValueWithBackgroundSyntax> valueNodes, JsonBackgroundSyntax backgroundAfter)
        {
            ValueNodes = ReadOnlySpanList<JsonValueWithBackgroundSyntax>.Create(valueNodes);

            if (ValueNodes.Count == 0)
            {
                throw new ArgumentException($"{nameof(valueNodes)} cannot be empty", nameof(valueNodes));
            }

            BackgroundAfter = backgroundAfter ?? throw new ArgumentNullException(nameof(backgroundAfter));
        }
    }

    public sealed class RedJsonMultiValueSyntax : JsonSyntax
    {
        public RedJsonListSyntax Parent { get; }
        public int ParentIndex { get; }

        public JsonMultiValueSyntax Green { get; }

        private readonly RedJsonValueWithBackgroundSyntax[] valueNodes;
        public int ValueNodeCount => valueNodes.Length;
        public RedJsonValueWithBackgroundSyntax GetValueNode(int index)
        {
            if (valueNodes[index] == null)
            {
                // Replace with an initialized value as an atomic operation.
                // Note that if multiple threads race to this statement, they'll all construct a new syntax,
                // but then only one of these syntaxes will 'win' and be returned.
                Interlocked.CompareExchange(ref valueNodes[index], new RedJsonValueWithBackgroundSyntax(this, index, Green.ValueNodes[index]), null);
            }

            return valueNodes[index];
        }

        private readonly SafeLazyObject<RedJsonBackgroundSyntax> backgroundAfter;
        public RedJsonBackgroundSyntax BackgroundAfter => backgroundAfter.Object;

        public override int Length => Green.Length;
        public override JsonSyntax ParentSyntax => Parent;

        // For root nodes.
        internal RedJsonMultiValueSyntax(JsonMultiValueSyntax green)
        {
            Green = green;
            int valueNodeCount = green.ValueNodes.Count;
            valueNodes = valueNodeCount > 0 ? new RedJsonValueWithBackgroundSyntax[valueNodeCount] : Array.Empty<RedJsonValueWithBackgroundSyntax>();
            backgroundAfter = new SafeLazyObject<RedJsonBackgroundSyntax>(() => new RedJsonBackgroundSyntax(this, Green.BackgroundAfter));
        }

        internal RedJsonMultiValueSyntax(RedJsonListSyntax parent, int parentIndex, JsonMultiValueSyntax green)
            : this(green)
        {
            Parent = parent;
            ParentIndex = parentIndex;
        }
    }
}

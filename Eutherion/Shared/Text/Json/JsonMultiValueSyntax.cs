﻿#region License
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
using System.Linq;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Contains a value node together with all value nodes and background that follow it.
    /// </summary>
    public sealed class JsonMultiValueSyntax
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
        //                 IgnoredNodes.Count == 0
        //                 BackgroundAfter.BackgroundSymbols.Count == 0
        //
        // [/**/]        - ValueNode.BackgroundBefore.BackgroundSymbols.Count == 1  (one JsonComment)
        //                 ValueNode.ContentNode is JsonMissingValueSyntax
        //                 IgnoredNodes.Count == 0
        //                 BackgroundAfter.BackgroundSymbols.Count == 0
        //
        // [/**/0]       - ValueNode.BackgroundBefore.BackgroundSymbols.Count == 1
        //                 ValueNode.ContentNode is JsonIntegerLiteralSyntax
        //                 IgnoredNodes.Count == 0
        //                 BackgroundAfter.BackgroundSymbols.Count == 0
        //
        // [/**/0/**/]   - ValueNode.BackgroundBefore.BackgroundSymbols.Count == 1
        //                 ValueNode.ContentNode is JsonIntegerLiteralSyntax
        //                 IgnoredNodes.Count == 0
        //                 BackgroundAfter.BackgroundSymbols.Count == 1
        //
        // [0 ]          - ValueNode.BackgroundBefore.BackgroundSymbols.Count == 0
        //                 ValueNode.ContentNode is JsonIntegerLiteralSyntax
        //                 IgnoredNodes.Count == 0
        //                 BackgroundAfter.BackgroundSymbols.Count == 1             (one JsonWhitespace)
        //
        // [ 0 false ]   - ValueNode.BackgroundBefore.BackgroundSymbols.Count == 1
        //                 ValueNode.ContentNode is JsonIntegerLiteralSyntax
        //                 IgnoredNodes.Count == 1
        //                 IgnoredNodes[0].BackgroundSymbols.Count == 1
        //                 IgnoredNodes[0].ContentNode is JsonBooleanLiteralSyntax
        //                 BackgroundAfter.BackgroundSymbols.Count == 1
        //
        // Only the first ValueNode can be JsonMissingValueSyntax, and if it is, IgnoredNodes.Count is always 0,
        // ValueNode.BackgroundBefore may still be non-empty, and BackgroundAfter is always empty.
        //
        // The main reason this structure exists like this is because there needs to be a place where
        // background symbols between two control symbols can be stored.

        /// <summary>
        /// Gets the syntax node containing the first value.
        /// Is JsonMissingValueSyntax if a value was expected but none given. (E.g. in "[0,,2]", middle element.)
        /// </summary>
        public JsonValueWithBackgroundSyntax ValueNode { get; }

        /// <summary>
        /// Gets the list of ignored value nodes after the first.
        /// </summary>
        public ReadOnlyList<JsonValueWithBackgroundSyntax> IgnoredNodes { get; }

        /// <summary>
        /// Gets the background after the value nodes.
        /// </summary>
        public JsonBackgroundSyntax BackgroundAfter { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="JsonMultiValueSyntax"/>.
        /// </summary>
        /// <param name="valueNode">
        /// The syntax node containing the first value.
        /// </param>
        /// <param name="ignoredNodes">
        /// The list of ignored value nodes after the first.
        /// </param>
        /// <param name="backgroundAfter">
        /// The background after the value nodes.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="valueNode"/> and/or <paramref name="ignoredNodes"/> and/or <paramref name="backgroundAfter"/> are null.
        /// </exception>
        public JsonMultiValueSyntax(
            JsonValueWithBackgroundSyntax valueNode,
            IEnumerable<JsonValueWithBackgroundSyntax> ignoredNodes,
            JsonBackgroundSyntax backgroundAfter)
        {
            ValueNode = valueNode ?? throw new ArgumentNullException(nameof(valueNode));
            IgnoredNodes = ReadOnlyList<JsonValueWithBackgroundSyntax>.Create(ignoredNodes);
            BackgroundAfter = backgroundAfter ?? throw new ArgumentNullException(nameof(backgroundAfter));
            Length = ValueNode.Length + IgnoredNodes.Sum(x => x.Length) + BackgroundAfter.Length;
        }
    }
}
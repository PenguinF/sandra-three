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

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a single key-value pair in a <see cref="JsonMapSyntax"/>.
    /// </summary>
    public class JsonKeyValueSyntax
    {
        /// <summary>
        /// Gets the syntax node containing the key of this <see cref="JsonKeyValueSyntax"/>.
        /// </summary>
        public JsonMultiValueSyntax KeyNode { get; }

        /// <summary>
        /// If <see cref="KeyNode"/> contains a valid key, returns it.
        /// </summary>
        public Maybe<JsonStringLiteralSyntax> ValidKey { get; }

        /// <summary>
        /// Gets the list of syntax nodes containing the value of this <see cref="JsonKeyValueSyntax"/>.
        /// </summary>
        public ReadOnlyList<JsonMultiValueSyntax> ValueNodes { get; }

        /// <summary>
        /// Initializes a new instance of a <see cref="JsonKeyValueSyntax"/>.
        /// </summary>
        /// <param name="keyNode">
        /// The syntax node containing the key.
        /// </param>
        /// <param name="validKey">
        /// Nothing if no valid key was found, just the valid key otherwise.
        /// </param>
        /// <param name="valueNodes">
        /// The list of syntax nodes containing the value.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="keyNode"/> and/or <paramref name="validKey"/> and/or <paramref name="valueNodes"/> are null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="validKey"/> is not the expected syntax node in <paramref name="keyNode"/>.
        /// </exception>
        public JsonKeyValueSyntax(JsonMultiValueSyntax keyNode, Maybe<JsonStringLiteralSyntax> validKey, IEnumerable<JsonMultiValueSyntax> valueNodes)
        {
            KeyNode = keyNode ?? throw new ArgumentNullException(nameof(keyNode));
            ValidKey = validKey ?? throw new ArgumentNullException(nameof(validKey));

            // If a valid key node is given, the node must always be equal to keyNode.ValueNode.Node.
            if (validKey.IsJust(out JsonStringLiteralSyntax validKeyNode)
                && validKeyNode != keyNode.ValueNode.ContentNode) throw new ArgumentException(nameof(validKey));

            ValueNodes = ReadOnlyList<JsonMultiValueSyntax>.Create(valueNodes);
        }
    }
}

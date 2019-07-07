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
        public JsonStringLiteralSyntax Key { get; }

        /// <summary>
        /// Gets the syntax node containing the value of this <see cref="JsonKeyValueSyntax"/>.
        /// </summary>
        public JsonValueSyntax Value { get; }

        /// <summary>
        /// Initializes a new instance of a <see cref="JsonKeyValueSyntax"/>.
        /// </summary>
        /// <param name="key">
        /// The syntax node containing the key.
        /// </param>
        /// <param name="value">
        /// The syntax node containing the value.
        /// </param>
        public JsonKeyValueSyntax(JsonStringLiteralSyntax key, JsonValueSyntax value)
        {
            Key = key;
            Value = value;
        }
    }
}

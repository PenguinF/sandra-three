#region License
/*********************************************************************************
 * JsonMapSyntax.cs
 * 
 * Copyright (c) 2004-2018 Henk Nicolai
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
 *********************************************************************************/
#endregion

using System.Collections.Generic;

namespace SysExtensions.Text.Json
{
    /// <summary>
    /// Represents a map syntax node.
    /// </summary>
    public sealed class JsonMapSyntax : JsonSyntaxNode
    {
        public IReadOnlyList<JsonMapNodeKeyValuePair> MapNodeKeyValuePairs { get; }

        public JsonMapSyntax(IReadOnlyList<JsonMapNodeKeyValuePair> mapNodeKeyValuePairs)
            => MapNodeKeyValuePairs = mapNodeKeyValuePairs;

        public override void Accept(JsonSyntaxNodeVisitor visitor) => visitor.VisitMapSyntax(this);
        public override TResult Accept<TResult>(JsonSyntaxNodeVisitor<TResult> visitor) => visitor.VisitMapSyntax(this);
        public override TResult Accept<T, TResult>(JsonSyntaxNodeVisitor<T, TResult> visitor, T arg) => visitor.VisitMapSyntax(this, arg);
    }

    /// <summary>
    /// Represents a single key-value pair in a <see cref="JsonMapSyntax"/>.
    /// </summary>
    public struct JsonMapNodeKeyValuePair
    {
        /// <summary>
        /// Gets the syntax node containing the key of this <see cref="JsonMapNodeKeyValuePair"/>.
        /// </summary>
        public JsonStringLiteralSyntax Key { get; }

        /// <summary>
        /// Gets the syntax node containing the value of this <see cref="JsonMapNodeKeyValuePair"/>.
        /// </summary>
        public JsonSyntaxNode Value { get; }

        /// <summary>
        /// Initializes a new instance of a <see cref="JsonMapNodeKeyValuePair"/>.
        /// </summary>
        /// <param name="key">
        /// The syntax node containing the key.
        /// </param>
        /// <param name="value">
        /// The syntax node containing the value.
        /// </param>
        public JsonMapNodeKeyValuePair(JsonStringLiteralSyntax key, JsonSyntaxNode value)
        {
            Key = key;
            Value = value;
        }
    }
}

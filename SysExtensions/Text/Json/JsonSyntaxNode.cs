#region License
/*********************************************************************************
 * JsonSyntaxNode.cs
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

namespace SysExtensions.Text.Json
{
    /// <summary>
    /// Represents a node in an abstract json syntax tree.
    /// </summary>
    public abstract class JsonSyntaxNode
    {
        /// <summary>
        /// Gets the start position of the text span corresponding with this node.
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length { get; }

        protected JsonSyntaxNode(int start, int length)
        {
            Start = start;
            Length = length;
        }

        public abstract void Accept(JsonSyntaxNodeVisitor visitor);
        public abstract TResult Accept<TResult>(JsonSyntaxNodeVisitor<TResult> visitor);
        public abstract TResult Accept<T, TResult>(JsonSyntaxNodeVisitor<T, TResult> visitor, T arg);
    }
}

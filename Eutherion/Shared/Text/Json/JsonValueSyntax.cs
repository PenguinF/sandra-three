#region License
/*********************************************************************************
 * JsonValueSyntax.cs
 *
 * Copyright (c) 2004-2022 Henk Nicolai
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
    /// Represents a node containing a single json value in an abstract json syntax tree.
    /// Use <see cref="GreenJsonValueSyntaxVisitor{T, TResult}"/> overrides to distinguish between implementations of this type.
    /// </summary>
    public abstract class GreenJsonValueSyntax : ISpan
    {
        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public abstract int Length { get; }

        public abstract void Accept(GreenJsonValueSyntaxVisitor visitor);
        public abstract TResult Accept<TResult>(GreenJsonValueSyntaxVisitor<TResult> visitor);
        public abstract TResult Accept<T, TResult>(GreenJsonValueSyntaxVisitor<T, TResult> visitor, T arg);
    }

    /// <summary>
    /// Represents a node containing a single json value in an abstract json syntax tree.
    /// Use <see cref="JsonValueSyntaxVisitor{T, TResult}"/> overrides to distinguish between implementations of this type.
    /// </summary>
    public abstract class JsonValueSyntax : JsonSyntax
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public JsonValueWithBackgroundSyntax Parent { get; }

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public sealed override int Start => Parent.BackgroundBefore.Length;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public sealed override JsonSyntax ParentSyntax => Parent;

        internal JsonValueSyntax(JsonValueWithBackgroundSyntax parent) => Parent = parent;

        public abstract void Accept(JsonValueSyntaxVisitor visitor);
        public abstract TResult Accept<TResult>(JsonValueSyntaxVisitor<TResult> visitor);
        public abstract TResult Accept<T, TResult>(JsonValueSyntaxVisitor<T, TResult> visitor, T arg);
    }
}

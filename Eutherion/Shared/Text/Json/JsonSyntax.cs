#region License
/*********************************************************************************
 * JsonSyntax.cs
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

using System;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a context sensitive node in an abstract json syntax tree.
    /// </summary>
    public abstract class JsonSyntax : ISpan
    {
        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public abstract int Length { get; }

        /// <summary>
        /// Gets the parent syntax node of this instance. Returns null for the root node.
        /// </summary>
        public abstract JsonSyntax ParentSyntax { get; }

        /// <summary>
        /// Returns the number of children of this syntax node.
        /// </summary>
        public virtual int ChildCount => 0;

        /// <summary>
        /// Returns if this syntax is a terminal symbol, i.e. if it has no children.
        /// </summary>
        public bool IsTerminalSymbol => ChildCount == 0;

        /// <summary>
        /// Initializes the child at the given index and returns it.
        /// </summary>
        public virtual JsonSyntax GetChild(int index) => throw new IndexOutOfRangeException();

        public virtual void Accept(JsonTerminalSymbolVisitor visitor) => throw new JsonSyntaxIsNotTerminalException(this);
        public virtual TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => throw new JsonSyntaxIsNotTerminalException(this);
        public virtual TResult Accept<T, TResult>(JsonTerminalSymbolVisitor<T, TResult> visitor, T arg) => throw new JsonSyntaxIsNotTerminalException(this);
    }

    /// <summary>
    /// Occurs when a <see cref="JsonTerminalSymbolVisitor"/> is called on a <see cref="JsonSyntax"/> instance
    /// which is not a terminal symbol, i.e. for which <see cref="JsonSyntax.IsTerminalSymbol"/> returns false.
    /// </summary>
    public class JsonSyntaxIsNotTerminalException : Exception
    {
        internal JsonSyntaxIsNotTerminalException(JsonSyntax syntax)
            : base($"{syntax.GetType().FullName} is not a terminal symbol.")
        {
        }
    }
}

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
using System.Collections.Generic;

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

        /// <summary>
        /// Gets the start position of the child at the given index, without initializing it.
        /// </summary>
        public virtual int GetChildStartPosition(int index) => throw new IndexOutOfRangeException();

        /// <summary>
        /// Enumerates all <see cref="JsonSyntax"/> descendants of this node that fall within the
        /// given range and have no child nodes.
        /// </summary>
        public IEnumerable<JsonSyntax> TerminalSymbolsInRange(int start, int length)
        {
            int end = start + length;

            if (IsTerminalSymbol)
            {
                // Yield return if ranges start..end and 0..Length intersect.
                if (start <= Length && 0 <= end)
                {
                    yield return this;
                }
            }
            else
            {
                int childIndex = 0;
                int childEndPosition = GetChildStartPosition(0);

                // Naive implementation traversing the entire child nodes collection.
                // TODO: find the first child node within the range using binary search.
                while (childIndex < ChildCount)
                {
                    int childStartPosition = childEndPosition;
                    int nextChildIndex = childIndex + 1;
                    childEndPosition = nextChildIndex == ChildCount ? Length : GetChildStartPosition(nextChildIndex);

                    // Yield return if intervals [start..end] and [childStartPosition..childEndPosition] intersect.
                    if (start <= childEndPosition && childStartPosition <= end)
                    {
                        // Translate to relative child position by subtracting childStartPosition.
                        foreach (var descendant in GetChild(childIndex).TerminalSymbolsInRange(start - childStartPosition, length))
                        {
                            yield return descendant;
                        }
                    }

                    childIndex = nextChildIndex;
                }
            }
        }

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

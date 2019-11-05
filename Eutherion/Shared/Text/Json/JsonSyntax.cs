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
        /// Gets the start position of this syntax node relative to its parent's start position, or 0 if this syntax node is the root node.
        /// </summary>
        public abstract int Start { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public abstract int Length { get; }

        /// <summary>
        /// Gets the parent syntax node of this instance. Returns null for the root node.
        /// </summary>
        public abstract JsonSyntax ParentSyntax { get; }

        /// <summary>
        /// Gets the absolute start position of this syntax node.
        /// </summary>
        public virtual int AbsoluteStart => ParentSyntax.AbsoluteStart + Start;

        /// <summary>
        /// Gets the number of children of this syntax node.
        /// </summary>
        public virtual int ChildCount => 0;

        /// <summary>
        /// Gets if this syntax is a terminal symbol, i.e. if it has no children.
        /// </summary>
        public bool IsTerminalSymbol => ChildCount == 0;

        /// <summary>
        /// Initializes the child at the given <paramref name="index"/> and returns it.
        /// </summary>
        public virtual JsonSyntax GetChild(int index) => throw new IndexOutOfRangeException();

        /// <summary>
        /// Gets the start position of the child at the given <paramref name="index"/>, without initializing it.
        /// </summary>
        public virtual int GetChildStartPosition(int index) => throw new IndexOutOfRangeException();

        /// <summary>
        /// Gets the start position of the child at the given <paramref name="index"/>,
        /// which is the end position of the child at <paramref name="index"/> - 1.
        /// If <paramref name="index"/> is equal to <see cref="ChildCount"/>, the end position of the last child is returned.
        /// In neither case will the child node be initialized.
        /// </summary>
        public int GetChildStartOrEndPosition(int index) => index == ChildCount ? Length : GetChildStartPosition(index);

        /// <summary>
        /// Returns the index of the <see cref="JsonSyntax"/> after the given position.
        /// <seealso cref="TextIndex{TTerminal}.GetElementAfter(int)"/>.
        /// </summary>
        private int GetChildIndexAfter(int position)
        {
            int minIndex = 0;
            int maxIndex = ChildCount - 1;

            while (minIndex <= maxIndex)
            {
                int childIndex = (minIndex + maxIndex) / 2;
                int childStartPosition = GetChildStartPosition(childIndex);
                int childEndPosition = GetChildStartOrEndPosition(childIndex + 1);

                if (position < childStartPosition)
                {
                    // Exclude higher part.
                    maxIndex = childIndex - 1;
                }
                else if (childEndPosition <= position)
                {
                    // Exclude lower part.
                    minIndex = childIndex + 1;
                }
                else
                {
                    return childIndex;
                }
            }

            throw new IndexOutOfRangeException(nameof(position));
        }

        private IEnumerable<JsonSyntax> ChildTerminalSymbolsInRange(int start, int length)
        {
            int childIndex = 0;
            int childEndPosition = GetChildStartPosition(0);

            // Naive implementation traversing the entire child nodes collection.
            // TODO: find the first child node within the range using binary search.
            while (childIndex < ChildCount && childEndPosition < start + length)
            {
                int childStartPosition = childEndPosition;
                childEndPosition = GetChildStartOrEndPosition(childIndex + 1);

                if (start < childEndPosition && childStartPosition < childEndPosition)
                {
                    JsonSyntax childNode = GetChild(childIndex);

                    if (childNode.IsTerminalSymbol)
                    {
                        yield return childNode;
                    }
                    else
                    {
                        // Translate to relative child position by subtracting childStartPosition.
                        foreach (var descendant in childNode.ChildTerminalSymbolsInRange(start - childStartPosition, length))
                        {
                            yield return descendant;
                        }
                    }
                }

                childIndex++;
            }
        }

        /// <summary>
        /// Enumerates all <see cref="JsonSyntax"/> descendants of this node that fall within the
        /// given range, have no child nodes, and have a length greater than 0.
        /// </summary>
        /// <param name="start">
        /// Start position of the range to search, relative to this syntax node.
        /// </param>
        /// <param name="length"></param>
        /// Length of the range to search, relative to this syntax node.
        /// <returns>
        /// All descendants of this node that intersect with the given range, have no child nodes, and have a length greater than 0. 
        /// </returns>
        public IEnumerable<JsonSyntax> TerminalSymbolsInRange(int start, int length)
        {
            // Yield return if ranges [start..start+length] and [0..Length] intersect.
            if (0 < length && 0 < Length && start < Length && 0 < start + length)
            {
                if (IsTerminalSymbol)
                {
                    return new[] { this };
                }
                else
                {
                    return ChildTerminalSymbolsInRange(start, length);
                }
            }

            return Array.Empty<JsonSyntax>();
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

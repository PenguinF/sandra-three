#region License
/*********************************************************************************
 * TextIndex.cs
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SysExtensions.TextIndex
{
    /// <summary>
    /// Manages an index of terminal symbols wrapped in <see cref="TextElement{TTerminal}"/> instances.
    /// </summary>
    /// <typeparam name="TTerminal">
    /// The type of terminal symbols to index.
    /// See also: https://en.wikipedia.org/wiki/Terminal_and_nonterminal_symbols
    /// </typeparam>
    public class TextIndex<TTerminal>
    {
        private readonly List<int> elementIndexes = new List<int>();

        private readonly List<TextElement<TTerminal>> elements = new List<TextElement<TTerminal>>();

        /// <summary>
        /// Gets a reference to the list of <see cref="TextElement{TTerminal}"/> instances.
        /// </summary>
        public IReadOnlyList<TextElement<TTerminal>> Elements => elements;

        /// <summary>
        /// Appends a terminal symbol to the end of the index.
        /// </summary>
        /// <param name="terminal">
        /// The terminal symbol to append.
        /// </param>
        /// <param name="length">
        /// The length of the terminal symbol to append.
        /// </param>
        /// <returns>
        /// The created <see cref="TextElement{TTerminal}"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="terminal"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length"/> is 0 or negative.
        /// </exception>
        public TextElement<TTerminal> AppendTerminalSymbol(TTerminal terminal, int length)
        {
            if (terminal == null) throw new ArgumentNullException(nameof(terminal));
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length), "Cannot append empty (lambda) terminals.");

            int start = elementIndexes.Count;
            elementIndexes.AddRange(Enumerable.Repeat(elements.Count, length));

            var textElement = new TextElement<TTerminal>(this)
            {
                TerminalSymbol = terminal,
                Start = start,
                Length = length,
            };

            elements.Add(textElement);
            assertInvariants();
            return textElement;
        }

        /// <summary>
        /// Clears all elements from the index.
        /// </summary>
        public void Clear()
        {
            elementIndexes.Clear();
            elements.ForEach(e => e.Detach());
            elements.Clear();
            assertInvariants();
        }

        /// <summary>
        /// Removes the range of elements from a start index to the end.
        /// </summary>
        /// <param name="start">
        /// The start index of the first element to remove.
        /// </param>
        public void RemoveFrom(int start)
        {
            int textStart = elements[start].Start;
            int textLength = elementIndexes.Count - textStart;

            elementIndexes.RemoveRange(textStart, textLength);
            elements.Skip(start).ForEach(e => e.Detach());
            elements.RemoveRange(start, elements.Count - start);

            assertInvariants();
        }

        /// <summary>
        /// Gets the size of the index.
        /// </summary>
        public int Size => elementIndexes.Count;

        /// <summary>
        /// Returns the text element before the given position. Returns null if the position is at the start of the text.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="position"/> is less than 0 or greater than or equal to <see cref="Size"/>.
        /// </exception>
        public TextElement<TTerminal> GetElementBefore(int position)
            => position == 0 ? null : elements[elementIndexes[position - 1]];

        /// <summary>
        /// Returns the text element after the given position. Returns null if the position is at the end of the text.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="position"/> is less than 0 or greater than or equal to <see cref="Size"/>.
        /// </exception>
        public TextElement<TTerminal> GetElementAfter(int position)
            => position == elementIndexes.Count ? null : elements[elementIndexes[position]];

        [Conditional("DEBUG")]
        private void assertInvariants()
        {
            // Assert invariants about lengths being equal.
            int textLength = elementIndexes.Count;
            if (textLength == 0)
            {
                Debug.Assert(elements.Count == 0);
            }
            else
            {
                var lastElementIndex = elementIndexes[textLength - 1];
                Debug.Assert(lastElementIndex + 1 == elements.Count);
                var lastElement = elements[lastElementIndex];
                Debug.Assert(lastElement.End == textLength);
            }
        }

    }
}

#region License
/*********************************************************************************
 * TextIndex.cs
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

namespace Eutherion.Text
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

            var textElement = new TextElement<TTerminal>(terminal)
            {
                Length = length,
            };

            AppendTerminalSymbol(textElement);
            return textElement;
        }

        /// <summary>
        /// Appends a terminal symbol to the end of the index, and updates its <see cref="TextElement{TTerminal}.Start"/> property.
        /// </summary>
        /// <param name="textElement">
        /// The text element to append.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="textElement"/> is null.
        /// </exception>
        public void AppendTerminalSymbol(TextElement<TTerminal> textElement)
        {
            if (textElement == null) throw new ArgumentNullException(nameof(textElement));
            if (textElement.Length <= 0) throw new ArgumentOutOfRangeException(nameof(textElement), "Cannot append empty (lambda) terminals.");

            textElement.Start = Size;
            Size += textElement.Length;
            elements.Add(textElement);
        }

        /// <summary>
        /// Clears all elements from the index.
        /// </summary>
        public void Clear()
        {
            Size = 0;
            elements.Clear();
        }

        /// <summary>
        /// Removes the range of elements from a start index to the end.
        /// </summary>
        /// <param name="start">
        /// The start index of the first element to remove.
        /// </param>
        public void RemoveFrom(int start)
        {
            Size = elements[start].Start;
            elements.RemoveRange(start, elements.Count - start);
        }

        /// <summary>
        /// Gets the size of the index.
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Returns the text element before the given position. Returns null if the position is at the start of the text.
        /// </summary>
        public TextElement<TTerminal> GetElementBefore(int position)
            => position == 0 ? null : GetElementAfter(position - 1);

        /// <summary>
        /// Returns the text element after the given position. Returns null if the position is at the end of the text.
        /// </summary>
        public TextElement<TTerminal> GetElementAfter(int position)
        {
            int minIndex = 0;
            int maxIndex = elements.Count - 1;

            while (minIndex <= maxIndex)
            {
                int index = (minIndex + maxIndex) / 2;
                TextElement<TTerminal> element = elements[index];

                if (position < element.Start)
                {
                    // Exclude higher part.
                    maxIndex = index - 1;
                }
                else if (element.End <= position)
                {
                    // Exclude lower part.
                    minIndex = index + 1;
                }
                else
                {
                    return element;
                }
            }

            return null;
        }
    }
}

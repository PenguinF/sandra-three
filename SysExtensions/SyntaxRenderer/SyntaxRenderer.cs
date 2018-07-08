#region License
/*********************************************************************************
 * SyntaxRenderer.cs
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

namespace SysExtensions.SyntaxRenderer
{
    /// <summary>
    /// Changes the behavior of a <see cref="ISyntaxRenderTarget"/> so it shows a read-only list of formatted text elements.
    /// </summary>
    /// <typeparam name="TTerminal">
    /// The type of terminal symbols to format.
    /// See also: https://en.wikipedia.org/wiki/Terminal_and_nonterminal_symbols
    /// </typeparam>
    /// <remarks>
    /// This component is work in progress.
    /// </remarks>
    public class SyntaxRenderer<TTerminal>
    {
        /// <summary>
        /// Attaches a syntax renderer to a render target.
        /// </summary>
        /// <param name="isSlave">
        /// True if the renderer should not update the render target but assume it is always in sync; false otherwise.
        /// </param>
        public static SyntaxRenderer<TTerminal> AttachTo(ISyntaxRenderTarget renderTarget, bool isSlave)
            => new SyntaxRenderer<TTerminal>(renderTarget, isSlave);

        private SyntaxRenderer(ISyntaxRenderTarget renderTarget, bool isSlave)
        {
            if (renderTarget == null) throw new ArgumentNullException(nameof(renderTarget));
            RenderTarget = renderTarget;
            this.isSlave = isSlave;

            Elements = elements;

            // Remove all text from the render target.
            renderTarget.RemoveText(0, int.MaxValue);
            renderTarget.CaretPosition.ValueChanged += tryInvokeCaretPositionChanged;

            assertInvariants();
        }

        internal readonly ISyntaxRenderTarget RenderTarget;

        private readonly bool isSlave;

        private readonly List<int> elementIndexes = new List<int>();

        private readonly List<TextElement<TTerminal>> elements = new List<TextElement<TTerminal>>();

        public readonly IReadOnlyList<TextElement<TTerminal>> Elements;

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

        public TextElement<TTerminal> AppendTerminalSymbol(TTerminal terminal, int length)
        {
            if (terminal == null) throw new ArgumentNullException(nameof(terminal));
            if (length == 0) throw new NotImplementedException("Cannot append empty (lambda) terminals yet.");

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
        /// Clears all syntax from the renderer.
        /// </summary>
        public void Clear()
        {
            elementIndexes.Clear();
            elements.ForEach(e => e.Detach());
            elements.Clear();
            assertInvariants();
        }

        public void RemoveFrom(int index)
        {
            int textStart = elements[index].Start;
            int textLength = elementIndexes.Count - textStart;

            elementIndexes.RemoveRange(textStart, textLength);
            elements.Skip(index).ForEach(e => e.Detach());
            elements.RemoveRange(index, elements.Count - index);

            assertInvariants();
        }

        /// <summary>
        /// Gets the length of the generated text.
        /// </summary>
        public int TextLength => elementIndexes.Count;

        /// <summary>
        /// Returns the text element before the given position. Returns null if the position is at the start of the text.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="position"/> is less than 0 or greater than or equal to <see cref="TextLength"/>.
        /// </exception>
        public TextElement<TTerminal> GetElementBefore(int position)
            => position == 0 ? null : elements[elementIndexes[position - 1]];

        /// <summary>
        /// Returns the text element after the given position. Returns null if the position is at the end of the text.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="position"/> is less than 0 or greater than or equal to <see cref="TextLength"/>.
        /// </exception>
        public TextElement<TTerminal> GetElementAfter(int position)
            => position == elementIndexes.Count ? null : elements[elementIndexes[position]];

        /// <summary>
        /// Occurs when the position of the caret is updated by the user, when no text is selected.
        /// </summary>
        public event Action<SyntaxRenderer<TTerminal>, CaretPositionChangedEventArgs<TTerminal>> CaretPositionChanged;

        private void tryInvokeCaretPositionChanged(int selectionStart)
        {
            TextElement<TTerminal> elementBefore = GetElementBefore(selectionStart);
            TextElement<TTerminal> elementAfter = GetElementAfter(selectionStart);
            int relativeCaretIndex = elementAfter == null ? 0 : selectionStart - elementAfter.Start;

            var eventArgs = new CaretPositionChangedEventArgs<TTerminal>(elementBefore,
                                                                         elementAfter,
                                                                         relativeCaretIndex);
            CaretPositionChanged?.Invoke(this, eventArgs);
        }
    }

    /// <summary>
    /// Provides data for the <see cref="SyntaxRenderer{TTerminal}.CaretPositionChanged"/> event.
    /// </summary>
    public class CaretPositionChangedEventArgs<TTerminal> : EventArgs
    {
        /// <summary>
        /// Gets the text element immediately before the caret, or null if the caret is at the start of the text.
        /// If the caret is inside a text element, <see cref="ElementBefore"/> returns the same element as <see cref="ElementAfter"/>.
        /// </summary>
        public TextElement<TTerminal> ElementBefore { get; }

        /// <summary>
        /// Gets the text element immediately after the caret, or null if the caret is at the end of the text.
        /// If the caret is inside a text element, <see cref="ElementAfter"/> returns the same element as <see cref="ElementBefore"/>.
        /// </summary>
        public TextElement<TTerminal> ElementAfter { get; }

        /// <summary>
        /// Returns the relative position of the caret in <see cref="ElementAfter"/>, or 0 if <see cref="ElementAfter"/> is null.
        /// </summary>
        public int RelativeCaretIndex { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CaretPositionChangedEventArgs"/> class.
        /// </summary>
        internal CaretPositionChangedEventArgs(TextElement<TTerminal> elementBefore,
                                               TextElement<TTerminal> elementAfter,
                                               int relativeCaretIndex)
        {
            ElementBefore = elementBefore;
            ElementAfter = elementAfter;
            RelativeCaretIndex = relativeCaretIndex;
        }
    }
}

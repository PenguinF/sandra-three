﻿/*********************************************************************************
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Changes the behavior of a <see cref="UpdatableRichTextBox"/> so it shows a read-only list of formatted text elements.
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
        public static SyntaxRenderer<TTerminal> AttachTo(UpdatableRichTextBox renderTarget) => new SyntaxRenderer<TTerminal>(renderTarget);

        private SyntaxRenderer(UpdatableRichTextBox renderTarget)
        {
            if (renderTarget == null) throw new ArgumentNullException(nameof(renderTarget));
            RenderTarget = renderTarget;

            Elements = elements.AsReadOnly();

            renderTarget.ReadOnly = true;
            renderTarget.Clear();
            renderTarget.SelectionChanged += (_, __) => tryInvokeCaretPositionChanged();

            assertInvariants();
        }

        internal readonly UpdatableRichTextBox RenderTarget;

        private readonly List<int> elementIndexes = new List<int>();

        private readonly List<TextElement<TTerminal>> elements = new List<TextElement<TTerminal>>();

        public readonly IReadOnlyList<TextElement<TTerminal>> Elements;

        [Conditional("DEBUG")]
        private void assertInvariants()
        {
            // Assert invariants about lengths being equal.
            int textLength = elementIndexes.Count;
            Debug.Assert(RenderTarget.TextLength == textLength);
            if (textLength == 0)
            {
                Debug.Assert(elements.Count == 0);
            }
            else
            {
                var lastElementIndex = elementIndexes[textLength - 1];
                Debug.Assert(lastElementIndex + 1 == elements.Count);
                var lastElement = elements[lastElementIndex];
                Debug.Assert(lastElement.Start + lastElement.Length == textLength);
            }
        }

        public TextElement<TTerminal> AppendTerminalSymbol(TTerminal terminal, string text)
        {
            if (terminal == null) throw new ArgumentNullException(nameof(terminal));
            if (text == null) throw new ArgumentNullException(nameof(text));

            int length = text.Length;
            if (length == 0) throw new NotImplementedException("Cannot append empty (lambda) terminals yet.");

            int start = RenderTarget.TextLength;
            RenderTarget.AppendText(text);
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
            elements.Clear();
            RenderTarget.Clear();
        }

        public void RemoveFrom(int index)
        {
            int textStart = elements[index].Start;
            int textLength = RenderTarget.TextLength - textStart;

            RenderTarget.Select(textStart, textLength);
            // This only works if not read-only, so temporarily turn it off.
            RenderTarget.ReadOnly = false;
            RenderTarget.SelectedText = string.Empty;
            RenderTarget.ReadOnly = true;

            elementIndexes.RemoveRange(textStart, textLength);
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
        public event Action<SyntaxRenderer<TTerminal>, EventArgs> CaretPositionChanged;

        private void tryInvokeCaretPositionChanged()
        {
            // Ignore updates as a result of all kinds of calls to Select()/SelectAll().
            // This is only to detect caret updates by interacting with the control.
            // Also check SelectionLength so the event is not raised for non-empty selections.
            if (!RenderTarget.IsUpdating && RenderTarget.SelectionLength == 0)
            {
                CaretPositionChanged?.Invoke(this, EventArgs.Empty);
            }
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

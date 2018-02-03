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
using System;
using System.Collections.Generic;

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
        }

        internal readonly UpdatableRichTextBox RenderTarget;

        private readonly List<TextElement<TTerminal>> elements = new List<TextElement<TTerminal>>();

        public readonly IReadOnlyList<TextElement<TTerminal>> Elements;

        public TextElement<TTerminal> AppendTerminalSymbol(TTerminal terminal, string text)
        {
            if (terminal == null) throw new ArgumentNullException(nameof(terminal));
            if (text == null) throw new ArgumentNullException(nameof(text));

            int length = text.Length;
            if (length == 0) throw new NotImplementedException("Cannot append empty (lambda) terminals yet.");

            int start = RenderTarget.TextLength;
            RenderTarget.AppendText(text);

            var textElement = new TextElement<TTerminal>(this)
            {
                TerminalSymbol = terminal,
                Start = start,
                Length = length,
            };

            elements.Add(textElement);
            return textElement;
        }

        /// <summary>
        /// Clears all syntax from the renderer.
        /// </summary>
        public void Clear()
        {
            elements.Clear();
            RenderTarget.Clear();
        }

        public void RemoveFrom(int index)
        {
            int textStart = elements[index].Start;
            RenderTarget.Select(textStart, RenderTarget.TextLength - textStart);
            // This only works if not read-only, so temporarily turn it off.
            RenderTarget.ReadOnly = false;
            RenderTarget.SelectedText = string.Empty;
            RenderTarget.ReadOnly = true;
            elements.RemoveRange(index, elements.Count - index);
        }

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
}

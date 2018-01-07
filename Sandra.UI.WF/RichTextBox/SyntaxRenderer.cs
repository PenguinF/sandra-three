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
            this.renderTarget = renderTarget;

            renderTarget.ReadOnly = true;
            renderTarget.Clear();
        }

        private readonly UpdatableRichTextBox renderTarget;

        public readonly List<TextElement<TTerminal>> Elements = new List<TextElement<TTerminal>>();

        public TextElement<TTerminal> AppendTerminalSymbol(TTerminal terminal, string text)
        {
            if (terminal == null) throw new ArgumentNullException(nameof(terminal));
            if (text == null) throw new ArgumentNullException(nameof(text));

            int length = text.Length;
            if (length == 0) throw new NotImplementedException("Cannot append empty (lambda) terminals yet.");

            int start = renderTarget.TextLength;
            renderTarget.AppendText(text);

            var textElement = new TextElement<TTerminal>()
            {
                TerminalSymbol = terminal,
                Start = start,
                Length = length,
            };

            Elements.Add(textElement);
            return textElement;
        }

        /// <summary>
        /// Clears all syntax from the renderer.
        /// </summary>
        public void Clear()
        {
            Elements.Clear();
            renderTarget.Clear();
        }

        public void RemoveFrom(int index)
        {
            int textStart = Elements[index].Start;
            renderTarget.Select(textStart, renderTarget.TextLength - textStart);
            // This only works if not read-only, so temporarily turn it off.
            renderTarget.ReadOnly = false;
            renderTarget.SelectedText = string.Empty;
            renderTarget.ReadOnly = true;
            Elements.RemoveRange(index, Elements.Count - index);
        }
    }
}

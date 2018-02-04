/*********************************************************************************
 * TextElement.cs
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
namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents an element of formatted text displayed by a <see cref="SyntaxRenderer"/>,
    /// which maps to exactly one terminal symbol.
    /// </summary>
    /// <typeparam name="TTerminal">
    /// The type of terminal symbols to format.
    /// See also: https://en.wikipedia.org/wiki/Terminal_and_nonterminal_symbols
    /// </typeparam>
    public sealed class TextElement<TTerminal>
    {
        private readonly SyntaxRenderer<TTerminal> renderer;

        internal TextElement(SyntaxRenderer<TTerminal> renderer)
        {
            this.renderer = renderer;
        }

        public TTerminal TerminalSymbol { get; internal set; }
        public int Start { get; internal set; }
        public int Length { get; internal set; }

        /// <summary>
        /// Returns the text element before this element. Returns null if this is the first text element.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// This element has been removed from a renderer.
        /// </exception>
        public TextElement<TTerminal> GetPreviousElement() => renderer.GetElementBefore(Start);

        /// <summary>
        /// Returns the text element before this element. Returns null if this is the first text element.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// This element has been removed from a renderer.
        /// </exception>
        public TextElement<TTerminal> GetNextElement() => renderer.GetElementAfter(Start + Length);

        /// <summary>
        /// Sets the caret directly before this text element and brings it into view.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// This element has been removed from a renderer.
        /// </exception>
        public void BringIntoViewBefore()
        {
            renderer.RenderTarget.Select(Start, 0);
            renderer.RenderTarget.ScrollToCaret();
        }

        /// <summary>
        /// Sets the caret directly after this text element and brings it into view.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// This element has been removed from a renderer.
        /// </exception>
        public void BringIntoViewAfter()
        {
            renderer.RenderTarget.Select(Start + Length, 0);
            renderer.RenderTarget.ScrollToCaret();
        }
    }
}

#region License
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
#endregion

using System;

namespace SysExtensions.TextIndex
{
    /// <summary>
    /// Represents an element of formatted text indexed by a <see cref="TextIndex{TTerminal}"/>,
    /// which maps to exactly one terminal symbol.
    /// </summary>
    /// <typeparam name="TTerminal">
    /// The type of terminal symbols to index.
    /// See also: https://en.wikipedia.org/wiki/Terminal_and_nonterminal_symbols
    /// </typeparam>
    public class TextElement<TTerminal>
    {
        private int start;
        private int length;

        /// <summary>
        /// Gets the terminal symbol associated with this element.
        /// </summary>
        public TTerminal TerminalSymbol { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="TextElement{TTerminal}"/>.
        /// </summary>
        /// <param name="terminalSymbol">
        /// The terminal symbol associated with this element.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="terminalSymbol"/> is negative.
        /// </exception>
        public TextElement(TTerminal terminalSymbol)
        {
            if (terminalSymbol == null) throw new ArgumentNullException(nameof(terminalSymbol));
            TerminalSymbol = terminalSymbol;
        }

        /// <summary>
        /// Gets the start position of this element.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="Start"/> is negative.
        /// </exception>
        public int Start
        {
            get { return start; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                start = value;
            }
        }

        /// <summary>
        /// Gets the length of this element.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="Length"/> is negative.
        /// </exception>
        public int Length
        {
            get { return length; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                length = value;
            }
        }

        /// <summary>
        /// Gets the end position of this element, which is <see cref="Length"/> added to <see cref="Start"/>.
        /// The end position is exclusive; the range of included characters is [<see cref="Start"/>..<see cref="End"/>-1].
        /// </summary>
        public int End => Start + Length;
    }
}

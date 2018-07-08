﻿#region License
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
        /// <summary>
        /// Gets the terminal symbol associated with this element.
        /// </summary>
        public TTerminal TerminalSymbol { get; internal set; }

        /// <summary>
        /// Gets the start position of this element.
        /// </summary>
        public int Start { get; internal set; }

        /// <summary>
        /// Gets the length of this element.
        /// </summary>
        public int Length { get; internal set; }

        /// <summary>
        /// Gets the end position of this element, which is <see cref="Length"/> added to <see cref="Start"/>.
        /// The end position is exclusive; the range of included characters is [<see cref="Start"/>..<see cref="End"/>-1].
        /// </summary>
        public int End => Start + Length;
    }
}

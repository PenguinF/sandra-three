#region License
/*********************************************************************************
 * PgnOverflowNagSyntax.cs
 *
 * Copyright (c) 2004-2020 Henk Nicolai
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

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a Numeric Annotation Glyph syntax node with an annotation value of 256 or larger.
    /// </summary>
    public sealed class GreenPgnOverflowNagSyntax : GreenPgnNagSyntax, IGreenPgnSymbol
    {
        /// <summary>
        /// The text containing the overflow NAG, including the '$' character.
        /// </summary>
        public string OverflowNagText { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public override int Length { get; }

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public override PgnSymbolType SymbolType => PgnSymbolType.OverflowNag;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnOverflowNagSyntax"/>.
        /// </summary>
        /// <param name="overflowNagText">
        /// The text containing the overflow NAG, including the '$' character.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="overflowNagText"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="overflowNagText"/> has a length of 3 or lower.
        /// </exception>
        public GreenPgnOverflowNagSyntax(string overflowNagText)
        {
            if (overflowNagText == null) throw new ArgumentNullException(nameof(overflowNagText));
            int length = overflowNagText.Length;
            if (length <= 3) throw new ArgumentOutOfRangeException(nameof(overflowNagText));

            OverflowNagText = overflowNagText;
            Length = length;
        }

        /// <summary>
        /// Generates the error associated with this symbol at a given start position.
        /// </summary>
        /// <param name="startPosition">
        /// The start position for which to generate the error.
        /// </param>
        /// <returns>
        /// The error associated with this symbol.
        /// </returns>
        public PgnErrorInfo GetError(int startPosition) => PgnNagSyntax.CreateOverflowNagMessage(OverflowNagText, startPosition);

        IEnumerable<PgnErrorInfo> IGreenPgnSymbol.GetErrors(int startPosition) => new SingleElementEnumerable<PgnErrorInfo>(GetError(startPosition));
    }
}

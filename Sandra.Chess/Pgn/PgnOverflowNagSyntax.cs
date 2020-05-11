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

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a Numeric Annotation Glyph syntax node with an annotation value of 256 or larger.
    /// </summary>
    public sealed class GreenPgnOverflowNagSyntax : GreenPgnNagSyntax
    {
        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public override PgnSymbolType SymbolType => PgnSymbolType.OverflowNag;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnOverflowNagSyntax"/>.
        /// </summary>
        /// <param name="length">
        /// The length of the text span corresponding with the node to create.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length"/> is 3 or lower.
        /// </exception>
        public GreenPgnOverflowNagSyntax(int length) : base(length)
        {
            if (length <= 3) throw new ArgumentOutOfRangeException(nameof(length));
        }
    }
}

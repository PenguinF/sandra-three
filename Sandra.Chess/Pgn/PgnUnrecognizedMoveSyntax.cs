#region License
/*********************************************************************************
 * PgnUnrecognizedMoveSyntax.cs
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
    /// Represents a PGN syntax node with an unrecognized move.
    /// </summary>
    public sealed class GreenPgnUnrecognizedMoveSyntax : GreenPgnMoveSyntax
    {
        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public override PgnSymbolType SymbolType => PgnSymbolType.UnrecognizedMove;

        /// <summary>
        /// Gets if this is an unrecognized move.
        /// </summary>
        public override bool IsUnrecognizedMove => true;

        /// <summary>
        /// Gets if this move syntax was parsed as a tag name but found outside of a tag section, and reinterpreted as an unknown move.
        /// If this property returns true, both <see cref="IsUnrecognizedMove"/> and <see cref="IsValidTagName"/> return true as well.
        /// </summary>
        public override bool IsConvertedFromTagName { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnUnrecognizedMoveSyntax"/>.
        /// </summary>
        /// <param name="length">
        /// The length of the text span corresponding with the node to create.
        /// </param>
        /// <param name="isConvertedFromTagName">
        /// If this move syntax was parsed as a tag name but found outside of a tag section, and reinterpreted as an unknown move.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length"/> is 0 or lower.
        /// </exception>
        public GreenPgnUnrecognizedMoveSyntax(int length, bool isConvertedFromTagName)
            : base(isConvertedFromTagName, length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            IsConvertedFromTagName = isConvertedFromTagName;
        }
    }
}

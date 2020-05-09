#region License
/*********************************************************************************
 * PgnMoveSyntax.cs
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
    /// Represents a syntax node which contains a move text.
    /// </summary>
    /// <remarks>
    /// When encountered in a tag pair, this node may be reinterpreted as a <see cref="GreenPgnTagNameSyntax"/>.
    /// </remarks>
    public sealed class GreenPgnMoveSyntax : IGreenPgnSymbol
    {
        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public PgnSymbolType SymbolType => PgnSymbolType.Move;

        /// <summary>
        /// Gets if the move syntax is a valid tag name (<see cref="GreenPgnTagNameSyntax"/>) as well.
        /// </summary>
        public bool IsValidTagName { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnMoveSyntax"/>.
        /// </summary>
        /// <param name="length">
        /// The length of the text span corresponding with the node to create.
        /// </param>
        /// <param name="isValidTagName">
        /// If the move syntax is a valid tag name (<see cref="GreenPgnTagNameSyntax"/>) as well.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length"/> is 1 or lower.
        /// </exception>
        public GreenPgnMoveSyntax(int length, bool isValidTagName)
        {
            if (length <= 1) throw new ArgumentOutOfRangeException(nameof(length));
            Length = length;
            IsValidTagName = isValidTagName;
        }

        IEnumerable<PgnErrorInfo> IGreenPgnSymbol.GetErrors(int startPosition) => EmptyEnumerable<PgnErrorInfo>.Instance;
    }

    public static class PgnMoveSyntax
    {
    }
}

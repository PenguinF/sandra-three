#region License
/*********************************************************************************
 * PgnUnknownSymbolSyntax.cs
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
    /// Represents a PGN syntax node with an unknown symbol.
    /// </summary>
    public sealed class GreenPgnUnknownSymbolSyntax : IGreenPgnSymbol
    {
        /// <summary>
        /// The text containing the unknown symbol.
        /// </summary>
        public string SymbolText { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public PgnSymbolType SymbolType => PgnSymbolType.Unknown;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnUnknownSymbolSyntax"/>.
        /// </summary>
        /// <param name="symbolText">
        /// The text containing the unknown symbol.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="symbolText"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="symbolText"/> has a length of 0.
        /// </exception>
        public GreenPgnUnknownSymbolSyntax(string symbolText)
        {
            if (symbolText == null) throw new ArgumentNullException(nameof(symbolText));
            int length = symbolText.Length;
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));

            SymbolText = symbolText;
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
        public PgnErrorInfo GetError(int startPosition) => PgnUnknownSymbolSyntax.CreateError(SymbolText, startPosition);

        IEnumerable<PgnErrorInfo> IGreenPgnSymbol.GetErrors(int startPosition) => new SingleElementEnumerable<PgnErrorInfo>(GetError(startPosition));
    }

    public static class PgnUnknownSymbolSyntax
    {
        /// <summary>
        /// Creates a <see cref="PgnErrorInfo"/> for a PGN syntax node with an unknown symbol.
        /// </summary>
        /// <param name="symbolText">
        /// The text containing the unknown symbol.
        /// </param>
        /// <param name="start">
        /// The start position of the unknown symbol.
        /// </param>
        public static PgnErrorInfo CreateError(string symbolText, int start)
            => new PgnErrorInfo(
                PgnErrorCode.UnknownSymbol,
                start,
                symbolText.Length,
                new[] { symbolText });
    }
}

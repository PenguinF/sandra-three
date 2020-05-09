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
using System.Collections.Generic;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a PGN syntax node with an unrecognized move.
    /// </summary>
    public sealed class GreenPgnUnrecognizedMoveSyntax : GreenPgnMoveSyntax, IGreenPgnSymbol
    {
        /// <summary>
        /// The text containing the unrecognized move.
        /// </summary>
        public string SymbolText { get; }

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public override PgnSymbolType SymbolType => PgnSymbolType.UnrecognizedMove;

        /// <summary>
        /// Gets if this is an unrecognized move.
        /// </summary>
        public override bool IsUnrecognizedMove => true;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnUnrecognizedMoveSyntax"/>.
        /// </summary>
        /// <param name="symbolText">
        /// The text containing the unrecognized move.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="symbolText"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="symbolText"/> has a length of 0.
        /// </exception>
        public GreenPgnUnrecognizedMoveSyntax(string symbolText) : base(symbolText == null ? 0 : symbolText.Length)
        {
            if (symbolText == null) throw new ArgumentNullException(nameof(symbolText));
            if (Length <= 0) throw new ArgumentException($"{symbolText} is empty.", nameof(symbolText));

            SymbolText = symbolText;
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
        public PgnErrorInfo GetError(int startPosition) => PgnMoveSyntax.CreateUnrecognizedMoveError(SymbolText, startPosition);

        IEnumerable<PgnErrorInfo> IGreenPgnSymbol.GetErrors(int startPosition) => new SingleElementEnumerable<PgnErrorInfo>(GetError(startPosition));
    }
}

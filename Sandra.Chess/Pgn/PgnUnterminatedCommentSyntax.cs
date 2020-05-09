#region License
/*********************************************************************************
 * PgnUnterminatedCommentSyntax.cs
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

using System.Collections.Generic;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a PGN syntax node which contains an unterminated comment.
    /// </summary>
    public sealed class GreenPgnUnterminatedCommentSyntax : GreenPgnCommentSyntax, IGreenPgnSymbol
    {
        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public override PgnSymbolType SymbolType => PgnSymbolType.UnterminatedComment;

        /// <summary>
        /// Gets if this is an unterminated comment.
        /// </summary>
        public override bool IsUnterminated => true;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnUnterminatedCommentSyntax"/>.
        /// </summary>
        /// <param name="length">
        /// The length of the syntax node, including delimiter characters ';' '{' '}',
        /// and excluding the '\n' and preceding '\r' that terminates an end-of-line comment.
        /// </param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="length"/> is 0 or lower.
        /// </exception>
        public GreenPgnUnterminatedCommentSyntax(int length)
            : base(length)
        {
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
        public PgnErrorInfo GetError(int startPosition) => PgnCommentSyntax.CreateUnterminatedCommentMessage(startPosition, Length);

        IEnumerable<PgnErrorInfo> IGreenPgnSymbol.GetErrors(int startPosition) => new SingleElementEnumerable<PgnErrorInfo>(GetError(startPosition));
    }
}

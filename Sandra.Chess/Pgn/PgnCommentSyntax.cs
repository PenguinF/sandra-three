#region License
/*********************************************************************************
 * PgnCommentSyntax.cs
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
    /// Represents a PGN syntax node which contains a comment.
    /// </summary>
    public class GreenPgnCommentSyntax : IGreenPgnSymbol
    {
        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public virtual PgnSymbolType SymbolType => PgnSymbolType.Comment;

        /// <summary>
        /// Gets if this is an unterminated comment.
        /// </summary>
        public virtual bool IsUnterminated => false;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnCommentSyntax"/>.
        /// </summary>
        /// <param name="length">
        /// The length of the syntax node, including delimiter characters ';' '{' '}',
        /// and excluding the '\n' and preceding '\r' that terminates an end-of-line comment.
        /// </param>
        public GreenPgnCommentSyntax(int length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            Length = length;
        }

        IEnumerable<PgnErrorInfo> IGreenPgnSymbol.GetErrors(int startPosition) => EmptyEnumerable<PgnErrorInfo>.Instance;
    }

    /// <summary>
    /// Represents a PGN syntax node which contains a comment.
    /// </summary>
    public static class PgnCommentSyntax
    {
        /// <summary>
        /// The character which starts an end-of-line comment.
        /// </summary>
        public const char EndOfLineCommentStartCharacter = ';';

        /// <summary>
        /// The character which starts an end-of-line comment.
        /// </summary>
        public const char MultiLineCommentStartCharacter = '{';

        /// <summary>
        /// The character which starts an end-of-line comment.
        /// </summary>
        public const char MultiLineCommentEndCharacter = '}';
    }
}

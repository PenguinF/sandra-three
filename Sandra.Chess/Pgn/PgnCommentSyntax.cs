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
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length"/> is 0 or lower.
        /// </exception>
        public GreenPgnCommentSyntax(int length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            Length = length;
        }
    }

    /// <summary>
    /// Represents a PGN syntax node which contains a comment.
    /// </summary>
    public sealed class PgnCommentSyntax : PgnSyntax, IPgnSymbol
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

        /// <summary>
        /// Creates a <see cref="PgnErrorInfo"/> for an unterminated comment.
        /// </summary>
        /// <param name="start">
        /// The start position of the unterminated comment.
        /// </param>
        /// <param name="length">
        /// The length of the unterminated comment.
        /// </param>
        public static PgnErrorInfo CreateUnterminatedCommentMessage(int start, int length)
            => new PgnErrorInfo(PgnErrorCode.UnterminatedMultiLineComment, start, length);

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public PgnTriviaElementSyntax Parent { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnCommentSyntax Green { get; }

        /// <summary>
        /// Gets if this is an unterminated comment.
        /// </summary>
        public bool IsUnterminated => Green.IsUnterminated;

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Green.BackgroundBefore.Length;

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public override int Length => Green.Length;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override PgnSyntax ParentSyntax => Parent;

        internal PgnCommentSyntax(PgnTriviaElementSyntax parent, GreenPgnCommentSyntax green)
        {
            Parent = parent;
            Green = green;
        }

        void IPgnSymbol.Accept(PgnSymbolVisitor visitor) => visitor.VisitCommentSyntax(this);
        TResult IPgnSymbol.Accept<TResult>(PgnSymbolVisitor<TResult> visitor) => visitor.VisitCommentSyntax(this);
        TResult IPgnSymbol.Accept<T, TResult>(PgnSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitCommentSyntax(this, arg);
    }
}

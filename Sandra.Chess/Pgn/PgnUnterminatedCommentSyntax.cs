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

using System;
using System.Collections.Generic;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a PGN syntax node which contains an unterminated comment.
    /// </summary>
    public sealed class GreenPgnUnterminatedCommentSyntax : GreenPgnBackgroundSyntax, IGreenPgnSymbol
    {
        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public override int Length { get; }

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public PgnSymbolType SymbolType => PgnSymbolType.UnterminatedComment;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnUnterminatedCommentSyntax"/>.
        /// </summary>
        /// <param name="length">
        /// The length of the syntax node, including delimiter characters ';' '{' '}',
        /// and excluding the '\n' and preceding '\r' that terminates an end-of-line comment.
        /// </param>
        public GreenPgnUnterminatedCommentSyntax(int length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            Length = length;
        }

        /// <summary>
        /// Generates the error associated with this symbol at a given start position.
        /// </summary>
        /// <param name="startPosition">
        /// The start position for which to generate the errors.
        /// </param>
        /// <returns>
        /// The errors associated with this symbol.
        /// </returns>
        public PgnErrorInfo GetError(int startPosition) => PgnUnterminatedCommentSyntax.CreateError(startPosition, Length);

        IEnumerable<PgnErrorInfo> IGreenPgnSymbol.GetErrors(int startPosition) => new SingleElementEnumerable<PgnErrorInfo>(GetError(startPosition));

        public override void Accept(GreenPgnBackgroundSyntaxVisitor visitor) => visitor.VisitUnterminatedCommentSyntax(this);
        public override TResult Accept<TResult>(GreenPgnBackgroundSyntaxVisitor<TResult> visitor) => visitor.VisitUnterminatedCommentSyntax(this);
        public override TResult Accept<T, TResult>(GreenPgnBackgroundSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitUnterminatedCommentSyntax(this, arg);
    }

    /// <summary>
    /// Represents a PGN syntax node which contains an unterminated comment.
    /// </summary>
    public sealed class PgnUnterminatedCommentSyntax : PgnBackgroundSyntax, IPgnSymbol
    {
        /// <summary>
        /// Creates a <see cref="PgnErrorInfo"/> for an unterminated comment.
        /// </summary>
        /// <param name="start">
        /// The start position of the unterminated comment.
        /// </param>
        /// <param name="length">
        /// The length of the unterminated comment.
        /// </param>
        public static PgnErrorInfo CreateError(int start, int length)
            => new PgnErrorInfo(PgnErrorCode.UnterminatedMultiLineComment, start, length);

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnUnterminatedCommentSyntax Green { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        internal PgnUnterminatedCommentSyntax(PgnSyntaxNodes parent, int parentIndex, GreenPgnUnterminatedCommentSyntax green)
            : base(parent, parentIndex)
            => Green = green;

        void IPgnSymbol.Accept(PgnSymbolVisitor visitor) => visitor.VisitUnterminatedCommentSyntax(this);
        TResult IPgnSymbol.Accept<TResult>(PgnSymbolVisitor<TResult> visitor) => visitor.VisitUnterminatedCommentSyntax(this);
        TResult IPgnSymbol.Accept<T, TResult>(PgnSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitUnterminatedCommentSyntax(this, arg);
    }
}

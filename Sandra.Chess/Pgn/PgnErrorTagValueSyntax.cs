#region License
/*********************************************************************************
 * PgnErrorTagValueSyntax.cs
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

using Eutherion.Text;
using System.Collections.Generic;
using System.Linq;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a tag value syntax node which contains errors.
    /// </summary>
    public sealed class GreenPgnErrorTagValueSyntax : GreenPgnTagValueSyntax, IGreenPgnSymbol
    {
        internal ReadOnlyList<PgnErrorInfo> Errors { get; }

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public override PgnSymbolType SymbolType => PgnSymbolType.ErrorTagValue;

        /// <summary>
        /// Gets if this tag value contains errors and therefore has an undefined value.
        /// </summary>
        public override bool ContainsErrors => true;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnErrorTagValueSyntax"/>.
        /// </summary>
        /// <param name="length">
        /// The length of the text span corresponding with the node to create.
        /// </param>
        /// <param name="errors">
        /// A sequence of errors associated with this symbol.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="errors"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="length"/> is 0 or lower.
        /// </exception>
        public GreenPgnErrorTagValueSyntax(int length, IEnumerable<PgnErrorInfo> errors)
            : base(length)
        {
            Errors = ReadOnlyList<PgnErrorInfo>.Create(errors);
        }

        /// <summary>
        /// Generates a sequence of errors associated with this symbol at a given start position.
        /// </summary>
        /// <param name="startPosition">
        /// The start position for which to generate the errors.
        /// </param>
        /// <returns>
        /// A sequence of errors associated with this symbol.
        /// </returns>
        public IEnumerable<PgnErrorInfo> GetErrors(int startPosition)
            => Errors.Select(error => new PgnErrorInfo(
                error.ErrorCode,
                error.Start + startPosition,
                error.Length,
                error.Parameters));

        public override void Accept(GreenPgnTagElementSyntaxVisitor visitor) => visitor.VisitErrorTagValueSyntax(this);
        public override TResult Accept<TResult>(GreenPgnTagElementSyntaxVisitor<TResult> visitor) => visitor.VisitErrorTagValueSyntax(this);
        public override TResult Accept<T, TResult>(GreenPgnTagElementSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitErrorTagValueSyntax(this, arg);
    }

    /// <summary>
    /// Represents a tag value syntax node which contains errors.
    /// </summary>
    public sealed class PgnErrorTagValueSyntax : PgnTagElementSyntax, IPgnSymbol
    {
        /// <summary>
        /// Creates a <see cref="PgnErrorInfo"/> for unterminated tag values.
        /// </summary>
        /// <param name="length">
        /// The length of the unterminated tag value.
        /// </param>
        public static PgnErrorInfo Unterminated(int length)
            => new PgnErrorInfo(PgnErrorCode.UnterminatedTagValue, 0, length);

        /// <summary>
        /// Creates a <see cref="PgnErrorInfo"/> for unrecognized escape sequences.
        /// </summary>
        /// <param name="displayCharValue">
        /// A friendly representation of the unrecognized escape sequence.
        /// </param>
        /// <param name="start">
        /// The start position of the unrecognized escape sequence relative to the start position of the tag value.
        /// </param>
        /// <param name="length">
        /// The length of the unrecognized escape sequence.
        /// </param>
        public static PgnErrorInfo UnrecognizedEscapeSequence(string displayCharValue, int start, int length)
            => new PgnErrorInfo(PgnErrorCode.UnrecognizedEscapeSequence, start, length, new[] { displayCharValue });

        /// <summary>
        /// Creates a <see cref="PgnErrorInfo"/> for illegal control characters in tag values.
        /// </summary>
        /// <param name="illegalCharacter">
        /// The illegal control character.
        /// </param>
        /// <param name="position">
        /// The position of the illegal control character relative to the start position of the tag value.
        /// </param>
        public static PgnErrorInfo IllegalControlCharacter(char illegalCharacter, int position)
            => new PgnErrorInfo(PgnErrorCode.IllegalControlCharacterInTagValue, position, 1, new[] { StringLiteral.EscapedCharacterString(illegalCharacter) });

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnErrorTagValueSyntax Green { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        internal PgnErrorTagValueSyntax(PgnTagElementWithTriviaSyntax parent, GreenPgnErrorTagValueSyntax green) : base(parent) => Green = green;

        public override void Accept(PgnTagElementSyntaxVisitor visitor) => visitor.VisitErrorTagValueSyntax(this);
        public override TResult Accept<TResult>(PgnTagElementSyntaxVisitor<TResult> visitor) => visitor.VisitErrorTagValueSyntax(this);
        public override TResult Accept<T, TResult>(PgnTagElementSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitErrorTagValueSyntax(this, arg);

        void IPgnSymbol.Accept(PgnSymbolVisitor visitor) => visitor.VisitErrorTagValueSyntax(this);
        TResult IPgnSymbol.Accept<TResult>(PgnSymbolVisitor<TResult> visitor) => visitor.VisitErrorTagValueSyntax(this);
        TResult IPgnSymbol.Accept<T, TResult>(PgnSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitErrorTagValueSyntax(this, arg);
    }
}

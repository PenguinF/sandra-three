#region License
/*********************************************************************************
 * PgnTagValueSyntax.cs
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
using System;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a tag value syntax node.
    /// </summary>
    public class GreenPgnTagValueSyntax : GreenPgnTagElementSyntax
    {
        /// <summary>
        /// Gets the value of this syntax node, or null if this is a <see cref="GreenPgnErrorTagValueSyntax"/>.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public sealed override int Length { get; }

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public override PgnSymbolType SymbolType => PgnSymbolType.TagValue;

        /// <summary>
        /// Gets if this tag value contains errors and therefore has an undefined value.
        /// </summary>
        public virtual bool ContainsErrors => false;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnTagValueSyntax"/>.
        /// </summary>
        /// <param name="value">
        /// The value of the tag.
        /// </param>
        /// <param name="length">
        /// The length of the text span corresponding with the node to create.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length"/> is 0 or lower.
        /// </exception>
        public GreenPgnTagValueSyntax(string value, int length)
            : this(length)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal GreenPgnTagValueSyntax(int length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            Length = length;
        }

        internal override PgnTagElementSyntax CreateRedNode(PgnTagElementWithTriviaSyntax parent) => new PgnTagValueSyntax(parent, this);
    }

    /// <summary>
    /// Represents a tag value syntax node.
    /// </summary>
    public sealed class PgnTagValueSyntax : PgnTagElementSyntax, IPgnSymbol
    {
        /// <summary>
        /// Creates a <see cref="PgnErrorInfo"/> for unterminated tag values.
        /// </summary>
        /// <param name="startPosition">
        /// The start position of the unterminated tag value.
        /// </param>
        /// <param name="length">
        /// The length of the unterminated tag value.
        /// </param>
        public static PgnErrorInfo UnterminatedError(int startPosition, int length)
            => new PgnErrorInfo(PgnErrorCode.UnterminatedTagValue, startPosition, length);

        /// <summary>
        /// Creates a <see cref="PgnErrorInfo"/> for unrecognized escape sequences.
        /// </summary>
        /// <param name="displayCharValue">
        /// A friendly representation of the unrecognized escape sequence.
        /// </param>
        /// <param name="startPosition">
        /// The start position of the unrecognized escape sequence relative to the start position of the tag value.
        /// </param>
        /// <param name="length">
        /// The length of the unrecognized escape sequence.
        /// </param>
        public static PgnErrorInfo UnrecognizedEscapeSequenceError(string displayCharValue, int startPosition, int length)
            => new PgnErrorInfo(PgnErrorCode.UnrecognizedEscapeSequence, startPosition, length, new[] { displayCharValue });

        /// <summary>
        /// Creates a <see cref="PgnErrorInfo"/> for illegal control characters in tag values.
        /// </summary>
        /// <param name="illegalCharacter">
        /// The illegal control character.
        /// </param>
        /// <param name="position">
        /// The position of the illegal control character relative to the start position of the tag value.
        /// </param>
        public static PgnErrorInfo IllegalControlCharacterError(char illegalCharacter, int position)
            => new PgnErrorInfo(PgnErrorCode.IllegalControlCharacterInTagValue, position, 1, new[] { StringLiteral.EscapedCharacterString(illegalCharacter) });

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnTagValueSyntax Green { get; }

        /// <summary>
        /// Gets if this tag value contains errors and therefore has an undefined value.
        /// </summary>
        public bool ContainsErrors => Green.ContainsErrors;

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        internal PgnTagValueSyntax(PgnTagElementWithTriviaSyntax parent, GreenPgnTagValueSyntax green) : base(parent) => Green = green;

        public override void Accept(PgnTagElementSyntaxVisitor visitor) => visitor.VisitTagValueSyntax(this);
        public override TResult Accept<TResult>(PgnTagElementSyntaxVisitor<TResult> visitor) => visitor.VisitTagValueSyntax(this);
        public override TResult Accept<T, TResult>(PgnTagElementSyntaxVisitor<T, TResult> visitor, T arg) => visitor.VisitTagValueSyntax(this, arg);

        void IPgnSymbol.Accept(PgnSymbolVisitor visitor) => visitor.VisitTagValueSyntax(this);
        TResult IPgnSymbol.Accept<TResult>(PgnSymbolVisitor<TResult> visitor) => visitor.VisitTagValueSyntax(this);
        TResult IPgnSymbol.Accept<T, TResult>(PgnSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitTagValueSyntax(this, arg);
    }
}

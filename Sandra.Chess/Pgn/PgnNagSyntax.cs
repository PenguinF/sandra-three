#region License
/*********************************************************************************
 * PgnNagSyntax.cs
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

using Sandra.Chess.Pgn.Temp;
using System;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a Numeric Annotation Glyph syntax node.
    /// </summary>
    public class GreenPgnNagSyntax : IGreenPgnSymbol
    {
        /// <summary>
        /// Gets the empty <see cref="GreenPgnNagSyntax"/>.
        /// </summary>
        public static GreenPgnNagSyntax Empty => GreenPgnEmptyNagSyntax.Value;

        /// <summary>
        /// Gets the annotation value of this syntax node.
        /// Is <see cref="PgnAnnotation.Null"/> for '$', '$0', and '$256' or greater.
        /// </summary>
        public PgnAnnotation Annotation { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public virtual PgnSymbolType SymbolType => PgnSymbolType.Nag;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnNagSyntax"/>.
        /// </summary>
        /// <param name="annotation">
        /// The annotation value.
        /// </param>
        /// <param name="length">
        /// The length of the text span corresponding with the node to create, including the '$' character.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length"/> is 1 or lower.
        /// </exception>
        public GreenPgnNagSyntax(PgnAnnotation annotation, int length) : this(length)
        {
            Annotation = annotation;
            if (length <= 1) throw new ArgumentOutOfRangeException(nameof(length));
        }

        internal GreenPgnNagSyntax(int length) => Length = length;
    }

    /// <summary>
    /// Represents a Numeric Annotation Glyph syntax node.
    /// </summary>
    public sealed class PgnNagSyntax : PgnSyntax, IPgnSymbol
    {
        public const char NagCharacter = '$';
        public const int NagLength = 1;

        /// <summary>
        /// Creates a <see cref="PgnErrorInfo"/> for an empty Numeric Annotation Glyph.
        /// </summary>
        /// <param name="start">
        /// The start position of the empty Numeric Annotation Glyph.
        /// </param>
        public static PgnErrorInfo CreateEmptyNagMessage(int start)
            => new PgnErrorInfo(PgnErrorCode.EmptyNag, start, NagLength);

        /// <summary>
        /// Creates a <see cref="PgnErrorInfo"/> for a Numeric Annotation Glyph with an annotation value of 256 or larger.
        /// </summary>
        /// <param name="overflowNagText">
        /// The text containing the overflow NAG, including the '$' character.
        /// </param>
        /// <param name="start">
        /// The start position of the Numeric Annotation Glyph with an annotation value of 256 or larger.
        /// </param>
        public static PgnErrorInfo CreateOverflowNagMessage(string overflowNagText, int start)
            => new PgnErrorInfo(
                PgnErrorCode.OverflowNag,
                start,
                overflowNagText.Length,
                new[] { overflowNagText });

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public PgnNagWithTriviaSyntax Parent { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenPgnNagSyntax Green { get; }

        /// <summary>
        /// Gets the annotation value of this syntax node.
        /// Is <see cref="PgnAnnotation.Null"/> for '$', '$0', and '$256' or greater.
        /// </summary>
        public PgnAnnotation Annotation => Green.Annotation;

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Green.LeadingTrivia.Length;

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override PgnSyntax ParentSyntax => Parent;

        internal PgnNagSyntax(PgnNagWithTriviaSyntax parent, GreenPgnNagSyntax green)
        {
            Parent = parent;
            Green = green;
        }

        void IPgnSymbol.Accept(PgnSymbolVisitor visitor) => visitor.VisitNagSyntax(this);
        TResult IPgnSymbol.Accept<TResult>(PgnSymbolVisitor<TResult> visitor) => visitor.VisitNagSyntax(this);
        TResult IPgnSymbol.Accept<T, TResult>(PgnSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitNagSyntax(this, arg);
    }

    /// <summary>
    /// Represents a Numeric Annotation Glyph syntax node, together with its leading trivia.
    /// </summary>
    public sealed class PgnNagWithTriviaSyntax : WithTriviaSyntax<PgnNagSyntax>, IPgnTopLevelSyntax
    {
        PgnSyntax IPgnTopLevelSyntax.ToPgnSyntax() => this;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public WithPlyFloatItemsSyntax Parent { get; }

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Green.LeadingFloatItems.Length;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override PgnSyntax ParentSyntax => Parent;

        internal override PgnNagSyntax CreateContentNode() => new PgnNagSyntax(this, (GreenPgnNagSyntax)Green.ContentNode);

        internal PgnNagWithTriviaSyntax(WithPlyFloatItemsSyntax parent, GreenWithTriviaSyntax green)
            : base(green)
        {
            Parent = parent;
        }
    }
}

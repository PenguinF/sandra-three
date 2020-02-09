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

using Eutherion;
using Eutherion.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a tag value syntax node which contains errors.
    /// </summary>
    public sealed class GreenPgnErrorTagValueSyntax : IPgnForegroundSymbol
    {
        internal ReadOnlyList<PgnErrorInfo> Errors { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnErrorTagValueSyntax"/>.
        /// </summary>
        /// <param name="length">
        /// The length of the text span corresponding with the node to create.
        /// </param>
        /// <param name="errors">
        /// A sequence of errors associated with this symbol.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="errors"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="length"/> is 0 or lower.
        /// </exception>
        public GreenPgnErrorTagValueSyntax(int length, IEnumerable<PgnErrorInfo> errors)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
            Length = length;
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

        Union<GreenPgnBackgroundSyntax, IPgnForegroundSymbol> IGreenPgnSymbol.AsBackgroundOrForeground() => this;
    }

    public static class PgnErrorTagValueSyntax
    {
        /// <summary>
        /// Creates a <see cref="PgnErrorInfo"/> for unterminated tag values.
        /// </summary>
        /// <param name="start">
        /// The start position of the unterminated tag value.
        /// </param>
        /// <param name="length">
        /// The length of the unterminated tag value.
        /// </param>
        public static PgnErrorInfo Unterminated(int start, int length)
            => new PgnErrorInfo(PgnErrorCode.UnterminatedTagValue, start, length);
    }
}

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
    }
}

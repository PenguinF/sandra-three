#region License
/*********************************************************************************
 * IPgnSymbol.cs
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
using Eutherion.Text;
using System.Collections.Generic;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a terminal PGN symbol.
    /// Instances of this type are returned by <see cref="PgnTokenizer"/>.
    /// </summary>
    public interface IGreenPgnSymbol : ISpan
    {
        /// <summary>
        /// Generates a sequence of errors associated with this symbol at a given start position.
        /// </summary>
        /// <param name="startPosition">
        /// The start position for which to generate the errors.
        /// </param>
        /// <returns>
        /// A sequence of errors associated with this symbol.
        /// </returns>
        IEnumerable<PgnErrorInfo> GetErrors(int startPosition);

        /// <summary>
        /// Converts this symbol into either a <see cref="GreenPgnBackgroundSyntax"/> or a <see cref="IPgnForegroundSymbol"/>.
        /// </summary>
        /// <returns>
        /// Either a <see cref="GreenPgnBackgroundSyntax"/> or a <see cref="IPgnForegroundSymbol"/>.
        /// </returns>
        Union<GreenPgnBackgroundSyntax, IPgnForegroundSymbol> AsBackgroundOrForeground();
    }

    public interface IPgnForegroundSymbol : IGreenPgnSymbol
    {
    }

    // Temporary placeholder
    public class PgnSymbol : IPgnForegroundSymbol
    {
        public int Length { get; }

        public PgnSymbol(int length) => Length = length;

        IEnumerable<PgnErrorInfo> IGreenPgnSymbol.GetErrors(int startPosition) => EmptyEnumerable<PgnErrorInfo>.Instance;

        Union<GreenPgnBackgroundSyntax, IPgnForegroundSymbol> IGreenPgnSymbol.AsBackgroundOrForeground() => this;
    }

    /// <summary>
    /// Represents a terminal PGN symbol.
    /// These are all <see cref="PgnSyntax"/> nodes which have no child <see cref="PgnSyntax"/> nodes.
    /// </summary>
    public interface IPgnSymbol : ISpan
    {
    }
}

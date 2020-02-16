#region License
/*********************************************************************************
 * PgnBracketOpenSyntax.cs
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
using System.Collections.Generic;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents the bracket open character '[' in PGN text.
    /// </summary>
    public sealed class GreenPgnBracketOpenSyntax : IPgnForegroundSymbol
    {
        /// <summary>
        /// Gets the single <see cref="GreenPgnBracketOpenSyntax"/> value.
        /// </summary>
        public static GreenPgnBracketOpenSyntax Value { get; } = new GreenPgnBracketOpenSyntax();

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length => PgnBracketOpenSyntax.BracketOpenLength;

        private GreenPgnBracketOpenSyntax() { }

        IEnumerable<PgnErrorInfo> IGreenPgnSymbol.GetErrors(int startPosition) => EmptyEnumerable<PgnErrorInfo>.Instance;
        Union<GreenPgnBackgroundSyntax, IPgnForegroundSymbol> IGreenPgnSymbol.AsBackgroundOrForeground() => this;
    }

    public static class PgnBracketOpenSyntax
    {
        public const char BracketOpenCharacter = '[';
        public const int BracketOpenLength = 1;
    }
}

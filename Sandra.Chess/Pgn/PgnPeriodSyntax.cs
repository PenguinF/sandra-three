#region License
/*********************************************************************************
 * PgnPeriodSyntax.cs
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
    /// Represents the period character '.' in PGN text.
    /// </summary>
    public sealed class GreenPgnPeriodSyntax : IPgnForegroundSymbol
    {
        /// <summary>
        /// Gets the single <see cref="GreenPgnPeriodSyntax"/> value.
        /// </summary>
        public static GreenPgnPeriodSyntax Value { get; } = new GreenPgnPeriodSyntax();

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length => PgnPeriodSyntax.PeriodLength;

        private GreenPgnPeriodSyntax() { }

        IEnumerable<PgnErrorInfo> IGreenPgnSymbol.GetErrors(int startPosition) => EmptyEnumerable<PgnErrorInfo>.Instance;
        Union<GreenPgnBackgroundSyntax, IPgnForegroundSymbol> IGreenPgnSymbol.AsBackgroundOrForeground() => this;
    }

    public static class PgnPeriodSyntax
    {
        public const char PeriodCharacter = '.';
        public const int PeriodLength = 1;
    }
}

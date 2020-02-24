﻿#region License
/*********************************************************************************
 * PgnUnknownSymbolSyntax.cs
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

using System.Collections.Generic;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a PGN syntax node with an unknown symbol.
    /// </summary>
    public sealed class GreenPgnUnknownSymbolSyntax : IGreenPgnSymbol
    {
        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public PgnSymbolType SymbolType => PgnSymbolType.Unknown;

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnUnknownSymbolSyntax"/>.
        /// </summary>
        public GreenPgnUnknownSymbolSyntax(int length)
        {
            Length = length;
        }

        IEnumerable<PgnErrorInfo> IGreenPgnSymbol.GetErrors(int startPosition) => EmptyEnumerable<PgnErrorInfo>.Instance;
    }
}
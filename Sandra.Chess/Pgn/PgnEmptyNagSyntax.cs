﻿#region License
/*********************************************************************************
 * PgnEmptyNagSyntax.cs
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

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a Numeric Annotation Glyph syntax node with an empty annotation.
    /// </summary>
    public sealed class GreenPgnEmptyNagSyntax : GreenPgnNagSyntax
    {
        /// <summary>
        /// Gets the single <see cref="GreenPgnEmptyNagSyntax"/> value.
        /// </summary>
        public static GreenPgnEmptyNagSyntax Value { get; } = new GreenPgnEmptyNagSyntax();

        /// <summary>
        /// Gets the type of this symbol.
        /// </summary>
        public override PgnSymbolType SymbolType => PgnSymbolType.EmptyNag;

        private GreenPgnEmptyNagSyntax() : base(PgnNagSyntax.NagLength) { }
    }
}

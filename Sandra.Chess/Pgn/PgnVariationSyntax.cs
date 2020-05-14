#region License
/*********************************************************************************
 * PgnVariationSyntax.cs
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
    /// Represents a syntax node which contains a side line, i.e. a list of plies and their surrounding parentheses.
    /// </summary>
    public sealed class GreenPgnVariationSyntax : ISpan
    {
        /// <summary>
        /// Gets the opening parenthesis.
        /// </summary>
        public GreenWithTriviaSyntax ParenthesisOpen { get; }

        /// <summary>
        /// Gets the list of plies and trailing floating items that are not captured by a ply.
        /// </summary>
        public GreenPgnPlyListSyntax PliesWithFloatItems { get; }

        /// <summary>
        /// Gets the closing parenthesis. The closing parenthesis can be null.
        /// </summary>
        public GreenWithTriviaSyntax ParenthesisClose { get; }

        /// <summary>
        /// Returns the closing parenthesis's length or 0 if it is missing.
        /// </summary>
        public int ParenthesisCloseLength => ParenthesisClose == null ? 0 : ParenthesisClose.Length;

        /// <summary>
        /// Gets the length of the text span corresponding with this node.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="GreenPgnVariationSyntax"/>.
        /// </summary>
        /// <param name="parenthesisOpen">
        /// The opening parenthesis.
        /// </param>
        /// <param name="pliesWithFloatItems">
        /// The list of plies and trailing floating items that are not captured by a ply.
        /// </param>
        /// <param name="parenthesisClose">
        /// The closing parenthesis. This is an optional parameter.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="parenthesisOpen"/> and/or <paramref name="pliesWithFloatItems"/> is null.
        /// </exception>
        public GreenPgnVariationSyntax(
            GreenWithTriviaSyntax parenthesisOpen,
            GreenPgnPlyListSyntax pliesWithFloatItems,
            GreenWithTriviaSyntax parenthesisClose)
        {
            ParenthesisOpen = parenthesisOpen ?? throw new ArgumentNullException(nameof(parenthesisOpen));
            PliesWithFloatItems = pliesWithFloatItems ?? throw new ArgumentNullException(nameof(pliesWithFloatItems));
            ParenthesisClose = parenthesisClose;

            Length = parenthesisOpen.Length + pliesWithFloatItems.Length + ParenthesisCloseLength;
        }
    }
}

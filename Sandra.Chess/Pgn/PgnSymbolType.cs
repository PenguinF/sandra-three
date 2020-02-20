#region License
/*********************************************************************************
 * PgnSymbolType.cs
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
    /// Denotes the type of a <see cref="IGreenPgnSymbol"/>.
    /// </summary>
    public enum PgnSymbolType
    {
        /// <summary>
        /// Type of a PGN syntax node which contains whitespace.
        /// </summary>
        Whitespace,

        /// <summary>
        /// Type of a character which is illegal in the PGN standard.
        /// </summary>
        IllegalCharacter,

        /// <summary>
        /// Type of a PGN syntax node which contains a comment.
        /// </summary>
        Comment,

        /// <summary>
        /// Type of the asterisk character '*' in PGN text.
        /// </summary>
        Asterisk,

        /// <summary>
        /// Type of the bracket close character ']' in PGN text.
        /// </summary>
        BracketClose,

        /// <summary>
        /// Type of the bracket open character '[' in PGN text.
        /// </summary>
        BracketOpen,

        /// <summary>
        /// Type of a tag value syntax node which contains errors.
        /// </summary>
        ErrorTagValue,

        /// <summary>
        /// Type of the parenthesis close character ')' in PGN text.
        /// </summary>
        ParenthesisClose,

        /// <summary>
        /// Type of the parenthesis open character '(' in PGN text.
        /// </summary>
        ParenthesisOpen,

        /// <summary>
        /// Type of the period character '.' in PGN text.
        /// </summary>
        Period,

        /// <summary>
        /// Type of a tag name syntax node.
        /// </summary>
        TagName,

        /// <summary>
        /// Type of a tag value syntax node.
        /// </summary>
        TagValue,

        OtherNotSpecified
    }
}

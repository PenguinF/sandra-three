#region License
/*********************************************************************************
 * JsonSymbolType.cs
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

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Denotes the type of a <see cref="IGreenJsonSymbol"/>.
    /// </summary>
    public enum JsonSymbolType
    {
        /// <summary>
        /// Type of a json syntax node which contains whitespace.
        /// </summary>
        Whitespace = 0,

        /// <summary>
        /// Type of a json syntax node which contains a comment.
        /// </summary>
        Comment = 1,

        /// <summary>
        /// Type of a json syntax node which contains an unterminated multi-line comment.
        /// </summary>
        UnterminatedMultiLineComment = 2,

        /// <summary>
        /// Type of a boolean literal value syntax node.
        /// </summary>
        BooleanLiteral = 4,

        /// <summary>
        /// Type of an integer literal value syntax node.
        /// </summary>
        IntegerLiteral = 5,

        /// <summary>
        /// Type of a string literal value syntax node.
        /// </summary>
        StringLiteral = 6,

        /// <summary>
        /// Type of a string literal value syntax node which contains errors.
        /// </summary>
        ErrorString = 7,

        /// <summary>
        /// Type of a json syntax node with an undefined or unsupported value.
        /// </summary>
        UndefinedValue = 8,

        /// <summary>
        /// Type of a json syntax node with an unknown symbol.
        /// </summary>
        UnknownSymbol = 9,

        /// <summary>
        /// Type of a json curly open syntax node.
        /// </summary>
        CurlyOpen = 10,

        /// <summary>
        /// Type of a json square bracket open syntax node.
        /// </summary>
        BracketOpen = 11,

        /// <summary>
        /// Type of a json colon syntax node.
        /// </summary>
        Colon = 12,

        /// <summary>
        /// Type of a json comma syntax node.
        /// </summary>
        Comma = 13,

        /// <summary>
        /// Type of a json curly close syntax node.
        /// </summary>
        CurlyClose = 14,

        /// <summary>
        /// Type of a json square bracket close syntax node.
        /// </summary>
        BracketClose = 15,

        /// <summary>
        /// Helper symbol type for internal use by the <see cref="JsonParser"/>.
        /// </summary>
        Eof = 16,
    }
}

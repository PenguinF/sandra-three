#region License
/*********************************************************************************
 * PgnErrorCode.cs
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
    /// Enumerates distinct PGN syntax and semantic error types.
    /// </summary>
    public enum PgnErrorCode
    {
        /// <summary>
        /// Occurs when a character is illegal.
        /// </summary>
        IllegalCharacter,

        /// <summary>
        /// Occurs when a tag value is not terminated before the end of the file.
        /// </summary>
        UnterminatedTagValue,

        /// <summary>
        /// Occurs when an escape sequence in a tag value is not recognized.
        /// </summary>
        UnrecognizedEscapeSequence,

        /// <summary>
        /// Occurs when a control character appears in a tag value.
        /// </summary>
        IllegalControlCharacterInTagValue,

        /// <summary>
        /// Occurs when a comment is not terminated before the end of the file.
        /// </summary>
        UnterminatedMultiLineComment,

        /// <summary>
        /// Occurs when a '$' character is not followed by an integer value.
        /// </summary>
        EmptyNag,

        /// <summary>
        /// Occurs when a '$' character is followed by an integer value which is too large (256 or higher).
        /// </summary>
        OverflowNag,

        /// <summary>
        /// Occurs when a symbol is not recognized as a move text, tag name, move number, or game termination marker.
        /// </summary>
        UnrecognizedMove,

        /// <summary>
        /// Occurs when a tag pair contains neither a name nor a value.
        /// </summary>
        EmptyTag,

        /// <summary>
        /// Occurs when a tag pair misses its '[' character.
        /// </summary>
        MissingTagBracketOpen,

        /// <summary>
        /// Occurs when a tag pair misses its tag name.
        /// </summary>
        MissingTagName,

        /// <summary>
        /// Occurs when a tag pair misses its tag value.
        /// </summary>
        MissingTagValue,

        /// <summary>
        /// Occurs when a tag pair contains two or more values.
        /// </summary>
        MultipleTagValues,

        /// <summary>
        /// Occurs when a tag pair misses its ']' character.
        /// </summary>
        MissingTagBracketClose,
    }
}

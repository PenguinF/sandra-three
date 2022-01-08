#region License
/*********************************************************************************
 * JsonSpecialCharacter.cs
 *
 * Copyright (c) 2004-2022 Henk Nicolai
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
    /// Contains definitions for special characters which carry a specific syntactical meaning.
    /// </summary>
    public static class JsonSpecialCharacter
    {
        /// <summary>
        /// Returns 1, which is the length of every special character.
        /// </summary>
        /// <remarks>
        /// This identifier is added in order to be able to clarify why a constant length of 1 is used somewhere.
        /// </remarks>
        public const int SpecialCharacterLength = 1;

        /// <summary>
        /// Returns the colon character ':', which separates keys from values.
        /// </summary>
        public const char ColonCharacter = ':';

        /// <summary>
        /// Returns the comma character ',', which separates values.
        /// </summary>
        public const char CommaCharacter = ',';

        /// <summary>
        /// Returns the curly close character '}', which is the closing character of a map.
        /// </summary>
        public const char CurlyCloseCharacter = '}';

        /// <summary>
        /// Returns the curly open character '{', which is the opening character of a map.
        /// </summary>
        public const char CurlyOpenCharacter = '{';

        /// <summary>
        /// Returns the square bracket close character ']', which is the closing character of a list/array.
        /// </summary>
        public const char SquareBracketCloseCharacter = ']';

        /// <summary>
        /// Returns the square bracket open character '[', which is the opening character of a list/array.
        /// </summary>
        public const char SquareBracketOpenCharacter = '[';

        /// <summary>
        /// Returns the slash character '/', which combined with <see cref="SingleLineCommentStartSecondCharacter"/>
        /// or <see cref="MultiLineCommentStartSecondCharacter"/> starts a comment section.
        /// </summary>
        public const char CommentStartFirstCharacter = '/';

        /// <summary>
        /// Returns the slash character '/', which when following a <see cref="CommentStartFirstCharacter"/>
        /// starts a single line comment section.
        /// </summary>
        public const char SingleLineCommentStartSecondCharacter = '/';

        /// <summary>
        /// Returns the asterisk character '*', which when following a <see cref="CommentStartFirstCharacter"/>
        /// starts a multi line comment section.
        /// </summary>
        public const char MultiLineCommentStartSecondCharacter = '*';

        /// <summary>
        /// Returns the string of length 2 (&quot;//&quot;) which starts a single line comment.
        /// </summary>
        public static readonly string SingleLineCommentStart
            = new string(new[] { CommentStartFirstCharacter, SingleLineCommentStartSecondCharacter });
    }
}

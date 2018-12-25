#region License
/*********************************************************************************
 * TextErrorInfo.cs
 * 
 * Copyright (c) 2004-2018 Henk Nicolai
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
 *********************************************************************************/
#endregion

using System;

namespace SysExtensions.Text
{
    /// <summary>
    /// Reports an error at a certain location in a source text.
    /// </summary>
    public class TextErrorInfo
    {
        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the start position of the text span where the error occurred.
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// Gets the length of the text span where the error occurred.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="TextErrorInfo"/>.
        /// </summary>
        /// <param name="message">
        /// The error message.
        /// </param>
        /// <param name="start">
        /// The start position of the text span where the error occurred.
        /// </param>
        /// <param name="length">
        /// The length of the text span where the error occurred.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="message"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Either <paramref name="start"/> or <paramref name="length"/>, or both are negative.
        /// </exception>
        public TextErrorInfo(string message, int start, int length)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));

            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));

            Start = start;
            Length = length;
        }
    }
}

﻿#region License
/*********************************************************************************
 * PgnErrorInfo.cs
 *
 * Copyright (c) 2004-2019 Henk Nicolai
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

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Reports an error at a certain location in a source PGN.
    /// </summary>
    public class PgnErrorInfo : ISpan
    {
        /// <summary>
        /// Gets the error code.
        /// </summary>
        public PgnErrorCode ErrorCode { get; }

        /// <summary>
        /// Gets the start position of the text span where the error occurred.
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// Gets the length of the text span where the error occurred.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="PgnErrorInfo"/>.
        /// </summary>
        /// <param name="errorCode">
        /// The error code.
        /// </param>
        /// <param name="start">
        /// The start position of the text span where the error occurred.
        /// </param>
        /// <param name="length">
        /// The length of the text span where the error occurred.
        /// </param>
        public PgnErrorInfo(PgnErrorCode errorCode, int start, int length)
        {
            ErrorCode = errorCode;
            Start = start;
            Length = length;
        }
    }
}

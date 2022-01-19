#region License
/*********************************************************************************
 * JsonErrorInfo.cs
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

using System;
using System.Collections.Generic;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Reports an error at a certain location in a source text.
    /// </summary>
    public class JsonErrorInfo : ISpan
    {
        /// <summary>
        /// Gets the error code.
        /// </summary>
        public JsonErrorCode ErrorCode { get; }

        /// <summary>
        /// Gets the severity level of the error.
        /// </summary>
        public JsonErrorLevel ErrorLevel { get; }

        /// <summary>
        /// Gets the start position of the text span where the error occurred.
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// Gets the length of the text span where the error occurred.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets the list of parameters.
        /// </summary>
        public ReadOnlyList<JsonErrorInfoParameter> Parameters { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="JsonErrorInfo"/>.
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
        /// <param name="parameters">
        /// Parameters of the error.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Either <paramref name="start"/> or <paramref name="length"/>, or both are negative.
        /// </exception>
        public JsonErrorInfo(JsonErrorCode errorCode, int start, int length, params JsonErrorInfoParameter[] parameters)
            : this(errorCode, JsonErrorLevel.Error, start, length, parameters)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="JsonErrorInfo"/>.
        /// </summary>
        /// <param name="errorCode">
        /// The error code.
        /// </param>
        /// <param name="errorLevel">
        /// The severity level of the error.
        /// </param>
        /// <param name="start">
        /// The start position of the text span where the error occurred.
        /// </param>
        /// <param name="length">
        /// The length of the text span where the error occurred.
        /// </param>
        /// <param name="parameters">
        /// Parameters of the error.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Either <paramref name="start"/> or <paramref name="length"/>, or both are negative.
        /// </exception>
        public JsonErrorInfo(JsonErrorCode errorCode, JsonErrorLevel errorLevel, int start, int length, params JsonErrorInfoParameter[] parameters)
        {
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));

            ErrorCode = errorCode;
            ErrorLevel = errorLevel;
            Start = start;
            Length = length;
            Parameters = parameters != null
                ? ReadOnlyList<JsonErrorInfoParameter>.Create(parameters)
                : ReadOnlyList<JsonErrorInfoParameter>.Empty;
        }
    }
}

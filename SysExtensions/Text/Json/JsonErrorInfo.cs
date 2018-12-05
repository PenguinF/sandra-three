#region License
/*********************************************************************************
 * JsonErrorInfo.cs
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

namespace SysExtensions.Text.Json
{
    public class JsonErrorInfo : TextErrorInfo
    {
        /// <summary>
        /// Gets the error code.
        /// </summary>
        public JsonErrorCode ErrorCode { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="JsonErrorInfo"/>.
        /// </summary>
        /// <param name="errorCode">
        /// The error code.
        /// </param>
        /// <param name="message">
        /// The error message.
        /// </param>
        /// <param name="start">
        /// The start position of the text span where the error occurred.
        /// </param>
        /// <param name="length">
        /// The length of the text span where the error occurred.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="message"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Either <paramref name="start"/> or <paramref name="length"/>, or both are negative.
        /// </exception>
        public JsonErrorInfo(JsonErrorCode errorCode, string message, int start, int length)
            : base(message, start, length)
        {
            ErrorCode = errorCode;
        }
    }
}

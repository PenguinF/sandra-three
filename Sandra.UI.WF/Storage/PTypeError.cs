#region License
/*********************************************************************************
 * PTypeError.cs
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

using SysExtensions.Text.Json;
using System;

namespace Sandra.UI.WF.Storage
{
    /// <summary>
    /// Represents an error caused by a value being of a different type than expected.
    /// </summary>
    public class PTypeError : JsonErrorInfo
    {
        /// <summary>
        /// Gets the context insensitive information for this error message.
        /// </summary>
        public ITypeErrorBuilder TypeErrorBuilder { get; }

        private PTypeError(ITypeErrorBuilder typeErrorBuilder, int start, int length)
            : base(JsonErrorCode.Custom, start, length)
        {
            TypeErrorBuilder = typeErrorBuilder;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PTypeError"/>.
        /// </summary>
        /// <param name="typeErrorBuilder">
        /// The context insensitive information for this error message.
        /// </param>
        /// <returns>
        /// A <see cref="PTypeError"/> instance which generates a localized error message.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="typeErrorBuilder"/> is null.
        /// </exception>
        public static PTypeError Create(ITypeErrorBuilder typeErrorBuilder, int start, int length)
        {
            if (typeErrorBuilder == null) throw new ArgumentNullException(nameof(typeErrorBuilder));

            return new PTypeError(
                typeErrorBuilder,
                start,
                length);
        }
    }
}

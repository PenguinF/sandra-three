﻿#region License
/*********************************************************************************
 * ITypeErrorBuilder.cs
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

namespace Sandra.UI.WF.Storage
{
    /// <summary>
    /// Contains information to build an error message caused by a value being of a different type than expected.
    /// </summary>
    public interface ITypeErrorBuilder
    {
        /// <summary>
        /// Gets the localized, context sensitive message for this error.
        /// </summary>
        /// <param name="localizer">
        /// The localizer to use.
        /// </param>
        /// <param name="propertyKey">
        /// The property key for which the error occurred, or null if there was none.
        /// </param>
        /// <param name="valueString">
        /// A string representation of the value in the source code.
        /// </param>
        /// <returns>
        /// The localized error message.
        /// </returns>
        string GetLocalizedTypeErrorMessage(Localizer localizer, string propertyKey, string valueString);
    }
}
#region License
/*********************************************************************************
 * JsonErrorInfoExtensions.cs
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

using Eutherion.Localization;
using Eutherion.Text.Json;
using Eutherion.Win.Storage;

namespace Eutherion.Win.AppTemplate
{
    public static class JsonErrorInfoExtensions
    {
        public static LocalizedStringKey GetLocalizedStringKey(JsonErrorCode jsonErrorCode)
        {
            const string UnspecifiedMessage = "Unspecified error";

            if (jsonErrorCode == JsonErrorCode.Unspecified)
            {
                return LocalizedStringKey.Unlocalizable(UnspecifiedMessage);
            }
            else
            {
                return new LocalizedStringKey($"JsonError{jsonErrorCode}");
            }
        }

        /// <summary>
        /// Gets the formatted and localized error message of a <see cref="JsonErrorInfo"/>.
        /// </summary>
        public static string Message(this JsonErrorInfo jsonErrorInfo, Localizer localizer)
        {
            if (jsonErrorInfo is PTypeError typeError)
            {
                return typeError.GetLocalizedMessage(localizer);
            }

            return localizer.Localize(GetLocalizedStringKey(jsonErrorInfo.ErrorCode), jsonErrorInfo.Parameters);
        }
    }
}

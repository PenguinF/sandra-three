﻿#region License
/*********************************************************************************
 * PgnErrorInfoExtensions.cs
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

using Eutherion.Localization;
using Sandra.Chess.Pgn;
using System.Collections.Generic;

namespace Sandra.UI
{
    public static class PgnErrorInfoExtensions
    {
        public static LocalizedStringKey GetLocalizedStringKey(PgnErrorCode pgnErrorCode)
            => new LocalizedStringKey($"PgnError{pgnErrorCode}");

        /// <summary>
        /// Gets the formatted and localized error message of a <see cref="PgnErrorInfo"/>.
        /// </summary>
        public static string Message(this PgnErrorInfo pgnErrorInfo, Localizer localizer)
            => localizer.Localize(GetLocalizedStringKey(pgnErrorInfo.ErrorCode), pgnErrorInfo.Parameters);

        public static IEnumerable<KeyValuePair<LocalizedStringKey, string>> DefaultEnglishPgnErrorTranslations => new Dictionary<LocalizedStringKey, string>
        {
            { GetLocalizedStringKey(PgnErrorCode.IllegalCharacter), "illegal character '{0}'" },
        };
    }
}
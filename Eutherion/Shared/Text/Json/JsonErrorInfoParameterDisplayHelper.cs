﻿#region License
/*********************************************************************************
 * JsonErrorInfoParameterDisplayHelper.cs
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

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Contains helper functions to generate default localized display values for common types of <see cref="JsonErrorInfoParameter{ParameterType}"/>.
    /// All parameter types generated by <see cref="JsonParser"/> are supported.
    /// </summary>
    public static class JsonErrorInfoParameterDisplayHelper
    {
        /// <summary>
        /// Gets the default localization key for displaying null values.
        /// </summary>
        public static readonly StringKey<ForFormattedText> NullString
            = new StringKey<ForFormattedText>(nameof(NullString));

        /// <summary>
        /// Gets the default localization key for displaying a <see cref="JsonErrorInfoParameter"/> of an unknown type.
        /// It expects one parameter, which is filled with the ToString() value of the value object.
        /// </summary>
        public static readonly StringKey<ForFormattedText> UntypedObjectString
            = new StringKey<ForFormattedText>(nameof(UntypedObjectString));

        /// <summary>
        /// Gets a formatted and localized error message of a <see cref="JsonErrorInfoParameter"/>.
        /// </summary>
        /// <param name="parameter">
        /// The <see cref="JsonErrorInfoParameter"/> to localize.
        /// </param>
        /// <param name="localizer">
        /// The <see cref="TextFormatter"/> to use for generating a display value.
        /// </param>
        /// <returns>
        /// The localized display value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="parameter"/> and/or <paramref name="localizer"/> are null.
        /// </exception>
        public static string GetLocalizedDisplayValue(JsonErrorInfoParameter parameter, TextFormatter localizer)
        {
            if (parameter == null) throw new ArgumentNullException(nameof(parameter));
            if (localizer == null) throw new ArgumentNullException(nameof(localizer));

            switch (parameter)
            {
                case JsonErrorInfoParameter<char> charParameter:
                    char c = charParameter.Value;
                    return CStyleStringLiteral.CharacterMustBeEscaped(c)
                        ? $"'{CStyleStringLiteral.EscapedCharacterString(c)}'"
                        : $"'{c}'";
                case JsonErrorInfoParameter<string> stringParameter:
                    return stringParameter.Value == null
                        ? localizer.Format(NullString)
                        : $"\"{stringParameter.Value}\"";
                default:
                    return parameter.UntypedValue == null
                        ? localizer.Format(NullString)
                        : localizer.Format(UntypedObjectString, parameter.UntypedValue.ToString());
            }
        }
    }
}

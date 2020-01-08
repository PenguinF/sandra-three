#region License
/*********************************************************************************
 * StringUtilities.cs
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

using System;

namespace Eutherion.Utils
{
    /// <summary>
    /// Contains utility methods for strings.
    /// </summary>
    public static class StringUtilities
    {
        /// <summary>
        /// Conditionally formats a string, based on whether or not it has parameters.
        /// </summary>
        public static string ConditionalFormat(string localizedString, string[] parameters)
        {
            if (parameters == null || parameters.Length == 0) return localizedString;

            try
            {
                return string.Format(localizedString, parameters);
            }
            catch (FormatException)
            {
                // The provided localized format string is in an incorrect format, and/or it contains
                // more parameter substitution locations than there are parameters provided.
                // Examples:
                // string.Format("Test with parameters {invalid parameter}", parameters)
                // string.Format("Test with parameters {0}, {1} and {2}", new string[] { "0", "1" })
                return $"{localizedString} {ToDefaultParameterListDisplayString(parameters)}";
            }
        }

        /// <summary>
        /// Generates a display string from an array of parameters in the format "({0}, {1}, ...)".
        /// </summary>
        /// <param name="parameters">
        /// The array of parameters to format.
        /// </param>
        /// <returns>
        /// If <paramref name="parameters"/> is null or empty, returns an empty string.
        /// If <paramref name="parameters"/> has exactly one element, returns "({0})" where {0} is replaced by the single element.
        /// If <paramref name="parameters"/> has more than one element, returns "({0}, {1}, ...)" where {0}, {1}... are replaced by elements of the array.
        /// </returns>
        public static string ToDefaultParameterListDisplayString(string[] parameters)
            => parameters == null || parameters.Length == 0
            ? string.Empty
            : $"({string.Join(", ", parameters)})";
    }
}

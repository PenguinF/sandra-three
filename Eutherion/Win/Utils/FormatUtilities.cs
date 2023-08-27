#region License
/*********************************************************************************
 * FormatUtilities.cs
 *
 * Copyright (c) 2004-2023 Henk Nicolai
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
using System;

namespace Eutherion
{
    public static class FormatUtilities
    {
        /// <summary>
        /// Formats a string, replacing substitution markers in the format string with a string value from a specified array
        /// or the empty string if this parameter is unavailable.
        /// </summary>
        /// <param name="format">
        /// A composite format string.
        /// </param>
        /// <param name="args">
        /// A string array that contains zero or more objects to format.
        /// </param>
        /// <returns>
        /// A copy of <paramref name="format"/> in which substitution markers have been replaced by values from <paramref name="args"/>,
        /// -or- a default representation if <paramref name="format"/> is null or invalid and <seealso cref="string.Format(string, object[])"/>
        /// would throw.
        /// </returns>
        /// <remarks>
        /// This implementation differs from <seealso cref="string.Format(string, object[])"/> in the sense
        /// that if a substitution point indexes outside of the args array, it does not throw, but rather replaces with an empty string.
        /// If <paramref name="format"/> is in an incorrect format, it returns a default representation using <see cref="ToDefaultParameterListDisplayString(string[])"/>.
        /// </remarks>
        public static string SoftFormat(string format, params string[] args)
        {
            int requiredParameterCount = StringUtilities.FormatStringRequiredArgumentCount(format, out bool wouldThrowException);

            if (wouldThrowException)
            {
                // The provided format string is in an incorrect format.
                // Example:
                // string.Format("Test with parameters {invalid parameter}", parameters)
                return $"{format}{StringUtilities.ToDefaultParameterListDisplayString(args)}";
            }

            args = args ?? Array.Empty<string>();

            return string.Format(format, args.PadRight(requiredParameterCount, string.Empty));
        }
    }
}

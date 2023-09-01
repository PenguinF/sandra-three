#region License
/*********************************************************************************
 * SettingKey.cs
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

using System.Text;

namespace Eutherion.Win.Storage
{
    /// <summary>
    /// Contains a method to convert a Pascal case identifier to snake case.
    /// </summary>
    public static class SettingKey
    {
        /// <summary>
        /// Converts a Pascal case identifier to snake case.
        /// </summary>
        public static string ToSnakeCase(string pascalCaseIdentifier)
        {
            // Start with converting to lower case.
            StringBuilder snakeCase = new StringBuilder(pascalCaseIdentifier.ToLowerInvariant());

            // Start at the end so the loop index doesn't need an update after insertion of an underscore.
            // Stop at index 1, to prevent underscore before the first letter.
            for (int i = pascalCaseIdentifier.Length - 1; i > 0; --i)
            {
                // Insert underscores before letters that have changed case.
                if (pascalCaseIdentifier[i] != snakeCase[i])
                {
                    snakeCase.Insert(i, '_');
                }
            }

            return snakeCase.ToString();
        }

        /// <summary>
        /// Converts a Pascal case identifier to snake case for use as a key in a settings file.
        /// </summary>
        public static StringKey<SettingSchema.Member> ToSnakeCaseKey(string pascalCaseIdentifier)
            => new StringKey<SettingSchema.Member>(ToSnakeCase(pascalCaseIdentifier));
    }
}

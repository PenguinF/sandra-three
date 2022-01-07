#region License
/*********************************************************************************
 * JsonValue.cs
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
using System.Globalization;
using System.Numerics;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Helper class to generate literal value terminal symbols from source text.
    /// </summary>
    public static class JsonValue
    {
        /// <summary>
        /// Represents the length of the 'false' literal value in source text.
        /// </summary>
        public const int FalseSymbolLength = 5;

        /// <summary>
        /// Represents the length of the 'true' literal value in source text.
        /// </summary>
        public const int TrueSymbolLength = 4;

        /// <summary>
        /// Gets the representation of the 'false' literal value in source text.
        /// </summary>
        public static readonly string False = "false";

        /// <summary>
        /// Gets the representation of the 'true' literal value in source text.
        /// </summary>
        public static readonly string True = "true";

        /// <summary>
        /// Attempts to create a syntax node from a string value.
        /// </summary>
        /// <param name="value">
        /// The value from which to create a syntax node.
        /// </param>
        /// <returns>
        /// The created value, or null if the value was unrecognized.
        /// </returns>
        public static IGreenJsonSymbol TryCreate(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (value.Length <= 0) throw new ArgumentException($"{nameof(value)} is empty", nameof(value));

            if (value == False) return GreenJsonBooleanLiteralSyntax.False.Instance;
            if (value == True) return GreenJsonBooleanLiteralSyntax.True.Instance;

            if (BigInteger.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out BigInteger integerValue))
            {
                return new GreenJsonIntegerLiteralSyntax(integerValue, value.Length);
            }

            return null;
        }
    }
}

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
        public static IGreenJsonSymbol TryCreate(ReadOnlySpan<char> value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (value.Length <= 0) throw new ArgumentException($"{nameof(value)} is empty", nameof(value));

            char firstCharacter = value[0];
            if (firstCharacter == 'f')
            {
                if (False.AsSpan().SequenceEqual(value)) return GreenJsonBooleanLiteralSyntax.False.Instance;
                return null;
            }
            else if (firstCharacter == 't')
            {
                if (True.AsSpan().SequenceEqual(value)) return GreenJsonBooleanLiteralSyntax.True.Instance;
                return null;
            }

            // Avoid BigInteger if possible.
            // This is the maximum value which when multiplied by 10 is still 10 or more below ulong.MaxValue,
            // i.e. can take another digit with guarantee it will not overflow.
            const ulong maxUnsignedLongValue = ulong.MaxValue / 10 - 1;

            // Try to parse as an integer with a leading sign.
            int index = 0;
            bool minus = false;

            // Process or eat sign.
            if (firstCharacter == '-') { minus = true; index++; }
            else if (firstCharacter == '+') { index++; }

            ulong ulongValue = 0;

            while (index < value.Length)
            {
                int digit = value[index] - '0';
                if (digit >= 0 && digit <= 9)
                {
                    if (ulongValue <= maxUnsignedLongValue)
                    {
                        ulongValue = ulongValue * 10 + (uint)digit;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    // Only a number if all characters are digits (except perhaps for the leading sign).
                    return null;
                }

                index++;
            }

            // Convert to BigInteger, continue if there are more characters to parse.
            BigInteger integerValue = ulongValue;

            while (index < value.Length)
            {
                int digit = value[index] - '0';
                if (digit >= 0 && digit <= 9)
                {
                    integerValue = integerValue * 10 + digit;
                }
                else
                {
                    // Only a number if all characters are digits (except perhaps for the leading sign).
                    return null;
                }

                index++;
            }

            return new GreenJsonIntegerLiteralSyntax(minus ? -integerValue : integerValue, value.Length);
        }
    }
}

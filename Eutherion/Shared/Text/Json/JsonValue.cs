#region License
/*********************************************************************************
 * JsonValue.cs
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
using System.Globalization;
using System.Numerics;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Helper class to generate json literal value terminal symbols from source json.
    /// </summary>
    public static class JsonValue
    {
        public const int FalseSymbolLength = 5;
        public const int TrueSymbolLength = 4;

        public static readonly string False = "false";
        public static readonly string True = "true";

        public static IJsonValueStarterSymbol Create(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (value.Length <= 0) throw new ArgumentException(nameof(value));

            if (value == False) return GreenJsonBooleanLiteralSyntax.False.Instance;
            if (value == True) return GreenJsonBooleanLiteralSyntax.True.Instance;

            if (BigInteger.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out BigInteger integerValue))
            {
                return new GreenJsonIntegerLiteralSyntax(integerValue, value.Length);
            }

            return new GreenJsonUndefinedValueSyntax(value);
        }
    }
}

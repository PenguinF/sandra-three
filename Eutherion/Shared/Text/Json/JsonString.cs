#region License
/*********************************************************************************
 * JsonString.cs
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

using System;
using System.Runtime.CompilerServices;

namespace Eutherion.Text.Json
{
    public class JsonString : JsonSymbol
    {
        public const char QuoteCharacter = '"';
        public const char EscapeCharacter = '\\';

        /// <summary>
        /// Generates the escape sequence string for a character.
        /// </summary>
        public static string EscapedCharacterString(char c)
        {
            switch (c)
            {
                case '\0': return "\\0";
                case '\b': return "\\b";
                case '\f': return "\\f";
                case '\n': return "\\n";
                case '\r': return "\\r";
                case '\t': return "\\t";
                case '\v': return "\\v";
                case QuoteCharacter: return "\\\"";
                case EscapeCharacter: return "\\\\";
                default: return $"\\u{((int)c).ToString("x4")}";
            }
        }

        private const char HighestControlCharacter = '\u009f';
        private const int ControlCharacterIndexLength = HighestControlCharacter + 1;

        // An index in memory is as fast as it gets for determining whether or not a character should be escaped.
        public static readonly bool[] CharacterMustBeEscapedIndex;

        static JsonString()
        {
            // Will be initialized with all false values.
            CharacterMustBeEscapedIndex = new bool[ControlCharacterIndexLength];

            //https://www.compart.com/en/unicode/category/Cc
            for (int i = 0; i < ' '; i++) CharacterMustBeEscapedIndex[i] = true;
            for (int i = '\u007f'; i <= HighestControlCharacter; i++) CharacterMustBeEscapedIndex[i] = true;

            // Individual characters.
            CharacterMustBeEscapedIndex[QuoteCharacter] = true;
            CharacterMustBeEscapedIndex[EscapeCharacter] = true;
        }

        /// <summary>
        /// Returns whether or not a character must be escaped when in a JSON string.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CharacterMustBeEscaped(char c)
        {
            if (c < ControlCharacterIndexLength) return CharacterMustBeEscapedIndex[c];

            // Express this as two inequality conditions so second condition may not have to be evaluated.
            //https://www.compart.com/en/unicode/category/Zl - line separator
            //https://www.compart.com/en/unicode/category/Zp - paragraph separator
            return c >= '\u2028' && c <= '\u2029';
        }

        public string Value { get; }

        public override bool IsValueStartSymbol => true;
        public override int Length { get; }

        public JsonString(string value, int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            Length = length;
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitString(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitString(this);
        public override TResult Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitString(this, arg);
    }
}

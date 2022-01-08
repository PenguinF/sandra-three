#region License
/*********************************************************************************
 * CompactSettingWriter.cs
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

using Eutherion.Text;
using Eutherion.Text.Json;
using System;
using System.Text;

namespace Eutherion.Win.Storage
{
    /// <summary>
    /// Used by <see cref="SettingsAutoSave"/> to convert a <see cref="PMap"/> to its compact representation in JSON.
    /// </summary>
    internal class CompactSettingWriter : PValueVisitor
    {
        public static string ConvertToJson(PMap map)
        {
            var writer = new CompactSettingWriter();
            writer.Visit(map);
            return writer.outputBuilder.ToString();
        }

        protected readonly StringBuilder outputBuilder = new StringBuilder();
        protected int currentDepth;

        internal void AppendString(string value)
        {
            outputBuilder.Append(StringLiteral.QuoteCharacter);

            if (value != null)
            {
                // Save on Append() operations by appending escape-char-less substrings
                // that are as large as possible.
                int firstNonEscapedCharPosition = 0;

                for (int i = 0; i < value.Length; i++)
                {
                    char c = value[i];
                    if (StringLiteral.CharacterMustBeEscaped(c))
                    {
                        // Non-empty substring between this character and the last?
                        if (firstNonEscapedCharPosition < i)
                        {
                            outputBuilder.Append(
                                value,
                                firstNonEscapedCharPosition,
                                i - firstNonEscapedCharPosition);
                        }

                        // Append the escape sequence.
                        outputBuilder.Append(StringLiteral.EscapedCharacterString(c));

                        firstNonEscapedCharPosition = i + 1;
                    }
                }

                if (firstNonEscapedCharPosition < value.Length)
                {
                    outputBuilder.Append(
                        value,
                        firstNonEscapedCharPosition,
                        value.Length - firstNonEscapedCharPosition);
                }
            }

            outputBuilder.Append(StringLiteral.QuoteCharacter);
        }

        public override void VisitBoolean(PBoolean value)
            => outputBuilder.Append(JsonBooleanLiteralSyntax.BoolJsonLiteral(value.Value).LiteralJsonValue);

        public override void VisitInteger(PInteger value)
            => outputBuilder.Append(value.Value.ToStringInvariant());

        public override void VisitString(PString value)
            => AppendString(value.Value);

        public override void VisitList(PList value)
        {
            outputBuilder.Append(JsonSpecialCharacter.SquareBracketOpenCharacter);
            currentDepth++;

            bool first = true;
            foreach (var element in value)
            {
                if (first) first = false;
                else outputBuilder.Append(JsonSpecialCharacter.CommaCharacter);

                Visit(element);
            }

            currentDepth--;
            outputBuilder.Append(JsonSpecialCharacter.SquareBracketCloseCharacter);
        }

        public override void VisitMap(PMap value)
        {
            outputBuilder.Append(JsonSpecialCharacter.CurlyOpenCharacter);
            currentDepth++;

            bool first = true;
            foreach (var kv in value)
            {
                if (first) first = false;
                else outputBuilder.Append(JsonSpecialCharacter.CommaCharacter);

                AppendString(kv.Key);
                outputBuilder.Append(JsonSpecialCharacter.ColonCharacter);
                Visit(kv.Value);
            }

            currentDepth--;
            outputBuilder.Append(JsonSpecialCharacter.CurlyCloseCharacter);
        }
    }
}

#region License
/*********************************************************************************
 * CompactSettingWriter.cs
 * 
 * Copyright (c) 2004-2018 Henk Nicolai
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
 *********************************************************************************/
#endregion

using System.Globalization;
using System.Text;

namespace Sandra.UI.WF.Storage
{
    /// <summary>
    /// Used by <see cref="AutoSave"/> to convert a <see cref="PMap"/> to its compact representation in JSON.
    /// </summary>
    internal class CompactSettingWriter : PValueVisitor
    {
        private readonly StringBuilder outputBuilder = new StringBuilder();

        private void AppendString(string value)
        {
            outputBuilder.Append(JsonString.QuoteCharacter);

            if (value != null)
            {
                // Save on Append() operations by appending escape-char-less substrings
                // that are as large as possible.
                int firstNonEscapedCharPosition = 0;

                for (int i = 0; i < value.Length; i++)
                {
                    char c = value[i];
                    if (JsonString.CharacterMustBeEscaped(c))
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
                        outputBuilder.Append(JsonString.EscapedCharacterString(c));

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

            outputBuilder.Append(JsonString.QuoteCharacter);
        }

        public override void VisitBoolean(PBoolean value)
            => outputBuilder.Append(value.Value ? JsonValue.True : JsonValue.False);

        public override void VisitInteger(PInteger value)
            => outputBuilder.Append(value.Value.ToString(CultureInfo.InvariantCulture));

        public override void VisitString(PString value)
            => AppendString(value.Value);

        public override void VisitList(PList value)
        {
            outputBuilder.Append(JsonSquareBracketOpen.SquareBracketOpenCharacter);

            bool first = true;
            foreach (var element in value)
            {
                if (first) first = false;
                else outputBuilder.Append(JsonComma.CommaCharacter);

                Visit(element);
            }

            outputBuilder.Append(JsonSquareBracketClose.SquareBracketCloseCharacter);
        }

        public string Output() => outputBuilder.ToString();
    }
}

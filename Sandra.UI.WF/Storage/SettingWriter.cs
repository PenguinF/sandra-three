#region License
/*********************************************************************************
 * SettingWriter.cs
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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sandra.UI.WF.Storage
{
    /// <summary>
    /// Represents a single iteration of writing settings to a file.
    /// </summary>
    internal class SettingWriter : PValueVisitor
    {
        private const int maxLineLength = 80;

        private static IEnumerable<string> GetCommentLines(string commentText, int indent)
        {
            List<string> lines = new List<string>();
            if (commentText == null) return lines;

            // Cut up the description in pieces.
            // Available length depends on the current indent level.
            int availableLength = maxLineLength - indent - JsonComment.SingleLineCommentStart.Length;
            int totalLength = commentText.Length;
            int remainingLength = totalLength;
            int currentPos = 0;

            // Use a StringBuilder for substring generation.
            StringBuilder text = new StringBuilder(commentText);

            // Set currentPos to first non-whitespace character.
            while (currentPos < totalLength && char.IsWhiteSpace(text[currentPos]))
            {
                currentPos++;
                remainingLength--;
            }

            // Invariants:
            // 1) currentPos is between 0 and totalLength.
            // 2) currentPos is at a non-whitespace character, or equal to totalLength.
            // 3) currentPos + remainingLength == totalLength.
            while (remainingLength > availableLength)
            {
                // Search for the first whitespace character before the maximum break position.
                int breakPos = currentPos + availableLength;
                while (breakPos > currentPos && !char.IsWhiteSpace(text[breakPos])) breakPos--;

                if (breakPos == currentPos)
                {
                    // Word longer than availableLength, just snip it up midway.
                    breakPos = currentPos + availableLength;
                }
                else
                {
                    // Find last non-whitespace character before the found whitespace.
                    while (breakPos > currentPos && char.IsWhiteSpace(text[breakPos])) breakPos--;
                    // Increase by 1 again to end up on the first whitespace character after the last word.
                    breakPos++;
                }

                // Add line which neither starts nor ends with a whitespace character.
                lines.Add(text.ToString(currentPos, breakPos - currentPos));
                currentPos = breakPos;
                remainingLength = totalLength - currentPos;

                // Set currentPos to first non-whitespace character again.
                while (currentPos < totalLength && char.IsWhiteSpace(text[currentPos]))
                {
                    currentPos++;
                    remainingLength--;
                }
            }

            if (remainingLength > 0)
            {
                lines.Add(text.ToString(currentPos, remainingLength));
            }

            return lines;
        }

        private static List<string> GetCommentLines(SettingComment comment, int indent)
        {
            List<string> lines = new List<string>();
            if (comment != null)
            {
                bool first = true;
                foreach (var paragraph in comment.Paragraphs)
                {
                    if (!first) lines.Add(string.Empty);
                    first = false;

                    // Add one extra indent because of the space between '//' and the text.
                    lines.AddRange(GetCommentLines(paragraph, indent + 1));
                }
            }
            return lines;
        }

        private List<string> GetCommentLines(SettingComment comment)
        {
            int indent = currentDepth * jsonTextWriter.Indentation;
            return GetCommentLines(comment, indent);
        }

        private class JsonPrettyPrinter : JsonTextWriter
        {
            internal bool commentOutNextToken;
            internal bool suppressNextValueDelimiter;

            public JsonPrettyPrinter(TextWriter writer) : base(writer)
            {
                Formatting = Formatting.Indented;
            }

            protected override void WriteValueDelimiter()
            {
                if (suppressNextValueDelimiter) suppressNextValueDelimiter = false;
                else base.WriteValueDelimiter();
            }

            protected override void WriteIndent()
            {
                base.WriteIndent();

                if (commentOutNextToken)
                {
                    WriteRaw(JsonComment.SingleLineCommentStart);
                }
            }

            public void _WriteIndent() => WriteIndent();
            public void _WriteValueDelimiter() => WriteValueDelimiter();
        }

        private readonly SettingSchema schema;
        private readonly StringBuilder outputBuilder;
        private readonly JsonPrettyPrinter jsonTextWriter;

        private readonly string newLine;
        private readonly bool commentOutProperties;
        private int currentDepth;

        public SettingWriter(SettingSchema schema, bool commentOutProperties)
        {
            outputBuilder = new StringBuilder();
            var stringWriter = new StringWriter(outputBuilder);
            stringWriter.NewLine = Environment.NewLine;

            this.schema = schema;
            this.commentOutProperties = commentOutProperties;
            newLine = stringWriter.NewLine;
            jsonTextWriter = new JsonPrettyPrinter(stringWriter);

            // Write schema description, if any.
            foreach (string commentLine in GetCommentLines(schema.Description))
            {
                // The base WriteComment wraps comments in /*-*/ delimiters,
                // so generate raw comments starting with // instead.
                jsonTextWriter.WriteRaw(JsonComment.SingleLineCommentStart);
                jsonTextWriter.WriteRaw(" ");
                jsonTextWriter.WriteRaw(commentLine);

                // Do this at the end to generate a line-break.
                jsonTextWriter._WriteIndent();
            }
        }

        public void WriteSettingPropertyName(string name, bool isFirst)
        {
            SettingProperty property;
            if (schema.TryGetProperty(new SettingKey(name), out property))
            {
                var commentLines = GetCommentLines(property.Description);

                // Only do the custom formatting when there are comments to write.
                if (commentLines.Any())
                {
                    // Prepare by doing a manual auto-completion of a previous value.
                    if (!isFirst)
                    {
                        // Write the value delimiter here already, and suppress it the next time it's called.
                        // This happens in WritePropertyName().
                        jsonTextWriter._WriteValueDelimiter();
                        jsonTextWriter.suppressNextValueDelimiter = true;
                        jsonTextWriter.WriteWhitespace(newLine);
                    }

                    foreach (string commentLine in commentLines)
                    {
                        jsonTextWriter._WriteIndent();
                        // The base WriteComment wraps comments in /*-*/ delimiters,
                        // so generate raw comments starting with // instead.
                        jsonTextWriter.WriteRaw(JsonComment.SingleLineCommentStart);
                        jsonTextWriter.WriteRaw(" ");
                        jsonTextWriter.WriteRaw(commentLine);
                    }
                }
            }

            // This assumes that all default setting values fit on one line.
            jsonTextWriter.commentOutNextToken = commentOutProperties;
            jsonTextWriter.WritePropertyName(name);
            jsonTextWriter.commentOutNextToken = false;
        }

        public override void VisitBoolean(PBoolean value)
        {
            jsonTextWriter.WriteValue(value.Value);
        }

        public override void VisitInteger(PInteger value)
        {
            jsonTextWriter.WriteValue(value.Value);
        }

        public override void VisitList(PList value)
        {
            jsonTextWriter.WriteStartArray();
            currentDepth++;

            foreach (var element in value)
            {
                Visit(element);
            }

            currentDepth--;
            jsonTextWriter.WriteEndArray();
        }

        public override void VisitMap(PMap value)
        {
            jsonTextWriter.WriteStartObject();
            currentDepth++;

            bool first = true;
            foreach (var kv in value)
            {
                WriteSettingPropertyName(kv.Key, first);
                first = false;
                Visit(kv.Value);
            }

            currentDepth--;
            jsonTextWriter.WriteEndObject();
        }

        public override void VisitString(PString value)
        {
            jsonTextWriter.WriteValue(value.Value);
        }

        /// <summary>
        /// Closes the <see cref="SettingWriter"/> and returns the output.
        /// </summary>
        /// <returns>
        /// The generated output.
        /// </returns>
        public string Output()
        {
            // End files with a newline character.
            jsonTextWriter.WriteWhitespace(newLine);
            jsonTextWriter.Close();
            return outputBuilder.ToString();
        }
    }
}

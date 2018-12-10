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

using SysExtensions.Text.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandra.UI.WF.Storage
{
    /// <summary>
    /// Represents a single iteration of writing settings to a file.
    /// </summary>
    internal class SettingWriter : CompactSettingWriter
    {
        public const int Indentation = 2;
        public const char SpaceChar = ' ';

        private const int maxLineLength = 80;

        public static string ConvertToJson(PMap map, SettingSchema schema, SettingWriterOptions options)
        {
            SettingWriter writer = new SettingWriter(schema, options);
            writer.Visit(map);

            // End files with a newline character.
            writer.outputBuilder.AppendLine();
            return writer.outputBuilder.ToString();
        }

        private static IEnumerable<string> GetCommentLines(string commentText, int availableWidth)
        {
            List<string> lines = new List<string>();
            if (commentText == null) return lines;

            // Cut up the description in pieces.
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
            while (remainingLength > availableWidth)
            {
                // Search for the first whitespace character before the maximum break position.
                int breakPos = currentPos + availableWidth;
                while (breakPos > currentPos && !char.IsWhiteSpace(text[breakPos])) breakPos--;

                if (breakPos == currentPos)
                {
                    // Word longer than availableLength, just snip it up midway.
                    breakPos = currentPos + availableWidth;
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

        private readonly SettingSchema schema;

        private readonly SettingWriterOptions options;

        public SettingWriter(SettingSchema schema, SettingWriterOptions options)
        {
            this.schema = schema;
            this.options = options;

            // Write schema description, if any.
            AppendCommentLines(schema.Description);
        }

        private void AppendIndent()
        {
            outputBuilder.Append(SpaceChar, currentDepth * Indentation);
        }

        private void AppendCommentLines(SettingComment comment)
        {
            if (comment != null)
            {
                // Available width for comments depends on the current indent level.
                // Decrease by an extra 1 because of the space between '//' and the text.
                int availableCommentWidth = maxLineLength
                                          - currentDepth * Indentation
                                          - JsonComment.SingleLineCommentStart.Length
                                          - 1;

                bool first = true;
                foreach (var paragraph in comment.Paragraphs)
                {
                    if (!first)
                    {
                        // Extra line with empty single line comment to separate paragraphs.
                        AppendIndent();
                        outputBuilder.Append(JsonComment.SingleLineCommentStart);
                        outputBuilder.AppendLine();
                    }
                    else first = false;

                    foreach (string commentLine in GetCommentLines(paragraph, availableCommentWidth))
                    {
                        AppendIndent();
                        outputBuilder.Append(JsonComment.SingleLineCommentStart);
                        outputBuilder.Append(SpaceChar);
                        outputBuilder.Append(commentLine);
                        outputBuilder.AppendLine();
                    }
                }
            }
        }

        public override void VisitMap(PMap value)
        {
            outputBuilder.Append(JsonCurlyOpen.CurlyOpenCharacter);
            currentDepth++;

            bool first = true;
            foreach (var kv in value)
            {
                bool extraNewLineBeforeComment = !first;

                if (!first)
                {
                    outputBuilder.Append(JsonComma.CommaCharacter);
                }
                else
                {
                    first = false;
                }

                outputBuilder.AppendLine();

                string name = kv.Key;
                if ((options & SettingWriterOptions.SuppressSettingComments) == 0
                    && schema.TryGetProperty(new SettingKey(name), out SettingProperty property))
                {
                    if (extraNewLineBeforeComment) outputBuilder.AppendLine();
                    AppendCommentLines(property.Description);
                }

                // This assumes that all default setting values fit on one line.
                AppendIndent();
                if ((options & SettingWriterOptions.CommentOutProperties) != 0)
                {
                    outputBuilder.Append(JsonComment.SingleLineCommentStart);
                }

                AppendString(name);
                outputBuilder.Append(JsonColon.ColonCharacter);
                outputBuilder.Append(SpaceChar);
                Visit(kv.Value);
            }

            currentDepth--;

            if (!first)
            {
                // Do this after decreasing currentDepth, so the closing bracket
                // is in the same x-position as the opening bracket.
                outputBuilder.AppendLine();
                AppendIndent();
            }

            outputBuilder.Append(JsonCurlyClose.CurlyCloseCharacter);
        }
    }

    [Flags]
    public enum SettingWriterOptions
    {
        Default,

        /// <summary>
        /// Generates all properties but comments them out.
        /// </summary>
        CommentOutProperties = 1,

        /// <summary>
        /// Suppresses generation of comments before setting properties.
        /// </summary>
        SuppressSettingComments = 2,
    }
}

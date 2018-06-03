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
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents a single iteration of writing settings to a file.
    /// </summary>
    internal class SettingWriter : PValueVisitor
    {
        private class CustomJsonTextWriter : JsonTextWriter
        {
            public CustomJsonTextWriter(TextWriter writer) : base(writer)
            {
            }
        }

        private readonly StringBuilder outputBuilder;
        private readonly CustomJsonTextWriter jsonTextWriter;

        public SettingWriter(bool indented)
        {
            outputBuilder = new StringBuilder();
            var stringWriter = new StringWriter(outputBuilder);
            stringWriter.NewLine = Environment.NewLine;
            jsonTextWriter = new CustomJsonTextWriter(stringWriter);

            if (indented)
            {
                jsonTextWriter.Formatting = Formatting.Indented;
                jsonTextWriter.Indentation = 2;
                jsonTextWriter.IndentChar = ' ';
            }
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
            value.ForEach(Visit);
            jsonTextWriter.WriteEndArray();
        }

        public override void VisitMap(PMap value)
        {
            jsonTextWriter.WriteStartObject();
            foreach (var kv in value)
            {
                jsonTextWriter.WritePropertyName(kv.Key);
                Visit(kv.Value);
            }
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
            // If pretty printing, end files with a newline character.
            if (jsonTextWriter.Formatting == Formatting.Indented)
            {
                jsonTextWriter.WriteWhitespace(Environment.NewLine);
            }

            jsonTextWriter.Close();
            return outputBuilder.ToString();
        }
    }
}

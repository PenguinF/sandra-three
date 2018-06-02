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
        private readonly StringBuilder outputBuilder;
        private readonly JsonTextWriter jsonTextWriter;

        public SettingWriter(bool indented)
        {
            outputBuilder = new StringBuilder();
            jsonTextWriter = new JsonTextWriter(new StringWriter(outputBuilder));

            if (indented)
            {
                jsonTextWriter.Formatting = Formatting.Indented;
                jsonTextWriter.Indentation = 1;
                jsonTextWriter.IndentChar = '\t';
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

        public void WriteToFile(FileStream outputStream, Encoder encoder, char[] buffer, byte[] encodedBuffer)
        {
            jsonTextWriter.Close();
            string output = outputBuilder.ToString();

            // How much of the output still needs to be written.
            int remainingLength = output.Length;

            // Number of characters already written from output. Loop invariant therefore is:
            // charactersCopied + remainingLength == output.Length.
            int charactersCopied = 0;

            // Fill up the character buffer before doing any writing.
            for (;;)
            {
                // Determine number of characters to write.
                // AutoSave.CharBufferSize is known to be equal to buffer.Length.
                int charWriteCount = AutoSave.CharBufferSize;

                // Remember if this fill up the entire buffer.
                bool bufferFull = charWriteCount <= remainingLength;
                if (!bufferFull) charWriteCount = remainingLength;

                // Now copy to the character buffer after checking its range.
                output.CopyTo(charactersCopied, buffer, 0, charWriteCount);

                // If the buffer is full, call the encoder to convert it into bytes.
                if (bufferFull)
                {
                    int bytes = encoder.GetBytes(buffer, 0, AutoSave.CharBufferSize, encodedBuffer, 0, false);
                    outputStream.Write(encodedBuffer, 0, bytes);
                }

                // Update loop variables.
                charactersCopied += charWriteCount;
                remainingLength -= charWriteCount;

                if (remainingLength == 0)
                {
                    // Process what's left in the buffer and Encoder.
                    int bytes = encoder.GetBytes(buffer, 0, bufferFull ? 0 : charWriteCount, encodedBuffer, 0, true);
                    if (bytes > 0)
                    {
                        outputStream.Write(encodedBuffer, 0, bytes);
                    }

                    // Make sure everything is written to the file.
                    outputStream.Flush();
                    return;
                }
            }
        }
    }
}

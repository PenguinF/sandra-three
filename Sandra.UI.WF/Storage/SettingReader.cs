#region License
/*********************************************************************************
 * SettingReader.cs
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
using System.Numerics;

namespace Sandra.UI.WF.Storage
{
    /// <summary>
    /// Temporary class which parses a list of <see cref="JsonTerminalSymbol"/>s directly into a <see cref="PValue"/> result.
    /// </summary>
    public class TempJsonParser
    {
        private readonly JsonTextReader jsonTextReader;

        private readonly List<JsonTerminalSymbol> tokens;

        public IReadOnlyList<JsonTerminalSymbol> Tokens => tokens.AsReadOnly();

        public TempJsonParser(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            jsonTextReader = new JsonTextReader(new StringReader(json));
            tokens = new JsonTokenizer(json).TokenizeAll().ToList();
        }

        private Exception TokenTypeNotSupported(JsonToken jsonToken)
            => new JsonReaderException($"Token type {jsonToken} is not supported for settings.");

        private JsonToken ReadSkipComments()
        {
            // Skip comments until encountering something meaningful.
            while (jsonTextReader.Read() && jsonTextReader.TokenType == JsonToken.Comment) ;
            return jsonTextReader.TokenType;
        }

        private PMap ParseMap()
        {
            Dictionary<string, PValue> mapBuilder = new Dictionary<string, PValue>();

            for (;;)
            {
                var tokenType = ReadSkipComments();
                if (tokenType == JsonToken.EndObject)
                {
                    return new PMap(mapBuilder);
                }

                if (tokenType != JsonToken.PropertyName)
                {
                    throw new JsonReaderException("PropertyName or EndObject '}' expected");
                }

                string key = (string)jsonTextReader.Value;

                // Expect unique keys.
                if (mapBuilder.ContainsKey(key))
                {
                    throw new JsonReaderException($"Non-unique key in object: {key}");
                }

                mapBuilder.Add(key, ParseValue(ReadSkipComments()));
            }
        }

        private PList ParseList()
        {
            List<PValue> listBuilder = new List<PValue>();

            for (;;)
            {
                var tokenType = ReadSkipComments();
                if (tokenType == JsonToken.EndArray)
                {
                    return new PList(listBuilder);
                }

                listBuilder.Add(ParseValue(tokenType));
            }
        }

        private PValue ParseValue(JsonToken currentTokenType)
        {
            switch (currentTokenType)
            {
                case JsonToken.Boolean:
                    return new PBoolean((bool)jsonTextReader.Value);

                case JsonToken.Integer:
                    return jsonTextReader.Value is BigInteger
                        ? new PInteger((BigInteger)jsonTextReader.Value)
                        : new PInteger((long)jsonTextReader.Value); ;

                case JsonToken.String:
                    return new PString((string)jsonTextReader.Value);

                case JsonToken.StartObject:
                    return ParseMap();

                case JsonToken.StartArray:
                    return ParseList();

                case JsonToken.None:
                    throw new JsonReaderException("Unexpected end of file");

                case JsonToken.Comment:
                case JsonToken.EndArray:
                case JsonToken.EndObject:
                case JsonToken.PropertyName:
                    throw new JsonReaderException("'{', '[', Boolean, Integer or String expected");

                case JsonToken.Bytes:
                case JsonToken.Date:
                case JsonToken.EndConstructor:
                case JsonToken.Float:
                case JsonToken.Null:
                case JsonToken.Raw:
                case JsonToken.StartConstructor:
                case JsonToken.Undefined:
                default:
                    throw TokenTypeNotSupported(currentTokenType);
            }
        }

        public bool TryParse(out PMap map, out List<TextErrorInfo> errors)
        {
            errors = new List<TextErrorInfo>();

            try
            {
                var tokenType = ReadSkipComments();
                if (tokenType != JsonToken.None)
                {
                    PValue rootValue = ParseValue(tokenType);

                    if (ReadSkipComments() != JsonToken.None)
                    {
                        throw new JsonReaderException("End of file expected");
                    }

                    if (!PType.Map.TryGetValidValue(rootValue, out map))
                    {
                        throw new JsonReaderException("Expected json object at root");
                    }

                    return true;
                }

                map = default(PMap);
                return false;
            }
            catch (JsonReaderException exception)
            {
                errors.Add(new TextErrorInfo(exception.Message, 0, 0));
                map = default(PMap);
                return false;
            }
        }
    }

    /// <summary>
    /// Represents a single iteration of loading settings from a file.
    /// </summary>
    internal class SettingReader
    {
        private readonly TempJsonParser parser;

        public SettingReader(string json)
        {
            parser = new TempJsonParser(json);
        }

        public void ReadWorkingCopy(SettingCopy workingCopy)
        {
            PMap map;
            List<TextErrorInfo> errors;

            if (parser.TryParse(out map, out errors))
            {
                foreach (var kv in map)
                {
                    SettingProperty property;
                    if (workingCopy.Schema.TryGetProperty(new SettingKey(kv.Key), out property))
                    {
                        workingCopy.AddOrReplaceRaw(property, kv.Value);
                    }
                }
            }

            if (errors.Count > 0)
            {
                throw new JsonReaderException(errors[0].Message);
            }
        }
    }
}

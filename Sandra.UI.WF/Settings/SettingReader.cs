﻿/*********************************************************************************
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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents a single iteration of loading settings from a file.
    /// </summary>
    internal class SettingReader
    {
        private readonly JsonTextReader jsonTextReader;

        public SettingReader(TextReader inputReader)
        {
            if (inputReader == null) throw new ArgumentNullException(nameof(inputReader));
            jsonTextReader = new JsonTextReader(inputReader);
        }

        private Exception TokenTypeNotSupported(JsonToken jsonToken)
            => new JsonReaderException($"Token type {jsonToken} is not supported for settings.");

        private JsonToken ReadSkipComments()
        {
            // Skip comments until encountering something meaningful.
            do jsonTextReader.Read(); while (jsonTextReader.TokenType == JsonToken.Comment);
            return jsonTextReader.TokenType;
        }

        public bool TryParseValue(out PValue value)
        {
            var tokenType = ReadSkipComments();
            if (tokenType == JsonToken.None)
            {
                value = default(PValue);
                return false;
            }

            value = ParseValue(tokenType);

            if (ReadSkipComments() != JsonToken.None)
            {
                throw new JsonReaderException("End of file expected");
            }

            return true;
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

        public SettingCopy ReadWorkingCopy()
        {
            // Load into an empty working copy.
            var workingCopy = new SettingCopy();

            PValue rootValue;
            if (TryParseValue(out rootValue))
            {
                if (!(rootValue is PMap))
                {
                    throw new JsonReaderException("Expected json object at root");
                }

                PMap map = (PMap)rootValue;
                foreach (var kv in map)
                {
                    workingCopy.KeyValueMapping[new SettingKey(kv.Key)] = kv.Value;
                }
            }

            return workingCopy;
        }
    }
}

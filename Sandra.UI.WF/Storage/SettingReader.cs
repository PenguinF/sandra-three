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
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace Sandra.UI.WF.Storage
{
    /// <summary>
    /// Temporary class which parses a list of <see cref="JsonTerminalSymbol"/>s directly into a <see cref="PValue"/> result.
    /// </summary>
    public class TempJsonParser
    {
        private class ParseRun : JsonTerminalSymbolVisitor<PValue>
        {
            private const string UnrecognizedValueMessage = "Unrecognized value '{0}'";
            private const string NoPMapMessage = "Expected json object at root";
            private const string FileShouldHaveEndedAlreadyMessage = "End of file expected";

            private readonly List<JsonTerminalSymbol> tokens;

            public readonly List<TextErrorInfo> Errors = new List<TextErrorInfo>();

            private int currentTokenIndex;

            public ParseRun(List<JsonTerminalSymbol> tokens)
            {
                this.tokens = tokens;
                currentTokenIndex = 0;
            }

            private JsonTerminalSymbol ReadSkipComments()
            {
                // Skip comments until encountering something meaningful.
                while (currentTokenIndex < tokens.Count)
                {
                    JsonTerminalSymbol current = tokens[currentTokenIndex];
                    Errors.AddRange(current.Errors);
                    currentTokenIndex++;
                    if (!current.IsBackground) return current;
                }
                return null;
            }

            private PMap ParseMap()
            {
                Dictionary<string, PValue> mapBuilder = new Dictionary<string, PValue>();

                JsonTerminalSymbol symbol = ReadSkipComments();
                if (symbol is JsonCurlyClose)
                {
                    return new PMap(mapBuilder);
                }

                for (;;)
                {
                    if (!(symbol is JsonString))
                    {
                        throw new JsonReaderException("PropertyName or EndObject '}' expected");
                    }

                    string key = ((JsonString)symbol).Value;

                    // Expect unique keys.
                    if (mapBuilder.ContainsKey(key))
                    {
                        throw new JsonReaderException($"Non-unique key in object: {key}");
                    }

                    symbol = ReadSkipComments();
                    if (!(symbol is JsonColon))
                    {
                        throw new JsonReaderException("Colon ':' expected");
                    }

                    symbol = ReadSkipComments();
                    if (symbol == null)
                    {
                        throw new JsonReaderException("Unexpected end of file");
                    }

                    mapBuilder.Add(key, ParseValue(symbol));

                    symbol = ReadSkipComments();
                    if (symbol is JsonCurlyClose)
                    {
                        return new PMap(mapBuilder);
                    }
                    else if (!(symbol is JsonComma))
                    {
                        throw new JsonReaderException("Comma ',' or EndObject '}' expected");
                    }

                    symbol = ReadSkipComments();
                }
            }

            private PList ParseList()
            {
                List<PValue> listBuilder = new List<PValue>();

                JsonTerminalSymbol symbol = ReadSkipComments();
                if (symbol is JsonSquareBracketClose)
                {
                    return new PList(listBuilder);
                }

                for (;;)
                {
                    if (symbol == null)
                    {
                        throw new JsonReaderException("Unexpected end of file");
                    }

                    listBuilder.Add(ParseValue(symbol));

                    symbol = ReadSkipComments();
                    if (symbol is JsonSquareBracketClose)
                    {
                        return new PList(listBuilder);
                    }
                    else if (!(symbol is JsonComma))
                    {
                        throw new JsonReaderException("Comma ',' or EndArray ']' expected");
                    }

                    symbol = ReadSkipComments();
                }
            }

            private PValue ParseValue(JsonTerminalSymbol symbol)
            {
                if (symbol is JsonValue)
                {
                    string value = symbol.GetText();
                    if (value == "true") return new PBoolean(true);
                    if (value == "false") return new PBoolean(false);

                    BigInteger integerValue;
                    if (BigInteger.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out integerValue))
                    {
                        return new PInteger(integerValue);
                    }

                    Errors.Add(new TextErrorInfo(string.Format(UnrecognizedValueMessage, value), symbol.Start, symbol.Length));
                    return PUndefined.Value;
                }

                if (symbol is JsonString)
                {
                    return new PString(((JsonString)symbol).Value);
                }

                if (symbol is JsonCurlyOpen)
                {
                    return ParseMap();
                }

                if (symbol is JsonSquareBracketOpen)
                {
                    return ParseList();
                }

                throw new JsonReaderException("'{', '[', Boolean, Integer or String expected");
            }

            public bool TryParse(out PMap map)
            {
                try
                {
                    JsonTerminalSymbol symbol = ReadSkipComments();
                    if (symbol != null)
                    {
                        PValue rootValue = ParseValue(symbol);

                        JsonTerminalSymbol extraSymbol = ReadSkipComments();
                        if (extraSymbol != null)
                        {
                            Errors.Add(new TextErrorInfo(FileShouldHaveEndedAlreadyMessage, extraSymbol.Start, extraSymbol.Length));
                        }

                        bool validMap = PType.Map.TryGetValidValue(rootValue, out map);
                        if (!validMap)
                        {
                            Errors.Add(new TextErrorInfo(NoPMapMessage, symbol.Start, symbol.Length));
                        }

                        return validMap;
                    }

                    map = default(PMap);
                    return false;
                }
                catch (JsonReaderException exception)
                {
                    Errors.Add(new TextErrorInfo(exception.Message, 0, 0));
                    map = default(PMap);
                    return false;
                }
            }
        }

        private readonly List<JsonTerminalSymbol> tokens;

        public IReadOnlyList<JsonTerminalSymbol> Tokens => tokens.AsReadOnly();

        public TempJsonParser(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            tokens = new JsonTokenizer(json).TokenizeAll().ToList();
        }

        public bool TryParse(out PMap map, out List<TextErrorInfo> errors)
        {
            ParseRun parseRun = new ParseRun(tokens);
            var validMap = parseRun.TryParse(out map);
            errors = parseRun.Errors;
            return validMap;
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

        public List<TextErrorInfo> ReadWorkingCopy(SettingCopy workingCopy)
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

            return errors;
        }
    }
}

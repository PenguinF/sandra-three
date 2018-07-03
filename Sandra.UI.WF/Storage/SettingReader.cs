﻿#region License
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
            private const string EmptyValueMessage = "Missing value";
            private const string MultipleValuesMessage = "',' expected";
            private const string EofInArrayMessage = "Unexpected end of file, expected ']'";
            private const string ControlSymbolInArrayMessage = "']' expected";
            private const string UnrecognizedValueMessage = "Unrecognized value '{0}'";
            private const string NoPMapMessage = "Expected json object at root";
            private const string FileShouldHaveEndedAlreadyMessage = "End of file expected";

            private readonly List<JsonTerminalSymbol> tokens;
            private readonly int sourceLength;

            public readonly List<TextErrorInfo> Errors = new List<TextErrorInfo>();

            private int currentTokenIndex;

            public ParseRun(List<JsonTerminalSymbol> tokens, int sourceLength)
            {
                this.tokens = tokens;
                this.sourceLength = sourceLength;
                currentTokenIndex = 0;
            }

            private JsonTerminalSymbol PeekSkipComments()
            {
                // Skip comments until encountering something meaningful.
                while (currentTokenIndex < tokens.Count)
                {
                    JsonTerminalSymbol current = tokens[currentTokenIndex];
                    if (!current.IsBackground) return current;
                    Errors.AddRange(current.Errors);
                    currentTokenIndex++;
                }
                return null;
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

            public override PValue VisitCurlyOpen(JsonCurlyOpen curlyOpen)
            {
                Dictionary<string, PValue> mapBuilder = new Dictionary<string, PValue>();

                // Maintain a separate set of keys to aid error reporting on duplicate keys.
                HashSet<string> foundKeys = new HashSet<string>();

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

                    string propertyKey = ((JsonString)symbol).Value;

                    // Expect unique keys.
                    if (foundKeys.Contains(propertyKey))
                    {
                        throw new JsonReaderException($"Non-unique key in object: {propertyKey}");
                    }

                    foundKeys.Add(propertyKey);

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

                    mapBuilder.Add(propertyKey, ParseValue(symbol));

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

            public override PValue VisitSquareBracketOpen(JsonSquareBracketOpen bracketOpen)
            {
                List<PValue> listBuilder = new List<PValue>();

                for (;;)
                {
                    PValue parsedValue;
                    JsonTerminalSymbol firstSymbol;

                    bool gotValue = ParseMultiValue(MultipleValuesMessage, out parsedValue, out firstSymbol);
                    if (gotValue) listBuilder.Add(parsedValue);

                    // ParseMultiValue() guarantees that the next symbol is never a ValueStartSymbol.
                    JsonTerminalSymbol symbol = ReadSkipComments();
                    if (symbol is JsonComma)
                    {
                        if (!gotValue)
                        {
                            // Two commas or '[,': add an empty PErrorValue.
                            Errors.Add(new TextErrorInfo(EmptyValueMessage, symbol.Start, symbol.Length));
                            listBuilder.Add(PUndefined.Value);
                        }
                    }
                    else
                    {
                        // Assume missing closing bracket ']' on EOF or control symbol.
                        if (symbol == null)
                        {
                            Errors.Add(new TextErrorInfo(EofInArrayMessage, sourceLength - 1, 1));
                        }
                        else if (!(symbol is JsonSquareBracketClose))
                        {
                            Errors.Add(new TextErrorInfo(ControlSymbolInArrayMessage, symbol.Start, symbol.Length));
                        }

                        return new PList(listBuilder);
                    }
                }
            }

            public override PValue VisitValue(JsonValue symbol)
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

            public override PValue VisitString(JsonString symbol) => new PString(symbol.Value);

            private PValue ParseValue(JsonTerminalSymbol symbol)
            {
                if (!symbol.IsValueStartSymbol)
                {
                    throw new JsonReaderException("'{', '[', Boolean, Integer or String expected");
                }
                if (symbol.Errors.Any())
                {
                    return PUndefined.Value;
                }
                return Visit(symbol);
            }

            private bool ParseMultiValue(string multipleValuesMessage,
                                         out PValue firstValue,
                                         out JsonTerminalSymbol firstValueSymbol)
            {
                firstValue = default(PValue);
                firstValueSymbol = default(JsonTerminalSymbol);

                JsonTerminalSymbol symbol = PeekSkipComments();
                if (symbol == null || !symbol.IsValueStartSymbol) return false;

                firstValueSymbol = symbol;
                bool hasValue = false;

                for (;;)
                {
                    // Read the same symbol again but now eat it.
                    symbol = ReadSkipComments();

                    if (!hasValue)
                    {
                        if (symbol.Errors.Any()) firstValue = PUndefined.Value;
                        else firstValue = Visit(symbol);
                        hasValue = true;
                    }
                    else if (!symbol.Errors.Any())
                    {
                        // Make sure consecutive symbols are parsed as if they were valid.
                        // Discard the result.
                        Visit(symbol);
                    }

                    // Peek at the next symbol.
                    // If IsValueStartSymbol == false in the first iteration, it means that exactly one value was parsed, as desired.
                    symbol = PeekSkipComments();
                    if (symbol == null || !symbol.IsValueStartSymbol) return true;

                    // Two or more consecutive values not allowed.
                    Errors.Add(new TextErrorInfo(multipleValuesMessage, symbol.Start, symbol.Length));
                }
            }

            public bool TryParse(out PMap map)
            {
                try
                {
                    PValue rootValue;
                    JsonTerminalSymbol symbol;

                    bool hasRootValue = ParseMultiValue(FileShouldHaveEndedAlreadyMessage, out rootValue, out symbol);

                    JsonTerminalSymbol extraSymbol = ReadSkipComments();
                    if (extraSymbol != null)
                    {
                        Errors.Add(new TextErrorInfo(FileShouldHaveEndedAlreadyMessage, extraSymbol.Start, extraSymbol.Length));
                    }

                    if (hasRootValue)
                    {
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

        private readonly string json;

        private readonly List<JsonTerminalSymbol> tokens;

        public IReadOnlyList<JsonTerminalSymbol> Tokens => tokens.AsReadOnly();

        public TempJsonParser(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            this.json = json;
            tokens = new JsonTokenizer(json).TokenizeAll().ToList();
        }

        public bool TryParse(out PMap map, out List<TextErrorInfo> errors)
        {
            ParseRun parseRun = new ParseRun(tokens, json.Length);
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

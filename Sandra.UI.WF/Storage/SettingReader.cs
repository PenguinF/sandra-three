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
    public class SettingReader
    {
        private class ParseRun : JsonTerminalSymbolVisitor<PValue>
        {
            private const string EmptyKeyMessage = "Missing property key";
            private const string EmptyValueMessage = "Missing value";
            private const string MultipleValuesMessage = "',' expected";
            private const string MultiplePropertyKeysMessage = "':' expected";
            private const string MultipleKeySectionsMessage = "Unexpected ':', expected ',' or '}'";
            private const string EofInObjectMessage = "Unexpected end of file, expected '}'";
            private const string InvalidKeyMessage = "Invalid property key";
            private const string DuplicateKeyMessage = "Key '{0}' already exists in object";
            private const string ControlSymbolInObjectMessage = "'}' expected";
            private const string EofInArrayMessage = "Unexpected end of file, expected ']'";
            private const string ControlSymbolInArrayMessage = "']' expected";
            private const string UnrecognizedValueMessage = "Unrecognized value '{0}'";
            private const string NoPMapMessage = "Expected json object at root";
            private const string FileShouldHaveEndedAlreadyMessage = "End of file expected";

            private readonly List<JsonTextElement> tokens;
            private readonly int sourceLength;

            public readonly List<TextErrorInfo> Errors = new List<TextErrorInfo>();

            private int currentTokenIndex;

            public ParseRun(List<JsonTextElement> tokens, int sourceLength)
            {
                this.tokens = tokens;
                this.sourceLength = sourceLength;
                currentTokenIndex = 0;
            }

            private JsonTextElement PeekSkipComments()
            {
                // Skip comments until encountering something meaningful.
                while (currentTokenIndex < tokens.Count)
                {
                    JsonTextElement current = tokens[currentTokenIndex];
                    if (!current.IsBackground) return current;
                    Errors.AddRange(current.Errors);
                    currentTokenIndex++;
                }
                return null;
            }

            private JsonTextElement ReadSkipComments()
            {
                // Skip comments until encountering something meaningful.
                while (currentTokenIndex < tokens.Count)
                {
                    JsonTextElement current = tokens[currentTokenIndex];
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

                for (;;)
                {
                    PValue parsedKey;
                    JsonTextElement first;
                    bool gotKey = ParseMultiValue(MultiplePropertyKeysMessage, out parsedKey, out first);

                    bool validKey = false;
                    string propertyKey = default(string);

                    if (gotKey)
                    {
                        // Analyze if this is an actual, unique property key.
                        if (parsedKey is PString)
                        {
                            propertyKey = ((PString)parsedKey).Value;

                            // Expect unique keys.
                            validKey = !foundKeys.Contains(propertyKey);

                            if (validKey)
                            {
                                foundKeys.Add(propertyKey);
                            }
                            else
                            {
                                Errors.Add(new TextErrorInfo(string.Format(DuplicateKeyMessage, propertyKey), first.Start, first.Length));
                            }
                        }
                        else
                        {
                            Errors.Add(new TextErrorInfo(InvalidKeyMessage, first.Start, first.Length));
                        }
                    }

                    // ParseMultiValue() guarantees that the next symbol is never a ValueStartSymbol.
                    JsonTextElement symbol = ReadSkipComments();
                    PValue parsedValue = default(PValue);

                    // If gotValue remains false, a missing value error will be reported.
                    bool gotValue = false;

                    // Loop parsing values until encountering a non ':'.
                    bool gotColon = false;
                    while (symbol.TerminalSymbol is JsonColon)
                    {
                        if (gotColon)
                        {
                            // Multiple ':' without a ','.
                            Errors.Add(new TextErrorInfo(MultipleKeySectionsMessage, symbol.Start, symbol.Length));
                        }

                        JsonTextElement firstValueSymbol;
                        gotValue |= ParseMultiValue(MultipleValuesMessage, out parsedValue, out firstValueSymbol);

                        // Only the first value can be valid, even if it's undefined.
                        if (validKey && !gotColon && gotValue)
                        {
                            mapBuilder.Add(propertyKey, parsedValue);
                        }

                        symbol = ReadSkipComments();
                        gotColon = true;
                    }

                    bool isComma = symbol.TerminalSymbol is JsonComma;
                    bool isCurlyClose = symbol.TerminalSymbol is JsonCurlyClose;

                    // '}' directly following a ',' should not report errors.
                    // '..., : }' however misses both a key and a value.
                    if (isComma || (isCurlyClose && (gotKey || gotColon)))
                    {
                        // Report missing property key and/or value.
                        if (!gotKey)
                        {
                            Errors.Add(new TextErrorInfo(EmptyKeyMessage, symbol.Start, symbol.Length));
                        }

                        if (!gotValue)
                        {
                            Errors.Add(new TextErrorInfo(EmptyValueMessage, symbol.Start, symbol.Length));
                        }
                    }

                    if (!isComma)
                    {
                        // Assume missing closing bracket '}' on EOF or control symbol.
                        if (symbol == null)
                        {
                            Errors.Add(new TextErrorInfo(EofInObjectMessage, sourceLength - 1, 1));
                        }
                        else if (!isCurlyClose)
                        {
                            Errors.Add(new TextErrorInfo(ControlSymbolInObjectMessage, symbol.Start, symbol.Length));
                        }

                        return new PMap(mapBuilder);
                    }
                }
            }

            public override PValue VisitSquareBracketOpen(JsonSquareBracketOpen bracketOpen)
            {
                List<PValue> listBuilder = new List<PValue>();

                for (;;)
                {
                    PValue parsedValue;
                    JsonTextElement firstSymbol;

                    bool gotValue = ParseMultiValue(MultipleValuesMessage, out parsedValue, out firstSymbol);
                    if (gotValue) listBuilder.Add(parsedValue);

                    // ParseMultiValue() guarantees that the next symbol is never a ValueStartSymbol.
                    JsonTextElement symbol = ReadSkipComments();
                    if (symbol.TerminalSymbol is JsonComma)
                    {
                        if (!gotValue)
                        {
                            // Two commas or '[,': add an empty PErrorValue.
                            Errors.Add(new TextErrorInfo(EmptyValueMessage, symbol.Start, symbol.Length));
                            listBuilder.Add(PConstantValue.Undefined);
                        }
                    }
                    else
                    {
                        // Assume missing closing bracket ']' on EOF or control symbol.
                        if (symbol == null)
                        {
                            Errors.Add(new TextErrorInfo(EofInArrayMessage, sourceLength - 1, 1));
                        }
                        else if (!(symbol.TerminalSymbol is JsonSquareBracketClose))
                        {
                            Errors.Add(new TextErrorInfo(ControlSymbolInArrayMessage, symbol.Start, symbol.Length));
                        }

                        return new PList(listBuilder);
                    }
                }
            }

            public override PValue VisitValue(JsonValue symbol)
            {
                string value = symbol.Value;
                if (value == JsonValue.True) return PConstantValue.True;
                if (value == JsonValue.False) return PConstantValue.False;

                BigInteger integerValue;
                if (BigInteger.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out integerValue))
                {
                    return new PInteger(integerValue);
                }

                Errors.Add(new TextErrorInfo(
                    string.Format(UnrecognizedValueMessage, value),
                    tokens[currentTokenIndex - 1].Start,
                    value.Length));

                return PConstantValue.Undefined;
            }

            public override PValue VisitString(JsonString symbol) => new PString(symbol.Value);

            private bool ParseMultiValue(string multipleValuesMessage,
                                         out PValue firstValue,
                                         out JsonTextElement firstValueSymbol)
            {
                firstValue = default(PValue);
                firstValueSymbol = default(JsonTextElement);

                JsonTextElement symbol = PeekSkipComments();
                if (symbol == null || !symbol.IsValueStartSymbol) return false;

                firstValueSymbol = symbol;
                bool hasValue = false;

                for (;;)
                {
                    // Read the same symbol again but now eat it.
                    symbol = ReadSkipComments();

                    if (!hasValue)
                    {
                        if (symbol.Errors.Any()) firstValue = PConstantValue.Undefined;
                        else firstValue = Visit(symbol.TerminalSymbol);
                        hasValue = true;
                    }
                    else if (!symbol.Errors.Any())
                    {
                        // Make sure consecutive symbols are parsed as if they were valid.
                        // Discard the result.
                        Visit(symbol.TerminalSymbol);
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
                PValue rootValue;
                JsonTextElement symbol;

                bool hasRootValue = ParseMultiValue(FileShouldHaveEndedAlreadyMessage, out rootValue, out symbol);

                JsonTextElement extraSymbol = ReadSkipComments();
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
        }

        private readonly string json;

        private readonly List<JsonTextElement> tokens;

        public IReadOnlyList<JsonTextElement> Tokens => tokens.AsReadOnly();

        public SettingReader(string json)
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

        /// <summary>
        /// Loads settings from a file into a <see cref="SettingCopy"/>.
        /// </summary>
        internal static List<TextErrorInfo> ReadWorkingCopy(string json, SettingCopy workingCopy)
        {
            var parser = new SettingReader(json);

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

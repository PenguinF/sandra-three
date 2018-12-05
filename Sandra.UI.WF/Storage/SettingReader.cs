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

using SysExtensions.Text;
using SysExtensions.Text.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace Sandra.UI.WF.Storage
{
    /// <summary>
    /// Temporary class which parses a list of <see cref="JsonSymbol"/>s directly into a <see cref="PValue"/> result.
    /// </summary>
    public class SettingReader
    {
        private class ParseRun : JsonSymbolVisitor<PValue>
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

            private readonly List<TextElement<JsonSymbol>> tokens;
            private readonly int sourceLength;

            public readonly List<JsonErrorInfo> Errors = new List<JsonErrorInfo>();

            private int currentTokenIndex;

            public ParseRun(List<TextElement<JsonSymbol>> tokens, int sourceLength)
            {
                this.tokens = tokens;
                this.sourceLength = sourceLength;
                currentTokenIndex = 0;
            }

            private TextElement<JsonSymbol> PeekSkipComments()
            {
                // Skip comments until encountering something meaningful.
                while (currentTokenIndex < tokens.Count)
                {
                    TextElement<JsonSymbol> current = tokens[currentTokenIndex];
                    if (!current.TerminalSymbol.IsBackground) return current;
                    Errors.AddRange(current.TerminalSymbol.Errors);
                    currentTokenIndex++;
                }
                return null;
            }

            private TextElement<JsonSymbol> ReadSkipComments()
            {
                // Skip comments until encountering something meaningful.
                while (currentTokenIndex < tokens.Count)
                {
                    TextElement<JsonSymbol> current = tokens[currentTokenIndex];
                    Errors.AddRange(current.TerminalSymbol.Errors);
                    currentTokenIndex++;
                    if (!current.TerminalSymbol.IsBackground) return current;
                }
                return null;
            }

            public override PValue VisitCurlyOpen(JsonCurlyOpen curlyOpen)
            {
                Dictionary<string, PValue> mapBuilder = new Dictionary<string, PValue>();

                // Maintain a separate set of keys to aid error reporting on duplicate keys.
                HashSet<string> foundKeys = new HashSet<string>();

                for (; ; )
                {
                    bool gotKey = ParseMultiValue(
                        JsonErrorCode.MultiplePropertyKeys,
                        MultiplePropertyKeysMessage,
                        out PValue parsedKey,
                        out TextElement<JsonSymbol> first);

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
                                Errors.Add(new JsonErrorInfo(
                                    JsonErrorCode.PropertyKeyAlreadyExists,
                                    string.Format(DuplicateKeyMessage, propertyKey),
                                    first.Start,
                                    first.Length));
                            }
                        }
                        else
                        {
                            Errors.Add(new JsonErrorInfo(
                                JsonErrorCode.InvalidPropertyKey,
                                InvalidKeyMessage,
                                first.Start,
                                first.Length));
                        }
                    }

                    // ParseMultiValue() guarantees that the next symbol is never a ValueStartSymbol.
                    TextElement<JsonSymbol> textElement = ReadSkipComments();
                    PValue parsedValue = default(PValue);

                    // If gotValue remains false, a missing value error will be reported.
                    bool gotValue = false;

                    // Loop parsing values until encountering a non ':'.
                    bool gotColon = false;
                    while (textElement != null && textElement.TerminalSymbol is JsonColon)
                    {
                        if (gotColon)
                        {
                            // Multiple ':' without a ','.
                            Errors.Add(new JsonErrorInfo(
                                JsonErrorCode.MultiplePropertyKeySections,
                                MultipleKeySectionsMessage,
                                textElement.Start,
                                textElement.Length));
                        }

                        gotValue |= ParseMultiValue(
                            JsonErrorCode.MultiplePropertyValues,
                            MultipleValuesMessage,
                            out parsedValue,
                            out TextElement<JsonSymbol> firstValueSymbol);

                        // Only the first value can be valid, even if it's undefined.
                        if (validKey && !gotColon && gotValue)
                        {
                            mapBuilder.Add(propertyKey, parsedValue);
                        }

                        textElement = ReadSkipComments();
                        gotColon = true;
                    }

                    bool isComma = textElement != null && textElement.TerminalSymbol is JsonComma;
                    bool isCurlyClose = textElement != null && textElement.TerminalSymbol is JsonCurlyClose;

                    // '}' directly following a ',' should not report errors.
                    // '..., : }' however misses both a key and a value.
                    if (isComma || (isCurlyClose && (gotKey || gotColon)))
                    {
                        // Report missing property key and/or value.
                        if (!gotKey)
                        {
                            Errors.Add(new JsonErrorInfo(
                                JsonErrorCode.MissingPropertyKey,
                                EmptyKeyMessage,
                                textElement.Start,
                                textElement.Length));
                        }

                        if (!gotValue)
                        {
                            Errors.Add(new JsonErrorInfo(
                                JsonErrorCode.MissingPropertyValue,
                                EmptyValueMessage,
                                textElement.Start,
                                textElement.Length));
                        }
                    }

                    if (!isComma)
                    {
                        // Assume missing closing bracket '}' on EOF or control symbol.
                        if (textElement == null)
                        {
                            Errors.Add(new JsonErrorInfo(
                                JsonErrorCode.UnexpectedEofInObject,
                                EofInObjectMessage,
                                sourceLength,
                                0));
                        }
                        else if (!isCurlyClose)
                        {
                            Errors.Add(new JsonErrorInfo(
                                JsonErrorCode.ControlSymbolInObject,
                                ControlSymbolInObjectMessage,
                                textElement.Start,
                                textElement.Length));
                        }

                        return new PMap(mapBuilder);
                    }
                }
            }

            public override PValue VisitSquareBracketOpen(JsonSquareBracketOpen bracketOpen)
            {
                List<PValue> listBuilder = new List<PValue>();

                for (; ; )
                {

                    bool gotValue = ParseMultiValue(
                        JsonErrorCode.MultiplePropertyValues,
                        MultipleValuesMessage,
                        out PValue parsedValue,
                        out TextElement<JsonSymbol> firstSymbol);

                    if (gotValue) listBuilder.Add(parsedValue);

                    // ParseMultiValue() guarantees that the next symbol is never a ValueStartSymbol.
                    TextElement<JsonSymbol> textElement = ReadSkipComments();
                    if (textElement != null && textElement.TerminalSymbol is JsonComma)
                    {
                        if (!gotValue)
                        {
                            // Two commas or '[,': add an empty PErrorValue.
                            Errors.Add(new JsonErrorInfo(
                                JsonErrorCode.MissingPropertyValue,
                                EmptyValueMessage,
                                textElement.Start,
                                textElement.Length));

                            listBuilder.Add(PConstantValue.Undefined);
                        }
                    }
                    else
                    {
                        // Assume missing closing bracket ']' on EOF or control symbol.
                        if (textElement == null)
                        {
                            Errors.Add(new JsonErrorInfo(
                                JsonErrorCode.UnexpectedEofInArray,
                                EofInArrayMessage,
                                sourceLength,
                                0));
                        }
                        else if (!(textElement.TerminalSymbol is JsonSquareBracketClose))
                        {
                            Errors.Add(new JsonErrorInfo(
                                JsonErrorCode.ControlSymbolInArray,
                                ControlSymbolInArrayMessage,
                                textElement.Start,
                                textElement.Length));
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

                if (BigInteger.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out BigInteger integerValue))
                {
                    return new PInteger(integerValue);
                }

                Errors.Add(new JsonErrorInfo(
                    JsonErrorCode.UnrecognizedValue,
                    string.Format(UnrecognizedValueMessage, value),
                    tokens[currentTokenIndex - 1].Start,
                    value.Length));

                return PConstantValue.Undefined;
            }

            public override PValue VisitString(JsonString symbol) => new PString(symbol.Value);

            private bool ParseMultiValue(JsonErrorCode multipleValuesErrorCode,
                                         string multipleValuesMessage,
                                         out PValue firstValue,
                                         out TextElement<JsonSymbol> firstValueSymbol)
            {
                firstValue = default(PValue);
                firstValueSymbol = default(TextElement<JsonSymbol>);

                TextElement<JsonSymbol> textElement = PeekSkipComments();
                if (textElement == null || !textElement.TerminalSymbol.IsValueStartSymbol) return false;

                firstValueSymbol = textElement;
                bool hasValue = false;

                for (; ; )
                {
                    // Read the same symbol again but now eat it.
                    textElement = ReadSkipComments();

                    if (!hasValue)
                    {
                        if (textElement.TerminalSymbol.Errors.Any()) firstValue = PConstantValue.Undefined;
                        else firstValue = Visit(textElement.TerminalSymbol);
                        hasValue = true;
                    }
                    else if (!textElement.TerminalSymbol.Errors.Any())
                    {
                        // Make sure consecutive symbols are parsed as if they were valid.
                        // Discard the result.
                        Visit(textElement.TerminalSymbol);
                    }

                    // Peek at the next symbol.
                    // If IsValueStartSymbol == false in the first iteration, it means that exactly one value was parsed, as desired.
                    textElement = PeekSkipComments();
                    if (textElement == null || !textElement.TerminalSymbol.IsValueStartSymbol) return true;

                    // Two or more consecutive values not allowed.
                    Errors.Add(new JsonErrorInfo(
                        multipleValuesErrorCode,
                        multipleValuesMessage,
                        textElement.Start,
                        textElement.Length));
                }
            }

            public bool TryParse(out PMap map)
            {
                bool hasRootValue = ParseMultiValue(
                    JsonErrorCode.ExpectedEof,
                    FileShouldHaveEndedAlreadyMessage,
                    out PValue rootValue,
                    out TextElement<JsonSymbol> textElement);

                TextElement<JsonSymbol> extraElement = ReadSkipComments();
                if (extraElement != null)
                {
                    Errors.Add(new JsonErrorInfo(
                        JsonErrorCode.ExpectedEof,
                        FileShouldHaveEndedAlreadyMessage,
                        extraElement.Start,
                        extraElement.Length));
                }

                if (hasRootValue)
                {
                    bool validMap = PType.Map.TryGetValidValue(rootValue, out map);
                    if (!validMap)
                    {
                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.Custom, // Custom error code because an empty json is technically valid.
                            NoPMapMessage,
                            textElement.Start,
                            textElement.Length));
                    }

                    return validMap;
                }

                map = default(PMap);
                return false;
            }
        }

        private readonly string json;

        private readonly List<TextElement<JsonSymbol>> tokens;

        public IReadOnlyList<TextElement<JsonSymbol>> Tokens => tokens.AsReadOnly();

        public SettingReader(string json)
        {
            this.json = json ?? throw new ArgumentNullException(nameof(json));
            tokens = new JsonTokenizer(json).TokenizeAll().ToList();
        }

        public bool TryParse(out PMap map, out List<JsonErrorInfo> errors)
        {
            ParseRun parseRun = new ParseRun(tokens, json.Length);
            var validMap = parseRun.TryParse(out map);
            errors = parseRun.Errors;
            return validMap;
        }

        /// <summary>
        /// Loads settings from a file into a <see cref="SettingCopy"/>.
        /// </summary>
        internal static List<JsonErrorInfo> ReadWorkingCopy(string json, SettingCopy workingCopy)
        {
            var parser = new SettingReader(json);

            if (parser.TryParse(out PMap map, out List<JsonErrorInfo> errors))
            {
                foreach (var kv in map)
                {
                    if (workingCopy.Schema.TryGetProperty(new SettingKey(kv.Key), out SettingProperty property))
                    {
                        workingCopy.AddOrReplaceRaw(property, kv.Value);
                    }
                }
            }

            return errors;
        }
    }
}

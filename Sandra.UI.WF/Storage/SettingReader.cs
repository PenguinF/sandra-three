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

using SysExtensions;
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
            private readonly ReadOnlyList<TextElement<JsonSymbol>> tokens;
            private readonly int sourceLength;

            public readonly List<JsonErrorInfo> Errors = new List<JsonErrorInfo>();

            private int currentTokenIndex;

            public ParseRun(ReadOnlyList<TextElement<JsonSymbol>> tokens, int sourceLength)
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
                                    first.Start,
                                    first.Length,
                                    new[] { propertyKey }));
                            }
                        }
                        else
                        {
                            Errors.Add(new JsonErrorInfo(
                                JsonErrorCode.InvalidPropertyKey,
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
                                textElement.Start,
                                textElement.Length));
                        }

                        gotValue |= ParseMultiValue(
                            JsonErrorCode.MultipleValues,
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
                                textElement.Start,
                                textElement.Length));
                        }

                        if (!gotValue)
                        {
                            Errors.Add(new JsonErrorInfo(
                                JsonErrorCode.MissingValue,
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
                                sourceLength,
                                0));
                        }
                        else if (!isCurlyClose)
                        {
                            Errors.Add(new JsonErrorInfo(
                                JsonErrorCode.ControlSymbolInObject,
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
                        JsonErrorCode.MultipleValues,
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
                                JsonErrorCode.MissingValue,
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
                                sourceLength,
                                0));
                        }
                        else if (!(textElement.TerminalSymbol is JsonSquareBracketClose))
                        {
                            Errors.Add(new JsonErrorInfo(
                                JsonErrorCode.ControlSymbolInArray,
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
                    tokens[currentTokenIndex - 1].Start,
                    value.Length,
                    new[] { value }));

                return PConstantValue.Undefined;
            }

            public override PValue VisitString(JsonString symbol) => new PString(symbol.Value);

            private bool ParseMultiValue(JsonErrorCode multipleValuesErrorCode,
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
                        textElement.Start,
                        textElement.Length));
                }
            }

            public bool TryParse(out PMap map)
            {
                bool hasRootValue = ParseMultiValue(
                    JsonErrorCode.ExpectedEof,
                    out PValue rootValue,
                    out TextElement<JsonSymbol> textElement);

                TextElement<JsonSymbol> extraElement = ReadSkipComments();
                if (extraElement != null)
                {
                    Errors.Add(new JsonErrorInfo(
                        JsonErrorCode.ExpectedEof,
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

        public ReadOnlyList<TextElement<JsonSymbol>> Tokens { get; }

        public SettingReader(string json)
        {
            this.json = json ?? throw new ArgumentNullException(nameof(json));
            Tokens = new ReadOnlyList<TextElement<JsonSymbol>>(new JsonTokenizer(json).TokenizeAll());
        }

        public bool TryParse(out PMap map, out List<JsonErrorInfo> errors)
        {
            ParseRun parseRun = new ParseRun(Tokens, json.Length);
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

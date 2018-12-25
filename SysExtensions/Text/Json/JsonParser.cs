#region License
/*********************************************************************************
 * JsonParser.cs
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

namespace SysExtensions.Text.Json
{
    /// <summary>
    /// Represents a single parse of a list of json tokens.
    /// </summary>
    public class JsonParser : JsonSymbolVisitor<JsonSyntaxNode>
    {
        private readonly ReadOnlyList<TextElement<JsonSymbol>> Tokens;
        private readonly int SourceLength;
        private readonly List<JsonErrorInfo> Errors = new List<JsonErrorInfo>();

        private int CurrentTokenIndex;

        public JsonParser(ReadOnlyList<TextElement<JsonSymbol>> tokens, int sourceLength)
        {
            Tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
            SourceLength = sourceLength;
            CurrentTokenIndex = 0;
        }

        private TextElement<JsonSymbol> PeekSkipComments()
        {
            // Skip comments until encountering something meaningful.
            while (CurrentTokenIndex < Tokens.Count)
            {
                TextElement<JsonSymbol> current = Tokens[CurrentTokenIndex];
                if (!current.TerminalSymbol.IsBackground) return current;
                Errors.AddRange(current.TerminalSymbol.Errors);
                CurrentTokenIndex++;
            }

            return null;
        }

        private TextElement<JsonSymbol> ReadSkipComments()
        {
            // Skip comments until encountering something meaningful.
            while (CurrentTokenIndex < Tokens.Count)
            {
                TextElement<JsonSymbol> current = Tokens[CurrentTokenIndex];
                Errors.AddRange(current.TerminalSymbol.Errors);
                CurrentTokenIndex++;
                if (!current.TerminalSymbol.IsBackground) return current;
            }

            return null;
        }

        public override JsonSyntaxNode VisitCurlyOpen(JsonCurlyOpen curlyOpen)
        {
            Dictionary<string, JsonSyntaxNode> mapBuilder = new Dictionary<string, JsonSyntaxNode>();

            // Maintain a separate set of keys to aid error reporting on duplicate keys.
            HashSet<string> foundKeys = new HashSet<string>();

            for (; ; )
            {
                bool gotKey = ParseMultiValue(
                    JsonErrorCode.MultiplePropertyKeys,
                    out JsonSyntaxNode parsedKey,
                    out TextElement<JsonSymbol> first);

                bool validKey = false;
                string propertyKey = default(string);

                if (gotKey)
                {
                    // Analyze if this is an actual, unique property key.
                    if (parsedKey is JsonStringLiteralSyntax)
                    {
                        propertyKey = ((JsonStringLiteralSyntax)parsedKey).Value;

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
                JsonSyntaxNode parsedValue = default(JsonSyntaxNode);

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
                            SourceLength,
                            0));
                    }
                    else if (!isCurlyClose)
                    {
                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.ControlSymbolInObject,
                            textElement.Start,
                            textElement.Length));
                    }

                    return new JsonMapSyntax(mapBuilder);
                }
            }
        }

        public override JsonSyntaxNode VisitSquareBracketOpen(JsonSquareBracketOpen bracketOpen)
        {
            List<JsonSyntaxNode> listBuilder = new List<JsonSyntaxNode>();

            for (; ; )
            {
                bool gotValue = ParseMultiValue(
                    JsonErrorCode.MultipleValues,
                    out JsonSyntaxNode parsedValue,
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

                        listBuilder.Add(new JsonUndefinedValueSyntax());
                    }
                }
                else
                {
                    // Assume missing closing bracket ']' on EOF or control symbol.
                    if (textElement == null)
                    {
                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.UnexpectedEofInArray,
                            SourceLength,
                            0));
                    }
                    else if (!(textElement.TerminalSymbol is JsonSquareBracketClose))
                    {
                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.ControlSymbolInArray,
                            textElement.Start,
                            textElement.Length));
                    }

                    return new JsonListSyntax(listBuilder);
                }
            }
        }

        public override JsonSyntaxNode VisitValue(JsonValue symbol)
        {
            string value = symbol.Value;
            if (value == JsonValue.True) return new JsonBooleanLiteralSyntax(true);
            if (value == JsonValue.False) return new JsonBooleanLiteralSyntax(false);

            if (BigInteger.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out BigInteger integerValue))
            {
                return new JsonIntegerLiteralSyntax(integerValue);
            }

            Errors.Add(new JsonErrorInfo(
                JsonErrorCode.UnrecognizedValue,
                Tokens[CurrentTokenIndex - 1].Start,
                value.Length,
                new[] { value }));

            return new JsonUndefinedValueSyntax();
        }

        public override JsonSyntaxNode VisitString(JsonString symbol) => new JsonStringLiteralSyntax(symbol.Value);

        private bool ParseMultiValue(JsonErrorCode multipleValuesErrorCode,
                                     out JsonSyntaxNode firstValue,
                                     out TextElement<JsonSymbol> firstValueSymbol)
        {
            firstValue = default(JsonSyntaxNode);
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
                    if (textElement.TerminalSymbol.Errors.Any()) firstValue = new JsonUndefinedValueSyntax();
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

        public bool TryParse(out JsonSyntaxNode rootValue, out TextElement<JsonSymbol> textElement, out List<JsonErrorInfo> errors)
        {
            bool hasRootValue = ParseMultiValue(
                JsonErrorCode.ExpectedEof,
                out rootValue,
                out textElement);

            TextElement<JsonSymbol> extraElement = ReadSkipComments();
            if (extraElement != null)
            {
                Errors.Add(new JsonErrorInfo(
                    JsonErrorCode.ExpectedEof,
                    extraElement.Start,
                    extraElement.Length));
            }

            errors = Errors;
            return hasRootValue;
        }
    }
}

#region License
/*********************************************************************************
 * JsonParser.cs
 *
 * Copyright (c) 2004-2019 Henk Nicolai
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
**********************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a single parse of a list of json tokens.
    /// </summary>
    // Visit calls return the parsed value syntax node, and true if the current token must still be processed.
    public class JsonParser : JsonSymbolVisitor<(JsonValueSyntax, bool)>
    {
        private readonly IEnumerator<JsonSymbol> Tokens;
        private readonly string Json;
        private readonly List<JsonErrorInfo> Errors = new List<JsonErrorInfo>();
        private readonly List<JsonSymbol> BackgroundBuilder = new List<JsonSymbol>();

        private JsonSymbol CurrentToken;

        // Used for parse error reporting.
        private int CurrentLength;

        public JsonParser(IEnumerable<JsonSymbol> tokens, string json)
        {
            if (tokens == null) throw new ArgumentNullException(nameof(tokens));
            Tokens = tokens.GetEnumerator();
            Json = json;
        }

        private void ShiftToNextForegroundToken()
        {
            // Skip comments until encountering something meaningful.
            for (; ; )
            {
                CurrentToken = Tokens.MoveNext() ? Tokens.Current : null;
                if (CurrentToken == null) break;

                Errors.AddRange(CurrentToken.GetErrors(CurrentLength));
                CurrentLength += CurrentToken.Length;
                if (!CurrentToken.IsBackground) break;
                BackgroundBuilder.Add(CurrentToken);
            }
        }

        public override (JsonValueSyntax, bool) VisitCurlyOpen(JsonCurlyOpen curlyOpen)
        {
            int start = CurrentLength - curlyOpen.Length;
            var mapBuilder = new List<JsonKeyValueSyntax>();

            // Maintain a separate set of keys to aid error reporting on duplicate keys.
            HashSet<string> foundKeys = new HashSet<string>();

            for (; ; )
            {
                bool gotKey = ParseMultiValue(JsonErrorCode.MultiplePropertyKeys, out JsonValueSyntax parsedKeyNode);

                bool validKey = false;
                JsonStringLiteralSyntax propertyKeyNode = default;

                if (gotKey)
                {
                    // Analyze if this is an actual, unique property key.
                    if (parsedKeyNode is JsonStringLiteralSyntax stringLiteral)
                    {
                        propertyKeyNode = stringLiteral;
                        string propertyKey = stringLiteral.Value;

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
                                parsedKeyNode.Start,
                                parsedKeyNode.Length,
                                // Take the substring, key may contain escape sequences.
                                new[] { Json.Substring(parsedKeyNode.Start, parsedKeyNode.Length) }));
                        }
                    }
                    else
                    {
                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.InvalidPropertyKey,
                            parsedKeyNode.Start,
                            parsedKeyNode.Length));
                    }
                }

                // If gotValue remains false, a missing value error will be reported.
                bool gotValue = false;

                // Loop parsing values until encountering a non ':'.
                bool gotColon = false;
                while (CurrentToken is JsonColon)
                {
                    if (gotColon)
                    {
                        // Multiple ':' without a ','.
                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.MultiplePropertyKeySections,
                            CurrentLength - CurrentToken.Length,
                            CurrentToken.Length));
                    }

                    // ParseMultiValue() guarantees that the next symbol is never a ValueStartSymbol.
                    gotValue |= ParseMultiValue(JsonErrorCode.MultipleValues, out JsonValueSyntax parsedValueNode);

                    // Only the first value can be valid, even if it's undefined.
                    if (validKey && !gotColon && gotValue)
                    {
                        mapBuilder.Add(new JsonKeyValueSyntax(propertyKeyNode, parsedValueNode));
                    }

                    gotColon = true;
                }

                bool isComma = CurrentToken is JsonComma;
                bool isCurlyClose = CurrentToken is JsonCurlyClose;

                // '}' directly following a ',' should not report errors.
                // '..., : }' however misses both a key and a value.
                if (isComma || (isCurlyClose && (gotKey || gotColon)))
                {
                    // Report missing property key and/or value.
                    if (!gotKey)
                    {
                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.MissingPropertyKey,
                            CurrentLength - CurrentToken.Length,
                            CurrentToken.Length));
                    }

                    if (!gotValue)
                    {
                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.MissingValue,
                            CurrentLength - CurrentToken.Length,
                            CurrentToken.Length));
                    }
                }

                if (!isComma)
                {
                    // Assume missing closing bracket '}' on EOF or control symbol.
                    bool unprocessedToken = false;
                    int endPosition;
                    if (CurrentToken == null)
                    {
                        endPosition = CurrentLength;

                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.UnexpectedEofInObject,
                            endPosition,
                            0));
                    }
                    else if (isCurlyClose)
                    {
                        endPosition = CurrentLength;
                    }
                    else
                    {
                        // ']'
                        unprocessedToken = true;
                        endPosition = CurrentLength - CurrentToken.Length;

                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.ControlSymbolInObject,
                            endPosition,
                            CurrentToken.Length));
                    }

                    int length = endPosition - start;
                    return (new JsonMapSyntax(mapBuilder, start, length), unprocessedToken);
                }
            }
        }

        public override (JsonValueSyntax, bool) VisitSquareBracketOpen(JsonSquareBracketOpen bracketOpen)
        {
            int start = CurrentLength - bracketOpen.Length;
            List<JsonValueSyntax> listBuilder = new List<JsonValueSyntax>();

            for (; ; )
            {
                bool gotValue = ParseMultiValue(JsonErrorCode.MultipleValues, out JsonValueSyntax parsedValueNode);

                if (gotValue) listBuilder.Add(parsedValueNode);

                // ParseMultiValue() guarantees that the next symbol is never a ValueStartSymbol.
                if (CurrentToken is JsonComma)
                {
                    if (!gotValue)
                    {
                        // Two commas or '[,'.
                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.MissingValue,
                            CurrentLength - CurrentToken.Length,
                            CurrentToken.Length));

                        listBuilder.Add(new JsonMissingValueSyntax(CurrentLength - CurrentToken.Length));
                    }
                }
                else
                {
                    // Assume missing closing bracket ']' on EOF or control symbol.
                    bool unprocessedToken = false;
                    int endPosition;
                    if (CurrentToken == null)
                    {
                        endPosition = CurrentLength;

                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.UnexpectedEofInArray,
                            endPosition,
                            0));
                    }
                    else if (CurrentToken is JsonSquareBracketClose)
                    {
                        endPosition = CurrentLength;
                    }
                    else
                    {
                        // ':', '}'
                        unprocessedToken = true;
                        endPosition = CurrentLength - CurrentToken.Length;

                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.ControlSymbolInArray,
                            endPosition,
                            CurrentToken.Length));
                    }

                    int length = endPosition - start;
                    return (new JsonListSyntax(listBuilder, start, length), unprocessedToken);
                }
            }
        }

        public override (JsonValueSyntax, bool) VisitValue(JsonValue symbol)
        {
            if (symbol == JsonValue.FalseJsonValue) return (new JsonBooleanLiteralSyntax(symbol, false, CurrentLength - JsonValue.FalseSymbolLength), false);
            if (symbol == JsonValue.TrueJsonValue) return (new JsonBooleanLiteralSyntax(symbol, true, CurrentLength - JsonValue.TrueSymbolLength), false);

            string value = symbol.Value;
            if (BigInteger.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out BigInteger integerValue))
            {
                return (new JsonIntegerLiteralSyntax(symbol, integerValue, CurrentLength - symbol.Length), false);
            }

            Errors.Add(new JsonErrorInfo(
                JsonErrorCode.UnrecognizedValue,
                CurrentLength - symbol.Length,
                symbol.Length,
                new[] { value }));

            return (new JsonUndefinedValueSyntax(symbol, CurrentLength - symbol.Length), false);
        }

        public override (JsonValueSyntax, bool) VisitString(JsonString symbol)
            => (new JsonStringLiteralSyntax(symbol, CurrentLength - symbol.Length), false);

        private bool ParseMultiValue(JsonErrorCode multipleValuesErrorCode,
                                     out JsonValueSyntax firstValueNode)
        {
            firstValueNode = null;

            ShiftToNextForegroundToken();
            if (CurrentToken == null || !CurrentToken.IsValueStartSymbol) return false;

            for (; ; )
            {
                // Make sure consecutive values are all parsed as if they were valid.
                // Interpret the first, discard the rest.
                JsonValueSyntax currentNode;
                bool unprocessedToken;
                if (CurrentToken.HasErrors)
                {
                    currentNode = new JsonUndefinedValueSyntax(CurrentToken, CurrentLength - CurrentToken.Length);
                    unprocessedToken = false;
                }
                else
                {
                    (currentNode, unprocessedToken) = Visit(CurrentToken);
                }

                if (firstValueNode == null)
                {
                    firstValueNode = currentNode;
                }

                // CurrentToken may be null, e.g. unterminated objects or arrays.
                if (CurrentToken == null) return true;

                // Move to the next symbol if CurrentToken was processed.
                if (!unprocessedToken) ShiftToNextForegroundToken();

                // If IsValueStartSymbol == false in the first iteration, it means that exactly one value was parsed, as desired.
                if (CurrentToken == null || !CurrentToken.IsValueStartSymbol) return true;

                // Two or more consecutive values not allowed.
                Errors.Add(new JsonErrorInfo(
                    multipleValuesErrorCode,
                    CurrentLength - CurrentToken.Length,
                    CurrentToken.Length));
            }
        }

        public bool TryParse(out JsonValueSyntax rootNode, out List<JsonErrorInfo> errors)
        {
            bool hasRootValue = ParseMultiValue(JsonErrorCode.ExpectedEof, out rootNode);

            if (CurrentToken != null)
            {
                Errors.Add(new JsonErrorInfo(
                    JsonErrorCode.ExpectedEof,
                    CurrentLength - CurrentToken.Length,
                    CurrentToken.Length));
            }

            errors = Errors;
            return hasRootValue;
        }
    }
}

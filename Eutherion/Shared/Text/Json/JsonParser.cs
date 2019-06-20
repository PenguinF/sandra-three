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
using System.Linq;
using System.Numerics;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a single parse of a list of json tokens.
    /// </summary>
    public class JsonParser : JsonSymbolVisitor<TextElement<JsonSymbol>, JsonSyntaxNode>
    {
        private readonly IEnumerator<TextElement<JsonSymbol>> Tokens;
        private readonly string Json;
        private readonly List<JsonErrorInfo> Errors = new List<JsonErrorInfo>();

        private TextElement<JsonSymbol> CurrentToken;

        public JsonParser(IEnumerable<TextElement<JsonSymbol>> tokens, string json)
        {
            if (tokens == null) throw new ArgumentNullException(nameof(tokens));
            Tokens = tokens.GetEnumerator();
            Json = json;
        }

        private void ShiftToNextForegroundToken()
        {
            // Skip comments until encountering something meaningful.
            do
            {
                CurrentToken = Tokens.MoveNext() ? Tokens.Current : null;
                if (CurrentToken != null) Errors.AddRange(CurrentToken.TerminalSymbol.Errors);
            }
            while (CurrentToken != null && CurrentToken.TerminalSymbol.IsBackground);
        }

        public override JsonSyntaxNode VisitCurlyOpen(JsonCurlyOpen curlyOpen, TextElement<JsonSymbol> visitedToken)
        {
            var mapBuilder = new List<JsonMapNodeKeyValuePair>();

            // Maintain a separate set of keys to aid error reporting on duplicate keys.
            HashSet<string> foundKeys = new HashSet<string>();

            for (; ; )
            {
                bool gotKey = ParseMultiValue(JsonErrorCode.MultiplePropertyKeys, out JsonSyntaxNode parsedKeyNode);

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

                // ParseMultiValue() guarantees that the next symbol is never a ValueStartSymbol.
                JsonSyntaxNode parsedValueNode = default;

                // If gotValue remains false, a missing value error will be reported.
                bool gotValue = false;

                // Loop parsing values until encountering a non ':'.
                bool gotColon = false;
                while (CurrentToken != null && CurrentToken.TerminalSymbol is JsonColon)
                {
                    if (gotColon)
                    {
                        // Multiple ':' without a ','.
                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.MultiplePropertyKeySections,
                            CurrentToken.Start,
                            CurrentToken.Length));
                    }

                    gotValue |= ParseMultiValue(JsonErrorCode.MultipleValues, out parsedValueNode);

                    // Only the first value can be valid, even if it's undefined.
                    if (validKey && !gotColon && gotValue)
                    {
                        mapBuilder.Add(new JsonMapNodeKeyValuePair(propertyKeyNode, parsedValueNode));
                    }

                    gotColon = true;
                }

                bool isComma = CurrentToken != null && CurrentToken.TerminalSymbol is JsonComma;
                bool isCurlyClose = CurrentToken != null && CurrentToken.TerminalSymbol is JsonCurlyClose;

                // '}' directly following a ',' should not report errors.
                // '..., : }' however misses both a key and a value.
                if (isComma || (isCurlyClose && (gotKey || gotColon)))
                {
                    // Report missing property key and/or value.
                    if (!gotKey)
                    {
                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.MissingPropertyKey,
                            CurrentToken.Start,
                            CurrentToken.Length));
                    }

                    if (!gotValue)
                    {
                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.MissingValue,
                            CurrentToken.Start,
                            CurrentToken.Length));
                    }
                }

                if (!isComma)
                {
                    // Assume missing closing bracket '}' on EOF or control symbol.
                    int endPosition;
                    if (CurrentToken == null)
                    {
                        endPosition = Json.Length;

                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.UnexpectedEofInObject,
                            endPosition,
                            0));
                    }
                    else if (!isCurlyClose)
                    {
                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.ControlSymbolInObject,
                            CurrentToken.Start,
                            CurrentToken.Length));

                        endPosition = CurrentToken.Start;
                    }
                    else
                    {
                        endPosition = CurrentToken.End;
                    }

                    int start = visitedToken.Start;
                    int length = endPosition - start;
                    return new JsonMapSyntax(mapBuilder, start, length);
                }
            }
        }

        public override JsonSyntaxNode VisitSquareBracketOpen(JsonSquareBracketOpen bracketOpen, TextElement<JsonSymbol> visitedToken)
        {
            List<JsonSyntaxNode> listBuilder = new List<JsonSyntaxNode>();

            for (; ; )
            {
                bool gotValue = ParseMultiValue(JsonErrorCode.MultipleValues, out JsonSyntaxNode parsedValueNode);

                if (gotValue) listBuilder.Add(parsedValueNode);

                // ParseMultiValue() guarantees that the next symbol is never a ValueStartSymbol.
                if (CurrentToken != null && CurrentToken.TerminalSymbol is JsonComma)
                {
                    if (!gotValue)
                    {
                        // Two commas or '[,': add an empty JsonUndefinedValueSyntax.
                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.MissingValue,
                            CurrentToken.Start,
                            CurrentToken.Length));

                        listBuilder.Add(new JsonMissingValueSyntax(CurrentToken.Start));
                    }
                }
                else
                {
                    // Assume missing closing bracket ']' on EOF or control symbol.
                    int endPosition;
                    if (CurrentToken == null)
                    {
                        endPosition = Json.Length;

                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.UnexpectedEofInArray,
                            endPosition,
                            0));
                    }
                    else if (!(CurrentToken.TerminalSymbol is JsonSquareBracketClose))
                    {
                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.ControlSymbolInArray,
                            CurrentToken.Start,
                            CurrentToken.Length));

                        endPosition = CurrentToken.Start;
                    }
                    else
                    {
                        endPosition = CurrentToken.End;
                    }

                    int start = visitedToken.Start;
                    int length = endPosition - start;
                    return new JsonListSyntax(listBuilder, start, length);
                }
            }
        }

        public override JsonSyntaxNode VisitValue(JsonValue symbol, TextElement<JsonSymbol> visitedToken)
        {
            string value = symbol.Value;
            if (value == JsonValue.True) return new JsonBooleanLiteralSyntax(visitedToken, true);
            if (value == JsonValue.False) return new JsonBooleanLiteralSyntax(visitedToken, false);

            if (BigInteger.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out BigInteger integerValue))
            {
                return new JsonIntegerLiteralSyntax(visitedToken, integerValue);
            }

            Errors.Add(new JsonErrorInfo(
                JsonErrorCode.UnrecognizedValue,
                visitedToken.Start,
                visitedToken.Length,
                new[] { value }));

            return new JsonUndefinedValueSyntax(visitedToken);
        }

        public override JsonSyntaxNode VisitString(JsonString symbol, TextElement<JsonSymbol> visitedToken)
            => new JsonStringLiteralSyntax(visitedToken, symbol.Value);

        private bool ParseMultiValue(JsonErrorCode multipleValuesErrorCode,
                                     out JsonSyntaxNode firstValueNode)
        {
            firstValueNode = default;

            ShiftToNextForegroundToken();
            if (CurrentToken == null || !CurrentToken.TerminalSymbol.IsValueStartSymbol) return false;

            bool hasValue = false;

            for (; ; )
            {
                if (!hasValue)
                {
                    if (CurrentToken.TerminalSymbol.Errors.Any()) firstValueNode = new JsonUndefinedValueSyntax(CurrentToken);
                    else firstValueNode = Visit(CurrentToken.TerminalSymbol, CurrentToken);
                    hasValue = true;
                }
                else if (!CurrentToken.TerminalSymbol.Errors.Any())
                {
                    // Make sure consecutive symbols are parsed as if they were valid.
                    // Discard the result.
                    Visit(CurrentToken.TerminalSymbol, CurrentToken);
                }

                // CurrentToken may be null, e.g. unterminated objects or arrays.
                if (CurrentToken == null) return true;

                // Move to the next symbol.
                // If IsValueStartSymbol == false in the first iteration, it means that exactly one value was parsed, as desired.
                ShiftToNextForegroundToken();
                if (CurrentToken == null || !CurrentToken.TerminalSymbol.IsValueStartSymbol) return true;

                // Two or more consecutive values not allowed.
                Errors.Add(new JsonErrorInfo(
                    multipleValuesErrorCode,
                    CurrentToken.Start,
                    CurrentToken.Length));
            }
        }

        public bool TryParse(out JsonSyntaxNode rootNode, out List<JsonErrorInfo> errors)
        {
            bool hasRootValue = ParseMultiValue(JsonErrorCode.ExpectedEof, out rootNode);

            if (CurrentToken != null)
            {
                Errors.Add(new JsonErrorInfo(
                    JsonErrorCode.ExpectedEof,
                    CurrentToken.Start,
                    CurrentToken.Length));
            }

            errors = Errors;
            return hasRootValue;
        }
    }
}

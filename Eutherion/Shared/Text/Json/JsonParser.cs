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

using Eutherion.Utils;
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

        private JsonBackgroundSyntax CaptureBackground()
        {
            var background = JsonBackgroundSyntax.Create(BackgroundBuilder);
            BackgroundBuilder.Clear();
            return background;
        }

        public override (JsonValueSyntax, bool) VisitCurlyOpen(JsonCurlyOpen curlyOpen)
        {
            var mapBuilder = new List<JsonKeyValueSyntax>();
            var keyValueSyntaxBuilder = new List<JsonMultiValueSyntax>();

            // Maintain a separate set of keys to aid error reporting on duplicate keys.
            HashSet<string> foundKeys = new HashSet<string>();

            for (; ; )
            {
                int keyStart = CurrentLength;
                JsonMultiValueSyntax multiKeyNode = ParseMultiValue(JsonErrorCode.MultiplePropertyKeys);
                JsonValueSyntax parsedKeyNode = multiKeyNode.ValueNode.ContentNode;

                // Analyze if this is an actual, unique property key.
                int parsedKeyNodeStart = keyStart + multiKeyNode.ValueNode.BackgroundBefore.Length;
                bool gotKey;
                Maybe<JsonStringLiteralSyntax> validKey = Maybe<JsonStringLiteralSyntax>.Nothing;

                switch (parsedKeyNode)
                {
                    case JsonMissingValueSyntax _:
                        gotKey = false;
                        break;
                    case JsonStringLiteralSyntax stringLiteral:
                        gotKey = true;
                        string propertyKey = stringLiteral.Value;

                        // Expect unique keys.
                        if (!foundKeys.Contains(propertyKey))
                        {
                            validKey = stringLiteral;
                            foundKeys.Add(propertyKey);
                        }
                        else
                        {
                            Errors.Add(new JsonErrorInfo(
                                JsonErrorCode.PropertyKeyAlreadyExists,
                                parsedKeyNodeStart,
                                parsedKeyNode.Length,
                                // Take the substring, key may contain escape sequences.
                                new[] { Json.Substring(parsedKeyNodeStart, parsedKeyNode.Length) }));
                        }
                        break;
                    default:
                        gotKey = true;
                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.InvalidPropertyKey,
                            parsedKeyNodeStart,
                            parsedKeyNode.Length));
                        break;
                }

                // Reuse keyValueSyntaxBuilder.
                keyValueSyntaxBuilder.Clear();

                // Keep parsing multi-values until encountering a non ':'.
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
                    else
                    {
                        gotColon = true;
                    }

                    // ParseMultiValue() guarantees that the next symbol is never a ValueStartSymbol.
                    // Always add the GreenJsonMissingValueSyntax, because it may contain background symbols.
                    keyValueSyntaxBuilder.Add(ParseMultiValue(JsonErrorCode.MultipleValues));
                }

                // One key-value section done.
                var jsonKeyValueSyntax = new JsonKeyValueSyntax(
                    multiKeyNode,
                    validKey,
                    keyValueSyntaxBuilder);

                mapBuilder.Add(jsonKeyValueSyntax);

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

                    // Report missing value error from being reported if all value sections are empty.
                    // Example: { "key1":: 2, "key2": , }
                    // The first section does not report a missing value, the second section does.
                    if (jsonKeyValueSyntax.ValueNodes.All(x => x.ValueNode.ContentNode is JsonMissingValueSyntax))
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
                        // Do not include the control symbol in the map.
                        unprocessedToken = true;
                        endPosition = CurrentLength - CurrentToken.Length;

                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.ControlSymbolInObject,
                            endPosition,
                            CurrentToken.Length));
                    }

                    // This code assumes that JsonCurlyOpen.CurlyOpenLength == JsonComma.CommaLength.
                    // The first iteration should be CurlyOpenLength rather than CommaLength.
                    int length = 0;

                    for (int i = 0; i < mapBuilder.Count; i++)
                    {
                        length += JsonComma.CommaLength;
                        length += mapBuilder[i].Length;
                    }

                    if (isCurlyClose)
                    {
                        length += JsonCurlyClose.CurlyCloseLength;
                    }

                    return (new JsonMapSyntax(mapBuilder, length) { Start = endPosition - length }, unprocessedToken);
                }
            }
        }

        public override (JsonValueSyntax, bool) VisitSquareBracketOpen(JsonSquareBracketOpen bracketOpen)
        {
            var listBuilder = new List<JsonMultiValueSyntax>();

            for (; ; )
            {
                JsonMultiValueSyntax parsedValueNode = ParseMultiValue(JsonErrorCode.MultipleValues);

                // Always add each value, because it may contain background symbols.
                listBuilder.Add(parsedValueNode);

                // ParseMultiValue() guarantees that the next symbol is never a ValueStartSymbol.
                if (CurrentToken is JsonComma)
                {
                    if (parsedValueNode.ValueNode.ContentNode is JsonMissingValueSyntax)
                    {
                        // Two commas or '[,'.
                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.MissingValue,
                            CurrentLength - CurrentToken.Length,
                            CurrentToken.Length));
                    }
                }
                else
                {
                    // Assume missing closing bracket ']' on EOF or control symbol.
                    bool missingSquareBracketClose = true;
                    bool unprocessedToken = false;
                    int endPosition;
                    if (CurrentToken == null)
                    {
                        endPosition = CurrentLength;

                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.UnexpectedEofInArray,
                            CurrentLength,
                            0));
                    }
                    else if (CurrentToken is JsonSquareBracketClose)
                    {
                        missingSquareBracketClose = false;
                        endPosition = CurrentLength;
                    }
                    else
                    {
                        // ':', '}'
                        // Do not include the control symbol in the list.
                        unprocessedToken = true;
                        endPosition = CurrentLength - CurrentToken.Length;

                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.ControlSymbolInArray,
                            endPosition,
                            CurrentToken.Length));
                    }

                    // This code assumes that JsonSquareBracketOpen.SquareBracketOpenLength == JsonComma.CommaLength.
                    // The first iteration should formally be SquareBracketOpenLength rather than CommaLength.
                    int length = 0;

                    for (int i = 0; i < listBuilder.Count; i++)
                    {
                        length += JsonComma.CommaLength;
                        length += listBuilder[i].Length;
                    }

                    if (!missingSquareBracketClose)
                    {
                        length += JsonSquareBracketClose.SquareBracketCloseLength;
                    }

                    return (new JsonListSyntax(listBuilder, length) { Start = endPosition - length }, unprocessedToken);
                }
            }
        }

        public override (JsonValueSyntax, bool) VisitValue(JsonValue symbol)
        {
            if (symbol == JsonValue.FalseJsonValue) return (new JsonBooleanLiteralSyntax(symbol, false) { Start = CurrentLength - JsonValue.FalseSymbolLength }, false);
            if (symbol == JsonValue.TrueJsonValue) return (new JsonBooleanLiteralSyntax(symbol, true) { Start = CurrentLength - JsonValue.TrueSymbolLength }, false);

            string value = symbol.Value;
            if (BigInteger.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out BigInteger integerValue))
            {
                return (new JsonIntegerLiteralSyntax(symbol, integerValue) { Start = CurrentLength - symbol.Length }, false);
            }

            Errors.Add(new JsonErrorInfo(
                JsonErrorCode.UnrecognizedValue,
                CurrentLength - symbol.Length,
                symbol.Length,
                new[] { value }));

            return (new JsonUndefinedValueSyntax(symbol) { Start = CurrentLength - symbol.Length }, false);
        }

        public override (JsonValueSyntax, bool) VisitString(JsonString symbol)
            => (new JsonStringLiteralSyntax(symbol) { Start = CurrentLength - symbol.Length }, false);

        private JsonMultiValueSyntax ParseMultiValue(JsonErrorCode multipleValuesErrorCode)
        {
            ShiftToNextForegroundToken();

            if (CurrentToken == null)
            {
                return new JsonMultiValueSyntax(
                    new JsonValueWithBackgroundSyntax(
                        CaptureBackground(),
                        new JsonMissingValueSyntax() { Start = CurrentLength }),
                    ReadOnlyList<JsonValueWithBackgroundSyntax>.Empty,
                    JsonBackgroundSyntax.Empty);
            }

            if (!CurrentToken.IsValueStartSymbol)
            {
                return new JsonMultiValueSyntax(
                    new JsonValueWithBackgroundSyntax(
                        CaptureBackground(),
                        new JsonMissingValueSyntax() { Start = CurrentLength - CurrentToken.Length }),
                    ReadOnlyList<JsonValueWithBackgroundSyntax>.Empty,
                    JsonBackgroundSyntax.Empty);
            }

            JsonValueWithBackgroundSyntax firstValueNode = null;
            var ignoredNodesBuilder = new List<JsonValueWithBackgroundSyntax>();

            for (; ; )
            {
                // Always create a value node, then decide if it must be ignored.
                // Have to clear the BackgroundBuilder before entering a recursive Visit() call.
                var backgroundBefore = CaptureBackground();

                JsonValueSyntax currentNode;
                bool unprocessedToken;
                if (CurrentToken.HasErrors)
                {
                    currentNode = new JsonUndefinedValueSyntax(CurrentToken) { Start = CurrentLength - CurrentToken.Length };
                    unprocessedToken = false;
                }
                else
                {
                    (currentNode, unprocessedToken) = Visit(CurrentToken);
                }

                var currentNodeWithBackgroundBefore = new JsonValueWithBackgroundSyntax(backgroundBefore, currentNode);
                if (firstValueNode == null)
                {
                    firstValueNode = currentNodeWithBackgroundBefore;
                }
                else
                {
                    // Ignore this node.
                    ignoredNodesBuilder.Add(currentNodeWithBackgroundBefore);
                }

                // CurrentToken may be null, e.g. unterminated objects or arrays.
                if (CurrentToken == null)
                {
                    // Apply invariant that BackgroundBuilder is always empty after a Visit() call.
                    // This means that here there's no need to capture the background.
                    return new JsonMultiValueSyntax(
                        firstValueNode,
                        ReadOnlyList<JsonValueWithBackgroundSyntax>.Create(ignoredNodesBuilder),
                        JsonBackgroundSyntax.Empty);
                }

                // Move to the next symbol if CurrentToken was processed.
                if (!unprocessedToken) ShiftToNextForegroundToken();

                // If IsValueStartSymbol == false in the first iteration, it means that exactly one value was parsed, as desired.
                if (CurrentToken == null || !CurrentToken.IsValueStartSymbol)
                {
                    // Capture the background following the last value.
                    var backgroundAfter = CaptureBackground();
                    return new JsonMultiValueSyntax(
                        firstValueNode,
                        ReadOnlyList<JsonValueWithBackgroundSyntax>.Create(ignoredNodesBuilder),
                        backgroundAfter);
                }

                // Two or more consecutive values not allowed.
                Errors.Add(new JsonErrorInfo(
                    multipleValuesErrorCode,
                    CurrentLength - CurrentToken.Length,
                    CurrentToken.Length));
            }
        }

        public JsonMultiValueSyntax TryParse(out List<JsonErrorInfo> errors)
        {
            JsonMultiValueSyntax multiValueNode = ParseMultiValue(JsonErrorCode.ExpectedEof);

            if (CurrentToken != null)
            {
                Errors.Add(new JsonErrorInfo(
                    JsonErrorCode.ExpectedEof,
                    CurrentLength - CurrentToken.Length,
                    CurrentToken.Length));
            }

            errors = Errors;

            return multiValueNode;
        }
    }
}

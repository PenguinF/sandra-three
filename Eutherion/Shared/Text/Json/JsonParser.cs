#region License
/*********************************************************************************
 * JsonParser.cs
 *
 * Copyright (c) 2004-2020 Henk Nicolai
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
using System.Linq;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a single parse of a list of json tokens.
    /// </summary>
    // Visit calls return the parsed value syntax node, and true if the current token must still be processed.
    public sealed class JsonParser
    {
        /// <summary>
        /// Helper class which is used after the last <see cref="IGreenJsonSymbol"/>,
        /// to reduce nullchecks during parsing, and reduce the chance of programming errors.
        /// </summary>
        private sealed class EofSymbol : IGreenJsonSymbol
        {
            public static readonly EofSymbol Value = new EofSymbol();

            public JsonSymbolType SymbolType => JsonSymbolType.Eof;

            private EofSymbol() { }

            int ISpan.Length => 0;

            IEnumerable<JsonErrorInfo> IGreenJsonSymbol.GetErrors(int startPosition) => EmptyEnumerable<JsonErrorInfo>.Instance;
        }

        private class MaximumDepthExceededException : Exception { }

        /// <summary>
        /// Gets the maximum allowed depth of any Json parse tree. As this is a simple recursive parser,
        /// it would otherwise be vulnerable to StackOverflowExceptions.
        /// </summary>
        public const int MaximumDepth = 40;

        private static RootJsonSyntax CreateParseTreeTooDeepRootSyntax(int startPosition, int length)
            => new RootJsonSyntax(
                new GreenJsonMultiValueSyntax(
                    new[] { new GreenJsonValueWithBackgroundSyntax(
                        GreenJsonBackgroundListSyntax.Empty,
                        GreenJsonMissingValueSyntax.Value) },
                    GreenJsonBackgroundListSyntax.Create(
                        new GreenJsonBackgroundSyntax[] { GreenJsonWhitespaceSyntax.Create(length) })),
                new List<JsonErrorInfo> { new JsonErrorInfo(JsonErrorCode.ParseTreeTooDeep, startPosition, 1) });

        internal const JsonSymbolType ForegroundThreshold = JsonSymbolType.BooleanLiteral;
        internal const JsonSymbolType ValueDelimiterThreshold = JsonSymbolType.Colon;

        private readonly IEnumerator<IGreenJsonSymbol> Tokens;
        private readonly string Json;
        private readonly List<JsonErrorInfo> Errors = new List<JsonErrorInfo>();
        private readonly List<GreenJsonBackgroundSyntax> BackgroundBuilder = new List<GreenJsonBackgroundSyntax>();

        private IGreenJsonSymbol CurrentToken;

        // Used for parse error reporting.
        private int CurrentLength;

        private int CurrentDepth;

        private JsonParser(string json)
        {
            Json = json ?? throw new ArgumentNullException(nameof(json));
            Tokens = JsonTokenizer.TokenizeAll(json).GetEnumerator();
        }

        private JsonSymbolType ShiftToNextForegroundToken()
        {
            // Skip background until encountering something meaningful.
            for (; ; )
            {
                IGreenJsonSymbol newToken = Tokens.MoveNext() ? Tokens.Current : EofSymbol.Value;
                Errors.AddRange(newToken.GetErrors(CurrentLength));
                CurrentLength += newToken.Length;
                JsonSymbolType symbolType = newToken.SymbolType;

                if (symbolType >= ForegroundThreshold)
                {
                    CurrentToken = newToken;
                    return symbolType;
                }

                BackgroundBuilder.Add((GreenJsonBackgroundSyntax)newToken);
            }
        }

        private GreenJsonBackgroundListSyntax CaptureBackground()
        {
            var background = GreenJsonBackgroundListSyntax.Create(BackgroundBuilder);
            BackgroundBuilder.Clear();
            return background;
        }

        public (GreenJsonValueSyntax, JsonSymbolType) ParseMap()
        {
            var mapBuilder = new List<GreenJsonKeyValueSyntax>();
            var keyValueSyntaxBuilder = new List<GreenJsonMultiValueSyntax>();

            // Maintain a separate set of keys to aid error reporting on duplicate keys.
            HashSet<string> foundKeys = new HashSet<string>();

            for (; ; )
            {
                // Save CurrentLength for error reporting before parsing the key.
                int keyStart = CurrentLength;
                GreenJsonMultiValueSyntax multiKeyNode = ParseMultiValue(JsonErrorCode.MultiplePropertyKeys);
                GreenJsonValueSyntax parsedKeyNode = multiKeyNode.ValueNode.ContentNode;

                // Analyze if this is an actual, unique property key.
                int parsedKeyNodeStart = keyStart + multiKeyNode.ValueNode.BackgroundBefore.Length;
                bool gotKey;
                Maybe<GreenJsonStringLiteralSyntax> validKey = Maybe<GreenJsonStringLiteralSyntax>.Nothing;

                switch (parsedKeyNode)
                {
                    case GreenJsonMissingValueSyntax _:
                        gotKey = false;
                        break;
                    case GreenJsonStringLiteralSyntax stringLiteral:
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
                keyValueSyntaxBuilder.Add(multiKeyNode);

                // Keep parsing multi-values until encountering a non ':'.
                JsonSymbolType symbolType = CurrentToken.SymbolType;
                bool gotColon = false;
                while (symbolType == JsonSymbolType.Colon)
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
                    keyValueSyntaxBuilder.Add(ParseMultiValue(JsonErrorCode.MultipleValues));
                    symbolType = CurrentToken.SymbolType;
                }

                // One key-value section done.
                var jsonKeyValueSyntax = new GreenJsonKeyValueSyntax(validKey, keyValueSyntaxBuilder);

                mapBuilder.Add(jsonKeyValueSyntax);

                bool isComma = symbolType == JsonSymbolType.Comma;
                bool isCurlyClose = symbolType == JsonSymbolType.CurlyClose;

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
                    // Skip the fist value section, it contains the key node.
                    if (jsonKeyValueSyntax.ValueSectionNodes.Skip(1).All(x => x.ValueNode.ContentNode is GreenJsonMissingValueSyntax))
                    {
                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.MissingValue,
                            CurrentLength - CurrentToken.Length,
                            CurrentToken.Length));
                    }
                }

                if (!isComma)
                {
                    if (isCurlyClose)
                    {
                        return (new GreenJsonMapSyntax(mapBuilder, missingCurlyClose: false), ShiftToNextForegroundToken());
                    }

                    // ']', EOF; assume missing closing bracket '}'.
                    Errors.Add(new JsonErrorInfo(
                        symbolType == JsonSymbolType.Eof ? JsonErrorCode.UnexpectedEofInObject : JsonErrorCode.ControlSymbolInObject,
                        CurrentLength - CurrentToken.Length,
                        CurrentToken.Length));

                    return (new GreenJsonMapSyntax(mapBuilder, missingCurlyClose: true), symbolType);
                }
            }
        }

        public (GreenJsonValueSyntax, JsonSymbolType) ParseList()
        {
            var listBuilder = new List<GreenJsonMultiValueSyntax>();

            for (; ; )
            {
                GreenJsonMultiValueSyntax parsedValueNode = ParseMultiValue(JsonErrorCode.MultipleValues);

                // Always add each value, because it may contain background symbols.
                listBuilder.Add(parsedValueNode);

                // ParseMultiValue() guarantees that the next symbol is never a ValueStartSymbol.
                JsonSymbolType symbolType = CurrentToken.SymbolType;
                if (symbolType == JsonSymbolType.Comma)
                {
                    if (parsedValueNode.ValueNode.ContentNode is GreenJsonMissingValueSyntax)
                    {
                        // Two commas or '[,'.
                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.MissingValue,
                            CurrentLength - CurrentToken.Length,
                            CurrentToken.Length));
                    }
                }
                else if (symbolType == JsonSymbolType.BracketClose)
                {
                    return (new GreenJsonListSyntax(listBuilder, missingSquareBracketClose: false), ShiftToNextForegroundToken());
                }
                else
                {
                    // ':', '}', EOF; assume missing closing bracket ']'.
                    Errors.Add(new JsonErrorInfo(
                        symbolType == JsonSymbolType.Eof ? JsonErrorCode.UnexpectedEofInArray : JsonErrorCode.ControlSymbolInArray,
                        CurrentLength - CurrentToken.Length,
                        CurrentToken.Length));

                    return (new GreenJsonListSyntax(listBuilder, missingSquareBracketClose: true), symbolType);
                }
            }
        }

        private void ParseValues(List<GreenJsonValueWithBackgroundSyntax> valueNodesBuilder, JsonErrorCode multipleValuesErrorCode)
        {
            JsonSymbolType symbolType = ShiftToNextForegroundToken();
            if (symbolType >= ValueDelimiterThreshold) return;

            // Invariant: discriminated != null && !discriminated.IsOption1().
            for (; ; )
            {
                // Have to clear the BackgroundBuilder before entering a recursive call.
                var backgroundBefore = CaptureBackground();

                GreenJsonValueSyntax valueNode;
                if (symbolType == JsonSymbolType.CurlyOpen)
                {
                    (valueNode, symbolType) = ParseMap();
                }
                else if (symbolType == JsonSymbolType.BracketOpen)
                {
                    (valueNode, symbolType) = ParseList();
                }
                else
                {
                    // All other IGreenJsonSymbols derive from GreenJsonValueSyntax.
                    valueNode = (GreenJsonValueSyntax)CurrentToken;
                    symbolType = ShiftToNextForegroundToken();
                }

                // Always add the value node, also if it contains an undefined value.
                valueNodesBuilder.Add(new GreenJsonValueWithBackgroundSyntax(backgroundBefore, valueNode));

                // If SymbolType >= JsonSymbolType.ValueDelimiterThreshold in the first iteration,
                // it means that exactly one value was parsed, as desired.
                if (symbolType >= ValueDelimiterThreshold) return;

                // Two or more consecutive values not allowed.
                Errors.Add(new JsonErrorInfo(
                    multipleValuesErrorCode,
                    CurrentLength - CurrentToken.Length,
                    CurrentToken.Length));
            }
        }

        private GreenJsonMultiValueSyntax CreateMultiValueNode(List<GreenJsonValueWithBackgroundSyntax> valueNodesBuilder)
        {
            var background = CaptureBackground();
            if (valueNodesBuilder.Count == 0)
            {
                valueNodesBuilder.Add(new GreenJsonValueWithBackgroundSyntax(background, GreenJsonMissingValueSyntax.Value));
                return new GreenJsonMultiValueSyntax(valueNodesBuilder, GreenJsonBackgroundListSyntax.Empty);
            }
            return new GreenJsonMultiValueSyntax(valueNodesBuilder, background);
        }

        private GreenJsonMultiValueSyntax ParseMultiValue(JsonErrorCode multipleValuesErrorCode)
        {
            CurrentDepth++;
            if (CurrentDepth >= MaximumDepth) throw new MaximumDepthExceededException();

            var valueNodesBuilder = new List<GreenJsonValueWithBackgroundSyntax>();
            ParseValues(valueNodesBuilder, multipleValuesErrorCode);

            CurrentDepth--;
            return CreateMultiValueNode(valueNodesBuilder);
        }

        // ParseMultiValue copy, except that it handles the SymbolType >= JsonSymbolType.ValueDelimiterThreshold case differently,
        // as it cannot go to a higher level in the stack to process value delimiter symbols.
        private RootJsonSyntax Parse()
        {
            var valueNodesBuilder = new List<GreenJsonValueWithBackgroundSyntax>();

            for (; ; )
            {
                try
                {
                    ParseValues(valueNodesBuilder, JsonErrorCode.ExpectedEof);
                }
                catch (MaximumDepthExceededException)
                {
                    // Just ignore everything so far and return a default parse tree.
                    return CreateParseTreeTooDeepRootSyntax(CurrentLength - 1, Json.Length);
                }

                if (CurrentToken.SymbolType == JsonSymbolType.Eof) return new RootJsonSyntax(CreateMultiValueNode(valueNodesBuilder), Errors);

                // ] } , : -- treat all of these at the top level as an undefined symbol without any semantic meaning.
                BackgroundBuilder.Add(new GreenJsonRootLevelValueDelimiterSyntax(CurrentToken));

                // Report an error if no value was encountered before the control symbol.
                Errors.Add(new JsonErrorInfo(
                    JsonErrorCode.ExpectedEof,
                    CurrentLength - CurrentToken.Length,
                    CurrentToken.Length));
            }
        }

        public static RootJsonSyntax Parse(string json) => new JsonParser(json).Parse();
    }
}

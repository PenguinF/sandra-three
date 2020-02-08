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
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a single parse of a list of json tokens.
    /// </summary>
    // Visit calls return the parsed value syntax node, and true if the current token must still be processed.
    public class JsonParser : JsonValueStarterSymbolVisitor<(GreenJsonValueSyntax, bool)>
    {
        private readonly IEnumerator<IGreenJsonSymbol> Tokens;
        private readonly string Json;
        private readonly List<JsonErrorInfo> Errors = new List<JsonErrorInfo>();
        private readonly List<GreenJsonBackgroundSyntax> BackgroundBuilder = new List<GreenJsonBackgroundSyntax>();

        private IJsonForegroundSymbol CurrentToken;

        // Used for parse error reporting.
        private int CurrentLength;

        private JsonParser(string json)
        {
            Json = json ?? throw new ArgumentNullException(nameof(json));
            Tokens = JsonTokenizer.TokenizeAll(json).GetEnumerator();
        }

        private void ShiftToNextForegroundToken()
        {
            // Skip background until encountering something meaningful.
            for (; ; )
            {
                IGreenJsonSymbol newToken = Tokens.MoveNext() ? Tokens.Current : null;
                if (newToken == null)
                {
                    CurrentToken = null;
                    break;
                }

                Errors.AddRange(newToken.GetErrors(CurrentLength));
                CurrentLength += newToken.Length;

                var discriminated = newToken.AsBackgroundOrForeground();
                if (discriminated.IsOption2(out IJsonForegroundSymbol foregroundSymbol))
                {
                    CurrentToken = foregroundSymbol;
                    break;
                }

                BackgroundBuilder.Add(discriminated.ToOption1());
            }
        }

        private GreenJsonBackgroundListSyntax CaptureBackground()
        {
            var background = GreenJsonBackgroundListSyntax.Create(BackgroundBuilder);
            BackgroundBuilder.Clear();
            return background;
        }

        public override (GreenJsonValueSyntax, bool) VisitCurlyOpenSyntax(GreenJsonCurlyOpenSyntax curlyOpen)
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
                bool gotColon = false;
                while (CurrentToken is GreenJsonColonSyntax)
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
                var jsonKeyValueSyntax = new GreenJsonKeyValueSyntax(validKey, keyValueSyntaxBuilder);

                mapBuilder.Add(jsonKeyValueSyntax);

                bool isComma = CurrentToken is GreenJsonCommaSyntax;
                bool isCurlyClose = CurrentToken is GreenJsonCurlyCloseSyntax;

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
                    // Assume missing closing bracket '}' on EOF or control symbol.
                    bool unprocessedToken = false;
                    if (CurrentToken == null)
                    {
                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.UnexpectedEofInObject,
                            CurrentLength,
                            0));
                    }
                    else if (!isCurlyClose)
                    {
                        // ']'
                        // Do not include the control symbol in the map.
                        unprocessedToken = true;

                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.ControlSymbolInObject,
                            CurrentLength - CurrentToken.Length,
                            CurrentToken.Length));
                    }

                    return (new GreenJsonMapSyntax(mapBuilder, missingCurlyClose: !isCurlyClose), unprocessedToken);
                }
            }
        }

        public override (GreenJsonValueSyntax, bool) VisitSquareBracketOpenSyntax(GreenJsonSquareBracketOpenSyntax bracketOpen)
        {
            var listBuilder = new List<GreenJsonMultiValueSyntax>();

            for (; ; )
            {
                GreenJsonMultiValueSyntax parsedValueNode = ParseMultiValue(JsonErrorCode.MultipleValues);

                // Always add each value, because it may contain background symbols.
                listBuilder.Add(parsedValueNode);

                // ParseMultiValue() guarantees that the next symbol is never a ValueStartSymbol.
                if (CurrentToken is GreenJsonCommaSyntax)
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
                else
                {
                    // Assume missing closing bracket ']' on EOF or control symbol.
                    bool missingSquareBracketClose = true;
                    bool unprocessedToken = false;
                    if (CurrentToken == null)
                    {
                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.UnexpectedEofInArray,
                            CurrentLength,
                            0));
                    }
                    else if (CurrentToken is GreenJsonSquareBracketCloseSyntax)
                    {
                        missingSquareBracketClose = false;
                    }
                    else
                    {
                        // ':', '}'
                        // Do not include the control symbol in the list.
                        unprocessedToken = true;

                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.ControlSymbolInArray,
                            CurrentLength - CurrentToken.Length,
                            CurrentToken.Length));
                    }

                    return (new GreenJsonListSyntax(listBuilder, missingSquareBracketClose), unprocessedToken);
                }
            }
        }

        public override (GreenJsonValueSyntax, bool) VisitValue(JsonValue symbol)
        {
            if (symbol == JsonValue.FalseJsonValue) return (GreenJsonBooleanLiteralSyntax.False.Instance, false);
            if (symbol == JsonValue.TrueJsonValue) return (GreenJsonBooleanLiteralSyntax.True.Instance, false);

            string value = symbol.Value;
            if (BigInteger.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out BigInteger integerValue))
            {
                return (new GreenJsonIntegerLiteralSyntax(symbol, integerValue), false);
            }

            Errors.Add(new JsonErrorInfo(
                JsonErrorCode.UnrecognizedValue,
                CurrentLength - symbol.Length,
                symbol.Length,
                new[] { value }));

            return (new GreenJsonUndefinedValueSyntax(symbol), false);
        }

        public override (GreenJsonValueSyntax, bool) VisitErrorStringSyntax(GreenJsonErrorStringSyntax symbol)
            => (new GreenJsonUndefinedValueSyntax(symbol), false);

        public override (GreenJsonValueSyntax, bool) VisitStringLiteralSyntax(JsonString symbol)
            => (new GreenJsonStringLiteralSyntax(symbol), false);

        public override (GreenJsonValueSyntax, bool) VisitUnknownSymbolSyntax(GreenJsonUnknownSymbolSyntax symbol)
            => (new GreenJsonUndefinedValueSyntax(symbol), false);

        // Returns whether or not the current token still needs to be processed.
        private bool ParseValueNode(List<GreenJsonValueWithBackgroundSyntax> valueNodesBuilder, IJsonValueStarterSymbol valueStarterSymbol)
        {
            // Have to clear the BackgroundBuilder before entering a recursive Visit() call.
            var backgroundBefore = CaptureBackground();
            (GreenJsonValueSyntax currentNode, bool unprocessedToken) = Visit(valueStarterSymbol);
            valueNodesBuilder.Add(new GreenJsonValueWithBackgroundSyntax(backgroundBefore, currentNode));
            return unprocessedToken;
        }

        private GreenJsonMultiValueSyntax ParseMultiValue(JsonErrorCode multipleValuesErrorCode)
        {
            var valueNodesBuilder = new List<GreenJsonValueWithBackgroundSyntax>();

            ShiftToNextForegroundToken();

            var discriminated = CurrentToken?.AsValueDelimiterOrStarter();
            if (discriminated == null || !discriminated.IsOption2(out IJsonValueStarterSymbol valueStarterSymbol))
            {
                valueNodesBuilder.Add(new GreenJsonValueWithBackgroundSyntax(CaptureBackground(), GreenJsonMissingValueSyntax.Value));
                return new GreenJsonMultiValueSyntax(valueNodesBuilder, GreenJsonBackgroundListSyntax.Empty);
            }

            // Invariant: discriminated != null && !discriminated.IsOption1().
            for (; ; )
            {
                // Always create a value node, even if it contains an undefined value.
                bool unprocessedToken = ParseValueNode(valueNodesBuilder, valueStarterSymbol);

                // CurrentToken may be null, e.g. unterminated objects or arrays.
                if (CurrentToken == null)
                {
                    // Apply invariant that BackgroundBuilder is always empty after a Visit() call.
                    // This means that here there's no need to capture the background.
                    return new GreenJsonMultiValueSyntax(valueNodesBuilder, GreenJsonBackgroundListSyntax.Empty);
                }

                // Move to the next symbol if CurrentToken was processed.
                if (!unprocessedToken) ShiftToNextForegroundToken();

                // If discriminated.IsOption2() is false in the first iteration, it means that exactly one value was parsed, as desired.
                discriminated = CurrentToken?.AsValueDelimiterOrStarter();
                if (discriminated == null || !discriminated.IsOption2(out valueStarterSymbol))
                {
                    // Capture the background following the last value.
                    return new GreenJsonMultiValueSyntax(valueNodesBuilder, CaptureBackground());
                }

                // Two or more consecutive values not allowed.
                Errors.Add(new JsonErrorInfo(
                    multipleValuesErrorCode,
                    CurrentLength - CurrentToken.Length,
                    CurrentToken.Length));
            }
        }

        // ParseMultiValue copy, except that it handles the discriminated.IsOption1() case differently,
        // as it cannot go to a higher level in the stack to process value delimiter symbols.
        private RootJsonSyntax Parse()
        {
            var valueNodesBuilder = new List<GreenJsonValueWithBackgroundSyntax>();

            ShiftToNextForegroundToken();

            if (CurrentToken == null)
            {
                valueNodesBuilder.Add(new GreenJsonValueWithBackgroundSyntax(CaptureBackground(), GreenJsonMissingValueSyntax.Value));
                return new RootJsonSyntax(
                    new GreenJsonMultiValueSyntax(valueNodesBuilder, GreenJsonBackgroundListSyntax.Empty),
                    Errors);
            }

            for (; ; )
            {
                var discriminated = CurrentToken.AsValueDelimiterOrStarter();

                bool unprocessedToken;
                if (discriminated.IsOption1(out IJsonValueDelimiterSymbol valueDelimiterSymbol))
                {
                    // ] } , : -- treat all of these at the top level as an undefined symbol without any semantic meaning.
                    if (valueNodesBuilder.Count == 0)
                    {
                        // Report an error if no value was encountered before the control symbol.
                        // For all later control symbols, the last statement in this loop will
                        // already have done it.
                        Errors.Add(new JsonErrorInfo(
                            JsonErrorCode.ExpectedEof,
                            CurrentLength - valueDelimiterSymbol.Length,
                            valueDelimiterSymbol.Length));
                    }

                    BackgroundBuilder.Add(new GreenJsonRootLevelValueDelimiterSyntax(valueDelimiterSymbol));
                    unprocessedToken = false;
                }
                else
                {
                    // Always create a value node, even if it contains an undefined value.
                    unprocessedToken = ParseValueNode(valueNodesBuilder, discriminated.ToOption2());

                    // CurrentToken may be null, e.g. unterminated objects or arrays.
                    if (CurrentToken == null)
                    {
                        // Apply invariant that BackgroundBuilder is always empty after a Visit() call.
                        // This means that here there's no need to capture the background.
                        return new RootJsonSyntax(
                            new GreenJsonMultiValueSyntax(valueNodesBuilder, GreenJsonBackgroundListSyntax.Empty),
                            Errors);
                    }
                }

                // Move to the next symbol if CurrentToken was processed.
                if (!unprocessedToken) ShiftToNextForegroundToken();

                if (CurrentToken == null)
                {
                    // valueNodesBuilder.Count == 0 at the end of e.g. "/**/ ]".
                    if (valueNodesBuilder.Count == 0)
                    {
                        valueNodesBuilder.Add(new GreenJsonValueWithBackgroundSyntax(CaptureBackground(), GreenJsonMissingValueSyntax.Value));
                        return new RootJsonSyntax(
                            new GreenJsonMultiValueSyntax(valueNodesBuilder, GreenJsonBackgroundListSyntax.Empty),
                            Errors);
                    }
                    else
                    {
                        // Capture the background following the last value.
                        return new RootJsonSyntax(
                            new GreenJsonMultiValueSyntax(valueNodesBuilder, CaptureBackground()),
                            Errors);
                    }
                }

                // Two or more consecutive values not allowed.
                Errors.Add(new JsonErrorInfo(
                    JsonErrorCode.ExpectedEof,
                    CurrentLength - CurrentToken.Length,
                    CurrentToken.Length));
            }
        }

        public static RootJsonSyntax Parse(string json) => new JsonParser(json).Parse();
    }
}

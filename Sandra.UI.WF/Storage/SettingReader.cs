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
using System.Linq;

namespace Sandra.UI.WF.Storage
{
    /// <summary>
    /// Temporary class which parses a list of <see cref="JsonSymbol"/>s directly into a <see cref="PValue"/> result.
    /// </summary>
    public class SettingReader
    {
        private readonly string json;

        public ReadOnlyList<TextElement<JsonSymbol>> Tokens { get; }

        public SettingReader(string json)
        {
            this.json = json ?? throw new ArgumentNullException(nameof(json));
            Tokens = new ReadOnlyList<TextElement<JsonSymbol>>(new JsonTokenizer(json).TokenizeAll());
        }

        private static bool TryParse(JsonParser parseRun, out PMap map)
        {
            bool hasRootValue = parseRun.TryParse(out JsonSyntaxNode rootNode, out TextElement<JsonSymbol> textElement);

            if (hasRootValue)
            {
                PValue rootValue = new ToPValueConverter().Visit(rootNode);
                bool validMap = PType.Map.TryGetValidValue(rootValue, out map);
                if (!validMap)
                {
                    parseRun.Errors.Add(new JsonErrorInfo(
                        JsonErrorCode.Custom, // Custom error code because an empty json is technically valid.
                        textElement.Start,
                        textElement.Length));
                }

                return validMap;
            }

            map = default(PMap);
            return false;
        }

        public bool TryParse(out PMap map, out List<JsonErrorInfo> errors)
        {
            JsonParser parseRun = new JsonParser(Tokens, json.Length);
            var validMap = TryParse(parseRun, out map);
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

    public class ToPValueConverter : JsonSyntaxNodeVisitor<PValue>
    {
        public PMap ConvertToMap(JsonMapSyntax value)
        {
            Dictionary<string, PValue> mapBuilder = new Dictionary<string, PValue>();

            foreach (var keyedNode in value.MapNodeKeyValuePairs)
            {
                mapBuilder.Add(keyedNode.Key, Visit(keyedNode.Value));
            }

            return new PMap(mapBuilder);
        }

        public override PValue VisitBooleanLiteralSyntax(JsonBooleanLiteralSyntax value) => value.Value ? PConstantValue.True : PConstantValue.False;
        public override PValue VisitIntegerLiteralSyntax(JsonIntegerLiteralSyntax value) => new PInteger(value.Value);
        public override PValue VisitListSyntax(JsonListSyntax value) => new PList(value.ElementNodes.Select(Visit));
        public override PValue VisitMapSyntax(JsonMapSyntax value) => ConvertToMap(value);
        public override PValue VisitStringLiteralSyntax(JsonStringLiteralSyntax value) => new PString(value.Value);
        public override PValue VisitUndefinedValueSyntax(JsonUndefinedValueSyntax value) => PConstantValue.Undefined;
    }
}

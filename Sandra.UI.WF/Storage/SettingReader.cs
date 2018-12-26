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
        public static readonly PTypeErrorBuilder RootValueShouldBeObjectTypeError
            = new PTypeErrorBuilder(new LocalizedStringKey(nameof(RootValueShouldBeObjectTypeError)));

        private readonly string json;

        public ReadOnlyList<TextElement<JsonSymbol>> Tokens { get; }

        public SettingReader(string json)
        {
            this.json = json ?? throw new ArgumentNullException(nameof(json));
            Tokens = new ReadOnlyList<TextElement<JsonSymbol>>(new JsonTokenizer(json).TokenizeAll());
        }

        public bool TryParse(SettingSchema schema, out PMap map, out List<JsonErrorInfo> errors)
        {
            JsonParser parser = new JsonParser(Tokens, json.Length);
            bool hasRootValue = parser.TryParse(out JsonSyntaxNode rootNode, out errors);

            if (hasRootValue)
            {
                if (rootNode is JsonMapSyntax mapNode)
                {
                    Dictionary<string, PValue> mapBuilder = new Dictionary<string, PValue>();
                    var converter = new ToPValueConverter();

                    // Analyze values with the provided schema while building the PMap.
                    foreach (var keyedNode in mapNode.MapNodeKeyValuePairs)
                    {
                        var convertedValue = converter.Visit(keyedNode.Value);

                        // TODO: should probably add a warning if a property key does not exist.
                        if (schema.TryGetProperty(new SettingKey(keyedNode.Key.Value), out SettingProperty property))
                        {
                            if (!property.IsValidValue(convertedValue, out ITypeErrorBuilder typeError))
                            {
                                errors.Add(PTypeError.Create(typeError, keyedNode.Value.Start, keyedNode.Value.Length));
                            }
                        }

                        mapBuilder.Add(keyedNode.Key.Value, convertedValue);
                    }

                    map = new PMap(mapBuilder);
                    return true;
                }

                errors.Add(PTypeError.Create(
                    RootValueShouldBeObjectTypeError,
                    rootNode.Start,
                    rootNode.Length));
            }

            map = default(PMap);
            return false;
        }

        /// <summary>
        /// Loads settings from a file into a <see cref="SettingCopy"/>.
        /// </summary>
        internal static List<JsonErrorInfo> ReadWorkingCopy(string json, SettingCopy workingCopy)
        {
            var parser = new SettingReader(json);

            if (parser.TryParse(workingCopy.Schema, out PMap map, out List<JsonErrorInfo> errors))
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
                mapBuilder.Add(keyedNode.Key.Value, Visit(keyedNode.Value));
            }

            return new PMap(mapBuilder);
        }

        public override PValue VisitBooleanLiteralSyntax(JsonBooleanLiteralSyntax value) => value.Value ? PConstantValue.True : PConstantValue.False;
        public override PValue VisitIntegerLiteralSyntax(JsonIntegerLiteralSyntax value) => new PInteger(value.Value);
        public override PValue VisitListSyntax(JsonListSyntax value) => new PList(value.ElementNodes.Select(Visit));
        public override PValue VisitMapSyntax(JsonMapSyntax value) => ConvertToMap(value);
        public override PValue VisitMissingValueSyntax(JsonMissingValueSyntax node) => PConstantValue.Undefined;
        public override PValue VisitStringLiteralSyntax(JsonStringLiteralSyntax value) => new PString(value.Value);
        public override PValue VisitUndefinedValueSyntax(JsonUndefinedValueSyntax value) => PConstantValue.Undefined;
    }
}

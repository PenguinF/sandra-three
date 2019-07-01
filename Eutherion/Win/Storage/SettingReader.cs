#region License
/*********************************************************************************
 * SettingReader.cs
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

using Eutherion.Text;
using Eutherion.Text.Json;
using Eutherion.Utils;
using System;
using System.Collections.Generic;

namespace Eutherion.Win.Storage
{
    /// <summary>
    /// Temporary class which parses a list of <see cref="JsonSymbol"/>s directly into a <see cref="PValue"/> result.
    /// </summary>
    public static class SettingReader
    {
        public static bool TryParse(string json, SettingSchema schema, out PMap map, out ReadOnlyList<TextElement<JsonSymbol>> tokens, out List<JsonErrorInfo> errors)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            tokens = new ReadOnlyList<TextElement<JsonSymbol>>(JsonTokenizer.TokenizeAll(json));

            JsonParser parser = new JsonParser(tokens, json);
            bool hasRootValue = parser.TryParse(out JsonSyntaxNode rootNode, out errors);

            if (hasRootValue)
            {
                if (rootNode is JsonMapSyntax mapNode)
                {
                    Dictionary<string, PValue> mapBuilder = new Dictionary<string, PValue>();

                    // Analyze values with the provided schema while building the PMap.
                    foreach (var keyedNode in mapNode.MapNodeKeyValuePairs)
                    {
                        if (schema.TryGetProperty(new SettingKey(keyedNode.Key.Value), out SettingProperty property))
                        {
                            var valueOrError = property.TryCreateValue(json, keyedNode.Value, errors);

                            if (valueOrError.IsOption2(out PValue convertedValue))
                            {
                                mapBuilder.Add(keyedNode.Key.Value, convertedValue);
                            }
                            else
                            {
                                valueOrError.IsOption1(out ITypeErrorBuilder typeError);
                                errors.Add(ValueTypeErrorAtPropertyKey.Create(typeError, keyedNode.Key, keyedNode.Value, json));
                            }
                        }
                        else
                        {
                            // TODO: add error levels, this should probably be a warning.
                            errors.Add(UnrecognizedPropertyKeyTypeError.Create(keyedNode.Key, json));
                        }
                    }

                    map = new PMap(mapBuilder);
                    return true;
                }

                errors.Add(ValueTypeError.Create(PType.MapTypeError, rootNode, json));
            }

            map = default;
            return false;
        }
    }
}

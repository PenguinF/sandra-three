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

using Eutherion.Text.Json;
using Eutherion.Utils;
using System.Collections.Generic;

namespace Eutherion.Win.Storage
{
    /// <summary>
    /// Temporary class which parses a list of <see cref="JsonSymbol"/>s directly into a <see cref="PValue"/> result.
    /// </summary>
    public static class SettingReader
    {
        public static bool TryParse(
            string json,
            SettingSchema schema,
            out SettingObject settingObject,
            out ReadOnlyList<JsonSymbol> tokens,
            out List<JsonErrorInfo> errors)
        {
            tokens = ReadOnlyList<JsonSymbol>.Create(JsonTokenizer.TokenizeAll(json));

            JsonParser parser = new JsonParser(tokens, json);
            bool hasRootValue = parser.TryParse(out JsonValueSyntax rootNode, out errors);

            if (hasRootValue)
            {
                if (schema.TryCreateValue(
                    json,
                    rootNode,
                    out settingObject,
                    rootNode.Start,
                    errors).IsOption1(out ITypeErrorBuilder typeError))
                {
                    errors.Add(ValueTypeError.Create(typeError, rootNode, json));
                    return false;
                }

                return true;
            }

            settingObject = default;
            return false;
        }
    }
}

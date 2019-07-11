﻿#region License
/*********************************************************************************
 * SettingSyntaxTree.cs
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
using System.Collections.Generic;

namespace Eutherion.Win.Storage
{
    /// <summary>
    /// Represents the result of parsing json and type-checking it against a <see cref="SettingSchema"/>.
    /// </summary>
    public class SettingSyntaxTree
    {
        public static SettingSyntaxTree ParseSettings(string json, SettingSchema schema)
        {
            JsonMultiValueSyntax rootNode = JsonParser.TryParse(json, out List<JsonErrorInfo> errors);

            if (rootNode.ValueNode.ContentNode is JsonMissingValueSyntax)
            {
                return new SettingSyntaxTree(rootNode, errors, null);
            }

            int rootNodeStart = rootNode.ValueNode.BackgroundBefore.Length;

            if (schema.TryCreateValue(
                json,
                rootNode.ValueNode.ContentNode,
                out SettingObject settingObject,
                rootNodeStart,
                errors).IsOption1(out ITypeErrorBuilder typeError))
            {
                errors.Add(ValueTypeError.Create(typeError, rootNode.ValueNode.ContentNode, json, rootNodeStart));
                return new SettingSyntaxTree(rootNode, errors, null);
            }

            return new SettingSyntaxTree(rootNode, errors, settingObject);
        }

        public JsonMultiValueSyntax JsonRootNode { get; }
        public List<JsonErrorInfo> Errors { get; }
        public SettingObject SettingObject { get; }

        public SettingSyntaxTree(JsonMultiValueSyntax jsonRootNode, List<JsonErrorInfo> errors, SettingObject settingObject)
        {
            JsonRootNode = jsonRootNode;
            Errors = errors;
            SettingObject = settingObject;
        }
    }
}

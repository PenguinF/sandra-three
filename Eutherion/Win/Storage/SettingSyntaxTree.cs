﻿#region License
/*********************************************************************************
 * SettingSyntaxTree.cs
 *
 * Copyright (c) 2004-2023 Henk Nicolai
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
            RootJsonSyntax rootNode = JsonParser.Parse(json);
            var errors = new ArrayBuilder<PTypeError>();

            // It is important to use green nodes here, so the schema doesn't need to create the entire parse tree to type-check its values.
            // Instead, schema.TryCreateValue accepts a rootNodeStart parameter to generate errors at the right locations.
            if (rootNode.Syntax.Green.ValueNode.ContentNode is GreenJsonMissingValueSyntax)
            {
                return new SettingSyntaxTree(rootNode, ReadOnlyList<PTypeError>.FromBuilder(errors), null);
            }

            int rootNodeStart = rootNode.Syntax.Green.ValueNode.BackgroundBefore.Length;

            if (schema.TryCreateValue(
                json,
                rootNode.Syntax.Green.ValueNode.ContentNode,
                out SettingObject settingObject,
                rootNodeStart,
                errors).IsOption1(out ITypeErrorBuilder typeError))
            {
                errors.Add(ValueTypeError.Create(typeError, rootNode.Syntax.Green.ValueNode.ContentNode, json, rootNodeStart));
                return new SettingSyntaxTree(rootNode, ReadOnlyList<PTypeError>.FromBuilder(errors), null);
            }

            return new SettingSyntaxTree(rootNode, ReadOnlyList<PTypeError>.FromBuilder(errors), settingObject);
        }

        public RootJsonSyntax JsonSyntaxTree { get; }
        public IEnumerable<JsonErrorInfo> Errors => JsonSyntaxTree.Errors;
        public ReadOnlyList<PTypeError> TypeErrors { get; }
        public SettingObject SettingObject { get; }

        private SettingSyntaxTree(RootJsonSyntax jsonSyntaxTree, ReadOnlyList<PTypeError> typeErrors, SettingObject settingObject)
        {
            JsonSyntaxTree = jsonSyntaxTree;
            SettingObject = settingObject;
            TypeErrors = typeErrors;
        }
    }
}

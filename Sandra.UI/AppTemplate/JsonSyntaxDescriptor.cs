#region License
/*********************************************************************************
 * JsonSyntaxDescriptor.cs
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

using Eutherion.Localization;
using Eutherion.Text.Json;
using Eutherion.Win.Storage;
using ScintillaNET;
using System;
using System.Collections.Generic;

namespace Eutherion.Win.AppTemplate
{
    /// <summary>
    /// Describes the interaction between json syntax and a syntax editor.
    /// Currently only used for testing.
    /// </summary>
    public class JsonSyntaxDescriptor : SyntaxDescriptor<RootJsonSyntax, JsonSymbol, JsonErrorInfo>
    {
        public static readonly string JsonFileExtension = "json";

        /// <summary>
        /// Schema which defines what kind of keys and values are valid in the parsed json.
        /// </summary>
        private readonly SettingSchema schema;

        /// <summary>
        /// Initializes a new instance of a <see cref="JsonSyntaxDescriptor"/>.
        /// </summary>
        /// <param name="schema">
        /// The schema which defines what kind of keys and values are valid in the parsed json.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="schema"/> is null.
        /// </exception>
        public JsonSyntaxDescriptor(SettingSchema schema)
        {
            this.schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        public override string FileExtension => JsonFileExtension;

        public override LocalizedStringKey FileExtensionLocalizedKey => SharedLocalizedStringKeys.JsonFiles;

        public override RootJsonSyntax Parse(string code)
        {
            var rootNode = JsonParser.Parse(code);

            if (rootNode.Errors.Count > 0)
            {
                rootNode.Errors.Sort((x, y)
                    => x.Start < y.Start ? -1
                    : x.Start > y.Start ? 1
                    : x.Length < y.Length ? -1
                    : x.Length > y.Length ? 1
                    : x.ErrorCode < y.ErrorCode ? -1
                    : x.ErrorCode > y.ErrorCode ? 1
                    : 0);
            }

            return rootNode;
        }

        public override IEnumerable<JsonSymbol> GetTerminals(RootJsonSyntax syntaxTree)
            => new JsonSymbolEnumerator(syntaxTree.Syntax);

        public override IEnumerable<JsonErrorInfo> GetErrors(RootJsonSyntax syntaxTree)
            => syntaxTree.Errors;

        public override Style GetStyle(SyntaxEditor<RootJsonSyntax, JsonSymbol, JsonErrorInfo> syntaxEditor, JsonSymbol terminalSymbol)
            => JsonStyleSelector<RootJsonSyntax, JsonErrorInfo>.Instance.Visit(terminalSymbol, syntaxEditor);

        public override int GetLength(JsonSymbol terminalSymbol)
            => terminalSymbol.Length;

        public override (int, int) GetErrorRange(JsonErrorInfo error)
            => (error.Start, error.Length);

        public override string GetErrorMessage(JsonErrorInfo error)
            => error.Message(Session.Current.CurrentLocalizer);
    }
}

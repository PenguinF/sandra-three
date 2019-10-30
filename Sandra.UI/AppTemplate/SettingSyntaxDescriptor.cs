#region License
/*********************************************************************************
 * SettingSyntaxDescriptor.cs
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
    /// Describes the interaction between json syntax used for setting objects and a syntax editor.
    /// </summary>
    public class SettingSyntaxDescriptor : SyntaxDescriptor<SettingSyntaxTree, JsonSymbol, JsonErrorInfo>
    {
        /// <summary>
        /// Schema which defines what kind of keys and values are valid in the parsed json.
        /// </summary>
        private readonly SettingSchema schema;

        /// <summary>
        /// Initializes a new instance of a <see cref="SettingSyntaxDescriptor"/>.
        /// </summary>
        /// <param name="schema">
        /// The schema which defines what kind of keys and values are valid in the parsed json.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="schema"/> is null.
        /// </exception>
        public SettingSyntaxDescriptor(SettingSchema schema)
        {
            this.schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        public override string FileExtension => JsonSyntaxDescriptor.JsonFileExtension;

        public override LocalizedStringKey FileExtensionLocalizedKey => SharedLocalizedStringKeys.JsonFiles;

        public override SettingSyntaxTree Parse(string code)
        {
            var settingSyntaxTree = SettingSyntaxTree.ParseSettings(code, schema);

            if (settingSyntaxTree.Errors.Count > 0)
            {
                settingSyntaxTree.Errors.Sort((x, y)
                    => x.Start < y.Start ? -1
                    : x.Start > y.Start ? 1
                    : x.Length < y.Length ? -1
                    : x.Length > y.Length ? 1
                    : x.ErrorCode < y.ErrorCode ? -1
                    : x.ErrorCode > y.ErrorCode ? 1
                    : 0);
            }

            return settingSyntaxTree;
        }

        public override IEnumerable<JsonSymbol> GetTerminals(SettingSyntaxTree syntaxTree)
            => new JsonSymbolEnumerator(syntaxTree.JsonSyntaxTree.Syntax);

        public override IEnumerable<JsonErrorInfo> GetErrors(SettingSyntaxTree syntaxTree)
            => syntaxTree.Errors;

        public override Style GetStyle(SyntaxEditor<SettingSyntaxTree, JsonSymbol, JsonErrorInfo> syntaxEditor, JsonSymbol terminalSymbol)
            => JsonStyleSelector<SettingSyntaxTree, JsonErrorInfo>.Instance.Visit(terminalSymbol, syntaxEditor);

        public override int GetLength(JsonSymbol terminalSymbol)
            => terminalSymbol.Length;

        public override (int, int) GetErrorRange(JsonErrorInfo error)
            => (error.Start, error.Length);

        public override string GetErrorMessage(JsonErrorInfo error)
            => error.Message(Session.Current.CurrentLocalizer);
    }
}

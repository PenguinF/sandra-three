#region License
/*********************************************************************************
 * JsonSyntaxDescriptor.cs
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

using Eutherion.Localization;
using Eutherion.Text.Json;
using ScintillaNET;
using System.Collections.Generic;

namespace Eutherion.Win.AppTemplate
{
    /// <summary>
    /// Describes the interaction between json syntax and a syntax editor.
    /// </summary>
    public class JsonSyntaxDescriptor : SyntaxDescriptor<RootJsonSyntax, IJsonSymbol, JsonErrorInfo>
    {
        public static readonly string JsonFileExtension = "json";

        public static readonly JsonSyntaxDescriptor Instance = new JsonSyntaxDescriptor();

        private JsonSyntaxDescriptor() { }

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

        public override IEnumerable<IJsonSymbol> GetTerminalsInRange(RootJsonSyntax syntaxTree, int start, int length)
            => syntaxTree.Syntax.TerminalSymbolsInRange(start, length);

        public override IEnumerable<JsonErrorInfo> GetErrors(RootJsonSyntax syntaxTree)
            => syntaxTree.Errors;

        public override Style GetStyle(SyntaxEditor<RootJsonSyntax, IJsonSymbol, JsonErrorInfo> syntaxEditor, IJsonSymbol terminalSymbol)
            => JsonStyleSelector<RootJsonSyntax>.Instance.Visit(terminalSymbol, syntaxEditor);

        public override (int, int) GetTokenSpan(IJsonSymbol terminalSymbol)
            => (terminalSymbol.ToSyntax().AbsoluteStart, terminalSymbol.Length);

        public override (int, int) GetErrorRange(JsonErrorInfo error)
            => (error.Start, error.Length);

        public override ErrorLevel GetErrorLevel(JsonErrorInfo error)
            => ErrorLevel.Error;

        public override string GetErrorMessage(JsonErrorInfo error)
            => error.Message(Session.Current.CurrentLocalizer);
    }
}

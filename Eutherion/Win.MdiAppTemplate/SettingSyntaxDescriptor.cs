#region License
/*********************************************************************************
 * SettingSyntaxDescriptor.cs
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

using Eutherion.Text;
using Eutherion.Text.Json;
using Eutherion.Win.Storage;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Eutherion.Win.MdiAppTemplate
{
    /// <summary>
    /// Describes the interaction between json syntax used for setting objects and a syntax editor.
    /// </summary>
    public class SettingSyntaxDescriptor : SyntaxDescriptor<SettingSyntaxTree, IJsonSymbol, Union<JsonErrorInfo, PTypeError>>
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

        public override string FileExtension
            => JsonSyntaxDescriptor.JsonFileExtension;

        public override StringKey<ForFormattedText> FileExtensionLocalizedKey
            => SharedLocalizedStringKeys.JsonFiles;

        public override SettingSyntaxTree Parse(string code)
            => SettingSyntaxTree.ParseSettings(code, schema);

        public override IEnumerable<IJsonSymbol> GetTerminalsInRange(SettingSyntaxTree syntaxTree, int start, int length)
            => syntaxTree.JsonSyntaxTree.Syntax.TerminalSymbolsInRange(start, length);

        public override IEnumerable<Union<JsonErrorInfo, PTypeError>> GetErrors(SettingSyntaxTree syntaxTree)
            => syntaxTree.Errors.Select(Union<JsonErrorInfo, PTypeError>.Option1)
            .Concat(syntaxTree.TypeErrors.Select(Union<JsonErrorInfo, PTypeError>.Option2));

        public override Style GetStyle(SyntaxEditor<SettingSyntaxTree, IJsonSymbol, Union<JsonErrorInfo, PTypeError>> syntaxEditor, IJsonSymbol terminalSymbol)
            => JsonStyleSelector<SettingSyntaxTree, Union<JsonErrorInfo, PTypeError>>.Instance.Visit(terminalSymbol, syntaxEditor);

        public override (int, int) GetTokenSpan(IJsonSymbol terminalSymbol)
            => (terminalSymbol.ToSyntax().AbsoluteStart, terminalSymbol.Length);

        public override (int, int) GetErrorRange(Union<JsonErrorInfo, PTypeError> error)
            => error.Match(
                whenOption1: x => (x.Start, x.Length),
                whenOption2: x => (x.Start, x.Length));

        public override ErrorLevel GetErrorLevel(Union<JsonErrorInfo, PTypeError> error)
            => error.Match(
                whenOption1: x => (ErrorLevel)x.ErrorLevel,
                whenOption2: x => x is UnrecognizedPropertyKeyWarning || x is DuplicatePropertyKeyWarning
                ? ErrorLevel.Warning : ErrorLevel.Error);

        public override string GetErrorMessage(Union<JsonErrorInfo, PTypeError> error)
            => error.Match(
                whenOption1: x => x.Message(Session.Current.CurrentLocalizer),
                whenOption2: x => x.FormatMessage(Session.Current.CurrentLocalizer));
    }
}

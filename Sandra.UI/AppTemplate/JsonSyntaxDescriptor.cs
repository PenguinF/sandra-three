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
using Eutherion.Text;
using Eutherion.Text.Json;
using Eutherion.Win.Storage;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Eutherion.Win.AppTemplate
{
    /// <summary>
    /// Describes the interaction between json syntax and a syntax editor.
    /// </summary>
    public class JsonSyntaxDescriptor : SyntaxDescriptor<JsonSymbol, JsonErrorInfo>
    {
        public static readonly string JsonFileExtension = "json";

        /// <summary>
        /// Schema which defines what kind of keys and values are valid in the parsed json.
        /// </summary>
        private readonly SettingSchema schema;

        /// <summary>
        /// Style selector for syntax highlighting.
        /// </summary>
        private readonly JsonStyleSelector styleSelector;

        /// <summary>
        /// Initializes a new instance of a <see cref="JsonSyntaxDescriptor"/>.
        /// </summary>
        /// <param name="schema">
        /// The schema which defines what kind of keys and values are valid in the parsed json.
        /// </param>
        /// <param name="styleSelector">
        /// The style selector for syntax highlighting.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="styleSelector"/> and/or <paramref name="schema"/> are null.
        /// </exception>
        public JsonSyntaxDescriptor(SettingSchema schema, JsonStyleSelector styleSelector)
        {
            this.schema = schema ?? throw new ArgumentNullException(nameof(schema));
            this.styleSelector = styleSelector ?? throw new ArgumentNullException(nameof(styleSelector));
        }

        public override string FileExtension => JsonFileExtension;

        public override LocalizedStringKey FileExtensionLocalizedKey => SharedLocalizedStringKeys.JsonFiles;

        public override (IEnumerable<TextElement<JsonSymbol>>, List<JsonErrorInfo>) Parse(string code)
        {
            var parser = new SettingReader(code);
            parser.TryParse(schema, out PMap dummy, out List<JsonErrorInfo> errors);

            if (errors.Count > 0)
            {
                errors.Sort((x, y)
                    => x.Start < y.Start ? -1
                    : x.Start > y.Start ? 1
                    : x.Length < y.Length ? -1
                    : x.Length > y.Length ? 1
                    : x.ErrorCode < y.ErrorCode ? -1
                    : x.ErrorCode > y.ErrorCode ? 1
                    : 0);
            }

            return (parser.Tokens, errors);
        }

        public override Style GetStyle(SyntaxEditor<JsonSymbol, JsonErrorInfo> syntaxEditor, JsonSymbol terminalSymbol)
            => styleSelector.Visit(terminalSymbol, syntaxEditor);

        public override (int, int) GetErrorRange(JsonErrorInfo error)
            => (error.Start, error.Length);

        public override string GetErrorMessage(JsonErrorInfo error)
            => error.Message(Session.Current.CurrentLocalizer);
    }

    /// <summary>
    /// A style selector for json syntax highlighting.
    /// </summary>
    public class JsonStyleSelector : JsonSymbolVisitor<SyntaxEditor<JsonSymbol, JsonErrorInfo>, Style>
    {
        private const int commentStyleIndex = 8;
        private const int valueStyleIndex = 9;
        private const int stringStyleIndex = 10;

        private static readonly Color commentForeColor = Color.FromArgb(128, 220, 220);
        private static readonly Font commentFont = new Font("Consolas", 10, FontStyle.Italic);

        private static readonly Color valueForeColor = Color.FromArgb(255, 255, 60);
        private static readonly Font valueFont = new Font("Consolas", 10, FontStyle.Bold);

        private static readonly Color stringForeColor = Color.FromArgb(255, 192, 144);

        public void InitializeStyles(SyntaxEditor<JsonSymbol, JsonErrorInfo> syntaxEditor)
        {
            syntaxEditor.Styles[commentStyleIndex].ForeColor = commentForeColor;
            commentFont.CopyTo(syntaxEditor.Styles[commentStyleIndex]);

            syntaxEditor.Styles[valueStyleIndex].ForeColor = valueForeColor;
            valueFont.CopyTo(syntaxEditor.Styles[valueStyleIndex]);

            syntaxEditor.Styles[stringStyleIndex].ForeColor = stringForeColor;
        }

        public override Style DefaultVisit(JsonSymbol symbol, SyntaxEditor<JsonSymbol, JsonErrorInfo> syntaxEditor)
            => syntaxEditor.DefaultStyle;

        public override Style VisitComment(JsonComment symbol, SyntaxEditor<JsonSymbol, JsonErrorInfo> syntaxEditor)
            => syntaxEditor.Styles[commentStyleIndex];

        public override Style VisitErrorString(JsonErrorString symbol, SyntaxEditor<JsonSymbol, JsonErrorInfo> syntaxEditor)
            => syntaxEditor.Styles[stringStyleIndex];

        public override Style VisitString(JsonString symbol, SyntaxEditor<JsonSymbol, JsonErrorInfo> syntaxEditor)
            => syntaxEditor.Styles[stringStyleIndex];

        public override Style VisitUnterminatedMultiLineComment(JsonUnterminatedMultiLineComment symbol, SyntaxEditor<JsonSymbol, JsonErrorInfo> syntaxEditor)
            => syntaxEditor.Styles[commentStyleIndex];

        public override Style VisitValue(JsonValue symbol, SyntaxEditor<JsonSymbol, JsonErrorInfo> syntaxEditor)
            => syntaxEditor.Styles[valueStyleIndex];
    }
}

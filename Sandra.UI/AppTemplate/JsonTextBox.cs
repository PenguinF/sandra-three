#region License
/*********************************************************************************
 * SettingsTextBox.cs
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
using Eutherion.Win.Storage;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Eutherion.Win.AppTemplate
{
    /// <summary>
    /// Represents a syntax editor which displays a json settings file.
    /// </summary>
    public class JsonTextBox : SyntaxEditor<JsonSymbol, JsonErrorInfo>
    {
        private const int commentStyleIndex = 8;
        private const int valueStyleIndex = 9;
        private const int stringStyleIndex = 10;

        private static readonly Color commentForeColor = Color.FromArgb(128, 220, 220);
        private static readonly Font commentFont = new Font("Consolas", 10, FontStyle.Italic);

        private static readonly Color valueForeColor = Color.FromArgb(255, 255, 60);
        private static readonly Font valueFont = new Font("Consolas", 10, FontStyle.Bold);

        private static readonly Color stringForeColor = Color.FromArgb(255, 192, 144);

        internal sealed class StyleSelector : JsonSymbolVisitor<Style>
        {
            private readonly JsonTextBox owner;

            public StyleSelector(JsonTextBox owner)
            {
                this.owner = owner;
            }

            public override Style DefaultVisit(JsonSymbol symbol) => owner.DefaultStyle;
            public override Style VisitComment(JsonComment symbol) => owner.Styles[commentStyleIndex];
            public override Style VisitErrorString(JsonErrorString symbol) => owner.Styles[stringStyleIndex];
            public override Style VisitString(JsonString symbol) => owner.Styles[stringStyleIndex];
            public override Style VisitUnterminatedMultiLineComment(JsonUnterminatedMultiLineComment symbol) => owner.Styles[commentStyleIndex];
            public override Style VisitValue(JsonValue symbol) => owner.Styles[valueStyleIndex];
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="JsonTextBox"/>.
        /// </summary>
        /// <param name="settingsFile">
        /// The settings file to show and/or edit.
        /// </param>
        /// <param name="initialTextGenerator">
        /// Optional function to generate initial text in case the settings file could not be loaded.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="settingsFile"/> is null.
        /// </exception>
        public JsonTextBox(SettingsFile settingsFile, Func<string> initialTextGenerator, SettingProperty<AutoSaveFileNamePair> autoSaveSetting)
            : base(new JsonSyntaxDescriptor(settingsFile), settingsFile, initialTextGenerator, autoSaveSetting)
        {
            Styles[commentStyleIndex].ForeColor = commentForeColor;
            commentFont.CopyTo(Styles[commentStyleIndex]);

            Styles[valueStyleIndex].ForeColor = valueForeColor;
            valueFont.CopyTo(Styles[valueStyleIndex]);

            Styles[stringStyleIndex].ForeColor = stringForeColor;

            if (Session.Current.TryGetAutoSaveValue(SharedSettings.JsonZoom, out int zoomFactor))
            {
                Zoom = zoomFactor;
            }
        }

        protected override void OnZoomFactorChanged(ZoomFactorChangedEventArgs e)
        {
            // Not only raise the event, but also save the zoom factor setting.
            base.OnZoomFactorChanged(e);
            Session.Current.AutoSave.Persist(SharedSettings.JsonZoom, e.ZoomFactor);
        }
    }

    /// <summary>
    /// Describes the interaction between json syntax and a syntax editor.
    /// </summary>
    public class JsonSyntaxDescriptor : SyntaxDescriptor<JsonSymbol, JsonErrorInfo>
    {
        /// <summary>
        /// Schema which defines what kind of keys and values are valid in the parsed json.
        /// </summary>
        private readonly SettingSchema schema;

        /// <summary>
        /// Initializes a new instance of a <see cref="JsonSyntaxDescriptor"/>.
        /// </summary>
        /// <param name="styleSelector">
        /// The style selector for syntax highlighting.
        /// </param>
        /// <param name="schema">
        /// The schema which defines what kind of keys and values are valid in the parsed json.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="styleSelector"/> and/or <paramref name="schema"/> are null.
        /// </exception>
        public JsonSyntaxDescriptor(SettingsFile settingsFile)
        {
            if (settingsFile == null) throw new ArgumentNullException(nameof(settingsFile));

            schema = settingsFile.Settings.Schema;
        }

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
        {
            var styleSelector = new JsonTextBox.StyleSelector((JsonTextBox)syntaxEditor);
            return styleSelector.Visit(terminalSymbol);
        }

        public override (int, int) GetErrorRange(JsonErrorInfo error) => (error.Start, error.Length);

        public override string GetErrorMessage(JsonErrorInfo error) => error.Message(Session.Current.CurrentLocalizer);
    }
}

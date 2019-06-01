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

using Eutherion.Text.Json;
using Eutherion.Win.Storage;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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

        private Style CommentStyle => Styles[commentStyleIndex];
        private Style ValueStyle => Styles[valueStyleIndex];
        private Style StringStyle => Styles[stringStyleIndex];

        private sealed class StyleSelector : JsonSymbolVisitor<Style>
        {
            private readonly JsonTextBox owner;

            public StyleSelector(JsonTextBox owner)
            {
                this.owner = owner;
            }

            public override Style DefaultVisit(JsonSymbol symbol) => owner.DefaultStyle;
            public override Style VisitComment(JsonComment symbol) => owner.CommentStyle;
            public override Style VisitErrorString(JsonErrorString symbol) => owner.StringStyle;
            public override Style VisitString(JsonString symbol) => owner.StringStyle;
            public override Style VisitUnterminatedMultiLineComment(JsonUnterminatedMultiLineComment symbol) => owner.CommentStyle;
            public override Style VisitValue(JsonValue symbol) => owner.ValueStyle;
        }

        /// <summary>
        /// Schema which defines what kind of keys and values are valid in the parsed json.
        /// </summary>
        private readonly SettingSchema schema;

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
            : base(new JsonSyntaxDescriptor(), settingsFile, autoSaveSetting)
        {
            if (settingsFile == null) throw new ArgumentNullException(nameof(settingsFile));

            schema = settingsFile.Settings.Schema;

            CommentStyle.ForeColor = commentForeColor;
            commentFont.CopyTo(CommentStyle);

            ValueStyle.ForeColor = valueForeColor;
            valueFont.CopyTo(ValueStyle);

            StringStyle.ForeColor = stringForeColor;

            if (Session.Current.TryGetAutoSaveValue(SharedSettings.JsonZoom, out int zoomFactor))
            {
                Zoom = zoomFactor;
            }

            // Only use initialTextGenerator if nothing was auto-saved.
            if (CodeFile.LoadException != null
                && string.IsNullOrEmpty(CodeFile.LocalCopyText))
            {
                Text = initialTextGenerator != null
                    ? (initialTextGenerator() ?? string.Empty)
                    : string.Empty;
            }
            else
            {
                CopyTextFromTextFile();
            }

            EmptyUndoBuffer();
        }

        protected override void OnZoomFactorChanged(ZoomFactorChangedEventArgs e)
        {
            // Not only raise the event, but also save the zoom factor setting.
            base.OnZoomFactorChanged(e);
            Session.Current.AutoSave.Persist(SharedSettings.JsonZoom, e.ZoomFactor);
        }

        private void ParseAndApplySyntaxHighlighting(string json)
        {
            int maxLineNumberLength = GetMaxLineNumberLength(Lines.Count);
            if (displayedMaxLineNumberLength != maxLineNumberLength)
            {
                Margins[0].Width = TextWidth(Style.LineNumber, new string('0', maxLineNumberLength + 1));
                Margins[1].Width = TextWidth(Style.LineNumber, "0");
                displayedMaxLineNumberLength = maxLineNumberLength;
            }

            TextIndex.Clear();

            var parser = new SettingReader(json);
            parser.Tokens.ForEach(TextIndex.AppendTerminalSymbol);

            var styleSelector = new StyleSelector(this);

            foreach (var textElement in TextIndex.Elements)
            {
                ApplyStyle(textElement, styleSelector.Visit(textElement.TerminalSymbol));
            }

            parser.TryParse(schema, out PMap dummy, out List<JsonErrorInfo> errors);

            IndicatorClearRange(0, TextLength);

            if (errors.Count == 0)
            {
                currentErrors = null;
            }
            else
            {
                errors.Sort((x, y)
                    => x.Start < y.Start ? -1
                    : x.Start > y.Start ? 1
                    : x.Length < y.Length ? -1
                    : x.Length > y.Length ? 1
                    : x.ErrorCode < y.ErrorCode ? -1
                    : x.ErrorCode > y.ErrorCode ? 1
                    : 0);

                currentErrors = errors;

                foreach (var error in errors)
                {
                    IndicatorFillRange(error.Start, error.Length);
                }
            }

            OnCurrentErrorsChanged(EventArgs.Empty);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            string currentText = Text;

            // This prevents re-entrancy into WorkingCopyTextFile.
            if (!copyingTextFromTextFile)
            {
                CodeFile.UpdateLocalCopyText(currentText, ContainsChanges);
            }

            ParseAndApplySyntaxHighlighting(currentText);
        }

        private List<JsonErrorInfo> currentErrors;

        public int CurrentErrorCount
            => currentErrors == null ? 0 : currentErrors.Count;

        public IEnumerable<JsonErrorInfo> CurrentErrors
            => currentErrors == null ? Enumerable.Empty<JsonErrorInfo>() : currentErrors.Enumerate();

        public event EventHandler CurrentErrorsChanged;

        protected virtual void OnCurrentErrorsChanged(EventArgs e)
        {
            CurrentErrorsChanged?.Invoke(this, e);
        }

        public void ActivateError(int errorIndex)
        {
            // Select the text that generated the error.
            if (currentErrors != null && 0 <= errorIndex && errorIndex < currentErrors.Count)
            {
                // Determine how many lines are visible in the top half of the control.
                int firstVisibleLine = FirstVisibleLine;
                int visibleLines = LinesOnScreen;
                int bottomVisibleLine = firstVisibleLine + visibleLines;

                // Then calculate which line should become the first visible line
                // so the error line ends up in the middle of the control.
                var hotError = currentErrors[errorIndex];
                int hotErrorLine = LineFromPosition(hotError.Start);

                // hotErrorLine in view?
                // Don't include the bottom line, it's likely not completely visible.
                if (hotErrorLine < firstVisibleLine || bottomVisibleLine <= hotErrorLine)
                {
                    int targetFirstVisibleLine = hotErrorLine - (visibleLines / 2);
                    if (targetFirstVisibleLine < 0) targetFirstVisibleLine = 0;
                    FirstVisibleLine = targetFirstVisibleLine;
                }

                GotoPosition(hotError.Start);
                if (hotError.Length > 0)
                {
                    SelectionEnd = hotError.Start + hotError.Length;
                }

                Focus();
            }
        }

        private IEnumerable<string> ActiveErrorMessages(int textPosition)
        {
            if (currentErrors != null && textPosition >= 0 && textPosition < TextLength)
            {
                foreach (var error in currentErrors)
                {
                    if (error.Start <= textPosition && textPosition < error.Start + error.Length)
                    {
                        yield return error.Message(Session.Current.CurrentLocalizer);
                    }
                }
            }
        }

        protected override void OnDwellStart(DwellEventArgs e)
        {
            base.OnDwellStart(e);

            int textPosition = e.Position;
            string toolTipText = string.Join("\n\n", ActiveErrorMessages(textPosition));

            if (toolTipText.Length > 0)
            {
                CallTipShow(textPosition, toolTipText);
            }
            else
            {
                CallTipCancel();
            }
        }
    }

    /// <summary>
    /// Describes the interaction between json syntax and a syntax editor.
    /// </summary>
    public class JsonSyntaxDescriptor : SyntaxDescriptor<JsonSymbol, JsonErrorInfo>
    {
    }
}

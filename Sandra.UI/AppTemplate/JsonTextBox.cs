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
using Eutherion.UIActions;
using Eutherion.Win.Storage;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Eutherion.Win.AppTemplate
{
    /// <summary>
    /// Represents a syntax editor which displays a json settings file.
    /// </summary>
    public class JsonTextBox : SyntaxEditor<JsonSymbol>
    {
        private const int commentStyleIndex = 8;
        private const int valueStyleIndex = 9;
        private const int stringStyleIndex = 10;
        private const int errorIndicatorIndex = 8;

        private static readonly Color callTipBackColor = Color.FromArgb(48, 32, 32);
        private static readonly Font callTipFont = new Font("Segoe UI", 10);

        private static readonly Color commentForeColor = Color.FromArgb(128, 220, 220);
        private static readonly Font commentFont = new Font("Consolas", 10, FontStyle.Italic);

        private static readonly Color valueForeColor = Color.FromArgb(255, 255, 60);
        private static readonly Font valueFont = new Font("Consolas", 10, FontStyle.Bold);

        private static readonly Color stringForeColor = Color.FromArgb(255, 192, 144);

        private Style CallTipStyle => Styles[Style.CallTip];
        private Style LineNumberStyle => Styles[Style.LineNumber];
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
        /// Gets the edited text file.
        /// </summary>
        public WorkingCopyTextFile WorkingCopyTextFile { get; }

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
        public JsonTextBox(SettingsFile settingsFile, Func<string> initialTextGenerator)
        {
            if (settingsFile == null) throw new ArgumentNullException(nameof(settingsFile));

            schema = settingsFile.Settings.Schema;

            WorkingCopyTextFile = WorkingCopyTextFile.OpenExisting(settingsFile);

            BorderStyle = BorderStyle.None;

            StyleResetDefault();
            DefaultStyle.BackColor = DefaultSyntaxEditorStyle.BackColor;
            DefaultStyle.ForeColor = DefaultSyntaxEditorStyle.ForeColor;
            DefaultSyntaxEditorStyle.Font.CopyTo(DefaultStyle);
            StyleClearAll();

            SetSelectionBackColor(true, DefaultSyntaxEditorStyle.ForeColor);
            SetSelectionForeColor(true, DefaultSyntaxEditorStyle.BackColor);

            LineNumberStyle.BackColor = DefaultSyntaxEditorStyle.BackColor;
            LineNumberStyle.ForeColor = DefaultSyntaxEditorStyle.LineNumberForeColor;
            DefaultSyntaxEditorStyle.Font.CopyTo(LineNumberStyle);

            CallTipStyle.BackColor = callTipBackColor;
            CallTipStyle.ForeColor = DefaultSyntaxEditorStyle.ForeColor;
            callTipFont.CopyTo(CallTipStyle);

            CommentStyle.ForeColor = commentForeColor;
            commentFont.CopyTo(CommentStyle);

            ValueStyle.ForeColor = valueForeColor;
            valueFont.CopyTo(ValueStyle);

            StringStyle.ForeColor = stringForeColor;

            Indicators[errorIndicatorIndex].Style = IndicatorStyle.Squiggle;
            Indicators[errorIndicatorIndex].ForeColor = Color.Red;
            IndicatorCurrent = errorIndicatorIndex;

            Margins[0].BackColor = DefaultSyntaxEditorStyle.BackColor;
            Margins[1].BackColor = DefaultSyntaxEditorStyle.BackColor;

            CaretForeColor = DefaultSyntaxEditorStyle.ForeColor;

            // Enable dwell events.
            MouseDwellTime = SystemInformation.MouseHoverTime;

            if (Session.Current.TryGetAutoSaveValue(SharedSettings.JsonZoom, out int zoomFactor))
            {
                Zoom = zoomFactor;
            }

            WorkingCopyTextFile.OpenTextFile.LoadedText.Match(
                whenOption1: exception =>
                {
                    if (initialTextGenerator != null)
                    {
                        Text = initialTextGenerator() ?? string.Empty;
                    }
                },
                whenOption2: loadedText =>
                {
                    Text = loadedText;
                    EmptyUndoBuffer();
                });
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

        private int displayedMaxLineNumberLength;

        private int GetMaxLineNumberLength(int maxLineNumberToDisplay)
        {
            if (maxLineNumberToDisplay <= 9) return 1;
            if (maxLineNumberToDisplay <= 99) return 2;
            if (maxLineNumberToDisplay <= 999) return 3;
            if (maxLineNumberToDisplay <= 9999) return 4;
            return (int)Math.Floor(Math.Log10(maxLineNumberToDisplay)) + 1;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            CallTipCancel();

            base.OnTextChanged(e);

            ParseAndApplySyntaxHighlighting(Text);
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

        protected override void OnDwellEnd(DwellEventArgs e)
        {
            CallTipCancel();
            base.OnDwellEnd(e);
        }

        public UIActionState TrySaveToFile(bool perform)
        {
            if (ReadOnly) return UIActionVisibility.Hidden;
            if (!Modified) return UIActionVisibility.Disabled;

            if (perform)
            {
                WorkingCopyTextFile.Save(Text);
                SetSavePoint();
            }

            return UIActionVisibility.Enabled;
        }
    }
}

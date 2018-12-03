#region License
/*********************************************************************************
 * SettingsTextBox.cs
 * 
 * Copyright (c) 2004-2018 Henk Nicolai
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
 *********************************************************************************/
#endregion

using Sandra.UI.WF.Storage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents a Windows rich text box which displays a json settings file.
    /// </summary>
    public partial class SettingsTextBox : SyntaxEditor<JsonTerminalSymbol>
    {
        /// <summary>
        /// Because the syntax renderer does not support discontinuous terminal symbols.
        /// </summary>
        private class JsonWhitespace : JsonTerminalSymbol
        {
            public static readonly JsonWhitespace Value = new JsonWhitespace();

            private JsonWhitespace() { }

            public override bool IsBackground => true;

            public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.DefaultVisit(this);
            public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.DefaultVisit(this);
        }

        private static readonly Color defaultBackColor = Color.FromArgb(16, 16, 16);
        private static readonly Color defaultForeColor = Color.WhiteSmoke;
        private static readonly Font defaultFont = new Font("Consolas", 10);

        private static readonly Color noErrorsForeColor = Color.FromArgb(176, 176, 176);
        private static readonly Font noErrorsFont = new Font("Calibri", 10, FontStyle.Italic);

        private static readonly Font errorsFont = new Font("Calibri", 10);

        private static readonly Color commentForeColor = Color.FromArgb(128, 220, 220);
        private static readonly Font commentFont = new Font("Consolas", 10, FontStyle.Italic);

        private static readonly Color valueForeColor = Color.FromArgb(255, 255, 60);
        private static readonly Font valueFont = new Font("Consolas", 10, FontStyle.Bold);

        private static readonly Color stringForeColor = Color.FromArgb(255, 192, 144);

        private readonly TextElementStyle CommentStyle = new TextElementStyle();
        private readonly TextElementStyle ValueStyle = new TextElementStyle();
        private readonly TextElementStyle StringStyle = new TextElementStyle();

        private sealed class StyleSelector : JsonTerminalSymbolVisitor<TextElementStyle>
        {
            private readonly SettingsTextBox owner;

            public StyleSelector(SettingsTextBox owner)
            {
                this.owner = owner;
            }

            public override TextElementStyle VisitComment(JsonComment symbol) => owner.CommentStyle;
            public override TextElementStyle VisitErrorString(JsonErrorString symbol) => owner.StringStyle;
            public override TextElementStyle VisitString(JsonString symbol) => owner.StringStyle;
            public override TextElementStyle VisitUnterminatedMultiLineComment(JsonUnterminatedMultiLineComment symbol) => owner.CommentStyle;
            public override TextElementStyle VisitValue(JsonValue symbol) => owner.ValueStyle;
        }

        private readonly SettingsFile settingsFile;

        private readonly UpdatableRichTextBox errorsTextBox;

        /// <summary>
        /// Initializes a new instance of a <see cref="SettingsTextBox"/>.
        /// </summary>
        /// <param name="settingsFile">
        /// The settings file to show and/or edit.
        /// </param>
        /// <param name="errorsTextBox">
        /// An optional <see cref="UpdatableRichTextBox"/> which displays JSON parse errors.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="settingsFile"/> is null.
        /// </exception>
        public SettingsTextBox(SettingsFile settingsFile, UpdatableRichTextBox errorsTextBox)
        {
            this.settingsFile = settingsFile ?? throw new ArgumentNullException(nameof(settingsFile));
            this.errorsTextBox = errorsTextBox;

            errorsTextBox.HideSelection = false;

            BorderStyle = BorderStyle.None;

            DefaultStyle.BackColor = defaultBackColor;
            DefaultStyle.ForeColor = defaultForeColor;
            DefaultStyle.Font = defaultFont;

            CommentStyle.ForeColor = commentForeColor;
            CommentStyle.Font = commentFont;

            ValueStyle.ForeColor = valueForeColor;
            ValueStyle.Font = valueFont;

            StringStyle.ForeColor = stringForeColor;

            // Set the Text property and use that as input, because it will not exactly match the json string.
            Text = File.ReadAllText(settingsFile.AbsoluteFilePath);

            if (errorsTextBox != null)
            {
                errorsTextBox.ReadOnly = true;
                errorsTextBox.DoubleClick += ErrorsTextBox_DoubleClick;
                errorsTextBox.KeyDown += ErrorsTextBox_KeyDown;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            using (var updateToken = BeginUpdateRememberState())
            {
                ParseAndApplySyntaxHighlighting(Text);
            }

            TextChanged += SettingsTextBox_TextChanged;
        }

        private void ParseAndApplySyntaxHighlighting(string json)
        {
            lastParsedText = json;

            ApplyDefaultStyle();

            int firstUnusedIndex = 0;

            TextIndex.Clear();

            var parser = new SettingReader(json);
            parser.Tokens.ForEach(x =>
            {
                if (firstUnusedIndex < x.Start)
                {
                    // Since whitespace is not returned from TokenizeAll().
                    int length = x.Start - firstUnusedIndex;
                    TextIndex.AppendTerminalSymbol(new JsonTextElement(
                        JsonWhitespace.Value,
                        json,
                        firstUnusedIndex,
                        length));
                }

                TextIndex.AppendTerminalSymbol(x);
                firstUnusedIndex = x.Start + x.Length;
            });

            if (firstUnusedIndex < json.Length)
            {
                // Since whitespace is not returned from TokenizeAll().
                int length = json.Length - firstUnusedIndex;
                TextIndex.AppendTerminalSymbol(new JsonTextElement(
                    JsonWhitespace.Value,
                    json,
                    firstUnusedIndex,
                    length));
            }

            var styleSelector = new StyleSelector(this);

            foreach (var textElement in TextIndex.Elements)
            {
                ApplyStyle(textElement, styleSelector.Visit(textElement.TerminalSymbol));
            }

            parser.TryParse(out PMap dummy, out List<TextErrorInfo> errors);

            if (errors.Count == 0)
            {
                DisplayNoErrors();
            }
            else
            {
                DisplayErrors(errors);
            }
        }

        private string lastParsedText;

        private void SettingsTextBox_TextChanged(object sender, EventArgs e)
        {
            // Only parse and analyze errors if the text actually changed, not just the style.
            string newText = Text;
            if (lastParsedText != newText)
            {
                using (var updateToken = BeginUpdateRememberState())
                {
                    ParseAndApplySyntaxHighlighting(newText);
                }
            }
        }

        private List<TextErrorInfo> currentErrors;

        private void DisplayNoErrors()
        {
            currentErrors = null;

            if (errorsTextBox != null)
            {
                using (var updateToken = errorsTextBox.BeginUpdate())
                {
                    errorsTextBox.Text = "(No errors)";
                    errorsTextBox.BackColor = defaultBackColor;
                    errorsTextBox.ForeColor = noErrorsForeColor;
                    errorsTextBox.Font = noErrorsFont;
                }
            }
        }

        private void DisplayErrors(List<TextErrorInfo> errors)
        {
            currentErrors = errors;

            foreach (var error in errors)
            {
                Select(error.Start, error.Length);
                SetErrorUnderline();
            }

            if (errorsTextBox != null)
            {
                using (var updateToken = errorsTextBox.BeginUpdate())
                {
                    var errorMessages = from error in errors
                                        let lineIndex = GetLineFromCharIndex(error.Start)
                                        let position = error.Start - GetFirstCharIndexFromLine(lineIndex)
                                        select $"{error.Message} at line {lineIndex + 1}, position {position + 1}";

                    errorsTextBox.Text = string.Join("\n", errorMessages);

                    errorsTextBox.BackColor = defaultBackColor;
                    errorsTextBox.ForeColor = defaultForeColor;
                    errorsTextBox.Font = errorsFont;
                }
            }
        }

        private void BringErrorIntoView(int charIndex)
        {
            // Select the text that generated the error.
            if (currentErrors != null)
            {
                int lineIndex = errorsTextBox.GetLineFromCharIndex(charIndex);
                if (0 <= lineIndex && lineIndex < currentErrors.Count)
                {
                    // Determine how many lines are visible in the top half of the control.
                    int firstVisibleLine = GetLineFromCharIndex(GetCharIndexFromPosition(Point.Empty));
                    int bottomVisibleLine = GetLineFromCharIndex(GetCharIndexFromPosition(new Point(0, ClientSize.Height - 1)));
                    int visibleLines = bottomVisibleLine - firstVisibleLine;

                    // Then calculate which line should become the first visible line
                    // so the error line ends up in the middle of the control.
                    var hotError = currentErrors[lineIndex];
                    int hotErrorLine = GetLineFromCharIndex(hotError.Start);

                    // Delay repaints while fooling around with SelectionStart.
                    using (var updateToken = BeginUpdate())
                    {
                        // hotErrorLine in view?
                        // Don't include the bottom line, it's likely not completely visible.
                        if (hotErrorLine < firstVisibleLine || bottomVisibleLine <= hotErrorLine)
                        {
                            int targetFirstVisibleLine = hotErrorLine - (visibleLines / 2);
                            if (targetFirstVisibleLine < 0) targetFirstVisibleLine = 0;

                            Select(TextLength, 0);
                            ScrollToCaret();
                            Select(GetFirstCharIndexFromLine(targetFirstVisibleLine), 0);
                            ScrollToCaret();
                        }

                        Select(hotError.Start, hotError.Length);
                    }

                    Focus();
                }
            }
        }

        private void ErrorsTextBox_DoubleClick(object sender, EventArgs e)
        {
            BringErrorIntoView(errorsTextBox.GetCharIndexFromPosition(errorsTextBox.PointToClient(MousePosition)));
        }

        private void ErrorsTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                BringErrorIntoView(errorsTextBox.SelectionStart);
            }
        }
    }
}

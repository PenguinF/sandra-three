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

        private static readonly TextElementStyle defaultStyle = new TextElementStyle()
        {
            HasBackColor = true,
            BackColor = Color.FromArgb(16, 16, 16),
            HasForeColor = true,
            ForeColor = Color.WhiteSmoke,
            Font = new Font("Consolas", 10),
        };

        protected override TextElementStyle DefaultStyle => defaultStyle;

        private static readonly Color noErrorsForeColor = Color.FromArgb(255, 176, 176, 176);
        private static readonly Font noErrorsFont = new Font("Calibri", 10f, FontStyle.Italic);
        private static readonly Font errorsFont = new Font("Calibri", 10f);
        private static readonly Color errorBackColor = Color.FromArgb(255, 72, 72);

        private static readonly TextElementStyle commentStyle = new TextElementStyle()
        {
            HasForeColor = true,
            ForeColor = Color.FromArgb(128, 220, 220),
            Font = new Font("Consolas", 10, FontStyle.Italic),
        };

        private static readonly TextElementStyle valueStyle = new TextElementStyle()
        {
            HasForeColor = true,
            ForeColor = Color.FromArgb(255, 255, 60),
            Font = new Font("Consolas", 10, FontStyle.Bold),
        };

        private static readonly TextElementStyle stringStyle = new TextElementStyle()
        {
            HasForeColor = true,
            ForeColor = Color.FromArgb(255, 192, 144),
        };

        private sealed class StyleSelector : JsonTerminalSymbolVisitor<TextElementStyle>
        {
            public override TextElementStyle VisitComment(JsonComment symbol) => commentStyle;
            public override TextElementStyle VisitErrorString(JsonErrorString symbol) => stringStyle;
            public override TextElementStyle VisitString(JsonString symbol) => stringStyle;
            public override TextElementStyle VisitUnterminatedMultiLineComment(JsonUnterminatedMultiLineComment symbol) => commentStyle;
            public override TextElementStyle VisitValue(JsonValue symbol) => valueStyle;
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
            if (settingsFile == null) throw new ArgumentNullException(nameof(settingsFile));
            this.settingsFile = settingsFile;
            this.errorsTextBox = errorsTextBox;

            BorderStyle = BorderStyle.None;

            // Set the Text property and use that as input, because it will not exactly match the json string.
            Text = File.ReadAllText(settingsFile.AbsoluteFilePath);

            if (errorsTextBox != null)
            {
                errorsTextBox.ReadOnly = true;
                errorsTextBox.Click += ErrorsTextBox_Click;
                errorsTextBox.KeyDown += ErrorsTextBox_KeyDown;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            using (var updateToken = BeginUpdateRememberState())
            {
                parseAndApplySyntaxHighlighting(Text);
            }

            TextChanged += SettingsTextBox_TextChanged;
        }

        private void parseAndApplySyntaxHighlighting(string json)
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

            var styleSelector = new StyleSelector();

            foreach (var textElement in TextIndex.Elements)
            {
                ApplyStyle(textElement, styleSelector.Visit(textElement.TerminalSymbol));
            }

            PMap dummy;
            List<TextErrorInfo> errors;
            parser.TryParse(out dummy, out errors);

            if (errors.Count == 0)
            {
                displayNoErrors();
            }
            else
            {
                displayErrors(errors);
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
                    parseAndApplySyntaxHighlighting(newText);
                }
            }
        }

        private List<TextErrorInfo> currentErrors;

        private void displayNoErrors()
        {
            if (errorsTextBox != null)
            {
                currentErrors = null;
                currentSelectedError = null;

                using (var updateToken = errorsTextBox.BeginUpdate())
                {
                    errorsTextBox.Text = "(No errors)";
                    errorsTextBox.BackColor = defaultStyle.BackColor;
                    errorsTextBox.ForeColor = noErrorsForeColor;
                    errorsTextBox.Font = noErrorsFont;
                }
            }
        }

        private void displayErrors(List<TextErrorInfo> errors)
        {
            if (errorsTextBox != null)
            {
                currentErrors = errors;
                currentSelectedError = null;

                foreach (var error in errors)
                {
                    Select(error.Start, error.Length);
                    SetErrorUnderline();
                }

                using (var updateToken = errorsTextBox.BeginUpdate())
                {
                    var errorMessages = from error in errors
                                        let lineIndex = GetLineFromCharIndex(error.Start)
                                        let position = error.Start - GetFirstCharIndexFromLine(lineIndex)
                                        select $"{error.Message} at line {lineIndex}, position {position}";

                    errorsTextBox.Text = string.Join("\n", errorMessages);

                    errorsTextBox.BackColor = defaultStyle.BackColor;
                    errorsTextBox.ForeColor = defaultStyle.ForeColor;
                    errorsTextBox.Font = errorsFont;
                }
            }
        }

        private TextErrorInfo currentSelectedError;

        private void resetSelectedErrorText()
        {
            if (currentSelectedError != null)
            {
                using (var updateToken = BeginUpdateRememberState())
                {
                    Select(currentSelectedError.Start, currentSelectedError.Length);
                    SelectionBackColor = defaultStyle.BackColor;
                }

                currentSelectedError = null;
            }
        }

        private void selectErrorText(int charIndex)
        {
            var oldSelectedError = currentSelectedError;

            // Reset the old error first.
            resetSelectedErrorText();

            // Select the text that generated the error.
            if (currentErrors != null)
            {
                int lineIndex = errorsTextBox.GetLineFromCharIndex(charIndex);
                if (0 <= lineIndex && lineIndex < currentErrors.Count)
                {
                    // Only show selected error if it's different.
                    // This way, if an error is clicked twice, its style gets deselected again.
                    if (oldSelectedError == null || !oldSelectedError.EqualTo(currentErrors[lineIndex]))
                    {
                        currentSelectedError = currentErrors[lineIndex];

                        using (var updateToken = errorsTextBox.BeginUpdateRememberState())
                        {
                            Select(currentSelectedError.Start, currentSelectedError.Length);
                            SelectionBackColor = errorBackColor;
                        }
                    }
                }
            }
        }

        private void ErrorsTextBox_Click(object sender, EventArgs e)
        {
            selectErrorText(errorsTextBox.GetCharIndexFromPosition(errorsTextBox.PointToClient(MousePosition)));
        }

        private void ErrorsTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                selectErrorText(errorsTextBox.SelectionStart);
            }
        }
    }
}

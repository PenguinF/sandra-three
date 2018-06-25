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
using SysExtensions.SyntaxRenderer;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents a Windows rich text box which displays a json settings file.
    /// </summary>
    public partial class SettingsTextBox : RichTextBoxBase
    {
        /// <summary>
        /// Because the syntax renderer does not support discontinuous terminal symbols.
        /// </summary>
        private class JsonWhitespace : JsonTerminalSymbol
        {
            public JsonWhitespace(string json, int start, int length) : base(json, start, length) { }

            public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.DefaultVisit(this);
            public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.DefaultVisit(this);
        }

        private readonly TextElementStyle defaultStyle = new TextElementStyle()
        {
            HasBackColor = true,
            BackColor = Color.FromArgb(16, 16, 16),
            HasForeColor = true,
            ForeColor = Color.WhiteSmoke,
            Font = new Font("Consolas", 10),
        };

        private sealed class StyleSelector : JsonTerminalSymbolVisitor<TextElementStyle>
        {
            private readonly TextElementStyle commentStyle = new TextElementStyle()
            {
                HasForeColor = true,
                ForeColor = Color.FromArgb(128, 220, 220),
                Font = new Font("Consolas", 10, FontStyle.Italic),
            };

            private readonly TextElementStyle valueStyle = new TextElementStyle()
            {
                HasForeColor = true,
                ForeColor = Color.FromArgb(255, 255, 60),
                Font = new Font("Consolas", 10, FontStyle.Bold),
            };

            private readonly TextElementStyle stringStyle = new TextElementStyle()
            {
                HasForeColor = true,
                ForeColor = Color.FromArgb(255, 192, 144),
            };

            private readonly TextElementStyle errorStyle = new TextElementStyle()
            {
                HasForeColor = true,
                ForeColor = Color.FromArgb(255, 72, 72),
                Font = new Font("Consolas", 10, FontStyle.Underline),
            };

            public override TextElementStyle VisitComment(JsonComment symbol) => commentStyle;
            public override TextElementStyle VisitString(JsonString symbol) => stringStyle;
            public override TextElementStyle VisitUnknownSymbol(JsonUnknownSymbol symbol) => errorStyle;
            public override TextElementStyle VisitValue(JsonValue symbol) => valueStyle;
        }

        private readonly SettingsFile settingsFile;

        private readonly SyntaxRenderer<JsonTerminalSymbol> syntaxRenderer;

        private void applyDefaultStyle()
        {
            using (var updateToken = BeginUpdateRememberCaret())
            {
                BackColor = defaultStyle.BackColor;
                ForeColor = defaultStyle.ForeColor;
                Font = defaultStyle.Font;
                SelectAll();
                SelectionBackColor = defaultStyle.BackColor;
                SelectionColor = defaultStyle.ForeColor;
                SelectionFont = defaultStyle.Font;
            }
        }

        private void applyStyle(TextElement<JsonTerminalSymbol> element, TextElementStyle style)
        {
            if (style != null)
            {
                using (var updateToken = BeginUpdateRememberCaret())
                {
                    Select(element.Start, element.Length);
                    if (style.HasBackColor) SelectionBackColor = style.BackColor;
                    if (style.HasForeColor) SelectionColor = style.ForeColor;
                    if (style.HasFont) SelectionFont = style.Font;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="SettingsTextBox"/>.
        /// </summary>
        /// <param name="settingsFile">
        /// The settings file to show and/or edit.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="settingsFile"/> is null.
        /// </exception>
        public SettingsTextBox(SettingsFile settingsFile)
        {
            if (settingsFile == null) throw new ArgumentNullException(nameof(settingsFile));
            this.settingsFile = settingsFile;

            BorderStyle = BorderStyle.None;
            syntaxRenderer = SyntaxRenderer<JsonTerminalSymbol>.AttachTo(this, isSlave: true);

            // Set the Text property and use that as input, because it will not exactly match the json string.
            Text = File.ReadAllText(settingsFile.AbsoluteFilePath);

            applyDefaultStyle();
            tokenize(Text);
            applySyntaxHighlighting();

            TextChanged += SettingsTextBox_TextChanged;
        }

        private void tokenize(string json)
        {
            int firstUnusedIndex = 0;

            syntaxRenderer.Clear();
            new JsonTokenizer(json).TokenizeAll().ForEach(x =>
            {
                if (firstUnusedIndex < x.Start)
                {
                        // Since whitespace is not returned from TokenizeAll().
                        int length = x.Start - firstUnusedIndex;
                    syntaxRenderer.AppendTerminalSymbol(
                        new JsonWhitespace(json, firstUnusedIndex, length),
                        length);
                }
                syntaxRenderer.AppendTerminalSymbol(x, x.Length);
                firstUnusedIndex = x.Start + x.Length;
            });

            if (firstUnusedIndex < json.Length)
            {
                // Since whitespace is not returned from TokenizeAll().
                int length = json.Length - firstUnusedIndex;
                syntaxRenderer.AppendTerminalSymbol(
                    new JsonWhitespace(json, firstUnusedIndex, length),
                    length);
            }
        }

        private void applySyntaxHighlighting()
        {
            using (var updateToken = BeginUpdateRememberCaret())
            {
                applyDefaultStyle();

                var styleSelector = new StyleSelector();

                foreach (var textElement in syntaxRenderer.Elements)
                {
                    applyStyle(textElement, styleSelector.Visit(textElement.TerminalSymbol));
                }
            }
        }

        protected override void OnSelectionChanged(EventArgs e)
        {
            // Swallow updates to the caret position.
            using (var updateToken = BeginUpdate())
            {
                base.OnSelectionChanged(e);
            }
        }

        private void SettingsTextBox_TextChanged(object sender, EventArgs e)
        {
            tokenize(Text);
            applySyntaxHighlighting();
        }
    }
}

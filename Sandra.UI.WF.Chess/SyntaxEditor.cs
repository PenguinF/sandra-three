#region License
/*********************************************************************************
 * RichTextBoxBase.cs
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
using SysExtensions.TextIndex;
using System.Drawing;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents a Windows rich text box with syntax highlighting.
    /// </summary>
    public abstract class SyntaxEditor<TTerminal> : RichTextBoxBase
    {
        protected sealed class TextElementStyle
        {
            public bool HasBackColor { get; set; }
            public Color BackColor { get; set; }

            public bool HasForeColor { get; set; }
            public Color ForeColor { get; set; }

            public bool HasFont => Font != null;
            public Font Font { get; set; }
        }

        protected readonly TextIndex<TTerminal> TextIndex;

        protected abstract TextElementStyle DefaultStyle { get; }

        public SyntaxEditor()
        {
            TextIndex = new TextIndex<TTerminal>();

            int zoomFactor;
            if (Program.TryGetAutoSaveValue(SettingKeys.Zoom, out zoomFactor))
            {
                ZoomFactor = PType.RichTextZoomFactor.FromDiscreteZoomFactor(zoomFactor);
            }
        }

        protected void ApplyDefaultStyle()
        {
            var defaultStyle = DefaultStyle;

            if (defaultStyle != null)
            {
                using (var updateToken = BeginUpdateRememberState())
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
        }

        protected void ApplyStyle(TextElement<TTerminal> element, TextElementStyle style)
        {
            if (style != null)
            {
                using (var updateToken = BeginUpdateRememberState())
                {
                    Select(element.Start, element.Length);
                    if (style.HasBackColor) SelectionBackColor = style.BackColor;
                    if (style.HasForeColor) SelectionColor = style.ForeColor;
                    if (style.HasFont) SelectionFont = style.Font;
                }
            }
        }

        protected override void OnZoomFactorChanged(ZoomFactorChangedEventArgs e)
        {
            base.OnZoomFactorChanged(e);
            Program.AutoSave.Persist(SettingKeys.Zoom, e.ZoomFactor);
        }
    }
}

﻿#region License
/*********************************************************************************
 * SyntaxEditor.cs
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
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents a Windows rich text box with syntax highlighting.
    /// </summary>
    public abstract partial class SyntaxEditor<TTerminal> : UpdatableRichTextBox, IUIActionHandlerProvider
    {
        protected sealed class TextElementStyle
        {
            public Color BackColor { get; set; }
            public Color ForeColor { get; set; }
            public Font Font { get; private set; }

            public void ApplyFont(Font value) => Font = value;
        }

        /// <summary>
        /// Gets the action handler for this control.
        /// </summary>
        public UIActionHandler ActionHandler { get; } = new UIActionHandler();

        protected readonly TextIndex<TTerminal> TextIndex;

        protected TextElementStyle DefaultStyle { get; } = new TextElementStyle();

        public SyntaxEditor()
        {
            TextIndex = new TextIndex<TTerminal>();

            if (Program.TryGetAutoSaveValue(SettingKeys.Zoom, out int zoomFactor))
            {
                Zoom = PType.RichTextZoomFactor.FromDiscreteZoomFactor(zoomFactor);
            }
        }

        protected void StyleClearAll()
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
                    if (style.BackColor.A > 0) SelectionBackColor = style.BackColor;
                    if (style.ForeColor.A > 0) SelectionColor = style.ForeColor;
                    if (style.Font != null) SelectionFont = style.Font;
                }
            }
        }

        /// <summary>
        /// Occurs when the zoom factor of this <see cref="SyntaxEditor{TTerminal}"/> is updated.
        /// </summary>
        public event EventHandler<ZoomFactorChangedEventArgs> ZoomFactorChanged;

        /// <summary>
        /// Raises the <see cref="ZoomFactorChanged"/> event.
        /// </summary>
        /// <param name="e">
        /// The data for the event.
        /// </param>
        protected virtual void OnZoomFactorChanged(ZoomFactorChangedEventArgs e)
        {
            ZoomFactorChanged?.Invoke(this, e);
        }

        private void RaiseZoomFactorChanged(ZoomFactorChangedEventArgs e)
        {
            // Not only raise the event, but also save the zoom factor setting.
            OnZoomFactorChanged(e);
            Program.AutoSave.Persist(SettingKeys.Zoom, e.ZoomFactor);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (ModifierKeys.HasFlag(Keys.Control))
            {
                // ZoomFactor isn't updated yet, so predict here what it's going to be.
                int newZoomFactorPrediction = PType.RichTextZoomFactor.ToDiscreteZoomFactor(Zoom) + Math.Sign(e.Delta);
                RaiseZoomFactorChanged(new ZoomFactorChangedEventArgs(newZoomFactorPrediction));
            }
        }

        /// <summary>
        /// Binds the regular cut/copy/paste/select all UIActions to this textbox.
        /// </summary>
        public void BindStandardEditUIActions()
        {
            this.BindActions(new UIActionBindings
            {
                { SharedUIAction.ZoomIn, TryZoomIn },
                { SharedUIAction.ZoomOut, TryZoomOut },

                { SharedUIAction.CutSelectionToClipBoard, TryCutSelectionToClipBoard },
                { SharedUIAction.CopySelectionToClipBoard, TryCopySelectionToClipBoard },
                { SharedUIAction.PasteSelectionFromClipBoard, TryPasteSelectionFromClipBoard },
                { SharedUIAction.SelectAllText, TrySelectAllText },
            });
        }

        public void DeleteRange(int textStart, int textLength)
        {
            if (textStart >= TextLength || textLength <= 0) return;

            if (textStart < 0) textStart = 0;
            if (textLength > TextLength) textLength = TextLength;

            Select(textStart, textLength);
            SelectedText = string.Empty;
        }

        public void GotoPosition(int caretPosition)
        {
            Select(caretPosition, 0);
            ScrollToCaret();
        }

        public void ClearAll() => Clear();

        public int SelectionEnd
        {
            get => SelectionStart + SelectionLength;
            set => Select(SelectionStart, value - SelectionStart);
        }

        public int LineFromPosition(int position) => GetLineFromCharIndex(position);

        public int FirstVisibleLine
        {
            get => GetLineFromCharIndex(GetCharIndexFromPosition(Point.Empty));
            set
            {
                GotoPosition(TextLength);
                GotoPosition(GetFirstCharIndexFromLine(value));
            }
        }

        public int LinesOnScreen => GetLineFromCharIndex(GetCharIndexFromPosition(new Point(0, ClientSize.Height - 1))) - FirstVisibleLine;

        public int GetColumn(int position) => position - GetFirstCharIndexFromLine(GetLineFromCharIndex(position));

        public float Zoom { get => ZoomFactor; set => ZoomFactor = value; }
    }

    /// <summary>
    /// Provides data for the <see cref="SyntaxEditor{TTerminal}.ZoomFactorChanged"/> event.
    /// </summary>
    public class ZoomFactorChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the new zoom factor, represented as an integer in the range [-9..649].
        /// </summary>
        public int ZoomFactor { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZoomFactorChangedEventArgs"/> class.
        /// </summary>
        /// <param name="zoomFactor">
        /// The new zoom factor, represented as an integer in the range [-9..649].
        /// </param>
        public ZoomFactorChangedEventArgs(int zoomFactor)
        {
            ZoomFactor = zoomFactor;
        }
    }
}

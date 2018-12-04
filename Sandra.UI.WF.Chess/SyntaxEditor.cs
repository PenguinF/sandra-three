#region License
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
using ScintillaNET;
using SysExtensions.TextIndex;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents a <see cref="Scintilla"/> control with syntax highlighting, a number of <see cref="UIAction"/> hooks,
    /// and a mouse-wheel event handler.
    /// </summary>
    public abstract partial class SyntaxEditor<TTerminal> : Scintilla, IUIActionHandlerProvider
    {
        /// <summary>
        /// Gets the action handler for this control.
        /// </summary>
        public UIActionHandler ActionHandler { get; } = new UIActionHandler();

        protected readonly TextIndex<TTerminal> TextIndex;

        protected Style DefaultStyle => Styles[Style.Default];

        public SyntaxEditor()
        {
            TextIndex = new TextIndex<TTerminal>();

            Margins.ForEach(x => x.Width = 0);

            if (Program.TryGetAutoSaveValue(SettingKeys.Zoom, out int zoomFactor))
            {
                Zoom = zoomFactor;
            }
        }

        protected void ApplyStyle(TextElement<TTerminal> element, Style style)
        {
            if (style != null)
            {
                StartStyling(element.Start);
                SetStyling(element.Length, style.Index);
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
    }

    public static class ScintillaExtensions
    {
        public static void ApplyFont(this Style style, Font font)
        {
            style.Font = font.FontFamily.Name;
            style.SizeF = font.Size;
            style.Bold = font.Bold;
            style.Italic = font.Italic;
            style.Underline = font.Underline;
        }
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

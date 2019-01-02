#region License
/*********************************************************************************
 * SyntaxEditor.cs
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

using Sandra.UI.WF.Storage;
using ScintillaNET;
using SysExtensions;
using SysExtensions.Text;
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

        /// <summary>
        /// Gets the back color of this syntax editor in areas where no style or syntax highlighting is applied.
        /// </summary>
        public Color NoStyleBackColor => DefaultStyle.BackColor;

        /// <summary>
        /// Gets the back color of this syntax editor in areas where no style or syntax highlighting is applied.
        /// </summary>
        public Color NoStyleForeColor => DefaultStyle.ForeColor;

        public SyntaxEditor()
        {
            TextIndex = new TextIndex<TTerminal>();

            UsePopup(false);

            BufferedDraw = false;
            Technology = Technology.DirectWrite;

            HScrollBar = false;
            VScrollBar = true;

            Margins.ForEach(x => x.Width = 0);

            //https://notepad-plus-plus.org/community/topic/12576/list-of-all-assigned-keyboard-shortcuts/8
            //https://scintilla.org/CommandValues.html
            //https://sourceforge.net/p/scintilla/code/ci/default/tree/src/KeyMap.h
            //https://sourceforge.net/p/scintilla/code/ci/default/tree/src/KeyMap.cxx
            ClearCmdKey(Keys.Control | Keys.F);
            ClearCmdKey(Keys.Control | Keys.H);
            ClearCmdKey(Keys.Control | Keys.L);
            ClearCmdKey(Keys.Control | Keys.R);
            ClearCmdKey(Keys.Control | Keys.U);

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
                // Zoom isn't updated yet, so predict here what it's going to be.
                int newZoomFactorPrediction = Zoom + Math.Sign(e.Delta);
                if (ScintillaZoomFactor.MinDiscreteValue <= newZoomFactorPrediction
                    && newZoomFactorPrediction <= ScintillaZoomFactor.MaxDiscreteValue)
                {
                    RaiseZoomFactorChanged(new ZoomFactorChangedEventArgs(newZoomFactorPrediction));
                }
            }
        }

        /// <summary>
        /// Binds the regular cut/copy/paste/select all UIActions to this textbox.
        /// </summary>
        public void BindStandardEditUIActions()
        {
            var editBindings = new UIActionBindings
            {
                { SharedUIAction.Undo, TryUndo },
                { SharedUIAction.Redo, TryRedo },

                { SharedUIAction.ZoomIn, TryZoomIn },
                { SharedUIAction.ZoomOut, TryZoomOut },

                { SharedUIAction.CutSelectionToClipBoard, TryCutSelectionToClipBoard },
                { SharedUIAction.CopySelectionToClipBoard, TryCopySelectionToClipBoard },
                { SharedUIAction.PasteSelectionFromClipBoard, TryPasteSelectionFromClipBoard },
                { SharedUIAction.SelectAllText, TrySelectAllText },
            };

            editBindings
                .Where(x => x.Binding.DefaultBinding.Shortcuts != null)
                .SelectMany(x => x.Binding.DefaultBinding.Shortcuts)
                .Select(KeyUtils.ToKeys)
                .ForEach(ClearCmdKey);

            ClearCmdKey(Keys.Oemplus | Keys.Shift | Keys.Control);
            ClearCmdKey(Keys.OemMinus | Keys.Control);

            this.BindActions(editBindings);
        }
    }

    /// <summary>
    /// Contains extension methods to interface between standard System.Windows.Forms and Scintilla classes.
    /// </summary>
    public static class ScintillaExtensions
    {
        /// <summary>
        /// Copies a <see cref="Font"/> definition to a Scintilla <see cref="Style"/>.
        /// </summary>
        /// <param name="font">
        /// The font to copy.
        /// </param>
        /// <param name="style">
        /// The style to copy to.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="font"/> and/or <paramref name="style"/> are null.
        /// </exception>
        public static void CopyTo(this Font font, Style style)
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
        /// Gets the new zoom factor, represented as an integer in the range [-10..20].
        /// </summary>
        public int ZoomFactor { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZoomFactorChangedEventArgs"/> class.
        /// </summary>
        /// <param name="zoomFactor">
        /// The new zoom factor, represented as an integer in the range [-10..20].
        /// </param>
        public ZoomFactorChangedEventArgs(int zoomFactor)
        {
            ZoomFactor = zoomFactor;
        }
    }

    public sealed class ScintillaZoomFactor : PType.Derived<PInteger, int>
    {
        /// <summary>
        /// Returns the minimum recommended value for the zoom factor
        /// of a <see cref="Scintilla"/> control which is between -10 and 20, endpoints included.
        /// </summary>
        public static readonly int MinDiscreteValue = -10;

        /// <summary>
        /// Returns the maximum recommended value for the zoom factor
        /// of a <see cref="Scintilla"/> control which is between -10 and 20, endpoints included.
        /// </summary>
        public static readonly int MaxDiscreteValue = 20;

        public static readonly ScintillaZoomFactor Instance = new ScintillaZoomFactor();

        private ScintillaZoomFactor() : base(new PType.RangedInteger(MinDiscreteValue, MaxDiscreteValue)) { }

        public override Union<ITypeErrorBuilder, int> TryGetTargetValue(PInteger integer)
            => ValidValue((int)integer.Value);

        public override PInteger GetBaseValue(int value) => new PInteger(value);
    }
}

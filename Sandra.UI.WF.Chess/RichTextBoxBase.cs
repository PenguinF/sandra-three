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
using System;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents a Windows rich text box with a number of application-wide features
    /// such as <see cref="UIAction"/> hooks and a mouse-wheel event handler.
    /// </summary>
    public partial class RichTextBoxBase : UpdatableRichTextBox, IUIActionHandlerProvider
    {
        /// <summary>
        /// Gets the action handler for this control.
        /// </summary>
        public UIActionHandler ActionHandler { get; } = new UIActionHandler();

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (ModifierKeys.HasFlag(Keys.Control))
            {
                // ZoomFactor isn't updated yet, so predict here what it's going to be.
                int newZoomFactorPrediction = PType.RichTextZoomFactor.ToDiscreteZoomFactor(ZoomFactor) + Math.Sign(e.Delta);
                OnZoomFactorChanged(new ZoomFactorChangedEventArgs(newZoomFactorPrediction));
            }
        }

        /// <summary>
        /// Occurs when the zoom factor of this <see cref="RichTextBox"/> is updated.
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

        /// <summary>
        /// Binds the regular cut/copy/paste/select all UIActions to this textbox.
        /// </summary>
        public void BindStandardEditUIActions()
        {
            this.BindActions(new UIActionBindings
            {
                { SharedUIAction.ZoomIn, TryZoomIn },
                { SharedUIAction.ZoomOut, TryZoomOut },

                { CutSelectionToClipBoard, TryCutSelectionToClipBoard },
                { CopySelectionToClipBoard, TryCopySelectionToClipBoard },
                { PasteSelectionFromClipBoard, TryPasteSelectionFromClipBoard },
                { SelectAllText, TrySelectAllText },
            });
        }
    }

    /// <summary>
    /// Provides data for the <see cref="RichTextBoxBase.ZoomFactorChanged"/> event.
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

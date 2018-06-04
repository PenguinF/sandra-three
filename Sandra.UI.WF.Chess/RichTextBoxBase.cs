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
using Sandra.UI.WF.Storage;
using System;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents a Windows rich text box with a number of applicatio-wide features
    /// such as <see cref="UIAction"/> hooks and a mouse-wheel event handler.
    /// </summary>
    public partial class RichTextBoxBase : UpdatableRichTextBox, IUIActionHandlerProvider
    {
        public RichTextBoxBase()
        {
            int zoomFactor;
            if (Program.TryGetAutoSaveValue(SettingKeys.Zoom, out zoomFactor))
            {
                ZoomFactor = PType.RichTextZoomFactor.FromDiscreteZoomFactor(zoomFactor);
            }
        }

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
                autoSaveZoomFactor(PType.RichTextZoomFactor.ToDiscreteZoomFactor(ZoomFactor) + Math.Sign(e.Delta));
            }
        }

        private void autoSaveZoomFactor(int zoomFactor)
        {
            Program.AutoSave.Persist(SettingKeys.Zoom, zoomFactor);
        }
    }
}

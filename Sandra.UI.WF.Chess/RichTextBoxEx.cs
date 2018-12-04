#region License
/*********************************************************************************
 * RichTextBoxEx.cs
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

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents a Windows rich text box which exposes a number of <see cref="UIAction"/> hooks.
    /// </summary>
    public partial class RichTextBoxEx : UpdatableRichTextBox, IUIActionHandlerProvider
    {
        /// <summary>
        /// Gets the action handler for this control.
        /// </summary>
        public UIActionHandler ActionHandler { get; } = new UIActionHandler();

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
}

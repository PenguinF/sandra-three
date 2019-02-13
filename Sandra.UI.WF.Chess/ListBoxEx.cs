#region License
/*********************************************************************************
 * ListBoxEx.cs
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

using Eutherion.Win.UIActions;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Represents a Windows list box which exposes a number of <see cref="UIAction"/> hooks.
    /// </summary>
    public partial class ListBoxEx : ListBox, IUIActionHandlerProvider
    {
        /// <summary>
        /// Gets the action handler for this control.
        /// </summary>
        public UIActionHandler ActionHandler { get; } = new UIActionHandler();

        /// <summary>
        /// Binds the regular copy/select-all UIActions to this listbox.
        /// </summary>
        public void BindStandardCopySelectUIActions()
        {
            this.BindActions(new UIActionBindings
            {
                { SharedUIAction.CopySelectionToClipBoard, TryCopySelectionToClipBoard },
                { SharedUIAction.SelectAllText, TrySelectAllText },
            });
        }
    }
}

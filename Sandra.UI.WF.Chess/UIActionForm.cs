#region License
/*********************************************************************************
 * UIActionForm.cs
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
using System;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Top level Form which ties in with the UIAction framework.
    /// </summary>
    public class UIActionForm : Form, IUIActionHandlerProvider
    {
        /// <summary>
        /// Gets the action handler for this control.
        /// </summary>
        public UIActionHandler ActionHandler { get; } = new UIActionHandler();

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            try
            {
                // This code makes shortcuts work for all UIActionHandlers.
                return KeyUtils.TryExecute(keyData) || base.ProcessCmdKey(ref msg, keyData);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return true;
            }
        }
    }
}

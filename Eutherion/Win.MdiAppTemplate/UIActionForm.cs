#region License
/*********************************************************************************
 * UIActionForm.cs
 *
 * Copyright (c) 2004-2020 Henk Nicolai
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

using Eutherion.UIActions;
using Eutherion.Win.Forms;
using Eutherion.Win.UIActions;
using System;
using System.Windows.Forms;

namespace Eutherion.Win.MdiAppTemplate
{
    /// <summary>
    /// Top level <see cref="Form"/> which ties in with the UIAction framework.
    /// </summary>
    public class UIActionForm : ConstrainedMoveResizeForm, IUIActionHandlerProvider
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
                return UIActionUtilities.TryExecute(keyData, FocusHelper.GetFocusedControl())
                    || base.ProcessCmdKey(ref msg, keyData);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return true;
            }
        }

        /// <summary>
        /// Gets the regular UIActions for this Form.
        /// </summary>
        public UIActionBindings StandardUIActionBindings => new UIActionBindings
        {
            { SharedUIAction.Close, TryClose },
        };

        /// <summary>
        /// Binds the regular UIActions to this Form.
        /// </summary>
        public void BindStandardUIActions() => this.BindActions(StandardUIActionBindings);

        public UIActionState TryClose(bool perform)
        {
            if (perform) Close();
            return UIActionVisibility.Enabled;
        }
    }
}

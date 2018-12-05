#region License
/*********************************************************************************
 * SettingsForm.UIActions.cs
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
    public partial class SettingsForm
    {
        public UIActionState TryGoToPreviousLocation(bool perform)
        {
            if (errorsListBox == null) return UIActionVisibility.Hidden;

            int errorCount = settingsTextBox.CurrentErrorCount;
            if (errorCount == 0) return UIActionVisibility.Disabled;

            if (perform)
            {
                // Go to previous or last position.
                int targetIndex = errorsListBox.SelectedIndex - 1;
                if (targetIndex < 0) targetIndex = errorCount - 1;
                errorsListBox.ClearSelected();
                errorsListBox.SelectedIndex = targetIndex;
                ActivateSelectedError();
            }

            return UIActionVisibility.Enabled;
        }

        public UIActionState TryGoToNextLocation(bool perform)
        {
            if (errorsListBox == null) return UIActionVisibility.Hidden;

            int errorCount = settingsTextBox.CurrentErrorCount;
            if (errorCount == 0) return UIActionVisibility.Disabled;

            if (perform)
            {
                // Go to next or first position.
                int targetIndex = errorsListBox.SelectedIndex + 1;
                if (targetIndex >= errorCount) targetIndex = 0;
                errorsListBox.ClearSelected();
                errorsListBox.SelectedIndex = targetIndex;
                ActivateSelectedError();
            }

            return UIActionVisibility.Enabled;
        }
    }
}

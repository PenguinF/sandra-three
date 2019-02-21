#region License
/*********************************************************************************
 * ListBoxEx.UIActions.cs
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

using Eutherion.UIActions;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Sandra.UI
{
    public partial class ListBoxEx
    {
        public UIActionState TryCopySelectionToClipBoard(bool perform)
        {
            if (SelectedItems.Count == 0) return UIActionVisibility.Disabled;

            if (perform)
            {
                // Copy to a list explictly first because SelectedObjectCollection only has a non-generic enumerator.
                List<object> selectedItems = new List<object>();
                foreach (object item in SelectedItems) selectedItems.Add(item);
                Clipboard.SetText(string.Join(Environment.NewLine, selectedItems) + Environment.NewLine);
            }

            return UIActionVisibility.Enabled;
        }

        public UIActionState TrySelectAllText(bool perform)
        {
            if (Items.Count == 0) return UIActionVisibility.Disabled;

            if (perform)
            {
                SelectedIndices.Clear();
                for (int i = 0; i < Items.Count; i++) SelectedIndices.Add(i);
            }

            return UIActionVisibility.Enabled;
        }
    }
}

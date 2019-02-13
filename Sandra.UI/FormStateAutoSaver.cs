#region License
/*********************************************************************************
 * FormStateAutoSaver.cs
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

using Eutherion.Win.Storage;
using System;
using System.Windows.Forms;

namespace Sandra.UI
{
    internal class FormStateAutoSaver
    {
        private readonly SettingProperty<PersistableFormState> autoSaveProperty;
        private readonly PersistableFormState formState;

        public FormStateAutoSaver(
            Form targetForm,
            SettingProperty<PersistableFormState> autoSaveProperty,
            PersistableFormState formState)
        {
            this.autoSaveProperty = autoSaveProperty;
            this.formState = formState;

            // Attach only after restoring.
            formState.AttachTo(targetForm);

            // This object goes out of scope when FormState goes out of scope,
            // which is when the target Form is closed.
            formState.Changed += FormState_Changed;
        }

        private void FormState_Changed(object sender, EventArgs e)
        {
            Program.AutoSave.Persist(autoSaveProperty, formState);
        }
    }
}

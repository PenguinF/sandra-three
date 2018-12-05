#region License
/*********************************************************************************
 * SettingsForm.cs
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    public class SettingsForm : UIActionForm
    {
        private static readonly Font noErrorsFont = new Font("Calibri", 10, FontStyle.Italic);
        private static readonly Font errorsFont = new Font("Calibri", 10);

        private readonly SettingProperty<PersistableFormState> formStateSetting;
        private readonly SettingProperty<int> errorHeightSetting;

        private readonly SplitContainer splitter;
        private readonly ListBoxEx errorsTextBox;
        private readonly SettingsTextBox settingsTextBox;

        public SettingsForm(bool isReadOnly,
                            SettingsFile settingsFile,
                            SettingProperty<PersistableFormState> formStateSetting,
                            SettingProperty<int> errorHeightSetting)
        {
            this.formStateSetting = formStateSetting;
            this.errorHeightSetting = errorHeightSetting;

            Text = Path.GetFileName(settingsFile.AbsoluteFilePath);

            settingsTextBox = new SettingsTextBox(settingsFile)
            {
                Dock = DockStyle.Fill,
                ReadOnly = isReadOnly,
            };

            settingsTextBox.BindActions(new UIActionBindings
            {
                { SettingsTextBox.SaveToFile, settingsTextBox.TrySaveToFile },
            });

            settingsTextBox.BindStandardEditUIActions();

            UIMenu.AddTo(settingsTextBox);

            // If there is no errorsTextBox, splitter will remain null,
            // and no splitter distance needs to be restored or auto-saved.
            if (settingsTextBox.ReadOnly && settingsTextBox.CurrentErrorCount == 0)
            {
                // No errors while read-only: do not display an errors textbox.
                Controls.Add(settingsTextBox);
            }
            else
            {
                errorsTextBox = new ListBoxEx
                {
                    Dock = DockStyle.Fill,
                    BorderStyle = BorderStyle.None,
                    HorizontalScrollbar = false,
                    ItemHeight = 14,
                    SelectionMode = SelectionMode.MultiExtended,
                };

                errorsTextBox.BindStandardCopySelectUIActions();

                UIMenu.AddTo(errorsTextBox);

                // Interaction between settingsTextBox and errorsTextBox.
                settingsTextBox.CurrentErrorsChanged += (_, __) => DisplayErrors();

                // Do an initial DisplayErrors() as well, because settingsTextBox might already contain errors.
                DisplayErrors();

                errorsTextBox.DoubleClick += (_, __) => ActivateSelectedError();
                errorsTextBox.KeyDown += ErrorsTextBox_KeyDown;

                splitter = new SplitContainer
                {
                    Dock = DockStyle.Fill,
                    SplitterWidth = 4,
                    FixedPanel = FixedPanel.Panel2,
                    Orientation = Orientation.Horizontal,
                };

                splitter.Panel1.Controls.Add(settingsTextBox);
                splitter.Panel2.Controls.Add(errorsTextBox);

                // Copy background color.
                errorsTextBox.BackColor = settingsTextBox.NoStyleBackColor;
                splitter.Panel2.BackColor = settingsTextBox.NoStyleBackColor;

                Controls.Add(splitter);
            }
        }

        private void ErrorsTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                ActivateSelectedError();
            }
        }

        private void ActivateSelectedError()
        {
            var index = errorsTextBox.SelectedIndex;
            if (0 <= index && index < errorsTextBox.Items.Count)
            {
                settingsTextBox.ActivateError(index);
            }
        }

        private void DisplayErrors()
        {
            errorsTextBox.Items.Clear();

            if (settingsTextBox.CurrentErrorCount == 0)
            {
                errorsTextBox.Items.Add("(No errors)");
                errorsTextBox.ForeColor = settingsTextBox.LineNumberForeColor;
                errorsTextBox.Font = noErrorsFont;
            }
            else
            {
                var errors = settingsTextBox.CurrentErrors;

                var errorMessages = from error in errors
                                    let lineIndex = settingsTextBox.LineFromPosition(error.Start)
                                    let position = settingsTextBox.GetColumn(error.Start)
                                    select $"{error.Message} at line {lineIndex + 1}, position {position + 1}";

                errorsTextBox.Items.AddRange(errorMessages.ToArray());
                errorsTextBox.ForeColor = settingsTextBox.NoStyleForeColor;
                errorsTextBox.Font = errorsFont;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            // Default value shows about 2 errors at default zoom level.
            const int defaultErrorHeight = 34;

            Program.AttachFormStateAutoSaver(this, formStateSetting, null);

            if (splitter != null && errorsTextBox != null)
            {
                if (!Program.TryGetAutoSaveValue(errorHeightSetting, out int targetErrorHeight))
                {
                    targetErrorHeight = defaultErrorHeight;
                }

                // Calculate target splitter distance which will restore the target error height exactly.
                int splitterDistance = ClientSize.Height - targetErrorHeight - splitter.SplitterWidth;
                if (splitterDistance >= 0) splitter.SplitterDistance = splitterDistance;

                splitter.SplitterMoved += (_, __) => Program.AutoSave.Persist(errorHeightSetting, errorsTextBox.Height);
            }
        }
    }
}

#region License
/*********************************************************************************
 * SettingsForm.cs
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

using Sandra.UI.WF.Storage;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    public partial class SettingsForm : UIActionForm
    {
        private static readonly Font noErrorsFont = new Font("Calibri", 10, FontStyle.Italic);
        private static readonly Font normalFont = new Font("Calibri", 10);

        private readonly SettingProperty<PersistableFormState> formStateSetting;
        private readonly SettingProperty<int> errorHeightSetting;

        private readonly SplitContainer splitter;
        private readonly ListBoxEx errorsListBox;
        private readonly SettingsTextBox settingsTextBox;

        private readonly LocalizedString noErrorsString;
        private readonly LocalizedString errorLocationString;

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

            settingsTextBox.BindActions(new UIActionBindings
            {
                { SharedUIAction.GoToPreviousLocation, TryGoToPreviousLocation },
                { SharedUIAction.GoToNextLocation, TryGoToNextLocation },
            });

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
                errorsListBox = new ListBoxEx
                {
                    Dock = DockStyle.Fill,
                    BorderStyle = BorderStyle.None,
                    HorizontalScrollbar = false,
                    ItemHeight = 14,
                    SelectionMode = SelectionMode.MultiExtended,
                };

                errorsListBox.BindStandardCopySelectUIActions();

                errorsListBox.BindActions(new UIActionBindings
                {
                    { SharedUIAction.GoToPreviousLocation, TryGoToPreviousLocation },
                    { SharedUIAction.GoToNextLocation, TryGoToNextLocation },
                });

                UIMenu.AddTo(errorsListBox);

                // Interaction between settingsTextBox and errorsTextBox.
                settingsTextBox.CurrentErrorsChanged += (_, __) => DisplayErrors();

                // Assume that if this display text changes, that of errorLocationString changes too.
                noErrorsString = new LocalizedString(LocalizedStringKeys.NoErrorsMessage);

                // Does an initial DisplayErrors() as well, because settingsTextBox might already contain errors.
                errorLocationString = new LocalizedString(LocalizedStringKeys.ErrorLocation);
                errorLocationString.DisplayText.ValueChanged += _ => DisplayErrors();

                errorsListBox.DoubleClick += (_, __) => ActivateSelectedError();
                errorsListBox.KeyDown += ErrorsListBox_KeyDown;

                splitter = new SplitContainer
                {
                    Dock = DockStyle.Fill,
                    SplitterWidth = 4,
                    FixedPanel = FixedPanel.Panel2,
                    Orientation = Orientation.Horizontal,
                };

                splitter.Panel1.Controls.Add(settingsTextBox);
                splitter.Panel2.Controls.Add(errorsListBox);

                // Copy background color.
                errorsListBox.BackColor = settingsTextBox.NoStyleBackColor;
                splitter.Panel2.BackColor = settingsTextBox.NoStyleBackColor;

                Controls.Add(splitter);
            }
        }

        private void ErrorsListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                ActivateSelectedError();
            }
        }

        private void ActivateSelectedError()
        {
            var index = errorsListBox.SelectedIndex;
            if (0 <= index && index < errorsListBox.Items.Count)
            {
                settingsTextBox.ActivateError(index);
            }
        }

        private void DisplayErrors()
        {
            errorsListBox.BeginUpdate();

            try
            {
                if (settingsTextBox.CurrentErrorCount == 0)
                {
                    errorsListBox.Items.Clear();
                    errorsListBox.Items.Add(noErrorsString.DisplayText.Value);
                    errorsListBox.ForeColor = settingsTextBox.LineNumberForeColor;
                    errorsListBox.Font = noErrorsFont;
                }
                else
                {
                    var errorMessages = (from error in settingsTextBox.CurrentErrors
                                         let lineIndex = (settingsTextBox.LineFromPosition(error.Start) + 1).ToString(CultureInfo.InvariantCulture)
                                         let position = (settingsTextBox.GetColumn(error.Start) + 1).ToString(CultureInfo.InvariantCulture)
                                         // Instead of using errorLocationString.DisplayText.Value,
                                         // use the current localizer to format the localized string.
                                         select Localizer.Current.Localize(
                                             errorLocationString.Key,
                                             new[] { error.Message(Localizer.Current), lineIndex, position })).ToArray();

                    int oldItemCount = errorsListBox.Items.Count;
                    var newErrorCount = errorMessages.Length;
                    int index = 0;

                    while (index < errorsListBox.Items.Count && index < newErrorCount)
                    {
                        // Copy to existing index so scroll position is maintained.
                        errorsListBox.Items[index] = errorMessages[index];
                        index++;
                    }

                    // Remove excess old items.
                    for (int k = oldItemCount - 1; index <= k; k--)
                    {
                        errorsListBox.Items.RemoveAt(k);
                    }

                    while (index < newErrorCount)
                    {
                        errorsListBox.Items.Add(errorMessages[index]);
                        index++;
                    }

                    errorsListBox.ForeColor = settingsTextBox.NoStyleForeColor;
                    errorsListBox.Font = normalFont;
                }
            }
            finally
            {
                errorsListBox.EndUpdate();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            // Default value shows about 2 errors at default zoom level.
            const int defaultErrorHeight = 34;

            Program.AttachFormStateAutoSaver(this, formStateSetting, null);

            if (splitter != null && errorsListBox != null)
            {
                if (!Program.TryGetAutoSaveValue(errorHeightSetting, out int targetErrorHeight))
                {
                    targetErrorHeight = defaultErrorHeight;
                }

                // Calculate target splitter distance which will restore the target error height exactly.
                int splitterDistance = ClientSize.Height - targetErrorHeight - splitter.SplitterWidth;
                if (splitterDistance >= 0) splitter.SplitterDistance = splitterDistance;

                splitter.SplitterMoved += (_, __) => Program.AutoSave.Persist(errorHeightSetting, errorsListBox.Height);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                noErrorsString?.Dispose();
                errorLocationString?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

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

using Eutherion.Localization;
using Eutherion.UIActions;
using Eutherion.Win.AppTemplate;
using Eutherion.Win.Storage;
using Eutherion.Win.UIActions;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Sandra.UI
{
    public partial class SettingsForm : UIActionForm
    {
        private const string ChangedMarker = "• ";

        private static readonly Font noErrorsFont = new Font("Calibri", 10, FontStyle.Italic);
        private static readonly Font normalFont = new Font("Calibri", 10);

        private readonly SettingProperty<PersistableFormState> formStateSetting;
        private readonly SettingProperty<int> errorHeightSetting;

        private readonly UIAutoHideMainMenu autoHideMainMenu;
        private readonly UIAutoHideMainMenuItem langMenu;
        private readonly UIAutoHideMainMenuItem fileMenu;
        private readonly UIAutoHideMainMenuItem editMenu;
        private readonly UIAutoHideMainMenuItem viewMenu;
        private readonly UIAutoHideMainMenuItem helpMenu;
        private readonly UIAutoHideMainMenuItem developerToolsMenu;

        private readonly SplitContainer splitter;
        private readonly ListBoxEx errorsListBox;
        private readonly JsonTextBox jsonTextBox;

        private readonly LocalizedString noErrorsString;
        private readonly LocalizedString errorLocationString;

        private readonly string fileName;

        public SettingsForm(bool isReadOnly,
                            SettingsFile settingsFile,
                            SettingProperty<PersistableFormState> formStateSetting,
                            SettingProperty<int> errorHeightSetting)
        {
            this.formStateSetting = formStateSetting;
            this.errorHeightSetting = errorHeightSetting;

            fileName = Path.GetFileName(settingsFile.AbsoluteFilePath);
            Text = fileName;

            jsonTextBox = new JsonTextBox(settingsFile)
            {
                Dock = DockStyle.Fill,
                ReadOnly = isReadOnly,
            };

            jsonTextBox.BindActions(new UIActionBindings
            {
                { JsonTextBox.SaveToFile, jsonTextBox.TrySaveToFile },
            });

            jsonTextBox.BindStandardEditUIActions();

            jsonTextBox.BindActions(new UIActionBindings
            {
                { SharedUIAction.GoToPreviousLocation, TryGoToPreviousLocation },
                { SharedUIAction.GoToNextLocation, TryGoToNextLocation },
            });

            UIMenu.AddTo(jsonTextBox);

            // If there is no errorsTextBox, splitter will remain null,
            // and no splitter distance needs to be restored or auto-saved.
            if (jsonTextBox.ReadOnly && jsonTextBox.CurrentErrorCount == 0)
            {
                // No errors while read-only: do not display an errors textbox.
                Controls.Add(jsonTextBox);
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

                // Save points.
                jsonTextBox.SavePointLeft += (_, __) => Text = ChangedMarker + fileName;
                jsonTextBox.SavePointReached += (_, __) => Text = fileName;

                // Interaction between settingsTextBox and errorsTextBox.
                jsonTextBox.CurrentErrorsChanged += (_, __) => DisplayErrors();

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

                splitter.Panel1.Controls.Add(jsonTextBox);
                splitter.Panel2.Controls.Add(errorsListBox);

                // Copy background color.
                errorsListBox.BackColor = jsonTextBox.NoStyleBackColor;
                splitter.Panel2.BackColor = jsonTextBox.NoStyleBackColor;

                Controls.Add(splitter);
            }

            // Initialize menu strip which becomes visible only when the ALT key is pressed.
            autoHideMainMenu = new UIAutoHideMainMenu(this);

            langMenu = autoHideMainMenu.AddMenuItem(null, Properties.Resources.globe);
            Localizers.Registered.ForEach(x => langMenu.BindAction(x.SwitchToLangUIActionBinding, alwaysVisible: false));

            fileMenu = autoHideMainMenu.AddMenuItem(LocalizedStringKeys.File);
            fileMenu.BindActions(
                ToolForms.EditPreferencesFile,
                ToolForms.ShowDefaultSettingsFile);

            editMenu = autoHideMainMenu.AddMenuItem(LocalizedStringKeys.Edit);
            editMenu.BindActions(
                SharedUIAction.Undo,
                SharedUIAction.Redo,
                SharedUIAction.CutSelectionToClipBoard,
                SharedUIAction.CopySelectionToClipBoard,
                SharedUIAction.PasteSelectionFromClipBoard,
                SharedUIAction.SelectAllText);

            viewMenu = autoHideMainMenu.AddMenuItem(LocalizedStringKeys.View);
            viewMenu.BindActions(
                SharedUIAction.ZoomIn,
                SharedUIAction.ZoomOut);

            developerToolsMenu = autoHideMainMenu.AddMenuItem(LocalizedStringKeys.DeveloperTools);
            developerToolsMenu.BindAction(ToolForms.EditCurrentLanguage, alwaysVisible: false);

            helpMenu = autoHideMainMenu.AddMenuItem(LocalizedStringKeys.Help);
            helpMenu.BindActions(
                ToolForms.OpenAbout,
                ToolForms.ShowCredits);

            // Implemtations for global UIActions.
            if (Localizers.Registered.Count() >= 2)
            {
                // More than one localizer: can switch between them.
                foreach (var localizer in Localizers.Registered)
                {
                    this.BindAction(localizer.SwitchToLangUIActionBinding, localizer.TrySwitchToLang);
                }
            }

            this.BindAction(ToolForms.EditPreferencesFile, ToolForms.TryEditPreferencesFile(this));
            this.BindAction(ToolForms.ShowDefaultSettingsFile, ToolForms.TryShowDefaultSettingsFile(this));
            this.BindAction(ToolForms.OpenAbout, ToolForms.TryOpenAbout(this));
            this.BindAction(ToolForms.ShowCredits, ToolForms.TryShowCredits(this));
            this.BindAction(ToolForms.EditCurrentLanguage, ToolForms.TryEditCurrentLanguage(this));
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
                jsonTextBox.ActivateError(index);
            }
        }

        private void DisplayErrors()
        {
            errorsListBox.BeginUpdate();

            try
            {
                if (jsonTextBox.CurrentErrorCount == 0)
                {
                    errorsListBox.Items.Clear();
                    errorsListBox.Items.Add(noErrorsString.DisplayText.Value);
                    errorsListBox.ForeColor = jsonTextBox.LineNumberForeColor;
                    errorsListBox.Font = noErrorsFont;
                }
                else
                {
                    var errorMessages = (from error in jsonTextBox.CurrentErrors
                                         let lineIndex = (jsonTextBox.LineFromPosition(error.Start) + 1).ToString(CultureInfo.InvariantCulture)
                                         let position = (jsonTextBox.GetColumn(error.Start) + 1).ToString(CultureInfo.InvariantCulture)
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

                    errorsListBox.ForeColor = jsonTextBox.NoStyleForeColor;
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

            Session.Current.AttachFormStateAutoSaver(this, formStateSetting, null);

            if (splitter != null && errorsListBox != null)
            {
                if (!Session.Current.TryGetAutoSaveValue(errorHeightSetting, out int targetErrorHeight))
                {
                    targetErrorHeight = defaultErrorHeight;
                }

                // Calculate target splitter distance which will restore the target error height exactly.
                int splitterDistance = ClientSize.Height - targetErrorHeight - splitter.SplitterWidth;
                if (splitterDistance >= 0) splitter.SplitterDistance = splitterDistance;

                splitter.SplitterMoved += (_, __) => Session.Current.AutoSave.Persist(errorHeightSetting, errorsListBox.Height);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Menu | Keys.Alt))
            {
                autoHideMainMenu.ToggleMainMenu();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                autoHideMainMenu.Dispose();
                noErrorsString?.Dispose();
                errorLocationString?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

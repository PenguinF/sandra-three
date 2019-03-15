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

using Eutherion.UIActions;
using Eutherion.Utils;
using Eutherion.Win.Storage;
using Eutherion.Win.UIActions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Eutherion.Win.AppTemplate
{
    public class SettingsForm : MenuCaptionBarForm, IWeakEventTarget
    {
        private const string ChangedMarker = "• ";

        private static readonly Font noErrorsFont = new Font("Calibri", 10, FontStyle.Italic);
        private static readonly Font normalFont = new Font("Calibri", 10);

        private readonly SettingProperty<PersistableFormState> formStateSetting;
        private readonly SettingProperty<int> errorHeightSetting;

        private readonly UIActionHandler mainMenuActionHandler;

        private readonly SplitContainer splitter;
        private readonly ListBoxEx errorsListBox;
        private readonly JsonTextBox jsonTextBox;

        private readonly LocalizedString noErrorsString;
        private readonly LocalizedString errorLocationString;

        public SettingsForm(bool isReadOnly,
                            SettingsFile settingsFile,
                            Func<string> initialTextGenerator,
                            SettingProperty<PersistableFormState> formStateSetting,
                            SettingProperty<int> errorHeightSetting,
                            SettingProperty<AutoSaveFileNamePair> autoSaveSetting)
        {
            this.formStateSetting = formStateSetting;
            this.errorHeightSetting = errorHeightSetting;

            // Set this before calling UpdateChangedMarker().
            UnsavedModificationsCloseButtonHoverColor = Color.FromArgb(0xff, 0xc0, 0xc0);

            jsonTextBox = new JsonTextBox(settingsFile, initialTextGenerator, autoSaveSetting)
            {
                Dock = DockStyle.Fill,
                ReadOnly = isReadOnly,
            };

            jsonTextBox.BindActions(new UIActionBindings
            {
                { SharedUIAction.SaveToFile, jsonTextBox.TrySaveToFile },
            });

            // Bind to this MenuCaptionBarForm as well so the save button is shown in the caption area.
            this.BindAction(SharedUIAction.SaveToFile, jsonTextBox.TrySaveToFile);

            jsonTextBox.BindStandardEditUIActions();

            jsonTextBox.BindActions(new UIActionBindings
            {
                { SharedUIAction.GoToPreviousLocation, TryGoToPreviousLocation },
                { SharedUIAction.GoToNextLocation, TryGoToNextLocation },
            });

            UIMenu.AddTo(jsonTextBox);

            // Initial changed marker.
            UpdateChangedMarker();

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
                jsonTextBox.SavePointLeft += (_, __) => UpdateChangedMarker();
                jsonTextBox.SavePointReached += (_, __) => UpdateChangedMarker();
                jsonTextBox.WorkingCopyTextFile.OpenTextFile.FileUpdated += OpenTextFile_FileUpdated;

                // Interaction between settingsTextBox and errorsTextBox.
                jsonTextBox.CurrentErrorsChanged += (_, __) => DisplayErrors();

                // Assume that if this display text changes, that of errorLocationString changes too.
                noErrorsString = new LocalizedString(SharedLocalizedStringKeys.NoErrorsMessage);

                // Does an initial DisplayErrors() as well, because settingsTextBox might already contain errors.
                errorLocationString = new LocalizedString(SharedLocalizedStringKeys.ErrorLocation);
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
                errorsListBox.BackColor = DefaultSyntaxEditorStyle.BackColor;
                splitter.Panel2.BackColor = DefaultSyntaxEditorStyle.BackColor;

                Controls.Add(splitter);
            }

            BindStandardUIActions();

            // Initialize menu strip.
            mainMenuActionHandler = new UIActionHandler();

            var fileMenu = new UIMenuNode.Container(SharedLocalizedStringKeys.File.ToTextProvider());
            fileMenu.Nodes.AddRange(BindMainMenuItemActions(
                SharedUIAction.SaveToFile,
                SharedUIAction.Close));

            var editMenu = new UIMenuNode.Container(SharedLocalizedStringKeys.Edit.ToTextProvider());
            editMenu.Nodes.AddRange(BindMainMenuItemActions(
                SharedUIAction.Undo,
                SharedUIAction.Redo,
                SharedUIAction.CutSelectionToClipBoard,
                SharedUIAction.CopySelectionToClipBoard,
                SharedUIAction.PasteSelectionFromClipBoard,
                SharedUIAction.SelectAllText));

            var viewMenu = new UIMenuNode.Container(SharedLocalizedStringKeys.View.ToTextProvider());
            viewMenu.Nodes.AddRange(BindMainMenuItemActions(
                SharedUIAction.ZoomIn,
                SharedUIAction.ZoomOut));

            MainMenuStrip = new MenuStrip();
            UIMenuBuilder.BuildMenu(mainMenuActionHandler, new[] { fileMenu, editMenu, viewMenu }, MainMenuStrip.Items);
            Controls.Add(MainMenuStrip);
            MainMenuStrip.BackColor = DefaultSyntaxEditorStyle.ForeColor;

            foreach (ToolStripDropDownItem mainMenuItem in MainMenuStrip.Items)
            {
                mainMenuItem.DropDownOpening += MainMenuItem_DropDownOpening;
            }

            Session.Current.CurrentLocalizerChanged += CurrentLocalizerChanged;
        }

        private void CurrentLocalizerChanged(object sender, EventArgs e)
        {
            if (MainMenuStrip != null)
            {
                UIMenu.UpdateMenu(MainMenuStrip.Items);
            }
        }

        private List<UIMenuNode> BindMainMenuItemActions(params DefaultUIActionBinding[] bindings)
        {
            var menuNodes = new List<UIMenuNode>();

            foreach (var binding in bindings)
            {
                if (binding.DefaultInterfaces.TryGet(out IContextMenuUIActionInterface contextMenuInterface))
                {
                    menuNodes.Add(new UIMenuNode.Element(binding.Action, contextMenuInterface));

                    mainMenuActionHandler.BindAction(new UIActionBinding(binding, perform =>
                    {
                        try
                        {
                            // Try to find a UIActionHandler that is willing to validate/perform the given action.
                            foreach (var actionHandler in UIActionUtilities.EnumerateUIActionHandlers(FocusHelper.GetFocusedControl()))
                            {
                                UIActionState currentActionState = actionHandler.TryPerformAction(binding.Action, perform);
                                if (currentActionState.UIActionVisibility != UIActionVisibility.Parent)
                                {
                                    return currentActionState.UIActionVisibility == UIActionVisibility.Hidden
                                        ? UIActionVisibility.Disabled
                                        : currentActionState;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }

                        // No handler in the chain that processes the UIAction actively, so set to disabled.
                        return UIActionVisibility.Disabled;
                    }));
                }
            }

            return menuNodes;
        }

        private void MainMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            var mainMenuItem = (ToolStripMenuItem)sender;

            foreach (var menuItem in mainMenuItem.DropDownItems.OfType<UIActionToolStripMenuItem>())
            {
                menuItem.Update(mainMenuActionHandler.TryPerformAction(menuItem.Action, false));
            }
        }

        private void OpenTextFile_FileUpdated(LiveTextFile sender, EventArgs e)
        {
            UpdateChangedMarker();
        }

        private void UpdateChangedMarker()
        {
            string openTextFilePath = jsonTextBox.WorkingCopyTextFile.OpenTextFilePath;
            string fileName = Path.GetFileName(openTextFilePath);
            Text = jsonTextBox.ContainsChanges ? ChangedMarker + fileName : fileName;

            // Invalidate to update the save button.
            ActionHandler.Invalidate();
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
                    errorsListBox.ForeColor = DefaultSyntaxEditorStyle.LineNumberForeColor;
                    errorsListBox.Font = noErrorsFont;
                }
                else
                {
                    var errorMessages = (from error in jsonTextBox.CurrentErrors
                                         let lineIndex = (jsonTextBox.LineFromPosition(error.Start) + 1).ToString(CultureInfo.InvariantCulture)
                                         let position = (jsonTextBox.GetColumn(error.Start) + 1).ToString(CultureInfo.InvariantCulture)
                                         // Instead of using errorLocationString.DisplayText.Value,
                                         // use the current localizer to format the localized string.
                                         select Session.Current.CurrentLocalizer.Localize(
                                             errorLocationString.Key,
                                             new[] { error.Message(Session.Current.CurrentLocalizer), lineIndex, position })).ToArray();

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

                    errorsListBox.ForeColor = DefaultSyntaxEditorStyle.ForeColor;
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Only show message box if there's no auto save file from which local changes can be recovered.
            if (jsonTextBox.ContainsChanges && jsonTextBox.WorkingCopyTextFile.AutoSaveFile == null)
            {
                string openTextFilePath = jsonTextBox.WorkingCopyTextFile.OpenTextFilePath;
                string fileName = Path.GetFileName(openTextFilePath);

                DialogResult result = MessageBox.Show(
                    Session.Current.CurrentLocalizer.Localize(SharedLocalizedStringKeys.SaveChangesQuery, new[] { fileName }),
                    Session.Current.CurrentLocalizer.Localize(SharedLocalizedStringKeys.UnsavedChangesTitle),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button3);

                switch (result)
                {
                    case DialogResult.Yes:
                        try
                        {
                            ActionHandler.TryPerformAction(SharedUIAction.SaveToFile.Action, true);
                        }
                        catch (Exception exception)
                        {
                            e.Cancel = true;
                            MessageBox.Show(exception.Message);
                        }
                        break;
                    case DialogResult.No:
                        break;
                    default:
                        e.Cancel = true;
                        break;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                jsonTextBox.WorkingCopyTextFile.OpenTextFile.FileUpdated -= OpenTextFile_FileUpdated;
                noErrorsString?.Dispose();
                errorLocationString?.Dispose();
            }

            base.Dispose(disposing);
        }

        public UIActionState TryGoToPreviousLocation(bool perform)
        {
            if (errorsListBox == null) return UIActionVisibility.Hidden;

            int errorCount = jsonTextBox.CurrentErrorCount;
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

            int errorCount = jsonTextBox.CurrentErrorCount;
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

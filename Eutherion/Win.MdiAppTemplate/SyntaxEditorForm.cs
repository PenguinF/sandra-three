﻿#region License
/*********************************************************************************
 * SyntaxEditorForm.cs
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
using Eutherion.Utils;
using Eutherion.Win.Storage;
using Eutherion.Win.UIActions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Eutherion.Win.MdiAppTemplate
{
    public class SyntaxEditorForm<TSyntaxTree, TTerminal, TError> : MenuCaptionBarForm, IWeakEventTarget
    {
        private class ErrorListPanel : Panel, IDockableControl
        {
            private static readonly Font noErrorsFont = new Font("Calibri", 10, FontStyle.Italic);
            private static readonly Font normalFont = new Font("Calibri", 10);

            private readonly SyntaxEditorForm<TSyntaxTree, TTerminal, TError> OwnerEditorForm;

            private readonly ListBoxEx errorsListBox;

            private readonly LocalizedString noErrorsString;
            private readonly LocalizedString errorLocationString;
            private readonly LocalizedString titleString;

            public DockProperties DockProperties { get; } = new DockProperties();

            public event Action DockPropertiesChanged;

            public ErrorListPanel(SyntaxEditorForm<TSyntaxTree, TTerminal, TError> ownerEditorForm)
            {
                OwnerEditorForm = ownerEditorForm;

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
                    { SharedUIAction.GoToPreviousError, OwnerEditorForm.TryGoToPreviousError },
                    { SharedUIAction.GoToNextError, OwnerEditorForm.TryGoToNextError },
                });

                UIMenu.AddTo(errorsListBox);

                // Assume that if this display text changes, that of errorLocationString changes too.
                noErrorsString = new LocalizedString(SharedLocalizedStringKeys.NoErrorsMessage);
                errorLocationString = new LocalizedString(SharedLocalizedStringKeys.ErrorLocation);
                titleString = new LocalizedString(SharedLocalizedStringKeys.ErrorPaneTitle);

                noErrorsString.DisplayText.ValueChanged += _ => DisplayErrors(OwnerEditorForm.SyntaxEditor);
                errorLocationString.DisplayText.ValueChanged += _ => DisplayErrors(OwnerEditorForm.SyntaxEditor);
                titleString.DisplayText.ValueChanged += _ => UpdateText();

                errorsListBox.DoubleClick += (_, __) => OwnerEditorForm.ActivateSelectedError(errorsListBox.SelectedIndex);
                errorsListBox.KeyDown += ErrorsListBox_KeyDown;

                Controls.Add(errorsListBox);
            }

            protected override void OnBackColorChanged(EventArgs e)
            {
                // Blend background colors.
                errorsListBox.BackColor = BackColor;
                base.OnBackColorChanged(e);
            }

            public void UpdateText()
            {
                DockProperties.CaptionText = StringUtilities.ConditionalFormat(titleString.DisplayText.Value, new[] { OwnerEditorForm.CodeFilePathDisplayString });
                DockPropertiesChanged?.Invoke();
            }

            public int SelectedErrorIndex
            {
                get => errorsListBox.SelectedIndex;
                set
                {
                    errorsListBox.ClearSelected();
                    errorsListBox.SelectedIndex = value;
                }
            }

            public void DisplayErrors(SyntaxEditor<TSyntaxTree, TTerminal, TError> syntaxEditor)
            {
                errorsListBox.BeginUpdate();

                try
                {
                    if (syntaxEditor.CurrentErrors.Count == 0)
                    {
                        errorsListBox.Items.Clear();
                        errorsListBox.Items.Add(noErrorsString.DisplayText.Value);
                        errorsListBox.ForeColor = DefaultSyntaxEditorStyle.LineNumberForeColor;
                        errorsListBox.Font = noErrorsFont;
                    }
                    else
                    {
                        var errorMessages = (from error in syntaxEditor.CurrentErrors
                                             let errorStart = syntaxEditor.SyntaxDescriptor.GetErrorRange(error).Item1
                                             let errorMessage = syntaxEditor.SyntaxDescriptor.GetErrorMessage(error)
                                             let lineIndex = (syntaxEditor.LineFromPosition(errorStart) + 1).ToStringInvariant()
                                             let position = (syntaxEditor.GetColumn(errorStart) + 1).ToStringInvariant()
                                             // Instead of using errorLocationString.DisplayText.Value,
                                             // use the current localizer to format the localized string.
                                             let fullErrorMessage = Session.Current.CurrentLocalizer.Localize(
                                                 errorLocationString.Key,
                                                 new[] { errorMessage, lineIndex, position })
                                             select Session.Current.CurrentLocalizer.ToSentenceCase(fullErrorMessage)).ToArray();

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

            private void ErrorsListBox_KeyDown(object sender, KeyEventArgs e)
            {
                if (e.KeyData == Keys.Enter)
                {
                    OwnerEditorForm.ActivateSelectedError(errorsListBox.SelectedIndex);
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    noErrorsString.Dispose();
                    errorLocationString.Dispose();
                    titleString.Dispose();
                }

                base.Dispose(disposing);
            }
        }

        private const string ChangedMarker = "• ";

        private readonly Box<Form> errorListFormBox = new Box<Form>();

        private readonly LocalizedString untitledString;

        public SyntaxEditor<TSyntaxTree, TTerminal, TError> SyntaxEditor { get; }

        public SyntaxEditorForm(SyntaxEditorCodeAccessOption codeAccessOption,
                                SyntaxDescriptor<TSyntaxTree, TTerminal, TError> syntaxDescriptor,
                                WorkingCopyTextFile codeFile,
                                SettingProperty<int> zoomSetting)
        {
            SyntaxEditor = new SyntaxEditor<TSyntaxTree, TTerminal, TError>(syntaxDescriptor, codeFile)
            {
                Dock = DockStyle.Fill,
                ReadOnly = codeAccessOption == SyntaxEditorCodeAccessOption.ReadOnly,
            };

            // Initialize zoom factor and listen to changes.
            if (Session.Current.TryGetAutoSaveValue(zoomSetting, out int zoomFactor))
            {
                SyntaxEditor.Zoom = zoomFactor;
            }

            SyntaxEditor.ZoomFactorChanged += (_, e) => Session.Current.AutoSave.Persist(zoomSetting, e.ZoomFactor);

            SyntaxEditor.BindActions(new UIActionBindings
            {
                { SharedUIAction.SaveToFile, SyntaxEditor.TrySaveToFile },
            });

            if (codeAccessOption == SyntaxEditorCodeAccessOption.Default)
            {
                SyntaxEditor.BindActions(new UIActionBindings
                {
                    { SharedUIAction.SaveAs, SyntaxEditor.TrySaveAs },
                });
            }

            SyntaxEditor.BindStandardEditUIActions();

            SyntaxEditor.BindActions(new UIActionBindings
            {
                { SharedUIAction.ShowErrorPane, TryShowErrorPane },
                { SharedUIAction.GoToPreviousError, TryGoToPreviousError },
                { SharedUIAction.GoToNextError, TryGoToNextError },
            });

            UIMenu.AddTo(SyntaxEditor);

            // Changed marker.
            untitledString = new LocalizedString(SharedLocalizedStringKeys.Untitled);
            untitledString.DisplayText.ValueChanged += _ =>
            {
                UpdateChangedMarker();
                if (errorListFormBox.Value is MenuCaptionBarForm<ErrorListPanel> errorListForm)
                {
                    errorListForm.DockedControl.UpdateText();
                }
            };

            // Save points.
            SyntaxEditor.SavePointLeft += (_, __) => UpdateChangedMarker();
            SyntaxEditor.SavePointReached += (_, __) => UpdateChangedMarker();
            SyntaxEditor.CodeFile.LoadedTextChanged += CodeFile_LoadedTextChanged;

            // Interaction between settingsTextBox and errorsTextBox.
            SyntaxEditor.CurrentErrorsChanged += (_, __) => UpdateErrorListForm();

            Controls.Add(SyntaxEditor);

            // Initialize menu strip.
            var fileMenu = new List<DefaultUIActionBinding>();

            switch (codeAccessOption)
            {
                default:
                case SyntaxEditorCodeAccessOption.Default:
                    fileMenu.AddRange(new[] {
                        SharedUIAction.SaveToFile,
                        SharedUIAction.SaveAs,
                        SharedUIAction.Close });
                    break;
                case SyntaxEditorCodeAccessOption.FixedFile:
                    fileMenu.AddRange(new[] {
                        SharedUIAction.SaveToFile,
                        SharedUIAction.Close });
                    break;
                case SyntaxEditorCodeAccessOption.ReadOnly:
                    fileMenu.AddRange(new[] {
                        SharedUIAction.Close });
                    break;
            }

            var editMenu = new List<DefaultUIActionBinding>();
            editMenu.AddRange(new[] {
                SharedUIAction.Undo,
                SharedUIAction.Redo,
                SharedUIAction.CutSelectionToClipBoard,
                SharedUIAction.CopySelectionToClipBoard,
                SharedUIAction.PasteSelectionFromClipBoard,
                SharedUIAction.SelectAllText });

            var viewMenu = new List<DefaultUIActionBinding>();
            viewMenu.AddRange(new[] {
                SharedUIAction.ZoomIn,
                SharedUIAction.ZoomOut });

            var mainMenuItems = new List<MainMenuDropDownItem>
            {
                new MainMenuDropDownItem
                {
                    Container = new UIMenuNode.Container(SharedLocalizedStringKeys.File.ToTextProvider()),
                    DropDownItems = fileMenu
                },
                new MainMenuDropDownItem
                {
                    Container = new UIMenuNode.Container(SharedLocalizedStringKeys.Edit.ToTextProvider()),
                    DropDownItems = editMenu
                },
                new MainMenuDropDownItem
                {
                    Container = new UIMenuNode.Container(SharedLocalizedStringKeys.View.ToTextProvider()),
                    DropDownItems = viewMenu
                },
            };

            UpdateMenu(mainMenuItems);

            Session.Current.CurrentLocalizerChanged += CurrentLocalizerChanged;
        }

        private void UpdateErrorListForm()
        {
            if (errorListFormBox.Value is MenuCaptionBarForm<ErrorListPanel> errorListForm)
            {
                errorListForm.DockedControl.DisplayErrors(SyntaxEditor);
            }
        }

        private void CurrentLocalizerChanged(object sender, EventArgs e)
        {
            // Individual error translations may have changed.
            UpdateErrorListForm();
        }

        private string CodeFilePathDisplayString
        {
            get
            {
                string openTextFilePath = SyntaxEditor.CodeFile.OpenTextFilePath;

                // Use untitledString for new files that are not yet saved.
                return openTextFilePath == null
                    ? untitledString.DisplayText.Value
                    : Path.GetFileName(openTextFilePath);
            }
        }

        private void CodeFile_LoadedTextChanged(WorkingCopyTextFile sender, EventArgs e)
        {
            UpdateChangedMarker();
        }

        private void UpdateChangedMarker()
        {
            string fileName = CodeFilePathDisplayString;
            Text = SyntaxEditor.ContainsChanges ? ChangedMarker + fileName : fileName;

            // Must guard call to ReadOnly, it throws an AccessViolationException if the control is already disposed.
            bool isModified = !IsDisposed && !Disposing && !SyntaxEditor.ReadOnly && SyntaxEditor.ContainsChanges;
            UpdateSaveButton(isModified);
        }

        private int currentActivatedErrorIndex;

        private void ActivateSelectedError(int index)
        {
            if (0 <= index && index < SyntaxEditor.CurrentErrors.Count)
            {
                currentActivatedErrorIndex = index;
                SyntaxEditor.ActivateError(index);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            bool cancel = e.Cancel;
            OnFormClosing(e.CloseReason, ref cancel);
            e.Cancel = cancel;
            base.OnFormClosing(e);
        }

        void OnFormClosing(CloseReason closeReason, ref bool cancel)
        {
            // Only show message box if there's no auto save file from which local changes can be recovered.
            if (SyntaxEditor.ContainsChanges && SyntaxEditor.CodeFile.AutoSaveFile == null)
            {
                DialogResult result = MessageBox.Show(
                    Session.Current.CurrentLocalizer.Localize(SharedLocalizedStringKeys.SaveChangesQuery, new[] { CodeFilePathDisplayString }),
                    Session.Current.CurrentLocalizer.Localize(SharedLocalizedStringKeys.UnsavedChangesTitle),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button3);

                switch (result)
                {
                    case DialogResult.Yes:
                        try
                        {
                            SyntaxEditor.ActionHandler.TryPerformAction(SharedUIAction.SaveToFile.Action, true);
                        }
                        catch (Exception exception)
                        {
                            cancel = true;
                            MessageBox.Show(exception.Message);
                        }
                        break;
                    case DialogResult.No:
                        break;
                    default:
                        cancel = true;
                        break;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                untitledString?.Dispose();
            }

            base.Dispose(disposing);
        }

        public UIActionState TryShowErrorPane(bool perform)
        {
            // No errors while read-only: no need to show this action.
            if (SyntaxEditor.ReadOnly && SyntaxEditor.CurrentErrors.Count == 0)
            {
                return UIActionVisibility.Hidden;
            }

            if (perform)
            {
                // Session.Current discouraged, but contains the application icon.
                Session.Current.OpenOrActivateToolForm(
                    this,
                    errorListFormBox,
                    () =>
                    {
                        // Estimate how high the error list form needs to be to show all the errors.
                        // Stay within a certain range.
                        const int minHeight = 100;
                        int estimatedHeight = SyntaxEditor.CurrentErrors.Count * 15;

                        // Add padding * 2.
                        estimatedHeight += 16;

                        int maxHeight = ClientSize.Height;
                        if (maxHeight < estimatedHeight) estimatedHeight = maxHeight;
                        if (estimatedHeight < minHeight) estimatedHeight = minHeight;

                        // Use a panel to blend the errors list box with the background and at least appear as if its height is constant.
                        return new MenuCaptionBarForm<ErrorListPanel>(
                            new ErrorListPanel(this)
                            {
                                Dock = DockStyle.Fill,
                                BackColor = DefaultSyntaxEditorStyle.BackColor,
                                Padding = new Padding(6),
                            })
                        {
                            CaptionHeight = 26,
                            ShowIcon = false,
                            ClientSize = new Size(Width, estimatedHeight),
                        };
                    });
            }

            return UIActionVisibility.Enabled;
        }

        public UIActionState TryGoToPreviousError(bool perform)
        {
            if (SyntaxEditor.ReadOnly && SyntaxEditor.CurrentErrors.Count == 0)
            {
                return UIActionVisibility.Hidden;
            }

            int errorCount = SyntaxEditor.CurrentErrors.Count;
            if (errorCount == 0) return UIActionVisibility.Disabled;

            if (perform)
            {
                // Go to previous or last position.
                MenuCaptionBarForm<ErrorListPanel> errorListForm = errorListFormBox.Value as MenuCaptionBarForm<ErrorListPanel>;
                int targetIndex = errorListForm == null ? currentActivatedErrorIndex : errorListForm.DockedControl.SelectedErrorIndex;

                // Decrease and range check, since the error count may have changed in the meantime.
                targetIndex--;
                if (targetIndex < 0) targetIndex = errorCount - 1;

                // Update error list form and activate error in the editor.
                if (errorListForm != null) errorListForm.DockedControl.SelectedErrorIndex = targetIndex;
                ActivateSelectedError(targetIndex);
            }

            return UIActionVisibility.Enabled;
        }

        public UIActionState TryGoToNextError(bool perform)
        {
            if (SyntaxEditor.ReadOnly && SyntaxEditor.CurrentErrors.Count == 0)
            {
                return UIActionVisibility.Hidden;
            }

            int errorCount = SyntaxEditor.CurrentErrors.Count;
            if (errorCount == 0) return UIActionVisibility.Disabled;

            if (perform)
            {
                // Go to next or first position.
                MenuCaptionBarForm<ErrorListPanel> errorListForm = errorListFormBox.Value as MenuCaptionBarForm<ErrorListPanel>;
                int targetIndex = errorListForm == null ? currentActivatedErrorIndex : errorListForm.DockedControl.SelectedErrorIndex;

                // Increase and range check, since the error count may have changed in the meantime.
                targetIndex++;
                if (targetIndex >= errorCount) targetIndex = 0;

                // Update error list form and activate error in the editor.
                if (errorListForm != null) errorListForm.DockedControl.SelectedErrorIndex = targetIndex;
                ActivateSelectedError(targetIndex);
            }

            return UIActionVisibility.Enabled;
        }
    }

    /// <summary>
    /// Specifies options for accessing the code file opened by a syntax editor.
    /// </summary>
    public enum SyntaxEditorCodeAccessOption
    {
        /// <summary>
        /// The code file is editable and can be saved under a different name.
        /// </summary>
        Default,

        /// <summary>
        /// The code file is editable but cannot be saved under a different name.
        /// </summary>
        FixedFile,

        /// <summary>
        /// The code file is read-only.
        /// </summary>
        ReadOnly,
    }
}

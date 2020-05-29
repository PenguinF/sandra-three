#region License
/*********************************************************************************
 * SyntaxEditor.cs
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

using Eutherion.Localization;
using Eutherion.UIActions;
using Eutherion.Utils;
using Eutherion.Win.Storage;
using Eutherion.Win.UIActions;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Eutherion.Win.MdiAppTemplate
{
    /// <summary>
    /// Represents a <see cref="ScintillaEx"/> control with syntax highlighting.
    /// </summary>
    /// <typeparam name="TSyntaxTree">
    /// The type of syntax tree.
    /// </typeparam>
    /// <typeparam name="TTerminal">
    /// The type of terminal symbol to display.
    /// </typeparam>
    /// <typeparam name="TError">
    /// The type of error to display.
    /// </typeparam>
    public class SyntaxEditor<TSyntaxTree, TTerminal, TError> : ScintillaEx, IDockableControl, IWeakEventTarget
    {
        private class ErrorListPanel : Panel, IDockableControl
        {
            private static readonly Font noErrorsFont = new Font("Calibri", 10, FontStyle.Italic);
            private static readonly Font normalFont = new Font("Calibri", 10);

            private readonly SyntaxEditor<TSyntaxTree, TTerminal, TError> OwnerEditor;

            private readonly ListBoxEx errorsListBox;

            private readonly LocalizedString noErrorsString;
            private readonly LocalizedString errorLocationString;
            private readonly LocalizedString titleString;

            public DockProperties DockProperties { get; } = new DockProperties();

            public event Action DockPropertiesChanged;

            public ErrorListPanel(SyntaxEditor<TSyntaxTree, TTerminal, TError> ownerEditor)
            {
                OwnerEditor = ownerEditor;

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
                    { SharedUIAction.GoToPreviousError, OwnerEditor.TryGoToPreviousError },
                    { SharedUIAction.GoToNextError, OwnerEditor.TryGoToNextError },
                });

                UIMenu.AddTo(errorsListBox);

                // Assume that if this display text changes, that of errorLocationString changes too.
                noErrorsString = new LocalizedString(SharedLocalizedStringKeys.NoErrorsMessage);
                errorLocationString = new LocalizedString(SharedLocalizedStringKeys.ErrorLocation);
                titleString = new LocalizedString(SharedLocalizedStringKeys.ErrorPaneTitle);

                noErrorsString.DisplayText.ValueChanged += _ => DisplayErrors(OwnerEditor);
                errorLocationString.DisplayText.ValueChanged += _ => DisplayErrors(OwnerEditor);
                titleString.DisplayText.ValueChanged += _ => UpdateText();

                errorsListBox.DoubleClick += (_, __) => OwnerEditor.ActivateSelectedError(errorsListBox.SelectedIndex);
                errorsListBox.KeyDown += ErrorsListBox_KeyDown;

                Controls.Add(errorsListBox);

                DockProperties.CaptionHeight = 26;
            }

            protected override void OnBackColorChanged(EventArgs e)
            {
                // Blend background colors.
                errorsListBox.BackColor = BackColor;
                base.OnBackColorChanged(e);
            }

            public void UpdateText()
            {
                DockProperties.CaptionText = StringUtilities.ConditionalFormat(titleString.DisplayText.Value, new[] { OwnerEditor.CodeFilePathDisplayString });
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
                    OwnerEditor.ActivateSelectedError(errorsListBox.SelectedIndex);
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

            void IDockableControl.OnClosing(CloseReason closeReason, ref bool cancel) { }
        }

        private const int ErrorIndicatorIndex = 8;
        private const int WarningIndicatorIndex = 9;
        private const int MessageIndicatorIndex = 10;

        private const string ChangedMarker = "• ";

        private readonly Box<MenuCaptionBarForm> errorListFormBox = new Box<MenuCaptionBarForm>();

        private readonly LocalizedString untitledString;

        public DockProperties DockProperties { get; } = new DockProperties();

        public event Action DockPropertiesChanged;

        /// <summary>
        /// Gets the syntax descriptor.
        /// </summary>
        public SyntaxDescriptor<TSyntaxTree, TTerminal, TError> SyntaxDescriptor { get; }

        /// <summary>
        /// Gets the edited text file.
        /// </summary>
        public WorkingCopyTextFile CodeFile { get; }

        /// <summary>
        /// Flag to keep track of whether or not the text should still appear modified
        /// even if the control is at its save-point.
        /// </summary>
        private bool containsChangesAtSavePoint;

        /// <summary>
        /// Returns if this <see cref="SyntaxEditor{TSyntaxTree, TTerminal, TError}"/> contains any unsaved changes.
        /// If the text file could not be opened, true is returned.
        /// </summary>
        public bool ContainsChanges
            => !ReadOnly
            && (Modified || containsChangesAtSavePoint);

        public Style DefaultStyle => Styles[Style.Default];
        private Style LineNumberStyle => Styles[Style.LineNumber];
        private Style CallTipStyle => Styles[Style.CallTip];

        /// <summary>
        /// Initializes a new instance of a <see cref="SyntaxEditor{TSyntaxTree, TTerminal, TError}"/>.
        /// </summary>
        /// <param name="syntaxDescriptor">
        /// The syntax descriptor.
        /// </param>
        /// <param name="codeFile">
        /// The code file to show and/or edit.
        /// It is disposed together with this <see cref="SyntaxEditor{TSyntaxTree, TTerminal, TError}"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="syntaxDescriptor"/> and/or <paramref name="codeFile"/> are null.
        /// </exception>
        public SyntaxEditor(SyntaxEditorCodeAccessOption codeAccessOption,
                            SyntaxDescriptor<TSyntaxTree, TTerminal, TError> syntaxDescriptor,
                            WorkingCopyTextFile codeFile,
                            SettingProperty<int> zoomSetting)
        {
            SyntaxDescriptor = syntaxDescriptor ?? throw new ArgumentNullException(nameof(syntaxDescriptor));
            CodeFile = codeFile ?? throw new ArgumentNullException(nameof(codeFile));

            HScrollBar = false;
            VScrollBar = true;

            BorderStyle = BorderStyle.None;

            StyleResetDefault();
            DefaultStyle.BackColor = DefaultSyntaxEditorStyle.BackColor;
            DefaultStyle.ForeColor = DefaultSyntaxEditorStyle.ForeColor;
            DefaultSyntaxEditorStyle.Font.CopyTo(DefaultStyle);
            StyleClearAll();

            SetSelectionBackColor(true, DefaultSyntaxEditorStyle.ForeColor);
            SetSelectionForeColor(true, DefaultSyntaxEditorStyle.BackColor);

            CaretForeColor = DefaultSyntaxEditorStyle.ForeColor;

            CallTipStyle.BackColor = DefaultSyntaxEditorStyle.CallTipBackColor;
            CallTipStyle.ForeColor = DefaultSyntaxEditorStyle.ForeColor;
            DefaultSyntaxEditorStyle.CallTipFont.CopyTo(CallTipStyle);

            Margins.ForEach(x => x.Width = 0);
            Margins[0].BackColor = DefaultSyntaxEditorStyle.BackColor;
            Margins[1].BackColor = DefaultSyntaxEditorStyle.BackColor;

            LineNumberStyle.BackColor = DefaultSyntaxEditorStyle.BackColor;
            LineNumberStyle.ForeColor = DefaultSyntaxEditorStyle.LineNumberForeColor;
            DefaultSyntaxEditorStyle.Font.CopyTo(LineNumberStyle);

            Indicators[ErrorIndicatorIndex].Style = IndicatorStyle.Squiggle;
            Indicators[ErrorIndicatorIndex].ForeColor = DefaultSyntaxEditorStyle.ErrorColor;

            Indicators[WarningIndicatorIndex].Style = IndicatorStyle.Squiggle;
            Indicators[WarningIndicatorIndex].ForeColor = DefaultSyntaxEditorStyle.WarningColor;

            Indicators[MessageIndicatorIndex].Style = IndicatorStyle.Dots;
            Indicators[MessageIndicatorIndex].ForeColor = DefaultSyntaxEditorStyle.LineNumberForeColor;

            // Enable dwell events.
            MouseDwellTime = SystemInformation.MouseHoverTime;

            CodeFile.LoadedTextChanged += CodeFile_LoadedTextChanged;

            // Initialize line number margin for new empty files.
            UpdateLineNumberMargin(1);

            // Only use initialTextGenerator if nothing was auto-saved.
            containsChangesAtSavePoint = CodeFile.ContainsChanges;
            CopyTextFromTextFile();
            EmptyUndoBuffer();

            ReadOnly = codeAccessOption == SyntaxEditorCodeAccessOption.ReadOnly;

            // Initialize zoom factor and listen to changes.
            if (Session.Current.TryGetAutoSaveValue(zoomSetting, out int zoomFactor))
            {
                Zoom = zoomFactor;
            }

            ZoomFactorChanged += (_, e) => Session.Current.AutoSave.Persist(zoomSetting, e.ZoomFactor);

            this.BindActions(new UIActionBindings
            {
                { SharedUIAction.SaveToFile, TrySaveToFile },
            });

            if (codeAccessOption == SyntaxEditorCodeAccessOption.Default)
            {
                this.BindActions(new UIActionBindings
                {
                    { SharedUIAction.SaveAs, TrySaveAs },
                });
            }

            BindStandardEditUIActions();

            this.BindActions(new UIActionBindings
            {
                { SharedUIAction.ShowErrorPane, TryShowErrorPane },
                { SharedUIAction.GoToPreviousError, TryGoToPreviousError },
                { SharedUIAction.GoToNextError, TryGoToNextError },
            });

            UIMenu.AddTo(this);

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
            SavePointLeft += (_, __) => UpdateChangedMarker();
            SavePointReached += (_, __) => UpdateChangedMarker();
            CodeFile.LoadedTextChanged += CodeFile_LoadedTextChanged;

            // Initialize menu strip.
            List<Union<DefaultUIActionBinding, MainMenuDropDownItem>> fileMenu;

            switch (codeAccessOption)
            {
                default:
                case SyntaxEditorCodeAccessOption.Default:
                    fileMenu = new List<Union<DefaultUIActionBinding, MainMenuDropDownItem>>
                    {
                        SharedUIAction.SaveToFile,
                        SharedUIAction.SaveAs,
                        SharedUIAction.Close
                    };
                    break;
                case SyntaxEditorCodeAccessOption.FixedFile:
                    fileMenu = new List<Union<DefaultUIActionBinding, MainMenuDropDownItem>>
                    {
                        SharedUIAction.SaveToFile,
                        SharedUIAction.Close
                    };
                    break;
                case SyntaxEditorCodeAccessOption.ReadOnly:
                    fileMenu = new List<Union<DefaultUIActionBinding, MainMenuDropDownItem>>
                    {
                        SharedUIAction.Close
                    };
                    break;
            }

            DockProperties.CaptionHeight = 30;
            DockProperties.MainMenuItems = new List<MainMenuDropDownItem>
            {
                new MainMenuDropDownItem
                {
                    Container = new UIMenuNode.Container(SharedLocalizedStringKeys.File.ToTextProvider()),
                    DropDownItems = fileMenu
                },
                new MainMenuDropDownItem
                {
                    Container = new UIMenuNode.Container(SharedLocalizedStringKeys.Edit.ToTextProvider()),
                    DropDownItems = new List<Union<DefaultUIActionBinding, MainMenuDropDownItem>>
                    {
                        SharedUIAction.Undo,
                        SharedUIAction.Redo,
                        SharedUIAction.CutSelectionToClipBoard,
                        SharedUIAction.CopySelectionToClipBoard,
                        SharedUIAction.PasteSelectionFromClipBoard,
                        SharedUIAction.SelectAllText,
                    }
                },
                new MainMenuDropDownItem
                {
                    Container = new UIMenuNode.Container(SharedLocalizedStringKeys.View.ToTextProvider()),
                    DropDownItems = new List<Union<DefaultUIActionBinding, MainMenuDropDownItem>>
                    {
                        SharedUIAction.ZoomIn,
                        SharedUIAction.ZoomOut,
                    }
                },
            };

            Session.Current.CurrentLocalizerChanged += CurrentLocalizerChanged;
        }

        private void CodeFile_LoadedTextChanged(WorkingCopyTextFile sender, EventArgs e)
        {
            containsChangesAtSavePoint = CodeFile.ContainsChanges;

            if (!containsChangesAtSavePoint)
            {
                if (ReadOnly && CodeFile.LoadException != null)
                {
                    // If read-only and the file becomes unavailable, just reload with an empty text.
                    if (!string.IsNullOrEmpty(CodeFile.LocalCopyText))
                    {
                        CodeFile.UpdateLocalCopyText(string.Empty, containsChanges: false);
                        CopyTextFromTextFile();
                    }
                }
                else
                {
                    CopyTextFromTextFile();
                }
            }

            UpdateChangedMarker();
        }

        private bool copyingTextFromTextFile;

        /// <summary>
        /// Sets the Text property without updating WorkingCopyTextFile
        /// when they're known to be the same.
        /// </summary>
        private void CopyTextFromTextFile()
        {
            copyingTextFromTextFile = true;
            try
            {
                Text = CodeFile.LocalCopyText;
                SetSavePoint();
            }
            finally
            {
                copyingTextFromTextFile = false;
            }
        }

        private int displayedMaxLineNumberLength;

        private int GetMaxLineNumberLength(int maxLineNumberToDisplay)
        {
            if (maxLineNumberToDisplay <= 9) return 1;
            if (maxLineNumberToDisplay <= 99) return 2;
            if (maxLineNumberToDisplay <= 999) return 3;
            if (maxLineNumberToDisplay <= 9999) return 4;
            return (int)Math.Floor(Math.Log10(maxLineNumberToDisplay)) + 1;
        }

        private void UpdateLineNumberMargin(int maxLineNumberLength)
        {
            Margins[0].Width = TextWidth(Style.LineNumber, new string('0', maxLineNumberLength + 1));
            Margins[1].Width = TextWidth(Style.LineNumber, "0");
            displayedMaxLineNumberLength = maxLineNumberLength;
        }

        // OnUpdateUI gets raised only once for an undo or redo action, whereas OnTextChanged gets raised for each individual keystroke.
        private bool textDirty;
        private TSyntaxTree syntaxTree;

        protected override void OnUpdateUI(UpdateUIEventArgs e)
        {
            if (textDirty)
            {
                string code = Text;

                // This prevents re-entrancy into WorkingCopyTextFile.
                if (!copyingTextFromTextFile)
                {
                    CodeFile.UpdateLocalCopyText(code, ContainsChanges);
                }

                int maxLineNumberLength = GetMaxLineNumberLength(Lines.Count);
                if (displayedMaxLineNumberLength != maxLineNumberLength)
                {
                    UpdateLineNumberMargin(maxLineNumberLength);
                }

                syntaxTree = SyntaxDescriptor.Parse(code);

                IndicatorClearRange(0, TextLength);

                CurrentErrors = ReadOnlyList<TError>.Create(SyntaxDescriptor.GetErrors(syntaxTree));

                // Keep track of indicatorCurrent here to skip P/Invoke calls to the Scintilla control.
                int indicatorCurrent = 0;

                foreach (var error in CurrentErrors)
                {
                    var (errorStart, errorLength) = SyntaxDescriptor.GetErrorRange(error);
                    var errorLevel = SyntaxDescriptor.GetErrorLevel(error);

                    int oldIndicatorCurrent = indicatorCurrent;
                    indicatorCurrent = errorLevel == ErrorLevel.Error ? ErrorIndicatorIndex
                                     : errorLevel == ErrorLevel.Warning ? WarningIndicatorIndex
                                     : MessageIndicatorIndex;

                    if (oldIndicatorCurrent != indicatorCurrent) IndicatorCurrent = indicatorCurrent;

                    IndicatorFillRange(errorStart, errorLength);
                }

                textDirty = false;

                UpdateErrorListForm();
            }

            if (syntaxTree == null) return;

            // Get the visible range of text to style.
            int firstVisibleLine = FirstVisibleLine;
            if (Lines.Count <= firstVisibleLine) return;
            int startPosition = Lines[FirstVisibleLine].Position;
            int bottomVisibleLine = firstVisibleLine + LinesOnScreen;
            if (Lines.Count <= bottomVisibleLine) bottomVisibleLine = Lines.Count - 1;
            if (bottomVisibleLine < firstVisibleLine) return;
            int endPosition = Lines[bottomVisibleLine].EndPosition;

            // Increase range on both ends by 1 to make the search for intersecting intervals inclusive,
            // so e.g. terminal symbols that end exactly at the end-styled position and which may be affected
            // by the latest change are returned as well.
            startPosition--;
            endPosition++;

            foreach (var token in SyntaxDescriptor.GetTerminalsInRange(syntaxTree, startPosition, endPosition - startPosition))
            {
                var (start, length) = SyntaxDescriptor.GetTokenSpan(token);
                ApplyStyle(SyntaxDescriptor.GetStyle(this, token), start, length);
            }

            base.OnUpdateUI(e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            textDirty = true;
            CallTipCancel();
            base.OnTextChanged(e);
        }

        public ReadOnlyList<TError> CurrentErrors { get; private set; } = ReadOnlyList<TError>.Empty;

        public void ActivateError(int errorIndex)
        {
            // Select the text that generated the error.
            if (0 <= errorIndex && errorIndex < CurrentErrors.Count)
            {
                // Determine how many lines are visible in the top half of the control.
                int firstVisibleLine = FirstVisibleLine;
                int visibleLines = LinesOnScreen;
                int bottomVisibleLine = firstVisibleLine + visibleLines;

                // Then calculate which line should become the first visible line
                // so the error line ends up in the middle of the control.
                var (hotErrorStart, hotErrorLength) = SyntaxDescriptor.GetErrorRange(CurrentErrors[errorIndex]);
                int hotErrorLine = LineFromPosition(hotErrorStart);

                // hotErrorLine in view?
                // Don't include the bottom line, it's likely not completely visible.
                if (hotErrorLine < firstVisibleLine || bottomVisibleLine <= hotErrorLine)
                {
                    int targetFirstVisibleLine = hotErrorLine - (visibleLines / 2);
                    if (targetFirstVisibleLine < 0) targetFirstVisibleLine = 0;
                    FirstVisibleLine = targetFirstVisibleLine;
                }

                GotoPosition(hotErrorStart);
                if (hotErrorLength > 0)
                {
                    SelectionEnd = hotErrorStart + hotErrorLength;
                }

                Focus();
            }
        }

        private IEnumerable<string> ActiveErrorMessages(int textPosition)
        {
            if (textPosition >= 0 && textPosition < TextLength)
            {
                foreach (var error in CurrentErrors)
                {
                    var (errorStart, errorLength) = SyntaxDescriptor.GetErrorRange(error);

                    if (errorStart <= textPosition && textPosition < errorStart + errorLength)
                    {
                        yield return SyntaxDescriptor.GetErrorMessage(error);
                    }
                }
            }
        }

        protected override void OnDwellStart(DwellEventArgs e)
        {
            base.OnDwellStart(e);

            int textPosition = e.Position;
            string toolTipText = string.Join(
                "\n\n",
                ActiveErrorMessages(textPosition).Select(x => Session.Current.CurrentLocalizer.ToSentenceCase(x)));

            if (toolTipText.Length > 0)
            {
                CallTipShow(textPosition, toolTipText);
            }
            else
            {
                CallTipCancel();
            }
        }

        protected override void OnDwellEnd(DwellEventArgs e)
        {
            CallTipCancel();
            base.OnDwellEnd(e);
        }

        private void UpdateErrorListForm()
        {
            if (errorListFormBox.Value is MenuCaptionBarForm<ErrorListPanel> errorListForm)
            {
                errorListForm.DockedControl.DisplayErrors(this);
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
                string openTextFilePath = CodeFile.OpenTextFilePath;

                // Use untitledString for new files that are not yet saved.
                return openTextFilePath == null
                    ? untitledString.DisplayText.Value
                    : Path.GetFileName(openTextFilePath);
            }
        }

        private void UpdateChangedMarker()
        {
            string fileName = CodeFilePathDisplayString;
            DockProperties.CaptionText = ContainsChanges ? ChangedMarker + fileName : fileName;

            // Must guard call to ReadOnly, it throws an AccessViolationException if the control is already disposed.
            DockProperties.IsModified = !IsDisposed && !Disposing && !ReadOnly && ContainsChanges;
            DockPropertiesChanged?.Invoke();
        }

        private int currentActivatedErrorIndex;

        private void ActivateSelectedError(int index)
        {
            if (0 <= index && index < CurrentErrors.Count)
            {
                currentActivatedErrorIndex = index;
                ActivateError(index);
            }
        }

        void IDockableControl.OnClosing(CloseReason closeReason, ref bool cancel)
        {
            // Only show message box if there's no auto save file from which local changes can be recovered.
            if (ContainsChanges && CodeFile.AutoSaveFile == null)
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
                            UIActionState actionState = ActionHandler.TryPerformAction(SharedUIAction.SaveToFile.Action, true);

                            if (actionState.UIActionVisibility == UIActionVisibility.Disabled)
                            {
                                // Save as dialog got canceled.
                                // If UIActionVisibility.Hidden was returned, the action isn't available at all, which shouldn't block a form close.
                                cancel = true;
                            }
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
                CodeFile.Dispose();
                untitledString?.Dispose();
            }

            base.Dispose(disposing);
        }

        public UIActionState TryShowErrorPane(bool perform)
        {
            // No errors while read-only: no need to show this action.
            if (ReadOnly && CurrentErrors.Count == 0)
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
                        int estimatedHeight = CurrentErrors.Count * 15;

                        // Add padding * 2.
                        estimatedHeight += 12;

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
                            ShowIcon = false,
                            ClientSize = new Size(Math.Min(Width, 600), estimatedHeight),
                        };
                    });
            }

            return UIActionVisibility.Enabled;
        }

        public UIActionState TryGoToPreviousError(bool perform)
        {
            if (ReadOnly && CurrentErrors.Count == 0)
            {
                return UIActionVisibility.Hidden;
            }

            int errorCount = CurrentErrors.Count;
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
            if (ReadOnly && CurrentErrors.Count == 0)
            {
                return UIActionVisibility.Hidden;
            }

            int errorCount = CurrentErrors.Count;
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

        public UIActionState TrySaveToFile(bool perform)
        {
            // Use "Save as..." if there's no back-end file.
            if (CodeFile.OpenTextFilePath == null) return TrySaveAs(perform);

            if (IsDisposed || Disposing) return UIActionVisibility.Hidden;
            if (ReadOnly) return UIActionVisibility.Hidden;
            if (!ContainsChanges) return UIActionVisibility.Disabled;

            if (perform)
            {
                CodeFile.Save();
                containsChangesAtSavePoint = CodeFile.ContainsChanges;
                SetSavePoint();
            }

            return UIActionVisibility.Enabled;
        }

        public UIActionState TrySaveAs(bool perform)
        {
            if (IsDisposed || Disposing) return UIActionVisibility.Hidden;
            if (ReadOnly) return UIActionVisibility.Hidden;
            if (!CodeFile.IsTextFileOwner) return UIActionVisibility.Hidden;

            if (perform)
            {
                string extension = SyntaxDescriptor.FileExtension;
                var extensionLocalizedKey = SyntaxDescriptor.FileExtensionLocalizedKey;

                var saveFileDialog = new SaveFileDialog
                {
                    AutoUpgradeEnabled = true,
                    DereferenceLinks = true,
                    DefaultExt = extension,
                    Filter = $"{Session.Current.CurrentLocalizer.Localize(extensionLocalizedKey)} (*.{extension})|*.{extension}|{Session.Current.CurrentLocalizer.Localize(SharedLocalizedStringKeys.AllFiles)} (*.*)|*.*",
                    SupportMultiDottedExtensions = true,
                    RestoreDirectory = true,
                    Title = Session.Current.CurrentLocalizer.Localize(SharedLocalizedStringKeys.SaveAs),
                    ValidateNames = true,
                };

                if (CodeFile.OpenTextFilePath != null)
                {
                    saveFileDialog.InitialDirectory = Path.GetDirectoryName(CodeFile.OpenTextFilePath);
                    saveFileDialog.FileName = Path.GetFileName(CodeFile.OpenTextFilePath);
                }

                var dialogResult = saveFileDialog.ShowDialog(TopLevelControl as Form);
                if (dialogResult == DialogResult.OK)
                {
                    CodeFile.Replace(saveFileDialog.FileName);
                    containsChangesAtSavePoint = CodeFile.ContainsChanges;
                    SetSavePoint();
                }
                else
                {
                    // Use this value to indicate a camcel action.
                    return UIActionVisibility.Disabled;
                }
            }

            return UIActionVisibility.Enabled;
        }
    }

    /// <summary>
    /// Describes the interaction between a syntax and how a <see cref="SyntaxDescriptor{TSyntaxTree, TTerminal, TError}"/> displays it.
    /// </summary>
    /// <typeparam name="TSyntaxTree">
    /// The type of syntax tree.
    /// </typeparam>
    /// <typeparam name="TTerminal">
    /// The type of terminal symbol to display.
    /// </typeparam>
    /// <typeparam name="TError">
    /// The type of error to display.
    /// </typeparam>
    public abstract class SyntaxDescriptor<TSyntaxTree, TTerminal, TError>
    {
        /// <summary>
        /// Gets the default file extension for this syntax.
        /// </summary>
        public abstract string FileExtension { get; }

        /// <summary>
        /// Gets the localized description for the default file extension.
        /// </summary>
        public abstract LocalizedStringKey FileExtensionLocalizedKey { get; }

        /// <summary>
        /// Parses the code, yielding lists of tokens and errors.
        /// </summary>
        public abstract TSyntaxTree Parse(string code);

        /// <summary>
        /// Enumerates terminal symbols of a syntax tree.
        /// </summary>
        public abstract IEnumerable<TTerminal> GetTerminalsInRange(TSyntaxTree syntaxTree, int start, int length);

        /// <summary>
        /// Enumerates errors of a syntax tree.
        /// </summary>
        public abstract IEnumerable<TError> GetErrors(TSyntaxTree syntaxTree);

        /// <summary>
        /// Gets the style for a terminal symbol.
        /// </summary>
        public abstract Style GetStyle(SyntaxEditor<TSyntaxTree, TTerminal, TError> syntaxEditor, TTerminal terminalSymbol);

        /// <summary>
        /// Gets the start and length of a terminal symbol.
        /// </summary>
        public abstract (int, int) GetTokenSpan(TTerminal terminalSymbol);

        /// <summary>
        /// Gets the start position and the length of the text span of an error.
        /// </summary>
        public abstract (int, int) GetErrorRange(TError error);

        /// <summary>
        /// Gets the severity level of an error.
        /// </summary>
        public abstract ErrorLevel GetErrorLevel(TError error);

        /// <summary>
        /// Gets the localized message of an error.
        /// </summary>
        public abstract string GetErrorMessage(TError error);
    }

    /// <summary>
    /// Contains default styles for syntax editors.
    /// </summary>
    public static class DefaultSyntaxEditorStyle
    {
        public static readonly Color BackColor = Color.FromArgb(0x21, 0x22, 0x1c);
        public static readonly Color ForeColor = Color.AntiqueWhite;
        public static readonly Font Font = new Font("Consolas", 10);

        /// <summary>
        /// Gets the default fore color used for displaying line numbers in a syntax editor.
        /// </summary>
        public static readonly Color LineNumberForeColor = Color.FromArgb(176, 176, 176);

        public static readonly Color ErrorColor = Color.Red;
        public static readonly Color WarningColor = Color.Yellow;

        public static readonly Color CallTipBackColor = Color.FromArgb(48, 32, 32);
        public static readonly Font CallTipFont = new Font("Segoe UI", 10);
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

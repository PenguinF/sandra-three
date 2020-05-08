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
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Eutherion.Win.AppTemplate
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
    public class SyntaxEditor<TSyntaxTree, TTerminal, TError> : ScintillaEx
    {
        private const int ErrorIndicatorIndex = 8;
        private const int WarningIndicatorIndex = 9;
        private const int MessageIndicatorIndex = 10;

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
        public SyntaxEditor(SyntaxDescriptor<TSyntaxTree, TTerminal, TError> syntaxDescriptor,
                            WorkingCopyTextFile codeFile)
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

            // Only use initialTextGenerator if nothing was auto-saved.
            containsChangesAtSavePoint = CodeFile.ContainsChanges;
            CopyTextFromTextFile();
            EmptyUndoBuffer();
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

        private TSyntaxTree syntaxTree;

        protected override void OnUpdateUI(UpdateUIEventArgs e)
        {
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
            CallTipCancel();
            base.OnTextChanged(e);

            string code = Text;

            // This prevents re-entrancy into WorkingCopyTextFile.
            if (!copyingTextFromTextFile)
            {
                CodeFile.UpdateLocalCopyText(code, ContainsChanges);
            }

            int maxLineNumberLength = GetMaxLineNumberLength(Lines.Count);
            if (displayedMaxLineNumberLength != maxLineNumberLength)
            {
                Margins[0].Width = TextWidth(Style.LineNumber, new string('0', maxLineNumberLength + 1));
                Margins[1].Width = TextWidth(Style.LineNumber, "0");
                displayedMaxLineNumberLength = maxLineNumberLength;
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

            CurrentErrorsChanged?.Invoke(this, EventArgs.Empty);
        }

        public ReadOnlyList<TError> CurrentErrors { get; private set; } = ReadOnlyList<TError>.Empty;

        public event EventHandler CurrentErrorsChanged;

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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CodeFile.Dispose();
            }

            base.Dispose(disposing);
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
}

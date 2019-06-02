#region License
/*********************************************************************************
 * SyntaxEditor.cs
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
using Eutherion.Text;
using Eutherion.UIActions;
using Eutherion.Utils;
using Eutherion.Win.Storage;
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
    /// <typeparam name="TTerminal">
    /// The type of terminal symbol to display.
    /// </typeparam>
    /// <typeparam name="TError">
    /// The type of error to display.
    /// </typeparam>
    public class SyntaxEditor<TTerminal, TError> : ScintillaEx
    {
        private const int ErrorIndicatorIndex = 8;

        /// <summary>
        /// This results in file names such as ".%_A8.tmp".
        /// </summary>
        private static readonly string AutoSavedLocalChangesFileName = ".%.tmp";

        private static FileStream CreateUniqueNewAutoSaveFileStream()
        {
            if (!Session.Current.TryGetAutoSaveValue(SharedSettings.AutoSaveCounter, out uint autoSaveFileCounter))
            {
                autoSaveFileCounter = 1;
            };

            var file = FileUtilities.CreateUniqueFile(
                Path.Combine(Session.Current.AppDataSubFolder, AutoSavedLocalChangesFileName),
                FileOptions.SequentialScan | FileOptions.Asynchronous,
                ref autoSaveFileCounter);

            Session.Current.AutoSave.Persist(SharedSettings.AutoSaveCounter, autoSaveFileCounter);

            return file;
        }

        private static WorkingCopyTextFile OpenWorkingCopyTextFile(LiveTextFile codeFile, SettingProperty<AutoSaveFileNamePair> autoSaveSetting)
        {
            if (autoSaveSetting != null && Session.Current.TryGetAutoSaveValue(autoSaveSetting, out AutoSaveFileNamePair autoSaveFileNamePair))
            {
                FileStreamPair fileStreamPair = null;

                try
                {
                    fileStreamPair = FileStreamPair.Create(
                        AutoSaveTextFile.OpenExistingAutoSaveFile,
                        Path.Combine(Session.Current.AppDataSubFolder, autoSaveFileNamePair.FileName1),
                        Path.Combine(Session.Current.AppDataSubFolder, autoSaveFileNamePair.FileName2));

                    return WorkingCopyTextFile.FromLiveTextFile(codeFile, fileStreamPair);
                }
                catch (Exception autoSaveLoadException)
                {
                    if (fileStreamPair != null) fileStreamPair.Dispose();

                    // Only trace exceptions resulting from e.g. a missing LOCALAPPDATA subfolder or insufficient access.
                    autoSaveLoadException.Trace();
                }
            }

            return WorkingCopyTextFile.FromLiveTextFile(codeFile, null);
        }

        /// <summary>
        /// Gets the syntax descriptor.
        /// </summary>
        public SyntaxDescriptor<TTerminal, TError> SyntaxDescriptor { get; }

        /// <summary>
        /// Gets the edited text file.
        /// </summary>
        public WorkingCopyTextFile CodeFile { get; }

        /// <summary>
        /// Returns if this <see cref="SyntaxEditor{TTerminal, TError}"/> contains any unsaved changes.
        /// If the text file could not be opened, true is returned.
        /// </summary>
        public bool ContainsChanges
            => !ReadOnly
            && (Modified || CodeFile.LoadException != null);

        /// <summary>
        /// Setting to use when an auto-save file name pair is generated.
        /// </summary>
        private readonly SettingProperty<AutoSaveFileNamePair> autoSaveSetting;

        private readonly TextIndex<TTerminal> TextIndex;

        public Style DefaultStyle => Styles[Style.Default];
        private Style LineNumberStyle => Styles[Style.LineNumber];
        private Style CallTipStyle => Styles[Style.CallTip];

        /// <summary>
        /// Initializes a new instance of a <see cref="SyntaxEditor{TTerminal, TError}"/>.
        /// </summary>
        /// <param name="syntaxDescriptor">
        /// The syntax descriptor.
        /// </param>
        /// <param name="codeFile">
        /// The code file to show and/or edit.
        /// </param>
        /// <param name="initialTextGenerator">
        /// Optional function to generate initial text in case the code file could not be loaded and was not auto-saved.
        /// </param>
        /// <param name="autoSaveSetting">
        /// The setting property to use to auto-save the file names of the file pair used for auto-saving local changes.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="syntaxDescriptor"/> is null.
        /// </exception>
        public SyntaxEditor(SyntaxDescriptor<TTerminal, TError> syntaxDescriptor,
                            LiveTextFile codeFile,
                            Func<string> initialTextGenerator,
                            SettingProperty<AutoSaveFileNamePair> autoSaveSetting)
        {
            SyntaxDescriptor = syntaxDescriptor ?? throw new ArgumentNullException(nameof(syntaxDescriptor));
            this.autoSaveSetting = autoSaveSetting;
            CodeFile = OpenWorkingCopyTextFile(codeFile, autoSaveSetting);

            TextIndex = new TextIndex<TTerminal>();

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
            IndicatorCurrent = ErrorIndicatorIndex;

            // Enable dwell events.
            MouseDwellTime = SystemInformation.MouseHoverTime;

            CodeFile.QueryAutoSaveFile += CodeFile_QueryAutoSaveFile;
            CodeFile.OpenTextFile.FileUpdated += OpenTextFile_FileUpdated;

            // Only use initialTextGenerator if nothing was auto-saved.
            if (CodeFile.LoadException != null && string.IsNullOrEmpty(CodeFile.LocalCopyText))
            {
                Text = initialTextGenerator != null
                    ? (initialTextGenerator() ?? string.Empty)
                    : string.Empty;
            }
            else
            {
                CopyTextFromTextFile();
            }

            EmptyUndoBuffer();
        }

        private void OpenTextFile_FileUpdated(LiveTextFile sender, EventArgs e)
        {
            if (!ContainsChanges)
            {
                // Reload the text if different.
                string reloadedText = CodeFile.LoadedText;

                // Without this check the undo buffer gets an extra empty entry which is weird.
                if (CodeFile.LocalCopyText != reloadedText)
                {
                    Text = reloadedText;
                    SetSavePoint();
                }
            }

            // Make sure to auto-save if ContainsChanges changed but its text did not.
            // This covers the case in which the file was saved and unmodified, but then deleted remotely.
            CodeFile.UpdateLocalCopyText(
                CodeFile.LocalCopyText,
                ContainsChanges);
        }

        private void CodeFile_QueryAutoSaveFile(WorkingCopyTextFile sender, QueryAutoSaveFileEventArgs e)
        {
            // Only open auto-save files if they can be stored in autoSaveSetting.
            if (autoSaveSetting != null)
            {
                FileStreamPair fileStreamPair = null;

                try
                {
                    fileStreamPair = FileStreamPair.Create(CreateUniqueNewAutoSaveFileStream, CreateUniqueNewAutoSaveFileStream);
                    e.AutoSaveFileStreamPair = fileStreamPair;

                    Session.Current.AutoSave.Persist(
                        autoSaveSetting,
                        new AutoSaveFileNamePair(
                            Path.GetFileName(fileStreamPair.FileStream1.Name),
                            Path.GetFileName(fileStreamPair.FileStream2.Name)));
                }
                catch (Exception autoSaveLoadException)
                {
                    if (fileStreamPair != null) fileStreamPair.Dispose();

                    // Only trace exceptions resulting from e.g. a missing LOCALAPPDATA subfolder or insufficient access.
                    autoSaveLoadException.Trace();
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

            TextIndex.Clear();

            var (tokens, errors) = SyntaxDescriptor.Parse(code);
            tokens.ForEach(TextIndex.AppendTerminalSymbol);

            foreach (var textElement in TextIndex.Elements)
            {
                ApplyStyle(SyntaxDescriptor.GetStyle(this, textElement.TerminalSymbol), textElement.Start, textElement.Length);
            }

            IndicatorClearRange(0, TextLength);

            if (errors == null || errors.Count == 0)
            {
                currentErrors = null;
            }
            else
            {
                currentErrors = errors;

                foreach (var error in errors)
                {
                    var (errorStart, errorLength) = SyntaxDescriptor.GetErrorRange(error);

                    IndicatorFillRange(errorStart, errorLength);
                }
            }

            CurrentErrorsChanged?.Invoke(this, EventArgs.Empty);
        }

        private List<TError> currentErrors;

        public int CurrentErrorCount
            => currentErrors == null ? 0 : currentErrors.Count;

        public IEnumerable<TError> CurrentErrors
            => currentErrors == null ? Enumerable.Empty<TError>() : currentErrors.Enumerate();

        public event EventHandler CurrentErrorsChanged;

        public void ActivateError(int errorIndex)
        {
            // Select the text that generated the error.
            if (currentErrors != null && 0 <= errorIndex && errorIndex < currentErrors.Count)
            {
                // Determine how many lines are visible in the top half of the control.
                int firstVisibleLine = FirstVisibleLine;
                int visibleLines = LinesOnScreen;
                int bottomVisibleLine = firstVisibleLine + visibleLines;

                // Then calculate which line should become the first visible line
                // so the error line ends up in the middle of the control.
                var (hotErrorStart, hotErrorLength) = SyntaxDescriptor.GetErrorRange(currentErrors[errorIndex]);
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
            if (currentErrors != null && textPosition >= 0 && textPosition < TextLength)
            {
                foreach (var error in currentErrors)
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
            string toolTipText = string.Join("\n\n", ActiveErrorMessages(textPosition));

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
                CodeFile.OpenTextFile.FileUpdated -= OpenTextFile_FileUpdated;
                CodeFile.Dispose();

                // If auto-save files have been deleted, remove from Session.Current.AutoSave as well.
                if (autoSaveSetting != null
                    && CodeFile.AutoSaveFile == null
                    && Session.Current.TryGetAutoSaveValue(autoSaveSetting, out AutoSaveFileNamePair _))
                {
                    Session.Current.AutoSave.Remove(autoSaveSetting);
                }
            }

            base.Dispose(disposing);
        }

        public UIActionState TrySaveToFile(bool perform)
        {
            if (ReadOnly) return UIActionVisibility.Hidden;
            if (!ContainsChanges) return UIActionVisibility.Disabled;

            if (perform)
            {
                CodeFile.Save();
                SetSavePoint();
            }

            return UIActionVisibility.Enabled;
        }
    }

    /// <summary>
    /// Describes the interaction between a syntax and how a <see cref="SyntaxDescriptor{TTerminal, TError}"/> displays it.
    /// </summary>
    /// <typeparam name="TTerminal">
    /// The type of terminal symbol to display.
    /// </typeparam>
    /// <typeparam name="TError">
    /// The type of error to display.
    /// </typeparam>
    public abstract class SyntaxDescriptor<TTerminal, TError>
    {
        /// <summary>
        /// Parses the code, yielding lists of tokens and errors.
        /// </summary>
        public abstract (IEnumerable<TextElement<TTerminal>>, List<TError>) Parse(string code);

        /// <summary>
        /// Gets the style for a terminal symbol.
        /// </summary>
        public abstract Style GetStyle(SyntaxEditor<TTerminal, TError> syntaxEditor, TTerminal terminalSymbol);

        /// <summary>
        /// Gets the start position and the length of the text span of an error.
        /// </summary>
        public abstract (int, int) GetErrorRange(TError error);

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
        public static readonly Color BackColor = Color.FromArgb(16, 16, 16);
        public static readonly Color ForeColor = Color.WhiteSmoke;
        public static readonly Font Font = new Font("Consolas", 10);

        /// <summary>
        /// Gets the default fore color used for displaying line numbers in a syntax editor.
        /// </summary>
        public static readonly Color LineNumberForeColor = Color.FromArgb(176, 176, 176);

        public static readonly Color ErrorColor = Color.Red;

        public static readonly Color CallTipBackColor = Color.FromArgb(48, 32, 32);
        public static readonly Font CallTipFont = new Font("Segoe UI", 10);
    }

    /// <summary>
    /// Represents two file names in the local application data folder.
    /// </summary>
    public struct AutoSaveFileNamePair
    {
        public string FileName1;
        public string FileName2;

        public AutoSaveFileNamePair(string fileName1, string fileName2)
        {
            FileName1 = fileName1;
            FileName2 = fileName2;
        }
    }

    /// <summary>
    /// Specialized PType that accepts pairs of legal file names that are used for auto-saving changes in text files.
    /// </summary>
    public sealed class AutoSaveFilePairPType : PType<AutoSaveFileNamePair>
    {
        public static readonly PTypeErrorBuilder AutoSaveFilePairTypeError
            = new PTypeErrorBuilder(new LocalizedStringKey(nameof(AutoSaveFilePairTypeError)));

        public static readonly AutoSaveFilePairPType Instance = new AutoSaveFilePairPType();

        private AutoSaveFilePairPType() { }

        public override Union<ITypeErrorBuilder, AutoSaveFileNamePair> TryGetValidValue(PValue value)
        {
            if (value is PList pair
                && pair.Count == 2
                && FileNameType.InstanceAllowStartWithDots.TryGetValidValue(pair[0]).IsOption2(out string fileName1)
                && FileNameType.InstanceAllowStartWithDots.TryGetValidValue(pair[1]).IsOption2(out string fileName2))
            {
                return ValidValue(new AutoSaveFileNamePair(fileName1, fileName2));
            }

            return InvalidValue(AutoSaveFilePairTypeError);
        }

        public override PValue GetPValue(AutoSaveFileNamePair value) => new PList(
            new PValue[]
            {
                FileNameType.InstanceAllowStartWithDots.GetPValue(value.FileName1),
                FileNameType.InstanceAllowStartWithDots.GetPValue(value.FileName2),
            });
    }
}

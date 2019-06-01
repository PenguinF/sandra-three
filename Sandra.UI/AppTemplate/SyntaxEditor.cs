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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Eutherion.Win.AppTemplate
{
    /// <summary>
    /// Represents a <see cref="ScintillaEx"/> control with syntax highlighting.
    /// </summary>
    public abstract class SyntaxEditor<TTerminal> : ScintillaEx
    {
        private const int ErrorIndicatorIndex = 8;

        /// <summary>
        /// This results in file names such as ".%_A8.tmp".
        /// </summary>
        private static readonly string AutoSavedLocalChangesFileName = ".%.tmp";

        protected static FileStream CreateUniqueNewAutoSaveFileStream()
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

        protected static WorkingCopyTextFile OpenWorkingCopyTextFile(LiveTextFile codeFile, SettingProperty<AutoSaveFileNamePair> autoSaveSetting)
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

                    return new WorkingCopyTextFile(codeFile, fileStreamPair);
                }
                catch (Exception autoSaveLoadException)
                {
                    if (fileStreamPair != null) fileStreamPair.Dispose();

                    // Only trace exceptions resulting from e.g. a missing LOCALAPPDATA subfolder or insufficient access.
                    autoSaveLoadException.Trace();
                }
            }

            return new WorkingCopyTextFile(codeFile, null);
        }

        /// <summary>
        /// Gets the edited text file.
        /// </summary>
        public WorkingCopyTextFile CodeFile { get; }

        /// <summary>
        /// Returns if this <see cref="SyntaxEditor{TTerminal}"/> contains any unsaved changes.
        /// If the text file could not be opened, true is returned.
        /// </summary>
        public bool ContainsChanges
            => !ReadOnly
            && (Modified || CodeFile.LoadException != null);

        /// <summary>
        /// Setting to use when an auto-save file name pair is generated.
        /// </summary>
        protected readonly SettingProperty<AutoSaveFileNamePair> autoSaveSetting;

        protected readonly TextIndex<TTerminal> TextIndex;

        protected Style DefaultStyle => Styles[Style.Default];
        private Style LineNumberStyle => Styles[Style.LineNumber];
        private Style CallTipStyle => Styles[Style.CallTip];

        /// <summary>
        /// Initializes a new instance of a <see cref="SyntaxEditor{TTerminal}"/>.
        /// </summary>
        /// <param name="codeFile">
        /// The code file to show and/or edit.
        /// </param>
        /// <param name="autoSaveSetting">
        /// The setting property to use to auto-save the file names of the file pair used for auto-saving local changes.
        /// </param>
        public SyntaxEditor(LiveTextFile codeFile,
                            SettingProperty<AutoSaveFileNamePair> autoSaveSetting)
        {
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

        protected void CodeFile_QueryAutoSaveFile(WorkingCopyTextFile sender, QueryAutoSaveFileEventArgs e)
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

        protected bool copyingTextFromTextFile;

        /// <summary>
        /// Sets the Text property without updating WorkingCopyTextFile
        /// when they're known to be the same.
        /// </summary>
        protected void CopyTextFromTextFile()
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

        protected void ApplyStyle(TextElement<TTerminal> element, Style style)
            => ApplyStyle(style, element.Start, element.Length);

        protected int displayedMaxLineNumberLength;

        protected int GetMaxLineNumberLength(int maxLineNumberToDisplay)
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

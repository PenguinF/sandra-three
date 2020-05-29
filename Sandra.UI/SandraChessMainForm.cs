#region License
/*********************************************************************************
 * SandraChessMainForm.cs
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

using Eutherion;
using Eutherion.UIActions;
using Eutherion.Utils;
using Eutherion.Win;
using Eutherion.Win.MdiAppTemplate;
using Sandra.Chess.Pgn;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Sandra.UI
{
    using PgnForm = MenuCaptionBarForm<SyntaxEditor<RootPgnSyntax, IPgnSymbol, PgnErrorInfo>>;

    internal class SandraChessMainForm : SingleInstanceMainForm
    {
        public const string SandraChessMainFormUIActionPrefix = nameof(SandraChessMainForm) + ".";

        private readonly string[] commandLineArgs;

        /// <summary>
        /// List of open PGN files indexed by their path. New PGN files are indexed under the empty path.
        /// </summary>
        private readonly Dictionary<string, List<PgnForm>> OpenPgnForms
            = new Dictionary<string, List<PgnForm>>(StringComparer.OrdinalIgnoreCase);

        private MdiContainerForm mdiContainerForm;

        public SandraChessMainForm(string[] commandLineArgs)
        {
            this.commandLineArgs = commandLineArgs;

            // This hides the window at startup.
            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
        }

        public override Session RequestSession()
        {
            // Use built-in localizer if none is provided.
            var builtInEnglishLocalizer = new BuiltInEnglishLocalizer(
                LocalizedStringKeys.DefaultEnglishTranslations,
                LocalizedConsoleKeys.DefaultEnglishTranslations,
                SharedLocalizedStringKeys.DefaultEnglishTranslations(Session.ExecutableFileNameWithoutExtension),
                JsonErrorInfoExtensions.DefaultEnglishJsonErrorTranslations,
                PgnErrorInfoExtensions.DefaultEnglishPgnErrorTranslations);

            return Session.Configure(this,
                                     new SettingsProvider(),
                                     builtInEnglishLocalizer,
                                     builtInEnglishLocalizer.Dictionary,
                                     Properties.Resources.Sandra);
        }

        protected override string GetMessageToSendToExistingInstance()
        {
            // \n is a good separator character because it's illegal in file/path names
            // and cannot be easily constructed as part of a command line argument.
            return string.Join("\n", commandLineArgs);
        }

        protected override void ReceivedMessageFromAnotherInstance(string message)
        {
            string[] receivedCommandLineArgs = message.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (mdiContainerForm != null && mdiContainerForm.IsHandleCreated && !mdiContainerForm.IsDisposed)
            {
                // Activate mdiContainerForm before opening pgn files.
                mdiContainerForm.EnsureActivated();
            }

            OpenCommandLineArgs(receivedCommandLineArgs);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!HasSession)
            {
                Close();
            }
            else
            {
#if DEBUG
                PieceImages.DeployRuntimePieceImageFiles();
#endif

                // Load chess piece images.
                PieceImages.LoadChessPieceImages();

                mdiContainerForm = new MdiContainerForm();

                mdiContainerForm.Shown += (_, __) => OpenCommandLineArgs(commandLineArgs);
                mdiContainerForm.FormClosed += (_, __) => Close();

                Visible = false;

                mdiContainerForm.Show();
            }
        }

        internal void OpenCommandLineArgs(string[] commandLineArgs)
        {
            // Interpret each command line argument as a file to open.
            commandLineArgs.ForEach(pgnFileName =>
            {
                // Catch exception for each open action individually.
                try
                {
                    OpenOrActivatePgnFile(pgnFileName, isReadOnly: false);
                }
                catch (Exception exception)
                {
                    // For now, show the exception to the user.
                    // Maybe user has no access to the path, or the given file name is not a valid.
                    // TODO: analyze what error conditions can occur and handle them appropriately.
                    MessageBox.Show(
                        $"Attempt to open code file '{pgnFileName}' failed with message: '{exception.Message}'",
                        pgnFileName,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            });
        }

        private void RemovePgnForm(string key, PgnForm pgnForm)
        {
            // Remove from the list it's currently in, and remove the list from the index altogether once it's empty.
            var pgnForms = OpenPgnForms[key ?? string.Empty];
            pgnForms.Remove(pgnForm);
            if (pgnForms.Count == 0) OpenPgnForms.Remove(key ?? string.Empty);
        }

        private void AddPgnForm(string key, PgnForm pgnForm)
        {
            OpenPgnForms.GetOrAdd(key ?? string.Empty, _ => new List<PgnForm>()).Add(pgnForm);
        }

        private void OpenPgnForm(string normalizedPgnFileName, bool isReadOnly)
        {
            var pgnFile = WorkingCopyTextFile.Open(normalizedPgnFileName, null);

            var pgnForm = new PgnForm(
                new SyntaxEditor<RootPgnSyntax, IPgnSymbol, PgnErrorInfo>(
                    isReadOnly ? SyntaxEditorCodeAccessOption.ReadOnly : SyntaxEditorCodeAccessOption.Default,
                    PgnSyntaxDescriptor.Instance,
                    pgnFile,
                    SettingKeys.PgnZoom))
            {
                ClientSize = new Size(600, 600),
                ShowInTaskbar = true,
                StartPosition = FormStartPosition.CenterScreen,
            };

            pgnForm.Load += (_, __) => Session.Current.AttachFormStateAutoSaver(pgnForm, SettingKeys.PgnWindow, null);

            // Bind SaveToFile action to the MenuCaptionBarForm to show the save button in the caption area.
            pgnForm.BindAction(SharedUIAction.SaveToFile, pgnForm.DockedControl.TrySaveToFile);

            PgnStyleSelector.InitializeStyles(pgnForm.DockedControl);

            // Don't index read-only PgnForms.
            if (!isReadOnly)
            {
                AddPgnForm(normalizedPgnFileName, pgnForm);

                // Re-index when pgnFile.OpenTextFilePath changes.
                pgnFile.OpenTextFilePathChanged += (_, e) =>
                {
                    RemovePgnForm(e.PreviousOpenTextFilePath, pgnForm);
                    AddPgnForm(pgnFile.OpenTextFilePath, pgnForm);
                };

                // Remove from index when pgnForm is closed.
                pgnForm.Disposed += (_, __) =>
                {
                    RemovePgnForm(pgnFile.OpenTextFilePath, pgnForm);
                };
            }

            pgnForm.EnsureActivated();
        }

        private void OpenNewPgnFile()
        {
            // Never create as read-only.
            OpenPgnForm(null, isReadOnly: false);
        }

        private void OpenOrActivatePgnFile(string pgnFileName, bool isReadOnly)
        {
            // Normalize the file name so it gets indexed correctly.
            string normalizedPgnFileName = FileUtilities.NormalizeFilePath(pgnFileName);

            if (isReadOnly || !OpenPgnForms.TryGetValue(normalizedPgnFileName, out List<PgnForm> pgnForms))
            {
                // File path not open yet, initialize new PGN Form.
                OpenPgnForm(normalizedPgnFileName, isReadOnly);
            }
            else
            {
                // Just activate the first Form in the list.
                pgnForms[0].EnsureActivated();
            }
        }

        public static readonly DefaultUIActionBinding NewPgnFile = new DefaultUIActionBinding(
            new UIAction(SandraChessMainFormUIActionPrefix + nameof(NewPgnFile)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control | KeyModifiers.Shift, ConsoleKey.N), },
                    IsFirstInGroup = true,
                    MenuTextProvider = LocalizedStringKeys.NewGameFile.ToTextProvider(),
                },
            });

        public UIActionState TryNewPgnFile(bool perform)
        {
            if (perform) OpenNewPgnFile();
            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding OpenPgnFile = new DefaultUIActionBinding(
            new UIAction(SandraChessMainFormUIActionPrefix + nameof(OpenPgnFile)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.O), },
                    MenuTextProvider = LocalizedStringKeys.OpenGameFile.ToTextProvider(),
                    OpensDialog = true,
                },
            });

        public UIActionState TryOpenPgnFile(bool perform)
        {
            if (perform)
            {
                string extension = PgnSyntaxDescriptor.Instance.FileExtension;
                var extensionLocalizedKey = PgnSyntaxDescriptor.Instance.FileExtensionLocalizedKey;

                var openFileDialog = new OpenFileDialog
                {
                    AutoUpgradeEnabled = true,
                    DereferenceLinks = true,
                    DefaultExt = extension,
                    Filter = $"{Session.Current.CurrentLocalizer.Localize(extensionLocalizedKey)} (*.{extension})|*.{extension}|{Session.Current.CurrentLocalizer.Localize(SharedLocalizedStringKeys.AllFiles)} (*.*)|*.*",
                    SupportMultiDottedExtensions = true,
                    RestoreDirectory = true,
                    Title = Session.Current.CurrentLocalizer.Localize(LocalizedStringKeys.OpenGameFile),
                    ValidateNames = true,
                    CheckFileExists = false,
                    ShowReadOnly = true,
                };

                var dialogResult = openFileDialog.ShowDialog(this);
                if (dialogResult == DialogResult.OK)
                {
                    OpenOrActivatePgnFile(openFileDialog.FileName, openFileDialog.ReadOnlyChecked);
                }
            }

            return UIActionVisibility.Enabled;
        }
    }
}

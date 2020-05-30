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
    using PgnEditor = SyntaxEditor<RootPgnSyntax, IPgnSymbol, PgnErrorInfo>;

    internal class SandraChessMainForm : SingleInstanceMainForm
    {
        public const string SandraChessMainFormUIActionPrefix = nameof(SandraChessMainForm) + ".";

        private readonly string[] commandLineArgs;

        /// <summary>
        /// List of open PGN editors indexed by their path. New PGN files are indexed under the empty path.
        /// </summary>
        private readonly Dictionary<string, List<MenuCaptionBarForm<PgnEditor>>> OpenPgnEditors
            = new Dictionary<string, List<MenuCaptionBarForm<PgnEditor>>>(StringComparer.OrdinalIgnoreCase);

        private readonly List<MdiContainerForm> mdiContainerForms = new List<MdiContainerForm>();

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

            // First mdiContainerForm in the list gets the honor of opening the new PGN files.
            foreach (var candidate in mdiContainerForms)
            {
                if (candidate.IsHandleCreated && !candidate.IsDisposed)
                {
                    // Activate mdiContainerForm before opening pgn files.
                    candidate.EnsureActivated();
                    OpenCommandLineArgs(candidate, receivedCommandLineArgs);
                    return;
                }
            }

            var mdiContainerForm = OpenNewMdiContainerForm();
            OpenCommandLineArgs(mdiContainerForm, receivedCommandLineArgs);
            mdiContainerForm.Show();
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

                Visible = false;
                var mdiContainerForm = OpenNewMdiContainerForm();

                // Only the first one should auto-save and open the stored command line arguments.
                mdiContainerForm.Load += MdiContainerForm_Load;
                mdiContainerForm.Show();
            }
        }

        private MdiContainerForm OpenNewMdiContainerForm()
        {
            var mdiContainerForm = new MdiContainerForm();

            mdiContainerForm.FormClosed += (_, __) =>
            {
                mdiContainerForms.Remove(mdiContainerForm);

                // Close the entire process after the last form is closed.
                if (mdiContainerForms.Count == 0) Close();
            };

            mdiContainerForms.Add(mdiContainerForm);

            return mdiContainerForm;
        }

        private void MdiContainerForm_Load(object sender, EventArgs e)
        {
            MdiContainerForm mdiContainerForm = (MdiContainerForm)sender;

            // Initialize from settings if available.
            Session.Current.AttachFormStateAutoSaver(
                mdiContainerForm,
                SettingKeys.Window,
                () =>
                {
                    // Show in the center of the monitor where the mouse currently is.
                    var activeScreen = Screen.FromPoint(MousePosition);
                    Rectangle workingArea = activeScreen.WorkingArea;

                    // Two thirds the size of the active monitor's working area.
                    workingArea.Inflate(-workingArea.Width / 6, -workingArea.Height / 6);

                    // Update the bounds of the form.
                    mdiContainerForm.SetBounds(workingArea.X, workingArea.Y, workingArea.Width, workingArea.Height, BoundsSpecified.All);
                });

            OpenCommandLineArgs(mdiContainerForm, commandLineArgs);
        }

        internal void OpenCommandLineArgs(MdiContainerForm mdiContainerForm, string[] commandLineArgs)
        {
            // Interpret each command line argument as a file to open.
            commandLineArgs.ForEach(pgnFileName =>
            {
                // Catch exception for each open action individually.
                try
                {
                    NewOrExistingPgnEditor(mdiContainerForm, pgnFileName, isReadOnly: false).EnsureActivated();
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

        internal void RemovePgnEditor(string key, MenuCaptionBarForm<PgnEditor> pgnEditor)
        {
            // Remove from the list it's currently in, and remove the list from the index altogether once it's empty.
            var pgnEditors = OpenPgnEditors[key ?? string.Empty];
            pgnEditors.Remove(pgnEditor);
            if (pgnEditors.Count == 0) OpenPgnEditors.Remove(key ?? string.Empty);
        }

        internal void AddPgnEditor(string key, MenuCaptionBarForm<PgnEditor> pgnEditor)
        {
            OpenPgnEditors.GetOrAdd(key ?? string.Empty, _ => new List<MenuCaptionBarForm<PgnEditor>>()).Add(pgnEditor);
        }

        private MenuCaptionBarForm<PgnEditor> NewPgnEditor(MdiContainerForm mdiContainerForm, string normalizedPgnFileName, bool isReadOnly)
        {
            var pgnFile = WorkingCopyTextFile.Open(normalizedPgnFileName, null);

            var pgnForm = new MenuCaptionBarForm<PgnEditor>(
                new PgnEditor(
                    isReadOnly ? SyntaxEditorCodeAccessOption.ReadOnly : SyntaxEditorCodeAccessOption.Default,
                    PgnSyntaxDescriptor.Instance,
                    pgnFile,
                    SettingKeys.PgnZoom));

            // Bind SaveToFile action to the MenuCaptionBarForm to show the save button in the caption area.
            pgnForm.BindAction(SharedUIAction.SaveToFile, pgnForm.DockedControl.TrySaveToFile);

            PgnStyleSelector.InitializeStyles(pgnForm.DockedControl);

            // Don't index read-only PgnForms.
            if (!isReadOnly)
            {
                AddPgnEditor(normalizedPgnFileName, pgnForm);

                // Re-index when pgnFile.OpenTextFilePath changes.
                pgnFile.OpenTextFilePathChanged += (_, e) =>
                {
                    RemovePgnEditor(e.PreviousOpenTextFilePath, pgnForm);
                    AddPgnEditor(pgnFile.OpenTextFilePath, pgnForm);
                };

                // Remove from index when pgnForm is closed.
                pgnForm.Disposed += (_, __) =>
                {
                    RemovePgnEditor(pgnFile.OpenTextFilePath, pgnForm);
                };
            }

            return pgnForm;
        }

        internal void OpenNewPgnEditor(MdiContainerForm mdiContainerForm)
        {
            // Never create as read-only.
            NewPgnEditor(mdiContainerForm, null, isReadOnly: false).EnsureActivated();
        }

        internal MenuCaptionBarForm<PgnEditor> NewOrExistingPgnEditor(MdiContainerForm mdiContainerForm, string pgnFileName, bool isReadOnly)
        {
            // Normalize the file name so it gets indexed correctly.
            string normalizedPgnFileName = FileUtilities.NormalizeFilePath(pgnFileName);

            if (isReadOnly || !OpenPgnEditors.TryGetValue(normalizedPgnFileName, out List<MenuCaptionBarForm<PgnEditor>> pgnForms))
            {
                // File path not open yet, initialize new PGN editor.
                return NewPgnEditor(mdiContainerForm, normalizedPgnFileName, isReadOnly);
            }
            else
            {
                // Just return the first editor in the list.
                return pgnForms[0];
            }
        }
    }
}

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
using Eutherion.Win;
using Eutherion.Win.MdiAppTemplate;
using Sandra.Chess.Pgn;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        private readonly Dictionary<string, List<PgnEditor>> OpenPgnEditors
            = new Dictionary<string, List<PgnEditor>>(StringComparer.OrdinalIgnoreCase);

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
                    candidate.OpenCommandLineArgs(receivedCommandLineArgs);
                    return;
                }
            }

            var mdiContainerForm = OpenNewMdiContainerForm();
            mdiContainerForm.OpenCommandLineArgs(receivedCommandLineArgs);
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

            mdiContainerForm.OpenCommandLineArgs(commandLineArgs);
        }

        internal bool TryGetPgnEditors(string key, out List<PgnEditor> pgnEditors)
            => OpenPgnEditors.TryGetValue(key, out pgnEditors);

        internal void RemovePgnEditor(string key, PgnEditor pgnEditor)
        {
            // Remove from the list it's currently in, and remove the list from the index altogether once it's empty.
            var pgnEditors = OpenPgnEditors[key ?? string.Empty];
            pgnEditors.Remove(pgnEditor);
            if (pgnEditors.Count == 0) OpenPgnEditors.Remove(key ?? string.Empty);
        }

        internal void AddPgnEditor(string key, PgnEditor pgnEditor)
        {
            OpenPgnEditors.GetOrAdd(key ?? string.Empty, _ => new List<PgnEditor>()).Add(pgnEditor);
        }
    }
}

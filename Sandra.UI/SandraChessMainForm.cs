#region License
/*********************************************************************************
 * SandraChessMainForm.cs
 *
 * Copyright (c) 2004-2022 Henk Nicolai
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

using Eutherion.Win.MdiAppTemplate;
using Eutherion.Win.Storage;
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
        private readonly string[] commandLineArgs;

        /// <summary>
        /// List of open PGN editors indexed by their path. New PGN files are indexed under the empty path.
        /// </summary>
        private readonly Dictionary<string, List<PgnEditor>> OpenPgnEditors
            = new Dictionary<string, List<PgnEditor>>(StringComparer.OrdinalIgnoreCase);

        private readonly List<MdiContainerWithState> mdiContainers = new List<MdiContainerWithState>();

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

            // Most recently activated MdiContainerForm gets the honor of opening the new PGN files.
            foreach (var candidate in mdiContainers.Select(x => x.Form))
            {
                if (candidate.IsHandleCreated && !candidate.IsDisposed)
                {
                    candidate.OpenCommandLineArgs(receivedCommandLineArgs);
                    return;
                }
            }

            ShowNewMdiContainerForm(receivedCommandLineArgs);
        }

        private bool FindMdiContainer(MdiContainerForm form, out int index)
        {
            // Linear search.
            for (index = 0; index < mdiContainers.Count; index++)
            {
                if (mdiContainers[index].Form == form) return true;
            }

            return false;
        }

        private void AutoSaveMdiContainerList()
        {
            Session.Current.AutoSave.Persist(SettingKeys.Windows, mdiContainers.Select(x => x.State.FormState));
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

                if (Session.Current.TryGetAutoSaveValue(SettingKeys.Windows, out IEnumerable<PersistableFormState> states))
                {
                    // First restore all states, then attach auto-save events.
                    // Create all forms before doing anything else.
                    foreach (PersistableFormState formState in states)
                    {
                        mdiContainers.Add(new MdiContainerWithState(new MdiContainerForm(), new MdiContainerState(formState)));
                    }

                    // Activate from back to front.
                    for (int i = mdiContainers.Count - 1; i >= 0; i--)
                    {
                        MdiContainerWithState formWithState = mdiContainers[i];
                        formWithState.Form.Load += (_, __) => RestoreMdiContainerState(formWithState);

                        if (i > 0)
                        {
                            // Activate by activating the docked control.
                            formWithState.Form.DockedControl.EnsureActivated();
                        }
                        else
                        {
                            // Most recently activated window should open command line arguments.
                            // Alternative is to open an additional new window, but this is inconsistent with ReceivedMessageFromAnotherInstance().
                            // TODO: maybe turn this into a preference? E.g. "open_files_in_new_window_on_launch".
                            formWithState.Form.OpenCommandLineArgs(commandLineArgs);
                        }
                    }

                    // Only now register auto-save events.
                    foreach (MdiContainerWithState formWithState in mdiContainers)
                    {
                        RegisterMdiContainerFormEvents(formWithState.Form);
                        AttachFormStateAutoSaver(formWithState);
                    }
                }
                else
                {
                    ShowNewMdiContainerForm(commandLineArgs);
                }
            }
        }

        private void ShowNewMdiContainerForm(string[] commandLineArgs)
        {
            CreateNewMdiContainerForm(setCenterWorkingScreenStartPosition: true)
                .OpenCommandLineArgs(commandLineArgs);
        }

        internal MdiContainerForm CreateNewMdiContainerForm(bool setCenterWorkingScreenStartPosition)
        {
            var mdiContainerForm = new MdiContainerForm();
            RegisterMdiContainerFormEvents(mdiContainerForm);
            var formWithDefaultState = new MdiContainerWithState(mdiContainerForm, new MdiContainerState(new PersistableFormState(false, Rectangle.Empty)));
            mdiContainers.Add(formWithDefaultState);
            mdiContainerForm.Load += (_, __) =>
            {
                if (setCenterWorkingScreenStartPosition) SetCenterWorkingScreenStartPosition(mdiContainerForm);
                AttachFormStateAutoSaver(formWithDefaultState);
            };
            return mdiContainerForm;
        }

        private void RegisterMdiContainerFormEvents(MdiContainerForm mdiContainerForm)
        {
            mdiContainerForm.FormClosed += (sender, _) =>
            {
                if (FindMdiContainer((MdiContainerForm)sender, out int index))
                {
                    mdiContainers.RemoveAt(index);

                    if (mdiContainers.Count > 0)
                    {
                        AutoSaveMdiContainerList();
                    }
                    else
                    {
                        // Close the entire process after the last form is closed.
                        // When the application is reopened, the state of this last form is restored.
                        Close();
                    }
                }
            };

            mdiContainerForm.Activated += (sender, _) =>
            {
                // Bring to front of list if activated.
                if (FindMdiContainer((MdiContainerForm)sender, out int index))
                {
                    var activatedFormWithState = mdiContainers[index];
                    mdiContainers.RemoveAt(index);
                    mdiContainers.Insert(0, activatedFormWithState);
                    AutoSaveMdiContainerList();
                }
            };
        }

        private void RestoreMdiContainerState(MdiContainerWithState mdiContainerWithState)
        {
            MdiContainerForm mdiContainerForm = mdiContainerWithState.Form;
            PersistableFormState formState = mdiContainerWithState.State.FormState;

            bool boundsInitialized = false;

            Rectangle targetBounds = formState.Bounds;

            // If all bounds are known initialize from those.
            // Do make sure the window ends up on a visible working area.
            targetBounds.Intersect(Screen.GetWorkingArea(targetBounds));
            if (targetBounds.Width >= mdiContainerForm.MinimumSize.Width && targetBounds.Height >= mdiContainerForm.MinimumSize.Height)
            {
                mdiContainerForm.SetBounds(targetBounds.Left, targetBounds.Top, targetBounds.Width, targetBounds.Height, BoundsSpecified.All);
                boundsInitialized = true;
            }

            // Determine a window state independently if no formState was applied successfully.
            if (!boundsInitialized)
            {
                SetCenterWorkingScreenStartPosition(mdiContainerForm);
            }

            // Restore maximized setting after setting the Bounds.
            if (formState.Maximized)
            {
                mdiContainerForm.WindowState = FormWindowState.Maximized;
            }
        }

        private void AttachFormStateAutoSaver(MdiContainerWithState mdiContainerWithState)
        {
            PersistableFormState formState = mdiContainerWithState.State.FormState;
            formState.AttachTo(mdiContainerWithState.Form);
            formState.Changed += (_, __) => AutoSaveMdiContainerList();
        }

        private static void SetCenterWorkingScreenStartPosition(MdiContainerForm mdiContainerForm)
        {
            // Show in the center of the monitor where the mouse currently is.
            var activeScreen = Screen.FromPoint(MousePosition);
            Rectangle workingArea = activeScreen.WorkingArea;

            // Two thirds the size of the active monitor's working area.
            workingArea.Inflate(-workingArea.Width / 6, -workingArea.Height / 6);

            // Update the bounds of the form.
            mdiContainerForm.SetBounds(workingArea.X, workingArea.Y, workingArea.Width, workingArea.Height, BoundsSpecified.All);
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

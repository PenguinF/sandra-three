﻿#region License
/*********************************************************************************
 * MdiContainerForm.cs
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
using Eutherion.Win.UIActions;
using Sandra.Chess.Pgn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Sandra.UI
{
    using PgnEditor = SyntaxEditor<RootPgnSyntax, IPgnSymbol, PgnErrorInfo>;

    /// <summary>
    /// Main MdiContainer Form.
    /// </summary>
    public partial class MdiContainerForm : MenuCaptionBarForm<MdiTabControl>, IWeakEventTarget
    {
        private static MdiTabControl CreateMdiTabControl()
        {
            var mdiTabControl = new MdiTabControl();

            mdiTabControl.DockProperties.CaptionHeight = 30;
            mdiTabControl.DockProperties.CaptionText = Session.ExecutableFileNameWithoutExtension;
            mdiTabControl.DockProperties.Icon = Session.Current.ApplicationIcon;

            // Define the main menu.
            var mainMenuRootNodes = new List<MainMenuDropDownItem>();

            if (Session.Current.RegisteredLocalizers.Count() >= 2)
            {
                // More than one localizer: can switch between them.
                var langMenu = new List<Union<DefaultUIActionBinding, MainMenuDropDownItem>>();
                langMenu.AddRange(Session.Current.RegisteredLocalizers.Select(x => (Union<DefaultUIActionBinding, MainMenuDropDownItem>)x.SwitchToLangUIActionBinding));

                mainMenuRootNodes.Add(new MainMenuDropDownItem
                {
                    Container = new UIMenuNode.Container(null, SharedResources.globe.ToImageProvider()),
                    DropDownItems = langMenu
                });
            }

            mainMenuRootNodes.Add(new MainMenuDropDownItem
            {
                Container = new UIMenuNode.Container(SharedLocalizedStringKeys.File.ToTextProvider()),
                DropDownItems = new List<Union<DefaultUIActionBinding, MainMenuDropDownItem>>
                {
                    NewPgnFile,
                    OpenPgnFile,
                    SharedUIAction.Exit,
                }
            });

            mainMenuRootNodes.Add(new MainMenuDropDownItem
            {
                Container = new UIMenuNode.Container(SharedLocalizedStringKeys.Edit.ToTextProvider()),
                DropDownItems = new List<Union<DefaultUIActionBinding, MainMenuDropDownItem>>
                {
                    OpenNewPlayingBoard,
                    new MainMenuDropDownItem
                    {
                        Container = new UIMenuNode.Container(LocalizedStringKeys.GoTo.ToTextProvider()) { IsFirstInGroup = true },
                        DropDownItems = new List<Union<DefaultUIActionBinding, MainMenuDropDownItem>>
                        {
                            // Add all these to a submenu.
                            InteractiveGame.GotoStart,
                            InteractiveGame.GotoFirstMove,
                            InteractiveGame.FastNavigateBackward,
                            InteractiveGame.GotoPreviousMove,
                            InteractiveGame.GotoNextMove,
                            InteractiveGame.FastNavigateForward,
                            InteractiveGame.GotoLastMove,
                            InteractiveGame.GotoEnd,
                            InteractiveGame.GotoPreviousVariation,
                            InteractiveGame.GotoNextVariation,
                        }
                    },

                    InteractiveGame.PromoteActiveVariation,
                    InteractiveGame.DemoteActiveVariation,
                    InteractiveGame.BreakActiveVariation,
                    InteractiveGame.DeleteActiveVariation,
                    StandardChessBoard.FlipBoard,
                    StandardChessBoard.TakeScreenshot,

                    SharedUIAction.Undo,
                    SharedUIAction.Redo,
                    SharedUIAction.CutSelectionToClipBoard,
                    SharedUIAction.CopySelectionToClipBoard,
                    SharedUIAction.PasteSelectionFromClipBoard,
                    SharedUIAction.SelectAllText,
                }
            });

            // Provide ContextMenuUIActionInterfaces for GotoChessBoardForm and GotoMovesForm
            // because they would otherwise remain invisible.
            var modifiedGotoChessBoardForm = new DefaultUIActionBinding(
                InteractiveGame.GotoChessBoardForm.Action,
                new ImplementationSet<IUIActionInterface>
                {
                    new CombinedUIActionInterface
                    {
                        Shortcuts = InteractiveGame.GotoChessBoardForm.DefaultInterfaces.Get<IShortcutKeysUIActionInterface>().Shortcuts,
                        IsFirstInGroup = true,
                        MenuTextProvider = LocalizedStringKeys.Chessboard.ToTextProvider(),
                    },
                });

            mainMenuRootNodes.Add(new MainMenuDropDownItem
            {
                Container = new UIMenuNode.Container(SharedLocalizedStringKeys.View.ToTextProvider()),
                DropDownItems = new List<Union<DefaultUIActionBinding, MainMenuDropDownItem>>
                {
                    modifiedGotoChessBoardForm,
                    SharedUIAction.ZoomIn,
                    SharedUIAction.ZoomOut,
                    SharedUIAction.ShowErrorPane,
                    SharedUIAction.GoToPreviousError,
                    SharedUIAction.GoToNextError,
                }
            });

            mainMenuRootNodes.Add(new MainMenuDropDownItem
            {
                Container = new UIMenuNode.Container(SharedLocalizedStringKeys.Tools.ToTextProvider()),
                DropDownItems = new List<Union<DefaultUIActionBinding, MainMenuDropDownItem>>
                {
                    Session.EditPreferencesFile,
                    Session.ShowDefaultSettingsFile,
                    Session.EditCurrentLanguage,
                    Session.OpenLocalAppDataFolder,
                    Session.OpenExecutableFolder,
                }
            });

            mainMenuRootNodes.Add(new MainMenuDropDownItem
            {
                Container = new UIMenuNode.Container(SharedLocalizedStringKeys.Help.ToTextProvider()),
                DropDownItems = new List<Union<DefaultUIActionBinding, MainMenuDropDownItem>>
                {
                    Session.OpenAbout,
                    Session.ShowCredits,
                }
            });

            mdiTabControl.DockProperties.MainMenuItems = mainMenuRootNodes;

            return mdiTabControl;
        }

        public MdiContainerForm() : base(CreateMdiTabControl())
        {
            // Actions which have their handler in this instance.
            foreach (var localizer in Session.Current.RegisteredLocalizers)
            {
                this.BindAction(localizer.SwitchToLangUIActionBinding, localizer.TrySwitchToLang);
            }

            this.BindAction(NewPgnFile, TryNewPgnFile);
            this.BindAction(OpenPgnFile, TryOpenPgnFile);
            this.BindAction(SharedUIAction.Exit, TryExit);

            this.BindAction(Session.EditPreferencesFile, Session.Current.TryEditPreferencesFile());
            this.BindAction(Session.ShowDefaultSettingsFile, Session.Current.TryShowDefaultSettingsFile());
            this.BindAction(Session.OpenAbout, Session.Current.TryOpenAbout(this));
            this.BindAction(Session.ShowCredits, Session.Current.TryShowCredits(this));
            this.BindAction(Session.EditCurrentLanguage, Session.Current.TryEditCurrentLanguage());
            this.BindAction(Session.OpenLocalAppDataFolder, Session.Current.TryOpenLocalAppDataFolder());
            this.BindAction(Session.OpenExecutableFolder, Session.Current.TryOpenExecutableFolder());

            AllowDrop = true;

            ObservableStyle.NotifyChange += ObservableStyle_NotifyChange;
        }

        private void ObservableStyle_NotifyChange(object sender, EventArgs e)
        {
            DockedControl.BackColor = ObservableStyle.BackColor;
            DockedControl.ForeColor = ObservableStyle.ForeColor;
            DockedControl.Font = ObservableStyle.Font;
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }

            base.OnDragEnter(e);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }

            base.OnDragOver(e);
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                OpenCommandLineArgs((string[])e.Data.GetData(DataFormats.FileDrop));
            }

            base.OnDragDrop(e);
        }

        internal void OpenCommandLineArgs(string[] commandLineArgs)
        {
            PgnEditor lastOpenedPgnEditor = null;

            // Interpret each command line argument as a file to open.
            commandLineArgs.ForEach(pgnFileName =>
            {
                // Catch exception for each open action individually.
                try
                {
                    lastOpenedPgnEditor = NewOrExistingPgnEditor(pgnFileName, isReadOnly: false);
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

            if (lastOpenedPgnEditor != null)
            {
                // Only activate the last opened PGN editor.
                lastOpenedPgnEditor.EnsureActivated();
            }
            else if (DockedControl.TabPages.Count == 0)
            {
                // Open default new untitled file.
                OpenNewPgnEditor();
            }
            else
            {
                // If no arguments were given, just activate the form.
                DockedControl.EnsureActivated();
            }
        }

        private PgnEditor NewPgnEditor(string normalizedPgnFileName, bool isReadOnly)
        {
            var pgnFile = WorkingCopyTextFile.Open(normalizedPgnFileName, null);

            var pgnEditor = new PgnEditor(
                isReadOnly ? SyntaxEditorCodeAccessOption.ReadOnly : SyntaxEditorCodeAccessOption.Default,
                PgnSyntaxDescriptor.Instance,
                pgnFile,
                SettingKeys.PgnZoom);

            PgnStyleSelector.InitializeStyles(pgnEditor);

            // Don't index read-only pgn editors.
            if (!isReadOnly)
            {
                Program.MainForm.AddPgnEditor(normalizedPgnFileName, pgnEditor);

                // Re-index when pgnFile.OpenTextFilePath changes.
                pgnFile.OpenTextFilePathChanged += (_, e) =>
                {
                    Program.MainForm.RemovePgnEditor(e.PreviousOpenTextFilePath, pgnEditor);
                    Program.MainForm.AddPgnEditor(pgnFile.OpenTextFilePath, pgnEditor);
                };

                // Remove from index when pgnEditor is closed.
                pgnEditor.Disposed += (_, __) =>
                {
                    Program.MainForm.RemovePgnEditor(pgnFile.OpenTextFilePath, pgnEditor);
                };
            }

            pgnEditor.BindAction(OpenNewPlayingBoard, perform => TryOpenNewPlayingBoard(pgnEditor, perform));

            // Open as new tab page.
            DockedControl.TabPages.Add(new MdiTabPage<PgnEditor>(pgnEditor));

            return pgnEditor;
        }

        private void OpenNewPgnEditor()
        {
            // Never create as read-only.
            NewPgnEditor(null, isReadOnly: false).EnsureActivated();
        }

        private PgnEditor NewOrExistingPgnEditor(string pgnFileName, bool isReadOnly)
        {
            // Normalize the file name so it gets indexed correctly.
            string normalizedPgnFileName = FileUtilities.NormalizeFilePath(pgnFileName);

            if (isReadOnly || !Program.MainForm.TryGetPgnEditors(normalizedPgnFileName, out List<PgnEditor> pgnEditors))
            {
                // File path not open yet, initialize new PGN editor.
                return NewPgnEditor(normalizedPgnFileName, isReadOnly);
            }
            else
            {
                // Just return the first editor in the list. It may be docked somewhere else.
                return pgnEditors[0];
            }
        }
    }
}

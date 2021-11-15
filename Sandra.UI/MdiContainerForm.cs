#region License
/*********************************************************************************
 * MdiContainerForm.cs
 *
 * Copyright (c) 2004-2021 Henk Nicolai
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
using Eutherion.Win;
using Eutherion.Win.Controls;
using Eutherion.Win.MdiAppTemplate;
using Eutherion.Win.UIActions;
using Sandra.Chess.Pgn;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
            var mdiTabControl = new MdiTabControl(Session.ExecutableFileNameWithoutExtension);

            mdiTabControl.DockProperties.CaptionHeight = 30;
            mdiTabControl.DockProperties.Icon = Session.Current.ApplicationIcon;

            // Define the main menu.
            var mainMenuRootNodes = new List<MainMenuDropDownItem>();

            if (Session.Current.RegisteredLocalizers.Count() >= 2)
            {
                // More than one localizer: can switch between them.
                var langWindowMenu = new List<Union<DefaultUIActionBinding, MainMenuDropDownItem>>();
                langWindowMenu.AddRange(Session.Current.RegisteredLocalizers.Select(x => (Union<DefaultUIActionBinding, MainMenuDropDownItem>)x.SwitchToLangUIActionBinding));

                langWindowMenu.Add(SharedUIAction.WindowMenuRestore);
                langWindowMenu.Add(SharedUIAction.WindowMenuMove);
                langWindowMenu.Add(SharedUIAction.WindowMenuSize);
                langWindowMenu.Add(SharedUIAction.WindowMenuMinimize);
                langWindowMenu.Add(SharedUIAction.WindowMenuMaximize);
                langWindowMenu.Add(SharedUIAction.Close);

                mainMenuRootNodes.Add(new MainMenuDropDownItem
                {
                    Container = new UIMenuNode.Container(null, SharedResources.globe.ToImageProvider()),
                    DropDownItems = langWindowMenu
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
                            StandardChessBoard.GotoStart,
                            StandardChessBoard.GotoFirstMove,
                            StandardChessBoard.FastNavigateBackward,
                            StandardChessBoard.GotoPreviousMove,
                            StandardChessBoard.GotoNextMove,
                            StandardChessBoard.FastNavigateForward,
                            StandardChessBoard.GotoLastMove,
                            StandardChessBoard.GotoEnd,
                            StandardChessBoard.GotoPreviousVariation,
                            StandardChessBoard.GotoNextVariation,
                        }
                    },

                    StandardChessBoard.PromoteActiveVariation,
                    StandardChessBoard.DemoteActiveVariation,
                    StandardChessBoard.BreakActiveVariation,
                    StandardChessBoard.DeleteActiveVariation,
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

            mainMenuRootNodes.Add(new MainMenuDropDownItem
            {
                Container = new UIMenuNode.Container(SharedLocalizedStringKeys.View.ToTextProvider()),
                DropDownItems = new List<Union<DefaultUIActionBinding, MainMenuDropDownItem>>
                {
                    OpenGame,
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
            this.BindAction(SharedUIAction.Exit, TryClose);

            this.BindAction(Session.EditPreferencesFile, Session.Current.TryEditPreferencesFile(DockedControl));
            this.BindAction(Session.ShowDefaultSettingsFile, Session.Current.TryShowDefaultSettingsFile(DockedControl));
            this.BindAction(Session.OpenAbout, Session.Current.TryOpenAbout(this));
            this.BindAction(Session.ShowCredits, Session.Current.TryShowCredits(this));
            this.BindAction(Session.EditCurrentLanguage, Session.Current.TryEditCurrentLanguage(DockedControl));
            this.BindAction(Session.OpenLocalAppDataFolder, Session.Current.TryOpenLocalAppDataFolder());
            this.BindAction(Session.OpenExecutableFolder, Session.Current.TryOpenExecutableFolder());

            AllowDrop = true;

            ObservableStyle.NotifyChange += ObservableStyle_NotifyChange;

            DockedControl.AfterTabRemoved += DockedControl_AfterTabRemoved;
            DockedControl.AfterTabClosed += DockedControl_AfterTabRemoved;
        }

        private void ObservableStyle_NotifyChange(object sender, EventArgs e)
        {
            DockedControl.BackColor = ObservableStyle.BackColor;
            DockedControl.ForeColor = ObservableStyle.ForeColor;
            DockedControl.Font = ObservableStyle.Font;
            DockedControl.InactiveTabHeaderHoverColor = ObservableStyle.HoverColor;
            DockedControl.InactiveTabHeaderHoverBorderColor = ObservableStyle.HoverBorderColor;
        }

        private void DockedControl_AfterTabRemoved(object sender, GlyphTabControlEventArgs e)
        {
            MdiTabControl mdiTabControl = (MdiTabControl)sender;
            if (mdiTabControl.TabPages.Count == 0)
            {
                // If the last tab page is closed, close the entire form.
                Close();
            }
            else if (mdiTabControl.ActiveTabPageIndex < 0)
            {
                // If the active tab page was closed, activate the tab page to the right. If there is none, go to the left.
                int targetIndex = e.TabPageIndex;
                if (targetIndex >= mdiTabControl.TabPages.Count) targetIndex = mdiTabControl.TabPages.Count - 1;
                mdiTabControl.ActivateTab(targetIndex);
            }
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Hide before disposing. Lots of initializing Scintilla editors will delay the close action by seconds.
            if (!e.Cancel) Visible = false;
        }

        internal void OpenCommandLineArgs(string[] commandLineArgs, bool isReadOnly = false)
        {
            PgnEditor lastOpenedPgnEditor = null;

            // Interpret each command line argument as a file to open.
            commandLineArgs.ForEach(pgnFileName =>
            {
                // Catch exception for each open action individually.
                try
                {
                    lastOpenedPgnEditor = NewOrExistingPgnEditor(pgnFileName, isReadOnly);
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

            pgnEditor.BindAction(OpenNewPlayingBoard, perform => TryOpenNewPlayingBoard(pgnEditor, perform));
            pgnEditor.BindAction(OpenGame, perform => TryOpenGame(pgnEditor, perform));
            pgnEditor.BindActions(pgnEditor.StandardSyntaxEditorUIActionBindings);
            UIMenu.AddTo(pgnEditor);

            pgnEditor.DoubleClick += (_, __) => TryOpenGame(pgnEditor, true);

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

        /// <summary>
        /// Opens a chess board for a certain game at a current position.
        /// </summary>
        private StandardChessBoard OpenChessBoard(PgnEditor ownerPgnEditor, Chess.Game game, string white, string black, string whiteElo, string blackElo)
        {
            var newChessBoard = new StandardChessBoard
            {
                Game = game,
                PieceImages = PieceImages.ImageArray
            };

            newChessBoard.PlayingBoard.ForegroundImageRelativeSize = 0.9f;

            newChessBoard.PlayingBoard.BindActions(new UIActionBindings
            {
                { StandardChessBoard.GotoStart, newChessBoard.TryGotoStart },
                { StandardChessBoard.GotoFirstMove, newChessBoard.TryGotoFirstMove },
                { StandardChessBoard.FastNavigateBackward, newChessBoard.TryFastNavigateBackward },
                { StandardChessBoard.GotoPreviousMove, newChessBoard.TryGotoPreviousMove },
                { StandardChessBoard.GotoNextMove, newChessBoard.TryGotoNextMove },
                { StandardChessBoard.FastNavigateForward, newChessBoard.TryFastNavigateForward },
                { StandardChessBoard.GotoLastMove, newChessBoard.TryGotoLastMove },
                { StandardChessBoard.GotoEnd, newChessBoard.TryGotoEnd },

                { StandardChessBoard.GotoPreviousVariation, newChessBoard.TryGotoPreviousVariation },
                { StandardChessBoard.GotoNextVariation, newChessBoard.TryGotoNextVariation },

                { StandardChessBoard.PromoteActiveVariation, newChessBoard.TryPromoteActiveVariation },
                { StandardChessBoard.DemoteActiveVariation, newChessBoard.TryDemoteActiveVariation },
                { StandardChessBoard.BreakActiveVariation, newChessBoard.TryBreakActiveVariation },
                { StandardChessBoard.DeleteActiveVariation, newChessBoard.TryDeleteActiveVariation },

                { StandardChessBoard.FlipBoard, newChessBoard.TryFlipBoard },
                { StandardChessBoard.TakeScreenshot, newChessBoard.TryTakeScreenshot },

                { SharedUIAction.ZoomIn, newChessBoard.TryZoomIn },
                { SharedUIAction.ZoomOut, newChessBoard.TryZoomOut },
            });

            UIMenu.AddTo(newChessBoard.PlayingBoard);

            if (string.IsNullOrWhiteSpace(white)) white = "?";
            if (string.IsNullOrWhiteSpace(black)) black = "?";
            if (!string.IsNullOrWhiteSpace(whiteElo)) white = $"{white} ({whiteElo})";
            if (!string.IsNullOrWhiteSpace(blackElo)) black = $"{black} ({blackElo})";

            newChessBoard.DockProperties.CaptionText = $"{white} - {black}";
            newChessBoard.DockProperties.CaptionHeight = 24;
            newChessBoard.DockProperties.Icon = Session.Current.ApplicationIcon;

            var newChessBoardForm = new MenuCaptionBarForm<StandardChessBoard>(newChessBoard)
            {
                Owner = ownerPgnEditor.FindForm(),
                MaximizeBox = false,
                ClientSize = new Size(400, 400),
            };

            StandardChessBoard.ConstrainClientSize(newChessBoardForm);

            return newChessBoard;
        }
    }
}

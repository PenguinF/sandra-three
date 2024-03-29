﻿#region License
/*********************************************************************************
 * MdiContainerForm.cs
 *
 * Copyright (c) 2004-2023 Henk Nicolai
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

            var systemWindowMenu = new List<Union<UIAction, MainMenuDropDownItem>>();

            if (Session.Current.RegisteredLocalizers.Skip(1).Any())
            {
                // More than one localizer: can switch between them.
                systemWindowMenu.AddRange(Session.Current.RegisteredLocalizers.Select(
                    x => Union<UIAction, MainMenuDropDownItem>.Option1(x.SwitchToLangUIActionBinding)));
            }

            systemWindowMenu.Add(SharedUIAction.WindowMenuRestore);
            systemWindowMenu.Add(SharedUIAction.WindowMenuMove);
            systemWindowMenu.Add(SharedUIAction.WindowMenuSize);
            systemWindowMenu.Add(SharedUIAction.WindowMenuMinimize);
            systemWindowMenu.Add(SharedUIAction.WindowMenuMaximize);
            systemWindowMenu.Add(SharedUIAction.Close);

            mainMenuRootNodes.Add(new MainMenuDropDownItem
            {
                Container = new UIMenuNode.Container(null, SharedResources.globe.ToImageProvider()),
                DropDownItems = systemWindowMenu
            });

            mainMenuRootNodes.Add(new MainMenuDropDownItem
            {
                Container = new UIMenuNode.Container(SharedLocalizedStringKeys.File.ToTextProvider()),
                DropDownItems = new List<Union<UIAction, MainMenuDropDownItem>>
                {
                    NewPgnFile,
                    OpenPgnFile,
                    SharedUIAction.Exit,
                }
            });

            mainMenuRootNodes.Add(new MainMenuDropDownItem
            {
                Container = new UIMenuNode.Container(SharedLocalizedStringKeys.Edit.ToTextProvider()),
                DropDownItems = new List<Union<UIAction, MainMenuDropDownItem>>
                {
                    OpenNewPlayingBoard,
                    OpenGame,
                    new MainMenuDropDownItem
                    {
                        Container = new UIMenuNode.Container(LocalizedStringKeys.GoTo.ToTextProvider()) { IsFirstInGroup = true },
                        DropDownItems = new List<Union<UIAction, MainMenuDropDownItem>>
                        {
                            // Add all these to a submenu.
                            StandardChessBoard.GotoFirstMove,
                            StandardChessBoard.FastNavigateBackward,
                            StandardChessBoard.GotoPreviousMove,
                            StandardChessBoard.GotoNextMove,
                            StandardChessBoard.FastNavigateForward,
                            StandardChessBoard.GotoLastMove,
                            StandardChessBoard.GotoPreviousVariation,
                            StandardChessBoard.GotoNextVariation,
                        }
                    },

                    StandardChessBoard.PromoteActiveVariation,
                    StandardChessBoard.DemoteActiveVariation,
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
                DropDownItems = new List<Union<UIAction, MainMenuDropDownItem>>
                {
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
                DropDownItems = new List<Union<UIAction, MainMenuDropDownItem>>
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
                DropDownItems = new List<Union<UIAction, MainMenuDropDownItem>>
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
            DockedControl.RequestTear += DockedControl_RequestTear;
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

        private MenuCaptionBarForm<MdiTabControl> DockedControl_RequestTear()
            => Program.MainForm.CreateNewMdiContainerForm(setCenterWorkingScreenStartPosition: false);

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

            pgnEditor.DoubleClick += (_, __) =>
            {
                try
                {
                    TryOpenGame(pgnEditor, true);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            };

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
        private StandardChessBoard OpenChessBoard(PgnEditor ownerPgnEditor, Chess.Game game)
        {
            var newChessBoard = new StandardChessBoard
            {
                Game = game,
                PieceImages = PieceImages.ImageArray
            };

            newChessBoard.PlayingBoard.ForegroundImageRelativeSize = 0.9f;

            newChessBoard.PlayingBoard.BindActions(new UIActionBindings
            {
                { StandardChessBoard.GotoFirstMove, newChessBoard.TryGotoFirstMove },
                { StandardChessBoard.FastNavigateBackward, newChessBoard.TryFastNavigateBackward },
                { StandardChessBoard.GotoPreviousMove, newChessBoard.TryGotoPreviousMove },
                { StandardChessBoard.GotoNextMove, newChessBoard.TryGotoNextMove },
                { StandardChessBoard.FastNavigateForward, newChessBoard.TryFastNavigateForward },
                { StandardChessBoard.GotoLastMove, newChessBoard.TryGotoLastMove },

                { StandardChessBoard.GotoPreviousVariation, newChessBoard.TryGotoPreviousVariation },
                { StandardChessBoard.GotoNextVariation, newChessBoard.TryGotoNextVariation },

                { StandardChessBoard.PromoteActiveVariation, newChessBoard.TryPromoteActiveVariation },
                { StandardChessBoard.DemoteActiveVariation, newChessBoard.TryDemoteActiveVariation },
                { StandardChessBoard.DeleteActiveVariation, newChessBoard.TryDeleteActiveVariation },

                { StandardChessBoard.FlipBoard, newChessBoard.TryFlipBoard },
                { StandardChessBoard.TakeScreenshot, newChessBoard.TryTakeScreenshot },

                { SharedUIAction.ZoomIn, newChessBoard.TryZoomIn },
                { SharedUIAction.ZoomOut, newChessBoard.TryZoomOut },
            });

            UIMenu.AddTo(newChessBoard.PlayingBoard);

            string white = game.White?.Value;
            string black = game.Black?.Value;
            string whiteElo = game.WhiteElo?.Value;
            string blackElo = game.BlackElo?.Value;

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

            // Keep selecting the active ply if it changes.
            newChessBoard.AfterGameUpdated += (_, __) =>
            {
                PgnPlySyntax activePly = newChessBoard.Game.ActivePly;
                int selectionStart;
                int caretPosition;
                if (activePly == null)
                {
                    // Put the caret before the first ply, but after leading trivia/float items if found.
                    var plyList = newChessBoard.Game.PgnGame.PlyList;
                    selectionStart = plyList.AbsoluteStart;

                    if (plyList.Plies.Any(out PgnPlySyntax firstPly)
                        && firstPly.ChildCount > 0
                        && firstPly.GetChild(0) is WithPlyFloatItemsSyntax withFloatItems)
                    {
                        if (withFloatItems.PlyContentNode is WithTriviaSyntax withTrivia)
                        {
                            selectionStart = withTrivia.ContentNode.AbsoluteStart;
                        }
                        else
                        {
                            selectionStart = withFloatItems.PlyContentNode.AbsoluteStart;
                        }
                    }

                    caretPosition = selectionStart;
                }
                else
                {
                    PgnMoveSyntax activeMove = activePly.Move?.PlyContentNode.ContentNode;
                    if (activeMove == null)
                    {
                        // Put the caret right after the ply without selecting anything.
                        selectionStart = activePly.AbsoluteStart + activePly.Length;
                        caretPosition = selectionStart;
                    }
                    else
                    {
                        // Select the move.
                        selectionStart = activeMove.AbsoluteStart;
                        caretPosition = selectionStart + activeMove.Length;
                    }
                }

                ownerPgnEditor.SetSelection(selectionStart, caretPosition);
                ownerPgnEditor.ScrollRange(selectionStart, caretPosition);
            };

            return newChessBoard;
        }
    }
}

﻿/*********************************************************************************
 * MdiContainerForm.cs
 * 
 * Copyright (c) 2004-2017 Henk Nicolai
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
 *********************************************************************************/
using Sandra.Chess;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Main MdiContainer Form.
    /// </summary>
    public class MdiContainerForm : Form, IUIActionHandlerProvider
    {
        public EnumIndexedArray<ColoredPiece, Image> PieceImages { get; private set; }

        public MdiContainerForm()
        {
            IsMdiContainer = true;

            // Initialize UIActions before building the MainMenuStrip based on it.
            initializeUIActions();

            MainMenuStrip = new MenuStrip();
            UIMenuBuilder.BuildMenu(mainMenuActionHandler, MainMenuStrip.Items);
            MainMenuStrip.Visible = true;
            Controls.Add(MainMenuStrip);

            // After building the MainMenuStrip, build an index of ToolstripMenuItems which are bound on focus dependent UIActions.
            indexFocusDependentUIActions(MainMenuStrip.Items);
        }

        /// <summary>
        /// Gets the action handler for this control.
        /// </summary>
        public UIActionHandler ActionHandler { get; } = new UIActionHandler();

        // Separate action handler for building the MainMenuStrip.
        readonly UIActionHandler mainMenuActionHandler = new UIActionHandler();

        public const string MdiContainerFormUIActionPrefix = nameof(MdiContainerForm) + ".";

        public static readonly UIAction OpenNewPlayingBoardUIAction = new UIAction(MdiContainerFormUIActionPrefix + nameof(OpenNewPlayingBoardUIAction));

        public UIActionState TryOpenNewPlayingBoard(bool perform)
        {
            if (perform) NewPlayingBoard();
            return UIActionVisibility.Enabled;
        }

        public UIActionBinding DefaultOpenNewPlayingBoardBinding()
        {
            return new UIActionBinding()
            {
                ShowInMenu = true,
                MenuCaption = "New game",
                MainShortcut = new ShortcutKeys(KeyModifiers.Control, ConsoleKey.N),
            };
        }


        class FocusDependentUIActionState
        {
            public UIActionToolStripMenuItem MenuItem;
            public UIActionHandler CurrentHandler;
            public bool IsDirty;
        }

        readonly Dictionary<UIAction, FocusDependentUIActionState> focusDependentUIActions = new Dictionary<UIAction, FocusDependentUIActionState>();

        void bindFocusDependentUIAction(UIMenuNode.Container container, UIAction action, UIActionBinding binding)
        {
            // Add a menu item inside the given container which will update itself after focus changes.
            binding.MenuContainer = container;

            // Always show in the menu, and clear the alternative shortcuts.
            binding.ShowInMenu = true;
            binding.AlternativeShortcuts = null;

            // Register in a Dictionary to be able to figure out which menu items should be updated.
            focusDependentUIActions.Add(action, new FocusDependentUIActionState());

            // This also means that if a menu item is clicked, TryPerformAction() is called on the mainMenuActionHandler.
            mainMenuActionHandler.BindAction(action, perform =>
            {
                try
                {
                    var state = focusDependentUIActions[action];

                    if (!perform)
                    {
                        // Only clear/set the state when called from updateFocusDependentMenuItems().
                        state.CurrentHandler = null;
                        state.IsDirty = false;
                    }

                    // Try to find a UIActionHandler that is willing to validate/perform the given action.
                    foreach (var actionHandler in UIActionHandler.EnumerateUIActionHandlers(FocusHelper.GetFocusedControl()))
                    {
                        UIActionState currentActionState = actionHandler.TryPerformAction(action, perform);
                        if (currentActionState.UIActionVisibility != UIActionVisibility.Parent)
                        {
                            // Remember the action handler this UIAction is now bound to.
                            if (!perform)
                            {
                                // Only clear/set the state when called from updateFocusDependentMenuItems().
                                state.CurrentHandler = actionHandler;
                            }
                            return currentActionState;
                        }

                        // Only consider handlers which are defined in the context of this one.
                        if (ActionHandler == actionHandler) break;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }

                // No handler in the chain that processes the UIAction actively, so set to disabled.
                return UIActionVisibility.Disabled;

            }, binding);
        }

        void initializeUIActions()
        {
            this.BindAction(OpenNewPlayingBoardUIAction, TryOpenNewPlayingBoard, DefaultOpenNewPlayingBoardBinding());

            UIMenuNode.Container container = new UIMenuNode.Container("Game");
            mainMenuActionHandler.RootMenuNode.Nodes.Add(container);

            // Also display in the main manu.
            bindFocusDependentUIAction(container,
                                       OpenNewPlayingBoardUIAction,
                                       DefaultOpenNewPlayingBoardBinding());

            bindFocusDependentUIAction(container,
                                       InteractiveGame.GotoPreviousMoveUIAction,
                                       InteractiveGame.DefaultGotoPreviousMoveBinding());

            bindFocusDependentUIAction(container,
                                       InteractiveGame.GotoNextMoveUIAction,
                                       InteractiveGame.DefaultGotoNextMoveBinding());

            bindFocusDependentUIAction(container,
                                       PlayingBoard.TakeScreenshotUIAction,
                                       PlayingBoard.DefaultTakeScreenshotBinding());

            FocusHelper.Instance.FocusChanged += focusHelper_FocusChanged;
        }

        void focusHelper_FocusChanged(object sender, FocusChangedEventArgs e)
        {
            UIActionHandler previousHandler;
            if (UIActionHandler.EnumerateUIActionHandlers(e.PreviousFocusedControl).Any(out previousHandler))
            {
                previousHandler.UIActionsInvalidated -= focusedHandler_UIActionsInvalidated;
            }
            UIActionHandler currentHandler;
            if (UIActionHandler.EnumerateUIActionHandlers(e.CurrentFocusedControl).Any(out currentHandler))
            {
                currentHandler.UIActionsInvalidated += focusedHandler_UIActionsInvalidated;
            }

            // Invalidate all focus dependent items.
            foreach (var state in focusDependentUIActions.Values)
            {
                state.IsDirty = true;
            }

            updateFocusDependentMenuItems();
        }

        void focusedHandler_UIActionsInvalidated(object sender, EventArgs e)
        {
            UIActionHandler activeHandler = (UIActionHandler)sender;
            foreach (var state in focusDependentUIActions.Values)
            {
                // Invalidate all UIActions which are influenced by the active handler.
                state.IsDirty |= state.CurrentHandler == activeHandler;
            }

            // Register on the Idle event since focus changes might happen as well.
            // Do make sure the Idle event is registered at most once.
            Application.Idle -= application_Idle;
            Application.Idle += application_Idle;
        }

        void application_Idle(object sender, EventArgs e)
        {
            updateFocusDependentMenuItems();
        }

        void updateFocusDependentMenuItems()
        {
            Application.Idle -= application_Idle;
            foreach (var state in focusDependentUIActions.Values.Where(x => x.IsDirty))
            {
                // If not yet indexed, then fast-exit.
                if (state.MenuItem == null) return;
                state.MenuItem.Update(mainMenuActionHandler.TryPerformAction(state.MenuItem.Action, false));
            }
        }

        void indexFocusDependentUIActions(ToolStripItemCollection collection)
        {
            foreach (ToolStripMenuItem item in collection.OfType<ToolStripMenuItem>())
            {
                UIActionToolStripMenuItem actionItem = item as UIActionToolStripMenuItem;
                if (actionItem != null)
                {
                    FocusDependentUIActionState state;
                    if (focusDependentUIActions.TryGetValue(actionItem.Action, out state))
                    {
                        state.IsDirty = true;
                        state.MenuItem = actionItem;
                    }
                }
                else
                {
                    indexFocusDependentUIActions(item.DropDownItems);
                }
            }

            updateFocusDependentMenuItems();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // This code makes shortcuts work for all UIActionHandlers.
            return KeyUtils.TryExecute(keyData) || base.ProcessCmdKey(ref msg, keyData);
        }


        public void NewPlayingBoard()
        {
            InteractiveGame game = new InteractiveGame(Position.GetInitialPosition());

            StandardChessBoardForm mdiChild = new StandardChessBoardForm()
            {
                MdiParent = this,
                ClientSize = new Size(400, 400),
            };
            mdiChild.Game = game;
            mdiChild.PieceImages = PieceImages;
            mdiChild.PlayingBoard.ForegroundImageRelativeSize = 0.9f;
            mdiChild.PerformAutoFit();

            mdiChild.PlayingBoard.BindAction(InteractiveGame.GotoPreviousMoveUIAction, game.TryGotoPreviousMove, InteractiveGame.DefaultGotoPreviousMoveBinding());
            mdiChild.PlayingBoard.BindAction(InteractiveGame.GotoNextMoveUIAction, game.TryGotoNextMove, InteractiveGame.DefaultGotoNextMoveBinding());
            mdiChild.PlayingBoard.BindAction(PlayingBoard.TakeScreenshotUIAction, mdiChild.PlayingBoard.TryTakeScreenshot, PlayingBoard.DefaultTakeScreenshotBinding());
            UIMenu.AddTo(mdiChild.PlayingBoard);

            mdiChild.Load += (_, __) =>
            {
                var mdiChildBounds = mdiChild.Bounds;
                SnappingMdiChildForm movesForm = new SnappingMdiChildForm()
                {
                    MdiParent = this,
                    StartPosition = FormStartPosition.Manual,
                    Left = mdiChildBounds.Right,
                    Top = mdiChildBounds.Top,
                    Width = 200,
                    Height = mdiChildBounds.Height,
                    ShowIcon = false,
                    MaximizeBox = false,
                    FormBorderStyle = FormBorderStyle.SizableToolWindow,
                };

                EnumIndexedArray<Piece, string> englishPieceSymbols = EnumIndexedArray<Piece, string>.New();
                englishPieceSymbols[Piece.Knight] = "N";
                englishPieceSymbols[Piece.Bishop] = "B";
                englishPieceSymbols[Piece.Rook] = "R";
                englishPieceSymbols[Piece.Queen] = "Q";
                englishPieceSymbols[Piece.King] = "K";

                var movesTextBox = new MovesTextBox()
                {
                    Dock = DockStyle.Fill,
                    Game = game,
                    MoveFormatter = new ShortAlgebraicMoveFormatter(englishPieceSymbols),
                };

                movesTextBox.BindAction(InteractiveGame.GotoPreviousMoveUIAction, game.TryGotoPreviousMove, InteractiveGame.DefaultGotoPreviousMoveBinding());
                movesTextBox.BindAction(InteractiveGame.GotoNextMoveUIAction, game.TryGotoNextMove, InteractiveGame.DefaultGotoNextMoveBinding());
                UIMenu.AddTo(movesTextBox);

                movesForm.Controls.Add(movesTextBox);

                movesForm.Visible = true;
            };

            mdiChild.Visible = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Show in the center of the monitor where the mouse currently is.
            var activeScreen = Screen.FromPoint(MousePosition);
            Rectangle workingArea = activeScreen.WorkingArea;

            // Two thirds the size of the active monitor's working area.
            workingArea.Inflate(-workingArea.Width / 6, -workingArea.Height / 6);

            // Update the bounds of the form.
            SetBounds(workingArea.X, workingArea.Y, workingArea.Width, workingArea.Height, BoundsSpecified.All);

            // Load chess piece images from a fixed path.
            PieceImages = loadChessPieceImages();

            NewPlayingBoard();
        }

        EnumIndexedArray<ColoredPiece, Image> loadChessPieceImages()
        {
            var array = EnumIndexedArray<ColoredPiece, Image>.New();
            array[ColoredPiece.BlackPawn] = loadImage("bp");
            array[ColoredPiece.BlackKnight] = loadImage("bn");
            array[ColoredPiece.BlackBishop] = loadImage("bb");
            array[ColoredPiece.BlackRook] = loadImage("br");
            array[ColoredPiece.BlackQueen] = loadImage("bq");
            array[ColoredPiece.BlackKing] = loadImage("bk");
            array[ColoredPiece.WhitePawn] = loadImage("wp");
            array[ColoredPiece.WhiteKnight] = loadImage("wn");
            array[ColoredPiece.WhiteBishop] = loadImage("wb");
            array[ColoredPiece.WhiteRook] = loadImage("wr");
            array[ColoredPiece.WhiteQueen] = loadImage("wq");
            array[ColoredPiece.WhiteKing] = loadImage("wk");
            return array;
        }

        static Image loadImage(string imageFileKey)
        {
            try
            {
                string basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                return Image.FromFile(Path.Combine(basePath, "Images", imageFileKey + ".png"));
            }
            catch (Exception exc)
            {
                Debug.Write(exc.Message);
                return null;
            }
        }
    }
}

/*********************************************************************************
 * MdiContainerForm.cs
 * 
 * Copyright (c) 2004-2018 Henk Nicolai
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

        /// <summary>
        /// Contains the number of plies to move forward of backward in a game for fast navigation.
        /// </summary>
        public int FastNavigationPlyCount { get; private set; }

        public MdiContainerForm()
        {
            IsMdiContainer = true;
            Icon = Properties.Resources.Sandra;
            Text = "Sandra";

            // Initialize UIActions before building the MainMenuStrip based on it.
            initializeUIActions();

            MainMenuStrip = new MenuStrip();
            UIMenuBuilder.BuildMenu(mainMenuActionHandler, MainMenuStrip.Items);
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

        public static readonly DefaultUIActionBinding OpenNewPlayingBoard = new DefaultUIActionBinding(
            new UIAction(MdiContainerFormUIActionPrefix + nameof(OpenNewPlayingBoard)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.NewGame,
                Shortcuts = new ShortcutKeys[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.N), },
            });

        public UIActionState TryOpenNewPlayingBoard(bool perform)
        {
            if (perform) NewPlayingBoard();
            return UIActionVisibility.Enabled;
        }



        class FocusDependentUIActionState
        {
            public UIActionToolStripMenuItem MenuItem;
            public UIActionHandler CurrentHandler;
            public bool IsDirty;
        }

        readonly Dictionary<UIAction, FocusDependentUIActionState> focusDependentUIActions = new Dictionary<UIAction, FocusDependentUIActionState>();

        void bindFocusDependentUIActions(UIMenuNode.Container container, params DefaultUIActionBinding[] bindings)
        {
            foreach (DefaultUIActionBinding binding in bindings)
            {
                // Copy the default binding and modify it.
                UIActionBinding modifiedBinding = binding.DefaultBinding;

                // Add a menu item inside the given container which will update itself after focus changes.
                modifiedBinding.MenuContainer = container;

                // Always show in the menu.
                modifiedBinding.ShowInMenu = true;

                // Register in a Dictionary to be able to figure out which menu items should be updated.
                focusDependentUIActions.Add(binding.Action, new FocusDependentUIActionState());

                // This also means that if a menu item is clicked, TryPerformAction() is called on the mainMenuActionHandler.
                mainMenuActionHandler.BindAction(binding.Action, modifiedBinding, perform =>
                {
                    try
                    {
                        var state = focusDependentUIActions[binding.Action];

                        if (!perform)
                        {
                            // Only clear/set the state when called from updateFocusDependentMenuItems().
                            state.CurrentHandler = null;
                            state.IsDirty = false;
                        }

                        // Try to find a UIActionHandler that is willing to validate/perform the given action.
                        foreach (var actionHandler in UIActionHandler.EnumerateUIActionHandlers(FocusHelper.GetFocusedControl()))
                        {
                            UIActionState currentActionState = actionHandler.TryPerformAction(binding.Action, perform);
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
                });
            }
        }

        void initializeUIActions()
        {
            this.BindAction(OpenNewPlayingBoard, TryOpenNewPlayingBoard);

            UIMenuNode.Container gameMenu = new UIMenuNode.Container(LocalizedStringKeys.Game);
            mainMenuActionHandler.RootMenuNode.Nodes.Add(gameMenu);

            // Add these actions to the "Game" dropdown list.
            bindFocusDependentUIActions(gameMenu,
                                        OpenNewPlayingBoard);

            UIMenuNode.Container goToMenu = new UIMenuNode.Container(LocalizedStringKeys.GoTo)
            {
                IsFirstInGroup = true,
            };
            gameMenu.Nodes.Add(goToMenu);

            // Add all these to a submenu.
            bindFocusDependentUIActions(goToMenu,
                                        InteractiveGame.GotoStart,
                                        InteractiveGame.GotoFirstMove,
                                        InteractiveGame.FastNavigateBackward,
                                        InteractiveGame.GotoPreviousMove,
                                        InteractiveGame.GotoNextMove,
                                        InteractiveGame.FastNavigateForward,
                                        InteractiveGame.GotoLastMove,
                                        InteractiveGame.GotoEnd,
                                        InteractiveGame.GotoPreviousVariation,
                                        InteractiveGame.GotoNextVariation);

            bindFocusDependentUIActions(gameMenu,
                                        InteractiveGame.PromoteActiveVariation,
                                        InteractiveGame.DemoteActiveVariation,
                                        InteractiveGame.BreakActiveVariation,
                                        InteractiveGame.DeleteActiveVariation,
                                        MovesTextBox.UsePGNPieceSymbols,
                                        MovesTextBox.UseLongAlgebraicNotation,
                                        StandardChessBoardForm.FlipBoard,
                                        StandardChessBoardForm.TakeScreenshot);

            UIMenuNode.Container viewMenu = new UIMenuNode.Container(LocalizedStringKeys.View);
            mainMenuActionHandler.RootMenuNode.Nodes.Add(viewMenu);

            // Add these actions to the "View" dropdown list.
            bindFocusDependentUIActions(viewMenu,
                                        InteractiveGame.GotoChessBoardForm,
                                        InteractiveGame.GotoMovesForm);

            // Track focus to detect when main menu items must be updated.
            FocusHelper.Instance.FocusChanged += focusHelper_FocusChanged;
        }

        void focusHelper_FocusChanged(FocusHelper sender, FocusChangedEventArgs e)
        {
            foreach (UIActionHandler previousHandler in UIActionHandler.EnumerateUIActionHandlers(e.PreviousFocusedControl))
            {
                previousHandler.UIActionsInvalidated -= focusedHandler_UIActionsInvalidated;
            }
            foreach (UIActionHandler currentHandler in UIActionHandler.EnumerateUIActionHandlers(e.CurrentFocusedControl))
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

        void focusedHandler_UIActionsInvalidated(UIActionHandler activeHandler)
        {
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
            try
            {
                // This code makes shortcuts work for all UIActionHandlers.
                return KeyUtils.TryExecute(keyData) || base.ProcessCmdKey(ref msg, keyData);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return true;
            }
        }


        public void NewPlayingBoard()
        {
            InteractiveGame game = new InteractiveGame(this, Position.GetInitialPosition());

            game.TryGotoChessBoardForm(true);
            game.TryGotoMovesForm(true);

            // Focus back on the chessboard form.
            game.TryGotoChessBoardForm(true);
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

            // 10 plies == 5 moves.
            FastNavigationPlyCount = 10;

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
                exc.Trace();
                return null;
            }
        }
    }
}

#region License
/*********************************************************************************
 * MdiContainerForm.cs
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

using Sandra.Chess;
using SysExtensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    /// <summary>
    /// Main MdiContainer Form.
    /// </summary>
    public partial class MdiContainerForm : UIActionForm
    {
        public EnumIndexedArray<ColoredPiece, Image> PieceImages { get; private set; }

        private Form openLocalSettingsForm;
        private Form openDefaultSettingsForm;
        private Form openAboutForm;
        private Form openCreditsForm;

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

        // Separate action handler for building the MainMenuStrip.
        readonly UIActionHandler mainMenuActionHandler = new UIActionHandler();

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
            // More than one localizer: can switch between them.
            if (Localizers.Registered.Count() >= 2)
            {
                foreach (var localizer in Localizers.Registered)
                {
                    this.BindAction(localizer.SwitchToLangUIActionBinding, localizer.TrySwitchToLang);
                }

                UIMenuNode.Container langMenu = new UIMenuNode.Container(null, Properties.Resources.globe);
                mainMenuActionHandler.RootMenuNode.Nodes.Add(langMenu);
                bindFocusDependentUIActions(langMenu, Localizers.Registered.Select(x => x.SwitchToLangUIActionBinding).ToArray());
            }

            // Actions which have their handler in this instance.
            this.BindAction(EditPreferencesFile, TryEditPreferencesFile);
            this.BindAction(ShowDefaultSettingsFile, TryShowDefaultSettingsFile);
            this.BindAction(Exit, TryExit);
            this.BindAction(OpenNewPlayingBoard, TryOpenNewPlayingBoard);
            this.BindAction(OpenAbout, TryOpenAbout);
            this.BindAction(ShowCredits, TryShowCredits);

            UIMenuNode.Container fileMenu = new UIMenuNode.Container(LocalizedStringKeys.File);
            mainMenuActionHandler.RootMenuNode.Nodes.Add(fileMenu);

            // Add these actions to the "File" dropdown list.
            bindFocusDependentUIActions(fileMenu,
                                        EditPreferencesFile,
                                        ShowDefaultSettingsFile,
                                        Exit);

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
                                        InteractiveGame.GotoMovesForm,
                                        SharedUIAction.ZoomIn,
                                        SharedUIAction.ZoomOut);

            UIMenuNode.Container helpMenu = new UIMenuNode.Container(LocalizedStringKeys.Help);
            mainMenuActionHandler.RootMenuNode.Nodes.Add(helpMenu);

            // Add these actions to the "Help" dropdown list.
            bindFocusDependentUIActions(helpMenu,
                                        OpenAbout,
                                        ShowCredits);

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
                if (item is UIActionToolStripMenuItem actionItem)
                {
                    if (focusDependentUIActions.TryGetValue(actionItem.Action, out FocusDependentUIActionState state))
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
            // Determine minimum size before restoring from settings: always show title bar and menu.
            MinimumSize = new Size(144, SystemInformation.CaptionHeight + MainMenuStrip.Height);

            base.OnLoad(e);

            // Initialize from settings if available.
            Program.AttachFormStateAutoSaver(
                this,
                SettingKeys.Window,
                () =>
                {
                    // Show in the center of the monitor where the mouse currently is.
                    var activeScreen = Screen.FromPoint(MousePosition);
                    Rectangle workingArea = activeScreen.WorkingArea;

                    // Two thirds the size of the active monitor's working area.
                    workingArea.Inflate(-workingArea.Width / 6, -workingArea.Height / 6);

                    // Update the bounds of the form.
                    SetBounds(workingArea.X, workingArea.Y, workingArea.Width, workingArea.Height, BoundsSpecified.All);
                });

            // Load chess piece images from a fixed path.
            PieceImages = loadChessPieceImages();

            NewPlayingBoard();
        }

        EnumIndexedArray<ColoredPiece, Image> loadChessPieceImages()
        {
            var array = EnumIndexedArray<ColoredPiece, Image>.New();
            array[ColoredPiece.BlackPawn] = Program.LoadImage("bp");
            array[ColoredPiece.BlackKnight] = Program.LoadImage("bn");
            array[ColoredPiece.BlackBishop] = Program.LoadImage("bb");
            array[ColoredPiece.BlackRook] = Program.LoadImage("br");
            array[ColoredPiece.BlackQueen] = Program.LoadImage("bq");
            array[ColoredPiece.BlackKing] = Program.LoadImage("bk");
            array[ColoredPiece.WhitePawn] = Program.LoadImage("wp");
            array[ColoredPiece.WhiteKnight] = Program.LoadImage("wn");
            array[ColoredPiece.WhiteBishop] = Program.LoadImage("wb");
            array[ColoredPiece.WhiteRook] = Program.LoadImage("wr");
            array[ColoredPiece.WhiteQueen] = Program.LoadImage("wq");
            array[ColoredPiece.WhiteKing] = Program.LoadImage("wk");
            return array;
        }
    }
}

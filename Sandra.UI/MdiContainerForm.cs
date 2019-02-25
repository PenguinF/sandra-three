﻿#region License
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

using Eutherion.Localization;
using Eutherion.UIActions;
using Eutherion.Utils;
using Eutherion.Win;
using Eutherion.Win.AppTemplate;
using Eutherion.Win.UIActions;
using Sandra.Chess;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Sandra.UI
{
    /// <summary>
    /// Main MdiContainer Form.
    /// </summary>
    public partial class MdiContainerForm : UIActionForm, IWeakEventTarget
    {
        public EnumIndexedArray<ColoredPiece, Image> PieceImages { get; private set; }

        private readonly LocalizedString developerTools = new LocalizedString(SharedLocalizedStringKeys.Tools);

        // Separate action handler and root menu node for building the MainMenuStrip.
        private readonly UIActionHandler mainMenuActionHandler = new UIActionHandler();
        private readonly List<UIMenuNode> mainMenuRootNodes = new List<UIMenuNode>();

        // Action handler for the developer tools dropdown item.
        private readonly UIActionHandler developerToolsActionHandler = new UIActionHandler();

        private readonly LocalizedToolStripMenuItem developerToolsMenuItem;

        public MdiContainerForm()
        {
            IsMdiContainer = true;
            Icon = Properties.Resources.Sandra;
            Text = "Sandra";

            // Initialize UIActions before building the MainMenuStrip based on it.
            InitializeUIActions();

            MainMenuStrip = new MenuStrip();
            UIMenuBuilder.BuildMenu(mainMenuActionHandler, mainMenuRootNodes, MainMenuStrip.Items);
            Controls.Add(MainMenuStrip);

            // After building the MainMenuStrip, build an index of ToolstripMenuItems which are bound on focus dependent UIActions.
            IndexFocusDependentUIActions(MainMenuStrip.Items);

            // Developer tools.
            developerToolsMenuItem = new LocalizedToolStripMenuItem { LocalizedText = developerTools };
            developerToolsMenuItem.LocalizedText.DisplayText.ValueChanged += displayText => developerToolsMenuItem.Text = displayText.Replace("&", "&&");

            // Insert before the Help menu.
            MainMenuStrip.Items.Insert(MainMenuStrip.Items.Count - 1, developerToolsMenuItem);

            UIMenuBuilder.BuildMenu(developerToolsActionHandler, developerToolsMenuItem.DropDownItems);
            UpdateDeveloperToolsMenu();

            Session.Current.LocalSettings.RegisterSettingsChangedHandler(Session.Current.DeveloperMode, DeveloperModeChanged);
            developerToolsActionHandler.UIActionsInvalidated += DeveloperToolsActionHandler_UIActionsInvalidated;
        }

        private void UpdateDeveloperToolsMenu()
        {
            bool atLeastOneItemVisible = false;

            foreach (var menuItem in developerToolsMenuItem.DropDownItems.OfType<UIActionToolStripMenuItem>())
            {
                var state = developerToolsActionHandler.TryPerformAction(menuItem.Action, false);
                menuItem.Update(state);
                atLeastOneItemVisible |= state.Visible;
            }

            developerToolsMenuItem.Visible = atLeastOneItemVisible;
        }

        private void DeveloperToolsActionHandler_UIActionsInvalidated(UIActionHandler _)
            => UpdateDeveloperToolsMenu();

        private void DeveloperModeChanged(object sender, EventArgs e)
            => developerToolsActionHandler.Invalidate();

        class FocusDependentUIActionState
        {
            public UIActionToolStripMenuItem MenuItem;
            public UIActionHandler CurrentHandler;
            public bool IsDirty;
        }

        readonly Dictionary<UIAction, FocusDependentUIActionState> focusDependentUIActions = new Dictionary<UIAction, FocusDependentUIActionState>();

        void BindFocusDependentUIActions(UIMenuNode.Container container, params DefaultUIActionBinding[] bindings)
        {
            foreach (DefaultUIActionBinding binding in bindings)
            {
                if (binding.DefaultInterfaces.TryGet(out ContextMenuUIActionInterface contextMenuInterface))
                {
                    // Copy the default binding and remove the ContextMenuUIActionInterface
                    // which is used below to construct a UIMenuNode.Element directly.
                    var modifiedBinding = new ImplementationSet<IUIActionInterface>();
                    if (binding.DefaultInterfaces.TryGet(out ShortcutKeysUIActionInterface shortcutKeysInterface))
                    {
                        modifiedBinding.Add(shortcutKeysInterface);
                    }

                    // Add a menu item inside the given container which will update itself after focus changes.
                    container.Nodes.Add(new UIMenuNode.Element(binding.Action, shortcutKeysInterface, contextMenuInterface));

                    // Register in a Dictionary to be able to figure out which menu items should be updated.
                    focusDependentUIActions.Add(binding.Action, new FocusDependentUIActionState());

                    // This also means that if a menu item is clicked, TryPerformAction() is called on the mainMenuActionHandler.
                    mainMenuActionHandler.BindAction(new UIActionBinding(binding.Action, modifiedBinding, perform =>
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
                            foreach (var actionHandler in UIActionUtilities.EnumerateUIActionHandlers(FocusHelper.GetFocusedControl()))
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
                    }));
                }
            }
        }

        void InitializeUIActions()
        {
            // More than one localizer: can switch between them.
            if (Session.Current.RegisteredLocalizers.Count() >= 2)
            {
                foreach (var localizer in Session.Current.RegisteredLocalizers)
                {
                    this.BindAction(localizer.SwitchToLangUIActionBinding, localizer.TrySwitchToLang);
                }

                UIMenuNode.Container langMenu = new UIMenuNode.Container(null, SharedResources.globe);
                mainMenuRootNodes.Add(langMenu);
                BindFocusDependentUIActions(langMenu, Session.Current.RegisteredLocalizers.Select(x => x.SwitchToLangUIActionBinding).ToArray());
            }

            // Actions which have their handler in this instance.
            this.BindAction(Session.Current.EditPreferencesFile, Session.Current.TryEditPreferencesFile(this));
            this.BindAction(Session.Current.ShowDefaultSettingsFile, Session.Current.TryShowDefaultSettingsFile(this));
            this.BindAction(SharedUIAction.Exit, TryExit);
            this.BindAction(OpenNewPlayingBoard, TryOpenNewPlayingBoard);
            this.BindAction(Session.Current.OpenAbout, Session.Current.TryOpenAbout(this));
            this.BindAction(Session.Current.ShowCredits, Session.Current.TryShowCredits(this));
            this.BindAction(Session.Current.EditCurrentLanguage, Session.Current.TryEditCurrentLanguage(this));

            // Use developerToolsActionHandler to add to the developer tools menu.
            developerToolsActionHandler.BindAction(new UIActionBinding(
                Session.Current.EditCurrentLanguage.Action,
                Session.Current.EditCurrentLanguage.DefaultInterfaces,
                Session.Current.TryEditCurrentLanguage(this)));

            UIMenuNode.Container fileMenu = new UIMenuNode.Container(SharedLocalizedStringKeys.File);
            mainMenuRootNodes.Add(fileMenu);

            // Add these actions to the "File" dropdown list.
            BindFocusDependentUIActions(fileMenu,
                                        Session.Current.EditPreferencesFile,
                                        Session.Current.ShowDefaultSettingsFile,
                                        SharedUIAction.Exit);

            UIMenuNode.Container gameMenu = new UIMenuNode.Container(LocalizedStringKeys.Game);
            mainMenuRootNodes.Add(gameMenu);

            // Add these actions to the "Game" dropdown list.
            BindFocusDependentUIActions(gameMenu,
                                        OpenNewPlayingBoard);

            UIMenuNode.Container goToMenu = new UIMenuNode.Container(LocalizedStringKeys.GoTo)
            {
                IsFirstInGroup = true,
            };
            gameMenu.Nodes.Add(goToMenu);

            // Add all these to a submenu.
            BindFocusDependentUIActions(goToMenu,
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

            BindFocusDependentUIActions(gameMenu,
                                        InteractiveGame.PromoteActiveVariation,
                                        InteractiveGame.DemoteActiveVariation,
                                        InteractiveGame.BreakActiveVariation,
                                        InteractiveGame.DeleteActiveVariation,
                                        MovesTextBox.UsePGNPieceSymbols,
                                        MovesTextBox.UseLongAlgebraicNotation,
                                        StandardChessBoardForm.FlipBoard,
                                        StandardChessBoardForm.TakeScreenshot);

            UIMenuNode.Container viewMenu = new UIMenuNode.Container(SharedLocalizedStringKeys.View);
            mainMenuRootNodes.Add(viewMenu);

            // Provide ContextMenuUIActionInterfaces for GotoChessBoardForm and GotoMovesForm
            // because they would otherwise remain invisible.
            var modifiedGotoChessBoardForm = new DefaultUIActionBinding(
                InteractiveGame.GotoChessBoardForm.Action,
                new ImplementationSet<IUIActionInterface>
                {
                    InteractiveGame.GotoChessBoardForm.DefaultInterfaces.Get<ShortcutKeysUIActionInterface>(),
                    new ContextMenuUIActionInterface
                    {
                        IsFirstInGroup = true,
                        MenuCaptionKey = LocalizedStringKeys.Chessboard,
                    },
                });

            var modifiedGotoMovesForm = new DefaultUIActionBinding(
                InteractiveGame.GotoMovesForm.Action,
                new ImplementationSet<IUIActionInterface>
                {
                    InteractiveGame.GotoMovesForm.DefaultInterfaces.Get<ShortcutKeysUIActionInterface>(),
                    new ContextMenuUIActionInterface
                    {
                        MenuCaptionKey = LocalizedStringKeys.Moves,
                    },
                });

            // Add these actions to the "View" dropdown list.
            BindFocusDependentUIActions(viewMenu,
                                        modifiedGotoChessBoardForm,
                                        modifiedGotoMovesForm,
                                        SharedUIAction.ZoomIn,
                                        SharedUIAction.ZoomOut);

            UIMenuNode.Container helpMenu = new UIMenuNode.Container(SharedLocalizedStringKeys.Help);
            mainMenuRootNodes.Add(helpMenu);

            // Add these actions to the "Help" dropdown list.
            BindFocusDependentUIActions(helpMenu,
                                        Session.Current.OpenAbout,
                                        Session.Current.ShowCredits);

            // Track focus to detect when main menu items must be updated.
            FocusHelper.Instance.FocusChanged += FocusHelper_FocusChanged;
        }

        void FocusHelper_FocusChanged(FocusHelper sender, FocusChangedEventArgs e)
        {
            foreach (UIActionHandler previousHandler in UIActionUtilities.EnumerateUIActionHandlers(e.PreviousFocusedControl))
            {
                previousHandler.UIActionsInvalidated -= FocusedHandler_UIActionsInvalidated;
            }
            foreach (UIActionHandler currentHandler in UIActionUtilities.EnumerateUIActionHandlers(e.CurrentFocusedControl))
            {
                currentHandler.UIActionsInvalidated += FocusedHandler_UIActionsInvalidated;
            }

            // Invalidate all focus dependent items.
            foreach (var state in focusDependentUIActions.Values)
            {
                state.IsDirty = true;
            }

            UpdateFocusDependentMenuItems();
        }

        void FocusedHandler_UIActionsInvalidated(UIActionHandler activeHandler)
        {
            foreach (var state in focusDependentUIActions.Values)
            {
                // Invalidate all UIActions which are influenced by the active handler.
                state.IsDirty |= state.CurrentHandler == activeHandler;
            }

            // Register on the Idle event since focus changes might happen as well.
            // Do make sure the Idle event is registered at most once.
            Application.Idle -= Application_Idle;
            Application.Idle += Application_Idle;
        }

        void Application_Idle(object sender, EventArgs e)
        {
            UpdateFocusDependentMenuItems();
        }

        void UpdateFocusDependentMenuItems()
        {
            Application.Idle -= Application_Idle;
            foreach (var state in focusDependentUIActions.Values.Where(x => x.IsDirty))
            {
                // If not yet indexed, then fast-exit.
                if (state.MenuItem == null) return;
                state.MenuItem.Update(mainMenuActionHandler.TryPerformAction(state.MenuItem.Action, false));
            }
        }

        void IndexFocusDependentUIActions(ToolStripItemCollection collection)
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
                    IndexFocusDependentUIActions(item.DropDownItems);
                }
            }

            UpdateFocusDependentMenuItems();
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
            // Enable live updates to localizers now a message loop exists.
            Session.Current.RegisteredLocalizers.ForEach(x => x.EnableLiveUpdates());

            // Determine minimum size before restoring from settings: always show title bar and menu.
            MinimumSize = new Size(144, SystemInformation.CaptionHeight + MainMenuStrip.Height);

            base.OnLoad(e);

            // Initialize from settings if available.
            Session.Current.AttachFormStateAutoSaver(
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
            PieceImages = LoadChessPieceImages();

            NewPlayingBoard();
        }

        EnumIndexedArray<ColoredPiece, Image> LoadChessPieceImages()
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                developerTools.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

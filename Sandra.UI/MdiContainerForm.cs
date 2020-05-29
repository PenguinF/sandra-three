#region License
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

using Eutherion.UIActions;
using Eutherion.Utils;
using Eutherion.Win;
using Eutherion.Win.MdiAppTemplate;
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
    public partial class MdiContainerForm : OldMenuCaptionBarForm, IWeakEventTarget
    {
        // Separate action handler and root menu node for building the MainMenuStrip.
        private readonly UIActionHandler mainMenuActionHandler = new UIActionHandler();
        private readonly List<UIMenuNode> mainMenuRootNodes = new List<UIMenuNode>();

        public MdiContainerForm()
        {
            Icon = Session.Current.ApplicationIcon;
            Text = Session.ExecutableFileNameWithoutExtension;

            // Initialize UIActions before building the MainMenuStrip based on it.
            InitializeUIActions();

            UIMenuBuilder.BuildMenu(mainMenuActionHandler, mainMenuRootNodes, MainMenuStrip.Items);

            // After building the MainMenuStrip, build an index of ToolstripMenuItems which are bound on focus dependent UIActions.
            IndexFocusDependentUIActions(MainMenuStrip.Items);

            Session.Current.LocalSettings.RegisterSettingsChangedHandler(Session.Current.DeveloperMode, DeveloperModeChanged);
            ShowOrHideEditCurrentLanguageItem();

            Session.Current.CurrentLocalizerChanged += CurrentLocalizerChanged;

            AllowDrop = true;
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
                Program.MainForm.OpenCommandLineArgs((string[])e.Data.GetData(DataFormats.FileDrop));
            }

            base.OnDragDrop(e);
        }

        private void CurrentLocalizerChanged(object sender, EventArgs e)
        {
            UIMenu.UpdateMenu(MainMenuStrip.Items);
        }

        private void ShowOrHideEditCurrentLanguageItem()
        {
            // Make an exception for developer actions: hide them when they're disabled.
            // This code is fragile because it assumes several things about the location
            // and the localized string keys of the involved menu items.
            foreach (ToolStripItem item in MainMenuStrip.Items)
            {
                if (item is LocalizedToolStripMenuItem localizedItem
                    && localizedItem.TextProvider is LocalizedTextProvider localizedTextProvider
                    && localizedTextProvider.Key == SharedLocalizedStringKeys.Tools)
                {
                    ToolStripItem previousItem = null;

                    foreach (ToolStripItem dropDownItem in localizedItem.DropDownItems)
                    {
                        if (dropDownItem is UIActionToolStripMenuItem uiActionItem
                            && uiActionItem.TextProvider is LocalizedTextProvider subLocalizedTextProvider
                            && (subLocalizedTextProvider.Key == SharedLocalizedStringKeys.EditCurrentLanguage
                                || subLocalizedTextProvider.Key == SharedLocalizedStringKeys.OpenLocalAppDataFolder
                                || subLocalizedTextProvider.Key == SharedLocalizedStringKeys.OpenExecutableFolder))
                        {
                            // Use ActionHandler rather than mainMenuActionHandler because it can return UIActionVisibility.Hidden.
                            var uiActionState = ActionHandler.TryPerformAction(uiActionItem.Action, false);

                            bool visible = uiActionState.UIActionVisibility != UIActionVisibility.Hidden;
                            uiActionItem.Visible = visible;

                            // Hide/show ToolStripSeparator as well.
                            previousItem.Visible = visible;
                        }

                        previousItem = dropDownItem;
                    }

                    break;
                }
            }
        }

        private void DeveloperModeChanged(object sender, EventArgs e)
        {
            UpdateFocusDependentMenuItems();
            ShowOrHideEditCurrentLanguageItem();
        }

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
                if (binding.DefaultInterfaces.TryGet(out IContextMenuUIActionInterface contextMenuInterface))
                {
                    // Add a menu item inside the given container which will update itself after focus changes.
                    container.Nodes.Add(new UIMenuNode.Element(binding.Action, contextMenuInterface));

                    // Register in a Dictionary to be able to figure out which menu items should be updated.
                    focusDependentUIActions.Add(binding.Action, new FocusDependentUIActionState());

                    // This also means that if a menu item is clicked, TryPerformAction() is called on the mainMenuActionHandler.
                    mainMenuActionHandler.BindAction(new UIActionBinding(binding, perform =>
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

                UIMenuNode.Container langMenu = new UIMenuNode.Container(null, SharedResources.globe.ToImageProvider());
                mainMenuRootNodes.Add(langMenu);
                BindFocusDependentUIActions(langMenu, Session.Current.RegisteredLocalizers.Select(x => x.SwitchToLangUIActionBinding).ToArray());
            }

            // Actions which have their handler in this instance.
            this.BindAction(SandraChessMainForm.NewPgnFile, Program.MainForm.TryNewPgnFile);
            this.BindAction(SandraChessMainForm.OpenPgnFile, Program.MainForm.TryOpenPgnFile);
            this.BindAction(SharedUIAction.Exit, TryExit);

            this.BindAction(Session.EditPreferencesFile, Session.Current.TryEditPreferencesFile());
            this.BindAction(Session.ShowDefaultSettingsFile, Session.Current.TryShowDefaultSettingsFile());
            this.BindAction(OpenNewPlayingBoard, TryOpenNewPlayingBoard);
            this.BindAction(Session.OpenAbout, Session.Current.TryOpenAbout(this));
            this.BindAction(Session.ShowCredits, Session.Current.TryShowCredits(this));
            this.BindAction(Session.EditCurrentLanguage, Session.Current.TryEditCurrentLanguage());
            this.BindAction(Session.OpenLocalAppDataFolder, Session.Current.TryOpenLocalAppDataFolder());
            this.BindAction(Session.OpenExecutableFolder, Session.Current.TryOpenExecutableFolder());

            UIMenuNode.Container fileMenu = new UIMenuNode.Container(SharedLocalizedStringKeys.File.ToTextProvider());
            mainMenuRootNodes.Add(fileMenu);

            // Add these actions to the "File" dropdown list.
            BindFocusDependentUIActions(fileMenu,
                                        SandraChessMainForm.NewPgnFile,
                                        SandraChessMainForm.OpenPgnFile,
                                        SharedUIAction.Exit);

            UIMenuNode.Container editMenu = new UIMenuNode.Container(SharedLocalizedStringKeys.Edit.ToTextProvider());
            mainMenuRootNodes.Add(editMenu);

            // Add these actions to the "Game" dropdown list.
            BindFocusDependentUIActions(editMenu,
                                        OpenNewPlayingBoard);

            UIMenuNode.Container goToMenu = new UIMenuNode.Container(LocalizedStringKeys.GoTo.ToTextProvider())
            {
                IsFirstInGroup = true,
            };
            editMenu.Nodes.Add(goToMenu);

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

            BindFocusDependentUIActions(editMenu,
                                        InteractiveGame.PromoteActiveVariation,
                                        InteractiveGame.DemoteActiveVariation,
                                        InteractiveGame.BreakActiveVariation,
                                        InteractiveGame.DeleteActiveVariation,
                                        MovesTextBox.UsePgnPieceSymbols,
                                        MovesTextBox.UseLongAlgebraicNotation,
                                        StandardChessBoardForm.FlipBoard,
                                        StandardChessBoardForm.TakeScreenshot);

            UIMenuNode.Container viewMenu = new UIMenuNode.Container(SharedLocalizedStringKeys.View.ToTextProvider());
            mainMenuRootNodes.Add(viewMenu);

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

            var modifiedGotoMovesForm = new DefaultUIActionBinding(
                InteractiveGame.GotoMovesForm.Action,
                new ImplementationSet<IUIActionInterface>
                {
                    new CombinedUIActionInterface
                    {
                        Shortcuts = InteractiveGame.GotoMovesForm.DefaultInterfaces.Get<IShortcutKeysUIActionInterface>().Shortcuts,
                        MenuTextProvider = LocalizedStringKeys.Moves.ToTextProvider(),
                    },
                });

            // Add these actions to the "View" dropdown list.
            BindFocusDependentUIActions(viewMenu,
                                        modifiedGotoChessBoardForm,
                                        modifiedGotoMovesForm,
                                        SharedUIAction.ZoomIn,
                                        SharedUIAction.ZoomOut);

            UIMenuNode.Container toolsMenu = new UIMenuNode.Container(SharedLocalizedStringKeys.Tools.ToTextProvider());
            mainMenuRootNodes.Add(toolsMenu);

            // Add these actions to the "Tools" dropdown list.
            BindFocusDependentUIActions(toolsMenu,
                                        Session.EditPreferencesFile,
                                        Session.ShowDefaultSettingsFile,
                                        Session.EditCurrentLanguage,
                                        Session.OpenLocalAppDataFolder,
                                        Session.OpenExecutableFolder);

            UIMenuNode.Container helpMenu = new UIMenuNode.Container(SharedLocalizedStringKeys.Help.ToTextProvider());
            mainMenuRootNodes.Add(helpMenu);

            // Add these actions to the "Help" dropdown list.
            BindFocusDependentUIActions(helpMenu,
                                        Session.OpenAbout,
                                        Session.ShowCredits);

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

            NewPlayingBoard();
        }
    }
}

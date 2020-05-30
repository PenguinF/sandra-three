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

using Eutherion;
using Eutherion.UIActions;
using Eutherion.Utils;
using Eutherion.Win.MdiAppTemplate;
using Eutherion.Win.UIActions;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Sandra.UI
{
    /// <summary>
    /// Main MdiContainer Form.
    /// </summary>
    public partial class MdiContainerForm : OldMenuCaptionBarForm, IWeakEventTarget
    {
        private readonly List<UIMenuNode> mainMenuRootNodes = new List<UIMenuNode>();

        public MdiContainerForm()
        {
            DockProperties dockProperties = new DockProperties
            {
                CaptionHeight = 30,
                CaptionText = Session.ExecutableFileNameWithoutExtension,
                Icon = Session.Current.ApplicationIcon,
                MainMenuItems = InitializeUIActions(),
            };

            UpdateFromDockProperties(dockProperties);

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
                Program.MainForm.OpenCommandLineArgs(this, (string[])e.Data.GetData(DataFormats.FileDrop));
            }

            base.OnDragDrop(e);
        }

        private List<MainMenuDropDownItem> InitializeUIActions()
        {
            // Define the main menu.
            var mainMenuRootNodes = new List<MainMenuDropDownItem>();

            if (Session.Current.RegisteredLocalizers.Count() >= 2)
            {
                // More than one localizer: can switch between them.
                foreach (var localizer in Session.Current.RegisteredLocalizers)
                {
                    this.BindAction(localizer.SwitchToLangUIActionBinding, localizer.TrySwitchToLang);
                }

                var langMenu = new List<Union<DefaultUIActionBinding, MainMenuDropDownItem>>();
                langMenu.AddRange(Session.Current.RegisteredLocalizers.Select(x => (Union<DefaultUIActionBinding, MainMenuDropDownItem>)x.SwitchToLangUIActionBinding));

                mainMenuRootNodes.Add(new MainMenuDropDownItem
                {
                    Container = new UIMenuNode.Container(null, SharedResources.globe.ToImageProvider()),
                    DropDownItems = langMenu
                });
            }

            // Actions which have their handler in this instance.
            this.BindAction(NewPgnFile, TryNewPgnFile);
            this.BindAction(OpenPgnFile, TryOpenPgnFile);
            this.BindAction(SharedUIAction.Exit, TryExit);

            this.BindAction(Session.EditPreferencesFile, Session.Current.TryEditPreferencesFile());
            this.BindAction(Session.ShowDefaultSettingsFile, Session.Current.TryShowDefaultSettingsFile());
            this.BindAction(OpenNewPlayingBoard, TryOpenNewPlayingBoard);
            this.BindAction(Session.OpenAbout, Session.Current.TryOpenAbout(this));
            this.BindAction(Session.ShowCredits, Session.Current.TryShowCredits(this));
            this.BindAction(Session.EditCurrentLanguage, Session.Current.TryEditCurrentLanguage());
            this.BindAction(Session.OpenLocalAppDataFolder, Session.Current.TryOpenLocalAppDataFolder());
            this.BindAction(Session.OpenExecutableFolder, Session.Current.TryOpenExecutableFolder());

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
                    MovesTextBox.UsePgnPieceSymbols,
                    MovesTextBox.UseLongAlgebraicNotation,
                    StandardChessBoardForm.FlipBoard,
                    StandardChessBoardForm.TakeScreenshot,

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

            mainMenuRootNodes.Add(new MainMenuDropDownItem
            {
                Container = new UIMenuNode.Container(SharedLocalizedStringKeys.View.ToTextProvider()),
                DropDownItems = new List<Union<DefaultUIActionBinding, MainMenuDropDownItem>>
                {
                    modifiedGotoChessBoardForm,
                    modifiedGotoMovesForm,
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

            return mainMenuRootNodes;
        }
    }
}

/*********************************************************************************
 * InteractiveGame.UIActions.cs
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
using System.Drawing;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    public partial class InteractiveGame
    {
        public const string InteractiveGameUIActionPrefix = nameof(InteractiveGame) + ".";


        public static readonly DefaultUIActionBinding GotoChessBoardForm = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(GotoChessBoardForm)),
            new UIActionBinding()
            {
                IsFirstInGroup = true,
                MenuCaption = "Chessboard",
                Shortcuts = new List<ShortcutKeys> { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.B), },
            });

        public UIActionState TryGotoChessBoardForm(bool perform)
        {
            if (perform)
            {
                if (chessBoardForm == null)
                {
                    StandardChessBoardForm newChessBoardForm = new StandardChessBoardForm();
                    newChessBoardForm.MdiParent = OwnerForm;
                    newChessBoardForm.Game = this;
                    newChessBoardForm.PieceImages = OwnerForm.PieceImages;
                    newChessBoardForm.PlayingBoard.ForegroundImageRelativeSize = 0.9f;

                    if (movesForm != null && movesForm.WindowState == FormWindowState.Normal)
                    {
                        // Place directly to the right.
                        var mdiChildBounds = movesForm.Bounds;
                        newChessBoardForm.StartPosition = FormStartPosition.Manual;
                        newChessBoardForm.ClientSize = new Size(movesForm.ClientSize.Height, movesForm.ClientSize.Height);
                        newChessBoardForm.Location = new Point(mdiChildBounds.Right, mdiChildBounds.Top);
                    }
                    else
                    {
                        // Only specify its default size.
                        newChessBoardForm.ClientSize = new Size(400, 400);
                    }

                    newChessBoardForm.PerformAutoFit();

                    newChessBoardForm.PlayingBoard.BindActions(new UIActionBindings
                    {
                        { GotoChessBoardForm, TryGotoChessBoardForm },
                        { GotoMovesForm, TryGotoMovesForm },

                        { GotoFirstMove, TryGotoFirstMove },
                        { FastNavigateBackward, TryFastNavigateBackward },
                        { GotoPreviousMove, TryGotoPreviousMove },
                        { GotoNextMove, TryGotoNextMove },
                        { FastNavigateForward, TryFastNavigateForward },
                        { GotoLastMove, TryGotoLastMove },

                        { GotoPreviousVariation, TryGotoPreviousVariation },
                        { GotoNextVariation, TryGotoNextVariation },

                        { PromoteActiveVariation, TryPromoteActiveVariation },
                        { DemoteActiveVariation, TryDemoteActiveVariation },

                        { StandardChessBoardForm.TakeScreenshot, newChessBoardForm.TryTakeScreenshot },
                    });

                    UIMenu.AddTo(newChessBoardForm.PlayingBoard);

                    chessBoardForm = newChessBoardForm;
                    chessBoardForm.Disposed += (_, __) =>
                    {
                        chessBoardForm = null;

                        // To refresh the state of the GotoChessBoardForm action elsewhere.
                        var movesTextBox = getMovesTextBox();
                        if (movesTextBox != null) movesTextBox.ActionHandler.Invalidate();
                    };
                }

                if (!chessBoardForm.ContainsFocus)
                {
                    chessBoardForm.Visible = true;
                    chessBoardForm.Activate();
                }
            }

            return UIActionVisibility.Enabled;
        }


        public static readonly DefaultUIActionBinding GotoMovesForm = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(GotoMovesForm)),
            new UIActionBinding()
            {
                MenuCaption = "Moves",
                Shortcuts = new List<ShortcutKeys> { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.M), },
            });

        public UIActionState TryGotoMovesForm(bool perform)
        {
            if (perform)
            {
                if (movesForm == null)
                {
                    SnappingMdiChildForm newMovesForm = new SnappingMdiChildForm()
                    {
                        MdiParent = OwnerForm,
                        ShowIcon = false,
                        MaximizeBox = false,
                        FormBorderStyle = FormBorderStyle.SizableToolWindow,
                    };

                    if (chessBoardForm != null && chessBoardForm.WindowState == FormWindowState.Normal)
                    {
                        // Place directly to the right.
                        var mdiChildBounds = chessBoardForm.Bounds;
                        newMovesForm.StartPosition = FormStartPosition.Manual;
                        newMovesForm.Location = new Point(mdiChildBounds.Right, mdiChildBounds.Top);
                        newMovesForm.ClientSize = new Size(200, chessBoardForm.ClientSize.Height);
                    }
                    else
                    {
                        // Only specify its default size.
                        newMovesForm.ClientSize = new Size(200, 400);
                    }

                    var movesTextBox = new MovesTextBox()
                    {
                        Dock = DockStyle.Fill,
                        Game = this,
                        MoveFormatter = new ShortAlgebraicMoveFormatter(OwnerForm.CurrentPieceSymbols),
                    };

                    movesTextBox.BindActions(new UIActionBindings
                    {
                        { GotoChessBoardForm, TryGotoChessBoardForm },
                        { GotoMovesForm, TryGotoMovesForm },

                        { GotoFirstMove, TryGotoFirstMove },
                        { FastNavigateBackward, TryFastNavigateBackward },
                        { GotoPreviousMove, TryGotoPreviousMove },
                        { GotoNextMove, TryGotoNextMove },
                        { FastNavigateForward, TryFastNavigateForward },
                        { GotoLastMove, TryGotoLastMove },

                        { GotoPreviousVariation, TryGotoPreviousVariation },
                        { GotoNextVariation, TryGotoNextVariation },

                        { PromoteActiveVariation, TryPromoteActiveVariation },
                        { DemoteActiveVariation, TryDemoteActiveVariation },
                    });

                    UIMenu.AddTo(movesTextBox);

                    newMovesForm.Controls.Add(movesTextBox);

                    movesForm = newMovesForm;
                    movesForm.Disposed += (_, __) =>
                    {
                        movesForm = null;

                        // To refresh the state of the GotoMovesForm action elsewhere.
                        if (chessBoardForm != null) chessBoardForm.PlayingBoard.ActionHandler.Invalidate();
                    };
                }

                if (!movesForm.ContainsFocus)
                {
                    movesForm.Visible = true;
                    movesForm.Activate();
                }
            }

            return UIActionVisibility.Enabled;
        }


        public static readonly DefaultUIActionBinding GotoFirstMove = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(GotoFirstMove)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                IsFirstInGroup = true,
                MenuCaption = "First move",
                Shortcuts = new List<ShortcutKeys>
                {
                    new ShortcutKeys(ConsoleKey.Home),
                    new ShortcutKeys(KeyModifiers.Control, ConsoleKey.Home),
                },
            });

        public UIActionState TryGotoFirstMove(bool perform)
        {
            if (Game.IsFirstMove) return UIActionVisibility.Disabled;
            if (perform)
            {
                do Game.Backward(); while (!Game.IsFirstMove);
                ActiveMoveTreeUpdated();
            }
            return UIActionVisibility.Enabled;
        }


        public static readonly DefaultUIActionBinding FastNavigateBackward = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(FastNavigateBackward)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                MenuCaption = "Fast backward",
                Shortcuts = new List<ShortcutKeys>
                {
                    new ShortcutKeys(ConsoleKey.PageUp),
                    new ShortcutKeys(KeyModifiers.Control, ConsoleKey.PageUp),
                },
            });

        public UIActionState TryFastNavigateBackward(bool perform)
        {
            if (Game.IsFirstMove) return UIActionVisibility.Disabled;
            if (perform)
            {
                OwnerForm.FastNavigationPlyCount.Times(Game.Backward);
                ActiveMoveTreeUpdated();
            }
            return UIActionVisibility.Enabled;
        }


        public static readonly DefaultUIActionBinding GotoPreviousMove = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(GotoPreviousMove)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                MenuCaption = "Previous move",
                Shortcuts = new List<ShortcutKeys>
                {
                    new ShortcutKeys(ConsoleKey.LeftArrow),
                    new ShortcutKeys(KeyModifiers.Control, ConsoleKey.LeftArrow),
                },
            });

        public UIActionState TryGotoPreviousMove(bool perform)
        {
            if (Game.IsFirstMove) return UIActionVisibility.Disabled;
            if (perform)
            {
                Game.Backward();
                ActiveMoveTreeUpdated();
            }
            return UIActionVisibility.Enabled;
        }


        public static readonly DefaultUIActionBinding GotoNextMove = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(GotoNextMove)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                MenuCaption = "Next move",
                Shortcuts = new List<ShortcutKeys>
                {
                    new ShortcutKeys(ConsoleKey.RightArrow),
                    new ShortcutKeys(KeyModifiers.Control, ConsoleKey.RightArrow),
                },
            });

        public UIActionState TryGotoNextMove(bool perform)
        {
            if (Game.IsLastMove) return UIActionVisibility.Disabled;
            if (perform)
            {
                Game.Forward();
                ActiveMoveTreeUpdated();
            }
            return UIActionVisibility.Enabled;
        }


        public static readonly DefaultUIActionBinding FastNavigateForward = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(FastNavigateForward)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                MenuCaption = "Fast forward",
                Shortcuts = new List<ShortcutKeys>
                {
                    new ShortcutKeys(ConsoleKey.PageDown),
                    new ShortcutKeys(KeyModifiers.Control, ConsoleKey.PageDown),
                },
            });

        public UIActionState TryFastNavigateForward(bool perform)
        {
            if (Game.IsLastMove) return UIActionVisibility.Disabled;
            if (perform)
            {
                OwnerForm.FastNavigationPlyCount.Times(Game.Forward);
                ActiveMoveTreeUpdated();
            }
            return UIActionVisibility.Enabled;
        }


        public static readonly DefaultUIActionBinding GotoLastMove = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(GotoLastMove)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                MenuCaption = "Last move",
                Shortcuts = new List<ShortcutKeys>
                {
                    new ShortcutKeys(ConsoleKey.End),
                    new ShortcutKeys(KeyModifiers.Control, ConsoleKey.End),
                },
            });

        public UIActionState TryGotoLastMove(bool perform)
        {
            if (Game.IsLastMove) return UIActionVisibility.Disabled;
            if (perform)
            {
                do Game.Forward(); while (!Game.IsLastMove);
                ActiveMoveTreeUpdated();
            }
            return UIActionVisibility.Enabled;
        }


        public static readonly DefaultUIActionBinding GotoPreviousVariation = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(GotoPreviousVariation)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                IsFirstInGroup = true,
                MenuCaption = "Previous line",
                Shortcuts = new List<ShortcutKeys>
                {
                    new ShortcutKeys(ConsoleKey.UpArrow),
                    new ShortcutKeys(KeyModifiers.Control, ConsoleKey.UpArrow),
                },
            });

        public UIActionState TryGotoPreviousVariation(bool perform)
        {
            return UIActionVisibility.Enabled;
        }


        public static readonly DefaultUIActionBinding GotoNextVariation = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(GotoNextVariation)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                MenuCaption = "Next line",
                Shortcuts = new List<ShortcutKeys>
                {
                    new ShortcutKeys(ConsoleKey.DownArrow),
                    new ShortcutKeys(KeyModifiers.Control, ConsoleKey.DownArrow),
                },
            });

        public UIActionState TryGotoNextVariation(bool perform)
        {
            return UIActionVisibility.Enabled;
        }


        public static readonly DefaultUIActionBinding PromoteActiveVariation = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(PromoteActiveVariation)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                IsFirstInGroup = true,
                MenuCaption = "Promote line",
                Shortcuts = new List<ShortcutKeys>
                {
                    new ShortcutKeys(ConsoleKey.P),
                    new ShortcutKeys(KeyModifiers.Control, ConsoleKey.P),
                },
            });

        public UIActionState TryPromoteActiveVariation(bool perform)
        {
            return UIActionVisibility.Enabled;
        }


        public static readonly DefaultUIActionBinding DemoteActiveVariation = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(DemoteActiveVariation)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                MenuCaption = "Demote line",
                Shortcuts = new List<ShortcutKeys>
                {
                    new ShortcutKeys(ConsoleKey.D),
                    new ShortcutKeys(KeyModifiers.Control, ConsoleKey.D),
                },
            });

        public UIActionState TryDemoteActiveVariation(bool perform)
        {
            return UIActionVisibility.Enabled;
        }
    }
}

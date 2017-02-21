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
                        { GotoPreviousMove, TryGotoPreviousMove },
                        { GotoNextMove, TryGotoNextMove },
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
                        { GotoPreviousMove, TryGotoPreviousMove },
                        { GotoNextMove, TryGotoNextMove },
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


        public static readonly DefaultUIActionBinding GotoPreviousMove = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(GotoPreviousMove)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                IsFirstInGroup = true,
                MenuCaption = "Previous move",
                Shortcuts = new List<ShortcutKeys>
                {
                    new ShortcutKeys(ConsoleKey.LeftArrow),
                    new ShortcutKeys(KeyModifiers.Control, ConsoleKey.LeftArrow),
                    new ShortcutKeys(ConsoleKey.Z),
                },
            });

        public UIActionState TryGotoPreviousMove(bool perform)
        {
            if (Game.ActiveMoveIndex == 0) return UIActionVisibility.Disabled;
            if (perform) Game.ActiveMoveIndex--;
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
                    new ShortcutKeys(ConsoleKey.X),
                },
            });

        public UIActionState TryGotoNextMove(bool perform)
        {
            if (Game.ActiveMoveIndex == Game.MoveCount) return UIActionVisibility.Disabled;
            if (perform) Game.ActiveMoveIndex++;
            return UIActionVisibility.Enabled;
        }
    }
}

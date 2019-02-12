#region License
/*********************************************************************************
 * InteractiveGame.UIActions.cs
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

using Eutherion;
using Eutherion.Win.Forms;
using Sandra.Chess;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    public partial class InteractiveGame
    {
        public const string InteractiveGameUIActionPrefix = nameof(InteractiveGame) + ".";


        public static readonly DefaultUIActionBinding GotoChessBoardForm = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(GotoChessBoardForm)),
            new UIActionBinding
            {
                IsFirstInGroup = true,
                MenuCaptionKey = LocalizedStringKeys.Chessboard,
                Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.B), },
            });

        public UIActionState TryGotoChessBoardForm(bool perform)
        {
            if (perform)
            {
                if (chessBoardForm == null)
                {
                    StandardChessBoardForm newChessBoardForm = new StandardChessBoardForm
                    {
                        MdiParent = OwnerForm,
                        Game = this,
                        PieceImages = OwnerForm.PieceImages
                    };

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

                    newChessBoardForm.PerformAutoFit(null);

                    newChessBoardForm.PlayingBoard.BindActions(new UIActionBindings
                    {
                        { GotoChessBoardForm, TryGotoChessBoardForm },
                        { GotoMovesForm, TryGotoMovesForm },

                        { GotoStart, TryGotoStart },
                        { GotoFirstMove, TryGotoFirstMove },
                        { FastNavigateBackward, TryFastNavigateBackward },
                        { GotoPreviousMove, TryGotoPreviousMove },
                        { GotoNextMove, TryGotoNextMove },
                        { FastNavigateForward, TryFastNavigateForward },
                        { GotoLastMove, TryGotoLastMove },
                        { GotoEnd, TryGotoEnd },

                        { GotoPreviousVariation, TryGotoPreviousVariation },
                        { GotoNextVariation, TryGotoNextVariation },

                        { PromoteActiveVariation, TryPromoteActiveVariation },
                        { DemoteActiveVariation, TryDemoteActiveVariation },
                        { BreakActiveVariation, TryBreakActiveVariation },
                        { DeleteActiveVariation, TryDeleteActiveVariation },

                        { StandardChessBoardForm.FlipBoard, newChessBoardForm.TryFlipBoard },
                        { StandardChessBoardForm.TakeScreenshot, newChessBoardForm.TryTakeScreenshot },

                        { SharedUIAction.ZoomIn, newChessBoardForm.TryZoomIn },
                        { SharedUIAction.ZoomOut, newChessBoardForm.TryZoomOut },
                    });

                    UIMenu.AddTo(newChessBoardForm.PlayingBoard);

                    chessBoardForm = newChessBoardForm;
                    chessBoardForm.Disposed += (_, __) =>
                    {
                        chessBoardForm = null;

                        // To refresh the state of the GotoChessBoardForm action elsewhere.
                        var movesTextBox = GetMovesTextBox();
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
            new UIActionBinding
            {
                MenuCaptionKey = LocalizedStringKeys.Moves,
                Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.M), },
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
                    };

                    movesTextBox.BindActions(new UIActionBindings
                    {
                        { GotoChessBoardForm, TryGotoChessBoardForm },
                        { GotoMovesForm, TryGotoMovesForm },

                        { GotoStart, TryGotoStart },
                        { GotoFirstMove, TryGotoFirstMove },
                        { FastNavigateBackward, TryFastNavigateBackward },
                        { GotoPreviousMove, TryGotoPreviousMove },
                        { GotoNextMove, TryGotoNextMove },
                        { FastNavigateForward, TryFastNavigateForward },
                        { GotoLastMove, TryGotoLastMove },
                        { GotoEnd, TryGotoEnd },

                        { GotoPreviousVariation, TryGotoPreviousVariation },
                        { GotoNextVariation, TryGotoNextVariation },

                        { PromoteActiveVariation, TryPromoteActiveVariation },
                        { DemoteActiveVariation, TryDemoteActiveVariation },
                        { BreakActiveVariation, TryBreakActiveVariation },
                        { DeleteActiveVariation, TryDeleteActiveVariation },

                        { MovesTextBox.UsePGNPieceSymbols, movesTextBox.TryUsePGNPieceSymbols },
                        { MovesTextBox.UseLongAlgebraicNotation, movesTextBox.TryUseLongAlgebraicNotation },
                    });

                    movesTextBox.BindStandardEditUIActions();

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


        public static readonly DefaultUIActionBinding GotoStart = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(GotoStart)),
            new UIActionBinding
            {
                ShowInMenu = true,
                IsFirstInGroup = true,
                MenuCaptionKey = LocalizedStringKeys.StartOfGame,
                Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.Home), },
            });

        public UIActionState TryGotoStart(bool perform)
        {
            if (Game.IsFirstMove) return UIActionVisibility.Disabled;
            if (perform)
            {
                do Game.Backward(); while (!Game.IsFirstMove);
                ActiveMoveTreeUpdated();
            }
            return UIActionVisibility.Enabled;
        }


        public static readonly DefaultUIActionBinding GotoFirstMove = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(GotoFirstMove)),
            new UIActionBinding
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.FirstMove,
                Shortcuts = new[] { new ShortcutKeys(ConsoleKey.Home), },
            });

        public UIActionState TryGotoFirstMove(bool perform)
        {
            if (Game.IsFirstMove) return UIActionVisibility.Disabled;
            if (perform)
            {
                // Go to the first move in this line, but make sure to not get stuck with repeated use.
                Game.Backward();
                while (Game.ActiveTree.ParentVariation != null
                    && Game.ActiveTree.ParentVariation.VariationIndex == 0)
                {
                    Game.Backward();
                }
                ActiveMoveTreeUpdated();
            }
            return UIActionVisibility.Enabled;
        }


        public static readonly DefaultUIActionBinding FastNavigateBackward = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(FastNavigateBackward)),
            new UIActionBinding
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.FastBackward,
                Shortcuts = new[]
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
                Program.GetSetting(SettingKeys.FastNavigationPlyCount).Times(Game.Backward);
                ActiveMoveTreeUpdated();
            }
            return UIActionVisibility.Enabled;
        }


        public static readonly DefaultUIActionBinding GotoPreviousMove = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(GotoPreviousMove)),
            new UIActionBinding
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.PreviousMove,
                Shortcuts = new[]
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
            new UIActionBinding
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.NextMove,
                Shortcuts = new[]
                {
                    new ShortcutKeys(ConsoleKey.RightArrow),
                    new ShortcutKeys(KeyModifiers.Control, ConsoleKey.RightArrow),
                },
            });

        public UIActionState TryGotoNextMove(bool perform)
        {
            // Use this action to be able to navigate to side lines beyond the end of the main line.
            if (Game.ActiveTree.MainLine == null && !Game.ActiveTree.SideLines.Any())
            {
                return UIActionVisibility.Disabled;
            }

            if (perform)
            {
                if (Game.ActiveTree.MainLine != null)
                {
                    Game.Forward();
                }
                else
                {
                    Game.SetActiveTree(Game.ActiveTree.SideLines.First().MoveTree);
                }
                ActiveMoveTreeUpdated();
            }
            return UIActionVisibility.Enabled;
        }


        public static readonly DefaultUIActionBinding FastNavigateForward = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(FastNavigateForward)),
            new UIActionBinding
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.FastForward,
                Shortcuts = new[]
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
                Program.GetSetting(SettingKeys.FastNavigationPlyCount).Times(Game.Forward);
                ActiveMoveTreeUpdated();
            }
            return UIActionVisibility.Enabled;
        }


        public static readonly DefaultUIActionBinding GotoLastMove = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(GotoLastMove)),
            new UIActionBinding
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.LastMove,
                Shortcuts = new[] { new ShortcutKeys(ConsoleKey.End), },
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


        public static readonly DefaultUIActionBinding GotoEnd = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(GotoEnd)),
            new UIActionBinding
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.EndOfGame,
                Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.End), },
            });

        public UIActionState TryGotoEnd(bool perform)
        {
            if (Game.IsLastMove && GetFirstMove(Game.ActiveTree.ParentVariation) == null)
            {
                // Last move in the main line of the game.
                return UIActionVisibility.Disabled;
            }

            if (perform)
            {
                Game.SetActiveTree(Game.MoveTree);
                while (!Game.IsLastMove) Game.Forward();
                ActiveMoveTreeUpdated();
            }
            return UIActionVisibility.Enabled;
        }


        public static readonly DefaultUIActionBinding GotoPreviousVariation = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(GotoPreviousVariation)),
            new UIActionBinding
            {
                ShowInMenu = true,
                IsFirstInGroup = true,
                MenuCaptionKey = LocalizedStringKeys.PreviousLine,
                Shortcuts = new[]
                {
                    new ShortcutKeys(ConsoleKey.UpArrow),
                    new ShortcutKeys(KeyModifiers.Control, ConsoleKey.UpArrow),
                },
            });

        public UIActionState TryGotoPreviousVariation(bool perform)
        {
            Variation currentVariation = Game.ActiveTree.ParentVariation;
            if (currentVariation != null && currentVariation.VariationIndex > 0)
            {
                Variation previousVariation = currentVariation.ParentTree.Variations[currentVariation.VariationIndex - 1];
                if (previousVariation != null)
                {
                    if (perform)
                    {
                        Game.SetActiveTree(previousVariation.MoveTree);
                        ActiveMoveTreeUpdated();
                    }
                    return UIActionVisibility.Enabled;
                }
            }
            return UIActionVisibility.Disabled;
        }


        public static readonly DefaultUIActionBinding GotoNextVariation = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(GotoNextVariation)),
            new UIActionBinding
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.NextLine,
                Shortcuts = new[]
                {
                    new ShortcutKeys(ConsoleKey.DownArrow),
                    new ShortcutKeys(KeyModifiers.Control, ConsoleKey.DownArrow),
                },
            });

        public UIActionState TryGotoNextVariation(bool perform)
        {
            Variation currentVariation = Game.ActiveTree.ParentVariation;
            if (currentVariation != null && currentVariation.VariationIndex + 1 < currentVariation.ParentTree.Variations.Count)
            {
                if (perform)
                {
                    Variation nextVariation = currentVariation.ParentTree.Variations[currentVariation.VariationIndex + 1];
                    Game.SetActiveTree(nextVariation.MoveTree);
                    ActiveMoveTreeUpdated();
                }
                return UIActionVisibility.Enabled;
            }
            return UIActionVisibility.Disabled;
        }


        public static readonly DefaultUIActionBinding PromoteActiveVariation = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(PromoteActiveVariation)),
            new UIActionBinding
            {
                ShowInMenu = true,
                IsFirstInGroup = true,
                MenuCaptionKey = LocalizedStringKeys.PromoteLine,
                Shortcuts = new[]
                {
                    new ShortcutKeys(ConsoleKey.P),
                    new ShortcutKeys(KeyModifiers.Control, ConsoleKey.P),
                },
            });

        public UIActionState TryPromoteActiveVariation(bool perform)
        {
            // Find the first move in this variation.
            Variation firstMoveInVariation = GetFirstMove(Game.ActiveTree.ParentVariation);

            if (firstMoveInVariation == null)
            {
                // Already the main line of the game.
                return UIActionVisibility.Disabled;
            }

            if (perform)
            {
                firstMoveInVariation.RepositionBefore(firstMoveInVariation.VariationIndex - 1);
                ActiveMoveTreeUpdated();
            }
            return UIActionVisibility.Enabled;
        }


        public static readonly DefaultUIActionBinding DemoteActiveVariation = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(DemoteActiveVariation)),
            new UIActionBinding
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.DemoteLine,
                Shortcuts = new[]
                {
                    new ShortcutKeys(ConsoleKey.D),
                    new ShortcutKeys(KeyModifiers.Control, ConsoleKey.D),
                },
            });

        public UIActionState TryDemoteActiveVariation(bool perform)
        {
            // Find the first move in this variation which has a 'less important' side line.
            Variation moveWithSideLine = Game.ActiveTree.ParentVariation;
            while (moveWithSideLine != null
                && moveWithSideLine.VariationIndex + 1 == moveWithSideLine.ParentTree.Variations.Count)
            {
                moveWithSideLine = moveWithSideLine.ParentTree.ParentVariation;
            }

            if (moveWithSideLine == null)
            {
                // Already no sidelines below this one.
                return UIActionVisibility.Disabled;
            }

            if (perform)
            {
                moveWithSideLine.RepositionAfter(moveWithSideLine.VariationIndex + 1);
                ActiveMoveTreeUpdated();
            }
            return UIActionVisibility.Enabled;
        }


        public static readonly DefaultUIActionBinding BreakActiveVariation = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(BreakActiveVariation)),
            new UIActionBinding
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.BreakAtCurrentPosition,
                Shortcuts = new[] { new ShortcutKeys(ConsoleKey.B), },
            });

        public UIActionState TryBreakActiveVariation(bool perform)
        {
            // If this move is the main line, turn it into a side line.
            if (Game.IsLastMove) return UIActionVisibility.Disabled;
            if (perform)
            {
                Game.ActiveTree.Break();
                ActiveMoveTreeUpdated();
            }
            return UIActionVisibility.Enabled;
        }


        public static readonly DefaultUIActionBinding DeleteActiveVariation = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(DeleteActiveVariation)),
            new UIActionBinding
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.DeleteLine,
                Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.Delete), },
            });

        public UIActionState TryDeleteActiveVariation(bool perform)
        {
            if (Game.IsFirstMove) return UIActionVisibility.Disabled;
            if (perform)
            {
                // Go backward, then remove the move which was just active and its move tree.
                Variation variationToRemove = Game.ActiveTree.ParentVariation;
                Game.Backward();
                Game.ActiveTree.RemoveVariation(variationToRemove);
                ActiveMoveTreeUpdated();
            }
            return UIActionVisibility.Enabled;
        }
    }
}

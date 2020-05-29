#region License
/*********************************************************************************
 * InteractiveGame.UIActions.cs
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
using Eutherion.Win;
using Eutherion.Win.Forms;
using Eutherion.Win.MdiAppTemplate;
using Eutherion.Win.UIActions;
using Sandra.Chess;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Sandra.UI
{
    public partial class InteractiveGame
    {
        public const string InteractiveGameUIActionPrefix = nameof(InteractiveGame) + ".";


        public static readonly DefaultUIActionBinding GotoChessBoardForm = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(GotoChessBoardForm)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.B), },
                },
            });

        public UIActionState TryGotoChessBoardForm(bool perform)
        {
            if (perform)
            {
                if (chessBoardForm == null)
                {
                    StandardChessBoardForm newChessBoardForm = new StandardChessBoardForm
                    {
                        Owner = OwnerForm,
                        Game = this,
                        PieceImages = PieceImages.ImageArray
                    };

                    newChessBoardForm.PlayingBoard.ForegroundImageRelativeSize = 0.9f;

                    if (movesForm != null && movesForm.WindowState == FormWindowState.Normal)
                    {
                        // Place directly to the right.
                        var mdiChildBounds = movesForm.Bounds;
                        newChessBoardForm.StartPosition = FormStartPosition.Manual;
                        newChessBoardForm.ClientSize = new Size(movesForm.ClientAreaSize.Height, movesForm.ClientAreaSize.Height);
                        newChessBoardForm.Location = new Point(mdiChildBounds.Right, mdiChildBounds.Top);
                    }
                    else
                    {
                        // Only specify its default size.
                        newChessBoardForm.ClientSize = new Size(400, 400);
                    }

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

                    newChessBoardForm.UpdateFromDockProperties(new DockProperties
                    {
                        CaptionHeight = 24,
                        Icon = Session.Current.ApplicationIcon,
                    });

                    // Only snap while moving.
                    OwnedFormSnapHelper snapHelper = OwnedFormSnapHelper.AttachTo(newChessBoardForm);
                    snapHelper.SnapWhileResizing = false;

                    newChessBoardForm.Load += (_, __) => newChessBoardForm.PerformAutoFit(null);

                    chessBoardForm = newChessBoardForm;
                    chessBoardForm.Disposed += (_, __) =>
                    {
                        chessBoardForm = null;

                        // To refresh the state of the GotoChessBoardForm action elsewhere.
                        var movesTextBox = GetMovesTextBox();
                        if (movesTextBox != null) movesTextBox.ActionHandler.Invalidate();
                    };
                }

                chessBoardForm.EnsureActivated();
            }

            return UIActionVisibility.Enabled;
        }


        public static readonly DefaultUIActionBinding GotoMovesForm = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(GotoMovesForm)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.M), },
                },
            });

        public UIActionState TryGotoMovesForm(bool perform)
        {
            if (perform)
            {
                if (movesForm == null)
                {
                    OldMenuCaptionBarForm newMovesForm = new OldMenuCaptionBarForm()
                    {
                        Owner = OwnerForm,
                        MaximizeBox = false,
                        FormBorderStyle = FormBorderStyle.SizableToolWindow,
                    };

                    if (chessBoardForm != null && chessBoardForm.WindowState == FormWindowState.Normal)
                    {
                        // Place directly to the right.
                        var mdiChildBounds = chessBoardForm.Bounds;
                        newMovesForm.StartPosition = FormStartPosition.Manual;
                        newMovesForm.Location = new Point(mdiChildBounds.Right, mdiChildBounds.Top);
                        newMovesForm.ClientSize = new Size(200, chessBoardForm.ClientAreaSize.Height);
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

                        { MovesTextBox.UsePgnPieceSymbols, movesTextBox.TryUsePgnPieceSymbols },
                        { MovesTextBox.UseLongAlgebraicNotation, movesTextBox.TryUseLongAlgebraicNotation },
                    });

                    movesTextBox.BindStandardEditUIActions();

                    UIMenu.AddTo(movesTextBox);

                    newMovesForm.Controls.Add(movesTextBox);

                    newMovesForm.UpdateFromDockProperties(new DockProperties
                    {
                        CaptionHeight = 24,
                        Icon = Session.Current.ApplicationIcon,
                    });

                    // Snap while moving and resizing.
                    OwnedFormSnapHelper.AttachTo(newMovesForm);

                    movesForm = newMovesForm;
                    movesForm.Disposed += (_, __) =>
                    {
                        movesForm = null;

                        // To refresh the state of the GotoMovesForm action elsewhere.
                        if (chessBoardForm != null) chessBoardForm.PlayingBoard.ActionHandler.Invalidate();
                    };
                }

                movesForm.EnsureActivated();
            }

            return UIActionVisibility.Enabled;
        }


        public static readonly DefaultUIActionBinding GotoStart = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(GotoStart)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.Home), },
                    IsFirstInGroup = true,
                    MenuTextProvider = LocalizedStringKeys.StartOfGame.ToTextProvider(),
                },
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
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(ConsoleKey.Home), },
                    MenuTextProvider = LocalizedStringKeys.FirstMove.ToTextProvider(),
                },
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
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[]
                    {
                        new ShortcutKeys(ConsoleKey.PageUp),
                        new ShortcutKeys(KeyModifiers.Control, ConsoleKey.PageUp),
                    },
                    MenuTextProvider = LocalizedStringKeys.FastBackward.ToTextProvider(),
                },
            });

        public UIActionState TryFastNavigateBackward(bool perform)
        {
            if (Game.IsFirstMove) return UIActionVisibility.Disabled;
            if (perform)
            {
                Session.Current.GetSetting(SettingKeys.FastNavigationPlyCount).Times(Game.Backward);
                ActiveMoveTreeUpdated();
            }
            return UIActionVisibility.Enabled;
        }


        public static readonly DefaultUIActionBinding GotoPreviousMove = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(GotoPreviousMove)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[]
                    {
                        new ShortcutKeys(ConsoleKey.LeftArrow),
                        new ShortcutKeys(KeyModifiers.Control, ConsoleKey.LeftArrow),
                    },
                    MenuTextProvider = LocalizedStringKeys.PreviousMove.ToTextProvider(),
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
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[]
                    {
                        new ShortcutKeys(ConsoleKey.RightArrow),
                        new ShortcutKeys(KeyModifiers.Control, ConsoleKey.RightArrow),
                    },
                    MenuTextProvider = LocalizedStringKeys.NextMove.ToTextProvider(),
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
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[]
                    {
                        new ShortcutKeys(ConsoleKey.PageDown),
                        new ShortcutKeys(KeyModifiers.Control, ConsoleKey.PageDown),
                    },
                    MenuTextProvider = LocalizedStringKeys.FastForward.ToTextProvider(),
                },
            });

        public UIActionState TryFastNavigateForward(bool perform)
        {
            if (Game.IsLastMove) return UIActionVisibility.Disabled;
            if (perform)
            {
                Session.Current.GetSetting(SettingKeys.FastNavigationPlyCount).Times(Game.Forward);
                ActiveMoveTreeUpdated();
            }
            return UIActionVisibility.Enabled;
        }


        public static readonly DefaultUIActionBinding GotoLastMove = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(GotoLastMove)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(ConsoleKey.End), },
                    MenuTextProvider = LocalizedStringKeys.LastMove.ToTextProvider(),
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


        public static readonly DefaultUIActionBinding GotoEnd = new DefaultUIActionBinding(
            new UIAction(InteractiveGameUIActionPrefix + nameof(GotoEnd)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.End), },
                    MenuTextProvider = LocalizedStringKeys.EndOfGame.ToTextProvider(),
                },
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
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[]
                    {
                        new ShortcutKeys(ConsoleKey.UpArrow),
                        new ShortcutKeys(KeyModifiers.Control, ConsoleKey.UpArrow),
                    },
                    IsFirstInGroup = true,
                    MenuTextProvider = LocalizedStringKeys.PreviousLine.ToTextProvider(),
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
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[]
                    {
                        new ShortcutKeys(ConsoleKey.DownArrow),
                        new ShortcutKeys(KeyModifiers.Control, ConsoleKey.DownArrow),
                    },
                    MenuTextProvider = LocalizedStringKeys.NextLine.ToTextProvider(),
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
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[]
                    {
                        new ShortcutKeys(ConsoleKey.P),
                        new ShortcutKeys(KeyModifiers.Control, ConsoleKey.P),
                    },
                    IsFirstInGroup = true,
                    MenuTextProvider = LocalizedStringKeys.PromoteLine.ToTextProvider(),
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
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[]
                    {
                        new ShortcutKeys(ConsoleKey.D),
                        new ShortcutKeys(KeyModifiers.Control, ConsoleKey.D),
                    },
                    MenuTextProvider = LocalizedStringKeys.DemoteLine.ToTextProvider(),
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
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(ConsoleKey.B), },
                    MenuTextProvider = LocalizedStringKeys.BreakAtCurrentPosition.ToTextProvider(),
                },
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
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.Delete), },
                    MenuTextProvider = LocalizedStringKeys.DeleteLine.ToTextProvider(),
                },
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

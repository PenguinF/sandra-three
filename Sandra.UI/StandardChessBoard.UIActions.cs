#region License
/*********************************************************************************
 * StandardChessBoard.UIActions.cs
 *
 * Copyright (c) 2004-2021 Henk Nicolai
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
using Eutherion.Win.MdiAppTemplate;
using Sandra.Chess;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Sandra.UI
{
    public partial class StandardChessBoard
    {
        public const string StandardChessBoardUIActionPrefix = nameof(StandardChessBoard) + ".";

        public static readonly DefaultUIActionBinding FlipBoard = new DefaultUIActionBinding(
            new UIAction(StandardChessBoardUIActionPrefix + nameof(FlipBoard)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.F), },
                    IsFirstInGroup = true,
                    MenuTextProvider = LocalizedStringKeys.FlipBoard.ToTextProvider(),
                    MenuIcon = Properties.Resources.flip.ToImageProvider(),
                },
            });

        public UIActionState TryFlipBoard(bool perform)
        {
            if (perform) IsBoardFlipped = !IsBoardFlipped;
            return new UIActionState(UIActionVisibility.Enabled, IsBoardFlipped);
        }

        public static readonly DefaultUIActionBinding TakeScreenshot = new DefaultUIActionBinding(
            new UIAction(StandardChessBoardUIActionPrefix + nameof(TakeScreenshot)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.C), },
                    IsFirstInGroup = true,
                    MenuTextProvider = LocalizedStringKeys.CopyDiagramToClipboard.ToTextProvider(),
                },
            });

        public UIActionState TryTakeScreenshot(bool perform)
        {
            if (perform)
            {
                Rectangle bounds = PlayingBoard.Bounds;
                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    PlayingBoard.DrawToBitmap(bitmap, new Rectangle(0, 0, bounds.Width, bounds.Height));
                    Clipboard.SetImage(bitmap);
                }
            }
            return UIActionVisibility.Enabled;
        }

        public UIActionState TryZoomIn(bool perform)
        {
            if (perform) PerformAutoFit(PlayingBoard.SquareSize + 1);
            return UIActionVisibility.Enabled;
        }

        public UIActionState TryZoomOut(bool perform)
        {
            if (PlayingBoard.SquareSize <= 1) return UIActionVisibility.Disabled;
            if (perform) PerformAutoFit(PlayingBoard.SquareSize - 1);
            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding GotoStart = new DefaultUIActionBinding(
            new UIAction(StandardChessBoardUIActionPrefix + nameof(GotoStart)),
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
            if (game.IsFirstMove) return UIActionVisibility.Disabled;
            if (perform)
            {
                do game.Backward(); while (!game.IsFirstMove);
                GameUpdated();
            }
            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding GotoFirstMove = new DefaultUIActionBinding(
            new UIAction(StandardChessBoardUIActionPrefix + nameof(GotoFirstMove)),
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
            if (game.IsFirstMove) return UIActionVisibility.Disabled;
            if (perform)
            {
                // Go to the first move in this line, but make sure to not get stuck with repeated use.
                game.Backward();
                while (game.ActiveTree.ParentVariation != null
                    && game.ActiveTree.ParentVariation.VariationIndex == 0)
                {
                    game.Backward();
                }
                GameUpdated();
            }
            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding FastNavigateBackward = new DefaultUIActionBinding(
            new UIAction(StandardChessBoardUIActionPrefix + nameof(FastNavigateBackward)),
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
            if (game.IsFirstMove) return UIActionVisibility.Disabled;
            if (perform)
            {
                Session.Current.GetSetting(SettingKeys.FastNavigationPlyCount).Times(game.Backward);
                GameUpdated();
            }
            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding GotoPreviousMove = new DefaultUIActionBinding(
            new UIAction(StandardChessBoardUIActionPrefix + nameof(GotoPreviousMove)),
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
            if (game.IsFirstMove) return UIActionVisibility.Disabled;
            if (perform)
            {
                game.Backward();
                GameUpdated();
            }
            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding GotoNextMove = new DefaultUIActionBinding(
            new UIAction(StandardChessBoardUIActionPrefix + nameof(GotoNextMove)),
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
            if (game.ActiveTree.MainLine == null && !game.ActiveTree.SideLines.Any())
            {
                return UIActionVisibility.Disabled;
            }

            if (perform)
            {
                if (game.ActiveTree.MainLine != null)
                {
                    game.Forward();
                }
                else
                {
                    game.SetActiveTree(game.ActiveTree.SideLines.First().MoveTree);
                }
                GameUpdated();
            }
            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding FastNavigateForward = new DefaultUIActionBinding(
            new UIAction(StandardChessBoardUIActionPrefix + nameof(FastNavigateForward)),
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
            if (game.IsLastMove) return UIActionVisibility.Disabled;
            if (perform)
            {
                Session.Current.GetSetting(SettingKeys.FastNavigationPlyCount).Times(game.Forward);
                GameUpdated();
            }
            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding GotoLastMove = new DefaultUIActionBinding(
            new UIAction(StandardChessBoardUIActionPrefix + nameof(GotoLastMove)),
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
            if (game.IsLastMove) return UIActionVisibility.Disabled;
            if (perform)
            {
                do game.Forward(); while (!game.IsLastMove);
                GameUpdated();
            }
            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding GotoEnd = new DefaultUIActionBinding(
            new UIAction(StandardChessBoardUIActionPrefix + nameof(GotoEnd)),
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
            if (game.IsLastMove && GetFirstMove(game.ActiveTree.ParentVariation) == null)
            {
                // Last move in the main line of the game.
                return UIActionVisibility.Disabled;
            }

            if (perform)
            {
                game.SetActiveTree(game.MoveTree);
                while (!game.IsLastMove) game.Forward();
                GameUpdated();
            }
            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding GotoPreviousVariation = new DefaultUIActionBinding(
            new UIAction(StandardChessBoardUIActionPrefix + nameof(GotoPreviousVariation)),
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
            Variation currentVariation = game.ActiveTree.ParentVariation;
            if (currentVariation != null && currentVariation.VariationIndex > 0)
            {
                Variation previousVariation = currentVariation.ParentTree.Variations[currentVariation.VariationIndex - 1];
                if (previousVariation != null)
                {
                    if (perform)
                    {
                        game.SetActiveTree(previousVariation.MoveTree);
                        GameUpdated();
                    }
                    return UIActionVisibility.Enabled;
                }
            }
            return UIActionVisibility.Disabled;
        }

        public static readonly DefaultUIActionBinding GotoNextVariation = new DefaultUIActionBinding(
            new UIAction(StandardChessBoardUIActionPrefix + nameof(GotoNextVariation)),
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
            Variation currentVariation = game.ActiveTree.ParentVariation;
            if (currentVariation != null && currentVariation.VariationIndex + 1 < currentVariation.ParentTree.Variations.Count)
            {
                if (perform)
                {
                    Variation nextVariation = currentVariation.ParentTree.Variations[currentVariation.VariationIndex + 1];
                    game.SetActiveTree(nextVariation.MoveTree);
                    GameUpdated();
                }
                return UIActionVisibility.Enabled;
            }
            return UIActionVisibility.Disabled;
        }

        public static readonly DefaultUIActionBinding PromoteActiveVariation = new DefaultUIActionBinding(
            new UIAction(StandardChessBoardUIActionPrefix + nameof(PromoteActiveVariation)),
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
            Variation firstMoveInVariation = GetFirstMove(game.ActiveTree.ParentVariation);

            if (firstMoveInVariation == null)
            {
                // Already the main line of the game.
                return UIActionVisibility.Disabled;
            }

            if (perform)
            {
                firstMoveInVariation.RepositionBefore(firstMoveInVariation.VariationIndex - 1);
                GameUpdated();
            }
            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding DemoteActiveVariation = new DefaultUIActionBinding(
            new UIAction(StandardChessBoardUIActionPrefix + nameof(DemoteActiveVariation)),
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
            Variation moveWithSideLine = game.ActiveTree.ParentVariation;
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
                GameUpdated();
            }
            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding BreakActiveVariation = new DefaultUIActionBinding(
            new UIAction(StandardChessBoardUIActionPrefix + nameof(BreakActiveVariation)),
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
            if (game.IsLastMove) return UIActionVisibility.Disabled;
            if (perform)
            {
                game.ActiveTree.Break();
                GameUpdated();
            }
            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding DeleteActiveVariation = new DefaultUIActionBinding(
            new UIAction(StandardChessBoardUIActionPrefix + nameof(DeleteActiveVariation)),
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
            if (game.IsFirstMove) return UIActionVisibility.Disabled;
            if (perform)
            {
                // Go backward, then remove the move which was just active and its move tree.
                Variation variationToRemove = game.ActiveTree.ParentVariation;
                game.Backward();
                game.ActiveTree.RemoveVariation(variationToRemove);
                GameUpdated();
            }
            return UIActionVisibility.Enabled;
        }
    }
}

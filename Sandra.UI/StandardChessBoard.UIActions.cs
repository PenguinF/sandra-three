#region License
/*********************************************************************************
 * StandardChessBoard.UIActions.cs
 *
 * Copyright (c) 2004-2023 Henk Nicolai
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
using Eutherion.Collections;
using Eutherion.UIActions;
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

        public static readonly UIAction FlipBoard = new UIAction(
            new StringKey<UIAction>(StandardChessBoardUIActionPrefix + nameof(FlipBoard)),
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

        public static readonly UIAction TakeScreenshot = new UIAction(
            new StringKey<UIAction>(StandardChessBoardUIActionPrefix + nameof(TakeScreenshot)),
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

        public static readonly UIAction GotoFirstMove = new UIAction(
            new StringKey<UIAction>(StandardChessBoardUIActionPrefix + nameof(GotoFirstMove)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    IsFirstInGroup = true,
                    Shortcuts = new[]
                    {
                        new ShortcutKeys(ConsoleKey.Home),
                        new ShortcutKeys(KeyModifiers.Control, ConsoleKey.Home),
                    },
                    MenuTextProvider = LocalizedStringKeys.FirstMove.ToTextProvider(),
                },
            });

        public UIActionState TryGotoFirstMove(bool perform)
        {
            if (game.IsFirstMove) return UIActionVisibility.Disabled;
            if (perform)
            {
                do game.Backward(); while (!game.IsFirstMove);
                GameUpdated();
            }
            return UIActionVisibility.Enabled;
        }

        public static readonly UIAction FastNavigateBackward = new UIAction(
            new StringKey<UIAction>(StandardChessBoardUIActionPrefix + nameof(FastNavigateBackward)),
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

        public static readonly UIAction GotoPreviousMove = new UIAction(
            new StringKey<UIAction>(StandardChessBoardUIActionPrefix + nameof(GotoPreviousMove)),
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

        public static readonly UIAction GotoNextMove = new UIAction(
            new StringKey<UIAction>(StandardChessBoardUIActionPrefix + nameof(GotoNextMove)),
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
            if (game.IsLastMove) return UIActionVisibility.Disabled;
            if (perform)
            {
                game.Forward();
                GameUpdated();
            }
            return UIActionVisibility.Enabled;
        }

        public static readonly UIAction FastNavigateForward = new UIAction(
            new StringKey<UIAction>(StandardChessBoardUIActionPrefix + nameof(FastNavigateForward)),
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

        public static readonly UIAction GotoLastMove = new UIAction(
            new StringKey<UIAction>(StandardChessBoardUIActionPrefix + nameof(GotoLastMove)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[]
                    {
                        new ShortcutKeys(ConsoleKey.End),
                        new ShortcutKeys(KeyModifiers.Control, ConsoleKey.End),
                    },
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

        public static readonly UIAction GotoPreviousVariation = new UIAction(
            new StringKey<UIAction>(StandardChessBoardUIActionPrefix + nameof(GotoPreviousVariation)),
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
            return UIActionVisibility.Disabled;
        }

        public static readonly UIAction GotoNextVariation = new UIAction(
            new StringKey<UIAction>(StandardChessBoardUIActionPrefix + nameof(GotoNextVariation)),
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
            return UIActionVisibility.Disabled;
        }

        public static readonly UIAction PromoteActiveVariation = new UIAction(
            new StringKey<UIAction>(StandardChessBoardUIActionPrefix + nameof(PromoteActiveVariation)),
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
            // Disable until we can modify PGN using its syntax tree.
            return UIActionVisibility.Disabled;
        }

        public static readonly UIAction DemoteActiveVariation = new UIAction(
            new StringKey<UIAction>(StandardChessBoardUIActionPrefix + nameof(DemoteActiveVariation)),
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
            // Disable until we can modify PGN using its syntax tree.
            return UIActionVisibility.Disabled;
        }

        public static readonly UIAction DeleteActiveVariation = new UIAction(
            new StringKey<UIAction>(StandardChessBoardUIActionPrefix + nameof(DeleteActiveVariation)),
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
            // Disable until we can modify PGN using its syntax tree.
            return UIActionVisibility.Disabled;
        }
    }
}

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

using Eutherion.UIActions;
using Eutherion.Utils;
using Eutherion.Win;
using Eutherion.Win.MdiAppTemplate;
using Eutherion.Win.UIActions;
using System;
using System.Drawing;

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
                if (chessBoard == null)
                {
                    var newChessBoard = new StandardChessBoard
                    {
                        Game = Game,
                        PieceImages = PieceImages.ImageArray
                    };

                    newChessBoard.PlayingBoard.ForegroundImageRelativeSize = 0.9f;

                    newChessBoard.PlayingBoard.BindActions(new UIActionBindings
                    {
                        { GotoChessBoardForm, TryGotoChessBoardForm },

                        { StandardChessBoard.GotoStart, newChessBoard.TryGotoStart },
                        { StandardChessBoard.GotoFirstMove, newChessBoard.TryGotoFirstMove },
                        { StandardChessBoard.FastNavigateBackward, newChessBoard.TryFastNavigateBackward },
                        { StandardChessBoard.GotoPreviousMove, newChessBoard.TryGotoPreviousMove },
                        { StandardChessBoard.GotoNextMove, newChessBoard.TryGotoNextMove },
                        { StandardChessBoard.FastNavigateForward, newChessBoard.TryFastNavigateForward },
                        { StandardChessBoard.GotoLastMove, newChessBoard.TryGotoLastMove },
                        { StandardChessBoard.GotoEnd, newChessBoard.TryGotoEnd },

                        { StandardChessBoard.GotoPreviousVariation, newChessBoard.TryGotoPreviousVariation },
                        { StandardChessBoard.GotoNextVariation, newChessBoard.TryGotoNextVariation },

                        { StandardChessBoard.PromoteActiveVariation, newChessBoard.TryPromoteActiveVariation },
                        { StandardChessBoard.DemoteActiveVariation, newChessBoard.TryDemoteActiveVariation },
                        { StandardChessBoard.BreakActiveVariation, newChessBoard.TryBreakActiveVariation },
                        { StandardChessBoard.DeleteActiveVariation, newChessBoard.TryDeleteActiveVariation },

                        { StandardChessBoard.FlipBoard, newChessBoard.TryFlipBoard },
                        { StandardChessBoard.TakeScreenshot, newChessBoard.TryTakeScreenshot },

                        { SharedUIAction.ZoomIn, newChessBoard.TryZoomIn },
                        { SharedUIAction.ZoomOut, newChessBoard.TryZoomOut },
                    });

                    UIMenu.AddTo(newChessBoard.PlayingBoard);

                    newChessBoard.DockProperties.CaptionHeight = 24;
                    newChessBoard.DockProperties.Icon = Session.Current.ApplicationIcon;

                    var newChessBoardForm = new MenuCaptionBarForm<StandardChessBoard>(newChessBoard)
                    {
                        Owner = OwnerPgnEditor.FindForm(),
                        MaximizeBox = false,
                        ClientSize = new Size(400, 400),
                    };

                    StandardChessBoard.ConstrainClientSize(newChessBoardForm);

                    chessBoard = newChessBoard;
                    chessBoard.Disposed += (_, __) => chessBoard = null;
                }

                chessBoard.EnsureActivated();
            }

            return UIActionVisibility.Enabled;
        }
    }
}

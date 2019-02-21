#region License
/*********************************************************************************
 * StandardChessBoardForm.UIActions.cs
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

using Eutherion.UIActions;
using Eutherion.Win.UIActions;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sandra.UI
{
    public partial class StandardChessBoardForm
    {
        public const string StandardChessBoardFormUIActionPrefix = nameof(StandardChessBoardForm) + ".";

        public static readonly DefaultUIActionBinding FlipBoard = new DefaultUIActionBinding(
            new UIAction(StandardChessBoardFormUIActionPrefix + nameof(FlipBoard)),
            new UIActionBinding
            {
                ShortcutKeysInterface = new ShortcutKeysUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.F), },
                },
                ContextMenuInterface = new ContextMenuUIActionInterface
                {
                    IsFirstInGroup = true,
                    MenuCaptionKey = LocalizedStringKeys.FlipBoard,
                    MenuIcon = Properties.Resources.flip,
                },
            });

        public UIActionState TryFlipBoard(bool perform)
        {
            if (perform) IsBoardFlipped = !IsBoardFlipped;
            return new UIActionState(UIActionVisibility.Enabled, IsBoardFlipped);
        }

        public static readonly DefaultUIActionBinding TakeScreenshot = new DefaultUIActionBinding(
            new UIAction(StandardChessBoardFormUIActionPrefix + nameof(TakeScreenshot)),
            new UIActionBinding
            {
                ShortcutKeysInterface = new ShortcutKeysUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.C), },
                },
                ContextMenuInterface = new ContextMenuUIActionInterface
                {
                    IsFirstInGroup = true,
                    MenuCaptionKey = LocalizedStringKeys.CopyDiagramToClipboard,
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
    }
}

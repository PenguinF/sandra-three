﻿/*********************************************************************************
 * StandardChessBoardForm.UIActions.cs
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
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    public partial class StandardChessBoardForm
    {
        public const string StandardChessBoardFormUIActionPrefix = nameof(StandardChessBoardForm) + ".";

        public static readonly DefaultUIActionBinding TakeScreenshot = new DefaultUIActionBinding(
            new UIAction(StandardChessBoardFormUIActionPrefix + nameof(TakeScreenshot)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                IsFirstInGroup = true,
                MenuCaption = "Copy diagram to clipboard",
                Shortcuts = new ShortcutKeys[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.C), },
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
    }
}

/*********************************************************************************
 * MdiContainerForm.UIActions.cs
 * 
 * Copyright (c) 2004-2018 Henk Nicolai
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

namespace Sandra.UI.WF
{
    /// <summary>
    /// Main MdiContainer Form.
    /// </summary>
    public partial class MdiContainerForm
    {
        public const string MdiContainerFormUIActionPrefix = nameof(MdiContainerForm) + ".";

        public static readonly DefaultUIActionBinding OpenNewPlayingBoard = new DefaultUIActionBinding(
            new UIAction(MdiContainerFormUIActionPrefix + nameof(OpenNewPlayingBoard)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.NewGame,
                Shortcuts = new ShortcutKeys[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.N), },
            });

        public UIActionState TryOpenNewPlayingBoard(bool perform)
        {
            if (perform) NewPlayingBoard();
            return UIActionVisibility.Enabled;
        }
    }
}

#region License
/*********************************************************************************
 * SharedUIAction.cs
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
#endregion

using System;

namespace Sandra.UI.WF
{
    internal static class SharedUIAction
    {
        public const string SharedUIActionPrefix = nameof(SharedUIAction) + ".";

        public static readonly DefaultUIActionBinding ZoomIn = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(ZoomIn)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                IsFirstInGroup = true,
                MenuCaptionKey = LocalizedStringKeys.ZoomIn,
                Shortcuts = new ShortcutKeys[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.Add), },
                MenuIcon = Properties.Resources.zoom_in,
            });

        public static readonly DefaultUIActionBinding ZoomOut = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(ZoomOut)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.ZoomOut,
                Shortcuts = new ShortcutKeys[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.Subtract), },
                MenuIcon = Properties.Resources.zoom_out,
            });
    }
}

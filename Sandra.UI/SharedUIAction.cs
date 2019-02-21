#region License
/*********************************************************************************
 * SharedUIAction.cs
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

namespace Sandra.UI
{
    internal static class SharedUIAction
    {
        public const string SharedUIActionPrefix = nameof(SharedUIAction) + ".";

        public static readonly DefaultUIActionBinding Undo = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(Undo)),
            new UIActionBinding
            {
                ShowInMenu = true,
                IsFirstInGroup = true,
                MenuCaptionKey = LocalizedStringKeys.Undo,
                Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.Z), },
                MenuIcon = Properties.Resources.undo,
            });

        public static readonly DefaultUIActionBinding Redo = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(Redo)),
            new UIActionBinding
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.Redo,
                Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.Y), },
                MenuIcon = Properties.Resources.redo,
            });

        public static readonly DefaultUIActionBinding ZoomIn = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(ZoomIn)),
            new UIActionBinding
            {
                ShowInMenu = true,
                IsFirstInGroup = true,
                MenuCaptionKey = LocalizedStringKeys.ZoomIn,
                Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.Add), },
                MenuIcon = Properties.Resources.zoom_in,
            });

        public static readonly DefaultUIActionBinding ZoomOut = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(ZoomOut)),
            new UIActionBinding
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.ZoomOut,
                Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.Subtract), },
                MenuIcon = Properties.Resources.zoom_out,
            });

        public static readonly DefaultUIActionBinding CutSelectionToClipBoard = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(CutSelectionToClipBoard)),
            new UIActionBinding
            {
                ShowInMenu = true,
                IsFirstInGroup = true,
                MenuCaptionKey = LocalizedStringKeys.Cut,
                Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.X), },
                MenuIcon = Properties.Resources.cut,
            });

        public static readonly DefaultUIActionBinding CopySelectionToClipBoard = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(CopySelectionToClipBoard)),
            new UIActionBinding
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.Copy,
                Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.C), },
                MenuIcon = Properties.Resources.copy,
            });

        public static readonly DefaultUIActionBinding PasteSelectionFromClipBoard = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(PasteSelectionFromClipBoard)),
            new UIActionBinding
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.Paste,
                Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.V), },
                MenuIcon = Properties.Resources.paste,
            });

        public static readonly DefaultUIActionBinding SelectAllText = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(SelectAllText)),
            new UIActionBinding
            {
                ShowInMenu = true,
                IsFirstInGroup = true,
                MenuCaptionKey = LocalizedStringKeys.SelectAll,
                Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.A), },
            });

        public static readonly DefaultUIActionBinding GoToPreviousLocation = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(GoToPreviousLocation)),
            new UIActionBinding
            {
                ShowInMenu = true,
                IsFirstInGroup = true,
                MenuCaptionKey = LocalizedStringKeys.GoToPreviousLocation,
                Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Shift, ConsoleKey.F8), },
            });

        public static readonly DefaultUIActionBinding GoToNextLocation = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(GoToNextLocation)),
            new UIActionBinding
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.GoToNextLocation,
                Shortcuts = new[] { new ShortcutKeys(ConsoleKey.F8), },
            });
    }
}

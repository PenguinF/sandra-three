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
using Eutherion.Utils;
using System;

namespace Eutherion.Win.AppTemplate
{
    internal static class SharedUIAction
    {
        public const string SharedUIActionPrefix = nameof(SharedUIAction) + ".";

        public static readonly DefaultUIActionBinding Exit = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(Exit)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Alt, ConsoleKey.F4), },
                    IsFirstInGroup = true,
                    MenuCaptionKey = SharedLocalizedStringKeys.Exit,
                },
            });

        public static readonly DefaultUIActionBinding Close = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(Close)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control | KeyModifiers.Alt, ConsoleKey.F4), },
                    IsFirstInGroup = true,
                    MenuCaptionKey = SharedLocalizedStringKeys.Close,
                },
            });

        public static readonly DefaultUIActionBinding Undo = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(Undo)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.Z), },
                    IsFirstInGroup = true,
                    MenuCaptionKey = SharedLocalizedStringKeys.Undo,
                    MenuIcon = SharedResources.undo.ToImageProvider(),
                },
            });

        public static readonly DefaultUIActionBinding Redo = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(Redo)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.Y), },
                    MenuCaptionKey = SharedLocalizedStringKeys.Redo,
                    MenuIcon = SharedResources.redo.ToImageProvider(),
                },
            });

        public static readonly DefaultUIActionBinding ZoomIn = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(ZoomIn)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.Add), },
                    IsFirstInGroup = true,
                    MenuCaptionKey = SharedLocalizedStringKeys.ZoomIn,
                    MenuIcon = SharedResources.zoom_in.ToImageProvider(),
                },
            });

        public static readonly DefaultUIActionBinding ZoomOut = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(ZoomOut)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.Subtract), },
                    MenuCaptionKey = SharedLocalizedStringKeys.ZoomOut,
                    MenuIcon = SharedResources.zoom_out.ToImageProvider(),
                },
            });

        public static readonly DefaultUIActionBinding CutSelectionToClipBoard = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(CutSelectionToClipBoard)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.X), },
                    IsFirstInGroup = true,
                    MenuCaptionKey = SharedLocalizedStringKeys.Cut,
                    MenuIcon = SharedResources.cut.ToImageProvider(),
                },
            });

        public static readonly DefaultUIActionBinding CopySelectionToClipBoard = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(CopySelectionToClipBoard)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.C), },
                    MenuCaptionKey = SharedLocalizedStringKeys.Copy,
                    MenuIcon = SharedResources.copy.ToImageProvider(),
                },
            });

        public static readonly DefaultUIActionBinding PasteSelectionFromClipBoard = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(PasteSelectionFromClipBoard)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.V), },
                    MenuCaptionKey = SharedLocalizedStringKeys.Paste,
                    MenuIcon = SharedResources.paste.ToImageProvider(),
                },
            });

        public static readonly DefaultUIActionBinding SelectAllText = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(SelectAllText)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.A), },
                    IsFirstInGroup = true,
                    MenuCaptionKey = SharedLocalizedStringKeys.SelectAll,
                },
            });

        public static readonly DefaultUIActionBinding SaveToFile = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(SaveToFile)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.S), },
                    IsFirstInGroup = true,
                    MenuCaptionKey = SharedLocalizedStringKeys.Save,
                    MenuIcon = SharedResources.save.ToImageProvider(),
                },
            });

        public static readonly DefaultUIActionBinding GoToPreviousLocation = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(GoToPreviousLocation)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Shift, ConsoleKey.F8), },
                    IsFirstInGroup = true,
                    MenuCaptionKey = SharedLocalizedStringKeys.GoToPreviousLocation,
                },
            });

        public static readonly DefaultUIActionBinding GoToNextLocation = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(GoToNextLocation)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(ConsoleKey.F8), },
                    MenuCaptionKey = SharedLocalizedStringKeys.GoToNextLocation,
                },
            });
    }
}

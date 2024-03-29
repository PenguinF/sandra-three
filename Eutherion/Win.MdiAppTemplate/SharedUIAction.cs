﻿#region License
/*********************************************************************************
 * SharedUIAction.cs
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

using Eutherion.Collections;
using Eutherion.UIActions;
using System;

namespace Eutherion.Win.MdiAppTemplate
{
    public static class SharedUIAction
    {
        public const string SharedUIActionPrefix = nameof(SharedUIAction) + ".";

        public static readonly UIAction Exit = new UIAction(
            new StringKey<UIAction>(SharedUIActionPrefix + nameof(Exit)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Alt, ConsoleKey.F4), },
                    IsFirstInGroup = true,
                    MenuTextProvider = SharedLocalizedStringKeys.Exit.ToTextProvider(),
                },
            });

        public static readonly UIAction WindowMenuRestore = new UIAction(
            new StringKey<UIAction>(SharedUIActionPrefix + nameof(WindowMenuRestore)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    IsFirstInGroup = true,
                    MenuTextProvider = SharedLocalizedStringKeys.WindowRestore .ToTextProvider(),
                },
            });

        public static readonly UIAction WindowMenuMove = new UIAction(
            new StringKey<UIAction>(SharedUIActionPrefix + nameof(WindowMenuMove)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    MenuTextProvider = SharedLocalizedStringKeys.WindowMove.ToTextProvider(),
                },
            });

        public static readonly UIAction WindowMenuSize = new UIAction(
            new StringKey<UIAction>(SharedUIActionPrefix + nameof(WindowMenuSize)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    MenuTextProvider = SharedLocalizedStringKeys.WindowSize.ToTextProvider(),
                },
            });

        public static readonly UIAction WindowMenuMinimize = new UIAction(
            new StringKey<UIAction>(SharedUIActionPrefix + nameof(WindowMenuMinimize)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    MenuTextProvider = SharedLocalizedStringKeys.WindowMinimize.ToTextProvider(),
                },
            });

        public static readonly UIAction WindowMenuMaximize = new UIAction(
            new StringKey<UIAction>(SharedUIActionPrefix + nameof(WindowMenuMaximize)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    MenuTextProvider = SharedLocalizedStringKeys.WindowMaximize.ToTextProvider(),
                },
            });

        public static readonly UIAction Close = new UIAction(
            new StringKey<UIAction>(SharedUIActionPrefix + nameof(Close)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Alt, ConsoleKey.F4), },
                    IsFirstInGroup = true,
                    MenuTextProvider = SharedLocalizedStringKeys.Close.ToTextProvider(),
                },
            });

        public static readonly UIAction Undo = new UIAction(
            new StringKey<UIAction>(SharedUIActionPrefix + nameof(Undo)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.Z), },
                    IsFirstInGroup = true,
                    MenuTextProvider = SharedLocalizedStringKeys.Undo.ToTextProvider(),
                    MenuIcon = SharedResources.undo.ToImageProvider(),
                },
            });

        public static readonly UIAction Redo = new UIAction(
            new StringKey<UIAction>(SharedUIActionPrefix + nameof(Redo)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.Y), },
                    MenuTextProvider = SharedLocalizedStringKeys.Redo.ToTextProvider(),
                    MenuIcon = SharedResources.redo.ToImageProvider(),
                },
            });

        public static readonly UIAction ZoomIn = new UIAction(
            new StringKey<UIAction>(SharedUIActionPrefix + nameof(ZoomIn)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.Add), },
                    IsFirstInGroup = true,
                    MenuTextProvider = SharedLocalizedStringKeys.ZoomIn.ToTextProvider(),
                    MenuIcon = SharedResources.zoom_in.ToImageProvider(),
                },
            });

        public static readonly UIAction ZoomOut = new UIAction(
            new StringKey<UIAction>(SharedUIActionPrefix + nameof(ZoomOut)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.Subtract), },
                    MenuTextProvider = SharedLocalizedStringKeys.ZoomOut.ToTextProvider(),
                    MenuIcon = SharedResources.zoom_out.ToImageProvider(),
                },
            });

        public static readonly UIAction CutSelectionToClipBoard = new UIAction(
            new StringKey<UIAction>(SharedUIActionPrefix + nameof(CutSelectionToClipBoard)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.X), },
                    IsFirstInGroup = true,
                    MenuTextProvider = SharedLocalizedStringKeys.Cut.ToTextProvider(),
                    MenuIcon = SharedResources.cut.ToImageProvider(),
                },
            });

        public static readonly UIAction CopySelectionToClipBoard = new UIAction(
            new StringKey<UIAction>(SharedUIActionPrefix + nameof(CopySelectionToClipBoard)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.C), },
                    MenuTextProvider = SharedLocalizedStringKeys.Copy.ToTextProvider(),
                    MenuIcon = SharedResources.copy.ToImageProvider(),
                },
            });

        public static readonly UIAction PasteSelectionFromClipBoard = new UIAction(
            new StringKey<UIAction>(SharedUIActionPrefix + nameof(PasteSelectionFromClipBoard)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.V), },
                    MenuTextProvider = SharedLocalizedStringKeys.Paste.ToTextProvider(),
                    MenuIcon = SharedResources.paste.ToImageProvider(),
                },
            });

        public static readonly UIAction SelectAllText = new UIAction(
            new StringKey<UIAction>(SharedUIActionPrefix + nameof(SelectAllText)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.A), },
                    IsFirstInGroup = true,
                    MenuTextProvider = SharedLocalizedStringKeys.SelectAll.ToTextProvider(),
                },
            });

        public static readonly UIAction SaveToFile = new UIAction(
            new StringKey<UIAction>(SharedUIActionPrefix + nameof(SaveToFile)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.S), },
                    IsFirstInGroup = true,
                    MenuTextProvider = SharedLocalizedStringKeys.Save.ToTextProvider(),
                    MenuIcon = SharedResources.save.ToImageProvider(),
                },
            });

        public static readonly UIAction SaveAs = new UIAction(
            new StringKey<UIAction>(SharedUIActionPrefix + nameof(SaveAs)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[]
                    {
                        new ShortcutKeys(KeyModifiers.Control | KeyModifiers.Shift, ConsoleKey.S),
                        new ShortcutKeys(ConsoleKey.F12),
                    },
                    MenuTextProvider = SharedLocalizedStringKeys.SaveAs.ToTextProvider(),
                    OpensDialog = true,
                },
            });

        public static readonly UIAction ShowErrorPane = new UIAction(
            new StringKey<UIAction>(SharedUIActionPrefix + nameof(ShowErrorPane)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    IsFirstInGroup = true,
                    MenuTextProvider = SharedLocalizedStringKeys.ShowErrorPane.ToTextProvider()
                },
            });

        public static readonly UIAction GoToPreviousError = new UIAction(
            new StringKey<UIAction>(SharedUIActionPrefix + nameof(GoToPreviousError)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Shift, ConsoleKey.F8), },
                    MenuTextProvider = SharedLocalizedStringKeys.GoToPreviousError.ToTextProvider(),
                },
            });

        public static readonly UIAction GoToNextError = new UIAction(
            new StringKey<UIAction>(SharedUIActionPrefix + nameof(GoToNextError)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(ConsoleKey.F8), },
                    MenuTextProvider = SharedLocalizedStringKeys.GoToNextError.ToTextProvider(),
                },
            });
    }
}

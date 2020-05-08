#region License
/*********************************************************************************
 * SharedUIAction.cs
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
                    MenuTextProvider = SharedLocalizedStringKeys.Exit.ToTextProvider(),
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
                    MenuTextProvider = SharedLocalizedStringKeys.Close.ToTextProvider(),
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
                    MenuTextProvider = SharedLocalizedStringKeys.Undo.ToTextProvider(),
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
                    MenuTextProvider = SharedLocalizedStringKeys.Redo.ToTextProvider(),
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
                    MenuTextProvider = SharedLocalizedStringKeys.ZoomIn.ToTextProvider(),
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
                    MenuTextProvider = SharedLocalizedStringKeys.ZoomOut.ToTextProvider(),
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
                    MenuTextProvider = SharedLocalizedStringKeys.Cut.ToTextProvider(),
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
                    MenuTextProvider = SharedLocalizedStringKeys.Copy.ToTextProvider(),
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
                    MenuTextProvider = SharedLocalizedStringKeys.Paste.ToTextProvider(),
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
                    MenuTextProvider = SharedLocalizedStringKeys.SelectAll.ToTextProvider(),
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
                    MenuTextProvider = SharedLocalizedStringKeys.Save.ToTextProvider(),
                    MenuIcon = SharedResources.save.ToImageProvider(),
                },
            });

        public static readonly DefaultUIActionBinding SaveAs = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(SaveAs)),
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

        public static readonly DefaultUIActionBinding ShowErrorPane = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(ShowErrorPane)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    IsFirstInGroup = true,
                    MenuTextProvider = SharedLocalizedStringKeys.ShowErrorPane.ToTextProvider()
                },
            });

        public static readonly DefaultUIActionBinding GoToPreviousError = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(GoToPreviousError)),
            new ImplementationSet<IUIActionInterface>
            {
                new CombinedUIActionInterface
                {
                    Shortcuts = new[] { new ShortcutKeys(KeyModifiers.Shift, ConsoleKey.F8), },
                    MenuTextProvider = SharedLocalizedStringKeys.GoToPreviousError.ToTextProvider(),
                },
            });

        public static readonly DefaultUIActionBinding GoToNextError = new DefaultUIActionBinding(
            new UIAction(SharedUIActionPrefix + nameof(GoToNextError)),
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

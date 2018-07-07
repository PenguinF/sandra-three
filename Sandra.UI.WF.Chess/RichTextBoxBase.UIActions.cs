#region License
/*********************************************************************************
 * RichTextBoxBase.UIActions.cs
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

using Sandra.UI.WF.Storage;
using System;
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    public partial class RichTextBoxBase
    {
        public const string RichTextBoxBaseUIActionPrefix = nameof(RichTextBoxBase) + ".";

        public static readonly DefaultUIActionBinding CutSelectionToClipBoard = new DefaultUIActionBinding(
            new UIAction(RichTextBoxBaseUIActionPrefix + nameof(CutSelectionToClipBoard)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                IsFirstInGroup = true,
                MenuCaptionKey = LocalizedStringKeys.Cut,
                Shortcuts = new ShortcutKeys[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.X), },
                MenuIcon = Properties.Resources.cut,
            });

        public UIActionState TryCutSelectionToClipBoard(bool perform)
        {
            if (ReadOnly) return UIActionVisibility.Hidden;
            if (SelectionLength == 0) return UIActionVisibility.Disabled;
            if (perform) Cut();
            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding CopySelectionToClipBoard = new DefaultUIActionBinding(
            new UIAction(RichTextBoxBaseUIActionPrefix + nameof(CopySelectionToClipBoard)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.Copy,
                Shortcuts = new ShortcutKeys[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.C), },
                MenuIcon = Properties.Resources.copy,
            });

        public UIActionState TryCopySelectionToClipBoard(bool perform)
        {
            if (SelectionLength == 0) return UIActionVisibility.Disabled;
            if (perform) Copy();
            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding PasteSelectionFromClipBoard = new DefaultUIActionBinding(
            new UIAction(RichTextBoxBaseUIActionPrefix + nameof(PasteSelectionFromClipBoard)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                MenuCaptionKey = LocalizedStringKeys.Paste,
                Shortcuts = new ShortcutKeys[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.V), },
                MenuIcon = Properties.Resources.paste,
            });

        public UIActionState TryPasteSelectionFromClipBoard(bool perform)
        {
            if (ReadOnly) return UIActionVisibility.Hidden;
            if (!Clipboard.ContainsText()) return UIActionVisibility.Disabled;
            if (perform) Paste();
            return UIActionVisibility.Enabled;
        }

        public static readonly DefaultUIActionBinding SelectAllText = new DefaultUIActionBinding(
            new UIAction(RichTextBoxBaseUIActionPrefix + nameof(SelectAllText)),
            new UIActionBinding()
            {
                ShowInMenu = true,
                IsFirstInGroup = true,
                MenuCaptionKey = LocalizedStringKeys.SelectAll,
                Shortcuts = new ShortcutKeys[] { new ShortcutKeys(KeyModifiers.Control, ConsoleKey.A), },
            });

        public UIActionState TrySelectAllText(bool perform)
        {
            if (TextLength == 0) return UIActionVisibility.Disabled;
            if (perform) SelectAll();
            return UIActionVisibility.Enabled;
        }

        public UIActionState TryZoomIn(bool perform)
        {
            int zoomFactor = PType.RichTextZoomFactor.ToDiscreteZoomFactor(ZoomFactor);
            if (zoomFactor >= PType.RichTextZoomFactor.MaxDiscreteValue) return UIActionVisibility.Disabled;
            if (perform)
            {
                zoomFactor++;
                ZoomFactor = PType.RichTextZoomFactor.FromDiscreteZoomFactor(zoomFactor);
                OnZoomFactorChanged(new ZoomFactorChangedEventArgs(zoomFactor));
            }
            return UIActionVisibility.Enabled;
        }

        public UIActionState TryZoomOut(bool perform)
        {
            int zoomFactor = PType.RichTextZoomFactor.ToDiscreteZoomFactor(ZoomFactor);
            if (zoomFactor <= PType.RichTextZoomFactor.MinDiscreteValue) return UIActionVisibility.Disabled;
            if (perform)
            {
                zoomFactor--;
                ZoomFactor = PType.RichTextZoomFactor.FromDiscreteZoomFactor(zoomFactor);
                OnZoomFactorChanged(new ZoomFactorChangedEventArgs(zoomFactor));
            }
            return UIActionVisibility.Enabled;
        }
    }
}

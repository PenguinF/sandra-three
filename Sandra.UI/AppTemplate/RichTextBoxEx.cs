#region License
/*********************************************************************************
 * RichTextBoxEx.cs
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
using Eutherion.Win.Controls;
using Eutherion.Win.UIActions;
using System;
using System.Windows.Forms;

namespace Eutherion.Win.AppTemplate
{
    /// <summary>
    /// Represents a Windows rich text box which exposes a number of <see cref="UIAction"/> hooks.
    /// </summary>
    public class RichTextBoxEx : UpdatableRichTextBox, IUIActionHandlerProvider
    {
        /// <summary>
        /// Gets the action handler for this control.
        /// </summary>
        public UIActionHandler ActionHandler { get; } = new UIActionHandler();

        /// <summary>
        /// Gets the regular cut/copy/paste/select-all UIActions for this textbox.
        /// </summary>
        public UIActionBindings StandardUIActionBindings => new UIActionBindings
        {
            { SharedUIAction.Undo, TryUndo },
            { SharedUIAction.Redo, TryRedo },

            { SharedUIAction.ZoomIn, TryZoomIn },
            { SharedUIAction.ZoomOut, TryZoomOut },

            { SharedUIAction.CutSelectionToClipBoard, TryCutSelectionToClipBoard },
            { SharedUIAction.CopySelectionToClipBoard, TryCopySelectionToClipBoard },
            { SharedUIAction.PasteSelectionFromClipBoard, TryPasteSelectionFromClipBoard },
            { SharedUIAction.SelectAllText, TrySelectAllText },
        };

        /// <summary>
        /// Binds the regular cut/copy/paste/select-all UIActions to this textbox.
        /// </summary>
        public void BindStandardEditUIActions()
            => this.BindActions(StandardUIActionBindings);

        public UIActionState TryUndo(bool perform)
        {
            if (ReadOnly) return UIActionVisibility.Hidden;
            if (!CanUndo) return UIActionVisibility.Disabled;
            if (perform) Undo();
            return UIActionVisibility.Enabled;
        }

        public UIActionState TryRedo(bool perform)
        {
            if (ReadOnly) return UIActionVisibility.Hidden;
            if (!CanRedo) return UIActionVisibility.Disabled;
            if (perform) Redo();
            return UIActionVisibility.Enabled;
        }

        public UIActionState TryCutSelectionToClipBoard(bool perform)
        {
            if (ReadOnly) return UIActionVisibility.Hidden;
            if (SelectionLength == 0) return UIActionVisibility.Disabled;
            if (perform) Cut();
            return UIActionVisibility.Enabled;
        }

        public UIActionState TryCopySelectionToClipBoard(bool perform)
        {
            if (SelectionLength == 0) return UIActionVisibility.Disabled;
            if (perform) Copy();
            return UIActionVisibility.Enabled;
        }

        public UIActionState TryPasteSelectionFromClipBoard(bool perform)
        {
            if (ReadOnly) return UIActionVisibility.Hidden;
            if (!Clipboard.ContainsText()) return UIActionVisibility.Disabled;
            if (perform) Paste();
            return UIActionVisibility.Enabled;
        }

        public UIActionState TrySelectAllText(bool perform)
        {
            if (TextLength == 0) return UIActionVisibility.Disabled;
            if (perform) SelectAll();
            return UIActionVisibility.Enabled;
        }

        public UIActionState TryZoomIn(bool perform)
        {
            int zoomFactor = RichTextZoomFactor.ToDiscreteZoomFactor(ZoomFactor);
            if (zoomFactor >= RichTextZoomFactor.MaxDiscreteValue) return UIActionVisibility.Disabled;
            if (perform)
            {
                zoomFactor++;
                ZoomFactor = RichTextZoomFactor.FromDiscreteZoomFactor(zoomFactor);
            }
            return UIActionVisibility.Enabled;
        }

        public UIActionState TryZoomOut(bool perform)
        {
            int zoomFactor = RichTextZoomFactor.ToDiscreteZoomFactor(ZoomFactor);
            if (zoomFactor <= RichTextZoomFactor.MinDiscreteValue) return UIActionVisibility.Disabled;
            if (perform)
            {
                zoomFactor--;
                ZoomFactor = RichTextZoomFactor.FromDiscreteZoomFactor(zoomFactor);
            }
            return UIActionVisibility.Enabled;
        }
    }

    public static class RichTextZoomFactor
    {
        /// <summary>
        /// Returns the minimum discrete integer value which when converted with
        /// <see cref="FromDiscreteZoomFactor"/> is still a valid value for the ZoomFactor property
        /// of a <see cref="RichTextBox"/> which is between 1/64 and 64, endpoints not included.
        /// </summary>
        public static readonly int MinDiscreteValue = -9;

        /// <summary>
        /// Returns the maximum discrete integer value which when converted with
        /// <see cref="FromDiscreteZoomFactor"/> is still a valid value for the ZoomFactor property
        /// of a <see cref="RichTextBox"/> which is between 1/64 and 64, endpoints not included.
        /// </summary>
        public static readonly int MaxDiscreteValue = 649;

        public static float FromDiscreteZoomFactor(int zoomFactor)
            => (zoomFactor + 10) / 10f;

        public static int ToDiscreteZoomFactor(float zoomFactor)
            // Assume discrete deltas of 0.1f.
            // Set 0 to be the default, so 1.0f should map to 0.
            => (int)Math.Round(zoomFactor * 10f) - 10;
    }
}

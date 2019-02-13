#region License
/*********************************************************************************
 * SyntaxEditor.UIActions.cs
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

using Eutherion.Win.UIActions;
using System.Windows.Forms;

namespace Sandra.UI
{
    public partial class SyntaxEditor<TTerminal>
    {
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
            if (SelectionStart == SelectionEnd) return UIActionVisibility.Disabled;
            if (perform) Cut();
            return UIActionVisibility.Enabled;
        }

        public UIActionState TryCopySelectionToClipBoard(bool perform)
        {
            if (SelectionStart == SelectionEnd) return UIActionVisibility.Disabled;
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
            int zoomFactor = Zoom;
            if (zoomFactor >= ScintillaZoomFactor.MaxDiscreteValue) return UIActionVisibility.Disabled;
            if (perform)
            {
                zoomFactor++;
                Zoom = zoomFactor;
                RaiseZoomFactorChanged(new ZoomFactorChangedEventArgs(zoomFactor));
            }
            return UIActionVisibility.Enabled;
        }

        public UIActionState TryZoomOut(bool perform)
        {
            int zoomFactor = Zoom;
            if (zoomFactor <= ScintillaZoomFactor.MinDiscreteValue) return UIActionVisibility.Disabled;
            if (perform)
            {
                zoomFactor--;
                Zoom = zoomFactor;
                RaiseZoomFactorChanged(new ZoomFactorChangedEventArgs(zoomFactor));
            }
            return UIActionVisibility.Enabled;
        }
    }
}

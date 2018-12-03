#region License
/*********************************************************************************
 * RichTextBoxEx.UIActions.cs
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
using System.Windows.Forms;

namespace Sandra.UI.WF
{
    public partial class RichTextBoxEx
    {
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

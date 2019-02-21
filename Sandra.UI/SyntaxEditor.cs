#region License
/*********************************************************************************
 * SyntaxEditor.cs
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

using Eutherion.Text;
using Eutherion.UIActions;
using Eutherion.Win.AppTemplate;
using ScintillaNET;
using System.Drawing;
using System.Linq;

namespace Sandra.UI
{
    /// <summary>
    /// Represents a <see cref="ScintillaEx"/> control with syntax highlighting, a number of <see cref="UIAction"/> hooks,
    /// and a mouse-wheel event handler.
    /// </summary>
    public abstract class SyntaxEditor<TTerminal> : ScintillaEx
    {
        protected readonly TextIndex<TTerminal> TextIndex;

        protected Style DefaultStyle => Styles[Style.Default];

        /// <summary>
        /// Gets the back color of this syntax editor in areas where no style or syntax highlighting is applied.
        /// </summary>
        public Color NoStyleBackColor => DefaultStyle.BackColor;

        /// <summary>
        /// Gets the back color of this syntax editor in areas where no style or syntax highlighting is applied.
        /// </summary>
        public Color NoStyleForeColor => DefaultStyle.ForeColor;

        public SyntaxEditor()
        {
            TextIndex = new TextIndex<TTerminal>();

            HScrollBar = false;
            VScrollBar = true;

            Margins.ForEach(x => x.Width = 0);

            if (Session.Current.TryGetAutoSaveValue(SettingKeys.Zoom, out int zoomFactor))
            {
                Zoom = zoomFactor;
            }
        }

        protected void ApplyStyle(TextElement<TTerminal> element, Style style)
        {
            if (style != null)
            {
                StartStyling(element.Start);
                SetStyling(element.Length, style.Index);
            }
        }

        protected override void OnZoomFactorChanged(ZoomFactorChangedEventArgs e)
        {
            // Not only raise the event, but also save the zoom factor setting.
            base.OnZoomFactorChanged(e);
            Session.Current.AutoSave.Persist(SettingKeys.Zoom, e.ZoomFactor);
        }
    }
}

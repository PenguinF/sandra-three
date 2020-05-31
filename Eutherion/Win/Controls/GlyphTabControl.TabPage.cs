#region License
/*********************************************************************************
 * GlyphTabControl.TabPage.cs
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

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Eutherion.Win.Controls
{
    public partial class GlyphTabControl
    {
        /// <summary>
        /// Represents a single tab page in a <see cref="GlyphTabControl"/>.
        /// </summary>
        public class TabPage
        {
            /// <summary>
            /// Gets or sets the text to display in the tab header.
            /// </summary>
            public string Text { get => text; set { if (text != value) { text = value; OnNotifyChange(); } } }

            private string text;

            /// <summary>
            /// Gets or sets if the client control contains any unsaved modifications.
            /// </summary>
            public bool IsModified { get => isModified; set { if (isModified != value) { isModified = value; OnNotifyChange(); } } }

            private bool isModified;

            /// <summary>
            /// Gets or sets the background color to display if the tab page is active.
            /// </summary>
            public Color ActiveBackColor { get => activeBackColor; set { if (activeBackColor != value) { activeBackColor = value; OnNotifyChange(); } } }

            private Color activeBackColor;

            /// <summary>
            /// Gets or sets the foreground color to display if the tab page is active.
            /// </summary>
            public Color ActiveForeColor { get => activeForeColor; set { if (activeForeColor != value) { activeForeColor = value; OnNotifyChange(); } } }

            private Color activeForeColor;

            /// <summary>
            /// Gets or sets the foreground color to display for the glyph if the tab page is active.
            /// If this color is empty, <see cref="ActiveForeColor"/> is used.
            /// </summary>
            public Color GlyphForeColor { get => glyphForeColor; set { if (glyphForeColor != value) { glyphForeColor = value; OnNotifyChange(); } } }

            private Color glyphForeColor;

            /// <summary>
            /// Gets or sets the foreground color to display for the glyph when the mouse is hovering over it and the tab page is active.
            /// If this color is empty, a lighter version of <see cref="GlyphForeColor"/> is used.
            /// </summary>
            public Color GlyphHoverColor { get => glyphHoverColor; set { if (glyphHoverColor != value) { glyphHoverColor = value; OnNotifyChange(); } } }

            private Color glyphHoverColor;

            /// <summary>
            /// Occurs when the style or text of the tab page was updated.
            /// </summary>
            public event Action<TabPage> NotifyChange;

            /// <summary>
            /// Raises the <see cref="NotifyChange"/> event.
            /// </summary>
            protected virtual void OnNotifyChange() => NotifyChange?.Invoke(this);

            /// <summary>
            /// Gets the client control for this tab page.
            /// </summary>
            public Control ClientControl { get; }

            /// <summary>
            /// Initializes a new instance of <see cref="TabPage"/>.
            /// </summary>
            /// <param name="clientControl">
            /// The client control for this tab page.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="clientControl"/> is null.
            /// </exception>
            public TabPage(Control clientControl)
                => ClientControl = clientControl ?? throw new ArgumentNullException(nameof(clientControl));
        }
    }
}

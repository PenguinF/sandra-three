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
using ScintillaNET;
using System.Drawing;
using System.Linq;

namespace Eutherion.Win.AppTemplate
{
    /// <summary>
    /// Represents a <see cref="ScintillaEx"/> control with syntax highlighting.
    /// </summary>
    public abstract class SyntaxEditor<TTerminal> : ScintillaEx
    {
        protected readonly TextIndex<TTerminal> TextIndex;

        protected Style DefaultStyle => Styles[Style.Default];

        public SyntaxEditor()
        {
            TextIndex = new TextIndex<TTerminal>();

            HScrollBar = false;
            VScrollBar = true;

            Margins.ForEach(x => x.Width = 0);
        }

        protected void ApplyStyle(TextElement<TTerminal> element, Style style)
        {
            if (style != null)
            {
                StartStyling(element.Start);
                SetStyling(element.Length, style.Index);
            }
        }
    }

    /// <summary>
    /// Contains default styles for syntax editors.
    /// </summary>
    public static class DefaultSyntaxEditorStyle
    {
        public static readonly Color BackColor = Color.FromArgb(16, 16, 16);
        public static readonly Color ForeColor = Color.WhiteSmoke;
        public static readonly Font Font = new Font("Consolas", 10);

        /// <summary>
        /// Gets the default fore color used for displaying line numbers in a syntax editor.
        /// </summary>
        public static readonly Color LineNumberForeColor = Color.FromArgb(176, 176, 176);
    }
}

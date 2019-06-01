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

using Eutherion.Localization;
using Eutherion.Text;
using Eutherion.Utils;
using Eutherion.Win.Storage;
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
            => ApplyStyle(style, element.Start, element.Length);
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

    /// <summary>
    /// Represents two file names in the local application data folder.
    /// </summary>
    public struct AutoSaveFileNamePair
    {
        public string FileName1;
        public string FileName2;

        public AutoSaveFileNamePair(string fileName1, string fileName2)
        {
            FileName1 = fileName1;
            FileName2 = fileName2;
        }
    }

    /// <summary>
    /// Specialized PType that accepts pairs of legal file names that are used for auto-saving changes in text files.
    /// </summary>
    public sealed class AutoSaveFilePairPType : PType<AutoSaveFileNamePair>
    {
        public static readonly PTypeErrorBuilder AutoSaveFilePairTypeError
            = new PTypeErrorBuilder(new LocalizedStringKey(nameof(AutoSaveFilePairTypeError)));

        public static readonly AutoSaveFilePairPType Instance = new AutoSaveFilePairPType();

        private AutoSaveFilePairPType() { }

        public override Union<ITypeErrorBuilder, AutoSaveFileNamePair> TryGetValidValue(PValue value)
        {
            if (value is PList pair
                && pair.Count == 2
                && FileNameType.InstanceAllowStartWithDots.TryGetValidValue(pair[0]).IsOption2(out string fileName1)
                && FileNameType.InstanceAllowStartWithDots.TryGetValidValue(pair[1]).IsOption2(out string fileName2))
            {
                return ValidValue(new AutoSaveFileNamePair(fileName1, fileName2));
            }

            return InvalidValue(AutoSaveFilePairTypeError);
        }

        public override PValue GetPValue(AutoSaveFileNamePair value) => new PList(
            new PValue[]
            {
                FileNameType.InstanceAllowStartWithDots.GetPValue(value.FileName1),
                FileNameType.InstanceAllowStartWithDots.GetPValue(value.FileName2),
            });
    }
}

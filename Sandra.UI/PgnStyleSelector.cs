#region License
/*********************************************************************************
 * PgnStyleSelector.cs
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

using Eutherion.Win.AppTemplate;
using Sandra.Chess.Pgn;
using ScintillaNET;
using System.Drawing;

namespace Sandra.UI
{
    /// <summary>
    /// A style selector for PGN syntax highlighting.
    /// </summary>
    public class PgnStyleSelector<TSyntaxTree, TError> : PgnSymbolVisitor<SyntaxEditor<TSyntaxTree, IPgnSymbol, TError>, Style>
    {
        private const int tagNameStyleIndex = 8;
        private const int tagValueStyleIndex = 9;
        private const int errorTagValueStyleIndex = 10;
        private const int illegalCharacterStyleIndex = 11;
        private const int symbolStyleIndex = 12;
        private const int escapedLineStyleIndex = 13;

        private static readonly Font tagNameAndEscapeFont = new Font("Consolas", 10, FontStyle.Italic);

        private static readonly Color tagNameForeColor = Color.FromArgb(0x90, 0xff, 0xf0);
        private static readonly Color tagValueForeColor = Color.FromArgb(0xff, 0xbb, 0x9e);
        private static readonly Color errorTagValueForeColor = Color.FromArgb(0xbc, 0x82, 0x70);
        private static readonly Color illegalCharacterForeColor = Color.FromArgb(0xa0, 0xa0, 0xa0);
        private static readonly Color symbolForeColor = Color.FromArgb(0xff, 0xff, 0xa0);
        private static readonly Color escapeForeColor = Color.FromArgb(0x8c, 0x8c, 0x8c);

        public static readonly PgnStyleSelector<TSyntaxTree, TError> Instance = new PgnStyleSelector<TSyntaxTree, TError>();

        public static void InitializeStyles(SyntaxEditor<TSyntaxTree, IPgnSymbol, TError> syntaxEditor)
        {
            syntaxEditor.Styles[tagNameStyleIndex].ForeColor = tagNameForeColor;
            tagNameAndEscapeFont.CopyTo(syntaxEditor.Styles[tagNameStyleIndex]);

            syntaxEditor.Styles[tagValueStyleIndex].ForeColor = tagValueForeColor;

            syntaxEditor.Styles[errorTagValueStyleIndex].ForeColor = errorTagValueForeColor;

            syntaxEditor.Styles[illegalCharacterStyleIndex].ForeColor = illegalCharacterForeColor;

            syntaxEditor.Styles[symbolStyleIndex].ForeColor = symbolForeColor;

            syntaxEditor.Styles[escapedLineStyleIndex].ForeColor = escapeForeColor;
            tagNameAndEscapeFont.CopyTo(syntaxEditor.Styles[escapedLineStyleIndex]);
        }

        private PgnStyleSelector() { }

        public override Style DefaultVisit(IPgnSymbol node, SyntaxEditor<TSyntaxTree, IPgnSymbol, TError> syntaxEditor)
            => syntaxEditor.Styles[symbolStyleIndex];

        public override Style VisitCommentSyntax(PgnCommentSyntax node, SyntaxEditor<TSyntaxTree, IPgnSymbol, TError> syntaxEditor)
            => syntaxEditor.DefaultStyle;

        public override Style VisitEscapeSyntax(PgnEscapeSyntax node, SyntaxEditor<TSyntaxTree, IPgnSymbol, TError> syntaxEditor)
            => syntaxEditor.Styles[escapedLineStyleIndex];

        public override Style VisitIllegalCharacterSyntax(PgnIllegalCharacterSyntax node, SyntaxEditor<TSyntaxTree, IPgnSymbol, TError> syntaxEditor)
            => syntaxEditor.Styles[illegalCharacterStyleIndex];

        public override Style VisitUnterminatedCommentSyntax(PgnUnterminatedCommentSyntax node, SyntaxEditor<TSyntaxTree, IPgnSymbol, TError> syntaxEditor)
            => syntaxEditor.DefaultStyle;

        public override Style VisitPgnSymbol(PgnSymbol node, SyntaxEditor<TSyntaxTree, IPgnSymbol, TError> syntaxEditor)
        {
            switch (node.Green.SymbolType)
            {
                case PgnSymbolType.TagName:
                    return syntaxEditor.Styles[tagNameStyleIndex];
                case PgnSymbolType.TagValue:
                    return syntaxEditor.Styles[tagValueStyleIndex];
                case PgnSymbolType.ErrorTagValue:
                    return syntaxEditor.Styles[errorTagValueStyleIndex];
            }

            return syntaxEditor.Styles[symbolStyleIndex];
        }
    }
}

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
using Sandra.Chess.Pgn.Temp;
using ScintillaNET;
using System.Drawing;

namespace Sandra.UI
{
    /// <summary>
    /// A style selector for PGN syntax highlighting.
    /// </summary>
    public class PgnStyleSelector : PgnSymbolVisitor<SyntaxEditor<RootPgnSyntax, IPgnSymbol, PgnErrorInfo>, Style>
    {
        private const int tagNameStyleIndex = 8;
        private const int tagValueStyleIndex = 9;
        private const int errorTagValueStyleIndex = 10;
        private const int illegalCharacterStyleIndex = 11;
        private const int moveNumberStyleIndex = 12;
        private const int moveTextStyleIndex = 13;
        private const int errorNagStyleIndex = 14;
        private const int escapedLineStyleIndex = 15;

        private static readonly Font tagNameAndEscapeFont = new Font("Consolas", 10, FontStyle.Italic);

        private static readonly Color tagNameForeColor = Color.FromArgb(0x90, 0xff, 0xf0);
        private static readonly Color tagValueForeColor = Color.FromArgb(0xff, 0xbb, 0x9e);
        private static readonly Color errorTagValueForeColor = Color.FromArgb(0xbc, 0x82, 0x70);
        private static readonly Color illegalCharacterForeColor = Color.FromArgb(0xa0, 0xa0, 0xa0);
        private static readonly Color moveNumberForeColor = Color.FromArgb(0xcc, 0xcc, 0x92);
        private static readonly Color moveTextForeColor = Color.FromArgb(0xbb, 0xff, 0x9e);
        private static readonly Color errorNagForeColor = Color.FromArgb(0x82, 0xbc, 0x70);
        private static readonly Color escapeForeColor = Color.FromArgb(0x8c, 0x8c, 0x8c);

        public static readonly PgnStyleSelector Instance = new PgnStyleSelector();

        public static void InitializeStyles(SyntaxEditor<RootPgnSyntax, IPgnSymbol, PgnErrorInfo> syntaxEditor)
        {
            syntaxEditor.Styles[tagNameStyleIndex].ForeColor = tagNameForeColor;
            tagNameAndEscapeFont.CopyTo(syntaxEditor.Styles[tagNameStyleIndex]);

            syntaxEditor.Styles[tagValueStyleIndex].ForeColor = tagValueForeColor;
            syntaxEditor.Styles[errorTagValueStyleIndex].ForeColor = errorTagValueForeColor;
            syntaxEditor.Styles[illegalCharacterStyleIndex].ForeColor = illegalCharacterForeColor;
            syntaxEditor.Styles[moveNumberStyleIndex].ForeColor = moveNumberForeColor;
            syntaxEditor.Styles[moveTextStyleIndex].ForeColor = moveTextForeColor;
            syntaxEditor.Styles[errorNagStyleIndex].ForeColor = errorNagForeColor;

            syntaxEditor.Styles[escapedLineStyleIndex].ForeColor = escapeForeColor;
            tagNameAndEscapeFont.CopyTo(syntaxEditor.Styles[escapedLineStyleIndex]);
        }

        private PgnStyleSelector() { }

        public override Style DefaultVisit(IPgnSymbol node, SyntaxEditor<RootPgnSyntax, IPgnSymbol, PgnErrorInfo> syntaxEditor)
            => syntaxEditor.DefaultStyle;

        public override Style VisitErrorTagValueSyntax(PgnErrorTagValueSyntax node, SyntaxEditor<RootPgnSyntax, IPgnSymbol, PgnErrorInfo> syntaxEditor)
            => syntaxEditor.Styles[errorTagValueStyleIndex];

        public override Style VisitEscapeSyntax(PgnEscapeSyntax node, SyntaxEditor<RootPgnSyntax, IPgnSymbol, PgnErrorInfo> syntaxEditor)
            => syntaxEditor.Styles[escapedLineStyleIndex];

        public override Style VisitIllegalCharacterSyntax(PgnIllegalCharacterSyntax node, SyntaxEditor<RootPgnSyntax, IPgnSymbol, PgnErrorInfo> syntaxEditor)
            => syntaxEditor.Styles[illegalCharacterStyleIndex];

        public override Style VisitTagNameSyntax(PgnTagNameSyntax node, SyntaxEditor<RootPgnSyntax, IPgnSymbol, PgnErrorInfo> syntaxEditor)
            => syntaxEditor.Styles[tagNameStyleIndex];

        public override Style VisitTagValueSyntax(PgnTagValueSyntax node, SyntaxEditor<RootPgnSyntax, IPgnSymbol, PgnErrorInfo> syntaxEditor)
            => syntaxEditor.Styles[tagValueStyleIndex];

        public override Style VisitPgnSymbol(PgnSymbol node, SyntaxEditor<RootPgnSyntax, IPgnSymbol, PgnErrorInfo> syntaxEditor)
        {
            switch (node.Green.SymbolType)
            {
                case PgnSymbolType.MoveNumber:
                case PgnSymbolType.Period:
                    return syntaxEditor.Styles[moveNumberStyleIndex];
                case PgnSymbolType.Move:
                case PgnSymbolType.Nag:
                // Don't darken this one like OverflowNag, got to go through this state before creating a valid NAG.
                case PgnSymbolType.EmptyNag:
                    return syntaxEditor.Styles[moveTextStyleIndex];
                case PgnSymbolType.OverflowNag:
                    return syntaxEditor.Styles[errorNagStyleIndex];
                case PgnSymbolType.Unknown:
                    return syntaxEditor.Styles[illegalCharacterStyleIndex];
                case PgnSymbolType.Asterisk:
                case PgnSymbolType.DrawMarker:
                case PgnSymbolType.WhiteWinMarker:
                case PgnSymbolType.BlackWinMarker:
                    return syntaxEditor.Styles[moveTextStyleIndex];
            }

            return syntaxEditor.DefaultStyle;
        }
    }
}

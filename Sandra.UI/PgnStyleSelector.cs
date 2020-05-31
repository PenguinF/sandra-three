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

using Eutherion.Win.MdiAppTemplate;
using Sandra.Chess.Pgn;
using ScintillaNET;
using System.Drawing;

namespace Sandra.UI
{
    using PgnEditor = SyntaxEditor<RootPgnSyntax, IPgnSymbol, PgnErrorInfo>;

    /// <summary>
    /// A style selector for PGN syntax highlighting.
    /// </summary>
    public class PgnStyleSelector : PgnSymbolVisitor<PgnEditor, Style>
    {
        private const int tagNameStyleIndex = 8;
        private const int tagValueStyleIndex = 9;
        private const int errorTagValueStyleIndex = 10;
        private const int illegalCharacterStyleIndex = 11;
        private const int moveNumberStyleIndex = 12;
        private const int moveTextStyleIndex = 13;
        private const int errorSymbolStyleIndex = 14;
        private const int escapedLineStyleIndex = 15;

        private static readonly Font tagNameAndEscapeFont = new Font("Consolas", 10, FontStyle.Italic);

        private static readonly Color tagNameForeColor = Color.FromArgb(0x90, 0xff, 0xf0);
        private static readonly Color tagValueForeColor = Color.FromArgb(0xff, 0xbb, 0x9e);
        private static readonly Color errorTagValueForeColor = Color.FromArgb(0xbc, 0x82, 0x70);
        private static readonly Color illegalCharacterForeColor = Color.FromArgb(0xa0, 0xa0, 0xa0);
        private static readonly Color moveNumberForeColor = Color.FromArgb(0xcc, 0xcc, 0x92);
        private static readonly Color moveTextForeColor = Color.FromArgb(0xbb, 0xff, 0x9e);
        private static readonly Color errorSymbolForeColor = Color.FromArgb(0x82, 0xbc, 0x70);
        private static readonly Color escapeForeColor = Color.FromArgb(0x8c, 0x8c, 0x8c);

        public static readonly PgnStyleSelector Instance = new PgnStyleSelector();

        public static void InitializeStyles(PgnEditor pgnEditor)
        {
            pgnEditor.Styles[tagNameStyleIndex].ForeColor = tagNameForeColor;
            tagNameAndEscapeFont.CopyTo(pgnEditor.Styles[tagNameStyleIndex]);

            pgnEditor.Styles[tagValueStyleIndex].ForeColor = tagValueForeColor;
            pgnEditor.Styles[errorTagValueStyleIndex].ForeColor = errorTagValueForeColor;
            pgnEditor.Styles[illegalCharacterStyleIndex].ForeColor = illegalCharacterForeColor;
            pgnEditor.Styles[moveNumberStyleIndex].ForeColor = moveNumberForeColor;
            pgnEditor.Styles[moveTextStyleIndex].ForeColor = moveTextForeColor;
            pgnEditor.Styles[errorSymbolStyleIndex].ForeColor = errorSymbolForeColor;

            pgnEditor.Styles[escapedLineStyleIndex].ForeColor = escapeForeColor;
            tagNameAndEscapeFont.CopyTo(pgnEditor.Styles[escapedLineStyleIndex]);
        }

        private PgnStyleSelector() { }

        public override Style DefaultVisit(IPgnSymbol node, PgnEditor pgnEditor)
            => pgnEditor.DefaultStyle;

        public override Style VisitEscapeSyntax(PgnEscapeSyntax node, PgnEditor pgnEditor)
            => pgnEditor.Styles[escapedLineStyleIndex];

        public override Style VisitGameResultSyntax(PgnGameResultSyntax node, PgnEditor pgnEditor)
            => pgnEditor.Styles[moveTextStyleIndex];

        public override Style VisitIllegalCharacterSyntax(PgnIllegalCharacterSyntax node, PgnEditor pgnEditor)
            => pgnEditor.Styles[illegalCharacterStyleIndex];

        public override Style VisitMoveNumberSyntax(PgnMoveNumberSyntax node, PgnEditor pgnEditor)
            => pgnEditor.Styles[moveNumberStyleIndex];

        public override Style VisitMoveSyntax(PgnMoveSyntax node, PgnEditor pgnEditor)
            => pgnEditor.Styles[node.IsUnrecognizedMove ? errorSymbolStyleIndex : moveTextStyleIndex];

        // Only darken OverflowNag, and display EmptyNag like a regular NAG. Got to go through that state before creating a valid NAG.
        public override Style VisitNagSyntax(PgnNagSyntax node, PgnEditor pgnEditor)
            => pgnEditor.Styles[node.Green.SymbolType == PgnSymbolType.OverflowNag ? errorSymbolStyleIndex : moveTextStyleIndex];

        public override Style VisitOrphanParenthesisCloseSyntax(PgnOrphanParenthesisCloseSyntax node, PgnEditor pgnEditor)
            => pgnEditor.Styles[illegalCharacterStyleIndex];

        public override Style VisitPeriodSyntax(PgnPeriodSyntax node, PgnEditor pgnEditor)
            => pgnEditor.Styles[moveNumberStyleIndex];

        public override Style VisitTagElementInMoveTreeSyntax(PgnTagElementInMoveTreeSyntax node, PgnEditor pgnEditor)
        {
            switch (node.Green.TagElement.SymbolType)
            {
                case PgnSymbolType.TagValue:
                case PgnSymbolType.ErrorTagValue:
                    return pgnEditor.Styles[errorTagValueStyleIndex];
            }

            // Display [ and ] like illegal characters.
            return pgnEditor.Styles[illegalCharacterStyleIndex];
        }

        public override Style VisitTagNameSyntax(PgnTagNameSyntax node, PgnEditor pgnEditor)
            => pgnEditor.Styles[tagNameStyleIndex];

        public override Style VisitTagValueSyntax(PgnTagValueSyntax node, PgnEditor pgnEditor)
            => pgnEditor.Styles[node.ContainsErrors ? errorTagValueStyleIndex : tagValueStyleIndex];
    }
}

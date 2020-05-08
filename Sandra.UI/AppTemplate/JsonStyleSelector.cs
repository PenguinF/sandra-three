#region License
/*********************************************************************************
 * JsonStyleSelector.cs
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

using Eutherion.Text.Json;
using ScintillaNET;
using System.Drawing;

namespace Eutherion.Win.AppTemplate
{
    /// <summary>
    /// A style selector for json syntax highlighting.
    /// </summary>
    public class JsonStyleSelector<TSyntaxTree> : JsonSymbolVisitor<SyntaxEditor<TSyntaxTree, IJsonSymbol, JsonErrorInfo>, Style>
    {
        private const int commentStyleIndex = 8;
        private const int booleanIntegerStyleIndex = 9;
        private const int stringStyleIndex = 10;
        private const int errorStringStyleIndex = 11;
        private const int undefinedValueStyleIndex = 12;
        private const int unknownSymbolStyleIndex = 13;

        private static readonly Color commentForeColor = Color.FromArgb(128, 220, 220);
        private static readonly Font commentFont = new Font("Consolas", 10, FontStyle.Italic);

        private static readonly Font valueFont = new Font("Consolas", 10, FontStyle.Bold);

        private static readonly Color boolIntegerForeColor = Color.FromArgb(255, 255, 60);
        private static readonly Color stringForeColor = Color.FromArgb(255, 192, 144);
        private static readonly Color errorStringForeColor = Color.FromArgb(224, 168, 126);
        private static readonly Color undefinedValueForeColor = Color.FromArgb(192, 192, 40);
        private static readonly Color unknownSymbolForeColor = Color.FromArgb(204, 204, 204);

        public static readonly JsonStyleSelector<TSyntaxTree> Instance = new JsonStyleSelector<TSyntaxTree>();

        public static void InitializeStyles(SyntaxEditor<TSyntaxTree, IJsonSymbol, JsonErrorInfo> syntaxEditor)
        {
            syntaxEditor.Styles[commentStyleIndex].ForeColor = commentForeColor;
            commentFont.CopyTo(syntaxEditor.Styles[commentStyleIndex]);

            syntaxEditor.Styles[booleanIntegerStyleIndex].ForeColor = boolIntegerForeColor;
            valueFont.CopyTo(syntaxEditor.Styles[booleanIntegerStyleIndex]);

            syntaxEditor.Styles[stringStyleIndex].ForeColor = stringForeColor;
            syntaxEditor.Styles[errorStringStyleIndex].ForeColor = errorStringForeColor;

            syntaxEditor.Styles[undefinedValueStyleIndex].ForeColor = undefinedValueForeColor;
            valueFont.CopyTo(syntaxEditor.Styles[undefinedValueStyleIndex]);

            syntaxEditor.Styles[unknownSymbolStyleIndex].ForeColor = unknownSymbolForeColor;
        }

        private JsonStyleSelector() { }

        public override Style DefaultVisit(IJsonSymbol node, SyntaxEditor<TSyntaxTree, IJsonSymbol, JsonErrorInfo> syntaxEditor)
            => syntaxEditor.DefaultStyle;

        public override Style VisitBooleanLiteralSyntax(JsonBooleanLiteralSyntax node, SyntaxEditor<TSyntaxTree, IJsonSymbol, JsonErrorInfo> syntaxEditor)
            => syntaxEditor.Styles[booleanIntegerStyleIndex];

        public override Style VisitCommentSyntax(JsonCommentSyntax node, SyntaxEditor<TSyntaxTree, IJsonSymbol, JsonErrorInfo> syntaxEditor)
            => syntaxEditor.Styles[commentStyleIndex];

        public override Style VisitErrorStringSyntax(JsonErrorStringSyntax node, SyntaxEditor<TSyntaxTree, IJsonSymbol, JsonErrorInfo> syntaxEditor)
            => syntaxEditor.Styles[errorStringStyleIndex];

        public override Style VisitIntegerLiteralSyntax(JsonIntegerLiteralSyntax node, SyntaxEditor<TSyntaxTree, IJsonSymbol, JsonErrorInfo> syntaxEditor)
            => syntaxEditor.Styles[booleanIntegerStyleIndex];

        public override Style VisitRootLevelValueDelimiterSyntax(JsonRootLevelValueDelimiterSyntax node, SyntaxEditor<TSyntaxTree, IJsonSymbol, JsonErrorInfo> syntaxEditor)
            => syntaxEditor.Styles[unknownSymbolStyleIndex];

        public override Style VisitStringLiteralSyntax(JsonStringLiteralSyntax node, SyntaxEditor<TSyntaxTree, IJsonSymbol, JsonErrorInfo> syntaxEditor)
            => syntaxEditor.Styles[stringStyleIndex];

        public override Style VisitUndefinedValueSyntax(JsonUndefinedValueSyntax node, SyntaxEditor<TSyntaxTree, IJsonSymbol, JsonErrorInfo> syntaxEditor)
            => syntaxEditor.Styles[undefinedValueStyleIndex];

        public override Style VisitUnknownSymbolSyntax(JsonUnknownSymbolSyntax node, SyntaxEditor<TSyntaxTree, IJsonSymbol, JsonErrorInfo> syntaxEditor)
            => syntaxEditor.Styles[unknownSymbolStyleIndex];

        public override Style VisitUnterminatedMultiLineCommentSyntax(JsonUnterminatedMultiLineCommentSyntax node, SyntaxEditor<TSyntaxTree, IJsonSymbol, JsonErrorInfo> syntaxEditor)
            => syntaxEditor.Styles[commentStyleIndex];
    }
}

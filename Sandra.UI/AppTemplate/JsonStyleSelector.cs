#region License
/*********************************************************************************
 * JsonStyleSelector.cs
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

using Eutherion.Text.Json;
using ScintillaNET;
using System.Drawing;

namespace Eutherion.Win.AppTemplate
{
    /// <summary>
    /// A style selector for json syntax highlighting.
    /// </summary>
    public class JsonStyleSelector<TSyntaxTree, TError> : JsonTerminalSymbolVisitor<SyntaxEditor<TSyntaxTree, JsonSyntax, TError>, Style>
    {
        private const int commentStyleIndex = 8;
        private const int booleanIntegerStyleIndex = 9;
        private const int stringStyleIndex = 10;
        private const int undefinedValueStyleIndex = 11;

        private static readonly Color commentForeColor = Color.FromArgb(128, 220, 220);
        private static readonly Font commentFont = new Font("Consolas", 10, FontStyle.Italic);

        private static readonly Font valueFont = new Font("Consolas", 10, FontStyle.Bold);

        private static readonly Color boolIntegerForeColor = Color.FromArgb(255, 255, 60);
        private static readonly Color stringForeColor = Color.FromArgb(255, 192, 144);
        private static readonly Color undefinedValueForeColor = Color.FromArgb(192, 192, 40);

        public static readonly JsonStyleSelector<TSyntaxTree, TError> Instance = new JsonStyleSelector<TSyntaxTree, TError>();

        public static void InitializeStyles(SyntaxEditor<TSyntaxTree, JsonSyntax, TError> syntaxEditor)
        {
            syntaxEditor.Styles[commentStyleIndex].ForeColor = commentForeColor;
            commentFont.CopyTo(syntaxEditor.Styles[commentStyleIndex]);

            syntaxEditor.Styles[booleanIntegerStyleIndex].ForeColor = boolIntegerForeColor;
            valueFont.CopyTo(syntaxEditor.Styles[booleanIntegerStyleIndex]);

            syntaxEditor.Styles[stringStyleIndex].ForeColor = stringForeColor;

            syntaxEditor.Styles[undefinedValueStyleIndex].ForeColor = undefinedValueForeColor;
            valueFont.CopyTo(syntaxEditor.Styles[undefinedValueStyleIndex]);
        }

        private JsonStyleSelector() { }

        public override Style DefaultVisit(JsonSyntax node, SyntaxEditor<TSyntaxTree, JsonSyntax, TError> syntaxEditor)
            => syntaxEditor.DefaultStyle;

        public override Style VisitBackgroundSyntax(JsonBackgroundSyntax node, SyntaxEditor<TSyntaxTree, JsonSyntax, TError> syntaxEditor)
            => syntaxEditor.Styles[commentStyleIndex];

        public override Style VisitBooleanLiteralSyntax(JsonBooleanLiteralSyntax node, SyntaxEditor<TSyntaxTree, JsonSyntax, TError> syntaxEditor)
            => syntaxEditor.Styles[booleanIntegerStyleIndex];

        public override Style VisitIntegerLiteralSyntax(JsonIntegerLiteralSyntax node, SyntaxEditor<TSyntaxTree, JsonSyntax, TError> syntaxEditor)
            => syntaxEditor.Styles[booleanIntegerStyleIndex];

        public override Style VisitStringLiteralSyntax(JsonStringLiteralSyntax node, SyntaxEditor<TSyntaxTree, JsonSyntax, TError> syntaxEditor)
            => syntaxEditor.Styles[stringStyleIndex];

        public override Style VisitUndefinedValueSyntax(JsonUndefinedValueSyntax node, SyntaxEditor<TSyntaxTree, JsonSyntax, TError> syntaxEditor)
            => node.Green.UndefinedToken is JsonErrorString
            ? syntaxEditor.Styles[stringStyleIndex]
            : syntaxEditor.Styles[undefinedValueStyleIndex];
    }
}

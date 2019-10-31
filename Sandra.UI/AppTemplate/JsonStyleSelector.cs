﻿#region License
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
        private const int valueStyleIndex = 9;
        private const int stringStyleIndex = 10;

        private static readonly Color commentForeColor = Color.FromArgb(128, 220, 220);
        private static readonly Font commentFont = new Font("Consolas", 10, FontStyle.Italic);

        private static readonly Color valueForeColor = Color.FromArgb(255, 255, 60);
        private static readonly Font valueFont = new Font("Consolas", 10, FontStyle.Bold);

        private static readonly Color stringForeColor = Color.FromArgb(255, 192, 144);

        public static readonly JsonStyleSelector<TSyntaxTree, TError> Instance = new JsonStyleSelector<TSyntaxTree, TError>();

        public static void InitializeStyles(SyntaxEditor<TSyntaxTree, JsonSyntax, TError> syntaxEditor)
        {
            syntaxEditor.Styles[commentStyleIndex].ForeColor = commentForeColor;
            commentFont.CopyTo(syntaxEditor.Styles[commentStyleIndex]);

            syntaxEditor.Styles[valueStyleIndex].ForeColor = valueForeColor;
            valueFont.CopyTo(syntaxEditor.Styles[valueStyleIndex]);

            syntaxEditor.Styles[stringStyleIndex].ForeColor = stringForeColor;
        }

        private JsonStyleSelector() { }

        public override Style DefaultVisit(JsonSyntax node, SyntaxEditor<TSyntaxTree, JsonSyntax, TError> syntaxEditor)
            => syntaxEditor.DefaultStyle;

        public override Style VisitBackgroundSyntax(RedJsonBackgroundSyntax node, SyntaxEditor<TSyntaxTree, JsonSyntax, TError> syntaxEditor)
            => syntaxEditor.Styles[commentStyleIndex];

        public override Style VisitBooleanLiteralSyntax(RedJsonBooleanLiteralSyntax node, SyntaxEditor<TSyntaxTree, JsonSyntax, TError> syntaxEditor)
            => syntaxEditor.Styles[valueStyleIndex];

        public override Style VisitIntegerLiteralSyntax(RedJsonIntegerLiteralSyntax node, SyntaxEditor<TSyntaxTree, JsonSyntax, TError> syntaxEditor)
            => syntaxEditor.Styles[valueStyleIndex];

        public override Style VisitStringLiteralSyntax(RedJsonStringLiteralSyntax node, SyntaxEditor<TSyntaxTree, JsonSyntax, TError> syntaxEditor)
            => syntaxEditor.Styles[stringStyleIndex];

        public override Style VisitUndefinedValueSyntax(RedJsonUndefinedValueSyntax node, SyntaxEditor<TSyntaxTree, JsonSyntax, TError> syntaxEditor)
            => node.Green.UndefinedToken is JsonErrorString
            ? syntaxEditor.Styles[stringStyleIndex]
            : syntaxEditor.Styles[valueStyleIndex];
    }
}

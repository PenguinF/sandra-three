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
using Eutherion.Win.Storage;
using ScintillaNET;
using System.Drawing;

namespace Eutherion.Win.AppTemplate
{
    /// <summary>
    /// A style selector for json syntax highlighting.
    /// </summary>
    public class JsonStyleSelector : JsonSymbolVisitor<SyntaxEditor<SettingSyntaxTree, JsonSymbol, JsonErrorInfo>, Style>
    {
        private const int commentStyleIndex = 8;
        private const int valueStyleIndex = 9;
        private const int stringStyleIndex = 10;

        private static readonly Color commentForeColor = Color.FromArgb(128, 220, 220);
        private static readonly Font commentFont = new Font("Consolas", 10, FontStyle.Italic);

        private static readonly Color valueForeColor = Color.FromArgb(255, 255, 60);
        private static readonly Font valueFont = new Font("Consolas", 10, FontStyle.Bold);

        private static readonly Color stringForeColor = Color.FromArgb(255, 192, 144);

        public static readonly JsonStyleSelector Instance = new JsonStyleSelector();

        public static void InitializeStyles(SyntaxEditor<SettingSyntaxTree, JsonSymbol, JsonErrorInfo> syntaxEditor)
        {
            syntaxEditor.Styles[commentStyleIndex].ForeColor = commentForeColor;
            commentFont.CopyTo(syntaxEditor.Styles[commentStyleIndex]);

            syntaxEditor.Styles[valueStyleIndex].ForeColor = valueForeColor;
            valueFont.CopyTo(syntaxEditor.Styles[valueStyleIndex]);

            syntaxEditor.Styles[stringStyleIndex].ForeColor = stringForeColor;
        }

        private JsonStyleSelector() { }

        public override Style DefaultVisit(JsonSymbol symbol, SyntaxEditor<SettingSyntaxTree, JsonSymbol, JsonErrorInfo> syntaxEditor)
            => syntaxEditor.DefaultStyle;

        public override Style VisitComment(JsonComment symbol, SyntaxEditor<SettingSyntaxTree, JsonSymbol, JsonErrorInfo> syntaxEditor)
            => syntaxEditor.Styles[commentStyleIndex];

        public override Style VisitErrorString(JsonErrorString symbol, SyntaxEditor<SettingSyntaxTree, JsonSymbol, JsonErrorInfo> syntaxEditor)
            => syntaxEditor.Styles[stringStyleIndex];

        public override Style VisitString(JsonString symbol, SyntaxEditor<SettingSyntaxTree, JsonSymbol, JsonErrorInfo> syntaxEditor)
            => syntaxEditor.Styles[stringStyleIndex];

        public override Style VisitUnterminatedMultiLineComment(JsonUnterminatedMultiLineComment symbol, SyntaxEditor<SettingSyntaxTree, JsonSymbol, JsonErrorInfo> syntaxEditor)
            => syntaxEditor.Styles[commentStyleIndex];

        public override Style VisitValue(JsonValue symbol, SyntaxEditor<SettingSyntaxTree, JsonSymbol, JsonErrorInfo> syntaxEditor)
            => syntaxEditor.Styles[valueStyleIndex];
    }
}

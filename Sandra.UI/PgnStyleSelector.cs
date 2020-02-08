﻿#region License
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
        private const int illegalCharacterStyleIndex = 8;

        private static readonly Font regularFont = new Font("Consolas", 10);

        private static readonly Color illegalCharacterForeColor = Color.FromArgb(192, 192, 40);

        public static readonly PgnStyleSelector<TSyntaxTree, TError> Instance = new PgnStyleSelector<TSyntaxTree, TError>();

        public static void InitializeStyles(SyntaxEditor<TSyntaxTree, IPgnSymbol, TError> syntaxEditor)
        {
            syntaxEditor.Styles[illegalCharacterStyleIndex].ForeColor = illegalCharacterForeColor;
            regularFont.CopyTo(syntaxEditor.Styles[illegalCharacterStyleIndex]);
        }

        private PgnStyleSelector() { }

        public override Style DefaultVisit(IPgnSymbol symbol, SyntaxEditor<TSyntaxTree, IPgnSymbol, TError> syntaxEditor)
            => syntaxEditor.DefaultStyle;

        public override Style VisitIllegalCharacterSyntax(PgnIllegalCharacterSyntax symbol, SyntaxEditor<TSyntaxTree, IPgnSymbol, TError> syntaxEditor)
            => syntaxEditor.Styles[illegalCharacterStyleIndex];
    }
}

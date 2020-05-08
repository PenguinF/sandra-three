#region License
/*********************************************************************************
 * PgnSyntaxDescriptor.cs
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

using Eutherion.Localization;
using Eutherion.Win.AppTemplate;
using Sandra.Chess.Pgn;
using ScintillaNET;
using System.Collections.Generic;

namespace Sandra.UI
{
    /// <summary>
    /// Describes the interaction between PGN syntax and a syntax editor.
    /// </summary>
    public class PgnSyntaxDescriptor : SyntaxDescriptor<RootPgnSyntax, IPgnSymbol, PgnErrorInfo>
    {
        public static readonly string PgnFileExtension = "pgn";

        public static readonly PgnSyntaxDescriptor Instance = new PgnSyntaxDescriptor();

        private PgnSyntaxDescriptor() { }

        public override string FileExtension => PgnFileExtension;

        public override LocalizedStringKey FileExtensionLocalizedKey => LocalizedStringKeys.PgnFiles;

        public override RootPgnSyntax Parse(string code)
        {
            var rootNode = PgnParser.Parse(code);

            if (rootNode.Errors.Count > 0)
            {
                rootNode.Errors.Sort((x, y)
                    => x.Start < y.Start ? -1
                    : x.Start > y.Start ? 1
                    : x.Length < y.Length ? -1
                    : x.Length > y.Length ? 1
                    : x.ErrorCode < y.ErrorCode ? -1
                    : x.ErrorCode > y.ErrorCode ? 1
                    : 0);
            }

            return rootNode;
        }

        public override IEnumerable<IPgnSymbol> GetTerminalsInRange(RootPgnSyntax syntaxTree, int start, int length)
            => syntaxTree.Syntax.TerminalSymbolsInRange(start, length);

        public override IEnumerable<PgnErrorInfo> GetErrors(RootPgnSyntax syntaxTree)
            => syntaxTree.Errors;

        public override Style GetStyle(SyntaxEditor<RootPgnSyntax, IPgnSymbol, PgnErrorInfo> syntaxEditor, IPgnSymbol terminalSymbol)
            => PgnStyleSelector.Instance.Visit(terminalSymbol, syntaxEditor);

        public override (int, int) GetTokenSpan(IPgnSymbol terminalSymbol)
            => (terminalSymbol.ToSyntax().AbsoluteStart, terminalSymbol.Length);

        public override (int, int) GetErrorRange(PgnErrorInfo error)
            => (error.Start, error.Length);

        public override ErrorLevel GetErrorLevel(PgnErrorInfo error)
            => ErrorLevel.Error;

        public override string GetErrorMessage(PgnErrorInfo error)
            => error.Message(Session.Current.CurrentLocalizer);
    }
}

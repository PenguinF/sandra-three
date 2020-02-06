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

using Eutherion;
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
    public class PgnSyntaxDescriptor : SyntaxDescriptor<RootPgnSyntax, IGreenPgnSymbol, PgnErrorInfo>
    {
        public static readonly string PgnFileExtension = "pgn";

        public override string FileExtension => PgnFileExtension;

        public override LocalizedStringKey FileExtensionLocalizedKey => LocalizedStringKeys.PgnFiles;

        public override RootPgnSyntax Parse(string code)
        {
            int length = code.Length;
            if (length == 0) return new RootPgnSyntax(EmptyEnumerable<PgnSymbol>.Instance);
            return new RootPgnSyntax(new PgnSymbol[] { new PgnSymbol(length) });
        }

        public override IEnumerable<IGreenPgnSymbol> GetTerminalsInRange(RootPgnSyntax syntaxTree, int start, int length)
            => syntaxTree.Terminals;

        public override IEnumerable<PgnErrorInfo> GetErrors(RootPgnSyntax syntaxTree)
            => syntaxTree.Errors;

        public override Style GetStyle(SyntaxEditor<RootPgnSyntax, IGreenPgnSymbol, PgnErrorInfo> syntaxEditor, IGreenPgnSymbol terminalSymbol)
            => syntaxEditor.DefaultStyle;

        public override (int, int) GetTokenSpan(IGreenPgnSymbol terminalSymbol)
            => (0, terminalSymbol.Length);

        public override (int, int) GetErrorRange(PgnErrorInfo error)
            => (error.Start, error.Length);

        public override string GetErrorMessage(PgnErrorInfo error)
            => error.Message(Session.Current.CurrentLocalizer);
    }

    public class PgnSymbol : IPgnForegroundSymbol
    {
        public int Length { get; }

        public PgnSymbol(int length) => Length = length;

        IEnumerable<PgnErrorInfo> IGreenPgnSymbol.GetErrors(int startPosition) => EmptyEnumerable<PgnErrorInfo>.Instance;

        Union<GreenPgnBackgroundSyntax, IPgnForegroundSymbol> IGreenPgnSymbol.AsBackgroundOrForeground() => this;
    }
}

#region License
/*********************************************************************************
 * PGNSyntaxDescriptor.cs
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

using Eutherion.Text;
using Eutherion.Win.AppTemplate;
using ScintillaNET;
using System.Collections.Generic;

namespace Sandra.UI
{
    /// <summary>
    /// Describes the interaction between PGN syntax and a syntax editor.
    /// </summary>
    public class PGNSyntaxDescriptor : SyntaxDescriptor<PGNSymbol, PGNErrorInfo>
    {
        public override (IEnumerable<TextElement<PGNSymbol>>, List<PGNErrorInfo>) Parse(string code)
            => (new TextElement<PGNSymbol>[] { new TextElement<PGNSymbol>(new PGNSymbol()) { Length = code.Length } }, new List<PGNErrorInfo>());

        public override Style GetStyle(SyntaxEditor<PGNSymbol, PGNErrorInfo> syntaxEditor, PGNSymbol terminalSymbol)
            => syntaxEditor.DefaultStyle;

        public override (int, int) GetErrorRange(PGNErrorInfo error)
            => (error.Start, error.Length);

        public override string GetErrorMessage(PGNErrorInfo error)
            => error.Message;
    }

    public class PGNSymbol
    {
    }

    public class PGNErrorInfo
    {
        public int Start { get; }
        public int Length { get; }
        public string Message { get; }

        public PGNErrorInfo(int start, int length, string message)
        {
            Start = start;
            Length = length;
            Message = message;
        }
    }
}

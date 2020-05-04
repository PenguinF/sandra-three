#region License
/*********************************************************************************
 * PgnSyntaxWithLeadingTrivia.cs
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
using Sandra.Chess.Pgn.Temp;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a syntax node together with its leading trivia.
    /// </summary>
    public abstract class PgnSyntaxWithLeadingTrivia : PgnSyntax
    {
        private readonly SafeLazyObject<PgnTriviaSyntax> leadingTrivia;

        /// <summary>
        /// Gets the leading trivia of the syntax node.
        /// </summary>
        public PgnTriviaSyntax LeadingTrivia => leadingTrivia.Object;

        internal PgnSyntaxWithLeadingTrivia(GreenPgnTopLevelSymbolSyntax green)
        {
            leadingTrivia = new SafeLazyObject<PgnTriviaSyntax>(() => new PgnTriviaSyntax(this, green.LeadingTrivia));
        }
    }
}

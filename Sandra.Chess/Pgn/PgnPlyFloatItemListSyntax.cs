#region License
/*********************************************************************************
 * PgnPlyFloatItemListSyntax.cs
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

using Eutherion.Text;
using Eutherion.Utils;
using Sandra.Chess.Pgn.Temp;
using System.Collections.Generic;

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a node with floating items within a ply.
    /// </summary>
    public sealed class PgnPlyFloatItemListSyntax : PgnSyntax
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public WithPlyFloatItemsSyntax Parent { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public ReadOnlySpanList<GreenWithTriviaSyntax> Green { get; }

        public ReadOnlySpanList<GreenPgnTopLevelSymbolSyntaxTempCopy> GreenTopLevelNodes { get; }

        public SafeLazyObjectCollection<IPgnTopLevelSyntax> TopLevelNodes { get; }

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => 0;

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override PgnSyntax ParentSyntax => Parent;

        /// <summary>
        /// Gets the number of children of this syntax node.
        /// </summary>
        public override int ChildCount => TopLevelNodes.Count;

        /// <summary>
        /// Initializes the child at the given <paramref name="index"/> and returns it.
        /// </summary>
        public override PgnSyntax GetChild(int index) => TopLevelNodes[index].ToPgnSyntax();

        /// <summary>
        /// Gets the start position of the child at the given <paramref name="index"/>, without initializing it.
        /// </summary>
        public override int GetChildStartPosition(int index) => GreenTopLevelNodes.GetElementOffset(index);

        internal PgnPlyFloatItemListSyntax(WithPlyFloatItemsSyntax parent, ReadOnlySpanList<GreenWithTriviaSyntax> green)
        {
            Parent = parent;
            Green = green;

            List<GreenPgnTopLevelSymbolSyntaxTempCopy> greenTopLevelNodes = new List<GreenPgnTopLevelSymbolSyntaxTempCopy>();

            foreach (var floatItem in green)
            {
                greenTopLevelNodes.Add(new GreenPgnTopLevelSymbolSyntaxTempCopy(floatItem, (innerParent, index, innerGreen) => new PgnPeriodWithTriviaSyntax(innerParent, index, innerGreen)));
            }

            GreenTopLevelNodes = ReadOnlySpanList<GreenPgnTopLevelSymbolSyntaxTempCopy>.Create(greenTopLevelNodes);

            TopLevelNodes = new SafeLazyObjectCollection<IPgnTopLevelSyntax>(
                greenTopLevelNodes.Count,
                index => GreenTopLevelNodes[index].SyntaxNodeConstructor(this, index, GreenTopLevelNodes[index].GreenNodeWithTrivia));
        }
    }
}

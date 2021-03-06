﻿#region License
/*********************************************************************************
 * PgnTagElementWithTriviaSyntax.cs
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

namespace Sandra.Chess.Pgn
{
    /// <summary>
    /// Represents a node containing a single tag section element together with its leading trivia.
    /// </summary>
    public sealed class PgnTagElementWithTriviaSyntax : WithTriviaSyntax<PgnTagElementSyntax>
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public PgnTagPairSyntax Parent { get; }

        /// <summary>
        /// Gets the index of this syntax node in its parent.
        /// </summary>
        public int ParentIndex { get; }

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Green.TagElementNodes.GetElementOffset(ParentIndex);

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override PgnSyntax ParentSyntax => Parent;

        internal override PgnTagElementSyntax CreateContentNode() => ((GreenPgnTagElementSyntax)Green.ContentNode).CreateRedNode(this);

        internal PgnTagElementWithTriviaSyntax(PgnTagPairSyntax parent, int parentIndex, GreenWithTriviaSyntax green)
            : base(green)
        {
            Parent = parent;
            ParentIndex = parentIndex;
        }
    }
}

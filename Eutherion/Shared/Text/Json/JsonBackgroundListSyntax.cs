#region License
/*********************************************************************************
 * JsonBackgroundListSyntax.cs
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

using Eutherion.Utils;
using System;
using System.Collections.Generic;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a node with background symbols in an abstract json syntax tree.
    /// </summary>
    public sealed class GreenJsonBackgroundListSyntax : ISpan
    {
        /// <summary>
        /// Gets the empty <see cref="GreenJsonBackgroundListSyntax"/>.
        /// </summary>
        public static readonly GreenJsonBackgroundListSyntax Empty = new GreenJsonBackgroundListSyntax(ReadOnlySpanList<GreenJsonBackgroundSyntax>.Empty);

        /// <summary>
        /// Initializes a new instance of <see cref="GreenJsonBackgroundListSyntax"/>.
        /// </summary>
        /// <param name="source">
        /// The source enumeration of <see cref="GreenJsonBackgroundSyntax"/>.
        /// </param>
        /// <returns>
        /// The new <see cref="GreenJsonBackgroundListSyntax"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> is null.
        /// </exception>
        public static GreenJsonBackgroundListSyntax Create(IEnumerable<GreenJsonBackgroundSyntax> source)
        {
            var readOnlyBackground = ReadOnlySpanList<GreenJsonBackgroundSyntax>.Create(source);
            if (readOnlyBackground.Count == 0) return Empty;
            return new GreenJsonBackgroundListSyntax(readOnlyBackground);
        }

        /// <summary>
        /// Gets the read-only list with background nodes.
        /// </summary>
        public ReadOnlySpanList<GreenJsonBackgroundSyntax> BackgroundNodes { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax.
        /// </summary>
        public int Length => BackgroundNodes.Length;

        private GreenJsonBackgroundListSyntax(ReadOnlySpanList<GreenJsonBackgroundSyntax> backgroundNodes) => BackgroundNodes = backgroundNodes;
    }

    /// <summary>
    /// Represents a node with background symbols in an abstract json syntax tree.
    /// </summary>
    public sealed class JsonBackgroundListSyntax : JsonSyntax
    {
        private class JsonValueWithBackgroundSyntaxCreator : GreenJsonBackgroundSyntaxVisitor<(JsonBackgroundListSyntax, int), JsonBackgroundSyntax>
        {
            public static readonly JsonValueWithBackgroundSyntaxCreator Instance = new JsonValueWithBackgroundSyntaxCreator();

            private JsonValueWithBackgroundSyntaxCreator() { }

            public override JsonBackgroundSyntax VisitCommentSyntax(GreenJsonCommentSyntax green, (JsonBackgroundListSyntax, int) parent)
                => new JsonCommentSyntax(parent.Item1, parent.Item2, green);

            public override JsonBackgroundSyntax VisitRootLevelValueDelimiterSyntax(GreenJsonRootLevelValueDelimiterSyntax green, (JsonBackgroundListSyntax, int) parent)
                => new JsonRootLevelValueDelimiterSyntax(parent.Item1, parent.Item2, green);

            public override JsonBackgroundSyntax VisitUnterminatedMultiLineCommentSyntax(GreenJsonUnterminatedMultiLineCommentSyntax green, (JsonBackgroundListSyntax, int) parent)
                => new JsonUnterminatedMultiLineCommentSyntax(parent.Item1, parent.Item2, green);

            public override JsonBackgroundSyntax VisitWhitespaceSyntax(GreenJsonWhitespaceSyntax green, (JsonBackgroundListSyntax, int) parent)
                => new JsonWhitespaceSyntax(parent.Item1, parent.Item2, green);
        }

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public Union<JsonValueWithBackgroundSyntax, JsonMultiValueSyntax> Parent { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenJsonBackgroundListSyntax Green { get; }

        /// <summary>
        /// Gets the collection of background nodes.
        /// </summary>
        public SafeLazyObjectCollection<JsonBackgroundSyntax> BackgroundNodes { get; }

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Match(
            whenOption1: valueWithBackgroundSyntax => 0,
            whenOption2: multiValueSyntax => multiValueSyntax.Length - Length);

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => Green.Length;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override JsonSyntax ParentSyntax => Parent.Match<JsonSyntax>(
            whenOption1: x => x,
            whenOption2: x => x);

        /// <summary>
        /// Gets the number of children of this syntax node.
        /// </summary>
        public override int ChildCount => BackgroundNodes.Count;

        /// <summary>
        /// Initializes the child at the given <paramref name="index"/> and returns it.
        /// </summary>
        public override JsonSyntax GetChild(int index) => BackgroundNodes[index];

        /// <summary>
        /// Gets the start position of the child at the given <paramref name="index"/>, without initializing it.
        /// </summary>
        public override int GetChildStartPosition(int index) => Green.BackgroundNodes.GetElementOffset(index);

        private JsonBackgroundListSyntax(Union<JsonValueWithBackgroundSyntax, JsonMultiValueSyntax> parent, GreenJsonBackgroundListSyntax green)
        {
            Parent = parent;
            Green = green;

            BackgroundNodes = new SafeLazyObjectCollection<JsonBackgroundSyntax>(
                green.BackgroundNodes.Count,
                index => JsonValueWithBackgroundSyntaxCreator.Instance.Visit(green.BackgroundNodes[index], (this, index)));
        }

        internal JsonBackgroundListSyntax(JsonValueWithBackgroundSyntax backgroundBeforeParent)
            : this(backgroundBeforeParent, backgroundBeforeParent.Green.BackgroundBefore)
        {
        }

        internal JsonBackgroundListSyntax(JsonMultiValueSyntax backgroundAfterParent)
            : this(backgroundAfterParent, backgroundAfterParent.Green.BackgroundAfter)
        {
        }
    }
}

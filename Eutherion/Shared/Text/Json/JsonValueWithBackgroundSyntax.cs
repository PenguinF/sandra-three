#region License
/*********************************************************************************
 * JsonValueWithBackgroundSyntax.cs
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

using Eutherion.Utils;
using System;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a <see cref="GreenJsonValueSyntax"/> node together with the background symbols
    /// which directly precede it in an abstract json syntax tree.
    /// </summary>
    public sealed class GreenJsonValueWithBackgroundSyntax : ISpan
    {
        /// <summary>
        /// Gets the background symbols which directly precede the content value node.
        /// </summary>
        public GreenJsonBackgroundSyntax BackgroundBefore { get; }

        /// <summary>
        /// Gets the content node containing the actual json value.
        /// </summary>
        public GreenJsonValueSyntax ContentNode { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="GreenJsonValueWithBackgroundSyntax"/>.
        /// </summary>
        /// <param name="backgroundBefore">
        /// The background symbols which directly precede the content value node.
        /// </param>
        /// <param name="contentNode">
        /// The content node containing the actual json value.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="backgroundBefore"/> and/or <paramref name="contentNode"/> are null.
        /// </exception>
        public GreenJsonValueWithBackgroundSyntax(GreenJsonBackgroundSyntax backgroundBefore, GreenJsonValueSyntax contentNode)
        {
            BackgroundBefore = backgroundBefore ?? throw new ArgumentNullException(nameof(backgroundBefore));
            ContentNode = contentNode ?? throw new ArgumentNullException(nameof(contentNode));
            Length = BackgroundBefore.Length + ContentNode.Length;
        }
    }

    public sealed class JsonValueWithBackgroundSyntax : JsonSyntax
    {
        private class JsonValueSyntaxCreator : GreenJsonValueSyntaxVisitor<JsonValueWithBackgroundSyntax, JsonValueSyntax>
        {
            public static readonly JsonValueSyntaxCreator Instance = new JsonValueSyntaxCreator();

            private JsonValueSyntaxCreator() { }

            public override JsonValueSyntax VisitBooleanLiteralSyntax(GreenJsonBooleanLiteralSyntax green, JsonValueWithBackgroundSyntax parent)
                => green.Match<JsonValueSyntax>(
                    whenFalse: () => new JsonBooleanLiteralSyntax.False(parent),
                    whenTrue: () => new JsonBooleanLiteralSyntax.True(parent));

            public override JsonValueSyntax VisitIntegerLiteralSyntax(GreenJsonIntegerLiteralSyntax green, JsonValueWithBackgroundSyntax parent)
                => new JsonIntegerLiteralSyntax(parent, green);

            public override JsonValueSyntax VisitListSyntax(GreenJsonListSyntax green, JsonValueWithBackgroundSyntax parent)
                => new JsonListSyntax(parent, green);

            public override JsonValueSyntax VisitMapSyntax(GreenJsonMapSyntax green, JsonValueWithBackgroundSyntax parent)
                => new JsonMapSyntax(parent, green);

            public override JsonValueSyntax VisitMissingValueSyntax(GreenJsonMissingValueSyntax green, JsonValueWithBackgroundSyntax parent)
                => new JsonMissingValueSyntax(parent, green);

            public override JsonValueSyntax VisitStringLiteralSyntax(GreenJsonStringLiteralSyntax green, JsonValueWithBackgroundSyntax parent)
                => new JsonStringLiteralSyntax(parent, green);

            public override JsonValueSyntax VisitUndefinedValueSyntax(GreenJsonUndefinedValueSyntax green, JsonValueWithBackgroundSyntax parent)
                => new JsonUndefinedValueSyntax(parent, green);
        }

        public JsonMultiValueSyntax Parent { get; }
        public int ParentValueNodeIndex { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenJsonValueWithBackgroundSyntax Green { get; }

        private readonly SafeLazyObject<JsonBackgroundSyntax> backgroundBefore;
        private readonly SafeLazyObject<JsonValueSyntax> contentNode;

        public JsonBackgroundSyntax BackgroundBefore => backgroundBefore.Object;

        public JsonValueSyntax ContentNode => contentNode.Object;

        public override int Start => Parent.Green.ValueNodes.GetElementOffset(ParentValueNodeIndex);
        public override int Length => Green.Length;
        public override JsonSyntax ParentSyntax => Parent;

        public override int ChildCount => 2;  // BackgroundBefore and ContentNode.

        public override JsonSyntax GetChild(int index)
        {
            if (index == 0) return BackgroundBefore;
            if (index == 1) return ContentNode;
            throw new IndexOutOfRangeException();
        }

        public override int GetChildStartPosition(int index)
        {
            if (index == 0) return 0;
            if (index == 1) return Green.BackgroundBefore.Length;
            throw new IndexOutOfRangeException();
        }

        internal JsonValueWithBackgroundSyntax(JsonMultiValueSyntax parent, int parentValueNodeIndex, GreenJsonValueWithBackgroundSyntax green)
        {
            Parent = parent;
            ParentValueNodeIndex = parentValueNodeIndex;
            Green = green;
            backgroundBefore = new SafeLazyObject<JsonBackgroundSyntax>(() => new JsonBackgroundSyntax(this, Green.BackgroundBefore));
            contentNode = new SafeLazyObject<JsonValueSyntax>(() => JsonValueSyntaxCreator.Instance.Visit(Green.ContentNode, this));
        }
    }
}

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
    /// Represents a <see cref="JsonValueSyntax"/> node together with the background symbols
    /// which directly precede it in an abstract json syntax tree.
    /// </summary>
    public sealed class JsonValueWithBackgroundSyntax : ISpan
    {
        /// <summary>
        /// Gets the background symbols which directly precede the content value node.
        /// </summary>
        public JsonBackgroundSyntax BackgroundBefore { get; }

        /// <summary>
        /// Gets the content node containing the actual json value.
        /// </summary>
        public JsonValueSyntax ContentNode { get; }

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="JsonValueWithBackgroundSyntax"/>.
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
        public JsonValueWithBackgroundSyntax(JsonBackgroundSyntax backgroundBefore, JsonValueSyntax contentNode)
        {
            BackgroundBefore = backgroundBefore ?? throw new ArgumentNullException(nameof(backgroundBefore));
            ContentNode = contentNode ?? throw new ArgumentNullException(nameof(contentNode));
            Length = BackgroundBefore.Length + ContentNode.Length;
        }
    }

    public sealed class RedJsonValueWithBackgroundSyntax : JsonSyntax
    {
        private class JsonValueSyntaxCreator : JsonValueSyntaxVisitor<RedJsonValueWithBackgroundSyntax, RedJsonValueSyntax>
        {
            public static readonly JsonValueSyntaxCreator Instance = new JsonValueSyntaxCreator();

            private JsonValueSyntaxCreator() { }

            public override RedJsonValueSyntax VisitBooleanLiteralSyntax(JsonBooleanLiteralSyntax green, RedJsonValueWithBackgroundSyntax parent)
                => green.Match<RedJsonValueSyntax>(
                    whenFalse: () => new RedJsonBooleanLiteralSyntax.False(parent),
                    whenTrue: () => new RedJsonBooleanLiteralSyntax.True(parent));

            public override RedJsonValueSyntax VisitIntegerLiteralSyntax(JsonIntegerLiteralSyntax green, RedJsonValueWithBackgroundSyntax parent)
                => new RedJsonIntegerLiteralSyntax(parent, green);

            public override RedJsonValueSyntax VisitListSyntax(JsonListSyntax green, RedJsonValueWithBackgroundSyntax parent)
                => new RedJsonListSyntax(parent, green);

            public override RedJsonValueSyntax VisitMapSyntax(JsonMapSyntax green, RedJsonValueWithBackgroundSyntax parent)
                => new RedJsonMapSyntax(parent, green);

            public override RedJsonValueSyntax VisitMissingValueSyntax(JsonMissingValueSyntax green, RedJsonValueWithBackgroundSyntax parent)
                => new RedJsonMissingValueSyntax(parent, green);

            public override RedJsonValueSyntax VisitStringLiteralSyntax(JsonStringLiteralSyntax green, RedJsonValueWithBackgroundSyntax parent)
                => new RedJsonStringLiteralSyntax(parent, green);

            public override RedJsonValueSyntax VisitUndefinedValueSyntax(JsonUndefinedValueSyntax green, RedJsonValueWithBackgroundSyntax parent)
                => new RedJsonUndefinedValueSyntax(parent, green);
        }

        public RedJsonMultiValueSyntax Parent { get; }
        public int ParentValueNodeIndex { get; }

        public JsonValueWithBackgroundSyntax Green { get; }

        private readonly SafeLazyObject<RedJsonBackgroundSyntax> backgroundBefore;
        public RedJsonBackgroundSyntax BackgroundBefore => backgroundBefore.Object;

        private readonly SafeLazyObject<RedJsonValueSyntax> contentNode;
        public RedJsonValueSyntax ContentNode => contentNode.Object;

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

        internal RedJsonValueWithBackgroundSyntax(RedJsonMultiValueSyntax parent, int parentValueNodeIndex, JsonValueWithBackgroundSyntax green)
        {
            Parent = parent;
            ParentValueNodeIndex = parentValueNodeIndex;
            Green = green;
            backgroundBefore = new SafeLazyObject<RedJsonBackgroundSyntax>(() => new RedJsonBackgroundSyntax(this, Green.BackgroundBefore));
            contentNode = new SafeLazyObject<RedJsonValueSyntax>(() => JsonValueSyntaxCreator.Instance.Visit(Green.ContentNode, this));
        }
    }
}

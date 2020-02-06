﻿#region License
/*********************************************************************************
 * JsonSquareBracketOpenSyntax.cs
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

namespace Eutherion.Text.Json
{
    public sealed class JsonSquareBracketOpen : JsonForegroundSymbol
    {
        public const char SquareBracketOpenCharacter = '[';
        public const int SquareBracketOpenLength = 1;

        public static readonly JsonSquareBracketOpen Value = new JsonSquareBracketOpen();

        public override bool IsValueStartSymbol => true;
        public override int Length => SquareBracketOpenLength;

        private JsonSquareBracketOpen() { }

        public override void Accept(JsonSymbolVisitor visitor) => visitor.VisitSquareBracketOpen(this);
        public override TResult Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitSquareBracketOpen(this);
        public override TResult Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitSquareBracketOpen(this, arg);
    }

    /// <summary>
    /// Represents a json square bracket open syntax node.
    /// </summary>
    public sealed class JsonSquareBracketOpenSyntax : JsonSyntax
    {
        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public JsonListSyntax Parent { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public JsonSquareBracketOpen Green => JsonSquareBracketOpen.Value;

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => 0;

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => JsonSquareBracketOpen.SquareBracketOpenLength;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override JsonSyntax ParentSyntax => Parent;

        internal JsonSquareBracketOpenSyntax(JsonListSyntax parent) => Parent = parent;

        public override void Accept(JsonTerminalSymbolVisitor visitor) => visitor.VisitSquareBracketOpen(this);
        public override TResult Accept<TResult>(JsonTerminalSymbolVisitor<TResult> visitor) => visitor.VisitSquareBracketOpen(this);
        public override TResult Accept<T, TResult>(JsonTerminalSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitSquareBracketOpen(this, arg);
    }
}

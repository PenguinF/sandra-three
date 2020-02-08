﻿#region License
/*********************************************************************************
 * JsonCurlyCloseSyntax.cs
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

using System.Collections.Generic;

namespace Eutherion.Text.Json
{
    /// <summary>
    /// Represents a json curly close syntax node.
    /// </summary>
    public sealed class GreenJsonCurlyCloseSyntax : IJsonForegroundSymbol
    {
        public static readonly GreenJsonCurlyCloseSyntax Value = new GreenJsonCurlyCloseSyntax();

        public int Length => JsonCurlyCloseSyntax.CurlyCloseLength;

        private GreenJsonCurlyCloseSyntax() { }

        IEnumerable<JsonErrorInfo> IGreenJsonSymbol.GetErrors(int startPosition) => EmptyEnumerable<JsonErrorInfo>.Instance;
        Union<GreenJsonBackgroundSyntax, IJsonForegroundSymbol> IGreenJsonSymbol.AsBackgroundOrForeground() => this;

        bool IJsonForegroundSymbol.IsValueStartSymbol => false;

        void IJsonForegroundSymbol.Accept(JsonForegroundSymbolVisitor visitor) => visitor.VisitCurlyCloseSyntax(this);
        TResult IJsonForegroundSymbol.Accept<TResult>(JsonForegroundSymbolVisitor<TResult> visitor) => visitor.VisitCurlyCloseSyntax(this);
        TResult IJsonForegroundSymbol.Accept<T, TResult>(JsonForegroundSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitCurlyCloseSyntax(this, arg);
    }

    /// <summary>
    /// Represents a json curly close syntax node.
    /// </summary>
    public sealed class JsonCurlyCloseSyntax : JsonSyntax, IJsonSymbol
    {
        public const char CurlyCloseCharacter = '}';
        public const int CurlyCloseLength = 1;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public JsonMapSyntax Parent { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenJsonCurlyCloseSyntax Green => GreenJsonCurlyCloseSyntax.Value;

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Length - CurlyCloseLength;

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => CurlyCloseLength;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override JsonSyntax ParentSyntax => Parent;

        internal JsonCurlyCloseSyntax(JsonMapSyntax parent) => Parent = parent;

        void IJsonSymbol.Accept(JsonSymbolVisitor visitor) => visitor.VisitCurlyCloseSyntax(this);
        TResult IJsonSymbol.Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitCurlyCloseSyntax(this);
        TResult IJsonSymbol.Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitCurlyCloseSyntax(this, arg);
    }
}

#region License
/*********************************************************************************
 * JsonSquareBracketCloseSyntax.cs
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
    /// Represents a json square bracket close syntax node.
    /// </summary>
    public sealed class GreenJsonSquareBracketCloseSyntax : IJsonValueDelimiterSymbol
    {
        public static readonly GreenJsonSquareBracketCloseSyntax Value = new GreenJsonSquareBracketCloseSyntax();

        public int Length => JsonSquareBracketCloseSyntax.SquareBracketCloseLength;

        private GreenJsonSquareBracketCloseSyntax() { }

        IEnumerable<JsonErrorInfo> IGreenJsonSymbol.GetErrors(int startPosition) => EmptyEnumerable<JsonErrorInfo>.Instance;
        Union<GreenJsonBackgroundSyntax, IJsonForegroundSymbol> IGreenJsonSymbol.AsBackgroundOrForeground() => this;
        Union<IJsonValueDelimiterSymbol, IJsonValueStarterSymbol> IJsonForegroundSymbol.AsValueDelimiterOrStarter() => this;
    }

    /// <summary>
    /// Represents a json square bracket close syntax node.
    /// </summary>
    public sealed class JsonSquareBracketCloseSyntax : JsonSyntax, IJsonSymbol
    {
        public const char SquareBracketCloseCharacter = ']';
        public const int SquareBracketCloseLength = 1;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public JsonListSyntax Parent { get; }

        /// <summary>
        /// Gets the bottom-up only 'green' representation of this syntax node.
        /// </summary>
        public GreenJsonSquareBracketCloseSyntax Green => GreenJsonSquareBracketCloseSyntax.Value;

        /// <summary>
        /// Gets the start position of this syntax node relative to its parent's start position.
        /// </summary>
        public override int Start => Parent.Length - SquareBracketCloseLength;

        /// <summary>
        /// Gets the length of the text span corresponding with this syntax node.
        /// </summary>
        public override int Length => SquareBracketCloseLength;

        /// <summary>
        /// Gets the parent syntax node of this instance.
        /// </summary>
        public override JsonSyntax ParentSyntax => Parent;

        internal JsonSquareBracketCloseSyntax(JsonListSyntax parent) => Parent = parent;

        void IJsonSymbol.Accept(JsonSymbolVisitor visitor) => visitor.VisitSquareBracketCloseSyntax(this);
        TResult IJsonSymbol.Accept<TResult>(JsonSymbolVisitor<TResult> visitor) => visitor.VisitSquareBracketCloseSyntax(this);
        TResult IJsonSymbol.Accept<T, TResult>(JsonSymbolVisitor<T, TResult> visitor, T arg) => visitor.VisitSquareBracketCloseSyntax(this, arg);
    }
}
